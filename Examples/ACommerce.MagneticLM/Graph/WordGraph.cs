namespace ACommerce.MagneticLM.Graph;

/// <summary>
/// الرسم البياني للكلمات: Modified Kneser-Ney + Cache + Semantic Layer
/// </summary>
public class WordGraph
{
    // === N-gram storage ===
    public Dictionary<string, Dictionary<string, double>> NgramCounts { get; } = new();
    public Dictionary<string, double> NgramTotals { get; } = new();
    public Dictionary<string, HashSet<string>> ContinuationContexts { get; } = new();
    public Dictionary<string, int> UniqueFollowers { get; } = new();

    /// <summary>
    /// عدد n-grams الفريدة التي ظهرت مرة واحدة فقط (n1)، مرتين (n2) - لحساب الخصومات
    /// </summary>
    public Dictionary<string, int> NgramsWithCount1 { get; } = new(); // لكل سياق: كم كلمة ظهرت مرة واحدة
    public Dictionary<string, int> NgramsWithCount2 { get; } = new(); // لكل سياق: كم كلمة ظهرت مرتين

    public int TotalUniqueBigrams { get; set; }

    public int MaxNgramOrder { get; set; } = 5;

    /// <summary>
    /// Modified Kneser-Ney: ثلاث خصومات بدل واحدة
    /// D1 لكلمات ظهرت مرة واحدة
    /// D2 لكلمات ظهرت مرتين
    /// D3 لكلمات ظهرت 3 مرات أو أكثر
    /// تُحسب تلقائياً من البيانات بعد التدريب
    /// </summary>
    public double D1 { get; set; } = 0.5;
    public double D2 { get; set; } = 0.75;
    public double D3Plus { get; set; } = 0.9;
    public bool DiscountsComputed { get; set; }

    // === العقد ===
    public Dictionary<string, WordNode> Nodes { get; } = new();
    public long TotalTokens { get; set; }

    // === الطبقة المعنوية ===
    public Dictionary<string, Dictionary<string, double>> SemanticEdges { get; } = new();
    public double SemanticThreshold { get; set; } = 0.1;
    public double TransitiveDecay { get; set; } = 0.5;

    /// <summary>
    /// تسجيل n-gram مع تحديث إحصائيات Modified Kneser-Ney
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

        // تحديث n1, n2 لحساب الخصومات
        var newCount = current + 1.0;
        if (newCount == 1) { NgramsWithCount1.TryGetValue(key, out var v); NgramsWithCount1[key] = v + 1; }
        if (newCount == 2) { NgramsWithCount1[key]--; NgramsWithCount2.TryGetValue(key, out var v); NgramsWithCount2[key] = v + 1; }
        if (newCount == 3) { NgramsWithCount2[key]--; }

