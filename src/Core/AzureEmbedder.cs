// The constructor that takes an authentication policy is still marked as preview.
#pragma warning disable OPENAI001

using System.ClientModel.Primitives;
using Azure.Identity;
using OpenAI;
using OpenAI.Embeddings;

namespace AskBilal.Core;

public class AzureEmbedder : IEmbedder
{
    private readonly EmbeddingClient client;

    public AzureEmbedder(string openAiEndpoint)
    {
        // No API key: the token comes from "az login" locally, or from the managed identity in Azure.
        var tokenPolicy = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");
        var options = new OpenAIClientOptions { Endpoint = new Uri(openAiEndpoint) };
        client = new EmbeddingClient("embed", tokenPolicy, options); // name of the deployment
    }

    public async Task<float[]> EmbedAsync(string text)
    {
        var vectors = await EmbedManyAsync([text]);
        return vectors[0];
    }

    /// <summary>One call for several texts (the service accepts up to 2,048 texts per call).</summary>
    public async Task<List<float[]>> EmbedManyAsync(IReadOnlyList<string> texts)
    {
        var result = (await client.GenerateEmbeddingsAsync(texts)).Value;
        return result.Select(embedding => embedding.ToFloats().ToArray()).ToList();
    }
}
