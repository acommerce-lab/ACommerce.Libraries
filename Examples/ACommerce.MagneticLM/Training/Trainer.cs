using ACommerce.MagneticLM.Graph;

namespace ACommerce.MagneticLM.Training;

/// <summary>
/// المدرّب: يستقبل جملاً ويسجلها كقيود في الرسم البياني.
///
/// لكل جملة:
/// 1. تسجيل الأوزان السياقية (كلمة→كلمة تالية)
/// 2. اكتشاف العلاقات المعنوية بعمق ±2
/// 3. إعادة تقييم العلاقات العابرة (ارتقاء معنوي)
///
/// التدريب = تسجيل قيود. لا backpropagation. لا gradient descent.
/// </summary>
public class Trainer
{
    private readonly WordGraph _graph;
    private readonly double _transitiveDecay;
    private readonly double _semanticThreshold;

    /// <summary>
    /// عدد الجمل المدرّبة
    /// </summary>
    public int SentencesTrained { get; private set; }

    public Trainer(WordGraph graph)
    {
        _graph = graph;
        _transitiveDecay = graph.TransitiveDecay;
        _semanticThreshold = graph.SemanticThreshold;
    }

    /// <summary>
    /// تدريب على جملة واحدة
    /// </summary>
    public void TrainSentence(string sentence)
    {
        var words = Tokenize(sentence);
        if (words.Length < 2) return;

        // === المرحلة 1: تسجيل الأوزان السياقية ===
        for (int i = 0; i < words.Length; i++)
        {
            var node = _graph.GetOrCreateNode(words[i]);
            node.Frequency++;

            if (i < words.Length - 1)
                _graph.AddContextualEdge(words[i], words[i + 1]);
        }

        // === المرحلة 2: اكتشاف العلاقات المعنوية بعمق ±2 ===
        // لكل كلمة: ننظر لأجدادها (i-2, i-1) وأحفادها (i+1, i+2)
        // الكلمات التي تتشارك أحفاداً/أجداداً → علاقة معنوية
        for (int i = 0; i < words.Length; i++)
        {
            var current = words[i];

            // جمع نافذة العمق ±2
            var window = new HashSet<string>();
            for (int d = -2; d <= 2; d++)
            {
                if (d == 0) continue;
                int idx = i + d;
                if (idx >= 0 && idx < words.Length)
                    window.Add(words[idx]);
            }

            // كل كلمة في النافذة تكتسب وزناً معنوياً مع الكلمة الحالية
            foreach (var neighbor in window)
            {
                // القوة تعتمد على المسافة: العمق 1 = 1.0, العمق 2 = transitiveDecay
                var distance = Math.Abs(Array.IndexOf(words, neighbor) - i);
                var weight = distance <= 1 ? 1.0 : _transitiveDecay;
                _graph.StrengthSemanticEdge(current, neighbor, weight * 0.1);
            }
        }

        // === المرحلة 3: إعادة التقييم العابر (Transitive Re-evaluation) ===
        // إذا A↔B معنوياً و B↔C معنوياً → A↔C تكتسب وزناً (أضعف)
        // نفعل هذا كل 10 جمل لتوفير الموارد
        SentencesTrained++;
        if (SentencesTrained % 10 == 0)
            PropagateTransitiveRelations(words);
    }

    /// <summary>
    /// نشر العلاقات العابرة:
    /// لكل كلمة في الجملة، ننظر لجيرانها المعنويين.
    /// إذا جار A مرتبط معنوياً بكلمة C، وC أيضاً جارة لكلمة أخرى B في الجملة
    /// → A و B يكتسبان وزناً معنوياً عابراً
    /// </summary>
    private void PropagateTransitiveRelations(string[] words)
    {
        var wordSet = new HashSet<string>(words);

        foreach (var word in words)
        {
            var semanticNeighbors = _graph.GetSemanticNeighbors(word).ToList();

            foreach (var (neighbor, weight) in semanticNeighbors)
            {
                if (weight < _semanticThreshold) continue;

                // جيران الجار المعنويين
                var neighborsOfNeighbor = _graph.GetSemanticNeighbors(neighbor);
                foreach (var (transitive, tWeight) in neighborsOfNeighbor)
                {
                    if (transitive == word) continue;
                    if (tWeight < _semanticThreshold) continue;

                    // العلاقة العابرة: word ↔ transitive عبر neighbor
                    // بقوة مُضمحلة
                    var transitiveWeight = weight * tWeight * _transitiveDecay * 0.01;
                    if (transitiveWeight > 0.001)
                        _graph.StrengthSemanticEdge(word, transitive, transitiveWeight);
                }
            }
        }
    }

    /// <summary>
    /// تدريب على مجموعة جمل
    /// </summary>
    public void TrainBatch(IEnumerable<string> sentences)
    {
        foreach (var sentence in sentences)
            TrainSentence(sentence);
    }

    /// <summary>
    /// تحويل جملة إلى كلمات (tokenization بسيط)
    /// </summary>
    public static string[] Tokenize(string sentence)
    {
        return sentence
            .Replace(".", " ").Replace("،", " ").Replace(",", " ")
            .Replace("!", " ").Replace("؟", " ").Replace("?", " ")
            .Replace("\"", " ").Replace("(", " ").Replace(")", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(w => w.Length > 0)
            .ToArray();
    }
}
