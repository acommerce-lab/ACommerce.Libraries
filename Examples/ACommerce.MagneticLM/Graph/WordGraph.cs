namespace ACommerce.MagneticLM.Graph;

/// <summary>
/// الرسم البياني للكلمات مع دعم n-gram (حتى 5-gram).
///
/// الطبقة 1 (سياقية): أوزان n-gram بأعماق متعددة
///   - unigram: P(w) = freq(w) / total
///   - bigram:  P(w|w1) = count(w1,w) / count(w1)
///   - trigram: P(w|w1,w2) = count(w1,w2,w) / count(w1,w2)
///   - وهكذا حتى 5-gram
///
/// الطبقة 2 (معنوية): علاقات مفاهيمية على عمق ±2
/// </summary>
public class WordGraph
{
    // === N-gram storage ===
    // المفتاح: السياق (كلمة واحدة أو أكثر مفصولة بـ |)
    // القيمة: [الكلمة التالية → عدد مرات الظهور]

    /// <summary>
    /// n-gram counts: ["w1|w2"]["w3"] = count of "w1 w2 w3"
    /// </summary>
    public Dictionary<string, Dictionary<string, double>> NgramCounts { get; } = new();

    /// <summary>
    /// مجموع العدّ لكل سياق (للتطبيع السريع)
    /// </summary>
    public Dictionary<string, double> NgramTotals { get; } = new();

    /// <summary>
    /// عدد السياقات الفريدة التي ظهرت فيها كل كلمة كتكملة
    /// (لحساب Kneser-Ney continuation probability)
    /// </summary>
    public Dictionary<string, int> ContinuationCounts { get; } = new();

    /// <summary>
    /// أقصى عمق n-gram (افتراضي: 4 = fourgram)
    /// </summary>
    public int MaxNgramOrder { get; set; } = 4;

    /// <summary>
    /// معامل الخصم لـ Kneser-Ney (عادةً 0.75)
    /// </summary>
    public double Discount { get; set; } = 0.75;

    // === العقد ===
    public Dictionary<string, WordNode> Nodes { get; } = new();
    public long TotalTokens { get; set; }

    // === الطبقة المعنوية ===
    public Dictionary<string, Dictionary<string, double>> SemanticEdges { get; } = new();
    public double SemanticThreshold { get; set; } = 0.1;
    public double TransitiveDecay { get; set; } = 0.5;

    /// <summary>
    /// تسجيل n-gram: سياق + كلمة تالية
    /// </summary>
    public void AddNgram(string[] context, string nextWord)
    {
        var key = string.Join("|", context);
        if (!NgramCounts.ContainsKey(key))
            NgramCounts[key] = new();

        NgramCounts[key].TryGetValue(nextWord, out var current);
        NgramCounts[key][nextWord] = current + 1.0;

        NgramTotals.TryGetValue(key, out var total);
        NgramTotals[key] = total + 1.0;

        // تحديث continuation count (عدد السياقات الفريدة لكل كلمة)
        var contKey = $"{key}→{nextWord}";
        if (!NgramCounts[key].ContainsKey(nextWord) || current == 0)
        {
            ContinuationCounts.TryGetValue(nextWord, out var cont);
            ContinuationCounts[nextWord] = cont + 1;
        }
    }

    /// <summary>
    /// الحصول على عقدة أو إنشاؤها
    /// </summary>
    public WordNode GetOrCreateNode(string word)
    {
        if (!Nodes.TryGetValue(word, out var node))
        {
            node = new WordNode(word);
            Nodes[word] = node;
        }
        return node;
    }

    /// <summary>
    /// احتمال n-gram مع Interpolated Backoff:
    /// P(w|w1,w2,w3) = λ4 * P_4gram + λ3 * P_3gram + λ2 * P_2gram + λ1 * P_1gram
    /// مع تنعيم مستوحى من Kneser-Ney
    /// </summary>
    public double GetInterpolatedProbability(string[] fullContext, string word)
    {
        double prob = 0;
        double totalWeight = 0;

        // من الأطول للأقصر (4-gram → 3-gram → bigram → unigram)
        for (int order = Math.Min(fullContext.Length, MaxNgramOrder); order >= 1; order--)
        {
            var context = fullContext[^order..];
            var key = string.Join("|", context);

            if (NgramTotals.TryGetValue(key, out var total) && total > 0)
            {
                NgramCounts[key].TryGetValue(word, out var count);

                // Discounted probability
                var discounted = Math.Max(count - Discount, 0) / total;

                // وزن أعلى للسياقات الأطول (أكثر تحديداً)
                var weight = Math.Pow(2.0, order);
                prob += weight * discounted;
                totalWeight += weight;
            }
        }

        // Unigram fallback
        if (Nodes.TryGetValue(word, out var node) && TotalTokens > 0)
        {
            var unigramProb = (double)node.Frequency / TotalTokens;
            prob += 1.0 * unigramProb;
            totalWeight += 1.0;
        }

        return totalWeight > 0 ? prob / totalWeight : 0;
    }

