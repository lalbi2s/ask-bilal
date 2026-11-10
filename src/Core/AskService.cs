namespace AskBilal.Core;

public class AskService(IEmbedder embedder, IChunkSearch search, IChat chat, string systemPrompt, double minimumScore)
{
    public const int SourceCount = 5;

    private const string ContactEmail = "contact@bilalsaidi.dev";

    /// <summary>Finds the best pieces of content, then asks the model to answer with them only.</summary>
    public async Task<AskResult> AskAsync(string question, string lang)
    {
        var vector = await embedder.EmbedAsync(question);
        var hits = await search.SearchAsync(question, vector, SourceCount);
        var bestScore = hits.Count == 0 ? 0 : hits.Max(hit => hit.Score);

        // Nothing relevant: refuse here, so the model is not called (no cost, no invented answer).
        if (hits.Count == 0 || bestScore < minimumScore)
        {
            return new AskResult(RefusalText(lang), [], Refused: true, bestScore, InputTokens: 0, OutputTokens: 0);
        }

        var answer = await chat.CompleteAsync(systemPrompt, BuildMessage(question, lang, hits));
        var sources = hits.Select(hit => new AskSource(hit.Chunk.Id, hit.Chunk.Type, hit.Chunk.Title, hit.Chunk.Url)).ToList();

        return new AskResult(answer.Text, sources, Refused: false, bestScore, answer.InputTokens, answer.OutputTokens);
    }

    private static string BuildMessage(string question, string lang, IReadOnlyList<SearchHit> hits)
    {
        var language = lang == "fr" ? "French" : "English";
        var sources = hits.Select((hit, i) => $"[{i + 1}] {hit.Chunk.Title}\n{hit.Chunk.Content}");

        // The question is data: it must not be able to close its own tag.
        var safeQuestion = question.Replace("</question>", "");

        return $"Answer in {language}.\n\nSources:\n{string.Join("\n\n", sources)}\n\n<question>\n{safeQuestion}\n</question>";
    }

    private static string RefusalText(string lang) => lang == "fr"
        ? $"Je n'ai pas cette information. Vous pouvez écrire à Bilal à {ContactEmail}."
        : $"I don't have that information. You can write to Bilal at {ContactEmail}.";
}