        if (isNew)
        {
            if (!ContinuationContexts.ContainsKey(nextWord))
                ContinuationContexts[nextWord] = new();
            ContinuationContexts[nextWord].Add(key);

            UniqueFollowers.TryGetValue(key, out var uf);
            UniqueFollowers[key] = uf + 1;

            if (context.Length == 1)
                TotalUniqueBigrams++;
        }
    }

    /// <summary>
    /// حساب الخصومات المثلى من البيانات (بعد انتهاء التدريب)
    /// Chen & Goodman formula: Di = i - (i+1) * Y * n_{i+1} / n_i
    /// حيث Y = n1 / (n1 + 2*n2)
    /// </summary>
    public void ComputeOptimalDiscounts()
    {
        long globalN1 = 0, globalN2 = 0, globalN3 = 0;

        foreach (var (key, counts) in NgramCounts)
        {
            foreach (var (_, count) in counts)
            {
                if (count == 1) globalN1++;
                else if (count == 2) globalN2++;
                else if (count == 3) globalN3++;
            }
        }

        if (globalN1 == 0 || globalN2 == 0) { DiscountsComputed = true; return; }

        var Y = (double)globalN1 / (globalN1 + 2.0 * globalN2);

        D1 = 1.0 - 2.0 * Y * globalN2 / globalN1;
        D2 = 2.0 - 3.0 * Y * globalN3 / globalN2;
        D3Plus = 3.0 - 4.0 * Y * (globalN3 > 0 ? (double)(globalN3 + 1) / globalN3 : 1.0);

        // تقييد في نطاق معقول
        D1 = Math.Clamp(D1, 0.1, 0.95);
        D2 = Math.Clamp(D2, 0.1, 0.95);
        D3Plus = Math.Clamp(D3Plus, 0.1, 0.95);

        DiscountsComputed = true;
    }

    private double GetDiscount(double count)
    {
        if (count <= 0) return 0;
        if (count == 1) return D1;
        if (count == 2) return D2;
        return D3Plus;
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

    // =========================================================================
    // Modified Interpolated Kneser-Ney
    // =========================================================================

    public double GetInterpolatedProbability(string[] fullContext, string word)
    {
        return ModifiedKneserNey(fullContext, word, Math.Min(fullContext.Length, MaxNgramOrder));
    }

    private double ModifiedKneserNey(string[] fullContext, string word, int order)
    {
        if (order == 0)
            return GetContinuationProbability(word);

        var contextStart = fullContext.Length - order;
        if (contextStart < 0) contextStart = 0;
        var context = fullContext[contextStart..];
        var key = string.Join("|", context);

        if (!NgramTotals.TryGetValue(key, out var total) || total == 0)
            return ModifiedKneserNey(fullContext, word, order - 1);

        NgramCounts[key].TryGetValue(word, out var count);

        // Modified: خصم مختلف حسب عدد التكرارات
        var d = GetDiscount(count);
        var discountedProb = Math.Max(count - d, 0) / total;

        // lambda: مُركّب من الخصومات الثلاثة
        // λ = (D1*n1 + D2*n2 + D3+*n3+) / total
        NgramsWithCount1.TryGetValue(key, out var n1);
        NgramsWithCount2.TryGetValue(key, out var n2);
        UniqueFollowers.TryGetValue(key, out var uniqueCount);
        var n3plus = Math.Max(uniqueCount - n1 - n2, 0);

        var lambda = (D1 * n1 + D2 * n2 + D3Plus * n3plus) / total;

        return discountedProb + lambda * ModifiedKneserNey(fullContext, word, order - 1);
    }

    private double GetContinuationProbability(string word)
    {
        if (TotalUniqueBigrams == 0) return 1.0 / Math.Max(Nodes.Count, 1);

        if (!ContinuationContexts.TryGetValue(word, out var contexts))
            return 0.5 / TotalUniqueBigrams;

        return (double)contexts.Count / TotalUniqueBigrams;
    }

    // =========================================================================
    // Continuous Cache (مستوحى من Neural Cache Paper + AWD-LSTM Cache)
    // بدلاً من عدّ تكرارات فقط، نستخدم تشابه السياق:
    // - كل كلمة في التاريخ لها "بصمة سياقية" (الكلمات حولها)
    // - نقارن السياق الحالي مع سياقات الكلمة في التاريخ
    // - تشابه أعلى = احتمال أعلى (pointer mechanism)
    // =========================================================================

    /// <summary>
    /// Continuous Cache:
    /// P_cache(w) = Σ sim(current_context, past_context_i) * I(past_word_i == w)
    /// مطبّع ليكون توزيعاً احتمالياً
    /// </summary>
    public double GetCachedProbability(string[] fullContext, string word,
        List<(string Word, string[] Context)>? cacheEntries, double cacheLambda = 0.1, double theta = 0.5)
    {
        var knProb = GetInterpolatedProbability(fullContext, word);

        if (cacheEntries == null || cacheEntries.Count == 0)
            return knProb;

        // Continuous Cache: تشابه السياق الحالي مع كل ظهور سابق
        double cacheScore = 0;
        double totalScore = 0;

        foreach (var (pastWord, pastContext) in cacheEntries)
        {
            // تشابه بين السياقين (عدد الكلمات المشتركة / الأقصى)
            var similarity = ContextSimilarity(fullContext, pastContext);
            if (similarity <= 0) continue;

            // رفع التشابه لأس θ للتركيز (θ < 1 = أكثر انتشاراً، θ > 1 = أكثر تركيزاً)
            var weight = Math.Pow(similarity, 1.0 / Math.Max(theta, 0.1));
            totalScore += weight;

            if (pastWord == word)
                cacheScore += weight;
        }

        var cacheProb = totalScore > 0 ? cacheScore / totalScore : 0;
        return (1.0 - cacheLambda) * knProb + cacheLambda * cacheProb;
    }

    /// <summary>
    /// تشابه سياقي بين مصفوفتي كلمات (Jaccard-like + position decay)
    /// مستوحى من Relative Position في Transformer-XL:
    /// الكلمات القريبة في السياق أهم من البعيدة
    /// </summary>
    private static double ContextSimilarity(string[] ctx1, string[] ctx2)
    {
        if (ctx1.Length == 0 || ctx2.Length == 0) return 0;

        double score = 0;
        double maxScore = 0;

        for (int i = 0; i < ctx1.Length; i++)
        {
            // وزن الموقع: الكلمة الأخيرة (الأقرب) وزنها أعلى
            var posWeight = 1.0 + (double)i / ctx1.Length;
            maxScore += posWeight;

            for (int j = 0; j < ctx2.Length; j++)
            {
                if (ctx1[i] == ctx2[j])
                {
                    // تطابق + مكافأة إذا نفس الموقع النسبي
                    var posBonus = (i == j) ? 1.5 : 1.0;
                    score += posWeight * posBonus;
                    break;
                }
            }
        }

        return maxScore > 0 ? score / maxScore : 0;
    }

    // =========================================================================
    // MagneticLM v4: Modified KN + Continuous Cache + Segment Memory + Semantic
    //
    // يدمج أفكار من:
    //   AWD-LSTM: Continuous Cache بتشابه dot-product
    //   Transformer-XL: Segment recurrence (ذاكرة عبر الجمل)
    //   Neural Cache: θ-sharpened cache probability
    //   + الطبقة المعنوية الأصلية (الجذب/الطرد)
    // =========================================================================

    public double GetMagneticProbability(string[] fullContext, string word,
        List<(string Word, string[] Context)>? cacheEntries = null,
        double cacheLambda = 0.08, double semanticLambda = 0.1, double theta = 0.5)
    {
        var knProb = GetInterpolatedProbability(fullContext, word);

        // === 1. Continuous Cache (مع تشابه السياق) ===
        double cacheProb = 0;
        if (cacheEntries != null && cacheEntries.Count > 0)
        {
            double cacheScore = 0, totalScore = 0;
            foreach (var (pastWord, pastContext) in cacheEntries)
            {
                var sim = ContextSimilarity(fullContext, pastContext);
                if (sim <= 0) continue;
                var weight = Math.Pow(sim, 1.0 / Math.Max(theta, 0.1));
                totalScore += weight;
                if (pastWord == word) cacheScore += weight;
            }
            cacheProb = totalScore > 0 ? cacheScore / totalScore : 0;
        }

        // === 2. Semantic: جذب/طرد من السياق الكامل + التاريخ ===
        double semanticScore = 0;
        int semCount = 0;

        // السياق المباشر (n-gram window)
        foreach (var ctx in fullContext)
        {
            var sp = GetNormalizedSemanticProb(ctx, word);
            if (sp != 0) { semanticScore += sp; semCount++; }
        }

        // Segment Memory: آخر 10 كلمات فريدة من التاريخ (خارج السياق المباشر)
        // مستوحى من Transformer-XL segment recurrence
        if (cacheEntries != null)
        {
            var recentUnique = cacheEntries.TakeLast(20)
                .Select(e => e.Word)
                .Where(w => !fullContext.Contains(w))
                .Distinct().Take(10);

            foreach (var ctx in recentUnique)
            {
                var sp = GetNormalizedSemanticProb(ctx, word);
                if (sp != 0)
                {
                    // اضمحلال: كلمات أبعد في التاريخ أضعف (relative position)
                    semanticScore += sp * 0.4;
                    semCount++;
                }
            }
        }

        var semanticProb = semCount > 0 ? semanticScore / semCount : 0;

        // === 3. المزج التكيّفي ===
        // المعنوي يساهم أكثر عندما KN غير واثق
        var adaptiveSem = knProb > 0.05 ? semanticLambda * 0.15 :
                          knProb > 0.01 ? semanticLambda * 0.4 :
                                          semanticLambda;

        // Cache يساهم أكثر في الوثائق الطويلة
        var adaptiveCache = cacheEntries != null && cacheEntries.Count > 20
            ? cacheLambda : cacheLambda * 0.3; // قلل Cache في السياقات القصيرة (PTB)

        double result;
        if (semanticProb >= 0)
        {
            var remainder = 1.0 - adaptiveCache - adaptiveSem;
            result = remainder * knProb + adaptiveCache * cacheProb + adaptiveSem * semanticProb;
        }
        else
        {
            result = (1.0 - adaptiveCache) * knProb + adaptiveCache * cacheProb;
            result *= (1.0 + semanticProb * adaptiveSem);
            result = Math.Max(result, knProb * 0.01);
        }

        return (double.IsNaN(result) || result <= 0) ? knProb : result;
    }

    private double GetNormalizedSemanticProb(string from, string to)
    {
        if (!SemanticEdges.TryGetValue(from, out var edges)) return 0;
        if (!edges.TryGetValue(to, out var weight)) return 0;
        if (Math.Abs(weight) < SemanticThreshold) return 0;

        var positiveSum = edges.Values.Where(w => w > SemanticThreshold).Sum();
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

    public double GetSemanticWeight(string w1, string w2)
    {
        if (!SemanticEdges.TryGetValue(w1, out var e)) return 0;
        e.TryGetValue(w2, out var w);
        return w;
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
