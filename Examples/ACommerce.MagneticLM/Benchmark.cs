using ACommerce.MagneticLM.Graph;
using ACommerce.MagneticLM.Training;
using System.Diagnostics;

namespace ACommerce.MagneticLM;

/// <summary>
/// اختبار معياري على Penn Treebank (PTB) - أصغر وأشهر بيانات معيارية لنماذج اللغة.
/// 42,068 جملة تدريب، 3,761 جملة اختبار، 10,000 كلمة في القاموس.
///
/// المقياس: Perplexity (كلما انخفض كان أفضل)
/// القيم المرجعية:
///   5-gram KN:        ~141
///   LSTM (Zaremba):   ~78
///   AWD-LSTM:         ~57
///   Transformer-XL:   ~54
/// </summary>
public static class Benchmark
{
    public static void Run(string trainPath, string testPath)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("╔═══════════════════════════════════════════════════╗");
        Console.WriteLine("║  PTB Benchmark: MagneticLM vs Known Models       ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════╝\n");

        // === 1. تحميل البيانات ===
        var trainLines = File.ReadAllLines(trainPath).Where(l => l.Trim().Length > 0).ToArray();
        var testLines = File.ReadAllLines(testPath).Where(l => l.Trim().Length > 0).ToArray();
        Console.WriteLine($"Train: {trainLines.Length} sentences");
        Console.WriteLine($"Test:  {testLines.Length} sentences");

        // === 2. التدريب ===
        var graph = new WordGraph { SemanticThreshold = 0.05, TransitiveDecay = 0.5 };
        var trainer = new Trainer(graph);

        var sw = Stopwatch.StartNew();
        trainer.TrainBatch(trainLines);
        sw.Stop();

        var (nodes, cEdges, sEdges) = graph.GetStats();
        Console.WriteLine($"\nTraining: {sw.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine($"Graph: {nodes} nodes, {cEdges} contextual edges, {sEdges} semantic edges");
        Console.WriteLine($"Memory: ~{(nodes * 50 + cEdges * 16 + sEdges * 16) / 1024}KB estimated\n");

        // === 3. حساب Perplexity ===
        Console.WriteLine("Computing perplexity on test set...");

        // 3a. Bigram baseline (سياقي فقط، بدون semantic)
        var bigramPPL = ComputePerplexity(graph, testLines, useSemantic: false);
        Console.WriteLine($"  Bigram (contextual only):     PPL = {bigramPPL:F1}");

        // 3b. نموذجنا (سياقي + معنوي)
        var magneticPPL = ComputePerplexity(graph, testLines, useSemantic: true);
        Console.WriteLine($"  MagneticLM (context+semantic): PPL = {magneticPPL:F1}");

        // === 4. مقارنة ===
        Console.WriteLine("\n════════════════════════════════════════════");
        Console.WriteLine("  Comparison on Penn Treebank (test set)");
        Console.WriteLine("════════════════════════════════════════════");
        Console.WriteLine($"  5-gram KN (baseline):          ~141");
        Console.WriteLine($"  MagneticLM (ours, bigram):     {bigramPPL:F1}");
        Console.WriteLine($"  MagneticLM (ours, +semantic):  {magneticPPL:F1}");
        Console.WriteLine($"  LSTM (Zaremba 2014):           ~78");
        Console.WriteLine($"  AWD-LSTM (Merity 2018):        ~57");
        Console.WriteLine($"  Transformer-XL (Dai 2019):     ~54");
        Console.WriteLine("════════════════════════════════════════════");

        var improvement = (bigramPPL - magneticPPL) / bigramPPL * 100;
        Console.WriteLine($"\n  Semantic layer improvement: {improvement:F1}%");

        if (magneticPPL < 141)
            Console.WriteLine("  ✓ Better than 5-gram KN!");
        if (magneticPPL < 200)
            Console.WriteLine("  → Worth continuing development");
        if (magneticPPL > 500)
            Console.WriteLine("  ✗ Needs significant improvement");
    }

    /// <summary>
    /// حساب Perplexity: PPL = exp(-1/N * Σ log P(w_i | w_{i-1}))
    /// N = عدد الكلمات الكلي في مجموعة الاختبار
    /// </summary>
    private static double ComputePerplexity(WordGraph graph, string[] testLines, bool useSemantic)
    {
        double totalLogProb = 0;
        int totalTokens = 0;

        // Smoothing: لتجنب log(0) عند كلمات غير مرئية
        double smoothingAlpha = 1e-6;
        int vocabSize = graph.Nodes.Count;

        int linesDone = 0;
        foreach (var line in testLines)
        {
            linesDone++;
            if (linesDone % 500 == 0)
                Console.Write($"\r    Evaluating: {linesDone}/{testLines.Length}...");

            var words = Trainer.Tokenize(line);
            if (words.Length < 2) continue;

            for (int i = 1; i < words.Length; i++)
            {
                var prevWord = words[i - 1];
                var currentWord = words[i];

                double prob;

                // الاحتمال السياقي
                var contextProb = graph.GetContextualWeight(prevWord, currentWord);

                if (useSemantic)
                {
                    var semanticWeight = graph.GetSemanticWeight(prevWord, currentWord);
                    var totalSemantic = graph.GetSemanticNeighbors(prevWord).Sum(n => n.Weight);
                    var semanticProb = totalSemantic > 0 ? semanticWeight / totalSemantic : 0;

                    // مزج: السياقي أساسي، المعنوي يدعمه
                    prob = 0.7 * contextProb + 0.3 * Math.Max(semanticProb, 0);
                }
                else
                {
                    prob = contextProb;
                }

                // Add-alpha smoothing
                prob = (prob + smoothingAlpha) / (1.0 + smoothingAlpha * vocabSize);

                // حماية من القيم غير الصالحة
                if (double.IsNaN(prob) || double.IsInfinity(prob) || prob <= 0)
                    prob = smoothingAlpha;

                totalLogProb += Math.Log(prob);
                totalTokens++;
            }
        }

        if (totalTokens == 0) return double.MaxValue;

        // PPL = exp(-1/N * Σ log P)
        return Math.Exp(-totalLogProb / totalTokens);
    }
}
