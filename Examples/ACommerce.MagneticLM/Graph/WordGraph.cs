namespace ACommerce.MagneticLM.Graph;

/// <summary>
/// الرسم البياني للكلمات مع دعم n-gram + Kneser-Ney + طبقة معنوية.
/// </summary>
public class WordGraph
{
    // === N-gram storage ===
    // المفتاح: السياق (كلمات مفصولة بـ |)
    // القيمة: [الكلمة التالية → عدد مرات الظهور]
    public Dictionary<string, Dictionary<string, double>> NgramCounts { get; } = new();
    public Dictionary<string, double> NgramTotals { get; } = new();

    /// <summary>
    /// عدد السياقات الفريدة التي ظهرت فيها كلمة كتكملة.
    /// Kneser-Ney يستخدم هذا: كلمة تظهر بعد سياقات متنوعة → احتمال أعلى في سياقات جديدة.
    /// مثال: "the" تظهر بعد آلاف السياقات → continuation count عالي
    ///        "Francisco" تظهر بعد "San" فقط → continuation count = 1
    /// </summary>
    public Dictionary<string, HashSet<string>> ContinuationContexts { get; } = new();

    /// <summary>
    /// عدد الأنواع الفريدة التي تلي كل سياق (لحساب lambda في KN).
    /// |{w : c(context, w) > 0}|
    /// </summary>
    public Dictionary<string, int> UniqueFollowers { get; } = new();

    /// <summary>
    /// إجمالي عدد السياقات الفريدة في كل رتبة (للتطبيع)
    /// </summary>
    public int TotalUniqueBigrams { get; set; }

    public int MaxNgramOrder { get; set; } = 5;
    public double Discount { get; set; } = 0.75;

    // === العقد ===
    public Dictionary<string, WordNode> Nodes { get; } = new();
    public long TotalTokens { get; set; }

    // === الطبقة المعنوية ===
    public Dictionary<string, Dictionary<string, double>> SemanticEdges { get; } = new();
    public double SemanticThreshold { get; set; } = 0.1;
    public double TransitiveDecay { get; set; } = 0.5;

    /// <summary>
    /// تسجيل n-gram مع تحديث إحصائيات Kneser-Ney
    /// </summary>
    public void AddNgram(string[] context, string nextWord)
    {
        var key = string.Join("|", context);

        if (!NgramCounts.ContainsKey(key))
            NgramCounts[key] = new();

        var isNew = !NgramCounts[key].ContainsKey(nextWord) || NgramCounts[key][nextWord] == 0;

        NgramCounts[key].TryGetValue(nextWord, out var current);
        NgramCounts[key][nextWord] = current + 1.0;

        NgramTotals.TryGetValue(key, out var total);
        NgramTotals[key] = total + 1.0;

        // تحديث Continuation Counts: في كم سياق فريد ظهرت هذه الكلمة؟
        if (isNew)
        {
            if (!ContinuationContexts.ContainsKey(nextWord))
                ContinuationContexts[nextWord] = new();
            ContinuationContexts[nextWord].Add(key);

            // عدد الكلمات الفريدة بعد هذا السياق
            UniqueFollowers.TryGetValue(key, out var uf);
            UniqueFollowers[key] = uf + 1;

            // تحديث إجمالي البيغرامات الفريدة
            if (context.Length == 1)
                TotalUniqueBigrams++;
        }
    }

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
    /// Interpolated Kneser-Ney Smoothing:
    ///
    /// P_KN(w|context) = max(count(context,w) - d, 0) / count(context)
    ///                  + λ(context) * P_KN(w|shorter_context)
    ///
    /// حيث:
    ///   d = معامل الخصم (0.75)
    ///   λ(context) = d * |{w : count(context,w) > 0}| / count(context)
    ///   P_continuation(w) = |{v : count(v,w) > 0}| / |total unique bigrams|
    ///
    /// هذا هو الفرق الجوهري عن التطبيق السابق:
    /// - λ محسوب ديناميكياً لكل سياق (ليس أوزاناً ثابتة)
    /// - المستوى الأدنى يستخدم continuation probability (ليس unigram عادي)
    /// </summary>
    public double GetInterpolatedProbability(string[] fullContext, string word)
    {
        return KneserNeyRecursive(fullContext, word, fullContext.Length);
    }

