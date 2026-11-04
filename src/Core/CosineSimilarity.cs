namespace AskBilal.Core;

public static class CosineSimilarity
{
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
