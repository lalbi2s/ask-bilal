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
}
