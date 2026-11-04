using AskBilal.Core;

namespace Unit;

public class CosineSimilarityTests
{
    [Fact]
    public void Identical_vectors_give_1()
    {
        float[] vector = [1f, 2f, 3f];

        Assert.Equal(1.0, CosineSimilarity.Compute(vector, vector), precision: 6);
    }

    [Fact]
    public void Only_the_direction_counts_not_the_length()
    {
        float[] vector = [1f, 2f, 3f];
        float[] threeTimesLonger = [3f, 6f, 9f];

        Assert.Equal(1.0, CosineSimilarity.Compute(vector, threeTimesLonger), precision: 6);
    }

    [Fact]
    public void Perpendicular_vectors_give_0()
    {
        Assert.Equal(0.0, CosineSimilarity.Compute([1f, 0f], [0f, 1f]), precision: 6);
    }

    [Fact]
    public void Opposite_vectors_give_minus_1()
    {
        Assert.Equal(-1.0, CosineSimilarity.Compute([1f, 2f], [-1f, -2f]), precision: 6);
    }

    [Fact]
    public void Vectors_of_different_sizes_are_refused()
    {
        Assert.Throws<ArgumentException>(() => CosineSimilarity.Compute([1f, 2f], [1f, 2f, 3f]));
    }

    [Fact]
    public void A_vector_of_zeros_is_refused()
    {
        Assert.Throws<ArgumentException>(() => CosineSimilarity.Compute([0f, 0f], [1f, 2f]));
    }

    [Fact]
    public void Four_texts_give_six_pairs()
    {
        string[] texts = ["a", "b", "c", "d"];
        float[][] vectors = [[1f, 0f], [0f, 1f], [1f, 1f], [2f, 1f]];

        var pairs = CosineSimilarity.RankPairs(texts, vectors);

        Assert.Equal(6, pairs.Count);
    }

    [Fact]
    public void Pairs_are_sorted_from_closest_to_farthest()
    {
        string[] texts = ["right", "up", "almost right"];
        float[][] vectors = [[1f, 0f], [0f, 1f], [10f, 1f]];

        var pairs = CosineSimilarity.RankPairs(texts, vectors);

        Assert.Equal("right", pairs[0].First);
        Assert.Equal("almost right", pairs[0].Second);
        Assert.Equal("right", pairs[2].First);
        Assert.Equal("up", pairs[2].Second);
        Assert.True(pairs[0].Score > pairs[1].Score);
        Assert.True(pairs[1].Score > pairs[2].Score);
    }

    [Fact]
    public void Texts_and_vectors_must_match()
    {
        Assert.Throws<ArgumentException>(() => CosineSimilarity.RankPairs(["a", "b"], [[1f, 0f]]));
    }
}
