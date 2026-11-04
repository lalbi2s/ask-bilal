namespace AskBilal.Core;

public record PairScore(string First, string Second, double Score);

public static class CosineSimilarity
{
    /// <summary>Compares every pair of texts once and sorts them from closest to farthest.</summary>
    public static List<PairScore> RankPairs(IReadOnlyList<string> texts, IReadOnlyList<float[]> vectors)
    {
        if (texts.Count != vectors.Count)
        {
            throw new ArgumentException("There must be one vector per text.");
        }

        var pairs = new List<PairScore>();
        for (var i = 0; i < texts.Count; i++)
        {
            for (var j = i + 1; j < texts.Count; j++)
            {
                var score = Compute(vectors[i], vectors[j]);
                pairs.Add(new PairScore(texts[i], texts[j], score));
            }
        }

        return pairs.OrderByDescending(pair => pair.Score).ToList();
    }

    /// <summary>Gives 1 for the same direction, 0 for unrelated vectors, -1 for opposite ones.</summary>
    public static double Compute(float[] a, float[] b)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException("Vectors must have the same size (same model?).");
        }

        double dot = 0;
        double lengthA = 0;
        double lengthB = 0;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            lengthA += a[i] * a[i];
            lengthB += b[i] * b[i];
        }

        if (lengthA == 0 || lengthB == 0)
        {
            throw new ArgumentException("A vector of zeros has no direction.");
        }

        return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));
    }
}
