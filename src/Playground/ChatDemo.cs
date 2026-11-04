// The reasoning options of the OpenAI package are still marked as preview.
#pragma warning disable OPENAI001

using System.ClientModel;
using System.ClientModel.Primitives;
using System.Diagnostics;
using AskBilal.Core;
using OpenAI;
using OpenAI.Chat;

namespace Playground;

public static class ChatDemo
{
    // Prices in EUR per million tokens: gpt-5.4-mini, Global Standard, Sweden Central.
    // Source: Azure Retail Prices API, effective 2026-03-01.
    private static readonly TokenPrices Prices = new(InputPerMillion: 0.6733m, OutputPerMillion: 4.0397m);

    public static async Task Run(BearerTokenPolicy tokenPolicy, OpenAIClientOptions options, string question)
    {
        var chat = new ChatClient(
            model: "chat", // name of the deployment, not of the model
            authenticationPolicy: tokenPolicy,
            options: options);

        if (question == "")
        {
            question = "What does a software tester do?";
        }

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
            var callOptions = new ChatCompletionOptions { ReasoningEffortLevel = new ChatReasoningEffortLevel(effort) };
            await AskAndPrint(chat, messages, $"reasoning = {effort}", callOptions);
        }

        // The documentation says reasoning models refuse "temperature". Check it for each level.
        foreach (var effort in new[] { "none", "low", "medium" })
        {
            var callOptions = new ChatCompletionOptions
            {
                ReasoningEffortLevel = new ChatReasoningEffortLevel(effort),
                Temperature = 0.2f,
            };
            await AskAndPrint(chat, messages, $"reasoning = {effort}, temperature = 0.2", callOptions);
        }
    }

    private static async Task AskAndPrint(
        ChatClient chat, List<ChatMessage> messages, string label, ChatCompletionOptions options)
    {
        Console.WriteLine($"--- {label}");
        var timer = Stopwatch.StartNew();
        try
        {
            ChatCompletion completion = await chat.CompleteChatAsync(messages, options);
            timer.Stop();

            var usage = completion.Usage;
            var cost = TokenCost.Compute(usage.InputTokenCount, usage.OutputTokenCount, Prices);
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
}
