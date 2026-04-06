namespace ACommerce.MagneticLM.Graph;

/// <summary>
/// MagneticLM v5: Modified KN + Emergent Embeddings + Softmax Semantic
///              + Node Importance + Conceptual Circles + Fixed Cache
/// </summary>
public class WordGraph
{
    // === N-gram storage (unchanged from v4) ===
    public Dictionary<string, Dictionary<string, double>> NgramCounts { get; } = new();
    public Dictionary<string, double> NgramTotals { get; } = new();
    public Dictionary<string, HashSet<string>> ContinuationContexts { get; } = new();
    public Dictionary<string, int> UniqueFollowers { get; } = new();
    public Dictionary<string, int> NgramsWithCount1 { get; } = new();
    public Dictionary<string, int> NgramsWithCount2 { get; } = new();
    public int TotalUniqueBigrams { get; set; }
    public int MaxNgramOrder { get; set; } = 5;
    public double D1 { get; set; } = 0.5;
    public double D2 { get; set; } = 0.75;
    public double D3Plus { get; set; } = 0.9;

    // === العقد ===
    public Dictionary<string, WordNode> Nodes { get; } = new();
    public long TotalTokens { get; set; }

    // === الطبقة المعنوية ===
    public Dictionary<string, Dictionary<string, double>> SemanticEdges { get; } = new();
    public double SemanticThreshold { get; set; } = 0.1;
    public double TransitiveDecay { get; set; } = 0.5;

    // === Emergent Embeddings ===
    // كل كلمة لها "متجه" = صف أوزان علاقاتها المعنوية
    // يُحسب بعد التدريب ويُخزّن مُسبقاً
    private Dictionary<string, Dictionary<string, double>> _embeddingVectors = new();
    private Dictionary<string, double> _embeddingNorms = new();

    // === Conceptual Circles (دوائر مفاهيمية) ===
    // مجموعات كلمات تعزز بعضها البعض (cycles في الرسم)
    private List<HashSet<string>> _circles = new();

    // === Node Importance ===
    // أهمية كل كلمة = log(1+connections) * log(1+frequency)
    private Dictionary<string, double> _importance = new();

    // =========================================================================
    // N-gram registration (unchanged)
    // =========================================================================
    public void AddNgram(string[] context, string nextWord)
    {
        var key = string.Join("|", context);
        if (!NgramCounts.ContainsKey(key)) NgramCounts[key] = new();
        var isNew = !NgramCounts[key].ContainsKey(nextWord) || NgramCounts[key][nextWord] == 0;
        NgramCounts[key].TryGetValue(nextWord, out var current);
        NgramCounts[key][nextWord] = current + 1.0;
        NgramTotals.TryGetValue(key, out var total);
        NgramTotals[key] = total + 1.0;
        var newCount = current + 1.0;
        if (newCount == 1) { NgramsWithCount1.TryGetValue(key, out var v); NgramsWithCount1[key] = v + 1; }
        if (newCount == 2) { NgramsWithCount1[key]--; NgramsWithCount2.TryGetValue(key, out var v); NgramsWithCount2[key] = v + 1; }
        if (newCount == 3) { NgramsWithCount2[key]--; }
        if (isNew)
        {
            if (!ContinuationContexts.ContainsKey(nextWord)) ContinuationContexts[nextWord] = new();
            ContinuationContexts[nextWord].Add(key);
            UniqueFollowers.TryGetValue(key, out var uf);
            UniqueFollowers[key] = uf + 1;
            if (context.Length == 1) TotalUniqueBigrams++;
        }
    }

    public WordNode GetOrCreateNode(string word)
    {
        if (!Nodes.TryGetValue(word, out var node)) { node = new WordNode(word); Nodes[word] = node; }
        return node;
    }

