namespace ACommerce.MagneticLM.Graph;

/// <summary>
/// الرسم البياني للكلمات: عقد + حواف بطبقتين من الأوزان.
/// الطبقة 1 (سياقية): كم مرة ظهرت B بعد A مباشرة
/// الطبقة 2 (معنوية): قوة الارتباط المفاهيمي بين A و B
///                     تُبنى من اكتشاف العلاقات على عمق ±2
/// </summary>
public class WordGraph
{
    /// <summary>
    /// كل العقد (كلمات) مفهرسة بالنص
    /// </summary>
    public Dictionary<string, WordNode> Nodes { get; } = new();

    /// <summary>
    /// حواف سياقية: [من][إلى] = وزن التتابع
    /// </summary>
    public Dictionary<string, Dictionary<string, double>> ContextualEdges { get; } = new();

    /// <summary>
    /// حواف معنوية: [كلمة1][كلمة2] = وزن الارتباط المفاهيمي
    /// </summary>
    public Dictionary<string, Dictionary<string, double>> SemanticEdges { get; } = new();

    /// <summary>
    /// عتبة إعادة التقييم المعنوي - لا تعالج علاقات ضعيفة جداً
    /// </summary>
    public double SemanticThreshold { get; set; } = 0.1;

    /// <summary>
    /// معامل إضمحلال العلاقات العابرة (transitive decay)
    /// </summary>
    public double TransitiveDecay { get; set; } = 0.5;

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
    /// تسجيل تتابع سياقي: الكلمة A ظهرت قبل B
    /// </summary>
    public void AddContextualEdge(string from, string to)
    {
        if (!ContextualEdges.ContainsKey(from))
            ContextualEdges[from] = new();

        ContextualEdges[from].TryGetValue(to, out var current);
        ContextualEdges[from][to] = current + 1.0;

        // تحديث العقد
        GetOrCreateNode(from).TotalOutgoing++;
        GetOrCreateNode(to).TotalIncoming++;
    }

    /// <summary>
    /// تقوية الوزن المعنوي بين كلمتين
    /// </summary>
    public void StrengthSemanticEdge(string word1, string word2, double amount)
    {
        if (word1 == word2) return;

        // ثنائية الاتجاه
        AddSemanticWeight(word1, word2, amount);
        AddSemanticWeight(word2, word1, amount);
    }

    private void AddSemanticWeight(string a, string b, double amount)
    {
        if (!SemanticEdges.ContainsKey(a))
            SemanticEdges[a] = new();
        SemanticEdges[a].TryGetValue(b, out var current);
        SemanticEdges[a][b] = current + amount;
    }

    /// <summary>
    /// الحصول على الوزن السياقي المُطبّع (احتمال)
    /// </summary>
    public double GetContextualWeight(string from, string to)
    {
        if (!ContextualEdges.TryGetValue(from, out var edges)) return 0;
        if (!edges.TryGetValue(to, out var weight)) return 0;
        var total = Nodes[from].TotalOutgoing;
        return total > 0 ? weight / total : 0;
    }

    /// <summary>
    /// الحصول على الوزن المعنوي
    /// </summary>
    public double GetSemanticWeight(string word1, string word2)
    {
        if (!SemanticEdges.TryGetValue(word1, out var edges)) return 0;
        edges.TryGetValue(word2, out var weight);
        return weight;
    }

    /// <summary>
    /// الحصول على كل الجيران السياقيين (الكلمات التي تأتي بعد word)
    /// </summary>
    public IEnumerable<(string Word, double Weight)> GetContextualNeighbors(string word)
    {
        if (!ContextualEdges.TryGetValue(word, out var edges)) yield break;
        var total = Nodes[word].TotalOutgoing;
        foreach (var (to, count) in edges)
            yield return (to, total > 0 ? count / total : 0);
    }

    /// <summary>
    /// الحصول على كل الجيران المعنويين
    /// </summary>
    public IEnumerable<(string Word, double Weight)> GetSemanticNeighbors(string word)
    {
        if (!SemanticEdges.TryGetValue(word, out var edges)) yield break;
        foreach (var (to, weight) in edges)
            if (weight >= SemanticThreshold)
                yield return (to, weight);
    }

    /// <summary>
    /// إحصائيات
    /// </summary>
    public (int Nodes, int ContextualEdges, int SemanticEdges) GetStats()
    {
        var cEdges = ContextualEdges.Values.Sum(e => e.Count);
        var sEdges = SemanticEdges.Values.Sum(e => e.Count);
        return (Nodes.Count, cEdges, sEdges);
    }
}

/// <summary>
/// عقدة في الرسم البياني = كلمة واحدة
/// </summary>
public class WordNode
{
    public string Word { get; }
    public int Frequency { get; set; }
    public int TotalOutgoing { get; set; }
    public int TotalIncoming { get; set; }

    /// <summary>
    /// الإثارة الحالية (تُستخدم أثناء التوليد)
    /// </summary>
    public double Excitation { get; set; }

    /// <summary>
    /// قوة الطرد المؤقتة (لمنع التكرار)
    /// </summary>
    public double Repulsion { get; set; }

    public WordNode(string word) => Word = word;
}
