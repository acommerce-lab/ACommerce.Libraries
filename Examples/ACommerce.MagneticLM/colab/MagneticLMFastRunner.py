#!/usr/bin/env python3
# MagneticLMFastRunner.py
# Single-file GPU runner for the graph-based MagneticLM language model.
# WikiText-103 only, full mode only, no cache, GPU + GPU RAM.
# CLI: --train-lines N --physics-iters K
import argparse, math, os, re, sys, time
from collections import defaultdict

try:
    import torch
except ImportError:
    print("ERROR: PyTorch required (pip install torch).", file=sys.stderr)
    sys.exit(1)

_SPLIT_RE = re.compile(r'[.,;!?()\[\]{}"]+')

def tokenize(line):
    return [w for w in _SPLIT_RE.sub(' ', line.lower()).split() if w]


# ---------------------------------------------------------------------------
# WikiText-103 loader (downloads once via HF datasets, then reuses .txt files)
# ---------------------------------------------------------------------------
def ensure_wt103(data_dir="data/wt103"):
    os.makedirs(data_dir, exist_ok=True)
    train_path = os.path.join(data_dir, "train.txt")
    test_path = os.path.join(data_dir, "test.txt")
    if os.path.exists(train_path) and os.path.exists(test_path):
        return train_path, test_path
    print("Downloading WikiText-103 (via HF datasets)...")
    try:
        from datasets import load_dataset
    except ImportError:
        print("ERROR: first-time download needs `pip install datasets`.",
              file=sys.stderr)
        sys.exit(1)
    ds = load_dataset("Salesforce/wikitext", "wikitext-103-v1")
    for split, path in (("train", train_path), ("test", test_path)):
        with open(path, "w", encoding="utf-8") as f:
            for item in ds[split]:
                t = item["text"].strip()
                if t and not t.startswith("="):
                    f.write(t + "\n")
    return train_path, test_path


