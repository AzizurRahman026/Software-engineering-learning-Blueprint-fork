namespace Application.Common.Ai;

/// <summary>
/// Math for comparing "embeddings" — the lists of numbers an AI model uses to represent
/// the MEANING of a piece of text, so that texts about the same thing end up with similar numbers.
///
/// In plain terms: this is how we ask "which of these texts means roughly the same as that one?"
/// Turning text into those numbers needs the AI model (done elsewhere); this file just compares
/// numbers we already have — pure calculation, no internet, no database.
/// </summary>
public static class VectorMath
{
    /// <summary>
    /// Scores how similar two texts are in MEANING, from -1 to 1:
    /// 1 = same meaning, 0 = unrelated, -1 = opposite.
    ///
    /// It compares the DIRECTION of the two number-lists, not their size — so a one-line note and
    /// a long essay about the same topic still score near 1. (A short note about "cancelling my
    /// plan" and a long help article on "stopping your subscription" would land close together.)
    /// A list of all-zeros has no direction to compare, so we return 0 rather than an error.
    /// </summary>
    /// <exception cref="ArgumentException">The two lists are different lengths (they must come from the same AI model).</exception>
    public static float CosineSimilarity(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        if (a.Length != b.Length)
        {
            throw new ArgumentException(
                $"Vectors must be the same length to compare (got {a.Length} and {b.Length}).");
        }

        // dot = how much the two point the same way; magA/magB = each list's size (used to cancel size out).
        double dot = 0, magA = 0, magB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        // An all-zeros list has no direction — call it "unrelated" (0) instead of dividing by zero.
        return magA == 0 || magB == 0 ? 0f : (float)(dot / (Math.Sqrt(magA) * Math.Sqrt(magB)));
    }

    /// <summary>
    /// Finds the <c>k</c> closest matches in a set of texts, best first — like sorting a list by
    /// "how well does this answer my question?" and taking the top few.
    ///
    /// It simply checks every item, which is perfectly fine for a small set kept in memory. Only
    /// when the set grows large (so checking every item on every search gets slow) is it worth
    /// moving to a dedicated search index like MongoDB Atlas Vector Search.
    /// </summary>
    public static IReadOnlyList<(int Index, float Score)> TopK(
        ReadOnlySpan<float> query,
        IReadOnlyList<float[]> corpus,
        int k)
    {
        ArgumentNullException.ThrowIfNull(corpus);
        if (k <= 0)
        {
            return Array.Empty<(int, float)>();
        }

        var scored = new List<(int Index, float Score)>(corpus.Count);
        for (var i = 0; i < corpus.Count; i++)
        {
            scored.Add((i, CosineSimilarity(query, corpus[i])));
        }

        scored.Sort((x, y) => y.Score.CompareTo(x.Score)); // best match first
        return scored.Count <= k ? scored : scored.GetRange(0, k);
    }
}
