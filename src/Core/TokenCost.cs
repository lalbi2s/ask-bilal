namespace AskBilal.Core;

/// <summary>Price of a model, in euros per million tokens.</summary>
public record TokenPrices(decimal InputPerMillion, decimal OutputPerMillion);

public static class TokenCost
{
    public static decimal Compute(int inputTokens, int outputTokens, TokenPrices prices)
    {
        var input = inputTokens * prices.InputPerMillion;
        var output = outputTokens * prices.OutputPerMillion;
        return (input + output) / 1_000_000m;
    }
}
