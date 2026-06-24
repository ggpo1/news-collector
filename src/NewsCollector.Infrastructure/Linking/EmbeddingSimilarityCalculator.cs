namespace NewsCollector.Infrastructure.Linking;

public static class EmbeddingSimilarityCalculator
{
    public static decimal CosineSimilarity(float[] left, float[] right)
    {
        if (left.Length == 0 || right.Length == 0 || left.Length != right.Length)
        {
            return 0m;
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;

        for (var i = 0; i < left.Length; i++)
        {
            dot += left[i] * right[i];
            leftNorm += left[i] * left[i];
            rightNorm += right[i] * right[i];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0m;
        }

        var similarity = dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
        return Math.Round((decimal)Math.Clamp(similarity, 0, 1), 4);
    }
}