    // =========================================================================
    // Post-training: حساب الخصومات + Embeddings + Importance + Circles
    // =========================================================================
    public void ComputeOptimalDiscounts()
    {
        long globalN1 = 0, globalN2 = 0, globalN3 = 0;
        foreach (var (_, counts) in NgramCounts)
            foreach (var (_, count) in counts)
            {
                if (count == 1) globalN1++;
                else if (count == 2) globalN2++;
                else if (count == 3) globalN3++;
            }
        if (globalN1 == 0 || globalN2 == 0) return;
        var Y = (double)globalN1 / (globalN1 + 2.0 * globalN2);
        D1 = Math.Clamp(1.0 - 2.0 * Y * globalN2 / globalN1, 0.1, 0.95);
        D2 = Math.Clamp(2.0 - 3.0 * Y * globalN3 / globalN2, 0.1, 0.95);
        D3Plus = Math.Clamp(3.0 - 4.0 * Y * (globalN3 > 0 ? (double)(globalN3 + 1) / globalN3 : 1.0), 0.1, 0.95);
    }

    /// <summary>
    /// بناء Emergent Embeddings + Node Importance + Conceptual Circles
    /// يُستدعى مرة واحدة بعد التدريب
    /// </summary>
    public void BuildPostTrainingStructures()
    {
        ComputeOptimalDiscounts();
        BuildEmergentEmbeddings();
        ComputeNodeImportance();
        DetectConceptualCircles();
    }

    /// <summary>
    /// Emergent Embeddings: متجه كل كلمة = صف أوزانها المعنوية
    /// + حساب الـ norm مسبقاً للسرعة
    /// </summary>
    private void BuildEmergentEmbeddings()
    {
        _embeddingVectors.Clear();
        _embeddingNorms.Clear();

        foreach (var (word, edges) in SemanticEdges)
        {
            // المتجه = فقط العلاقات القوية (فوق العتبة)
            var vec = new Dictionary<string, double>();
            double normSq = 0;
            foreach (var (target, weight) in edges)
            {
                if (Math.Abs(weight) >= SemanticThreshold)
                {
                    vec[target] = weight;
                    normSq += weight * weight;
                }
            }
            if (vec.Count > 0)
            {
                _embeddingVectors[word] = vec;
                _embeddingNorms[word] = Math.Sqrt(normSq);
            }
        }
    }

    /// <summary>
    /// Cosine Similarity بين "متجهي" كلمتين
    /// </summary>
    public double EmbeddingSimilarity(string word1, string word2)
    {
        if (!_embeddingVectors.TryGetValue(word1, out var vec1)) return 0;
        if (!_embeddingVectors.TryGetValue(word2, out var vec2)) return 0;
        if (!_embeddingNorms.TryGetValue(word1, out var norm1) || norm1 == 0) return 0;
        if (!_embeddingNorms.TryGetValue(word2, out var norm2) || norm2 == 0) return 0;

        // Dot product على المفاتيح المشتركة فقط (sparse)
        double dot = 0;
        foreach (var (key, val1) in vec1)
        {
            if (vec2.TryGetValue(key, out var val2))
                dot += val1 * val2;
        }

        return dot / (norm1 * norm2);
    }

    /// <summary>
    /// Node Importance: log(1+connections) * log(1+frequency)
    /// كلمة متصلة بكثير ومتكررة = مهمة
    /// </summary>
    private void ComputeNodeImportance()
    {
        _importance.Clear();
        foreach (var (word, node) in Nodes)
        {
            var connections = SemanticEdges.TryGetValue(word, out var e) ? e.Count : 0;
            _importance[word] = Math.Log(1 + connections) * Math.Log(1 + node.Frequency);
        }
    }

    public double GetImportance(string word) =>
        _importance.TryGetValue(word, out var imp) ? imp : 0;

