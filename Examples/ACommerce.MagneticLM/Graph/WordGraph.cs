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
    // Cache Model: كلمات ظهرت مؤخراً في النص أكثر احتمالاً للتكرار
    // =========================================================================

    /// <summary>
    /// P_cache(w|history) = count(w in history) / |history|
    /// المزج: P = (1-λ_cache) * P_KN + λ_cache * P_cache
    /// </summary>
    public double GetCachedProbability(string[] fullContext, string word,
        string[] recentHistory, double cacheLambda = 0.1)
    {
        var knProb = GetInterpolatedProbability(fullContext, word);

        if (recentHistory == null || recentHistory.Length == 0)
            return knProb;

        // عدد ظهور الكلمة في آخر N كلمة
        var cacheCount = recentHistory.Count(w => w == word);
        var cacheProb = (double)cacheCount / recentHistory.Length;

        return (1.0 - cacheLambda) * knProb + cacheLambda * cacheProb;
    }

    // =========================================================================
    // MagneticLM: KN + Cache + Semantic (الثلاثة معاً)
    // =========================================================================

    /// <summary>
    /// P_magnetic = (1 - λ_cache - λ_sem) * P_KN
    ///            + λ_cache * P_cache
    ///            + λ_sem * P_semantic
    ///
    /// الطبقة المعنوية تُستخدم فقط عندما KN غير واثق (entropy عالي)
    /// → تدفع الاحتمال نحو كلمات مرتبطة معنوياً
    /// → تطرد عن كلمات مرتبطة سلبياً
    /// </summary>
    public double GetMagneticProbability(string[] fullContext, string word,
        string[]? recentHistory = null, double cacheLambda = 0.1, double semanticLambda = 0.1)
    {
        var knProb = GetInterpolatedProbability(fullContext, word);

        // === Cache ===
        double cacheProb = 0;
        if (recentHistory != null && recentHistory.Length > 0)
        {
            var cacheCount = recentHistory.Count(w => w == word);
            cacheProb = (double)cacheCount / recentHistory.Length;
        }

        // === Semantic: جمع قوى الجذب/الطرد من كل كلمات السياق ===
        double semanticBoost = 0;
        int semanticContributors = 0;
        foreach (var ctx in fullContext)
        {
            var sp = GetNormalizedSemanticProb(ctx, word);
            if (sp != 0)
            {
                semanticBoost += sp;
                semanticContributors++;
            }
        }
        // إضافة تأثير الكلمات الأخيرة في التاريخ (ليست فقط السياق المباشر)
        if (recentHistory != null)
        {
            // آخر 5 كلمات غير موجودة في السياق المباشر
            var extraContext = recentHistory.TakeLast(5)
                .Where(w => !fullContext.Contains(w)).Distinct();
            foreach (var ctx in extraContext)
            {
                var sp = GetNormalizedSemanticProb(ctx, word);
                if (sp != 0)
                {
                    semanticBoost += sp * 0.5; // نصف الوزن للكلمات الأبعد
                    semanticContributors++;
                }
            }
        }

        var semanticProb = semanticContributors > 0 ? semanticBoost / semanticContributors : 0;

        // مزج تكيّفي: المعنوي يساهم أكثر عندما KN غير واثق
        var adaptiveSemanticLambda = knProb > 0.05 ? semanticLambda * 0.2 : // KN واثق → قلل المعنوي
                                    knProb > 0.01 ? semanticLambda * 0.5 : // KN متوسط
                                                    semanticLambda;         // KN ضعيف → كامل المعنوي

        // المعنوي السالب (طرد) يُطبّق كعقوبة مباشرة
        double result;
        if (semanticProb >= 0)
        {
            result = (1.0 - cacheLambda - adaptiveSemanticLambda) * knProb
                   + cacheLambda * cacheProb
                   + adaptiveSemanticLambda * semanticProb;
        }
        else
        {
            // طرد: يقلل الاحتمال مباشرة (لكن لا يصل للصفر)
            result = (1.0 - cacheLambda) * knProb
                   + cacheLambda * cacheProb;
            result *= (1.0 + semanticProb * adaptiveSemanticLambda); // semanticProb سالب → يقلل
            result = Math.Max(result, knProb * 0.01); // لا ينزل تحت 1% من KN
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
