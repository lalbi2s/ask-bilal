using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace AskBilal.Core;

/// <summary>Hybrid search: the words of the question and its vector, merged by Azure AI Search.</summary>
public class AzureChunkSearch : IChunkSearch
{
    private readonly SearchClient client;
    private readonly bool useSemanticRanker;

    public AzureChunkSearch(string searchEndpoint, bool useSemanticRanker)
    {
        client = new SearchClient(new Uri(searchEndpoint), ChunkIndex.Name, new DefaultAzureCredential());
        this.useSemanticRanker = useSemanticRanker;
    }

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(string question, float[] vector, int top)
    {
        var options = new SearchOptions
        {
            Size = top,
            VectorSearch = new VectorSearchOptions
            {
                Queries = { new VectorizedQuery(vector) { KNearestNeighborsCount = 50, Fields = { "contentVector" } } },
            },
        };

        if (useSemanticRanker)
        {
            options.QueryType = SearchQueryType.Semantic;
            options.SemanticSearch = new SemanticSearchOptions { SemanticConfigurationName = ChunkIndex.SemanticConfigurationName };
        }

        var response = await client.SearchAsync<IndexedChunk>(question, options);

        var hits = new List<SearchHit>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            // The semantic score goes from 0 to 4. The hybrid score is a small number that is hard to read.
            var score = useSemanticRanker ? result.SemanticSearch?.RerankerScore ?? 0 : result.Score ?? 0;
            hits.Add(new SearchHit(result.Document.ToChunk(), score));
        }

        return hits;
    }
}