    /// <summary>
    /// اكتشاف الدوائر المفاهيمية:
    /// مجموعات كلمات حيث كل كلمة مرتبطة معنوياً بكل الأخريات.
    /// نستخدم cliques بدل cycles (أسرع وأكثر معنى).
    /// </summary>
    private void DetectConceptualCircles()
    {
        _circles.Clear();

        // بناء رسم غير موجه من العلاقات المعنوية القوية
        var strongThreshold = SemanticThreshold * 3;
        var neighbors = new Dictionary<string, HashSet<string>>();

        foreach (var (word, edges) in SemanticEdges)
        {
            foreach (var (target, weight) in edges)
            {
                if (weight < strongThreshold) continue;
                // تحقق من ثنائية الاتجاه
                if (SemanticEdges.TryGetValue(target, out var reverseEdges) &&
                    reverseEdges.TryGetValue(word, out var reverseWeight) &&
                    reverseWeight >= strongThreshold)
                {
                    if (!neighbors.ContainsKey(word)) neighbors[word] = new();
                    neighbors[word].Add(target);
                }
            }
        }

        // اكتشاف cliques (مبسط: مجموعات من 3-5 كلمات مترابطة بالكامل)
        var processed = new HashSet<string>();
        foreach (var (word, wordNeighbors) in neighbors)
        {
            if (processed.Contains(word)) continue;

            // ابدأ من الكلمة الحالية، أضف كل جار متصل بكل أعضاء المجموعة
            var clique = new HashSet<string> { word };
            foreach (var candidate in wordNeighbors)
            {
                if (!neighbors.ContainsKey(candidate)) continue;
                // هل candidate متصل بكل أعضاء الـ clique الحاليين؟
                if (clique.All(member => neighbors.ContainsKey(member) && neighbors[member].Contains(candidate)))
                {
                    clique.Add(candidate);
                    if (clique.Count >= 5) break; // حد أقصى
                }
            }

            if (clique.Count >= 3) // حد أدنى 3 كلمات
            {
                _circles.Add(clique);
                foreach (var c in clique) processed.Add(c);
            }
        }
    }

    /// <summary>
    /// هل كلمتان في نفس الدائرة المفاهيمية؟
    /// </summary>
    public double GetCircleBoost(string word1, string word2)
    {
        foreach (var circle in _circles)
            if (circle.Contains(word1) && circle.Contains(word2))
                return 1.5; // تعزيز 50%
        return 1.0; // لا تعزيز
    }

    // =========================================================================
    // Modified Kneser-Ney (unchanged)
    // =========================================================================
    public double GetInterpolatedProbability(string[] fullContext, string word)
        => ModifiedKneserNey(fullContext, word, Math.Min(fullContext.Length, MaxNgramOrder));

    private double ModifiedKneserNey(string[] fullContext, string word, int order)
    {
        if (order == 0) return GetContinuationProbability(word);
        var contextStart = Math.Max(fullContext.Length - order, 0);
        var context = fullContext[contextStart..];
        var key = string.Join("|", context);
        if (!NgramTotals.TryGetValue(key, out var total) || total == 0)
            return ModifiedKneserNey(fullContext, word, order - 1);
        NgramCounts[key].TryGetValue(word, out var count);
        var d = count <= 0 ? 0 : count == 1 ? D1 : count == 2 ? D2 : D3Plus;
        var discountedProb = Math.Max(count - d, 0) / total;
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
        if (!ContinuationContexts.TryGetValue(word, out var contexts)) return 0.5 / TotalUniqueBigrams;
        return (double)contexts.Count / TotalUniqueBigrams;
    }

    // =========================================================================
    // MagneticLM v5: KN + Embedding Similarity + Softmax Semantic
    //              + Node Importance + Circle Boost + Fixed Cache
    // =========================================================================

