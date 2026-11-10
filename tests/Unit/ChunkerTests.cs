using AskBilal.Core;

namespace Unit;

public class ChunkerTests
{
    private static SourceDocument Document(string text) =>
        new("readme-agent-controller", "agent-controller", "agent-controller",
            "https://github.com/lalbi2s/agent-controller", "en", "project", text);

    [Fact]
    public void Split_ShortText_GivesOneChunkWithTheMetadata()
    {
        var chunks = Chunker.Split(Document("A short text."));

        var chunk = Assert.Single(chunks);
        Assert.Equal("A short text.", chunk.Content);
        Assert.Equal("agent-controller", chunk.Project);
        Assert.Equal("https://github.com/lalbi2s/agent-controller", chunk.Url);
        Assert.Equal("en", chunk.Lang);
        Assert.Equal("project", chunk.Type);
    }

    [Fact]
    public void Split_CutsMarkdownAtLevelTwoHeadings()
    {
        var text = "Intro line.\n\n## Why\nBecause.\n\n### Detail\nStays with Why.\n\n## How\nLike this.";

        var chunks = Chunker.Split(Document(text));

        Assert.Equal(3, chunks.Count);
        Assert.StartsWith("Intro line.", chunks[0].Content);
        Assert.StartsWith("## Why", chunks[1].Content);
        Assert.Contains("Stays with Why.", chunks[1].Content);
        Assert.StartsWith("## How", chunks[2].Content);
    }

    [Fact]
    public void Split_PutsTheHeadingInTheTitle()
    {
        var chunks = Chunker.Split(Document("## How it works\nSome text."));

        Assert.Equal("agent-controller: How it works", chunks[0].Title);
    }

    [Fact]
    public void Split_IgnoresHeadingsInsideCodeBlocks()
    {
        var text = "## Usage\n```bash\n## not a heading\nrun it\n```\nDone.";

        var chunks = Chunker.Split(Document(text));

        Assert.Single(chunks);
    }

    [Fact]
    public void Split_SkipsEmptySections()
    {
        var chunks = Chunker.Split(Document("## Empty\n\n## Full\nSome text."));

        var chunk = Assert.Single(chunks);
        Assert.StartsWith("## Full", chunk.Content);
    }

    [Fact]
    public void Split_LongText_StaysUnderTheLimitAndOverlaps()
    {
        var text = string.Join(' ', Enumerable.Range(1, 1000).Select(n => $"word{n:0000}"));

        var chunks = Chunker.Split(Document(text));

        Assert.True(chunks.Count > 3);
        Assert.All(chunks, chunk => Assert.True(chunk.Content.Length <= Chunker.MaxLength));
        for (var i = 1; i < chunks.Count; i++)
        {
            var start = chunks[i].Content[..50];
            Assert.Contains(start, chunks[i - 1].Content);
        }
        Assert.Contains("word0001", chunks[0].Content);
        Assert.Contains("word1000", chunks[^1].Content);
    }

    [Fact]
    public void Split_GivesStableUniqueIdsWithAllowedCharacters()
    {
        var text = "## A\nOne.\n\n## B\n" + string.Join(' ', Enumerable.Repeat("filler", 600));
        var document = new SourceDocument("repo/readme.md", "t", "p", "u", "en", "project", text);

        var first = Chunker.Split(document).Select(chunk => chunk.Id).ToList();
        var second = Chunker.Split(document).Select(chunk => chunk.Id).ToList();

        Assert.Equal(first, second);
        Assert.Equal(first.Count, first.Distinct().Count());
        Assert.All(first, id => Assert.Matches("^[A-Za-z0-9_=-]+$", id));
    }
}
