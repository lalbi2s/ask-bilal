using Indexer;
using Microsoft.Extensions.Configuration;

const string ContentFolder = "content";

var command = args.FirstOrDefault();
if (command != "fetch-readmes" && command != "rebuild")
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/Indexer -- fetch-readmes   download the READMEs of the public repositories");
    Console.WriteLine("  dotnet run --project src/Indexer -- rebuild         delete, recreate and fill the search index");
    return 1;
}

if (command == "fetch-readmes")
{
    await GitHubReadmes.Fetch("lalbi2s", Path.Combine(ContentFolder, "readmes"));
    return 0;
}

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var openAiEndpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing secret AzureOpenAI:Endpoint (see README).");
var searchEndpoint = config["AzureSearch:Endpoint"]
    ?? throw new InvalidOperationException("Missing secret AzureSearch:Endpoint (see README).");

return await Rebuild.Run(ContentFolder, openAiEndpoint, searchEndpoint);