    public double GetMagneticProbability(string[] fullContext, string word,
        List<(string Word, string[] Context)>? cacheEntries = null,
        bool isNewSentence = false,
        double cacheLambda = 0.08, double semanticLambda = 0.12, double embeddingLambda = 0.08)
    {
        var knProb = GetInterpolatedProbability(fullContext, word);

        // === 1. Cache (fixed: لا يُستخدم عبر جمل مستقلة في PTB) ===
        double cacheProb = 0;
        if (!isNewSentence && cacheEntries != null && cacheEntries.Count > 5)
        {
            double cacheScore = 0, totalScore = 0;
            // آخر 100 فقط
            var recent = cacheEntries.Count > 100 ? cacheEntries.GetRange(cacheEntries.Count - 100, 100) : cacheEntries;
            foreach (var (pastWord, pastContext) in recent)
            {
                var sim = ContextSimilarity(fullContext, pastContext);
                if (sim <= 0) continue;
                var weight = Math.Pow(sim, 2.0); // θ=0.5 → pow 2 for sharpening
                totalScore += weight;
                if (pastWord == word) cacheScore += weight;
            }
            cacheProb = totalScore > 0 ? cacheScore / totalScore : 0;
        }

        // === 2. Semantic with Softmax + Importance + Circle Boost ===
        double semanticScore = ComputeSoftmaxSemanticScore(fullContext, word, cacheEntries);

        // === 3. Embedding Similarity: كلمات مشابهة للسياق (غير مرتبطة مباشرة) ===
        double embeddingScore = ComputeEmbeddingScore(fullContext, word);

        // === 4. المزج التكيّفي ===
        var adaptiveSem = knProb > 0.05 ? semanticLambda * 0.15 :
                          knProb > 0.01 ? semanticLambda * 0.4 : semanticLambda;
        var adaptiveEmb = knProb > 0.05 ? embeddingLambda * 0.1 :
                          knProb > 0.01 ? embeddingLambda * 0.3 : embeddingLambda;
        var adaptiveCache = (!isNewSentence && cacheEntries != null && cacheEntries.Count > 20)
            ? cacheLambda : 0; // إيقاف cache تماماً في السياقات القصيرة

        var remainder = Math.Max(1.0 - adaptiveCache - adaptiveSem - adaptiveEmb, 0.5);
        var result = remainder * knProb
                   + adaptiveCache * cacheProb
                   + adaptiveSem * Math.Max(semanticScore, 0)
                   + adaptiveEmb * Math.Max(embeddingScore, 0);

        // طرد معنوي: يقلل الاحتمال مباشرة
        if (semanticScore < 0)
            result *= (1.0 + semanticScore * adaptiveSem * 2);

        result = Math.Max(result, knProb * 0.001);
        // حماية: الاحتمال لا يتجاوز 1 ولا ينزل تحت floor
        if (double.IsNaN(result) || result <= 0) return knProb;
        return Math.Min(result, 0.999);
    }

    /// <summary>
    /// Softmax Semantic: بدل التطبيع الخطي
    /// + Node Importance يُرجّح المساهمات
    /// + Circle Boost يُعزز كلمات الدائرة
    /// </summary>
    private double ComputeSoftmaxSemanticScore(string[] fullContext, string word,
        List<(string Word, string[] Context)>? cacheEntries)
    {
        double totalScore = 0;
        double totalWeight = 0;

        // من السياق المباشر
        foreach (var ctx in fullContext)
        {
            var raw = GetRawSemanticWeight(ctx, word);
            if (raw == 0) continue;

            // أهمية الكلمة المصدر تُرجّح مساهمتها
            var imp = GetImportance(ctx);
            var impWeight = 1.0 + imp * 0.1;

            // تعزيز الدائرة المفاهيمية
            var circleBoost = GetCircleBoost(ctx, word);

            totalScore += raw * impWeight * circleBoost;
            totalWeight += impWeight;
        }

        // من التاريخ الأخير (segment memory)
        if (cacheEntries != null)
        {
            var recentWords = cacheEntries.TakeLast(15)
                .Select(e => e.Word).Where(w => !fullContext.Contains(w)).Distinct().Take(8);
            foreach (var ctx in recentWords)
            {
                var raw = GetRawSemanticWeight(ctx, word);
                if (raw == 0) continue;
                totalScore += raw * 0.3; // وزن أقل للتاريخ
                totalWeight += 0.3;
            }
        }

        if (totalWeight == 0) return 0;
        var avgScore = totalScore / totalWeight;

        // Softmax-like: تطبيع عبر كل الأوزان المعنوية من المصدر
        // بدل weight/sum → exp(weight)/sum(exp(weights))
        return SoftmaxNormalize(fullContext[^1], word, avgScore);
    }

