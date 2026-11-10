using AskBilal.Core;

namespace Unit;

public class ContentFilterTests
{
    [Theory]
    [InlineData(".env")]
    [InlineData("repo/.env.local")]
    [InlineData("docs/design.md")]
    [InlineData("repo/docs/notes.md")]
    [InlineData("CLAUDE.md")]
    [InlineData("repo/CLAUDE.md")]
    [InlineData("repo/.claude/settings.json")]
    [InlineData("repo/local.settings.json")]
    public void IsExcludedPath_RefusesPrivateFiles(string path)
    {
        Assert.True(ContentFilter.IsExcludedPath(path));
    }

    [Theory]
    [InlineData("README.md")]
    [InlineData("repo/README.md")]
    [InlineData("knowledge.json")]
    public void IsExcludedPath_AllowsPublicFiles(string path)
    {
        Assert.False(ContentFilter.IsExcludedPath(path));
    }

    [Theory]
    [InlineData("Call me on +32 470 12 34 56")]
    [InlineData("Phone: 0470/12.34.56")]
    [InlineData("AccountKey=abcdef")]
    [InlineData("api_key: 123")]
    public void LooksSensitive_FindsPhoneNumbersAndKeys(string text)
    {
        Assert.True(ContentFilter.LooksSensitive(text));
    }

    [Theory]
    [InlineData("Built with .NET 10 in 2026, 120 tests, 1 536 dimensions.")]
    [InlineData("Write to contact@bilalsaidi.dev for more.")]
    public void LooksSensitive_AllowsNormalText(string text)
    {
        Assert.False(ContentFilter.LooksSensitive(text));
    }
}
