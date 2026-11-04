// The reasoning options of the OpenAI package are still marked as preview.
#pragma warning disable OPENAI001

using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using Azure.Identity;
using AskBilal.Core;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

// Prices in EUR per million tokens: gpt-5.4-mini, Global Standard, Sweden Central.
// Source: Azure Retail Prices API, effective 2026-03-01.
var prices = new TokenPrices(InputPerMillion: 0.6733m, OutputPerMillion: 4.0397m);

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var endpoint = config["AzureOpenAI:Endpoint"]
    ?? throw new InvalidOperationException("Missing secret AzureOpenAI:Endpoint (see README).");

// No API key: the token comes from the account used with "az login".
var tokenPolicy = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");
var chat = new ChatClient(
    model: "chat", // name of the deployment, not of the model
    authenticationPolicy: tokenPolicy,
    options: new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

var question = args.Length > 0 ? string.Join(' ', args) : "What does a software tester do?";
List<ChatMessage> messages =
[
    new SystemChatMessage("You answer questions about software development. Answer in two sentences."),
    new UserChatMessage(question),
];

Console.WriteLine($"Question: {question}");
Console.WriteLine();

// Try several reasoning levels. "none" is the cheapest and fastest.
foreach (var effort in new[] { "none", "low", "medium" })
{
    var options = new ChatCompletionOptions { ReasoningEffortLevel = new ChatReasoningEffortLevel(effort) };
    await AskAndPrint($"reasoning = {effort}", options);
}

// The documentation says reasoning models refuse "temperature". Check it for real.
var withTemperature = new ChatCompletionOptions
{
    ReasoningEffortLevel = new ChatReasoningEffortLevel("none"),
    Temperature = 0.2f,
};
await AskAndPrint("reasoning = none, temperature = 0.2", withTemperature);

async Task AskAndPrint(string label, ChatCompletionOptions options)
{
    Console.WriteLine($"--- {label}");
    var timer = Stopwatch.StartNew();
    try
    {
        ChatCompletion completion = await chat.CompleteChatAsync(messages, options);
        timer.Stop();

        var usage = completion.Usage;
        var cost = TokenCost.Compute(usage.InputTokenCount, usage.OutputTokenCount, prices);
        Console.WriteLine(completion.Content[0].Text);
        Console.WriteLine($"tokens in: {usage.InputTokenCount}, out: {usage.OutputTokenCount} " +
                          $"(thinking: {usage.OutputTokenDetails.ReasoningTokenCount}), " +
                          $"{timer.ElapsedMilliseconds} ms, cost: {cost * 100:F4} cents");
    }
    catch (ClientResultException error)
    {
        Console.WriteLine($"HTTP {error.Status}: {error.Message}");
    }
    Console.WriteLine();
}