    private double SoftmaxNormalize(string contextWord, string targetWord, double rawScore)
    {
        if (!SemanticEdges.TryGetValue(contextWord, out var edges) || edges.Count == 0)
            return rawScore > 0 ? rawScore * 0.01 : 0;

        // Softmax: exp(score) / Σ exp(scores)
        var maxWeight = edges.Values.Where(w => Math.Abs(w) >= SemanticThreshold).DefaultIfEmpty(1).Max();
        if (maxWeight == 0) maxWeight = 1;

        var scaledScore = rawScore / maxWeight; // تطبيع لتجنب overflow
        var expScore = Math.Exp(scaledScore);
        var sumExp = edges.Values
            .Where(w => Math.Abs(w) >= SemanticThreshold)
            .Sum(w => Math.Exp(w / maxWeight));

        return sumExp > 0 ? expScore / sumExp : 0;
    }

    /// <summary>
    /// Embedding Score: تشابه cosine بين الكلمة المرشحة وكلمات السياق
    /// يلتقط كلمات مشابهة حتى بدون علاقة مباشرة
    /// </summary>
    private double ComputeEmbeddingScore(string[] fullContext, string word)
    {
        if (_embeddingVectors.Count == 0) return 0;

        double totalSim = 0;
        int count = 0;

        foreach (var ctx in fullContext)
        {
            var sim = EmbeddingSimilarity(ctx, word);
            if (sim > 0.1) // فقط التشابهات ذات المعنى
            {
                totalSim += sim;
                count++;
            }
        }

        if (count == 0) return 0;

        // تطبيع: متوسط التشابه مقسوم على عدد الكلمات في القاموس (ليصبح كالاحتمال)
        return (totalSim / count) / Math.Log(1 + Nodes.Count);
    }

    private double GetRawSemanticWeight(string from, string to)
    {
        if (!SemanticEdges.TryGetValue(from, out var edges)) return 0;
        if (!edges.TryGetValue(to, out var weight)) return 0;
        return Math.Abs(weight) >= SemanticThreshold ? weight : 0;
    }

    private static double ContextSimilarity(string[] ctx1, string[] ctx2)
    {
        if (ctx1.Length == 0 || ctx2.Length == 0) return 0;
        double score = 0, maxScore = 0;
        for (int i = 0; i < ctx1.Length; i++)
        {
            var posWeight = 1.0 + (double)i / ctx1.Length;
            maxScore += posWeight;
            for (int j = 0; j < ctx2.Length; j++)
                if (ctx1[i] == ctx2[j]) { score += posWeight * (i == j ? 1.5 : 1.0); break; }
        }
        return maxScore > 0 ? score / maxScore : 0;
    }

    // === Semantic layer ===
    public void StrengthSemanticEdge(string w1, string w2, double amount)
    {
        if (w1 == w2) return;
        AddSemanticWeight(w1, w2, amount);
        AddSemanticWeight(w2, w1, amount);
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
        e.TryGetValue(w2, out var w); return w;
    }
    public IEnumerable<(string Word, double Weight)> GetSemanticNeighbors(string word)
    {
        if (!SemanticEdges.TryGetValue(word, out var edges)) yield break;
        foreach (var (to, weight) in edges)
            if (Math.Abs(weight) >= SemanticThreshold) yield return (to, weight);
    }

    public (int Nodes, long NgramEntries, int SemanticEdges, int Circles, int EmbeddingVectors) GetStatsV5()
    {
        var nEntries = NgramCounts.Values.Sum(e => (long)e.Count);
        var sEdges = SemanticEdges.Values.Sum(e => e.Count);
        return (Nodes.Count, nEntries, sEdges, _circles.Count, _embeddingVectors.Count);
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
