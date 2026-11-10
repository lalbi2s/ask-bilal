using AskBilal.Core;
using Azure;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;

namespace Indexer;

public static class Rebuild
{
    private const int BatchSize = 50;

    /// <summary>Deletes the index, creates it again and loads all the content. Safe to run many times.</summary>
    public static async Task<int> Run(string contentFolder, string openAiEndpoint, string searchEndpoint)
    {
        var documents = ContentLoader.Load(contentFolder);
        if (documents.Count == 0)
        {
            Console.WriteLine($"No content found in '{contentFolder}'. See the README.");
            return 1;
        }

        var chunks = documents.SelectMany(Chunker.Split).ToList();
        Console.WriteLine($"{documents.Count} documents cut into {chunks.Count} chunks.");

        // Last check before anything leaves this computer.
        var suspicious = chunks.Where(chunk => ContentFilter.LooksSensitive(chunk.Content)).ToList();
        if (suspicious.Count > 0)
        {
            Console.WriteLine("Stopped, nothing was sent. These chunks look like a phone number or a secret:");
            suspicious.ForEach(chunk => Console.WriteLine($"  {chunk.Id}"));
            return 1;
        }

        var embedder = new AzureEmbedder(openAiEndpoint);
        var indexedChunks = new List<IndexedChunk>();
        foreach (var batch in chunks.Chunk(BatchSize))
        {
            var vectors = await embedder.EmbedManyAsync(batch.Select(chunk => chunk.Content).ToList());
            indexedChunks.AddRange(batch.Select((chunk, i) => IndexedChunk.From(chunk, vectors[i])));
        }

        var indexClient = new SearchIndexClient(new Uri(searchEndpoint), new DefaultAzureCredential());
        await DeleteIndexIfItExists(indexClient);
        await indexClient.CreateIndexAsync(ChunkIndex.Create());

        var searchClient = indexClient.GetSearchClient(ChunkIndex.Name);
        var options = new IndexDocumentsOptions { ThrowOnAnyError = true };
        foreach (var batch in indexedChunks.Chunk(BatchSize))
        {
            await searchClient.MergeOrUploadDocumentsAsync(batch, options);
        }

        Console.WriteLine($"Index '{ChunkIndex.Name}' rebuilt with {await WaitForCount(searchClient, chunks.Count)} documents.");
        return 0;
    }

    private static async Task DeleteIndexIfItExists(SearchIndexClient indexClient)
    {
        try
        {
            await indexClient.DeleteIndexAsync(ChunkIndex.Name, CancellationToken.None);
        }
        catch (RequestFailedException error) when (error.Status == 404)
        {
            // First run: nothing to delete.
        }
    }

    // The document count can take a few seconds to catch up after an upload.
    private static async Task<long> WaitForCount(SearchClient searchClient, int expected)
    {
        long count = 0;
        for (var attempt = 0; attempt < 10 && count < expected; attempt++)
        {
            await Task.Delay(1000);
            count = (await searchClient.GetDocumentCountAsync()).Value;
        }

        return count;
    }
}
