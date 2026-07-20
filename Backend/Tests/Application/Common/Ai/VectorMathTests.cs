using Application.Common.Ai;

namespace Tests.Application.Common.Ai;

/// <summary>
/// Checks the meaning-comparison math always behaves the way we expect (same result every time,
/// no internet or database involved). In plain terms it confirms: same meaning scores 1, unrelated
/// scores 0, opposite scores -1, text length doesn't skew the score, and odd inputs (all-zeros or
/// mismatched sizes) are handled safely instead of crashing or giving a nonsense answer.
/// </summary>
public class VectorMathTests
{
    private const float Tol = 1e-5f;

    [Fact]
    public void CosineSimilarity_identical_direction_is_one()
    {
        float[] a = { 1f, 2f, 3f };
        Assert.Equal(1f, VectorMath.CosineSimilarity(a, a), Tol);
    }

    [Fact]
    public void CosineSimilarity_is_length_invariant_only_direction_matters()
    {
        // Same direction, very different magnitudes — meaning, not length, is what we compare.
        float[] a = { 1f, 1f };
        float[] b = { 100f, 100f };
        Assert.Equal(1f, VectorMath.CosineSimilarity(a, b), Tol);
    }

    [Fact]
    public void CosineSimilarity_orthogonal_is_zero()
    {
        float[] a = { 1f, 0f };
        float[] b = { 0f, 1f };
        Assert.Equal(0f, VectorMath.CosineSimilarity(a, b), Tol);
    }

    [Fact]
    public void CosineSimilarity_opposite_direction_is_minus_one()
    {
        float[] a = { 1f, 2f };
        float[] b = { -1f, -2f };
        Assert.Equal(-1f, VectorMath.CosineSimilarity(a, b), Tol);
    }

    [Fact]
    public void CosineSimilarity_zero_vector_returns_zero_not_nan()
    {
        float[] a = { 0f, 0f, 0f };
        float[] b = { 1f, 2f, 3f };
        var score = VectorMath.CosineSimilarity(a, b);
        Assert.False(float.IsNaN(score));
        Assert.Equal(0f, score, Tol);
    }

    [Fact]
    public void CosineSimilarity_mismatched_lengths_throws()
    {
        float[] a = { 1f, 2f };
        float[] b = { 1f, 2f, 3f };
        Assert.Throws<ArgumentException>(() => VectorMath.CosineSimilarity(a, b));
    }

    [Fact]
    public void TopK_returns_most_similar_first_and_respects_k()
    {
        float[] query = { 1f, 0f };
        var corpus = new List<float[]>
        {
            new[] { -1f, 0f }, // index 0: opposite  (score -1)
            new[] { 0f, 1f },  // index 1: orthogonal (score 0)
            new[] { 1f, 0f },  // index 2: identical  (score 1)
        };

        var top = VectorMath.TopK(query, corpus, k: 2);

        Assert.Equal(2, top.Count);
        Assert.Equal(2, top[0].Index); // best match first
        Assert.Equal(1, top[1].Index); // then the orthogonal one, ahead of the opposite
    }

    [Fact]
    public void TopK_non_positive_k_returns_empty()
    {
        float[] query = { 1f, 0f };
        var corpus = new List<float[]> { new[] { 1f, 0f } };
        Assert.Empty(VectorMath.TopK(query, corpus, k: 0));
    }
}
