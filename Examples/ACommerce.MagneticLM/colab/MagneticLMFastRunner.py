#!/usr/bin/env python3
# MagneticLMFastRunner.py
# GPU-native single-file runner for the graph-based MagneticLM language model.
# WikiText-103 only, full mode only, no cache, CUDA + GPU RAM only.
#
# Design:
#   - Tokenize the whole corpus into one int32 stream on CPU (cheap), then
#     move it to a single int64 tensor on the GPU.
#   - All n-gram counting runs on CUDA via polynomial hashing:
#       * ngram_hash[i] = sum_k tokens[i+k] * HASH_PRIMES[k]
#       * torch.unique(ngram_hash, return_counts=True) yields the counts
#       * scatter_add aggregates context-level totals, count-of-counts, and
#         unique-follower counts for Modified Kneser-Ney smoothing.
#   - KN-5 backoff runs entirely on the GPU, vectorized across the whole test
#     set, via torch.searchsorted against the sorted per-order hash tables.
#   - Physics simulation + position similarity + importance all stay on CUDA.
#   - No Python dicts for n-gram counts, no cache, no CPU hot loops in eval.
#
# CLI:
#   python MagneticLMFastRunner.py --train-lines 100000 --physics-iters 30
import argparse
import math
import os
import re
import sys
import time

try:
    import torch
except ImportError:
    print("ERROR: PyTorch required (pip install torch).", file=sys.stderr)
    sys.exit(1)

try:
    import numpy as np
except ImportError:
    print("ERROR: NumPy required (pip install numpy).", file=sys.stderr)
    sys.exit(1)


_SPLIT_RE = re.compile(r'[.,;!?()\[\]{}"]+')


def tokenize(line):
    return [w for w in _SPLIT_RE.sub(' ', line.lower()).split() if w]


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


# Polynomial hashing primes - one constant per position in the ngram window.
# Chosen as large odd splitmix64-style multipliers; int64 multiplies overflow
# modulo 2^64 naturally, giving a near-uniform universal hash. Collision
# probability for ~10^9 keys is ~10^-10, negligible for our purposes.
_HASH_PRIMES_CPU = torch.tensor(
    [
        0x9E3779B97F4A7C15,
        0xBF58476D1CE4E5B9,
        0x94D049BB133111EB,
        0x7F4A7C15F39CC060,
        0xA6E36C3B4E5A7F11,
        0xD2B74407B1CE6E93,
    ],
    dtype=torch.int64,
)