    private double KneserNeyRecursive(string[] fullContext, string word, int order)
    {
        // الحالة الأساسية: Continuation probability (أو unigram)
        if (order == 0)
        {
            return GetContinuationProbability(word);
        }

        var contextStart = fullContext.Length - order;
        if (contextStart < 0) contextStart = 0;
        var context = fullContext[contextStart..];
        var key = string.Join("|", context);

        if (!NgramTotals.TryGetValue(key, out var total) || total == 0)
        {
            // سياق لم نره → تراجع مباشرة للرتبة الأقل
            return KneserNeyRecursive(fullContext, word, order - 1);
        }

        // الاحتمال المخصوم
        NgramCounts[key].TryGetValue(word, out var count);
        var discountedProb = Math.Max(count - Discount, 0) / total;

        // lambda: وزن التراجع (backoff weight)
        // = d * عدد الكلمات الفريدة بعد هذا السياق / مجموع العدّ
        UniqueFollowers.TryGetValue(key, out var uniqueCount);
        var lambda = (Discount * uniqueCount) / total;

        // الاحتمال المُركّب: مخصوم + lambda * تراجع
        return discountedProb + lambda * KneserNeyRecursive(fullContext, word, order - 1);
    }

    /// <summary>
    /// Continuation Probability:
    /// P_cont(w) = |{v : count(v,w) > 0}| / |total unique bigrams|
    ///
    /// "كم سياق فريد ظهرت بعده هذه الكلمة؟"
    /// كلمة تظهر في سياقات متنوعة → احتمال أعلى في سياقات جديدة
    /// </summary>
    private double GetContinuationProbability(string word)
    {
        if (TotalUniqueBigrams == 0) return 1.0 / Math.Max(Nodes.Count, 1);

        if (!ContinuationContexts.TryGetValue(word, out var contexts))
            return 0.5 / TotalUniqueBigrams; // تنعيم أدنى

        return (double)contexts.Count / TotalUniqueBigrams;
    }

    /// <summary>
    /// احتمال مع الطبقة المعنوية
    /// </summary>
    public double GetMagneticProbability(string[] fullContext, string word, double semanticLambda = 0.15)
    {
        var ngramProb = GetInterpolatedProbability(fullContext, word);

        // الطبقة المعنوية: جمع الجذب من كل كلمات السياق
        double semanticProb = 0;
        foreach (var ctx in fullContext)
        {
            semanticProb += GetNormalizedSemanticProb(ctx, word);
        }
        semanticProb /= fullContext.Length;

        // مزج تكيّفي
        var adaptiveLambda = ngramProb > 0.01 ? semanticLambda * 0.3 : semanticLambda;
        var result = (1.0 - adaptiveLambda) * ngramProb + adaptiveLambda * semanticProb;

        return double.IsNaN(result) || double.IsInfinity(result) ? ngramProb : result;
    }

    private double GetNormalizedSemanticProb(string from, string to)
    {
        if (!SemanticEdges.TryGetValue(from, out var edges)) return 0;
        if (!edges.TryGetValue(to, out var weight)) return 0;
        if (Math.Abs(weight) < SemanticThreshold) return 0;

        var strongEdges = edges.Values.Where(w => Math.Abs(w) >= SemanticThreshold);
        var positiveSum = strongEdges.Where(w => w > 0).Sum();
        if (positiveSum <= 0) return 0;

        return weight / positiveSum;
    }

    // === Semantic layer ===
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
            if (Math.Abs(weight) >= SemanticThreshold)
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
