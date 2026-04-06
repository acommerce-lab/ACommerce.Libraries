using ACommerce.MagneticLM.Graph;
using ACommerce.MagneticLM.Training;
using System.Diagnostics;

namespace ACommerce.MagneticLM;

public static class Benchmark
{
    public static void Run(string trainPath, string testPath)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("╔═══════════════════════════════════════════════════╗");
        Console.WriteLine("║  PTB Benchmark: MagneticLM v2 (n-gram + semantic)║");
        Console.WriteLine("╚═══════════════════════════════════════════════════╝\n");

        var trainLines = File.ReadAllLines(trainPath).Where(l => l.Trim().Length > 0).ToArray();
        var testLines = File.ReadAllLines(testPath).Where(l => l.Trim().Length > 0).ToArray();
        Console.WriteLine($"Train: {trainLines.Length:N0} sentences");
        Console.WriteLine($"Test:  {testLines.Length:N0} sentences");

        // === Training ===
        var graph = new WordGraph { MaxNgramOrder = 4, Discount = 0.75, SemanticThreshold = 0.05, TransitiveDecay = 0.5 };
        var trainer = new Trainer(graph);

        var sw = Stopwatch.StartNew();
        trainer.TrainBatch(trainLines);
        sw.Stop();

        var (nodes, ngramEntries, sEdges) = graph.GetStats();
        Console.WriteLine($"\nTraining: {sw.Elapsed.TotalSeconds:F1}s");
        Console.WriteLine($"Graph: {nodes:N0} nodes, {ngramEntries:N0} n-gram entries, {sEdges:N0} semantic edges");
        Console.WriteLine($"Total tokens: {graph.TotalTokens:N0}\n");

        // === Perplexity computation ===
        Console.Write("Computing perplexity...");

        var pplBigram = ComputePerplexity(graph, testLines, mode: "bigram");
        Console.Write($"\r  Bigram only:                   PPL = {pplBigram:F1}\n");

        var pplNgram = ComputePerplexity(graph, testLines, mode: "ngram");
        Console.Write($"  N-gram interpolated (n={graph.MaxNgramOrder}):     PPL = {pplNgram:F1}\n");

        var pplMagnetic = ComputePerplexity(graph, testLines, mode: "magnetic");
        Console.Write($"  MagneticLM (n-gram + semantic): PPL = {pplMagnetic:F1}\n");

        // === Comparison ===
        Console.WriteLine("\n════════════════════════════════════════════════════");
        Console.WriteLine("  Model                          | Perplexity");
        Console.WriteLine("  ───────────────────────────────┼───────────");
        Console.WriteLine($"  Our Bigram                     | {pplBigram:F1}");
        Console.WriteLine($"  Our N-gram (interpolated, n=4) | {pplNgram:F1}");
        Console.WriteLine($"  Our MagneticLM (n-gram+sem)    | {pplMagnetic:F1}");
        Console.WriteLine("  ───────────────────────────────┼───────────");
        Console.WriteLine("  5-gram KN (reference)          | ~141");
        Console.WriteLine("  LSTM (Zaremba 2014)            | ~78");
        Console.WriteLine("  AWD-LSTM (Merity 2018)         | ~57");
        Console.WriteLine("  Transformer-XL (Dai 2019)      | ~54");
        Console.WriteLine("════════════════════════════════════════════════════");

        var ngramImprove = (pplBigram - pplNgram) / pplBigram * 100;
        var semImprove = (pplNgram - pplMagnetic) / pplNgram * 100;
        Console.WriteLine($"\n  N-gram vs bigram improvement: {ngramImprove:F1}%");
        Console.WriteLine($"  Semantic layer improvement:    {semImprove:F1}%");

        if (pplMagnetic < 141) Console.WriteLine("\n  >>> BETTER than 5-gram KN! <<<");
        else if (pplMagnetic < 200) Console.WriteLine("\n  Close to 5-gram KN - worth continuing");
        else if (pplMagnetic < 300) Console.WriteLine("\n  Reasonable - needs more data or deeper n-grams");
        else Console.WriteLine("\n  Needs improvement - consider n=5 or better smoothing");
    }

    private static double ComputePerplexity(WordGraph graph, string[] testLines, string mode)
    {
        double totalLogProb = 0;
        int totalTokens = 0;
        double smoothing = 1.0 / (graph.Nodes.Count * 10); // uniform backoff

        int done = 0;
        foreach (var line in testLines)
        {
            done++;
            if (done % 1000 == 0) Console.Write($"\r  [{mode}] {done}/{testLines.Length}...          ");

            var words = Trainer.Tokenize(line);
            if (words.Length < 2) continue;

            for (int i = 1; i < words.Length; i++)
            {
                var currentWord = words[i];
                double prob;

                if (mode == "bigram")
                {
                    // فقط P(w|w-1) bigram
                    var ctx = new[] { words[i - 1] };
                    var key = words[i - 1];
                    if (graph.NgramTotals.TryGetValue(key, out var total) && total > 0)
                    {
                        graph.NgramCounts[key].TryGetValue(currentWord, out var count);
                        prob = count / total;
                    }
                    else prob = 0;
                }
                else
                {
                    // سياق كامل حتى MaxNgramOrder
                    var contextStart = Math.Max(0, i - graph.MaxNgramOrder);
                    var fullContext = words[contextStart..i];

                    if (mode == "magnetic")
                        prob = graph.GetMagneticProbability(fullContext, currentWord);
                    else
                        prob = graph.GetInterpolatedProbability(fullContext, currentWord);
                }

                // Smoothing: ضمان لا صفر
                prob = Math.Max(prob, smoothing);

                totalLogProb += Math.Log(prob);
                totalTokens++;
            }
        }

        return totalTokens > 0 ? Math.Exp(-totalLogProb / totalTokens) : double.MaxValue;
    }
}