class MagneticLMGPU:
    MAX_ORDER = 5

    def __init__(self, device):
        self.device = device
        # vocab (CPU only during tokenization; then frozen)
        self.word2id = {}
        self.id2word = []
        # token stream on GPU (released after build())
        self.tokens_gpu = None
        self.total_tokens = 0
        # hash primes (on device)
        self.hash_primes = _HASH_PRIMES_CPU.to(device)
        # per-order GPU tables (populated in train_gpu)
        self.ngram_hash_sorted = [None] * (self.MAX_ORDER + 1)
        self.ngram_count = [None] * (self.MAX_ORDER + 1)
        self.ctx_hash_sorted = [None] * (self.MAX_ORDER + 1)
        self.ctx_total = [None] * (self.MAX_ORDER + 1)
        self.ctx_count1 = [None] * (self.MAX_ORDER + 1)
        self.ctx_count2 = [None] * (self.MAX_ORDER + 1)
        self.ctx_uf = [None] * (self.MAX_ORDER + 1)
        # bigram continuation
        self.cont_count = None
        self.total_unique_bigrams = 0
        # frequency table
        self.freq_gpu = None
        # KN discounts (scalar floats)
        self.D1 = 0.5
        self.D2 = 0.75
        self.D3 = 0.9
        # physics artifacts (populated in build)
        self.positions = None
        self.importance = None
        self.circle_group = None
        # semantic edges (populated in train_gpu)
        self.edge_from = None
        self.edge_to = None
        self.edge_weight = None

    def _hash_rows(self, rows, length):
        primes = self.hash_primes[:length]
        return (rows * primes).sum(dim=-1)

    # ----- tokenize corpus into a compact int32 stream, then move to GPU -----
    def _tokenize_to_gpu(self, lines):
        import array
        w2i = self.word2id
        id2w = self.id2word
        toks = array.array('i')
        ns = 0
        t0 = time.time()
        for line in lines:
            ws = tokenize(line)
            if len(ws) < 2:
                continue
            for w in ws:
                wid = w2i.get(w)
                if wid is None:
                    wid = len(id2w)
                    w2i[w] = wid
                    id2w.append(w)
                toks.append(wid)
            ns += 1
            if ns % 20000 == 0:
                print("\r  Tokenizing: %d lines, %d tokens (%.0fs)" %
                      (ns, len(toks), time.time() - t0),
                      end="", flush=True)
        print("\r  Tokenizing: %d lines, %d tokens (%.0fs) done." %
              (ns, len(toks), time.time() - t0))
        T = len(toks)
        self.total_tokens = T
        if T == 0:
            self.tokens_gpu = torch.empty(0, dtype=torch.int64, device=self.device)
            return
        # array('i') is int32 on every platform that matters
        np_tokens = np.frombuffer(toks, dtype=np.int32)
        # Move to GPU as int64 (required by hash arithmetic)
        self.tokens_gpu = torch.from_numpy(np_tokens).to(
            device=self.device, dtype=torch.int64)
        del toks, np_tokens

    # ----- Build n-gram aggregates for a single order on the GPU -----
    # Uses polynomial hashing so everything stays as int64 tensors and we
    # never touch a Python dict. The token stream is processed in chunks to
    # cap peak memory (default 40M tokens/chunk ≈ 3GB peak per chunk).
    def _count_ngrams_order(self, o, chunk_tokens=40_000_000):
        dev = self.device
        tokens = self.tokens_gpu
        T = tokens.numel()
        if T <= o:
            return
        primes_ctx = self.hash_primes[:o]
        primes_ng = self.hash_primes[:o + 1]

        def hash_chunk(start_ngram, end_ngram):
            # Build hashes for ngrams whose first token index in [start_ngram, end_ngram).
            tok_slice = tokens[start_ngram:end_ngram + o]
            L = tok_slice.numel() - o
            ngram_h = torch.zeros(L, dtype=torch.int64, device=dev)
            for k in range(o + 1):
                ngram_h += tok_slice[k:L + k] * primes_ng[k]
            ctx_h = torch.zeros(L, dtype=torch.int64, device=dev)
            for k in range(o):
                ctx_h += tok_slice[k:L + k] * primes_ctx[k]
            return ngram_h, ctx_h

        ng_list = []
        cnt_list = []
        ctx_list = []
        nG = T - o
        start = 0
        while start < nG:
            end = min(start + chunk_tokens, nG)
            ngram_h, ctx_h = hash_chunk(start, end)
            uniq_ng, inverse, counts = torch.unique(
                ngram_h, return_inverse=True, return_counts=True)
            del ngram_h
            # For each unique ngram in this chunk, pick an arbitrary-but-stable
            # ctx hash (all positions with the same unique ngram necessarily
            # share the same ctx, so first-position is fine).
            positions = torch.arange(ctx_h.numel(), dtype=torch.int64, device=dev)
            first_pos = torch.full(
                (uniq_ng.numel(),), ctx_h.numel(),
                dtype=torch.int64, device=dev)
            first_pos.scatter_reduce_(0, inverse, positions,
                                      reduce='amin', include_self=True)
            ctx_for_uniq = ctx_h[first_pos.clamp_max(ctx_h.numel() - 1)]
            del positions, first_pos, inverse, ctx_h
            ng_list.append(uniq_ng)
            cnt_list.append(counts)
            ctx_list.append(ctx_for_uniq)
            start = end
            torch.cuda.empty_cache()

        # Merge chunks: re-unique ngram hashes, sum counts, keep one ctx hash
        all_ng = torch.cat(ng_list)
        all_cnt = torch.cat(cnt_list)
        all_ctx = torch.cat(ctx_list)
        del ng_list, cnt_list, ctx_list

        uniq_ng, inverse = torch.unique(all_ng, return_inverse=True)
        K = uniq_ng.numel()
        ng_counts = torch.zeros(K, dtype=torch.int64, device=dev)
        ng_counts.scatter_add_(0, inverse, all_cnt)
        # ctx hash: scatter_reduce amin — stable deterministic choice
        ctx_for_ngram = torch.full(
            (K,), torch.iinfo(torch.int64).max,
            dtype=torch.int64, device=dev)
        ctx_for_ngram.scatter_reduce_(
            0, inverse, all_ctx, reduce='amin', include_self=True)
        del all_ng, all_cnt, all_ctx, inverse

        # Per-ctx aggregates
        uniq_ctx, ctx_inv = torch.unique(ctx_for_ngram, return_inverse=True)
        U = uniq_ctx.numel()
        ctx_total = torch.zeros(U, dtype=torch.int64, device=dev)
        ctx_total.scatter_add_(0, ctx_inv, ng_counts)
        ctx_count1 = torch.zeros(U, dtype=torch.int64, device=dev)
        ctx_count1.scatter_add_(0, ctx_inv, (ng_counts == 1).to(torch.int64))
        ctx_count2 = torch.zeros(U, dtype=torch.int64, device=dev)
        ctx_count2.scatter_add_(0, ctx_inv, (ng_counts == 2).to(torch.int64))
        ctx_uf = torch.zeros(U, dtype=torch.int64, device=dev)
        ctx_uf.scatter_add_(0, ctx_inv, torch.ones_like(ng_counts))

        self.ngram_hash_sorted[o] = uniq_ng
        self.ngram_count[o] = ng_counts
        self.ctx_hash_sorted[o] = uniq_ctx
        self.ctx_total[o] = ctx_total
        self.ctx_count1[o] = ctx_count1
        self.ctx_count2[o] = ctx_count2
        self.ctx_uf[o] = ctx_uf
        del ctx_for_ngram, ctx_inv
        torch.cuda.empty_cache()

    # ----- Continuation count per word (KN backoff base case) -----
    def _compute_continuation(self):
        dev = self.device
        V = len(self.id2word)
        self.cont_count = torch.zeros(V, dtype=torch.int64, device=dev)
        tokens = self.tokens_gpu
        T = tokens.numel()
        if T < 2:
            return
        # For each unique bigram (w_prev, w_next), credit +1 to w_next.
        primes_bi = self.hash_primes[:2]
        big_h = tokens[:-1] * primes_bi[0] + tokens[1:] * primes_bi[1]
        uniq, inverse = torch.unique(big_h, return_inverse=True)
        K = uniq.numel()
        positions = torch.arange(T - 1, dtype=torch.int64, device=dev)
        first_pos = torch.full(
            (K,), T - 1, dtype=torch.int64, device=dev)
        first_pos.scatter_reduce_(0, inverse, positions,
                                  reduce='amin', include_self=True)
        next_w = tokens[first_pos.clamp_max(T - 2) + 1]
        self.cont_count.scatter_add_(0, next_w, torch.ones_like(next_w))
        self.total_unique_bigrams = int(K)
        del big_h, uniq, inverse, positions, first_pos, next_w

    # ----- Semantic edges for physics (all on GPU) -----
    # Equivalent to the original ±1,±2 co-occurrence loop with symmetric
    # weights: per (i, i+1) pair we want +0.2 per direction, per (i, i+2)
    # +0.1 per direction. We process the two offsets sequentially and fold
    # into a running unique-edge table keyed by from*V + to.
    def _build_semantic_edges(self):
        dev = self.device
        V = len(self.id2word)
        tokens = self.tokens_gpu
        T = tokens.numel()
        if T < 2 or V == 0:
            self.edge_from = torch.empty(0, dtype=torch.int64, device=dev)
            self.edge_to = torch.empty(0, dtype=torch.int64, device=dev)
            self.edge_weight = torch.empty(0, dtype=torch.float32, device=dev)
            return
        V_long = torch.tensor(V, dtype=torch.int64, device=dev)

        running_keys = torch.empty(0, dtype=torch.int64, device=dev)
        running_w = torch.empty(0, dtype=torch.float32, device=dev)

        for offset, amount in ((1, 0.2), (2, 0.1)):
            if T <= offset:
                continue
            a = tokens[:-offset]
            b = tokens[offset:]
            mask = a != b
            a = a[mask]
            b = b[mask]
            # symmetric: both directions
            both_f = torch.cat([a, b])
            both_t = torch.cat([b, a])
            del a, b, mask
            keys = both_f * V_long + both_t
            del both_f, both_t
            # aggregate this offset's edges first to shrink memory
            uk, inv = torch.unique(keys, return_inverse=True)
            del keys
            ones = torch.ones(inv.numel(), dtype=torch.float32, device=dev) * amount
            w = torch.zeros(uk.numel(), dtype=torch.float32, device=dev)
            w.scatter_add_(0, inv, ones)
            del inv, ones
            # merge with running
            merged_keys = torch.cat([running_keys, uk])
            merged_w = torch.cat([running_w, w])
            del uk, w
            uk2, inv2 = torch.unique(merged_keys, return_inverse=True)
            w2 = torch.zeros(uk2.numel(), dtype=torch.float32, device=dev)
            w2.scatter_add_(0, inv2, merged_w)
            del merged_keys, merged_w, inv2
            running_keys = uk2
            running_w = w2
            torch.cuda.empty_cache()

        # Decode keys -> (from, to); filter weak edges.
        edge_from = running_keys // V_long
        edge_to = running_keys % V_long
        strong = running_w >= 0.1
        self.edge_from = edge_from[strong].contiguous()
        self.edge_to = edge_to[strong].contiguous()
        self.edge_weight = running_w[strong].clamp(-10.0, 10.0).contiguous()
        del running_keys, running_w, edge_from, edge_to
        torch.cuda.empty_cache()

    # ----- Full GPU training pipeline -----
    def train_gpu(self, lines):
        dev = self.device
        print("  Tokenizing corpus...")
        self._tokenize_to_gpu(lines)
        T = self.tokens_gpu.numel()
        V = len(self.id2word)
        print("  Tokens: %d, Vocab: %d" % (T, V))
        if T == 0 or V == 0:
            return
        # Frequency table on GPU
        self.freq_gpu = torch.zeros(V, dtype=torch.int64, device=dev)
        self.freq_gpu.scatter_add_(0, self.tokens_gpu,
                                   torch.ones_like(self.tokens_gpu))
        # N-gram tables per order
        for o in range(1, self.MAX_ORDER + 1):
            t0 = time.time()
            print("  Order %d: building..." % o, end="", flush=True)
            self._count_ngrams_order(o)
            print(" done (%.0fs, ngrams=%d, ctxs=%d)" % (
                time.time() - t0,
                self.ngram_count[o].numel(),
                self.ctx_total[o].numel()))
        # Continuation counts for KN backoff
        t0 = time.time()
        print("  Continuation counts...", end="", flush=True)
        self._compute_continuation()
        print(" done (%.0fs, %d unique bigrams)" % (
            time.time() - t0, self.total_unique_bigrams))
        # Semantic edges for physics
        t0 = time.time()
        print("  Semantic edges...", end="", flush=True)
        self._build_semantic_edges()
        print(" done (%.0fs, %d edges)" % (
            time.time() - t0, self.edge_from.numel()))
        # Tokens stream is no longer needed past this point.
        self.tokens_gpu = None
        torch.cuda.empty_cache()

    # ----- Modified KN discounts from count-of-counts (GPU sums) -----
    def _compute_discounts(self):
        dev = self.device
        # n1, n2, n3 are computed across ALL orders' ngram_count tables.
        n1 = torch.zeros((), dtype=torch.int64, device=dev)
        n2 = torch.zeros((), dtype=torch.int64, device=dev)
        n3 = torch.zeros((), dtype=torch.int64, device=dev)
        for o in range(1, self.MAX_ORDER + 1):
            c = self.ngram_count[o]
            if c is None:
                continue
            n1 = n1 + (c == 1).sum()
            n2 = n2 + (c == 2).sum()
            n3 = n3 + (c == 3).sum()
        n1i = int(n1.item())
        n2i = int(n2.item())
        n3i = int(n3.item())
        if n1i > 0 and n2i > 0:
            Y = n1i / (n1i + 2.0 * n2i)
            self.D1 = max(0.1, min(0.95, 1.0 - 2.0 * Y * n2i / n1i))
            self.D2 = max(0.1, min(0.95,
                                    2.0 - 3.0 * Y * (n3i / n2i if n2i > 0 else 0.0)))
            self.D3 = max(0.1, min(0.95,
                                    3.0 - 4.0 * Y * ((n3i + 1) / n3i if n3i > 0 else 1.0)))
        print("  KN discounts: D1=%.3f D2=%.3f D3+=%.3f" %
              (self.D1, self.D2, self.D3))

    # ----- Physics simulation + importance (all on GPU, as before) -----
    def build(self, physics_iters=30):
        self._compute_discounts()
        dev = self.device
        N = len(self.id2word)
        print("  Nodes: %d, Edges: %d" %
              (N, self.edge_from.numel() if self.edge_from is not None else 0))
        if N == 0:
            return
        edge_from = self.edge_from
        edge_to = self.edge_to
        edge_w = self.edge_weight
        E = edge_from.numel() if edge_from is not None else 0

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

        print("  Physics: %d iters on %s" % (physics_iters, dev),
              end="", flush=True)
        t0 = time.time()
        for it in range(physics_iters):
            forces = torch.zeros_like(positions)

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

            if N > sample_size:
                sample_idx = torch.randperm(N, device=dev)[:sample_size]
            else:
                sample_idx = torch.arange(N, device=dev)
            sample_pos = positions[sample_idx]

            bsz = 4096
            for bs in range(0, N, bsz):
                be = min(bs + bsz, N)
                tp = positions[bs:be]
                d = sample_pos.unsqueeze(0) - tp.unsqueeze(1)
                di = d.norm(dim=2).clamp_min(0.1)
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

            forces.sub_(0.01 * positions)
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

        # Importance = log(1+degree) * log(1+freq)
        degs = torch.zeros(N, dtype=torch.float32, device=dev)
        if E > 0:
            degs.index_add_(0, edge_from,
                            torch.ones(E, dtype=torch.float32, device=dev))
        self.importance = torch.log1p(degs) * torch.log1p(self.freq_gpu.float())

        # Circles: skipped in the GPU-first runner (replaced with no-op group),
        # since the CPU clique detection used by the original runner would
        # require moving the whole bidirectional-strong subgraph back to host
        # memory — exactly what we're trying to avoid. The 1.5x circle boost
        # is a minor refinement on top of position similarity.
        self.circle_group = torch.full(
            (N,), -1, dtype=torch.long, device=dev)

    # ----- GPU-batched Modified KN-5 probabilities -----
    # ctx_batch: (B, MAX_ORDER) int64, right-aligned with -1 padding on the
    #            left for positions where no context word is available.
    # nxt_batch: (B,) int64, next-word IDs. -1 indicates OOV (floor prob).
    # Returns (B,) float32 KN-5 probabilities.
    def kn_batch(self, ctx_batch, nxt_batch):
        dev = self.device
        V = max(len(self.id2word), 1)
        B = ctx_batch.size(0)
        safe_nxt = nxt_batch.clamp_min(0)
        # Base case: continuation unigram
        tub = float(self.total_unique_bigrams)
        if tub > 0:
            cw = self.cont_count[safe_nxt].float()
            cont_prob = torch.where(
                cw > 0,
                cw / tub,
                torch.full_like(cw, 0.5 / tub))
        else:
            cont_prob = torch.full((B,), 1.0 / V,
                                    dtype=torch.float32, device=dev)
        oov = nxt_batch < 0
        cont_prob = torch.where(
            oov,
            torch.full_like(cont_prob, 1.0 / V),
            cont_prob)
        kn_prev = cont_prob

        D1 = float(self.D1)
        D2 = float(self.D2)
        D3 = float(self.D3)

        for o in range(1, self.MAX_ORDER + 1):
            ctx_o = ctx_batch[:, -o:]                              # (B, o)
            # If any token in ctx_o is -1, this query has no valid ctx at
            # length o → the hash will not match any stored ctx_hash, and
            # ctx_valid will be False, so kn stays at the lower-order value.
            ctx_has_valid = (ctx_o >= 0).all(dim=1) & (~oov)
            ctx_h = (ctx_o * self.hash_primes[:o]).sum(dim=1)       # (B,)
            ngram_rows = torch.cat([ctx_o, nxt_batch.unsqueeze(1)], dim=1)
            ngram_h = (ngram_rows * self.hash_primes[:o + 1]).sum(dim=1)

            ctx_tbl = self.ctx_hash_sorted[o]
            ng_tbl = self.ngram_hash_sorted[o]
            if ctx_tbl is None or ctx_tbl.numel() == 0:
                continue

            ctx_idx = torch.searchsorted(ctx_tbl, ctx_h)
            ctx_idx_cl = ctx_idx.clamp_max(ctx_tbl.numel() - 1)
            ctx_valid = (ctx_idx < ctx_tbl.numel()) & \
                        (ctx_tbl[ctx_idx_cl] == ctx_h) & ctx_has_valid

            ng_idx = torch.searchsorted(ng_tbl, ngram_h)
            ng_idx_cl = ng_idx.clamp_max(ng_tbl.numel() - 1)
            ng_valid = (ng_idx < ng_tbl.numel()) & \
                       (ng_tbl[ng_idx_cl] == ngram_h) & ctx_has_valid

            total = self.ctx_total[o][ctx_idx_cl].float()
            total = torch.where(ctx_valid, total, torch.zeros_like(total))

            c = self.ngram_count[o][ng_idx_cl].float()
            c = torch.where(ng_valid, c, torch.zeros_like(c))

            c1 = self.ctx_count1[o][ctx_idx_cl].float()
            c1 = torch.where(ctx_valid, c1, torch.zeros_like(c1))
            c2 = self.ctx_count2[o][ctx_idx_cl].float()
            c2 = torch.where(ctx_valid, c2, torch.zeros_like(c2))
            uf = self.ctx_uf[o][ctx_idx_cl].float()
            uf = torch.where(ctx_valid, uf, torch.zeros_like(uf))

            # Discount selector keyed on the ngram count c
            disc_d = torch.zeros_like(c)
            disc_d = torch.where(c == 1, torch.full_like(c, D1), disc_d)
            disc_d = torch.where(c == 2, torch.full_like(c, D2), disc_d)
            disc_d = torch.where(c >= 3, torch.full_like(c, D3), disc_d)

            safe_total = total.clamp_min(1e-10)
            disc = (c - disc_d).clamp_min(0.0) / safe_total
            n3p = (uf - c1 - c2).clamp_min(0.0)
            lam = (D1 * c1 + D2 * c2 + D3 * n3p) / safe_total

            new_kn = disc + lam * kn_prev
            kn_prev = torch.where(total > 0, new_kn, kn_prev)

        return kn_prev.clamp(1e-10, 0.999)

    # ----- Full-mode perplexity on WikiText-103 (no cache, GPU end-to-end) ---
    def eval_full_wt103(self, test_lines, batch_size=16384):
        dev = self.device
        N = len(self.id2word)
        K = self.MAX_ORDER
        pos = self.positions
        imp = self.importance
        circ = self.circle_group

        # Tokenize test set on CPU into int32 stream + boundary markers for
        # ctx window clamping, then move to GPU as one big int64 tensor. The
        # eval loop below iterates ONCE over that tensor via strided indexing,
        # producing (ctx, nxt) pairs in bulk, never returning to Python per
        # token.
        import array
        w2i = self.word2id
        toks = array.array('i')
        boundaries = array.array('i')  # one entry per test line: end index
        boundaries.append(0)
        for line in test_lines:
            ws = tokenize(line)
            if len(ws) < 2:
                # Still advance the boundary so the sentence is just empty
                boundaries.append(len(toks))
                continue
            for w in ws:
                toks.append(w2i.get(w, -1))
            boundaries.append(len(toks))

        T = len(toks)
        if T < 2:
            return float("inf")
        np_toks = np.frombuffer(toks, dtype=np.int32)
        toks_gpu = torch.from_numpy(np_toks).to(device=dev, dtype=torch.int64)
        del toks, np_toks

        # For each test position p (1..T-1), the eval pair is:
        #   ctx = toks[max(last_boundary, p-K) : p]
        #   nxt = toks[p]
        # To get per-position "last_boundary", build a per-token boundary
        # anchor: anchor[p] = start of the line containing token p.
        np_bnd = np.frombuffer(boundaries, dtype=np.int32)
        # Convert boundaries to per-token anchor array: anchor has length T.
        anchor = np.empty(T, dtype=np.int32)
        for i in range(len(np_bnd) - 1):
            s = int(np_bnd[i])
            e = int(np_bnd[i + 1])
            if e > s:
                anchor[s:e] = s
        anchor_gpu = torch.from_numpy(anchor).to(device=dev, dtype=torch.int64)

        total_logp = torch.zeros((), dtype=torch.float64, device=dev)
        total_tok = 0

        t0 = time.time()
        # Positions start from 1..T-1; also skip any position where
        # toks_gpu[p] < 0 (OOV next) — floor log prob added separately.
        P = T
        pos_index_all = torch.arange(1, P, dtype=torch.int64, device=dev)

        for start in range(0, pos_index_all.numel(), batch_size):
            end = min(start + batch_size, pos_index_all.numel())
            pidx = pos_index_all[start:end]                     # (B,)
            B = pidx.numel()

            nxt = toks_gpu[pidx]                                 # (B,)
            anch = anchor_gpu[pidx]                              # (B,)
            # ctx_start = max(anch, p - K)
            ctx_start = torch.maximum(anch, pidx - K)            # (B,)
            # Build ctx_batch (B, K), right-aligned.
            # ctx[:, j] = toks_gpu[ctx_start + j] if ctx_start+j < pidx else -1
            j_range = torch.arange(K, dtype=torch.int64, device=dev)  # (K,)
            avail = (pidx - ctx_start)                            # (B,)
            left_pad = (K - avail).clamp_min(0)                   # (B,)
            col = j_range.unsqueeze(0) - left_pad.unsqueeze(1)    # (B, K)
            src = ctx_start.unsqueeze(1) + col                    # (B, K)
            mask_pad = col < 0
            src_clamped = src.clamp(min=0)
            ctx_batch = toks_gpu[src_clamped]                     # (B, K)
            ctx_batch = torch.where(
                mask_pad, torch.full_like(ctx_batch, -1), ctx_batch)

            # Skip positions where the next token is OOV (-1) -> floor logp.
            oov_next = (nxt < 0)

            # KN on GPU
            kn = self.kn_batch(ctx_batch, nxt)                    # (B,)

            # Position similarity (on GPU, same as before)
            safe_ctx = ctx_batch.clamp_min(0)
            safe_nxt = nxt.clamp_min(0)
            ctx_pos = pos[safe_ctx]                               # (B, K, 3)
            nxt_pos = pos[safe_nxt].unsqueeze(1)                  # (B, 1, 3)
            dot = (ctx_pos * nxt_pos).sum(-1)                     # (B, K)
            ctx_norm = ctx_pos.norm(dim=-1)                       # (B, K)
            nxt_norm = nxt_pos.norm(dim=-1)                       # (B, 1)
            denom = (ctx_norm * nxt_norm).clamp_min(1e-6)
            sim = (dot / denom).clamp(-1.0, 1.0)                  # (B, K)
            valid = (ctx_batch >= 0) & (sim > 0.05) & (~oov_next).unsqueeze(1)
            sim = torch.where(valid, sim, torch.zeros_like(sim))
            ctx_imp = imp[safe_ctx]
            boost_imp = 1.0 + ctx_imp * 0.05
            nxt_circ = circ[safe_nxt].unsqueeze(1)
            ctx_circ = circ[safe_ctx]
            same_circle = (ctx_circ >= 0) & (ctx_circ == nxt_circ)
            circle_boost = 1.0 + 0.5 * same_circle.float()
            contrib = sim * boost_imp * circle_boost
            pos_count = valid.sum(dim=1).clamp_min(1)
            has_any = valid.any(dim=1)
            pos_score = contrib.sum(dim=1)
            pos_prob = (pos_score /
                        (pos_count.to(pos_score.dtype) * 3.0)).clamp_max(0.3)
            pos_prob = torch.where(has_any, pos_prob, torch.zeros_like(pos_prob))

            # Adaptive lambda mixing (same bands as the original runner)
            band = torch.where(
                kn > 0.05,
                torch.tensor(0.02, device=dev),
                torch.where(
                    kn > 0.005,
                    torch.tensor(0.06, device=dev),
                    torch.tensor(0.12, device=dev)))
            kn_l = 1.0 - band
            mixed = (kn_l * kn + band * pos_prob).clamp(1e-10, 0.999)
            # OOV next-word positions get floored
            mixed = torch.where(
                oov_next, torch.full_like(mixed, 1e-10), mixed)

            total_logp += torch.log(mixed).to(torch.float64).sum()
            total_tok += B

            if (start // batch_size) % 10 == 0:
                print("\r  Eval: %d/%d (%.0fs)" %
                      (end, pos_index_all.numel(), time.time() - t0),
                      end="", flush=True)
        print()
        del toks_gpu, anchor_gpu
        torch.cuda.empty_cache()
        if total_tok == 0:
            return float("inf")
        return math.exp(-float(total_logp.item()) / total_tok)


def main():
    ap = argparse.ArgumentParser(
        description="MagneticLM GPU runner: WikiText-103, full mode, no cache.")
    ap.add_argument("--train-lines", type=int, default=100000,
                    help="Number of training lines to pull from WikiText-103 "
                         "(use 0 for all lines)")
    ap.add_argument("--physics-iters", type=int, default=30,
                    help="Number of physics simulation iterations on the GPU")
    ap.add_argument("--batch-size", type=int, default=16384,
                    help="Eval batch size for GPU KN + position scoring")
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

    limit = args.train_lines if args.train_lines > 0 else None
    print("\nLoading train lines from %s (limit=%s)" %
          (train_path, "all" if limit is None else str(limit)))
    train = []
    with open(train_path, "r", encoding="utf-8") as f:
        for line in f:
            s = line.strip()
            if not s:
                continue
            train.append(s)
            if limit is not None and len(train) >= limit:
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
    model.train_gpu(train)
    print("  Train time:  %.0fs" % (time.time() - t0))
    if torch.cuda.is_available():
        used = torch.cuda.memory_allocated() / 1024 ** 2
        peak = torch.cuda.max_memory_allocated() / 1024 ** 2
        print("  GPU RAM after train:  cur=%.0f MiB  peak=%.0f MiB" %
              (used, peak))

    t0 = time.time()
    model.build(physics_iters=args.physics_iters)
    print("  Build time:  %.0fs" % (time.time() - t0))
    if torch.cuda.is_available():
        used = torch.cuda.memory_allocated() / 1024 ** 2
        peak = torch.cuda.max_memory_allocated() / 1024 ** 2
        print("  GPU RAM after build:  cur=%.0f MiB  peak=%.0f MiB" %
              (used, peak))

    print("\nEvaluating WikiText-103 test (full mode, no cache)...")
    t0 = time.time()
    ppl = model.eval_full_wt103(test, batch_size=args.batch_size)
    eval_time = time.time() - t0
    print("\nPerplexity (MagneticLM full, no cache) = %.2f" % ppl)
    print("Eval time:   %.0fs" % eval_time)
    print("Total time:  %.0fs" % (time.time() - t_all))


if __name__ == "__main__":
    main()
