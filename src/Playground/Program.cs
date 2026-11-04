using System.ClientModel.Primitives;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Playground;

var command = args.FirstOrDefault();
if (command != "chat")
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project src/Playground -- chat [question]");
    return;
}

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing secret AzureOpenAI:Endpoint (see README).");

// No API key: the token comes from the account used with "az login".
var tokenPolicy = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");
var options = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };

await ChatDemo.Run(tokenPolicy, options, string.Join(' ', args.Skip(1)));
