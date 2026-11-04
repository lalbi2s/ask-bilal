// The constructor that takes an authentication policy is still marked as preview.
#pragma warning disable OPENAI001

using System.ClientModel;
using System.ClientModel.Primitives;
using AskBilal.Core;
using OpenAI;
using OpenAI.Embeddings;

namespace Playground;

public static class EmbeddingDemo
{
    private static readonly string[] Sentences =
    [
        "A unit test checks that a small piece of C# code behaves as expected.",
        "The .NET SDK builds and runs the application from the command line.",
        "xUnit is a testing framework for .NET.",
        "Simmer the tomato sauce for twenty minutes before adding the basil.",
        "Whisk the eggs and sugar, then fold in the flour to make the batter.",
        "The striker scored twice in the second half of the football match.",
    ];

    // Same meaning in English and in French, to see if the language matters.
    private const string English = "The unit test failed because the date was wrong.";
    private const string French = "Le test unitaire a échoué parce que la date était fausse.";

    public static async Task Run(BearerTokenPolicy tokenPolicy, OpenAIClientOptions options)
    {
        var client = new EmbeddingClient(
            model: "embed", // name of the deployment, not of the model
            authenticationPolicy: tokenPolicy,
            options: options);

        // One call for all the texts is faster than one call per text.
        List<string> texts = [.. Sentences, English, French];
        OpenAIEmbeddingCollection result;
        try
        {
            result = (await client.GenerateEmbeddingsAsync(texts)).Value;
        }
        catch (ClientResultException error)
        {
            Console.WriteLine($"HTTP {error.Status}: {error.Message}");
            return;
        }

        var vectors = result.Select(embedding => embedding.ToFloats().ToArray()).ToList();

        Console.WriteLine($"Vector size: {vectors[0].Length} numbers");
        Console.WriteLine($"First 5 numbers of the first sentence: {string.Join(", ", vectors[0].Take(5))}");
        Console.WriteLine($"Tokens used: {result.Usage.InputTokenCount}");
        Console.WriteLine();

        Console.WriteLine("Pairs, from closest to farthest:");
        var pairs = CosineSimilarity.RankPairs(Sentences, vectors.Take(Sentences.Length).ToList());
        foreach (var pair in pairs)
        {
            Console.WriteLine($"{pair.Score:F3}  \"{pair.First}\"  <->  \"{pair.Second}\"");
        }
        Console.WriteLine();

        var englishVector = vectors[Sentences.Length];
        var frenchVector = vectors[Sentences.Length + 1];
        var unrelatedVector = vectors[3];
        Console.WriteLine("Same sentence in English and in French:");
        Console.WriteLine($"{CosineSimilarity.Compute(englishVector, frenchVector):F3}  English <-> French");
        Console.WriteLine($"{CosineSimilarity.Compute(englishVector, unrelatedVector):F3}  English <-> tomato sauce sentence");
    }
}
