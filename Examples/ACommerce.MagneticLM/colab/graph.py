"""
MagneticLM v6 - Word Graph with Physics Simulation
Port from C# to Python for Google Colab + WikiText-103

Architecture:
  Modified Kneser-Ney 5-gram
  + Physics-based word positions (5 forces: contextual, frequency, attraction, repulsion, damping)
  + Continuous Cache with Logarithmic Decay + Dynamic Theta
  + Cosine Similarity between 3D positions
  + Node Importance + Conceptual Circles
"""

import numpy as np
from collections import defaultdict
import math
from typing import Dict, List, Tuple, Optional, Set


class WordNode:
    __slots__ = ['word', 'freq', 'px', 'py', 'pz', 'vx', 'vy', 'vz']

    def __init__(self, word: str):
        self.word = word
        self.freq = 0
        # Random initial position
        self.px = np.random.uniform(-5, 5)
        self.py = np.random.uniform(-5, 5)
        self.pz = np.random.uniform(-5, 5)
        # Velocity
        self.vx = 0.0
        self.vy = 0.0
        self.vz = 0.0


class WordGraph:
    def __init__(self, max_ngram_order=5):
        self.max_order = max_ngram_order
        self.nodes: Dict[str, WordNode] = {}
        self.total_tokens = 0

        # N-gram counts
        self.ngram_counts: Dict[str, Dict[str, float]] = defaultdict(lambda: defaultdict(float))
        self.ngram_totals: Dict[str, float] = defaultdict(float)
        self.continuation_contexts: Dict[str, Set[str]] = defaultdict(set)
        self.unique_followers: Dict[str, int] = defaultdict(int)
        self.total_unique_bigrams = 0

        # Modified KN discount tracking
        self._count1: Dict[str, int] = defaultdict(int)
        self._count2: Dict[str, int] = defaultdict(int)

        # Discounts
        self.D1 = 0.5
        self.D2 = 0.75
        self.D3 = 0.9

        # Semantic edges
        self.semantic: Dict[str, Dict[str, float]] = defaultdict(lambda: defaultdict(float))
        self.semantic_threshold = 0.1

        # Physics params
        self.K_context = 2.0
        self.K_frequency = 1.5
        self.K_attraction = 0.5
        self.K_repulsion = 0.3
        self.damping = 0.15
        self.physics_lr = 0.02
        self.optimal_dist = 3.0
        self.max_radius = 15.0

        # Post-training structures
        self._importance: Dict[str, float] = {}
        self._circles: List[Set[str]] = []

    def get_or_create(self, word: str) -> WordNode:
        if word not in self.nodes:
            self.nodes[word] = WordNode(word)
        return self.nodes[word]

    # =========================================================================
    # N-gram registration
    # =========================================================================
    def add_ngram(self, context: Tuple[str, ...], next_word: str):
        key = "|".join(context)
        is_new = self.ngram_counts[key][next_word] == 0

        old_count = self.ngram_counts[key][next_word]
        self.ngram_counts[key][next_word] = old_count + 1
        self.ngram_totals[key] += 1

        nc = old_count + 1
        if nc == 1:
            self._count1[key] += 1
        if nc == 2:
            self._count1[key] -= 1
            self._count2[key] += 1
        if nc == 3:
            self._count2[key] -= 1

        if is_new:
            self.continuation_contexts[next_word].add(key)
            self.unique_followers[key] += 1
            if len(context) == 1:
                self.total_unique_bigrams += 1

    def add_semantic(self, w1: str, w2: str, amount: float):
        if w1 == w2:
            return
        self.semantic[w1][w2] += amount
        self.semantic[w2][w1] += amount

    # =========================================================================
    # Post-training
    # =========================================================================
    def build_post_training(self, physics_iterations=50):
        self._compute_discounts()
        print(f"  Discounts: D1={self.D1:.3f} D2={self.D2:.3f} D3+={self.D3:.3f}")

        print(f"  Physics simulation ({physics_iterations} iterations)...", end="", flush=True)
        self._run_physics(physics_iterations)
        print(" done.")

        self._compute_importance()
        self._detect_circles()
        print(f"  Circles: {len(self._circles)}, Importance computed for {len(self._importance)} nodes")

    def _compute_discounts(self):
        n1 = n2 = n3 = 0
        for counts in self.ngram_counts.values():
            for c in counts.values():
                if c == 1: n1 += 1
                elif c == 2: n2 += 1
                elif c == 3: n3 += 1
        if n1 == 0 or n2 == 0:
            return
        Y = n1 / (n1 + 2.0 * n2)
        self.D1 = max(0.1, min(0.95, 1.0 - 2.0 * Y * n2 / n1))
        self.D2 = max(0.1, min(0.95, 2.0 - 3.0 * Y * n3 / n2))
        self.D3 = max(0.1, min(0.95, 3.0 - 4.0 * Y * ((n3 + 1) / n3 if n3 > 0 else 1.0)))

    def _run_physics(self, iterations: int):
        """Full physics simulation with 5 forces."""
        node_list = list(self.nodes.values())
        n = len(node_list)
        if n == 0:
            return

        # Build edge lookup for speed
        edge_lookup: Dict[str, List[Tuple[str, float]]] = defaultdict(list)
        for w1, edges in self.semantic.items():
            for w2, weight in edges.items():
                if abs(weight) >= self.semantic_threshold:
                    edge_lookup[w1].append((w2, weight))

        # Sample size for repulsion/attraction (avoid O(n^2))
        sample_step = max(1, n // 100)

        for _iter in range(iterations):
            for node in node_list:
                fx, fy, fz = 0.0, 0.0, 0.0

                # Forces from semantic relations
                for other_word, weight in edge_lookup.get(node.word, []):
                    other = self.nodes.get(other_word)
                    if other is None:
                        continue
                    dx = other.px - node.px
                    dy = other.py - node.py
                    dz = other.pz - node.pz
                    dist = max(math.sqrt(dx*dx + dy*dy + dz*dz), 0.1)
                    ux, uy, uz = dx/dist, dy/dist, dz/dist

                    k = self.K_context if weight > 1.0 else self.K_frequency
                    f = k * min(weight, 10) / dist
                    fx += f * ux
                    fy += f * uy
                    fz += f * uz

                # Repulsion + Attraction (sampled)
                for i in range(0, n, sample_step):
                    other = node_list[i]
                    if other is node:
                        continue
                    dx = other.px - node.px
                    dy = other.py - node.py
                    dz = other.pz - node.pz
                    dist = max(math.sqrt(dx*dx + dy*dy + dz*dz), 0.1)
                    ux, uy, uz = dx/dist, dy/dist, dz/dist

                    # Repulsion
                    rep = -self.K_repulsion / (dist * dist + 1)
                    fx += rep * ux
                    fy += rep * uy
                    fz += rep * uz

                    # Attraction beyond optimal distance
                    if dist > self.optimal_dist:
                        att = self.K_attraction * (dist - self.optimal_dist) * 0.01
                        fx += att * ux
                        fy += att * uy
                        fz += att * uz

                # Center gravity
                fx -= 0.01 * node.px
                fy -= 0.01 * node.py
                fz -= 0.01 * node.pz

                # Newton + Damping
                node.vx = (node.vx + fx * self.physics_lr) * (1 - self.damping)
                node.vy = (node.vy + fy * self.physics_lr) * (1 - self.damping)
                node.vz = (node.vz + fz * self.physics_lr) * (1 - self.damping)

                node.px += node.vx * self.physics_lr
                node.py += node.vy * self.physics_lr
                node.pz += node.vz * self.physics_lr

                # Boundary
                mag = math.sqrt(node.px**2 + node.py**2 + node.pz**2)
                if mag > self.max_radius:
                    s = self.max_radius / mag
                    node.px *= s; node.py *= s; node.pz *= s
                    node.vx *= 0.5; node.vy *= 0.5; node.vz *= 0.5

    def _compute_importance(self):
        self._importance.clear()
        for word, node in self.nodes.items():
            conn = len(self.semantic.get(word, {}))
            self._importance[word] = math.log(1 + conn) * math.log(1 + node.freq)

    def _detect_circles(self):
        """Find cliques (fully connected groups) in strong semantic edges."""
        self._circles.clear()
        threshold = self.semantic_threshold * 3
        neighbors: Dict[str, Set[str]] = defaultdict(set)

        for w1, edges in self.semantic.items():
            for w2, weight in edges.items():
                if weight >= threshold:
                    rev = self.semantic.get(w2, {}).get(w1, 0)
                    if rev >= threshold:
                        neighbors[w1].add(w2)

        processed = set()
        for word, nbrs in neighbors.items():
            if word in processed:
                continue
            clique = {word}
            for cand in nbrs:
                if cand not in neighbors:
                    continue
                if all(m in neighbors.get(cand, set()) for m in clique):
                    clique.add(cand)
                    if len(clique) >= 5:
                        break
            if len(clique) >= 3:
                self._circles.append(clique)
                processed.update(clique)

    # =========================================================================
    # Cosine similarity between 3D positions
    # =========================================================================
    def position_similarity(self, w1: str, w2: str) -> float:
        n1 = self.nodes.get(w1)
        n2 = self.nodes.get(w2)
        if n1 is None or n2 is None:
            return 0.0
        dot = n1.px*n2.px + n1.py*n2.py + n1.pz*n2.pz
        mag1 = math.sqrt(n1.px**2 + n1.py**2 + n1.pz**2)
        mag2 = math.sqrt(n2.px**2 + n2.py**2 + n2.pz**2)
        if mag1 < 0.001 or mag2 < 0.001:
            return 0.0
        return max(-1.0, min(1.0, dot / (mag1 * mag2)))

    def get_importance(self, word: str) -> float:
        return self._importance.get(word, 0.0)

    def get_circle_boost(self, w1: str, w2: str) -> float:
        for circle in self._circles:
            if w1 in circle and w2 in circle:
                return 1.5
        return 1.0

    # =========================================================================
    # Modified Kneser-Ney
    # =========================================================================
    def kn_probability(self, context: Tuple[str, ...], word: str) -> float:
        return self._mod_kn(context, word, min(len(context), self.max_order))

    def _mod_kn(self, context: Tuple[str, ...], word: str, order: int) -> float:
        if order == 0:
            return self._cont_prob(word)

        start = max(len(context) - order, 0)
        key = "|".join(context[start:])

        total = self.ngram_totals.get(key, 0)
        if total == 0:
            return self._mod_kn(context, word, order - 1)

        count = self.ngram_counts.get(key, {}).get(word, 0)
        d = 0 if count <= 0 else self.D1 if count == 1 else self.D2 if count == 2 else self.D3
        disc = max(count - d, 0) / total

        n1 = self._count1.get(key, 0)
        n2 = self._count2.get(key, 0)
        uf = self.unique_followers.get(key, 0)
        n3p = max(uf - n1 - n2, 0)
        lam = (self.D1 * n1 + self.D2 * n2 + self.D3 * n3p) / total

        return disc + lam * self._mod_kn(context, word, order - 1)

    def _cont_prob(self, word: str) -> float:
        if self.total_unique_bigrams == 0:
            return 1.0 / max(len(self.nodes), 1)
        ctx = self.continuation_contexts.get(word)
        if ctx is None:
            return 0.5 / self.total_unique_bigrams
        return len(ctx) / self.total_unique_bigrams

    # =========================================================================
    # MagneticLM v6: KN + Physics Position + Cache + Importance + Circles
    # =========================================================================
    def magnetic_probability(self, context: Tuple[str, ...], word: str,
                             cache: Optional[List[Tuple[str, Tuple[str, ...]]]] = None,
                             is_new_sentence: bool = False) -> float:
        kn = self.kn_probability(context, word)

        # === 1. Position Similarity ===
        pos_score = 0.0
        pos_count = 0
        for ctx in context:
            sim = self.position_similarity(ctx, word)
            if sim > 0.05:
                imp = 1.0 + self.get_importance(ctx) * 0.05
                boost = self.get_circle_boost(ctx, word)
                pos_score += sim * imp * boost
                pos_count += 1
        pos_prob = min(pos_score / (pos_count * 3.0), 0.3) if pos_count > 0 else 0.0

        # === 2. Cache with Logarithmic Decay + Dynamic Theta ===
        cache_prob = 0.0
        if not is_new_sentence and cache and len(cache) > 10:
            window = 3785
            start = max(0, len(cache) - window)
            cache_score = 0.0
            total_w = 0.0

            for ci in range(start, len(cache)):
                past_word, past_ctx = cache[ci]
                sim = self._ctx_similarity(context, past_ctx)
                if sim <= 0:
                    continue
                age = len(cache) - 1 - ci
                decay = 1.0 / math.log(2.0 + age)
                theta = 2.0 / (1.0 + age * 0.01)
                w = (sim ** theta) * decay
                total_w += w
                if past_word == word:
                    cache_score += w

            cache_prob = cache_score / total_w if total_w > 0 else 0.0

        # === 3. Adaptive mixing ===
        if kn > 0.05:
            pos_l, cache_l = 0.02, 0.01
        elif kn > 0.005:
            pos_l, cache_l = 0.06, 0.03
        else:
            pos_l, cache_l = 0.12, 0.05

        if is_new_sentence or not cache or len(cache) < 10:
            cache_l = 0.0

        kn_l = 1.0 - pos_l - cache_l
        result = kn_l * kn + pos_l * pos_prob + cache_l * cache_prob

        return max(min(result, 0.999), 1e-10)

    @staticmethod
    def _ctx_similarity(ctx1: Tuple[str, ...], ctx2: Tuple[str, ...]) -> float:
        if not ctx1 or not ctx2:
            return 0.0
        score = 0.0
        mx = 0.0
        for i, w1 in enumerate(ctx1):
            pw = 1.0 + i / len(ctx1)
            mx += pw
            for j, w2 in enumerate(ctx2):
                if w1 == w2:
                    score += pw * (1.5 if i == j else 1.0)
                    break
        return score / mx if mx > 0 else 0.0

    def get_stats(self):
        ng = sum(len(v) for v in self.ngram_counts.values())
        se = sum(len(v) for v in self.semantic.values())
        return len(self.nodes), ng, se, len(self._circles)