    /// <summary>
    /// احتمال مع الطبقة المعنوية:
    /// P_final = (1-λ_sem) * P_ngram + λ_sem * P_semantic
    /// الطبقة المعنوية تدعم فقط عندما n-gram ضعيف
    /// </summary>
    public double GetMagneticProbability(string[] fullContext, string word, double semanticLambda = 0.15)
    {
        var ngramProb = GetInterpolatedProbability(fullContext, word);

        // الطبقة المعنوية: جمع الجذب من كل كلمات السياق (وليس آخر كلمة فقط)
        double semanticProb = 0;
        foreach (var ctx in fullContext)
        {
            semanticProb += GetNormalizedSemanticProb(ctx, word);
        }
        semanticProb /= fullContext.Length; // متوسط

        // مزج تكيّفي: كلما كان n-gram أقوى، قلّ تأثير المعنوي
        var adaptiveLambda = ngramProb > 0.01 ? semanticLambda * 0.3 : semanticLambda;
        var result = (1.0 - adaptiveLambda) * ngramProb + adaptiveLambda * semanticProb;

        // حماية
        return double.IsNaN(result) || double.IsInfinity(result) ? ngramProb : result;
    }

    /// <summary>
    /// احتمال معنوي مُطبّع:
    /// - أوزان ضعيفة (|w| < threshold) → تُتجاهل (ضوضاء)
    /// - أوزان موجبة قوية → جذب (تزيد الاحتمال)
    /// - أوزان سالبة قوية → طرد (تُنقص الاحتمال)
    /// </summary>
    private double GetNormalizedSemanticProb(string from, string to)
    {
        if (!SemanticEdges.TryGetValue(from, out var edges)) return 0;
        if (!edges.TryGetValue(to, out var weight)) return 0;

        // تجاهل الضعيفة (ضوضاء)
        if (Math.Abs(weight) < SemanticThreshold) return 0;

        // التطبيع على الأوزان القوية فقط (موجبة + سالبة)
        var strongEdges = edges.Values.Where(w => Math.Abs(w) >= SemanticThreshold);
        var positiveSum = strongEdges.Where(w => w > 0).Sum();

        if (positiveSum <= 0) return 0;

        // الموجبة → احتمال جذب (0 إلى 1)
        // السالبة → عقوبة (تقلل الاحتمال)
        return weight / positiveSum; // سالب يعني طرد، موجب يعني جذب
    }

    // === Semantic layer (unchanged) ===
    public void StrengthSemanticEdge(string word1, string word2, double amount)
    {
        if (word1 == word2) return;
        AddSemanticWeight(word1, word2, amount);
        AddSemanticWeight(word2, word1, amount);
    }

    private void AddSemanticWeight(string a, string b, double amount)
    {
        if (!SemanticEdges.ContainsKey(a)) SemanticEdges[a] = new();
        SemanticEdges[a].TryGetValue(b, out var current);
        SemanticEdges[a][b] = current + amount;
    }

    public double GetSemanticWeight(string word1, string word2)
    {
        if (!SemanticEdges.TryGetValue(word1, out var edges)) return 0;
        edges.TryGetValue(word2, out var weight);
        return weight;
    }

    public IEnumerable<(string Word, double Weight)> GetSemanticNeighbors(string word)
    {
        if (!SemanticEdges.TryGetValue(word, out var edges)) yield break;
        foreach (var (to, weight) in edges)
            if (Math.Abs(weight) >= SemanticThreshold) // قوية فقط: جذب أو طرد
                yield return (to, weight);
    }

    public (int Nodes, long NgramEntries, int SemanticEdges) GetStats()
    {
        var nEntries = NgramCounts.Values.Sum(e => (long)e.Count);
        var sEdges = SemanticEdges.Values.Sum(e => e.Count);
        return (Nodes.Count, nEntries, sEdges);
    }
}

public class WordNode
{
    public string Word { get; }
    public int Frequency { get; set; }
    public double Excitation { get; set; }
    public double Repulsion { get; set; }

    public WordNode(string word) => Word = word;
}
