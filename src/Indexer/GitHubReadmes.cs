using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Indexer;

public static class GitHubReadmes
{
    /// <summary>Downloads the README of each public repository of a user (no login needed).</summary>
    public static async Task Fetch(string user, string outputFolder)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("ask-bilal-indexer");

        var reposJson = await http.GetStringAsync($"https://api.github.com/users/{user}/repos?per_page=100&type=owner");
        using var repos = JsonDocument.Parse(reposJson);

        Directory.CreateDirectory(outputFolder);
        foreach (var repo in repos.RootElement.EnumerateArray())
        {
            var name = repo.GetProperty("name").GetString()!;
            if (repo.GetProperty("fork").GetBoolean())
            {
                continue;
            }

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{user}/{name}/readme");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.raw+json"));
            var response = await http.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"{name}: no README");
                continue;
            }

            response.EnsureSuccessStatusCode();
            var text = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync(Path.Combine(outputFolder, $"{name}.md"), text);
            Console.WriteLine($"{name}: {text.Length} characters");
        }
    }
}
