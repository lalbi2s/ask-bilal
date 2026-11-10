using System.Text;
using System.Text.RegularExpressions;

namespace AskBilal.Core;

public static partial class Chunker
{
    public const int MaxLength = 1500;
    public const int Overlap = 200;

    /// <summary>Cuts a document at its "##" headings, then cuts long sections into overlapping pieces.</summary>
    public static List<Chunk> Split(SourceDocument document)
    {
        var chunks = new List<Chunk>();
        var documentId = InvalidIdCharacters().Replace(document.Id, "_");

        foreach (var section in SplitSections(document.Text))
        {
            var title = section.Heading is null ? document.Title : $"{document.Title}: {section.Heading}";

            foreach (var piece in SplitLongText(section.Text))
            {
                var id = $"{documentId}-{chunks.Count}";
                chunks.Add(new Chunk(id, piece, title, document.Project, document.Url, document.Lang, document.Type));
            }
        }

        return chunks;
    }

    private record Section(string? Heading, string Text);

    private static List<Section> SplitSections(string text)
    {
        var sections = new List<Section>();
        var heading = (string?)null;
        var current = new StringBuilder();
        var insideCodeBlock = false;

        void CloseSection()
        {
            var sectionText = current.ToString().Trim();
            if (sectionText.Length > 0 && sectionText != $"## {heading}")
            {
                sections.Add(new Section(heading, sectionText));
            }
            current.Clear();
        }

        foreach (var line in text.ReplaceLineEndings("\n").Split('\n'))
        {
            if (line.TrimStart().StartsWith("```"))
            {
                insideCodeBlock = !insideCodeBlock;
            }

            var isHeading = !insideCodeBlock && line.StartsWith("## ");
            if (isHeading)
            {
                CloseSection();
                heading = line[3..].Trim();
            }

            current.AppendLine(line);
        }

        CloseSection();
        return sections;
    }

    private static List<string> SplitLongText(string text)
    {
        if (text.Length <= MaxLength)
        {
            return [text];
        }

        var pieces = new List<string>();
        var start = 0;
        while (start < text.Length)
        {
            var end = Math.Min(start + MaxLength, text.Length);
            if (end < text.Length)
            {
                end = LastSpaceBefore(text, end, start + MaxLength / 2);
            }

            pieces.Add(text[start..end].Trim());
            if (end >= text.Length)
            {
                break;
            }

            // The next piece starts a little before the end, at the beginning of a word.
            start = end - Overlap;
            while (start < end && !char.IsWhiteSpace(text[start]))
            {
                start++;
            }
        }

        return pieces;
    }

    private static int LastSpaceBefore(string text, int end, int minimum)
    {
        for (var i = end; i > minimum; i--)
        {
            if (char.IsWhiteSpace(text[i - 1]))
            {
                return i;
            }
        }

        return end;
    }

    [GeneratedRegex(@"[^A-Za-z0-9_=-]")]
    private static partial Regex InvalidIdCharacters();
}
