using AskBilal.Core;

namespace Unit;

public class KnowledgeFileTests
{
    [Fact]
    public void Parse_ReadsEveryItemAsADocument()
    {
        var json = """
            [
              { "id": "experience-evs-en", "type": "experience", "title": "EVS", "url": "https://bilalsaidi.dev/experience/",
                "lang": "en", "project": "", "text": "R&D intern." },
              { "id": "project-x-fr", "type": "project", "title": "x", "url": "https://bilalsaidi.dev/fr/work/x/",
                "lang": "fr", "project": "x", "text": "Un projet." }
            ]
            """;

        var documents = KnowledgeFile.Parse(json);

        Assert.Equal(2, documents.Count);
        Assert.Equal(new SourceDocument("experience-evs-en", "EVS", "", "https://bilalsaidi.dev/experience/", "en", "experience", "R&D intern."), documents[0]);
        Assert.Equal("x", documents[1].Project);
        Assert.Equal("fr", documents[1].Lang);
    }

    [Fact]
    public void Parse_NotAnArray_Fails()
    {
        Assert.Throws<InvalidOperationException>(() => KnowledgeFile.Parse("{}"));
    }
}
