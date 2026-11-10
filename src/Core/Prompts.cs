namespace AskBilal.Core;

public static class Prompts
{
    /// <summary>The rules of the assistant, from prompts/ask.txt.</summary>
    public static string Ask()
    {
        using var stream = typeof(Prompts).Assembly.GetManifestResourceStream("ask.txt")
            ?? throw new InvalidOperationException("The prompt ask.txt is missing from the library.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