# ---------------------------------------------------------------------------
# MagneticLMGPU: graph + physics on CUDA, KN-5 + position scoring, no cache
# ---------------------------------------------------------------------------
class MagneticLMGPU:
    MAX_ORDER = 5

    def __init__(self, device):
        self.device = device
        self.word2id = {}
        self.id2word = []
        self.freq = []

        # n-gram counts keyed by int-tuple contexts (CPU dicts during training)
        self.ngram_counts = defaultdict(dict)     # ctx_tuple -> {next_id: count}
        self.ngram_totals = defaultdict(int)
        self.count1 = defaultdict(int)
        self.count2 = defaultdict(int)
        self.continuation_ctx = defaultdict(set)  # next_id -> set of contexts
        self.unique_followers = defaultdict(int)
        self.total_unique_bigrams = 0
        self.total_tokens = 0

        self.semantic = defaultdict(lambda: defaultdict(float))

        self.D1 = 0.5
        self.D2 = 0.75
        self.D3 = 0.9

        # GPU tensors (populated in build)
        self.positions = None      # (N, 3) float32 CUDA
        self.importance = None     # (N,)   float32 CUDA
        self.circle_group = None   # (N,)   int64   CUDA, -1 for none

    def _wid(self, w):
        wid = self.word2id.get(w)
        if wid is None:
            wid = len(self.id2word)
            self.word2id[w] = wid
            self.id2word.append(w)
            self.freq.append(0)
        return wid

    # ----- training: CPU text pass, int-keyed dicts -----
    def train(self, lines):
        wid = self._wid
        nc = self.ngram_counts
        nt = self.ngram_totals
        c1 = self.count1
        c2 = self.count2
        cc = self.continuation_ctx
        uf = self.unique_followers
        sem = self.semantic
        mx = self.MAX_ORDER
        ns = 0
        t0 = time.time()
        for line in lines:
            ws = tokenize(line)
            if len(ws) < 2:
                continue
            ids = [wid(w) for w in ws]
            for i in ids:
                self.freq[i] += 1
            self.total_tokens += len(ids)
            L = len(ids)
            for i in range(1, L):
                nw = ids[i]
                start = max(0, i - mx)
                for o in range(1, i - start + 1):
                    ctx = tuple(ids[i - o:i])
                    d = nc[ctx]
                    old = d.get(nw, 0)
                    new = old + 1
                    d[nw] = new
                    nt[ctx] += 1
                    if new == 1:
                        c1[ctx] += 1
                        cc[nw].add(ctx)
                        uf[ctx] += 1
                        if o == 1:
                            self.total_unique_bigrams += 1
                    elif new == 2:
                        c1[ctx] -= 1
                        c2[ctx] += 1
                    elif new == 3:
                        c2[ctx] -= 1
            # semantic co-occurrence (window +-2)
            for i in range(L):
                wi = ids[i]
                si = sem[wi]
                for d in (-2, -1, 1, 2):
                    j = i + d
                    if 0 <= j < L:
                        wj = ids[j]
                        if wi == wj:
                            continue
                        amount = 0.1 if abs(d) == 1 else 0.05
                        si[wj] = si.get(wj, 0.0) + amount
                        sem[wj][wi] = sem[wj].get(wi, 0.0) + amount
            ns += 1
            if ns % 2000 == 0:
                print("\r  Training: %d lines (%.0fs)" % (ns, time.time() - t0),
                      end="", flush=True)
        print("\r  Training: %d lines done (%.0fs)." % (ns, time.time() - t0))

    # ----- build: KN discounts + physics on GPU + importance + circles -----
    def build(self, physics_iters=30):
        # Modified KN discounts from count-of-counts
        n1 = n2 = n3 = 0
        for d in self.ngram_counts.values():
            for c in d.values():
                if c == 1:
                    n1 += 1
                elif c == 2:
                    n2 += 1
                elif c == 3:
                    n3 += 1
        if n1 > 0 and n2 > 0:
            Y = n1 / (n1 + 2 * n2)
            self.D1 = max(0.1, min(0.95, 1 - 2 * Y * n2 / n1))
            self.D2 = max(0.1, min(0.95, 2 - 3 * Y * (n3 / n2 if n2 > 0 else 0)))
            self.D3 = max(0.1, min(0.95,
                                    3 - 4 * Y * ((n3 + 1) / n3 if n3 > 0 else 1)))
        print("  KN discounts: D1=%.3f D2=%.3f D3+=%.3f" %
              (self.D1, self.D2, self.D3))

        dev = self.device
        N = len(self.id2word)
        print("  Nodes: %d" % N)

        # Semantic edges -> GPU tensors
        ef, et, ew = [], [], []
        for a, edges in self.semantic.items():
            for b, w in edges.items():
                if abs(w) < 0.1:
                    continue
                ef.append(a)
                et.append(b)
                ew.append(min(w, 10.0))
        if ef:
            edge_from = torch.tensor(ef, dtype=torch.long, device=dev)
            edge_to = torch.tensor(et, dtype=torch.long, device=dev)
            edge_w = torch.tensor(ew, dtype=torch.float32, device=dev)
        else:
            edge_from = torch.empty(0, dtype=torch.long, device=dev)
            edge_to = torch.empty(0, dtype=torch.long, device=dev)
            edge_w = torch.empty(0, dtype=torch.float32, device=dev)
        E = int(edge_w.numel())
        print("  Semantic edges: %d" % E)

        # Positions + velocities on GPU
        torch.manual_seed(42)
        positions = (torch.rand((N, 3), device=dev, dtype=torch.float32) * 10.0 - 5.0)
        velocities = torch.zeros((N, 3), device=dev, dtype=torch.float32)

        K_context = 2.0
        K_frequency = 1.5
        K_attraction = 0.5
        K_repulsion = 0.3
        damping = 0.15
        lr = 0.02
        optimal_dist = 3.0
        max_radius = 15.0
        sample_size = min(N, 200)

        print("  Physics: %d iters on %s" % (physics_iters, dev), end="", flush=True)
        t0 = time.time()
        for it in range(physics_iters):
            forces = torch.zeros_like(positions)

            # semantic pull/push forces along edges
            if E > 0:
                pf = positions[edge_from]
                pt = positions[edge_to]
                diff = pt - pf
                dist = diff.norm(dim=1, keepdim=True).clamp_min(0.1)
                unit = diff / dist
                k_tensor = torch.where(
                    edge_w > 1.0,
                    torch.full_like(edge_w, K_context),
                    torch.full_like(edge_w, K_frequency))
                fmag = k_tensor * edge_w / dist.squeeze(1)
                fvec = unit * fmag.unsqueeze(1)
                forces.index_add_(0, edge_from, fvec)

            # repulsion + attraction against a random sample
            if N > sample_size:
                sample_idx = torch.randperm(N, device=dev)[:sample_size]
            else:
                sample_idx = torch.arange(N, device=dev)
            sample_pos = positions[sample_idx]  # (S, 3)

            bsz = 4096
            for bs in range(0, N, bsz):
                be = min(bs + bsz, N)
                tp = positions[bs:be]                        # (B, 3)
                d = sample_pos.unsqueeze(0) - tp.unsqueeze(1)  # (B, S, 3)
                di = d.norm(dim=2).clamp_min(0.1)             # (B, S)
                u = d / di.unsqueeze(2)
                rep = -K_repulsion / (di * di + 1.0)
                rep_vec = u * rep.unsqueeze(2)
                forces[bs:be] += rep_vec.sum(dim=1)
                beyond = (di > optimal_dist)
                att = torch.where(
                    beyond,
                    K_attraction * (di - optimal_dist) * 0.01,
                    torch.zeros_like(di))
                att_vec = u * att.unsqueeze(2)
                forces[bs:be] += att_vec.sum(dim=1)

            forces.sub_(0.01 * positions)  # gravity toward origin
            velocities = (velocities + forces * lr) * (1.0 - damping)
            positions.add_(velocities * lr)

            mag = positions.norm(dim=1)
            overflow = mag > max_radius
            if overflow.any():
                scale = (max_radius / mag[overflow]).unsqueeze(1)
                positions[overflow] = positions[overflow] * scale
                velocities[overflow] = velocities[overflow] * 0.5

            if (it + 1) % 5 == 0:
                print(".", end="", flush=True)
        print(" done (%.0fs)" % (time.time() - t0))

        self.positions = positions

        # Importance = log(1+degree) * log(1+freq), all on GPU
        degs = torch.zeros(N, dtype=torch.float32, device=dev)
        if E > 0:
            degs.index_add_(0, edge_from,
                            torch.ones(E, dtype=torch.float32, device=dev))
        freq_t = torch.tensor(self.freq, dtype=torch.float32, device=dev)
        self.importance = torch.log1p(degs) * torch.log1p(freq_t)

        # Circles: bidirectional strong-tie cliques (small + one-time; CPU side)
        threshold = 0.3
        group = [-1] * N
        neighbors = defaultdict(set)
        for a, edges in self.semantic.items():
            for b, w in edges.items():
                if w >= threshold and self.semantic.get(b, {}).get(a, 0) >= threshold:
                    neighbors[a].add(b)
        gid = 0
        for a, nbs in neighbors.items():
            if group[a] != -1:
                continue
            clique = {a}
            for c in nbs:
                if c not in neighbors:
                    continue
                if all(m in neighbors.get(c, set()) for m in clique):
                    clique.add(c)
                    if len(clique) >= 5:
                        break
            if len(clique) >= 3:
                for m in clique:
                    if group[m] == -1:
                        group[m] = gid
                gid += 1
        self.circle_group = torch.tensor(group, dtype=torch.long, device=dev)
        print("  Circles: %d" % gid)

    # ----- Modified KN-5 on CPU dicts (cheap vs physics/eval GPU work) -----
    def _kn(self, ctx_tuple, nw):
        D1, D2, D3 = self.D1, self.D2, self.D3
        nc = self.ngram_counts
        nt = self.ngram_totals
        c1 = self.count1
        c2 = self.count2
        uf = self.unique_followers
        cc = self.continuation_ctx
        V = max(len(self.id2word), 1)
        tub = self.total_unique_bigrams

        def rec(o):
            if o == 0:
                if tub == 0:
                    return 1.0 / V
                s = cc.get(nw)
                if not s:
                    return 0.5 / tub
                return len(s) / tub
            key = ctx_tuple[-o:]
            t = nt.get(key, 0)
            if t == 0:
                return rec(o - 1)
            d = nc.get(key, {})
            c = d.get(nw, 0)
            disc_d = 0 if c <= 0 else D1 if c == 1 else D2 if c == 2 else D3
            disc = max(c - disc_d, 0) / t
            n1 = c1.get(key, 0)
            n2 = c2.get(key, 0)
            u = uf.get(key, 0)
            lam = (D1 * n1 + D2 * n2 + D3 * max(u - n1 - n2, 0)) / t
            return disc + lam * rec(o - 1)

        return rec(len(ctx_tuple))

    # ----- full-mode eval (no cache). KN CPU, positional score GPU batched. -----
    def eval_full_wt103(self, test_lines, batch_size=8192):
        dev = self.device
        N = len(self.id2word)
        pos = self.positions
        imp = self.importance
        circ = self.circle_group

        total_logp = 0.0
        total_tok = 0

        kn_buf = []
        ctx_buf = []
        nxt_buf = []
        band_buf = []

        pos_l_table = torch.tensor(
            [0.02, 0.06, 0.12], dtype=torch.float32, device=dev)  # hi/mid/lo bands

        K = self.MAX_ORDER

        def flush():
            nonlocal total_logp, total_tok
            if not kn_buf:
                return
            B = len(kn_buf)
            ctx_arr = torch.full((B, K), -1, dtype=torch.long)
            for i, cs in enumerate(ctx_buf):
                lc = min(len(cs), K)
                if lc > 0:
                    ctx_arr[i, :lc] = torch.as_tensor(cs[-lc:], dtype=torch.long)
            ctx_arr = ctx_arr.to(dev, non_blocking=True)
            nxt_arr = torch.as_tensor(nxt_buf, dtype=torch.long, device=dev)
            kn_arr = torch.as_tensor(kn_buf, dtype=torch.float32, device=dev)
            band = torch.as_tensor(band_buf, dtype=torch.long, device=dev)

            valid = (ctx_arr >= 0)                           # (B, K)
            safe_ctx = ctx_arr.clamp_min(0)
            ctx_pos = pos[safe_ctx]                          # (B, K, 3)
            nxt_pos = pos[nxt_arr].unsqueeze(1)              # (B, 1, 3)
            dot = (ctx_pos * nxt_pos).sum(-1)                # (B, K)
            ctx_norm = ctx_pos.norm(dim=-1)                  # (B, K)
            nxt_norm = nxt_pos.norm(dim=-1)                  # (B, 1)
            denom = (ctx_norm * nxt_norm).clamp_min(1e-6)
            sim = (dot / denom).clamp(-1.0, 1.0)
            mask = valid & (sim > 0.05)
            sim = torch.where(mask, sim, torch.zeros_like(sim))

            ctx_imp = imp[safe_ctx]                          # (B, K)
            boost_imp = 1.0 + ctx_imp * 0.05
            nxt_circ = circ[nxt_arr].unsqueeze(1)            # (B, 1)
            ctx_circ = circ[safe_ctx]                        # (B, K)
            same_circle = (ctx_circ >= 0) & (ctx_circ == nxt_circ)
            circle_boost = 1.0 + 0.5 * same_circle.float()

            contrib = sim * boost_imp * circle_boost
            pos_count = mask.sum(dim=1).clamp_min(1)
            has_any = mask.any(dim=1)
            pos_score = contrib.sum(dim=1)
            pos_prob = (pos_score /
                        (pos_count.to(pos_score.dtype) * 3.0)).clamp_max(0.3)
            pos_prob = torch.where(has_any, pos_prob, torch.zeros_like(pos_prob))

            pos_l = pos_l_table[band]
            kn_l = 1.0 - pos_l
            mixed = (kn_l * kn_arr + pos_l * pos_prob).clamp(1e-10, 0.999)
            total_logp += float(torch.log(mixed).sum().item())
            total_tok += B

            kn_buf.clear()
            ctx_buf.clear()
            nxt_buf.clear()
            band_buf.clear()

        w2i = self.word2id
        log_floor = math.log(1e-10)
        t0 = time.time()
        for li, line in enumerate(test_lines):
            ws = tokenize(line)
            if len(ws) < 2:
                continue
            ids = [w2i.get(w, -1) for w in ws]
            for i in range(1, len(ids)):
                nw = ids[i]
                if nw < 0:
                    total_logp += log_floor
                    total_tok += 1
                    continue
                start = max(0, i - K)
                ctx = tuple(c for c in ids[start:i] if c >= 0)
                if not ctx:
                    kn = 1.0 / max(N, 1)
                else:
                    kn = self._kn(ctx, nw)
                if kn > 0.05:
                    band = 0
                elif kn > 0.005:
                    band = 1
                else:
                    band = 2
                kn_buf.append(kn)
                ctx_buf.append(list(ctx))
                nxt_buf.append(nw)
                band_buf.append(band)
                if len(kn_buf) >= batch_size:
                    flush()
            if (li + 1) % 2000 == 0:
                print("\r  Eval: %d/%d lines (%.0fs)" %
                      (li + 1, len(test_lines), time.time() - t0),
                      end="", flush=True)
        flush()
        print()
        if total_tok == 0:
            return float("inf")
        return math.exp(-total_logp / total_tok)


