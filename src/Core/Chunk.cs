namespace AskBilal.Core;

/// <summary>A piece of public content before it is cut: a README, or one page of the site.</summary>
public record SourceDocument(string Id, string Title, string Project, string Url, string Lang, string Type, string Text);

/// <summary>One searchable piece of content, with what is needed to cite its source.</summary>
public record Chunk(string Id, string Content, string Title, string Project, string Url, string Lang, string Type);
