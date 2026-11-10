using System.ClientModel.Primitives;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Playground;

var command = args.FirstOrDefault();
if (command != "chat" && command != "embed" && command != "ask" && command != "scores")
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/Playground -- chat [question]");
    Console.WriteLine("  dotnet run --project src/Playground -- embed");
    Console.WriteLine("  dotnet run --project src/Playground -- ask [--lang fr] [--no-semantic] [--min 1.8] question");
    Console.WriteLine("  dotnet run --project src/Playground -- scores");
    return;
}

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing secret AzureOpenAI:Endpoint (see README).");

// No API key: the token comes from the account used with "az login".
var tokenPolicy = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");
var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };

if (command == "chat")
{
    await ChatDemo.Run(tokenPolicy, options, string.Join(' ', args.Skip(1)));
}
else if (command == "embed")
{
    await EmbeddingDemo.Run(tokenPolicy, options);
}
else
{
    var searchEndpoint = config["AzureSearch:Endpoint"]
        ?? throw new InvalidOperationException("Missing secret AzureSearch:Endpoint (see README).");

    if (command == "scores")
    {
        await AskDemo.Scores(endpoint, searchEndpoint);
        return;
    }

    var semantic = !args.Contains("--no-semantic");
    var minimumScore = AskBilal.Core.AskService.DefaultMinimumScore;
    var lang = "en";
    var words = new List<string>();
    for (var i = 1; i < args.Length; i++)
    {
        if (args[i] == "--min")
        {
            minimumScore = double.Parse(args[++i], System.Globalization.CultureInfo.InvariantCulture);
        }
        else if (args[i] == "--lang")
        {
            lang = args[++i];
        }
        else if (args[i] != "--no-semantic")
        {
            words.Add(args[i]);
        }
    }

    await AskDemo.Ask(endpoint, searchEndpoint, string.Join(' ', words), lang, semantic, minimumScore);
}
