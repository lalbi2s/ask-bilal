// The reasoning options of the OpenAI package are still marked as preview.
#pragma warning disable OPENAI001

using System.ClientModel.Primitives;
using Azure.Identity;
using OpenAI;
using OpenAI.Chat;

namespace AskBilal.Core;

public class AzureChat : IChat
{
    private readonly ChatClient client;

    public AzureChat(string openAiEndpoint)
    {
        var tokenPolicy = new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default");
        var options = new OpenAIClientOptions { Endpoint = new Uri(openAiEndpoint) };
        client = new ChatClient("chat", tokenPolicy, options); // name of the deployment
    }

    public async Task<ChatAnswer> CompleteAsync(string systemPrompt, string userMessage)
    {
        List<ChatMessage> messages = [new SystemChatMessage(systemPrompt), new UserChatMessage(userMessage)];

        // Reasoning "none": cheaper and faster, and the only level that accepts a temperature.
        var options = new ChatCompletionOptions
        {
            ReasoningEffortLevel = new ChatReasoningEffortLevel("none"),
            Temperature = 0.2f,
        };

        ChatCompletion completion = (await client.CompleteChatAsync(messages, options)).Value;
        var usage = completion.Usage;
        return new ChatAnswer(completion.Content[0].Text, usage.InputTokenCount, usage.OutputTokenCount);
    }
}
