using AskBilal.Core;

namespace Indexer;

public static class ContentLoader
{
    private const string GitHubUser = "lalbi2s";

    /// <summary>Reads content/knowledge.json (from the site) and content/readmes/*.md (from GitHub).</summary>
    public static List<SourceDocument> Load(string contentFolder)
    {
        var documents = new List<SourceDocument>();

        var knowledgePath = Path.Combine(contentFolder, "knowledge.json");
        if (File.Exists(knowledgePath))
        {
            documents.AddRange(KnowledgeFile.Parse(File.ReadAllText(knowledgePath)));
        }

        var readmeFolder = Path.Combine(contentFolder, "readmes");
        if (Directory.Exists(readmeFolder))
        {
            foreach (var path in Directory.GetFiles(readmeFolder, "*.md").Order())
            {
                var repo = Path.GetFileNameWithoutExtension(path);

                // The file is saved as <repo>.md, but it comes from <repo>/README.md.
                if (ContentFilter.IsExcludedPath($"{repo}/README.md"))
                {
                    continue;
                }

                documents.Add(new SourceDocument(
                    Id: $"readme-{repo}",
                    Title: $"{repo} README",
                    Project: repo,
                    Url: $"https://github.com/{GitHubUser}/{repo}",
                    Lang: "en",
                    Type: "project",
                    Text: File.ReadAllText(path)));
            }
        }

        return documents;
    }
}
