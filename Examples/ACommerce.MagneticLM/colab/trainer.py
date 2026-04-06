"""
MagneticLM Trainer - Training as accounting entries.
Each sentence = chain of entries recording contextual + semantic relationships.
"""

from graph import WordGraph
from typing import List


def tokenize(sentence: str) -> List[str]:
    """Simple whitespace tokenizer with punctuation removal."""
    for ch in '.,;!?؟،"()[]{}':
        sentence = sentence.replace(ch, ' ')
    return [w for w in sentence.lower().split() if len(w) > 0]


class Trainer:
    def __init__(self, graph: WordGraph):
        self.graph = graph
        self.sentences_trained = 0

    def train_sentence(self, sentence: str):
        words = tokenize(sentence)
        if len(words) < 2:
            return

        # === 1. Register n-grams at all depths ===
        for i in range(len(words)):
            node = self.graph.get_or_create(words[i])
            node.freq += 1
            self.graph.total_tokens += 1

            if i < len(words) - 1:
                next_word = words[i + 1]
                for order in range(1, self.graph.max_order + 1):
                    if order > i + 1:
                        break
                    context = tuple(words[i + 1 - order:i + 1])
                    self.graph.add_ngram(context, next_word)

        # === 2. Semantic discovery at depth +/-2 ===
        for i in range(len(words)):
            for d in range(-2, 3):
                if d == 0:
                    continue
                j = i + d
                if 0 <= j < len(words):
                    weight = 1.0 if abs(d) <= 1 else 0.5
                    self.graph.add_semantic(words[i], words[j], weight * 0.1)

            # Reward/Penalty
            if i < len(words) - 1:
                actual = words[i + 1]
                neighbors = list(self.graph.semantic.get(words[i], {}).items())[:20]
                for neighbor, w in neighbors:
                    if neighbor == actual:
                        self.graph.add_semantic(words[i], actual, 0.05)
                    elif w > 0.5:
                        self.graph.add_semantic(words[i], neighbor, -0.02)

        # === 3. Transitive propagation (every 200 sentences) ===
        self.sentences_trained += 1
        if self.sentences_trained % 200 == 0:
            self._propagate(words)

        if self.sentences_trained % 1000 == 0:
            print(f"\r  Training: {self.sentences_trained:,} sentences...", end="", flush=True)

    def _propagate(self, words: List[str]):
        threshold = self.graph.semantic_threshold
        decay = 0.5
        for word in words:
            neighbors = sorted(
                ((n, w) for n, w in self.graph.semantic.get(word, {}).items() if abs(w) >= threshold),
                key=lambda x: -abs(x[1])
            )[:8]
            for neighbor, w1 in neighbors:
                for trans, w2 in sorted(
                    ((n, w) for n, w in self.graph.semantic.get(neighbor, {}).items() if abs(w) >= threshold),
                    key=lambda x: -abs(x[1])
                )[:8]:
                    if trans == word:
                        continue
                    tw = w1 * w2 * decay * 0.01
                    if tw > 0.001:
                        self.graph.add_semantic(word, trans, tw)

    def train_batch(self, sentences):
        for s in sentences:
            self.train_sentence(s)
        if self.sentences_trained >= 1000:
            print()
