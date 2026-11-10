using System.Text.Json.Serialization;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;

namespace AskBilal.Core;

/// <summary>The index schema lives in code, so the index can be rebuilt with one command.</summary>
public static class ChunkIndex
{
    public const string Name = "ask-bilal";
    public const int VectorDimensions = 1536;
    public const string SemanticConfigurationName = "default";

    private const string VectorProfileName = "vector-profile";
    private const string VectorAlgorithmName = "hnsw";

    public static SearchIndex Create()
    {
        return new SearchIndex(Name)
        {
            Fields =
            {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
                new SearchableField("content"),
                new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = VectorDimensions,
                    VectorSearchProfileName = VectorProfileName,
                },
                new SearchableField("title"),
                // Filterable now: this cannot be changed later without rebuilding the index.
                new SimpleField("project", SearchFieldDataType.String) { IsFilterable = true },
                new SimpleField("url", SearchFieldDataType.String),
                new SimpleField("lang", SearchFieldDataType.String) { IsFilterable = true },
                new SimpleField("type", SearchFieldDataType.String) { IsFilterable = true },
            },
            VectorSearch = new VectorSearch
            {
                Algorithms = { new HnswAlgorithmConfiguration(VectorAlgorithmName) },
                Profiles = { new VectorSearchProfile(VectorProfileName, VectorAlgorithmName) },
            },
            SemanticSearch = new SemanticSearch
            {
                Configurations =
                {
                    new SemanticConfiguration(SemanticConfigurationName, new SemanticPrioritizedFields
                    {
                        TitleField = new SemanticField("title"),
                        ContentFields = { new SemanticField("content") },
                    }),
                },
            },
        };
    }
}

/// <summary>One document of the index: a chunk and its vector.</summary>
public class IndexedChunk
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("content")] public string Content { get; set; } = "";
    [JsonPropertyName("contentVector")] public float[] ContentVector { get; set; } = [];
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("project")] public string Project { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("lang")] public string Lang { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";

    public static IndexedChunk From(Chunk chunk, float[] vector) => new()
    {
        Id = chunk.Id,
        Content = chunk.Content,
        ContentVector = vector,
        Title = chunk.Title,
        Project = chunk.Project,
        Url = chunk.Url,
        Lang = chunk.Lang,
        Type = chunk.Type,
    };

    public Chunk ToChunk() => new(Id, Content, Title, Project, Url, Lang, Type);
}
