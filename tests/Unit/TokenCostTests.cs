using AskBilal.Core;

namespace Unit;

public class TokenCostTests
{
    private static readonly TokenPrices Prices = new(0.5m, 4m);

    [Fact]
    public void One_million_input_tokens_cost_the_input_price()
    {
        Assert.Equal(0.5m, TokenCost.Compute(1_000_000, 0, Prices));
    }

    [Fact]
    public void Input_and_output_tokens_are_priced_separately()
    {
        // 2 000 * 0.5 + 500 * 4 = 3 000 per million
        Assert.Equal(0.003m, TokenCost.Compute(2_000, 500, Prices));
    }

    [Fact]
    public void No_tokens_cost_nothing()
    {
        Assert.Equal(0m, TokenCost.Compute(0, 0, Prices));
    }
}
