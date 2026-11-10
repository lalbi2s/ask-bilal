using System.Text.RegularExpressions;

namespace AskBilal.Core;

public static partial class ContentFilter
{
    /// <summary>
    /// True for files that must never be indexed: secrets, internal notes, hidden folders.
    /// Among the Markdown files, only README.md is public content.
    /// </summary>
    public static bool IsExcludedPath(string path)
    {
        var parts = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var fileName = parts[^1];

        var isOtherMarkdown = fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            && !fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase);

        return fileName.StartsWith(".env", StringComparison.OrdinalIgnoreCase)
            || fileName.Equals("local.settings.json", StringComparison.OrdinalIgnoreCase)
            || isOtherMarkdown
            || parts[..^1].Any(folder => folder.StartsWith('.') || folder.Equals("docs", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Safety net before sending text to the index: a phone number or a secret by mistake.</summary>
    public static bool LooksSensitive(string text)
    {
        return InternationalPhone().IsMatch(text)
            || BelgianPhone().IsMatch(text)
            || SecretAssignment().IsMatch(text);
    }

    [GeneratedRegex(@"\+\d{1,3}[\s.-]?\d(?:[\s./-]?\d){7,}")]
    private static partial Regex InternationalPhone();

    [GeneratedRegex(@"\b0\d{2,3}[\s./-]?\d{2}[\s./-]?\d{2}[\s./-]?\d{2}\b")]
    private static partial Regex BelgianPhone();

    [GeneratedRegex(@"(account ?key|api[-_ ]?key|secret|password|sharedaccesskey)\s*[=:]", RegexOptions.IgnoreCase)]
    private static partial Regex SecretAssignment();
}
