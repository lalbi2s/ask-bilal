namespace AskBilal.Core;

/// <summary>A piece of content found by the search, with its relevance score.</summary>
public record SearchHit(Chunk Chunk, double Score);

public record ChatAnswer(string Text, int InputTokens, int OutputTokens);

/// <summary>What the site needs to show a source: [1] in the answer is Sources[0].</summary>
public record AskSource(string Id, string Type, string Title, string Url);

public record AskResult(
    string Answer,
    IReadOnlyList<AskSource> Sources,
    bool Refused,
    double BestScore,
    int InputTokens,
    int OutputTokens);

public interface IEmbedder
{
    Task<float[]> EmbedAsync(string text);
}

public interface IChunkSearch
{
    Task<IReadOnlyList<SearchHit>> SearchAsync(string question, float[] vector, int top);
}

public interface IChat
{
    Task<ChatAnswer> CompleteAsync(string systemPrompt, string userMessage);
}