# ---------------------------------------------------------------------------
# CLI entry
# ---------------------------------------------------------------------------
def main():
    ap = argparse.ArgumentParser(
        description="MagneticLM GPU runner: WikiText-103, full mode, no cache.")
    ap.add_argument("--train-lines", type=int, default=100000,
                    help="Number of training lines to pull from WikiText-103")
    ap.add_argument("--physics-iters", type=int, default=30,
                    help="Number of physics simulation iterations on the GPU")
    ap.add_argument("--batch-size", type=int, default=8192,
                    help="Eval batch size for GPU position scoring")
    ap.add_argument("--data-dir", default="data/wt103",
                    help="Directory for WikiText-103 train.txt / test.txt")
    args = ap.parse_args()

    if not torch.cuda.is_available():
        print("ERROR: CUDA GPU required. This runner is GPU-only.",
              file=sys.stderr)
        sys.exit(2)
    device = torch.device("cuda")
    torch.backends.cuda.matmul.allow_tf32 = True
    props = torch.cuda.get_device_properties(0)
    print("Device: %s (%.1f GB)" %
          (torch.cuda.get_device_name(0), props.total_memory / 1024 ** 3))

    train_path, test_path = ensure_wt103(args.data_dir)

    print("\nLoading %d train lines from %s" % (args.train_lines, train_path))
    train = []
    with open(train_path, "r", encoding="utf-8") as f:
        for line in f:
            s = line.strip()
            if not s:
                continue
            train.append(s)
            if len(train) >= args.train_lines:
                break
    print("Loaded %d train lines" % len(train))

    test = []
    with open(test_path, "r", encoding="utf-8") as f:
        for line in f:
            s = line.strip()
            if s:
                test.append(s)
    print("Loaded %d test lines" % len(test))

    print("\n" + "=" * 60)
    print("  MagneticLM Fast Runner (GPU, no cache, full mode)")
    print("=" * 60)

    t_all = time.time()
    model = MagneticLMGPU(device=device)

    t0 = time.time()
    model.train(train)
    print("  Train time: %.0fs" % (time.time() - t0))

    t0 = time.time()
    model.build(physics_iters=args.physics_iters)
    print("  Build time: %.0fs" % (time.time() - t0))

    if torch.cuda.is_available():
        used = torch.cuda.memory_allocated() / 1024 ** 2
        print("  GPU RAM used: %.0f MiB" % used)

    print("\nEvaluating WikiText-103 test (full mode, no cache)...")
    t0 = time.time()
    ppl = model.eval_full_wt103(test, batch_size=args.batch_size)
    eval_time = time.time() - t0
    print("\nPerplexity (MagneticLM full, no cache) = %.2f" % ppl)
    print("Eval time:  %.0fs" % eval_time)
    print("Total time: %.0fs" % (time.time() - t_all))


if __name__ == "__main__":
    main()
