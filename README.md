# ask-bilal

Backend of the assistant on my portfolio [bilalsaidi.dev](https://bilalsaidi.dev). It answers recruiters' questions about my background, with sources, and checks how well a job offer matches my profile. Answers are built only from my public content (retrieval-augmented generation), and the assistant is tested automatically.

Work in progress. Target release: November 2026.

## Stack

- C# and .NET 10
- Azure OpenAI in Microsoft Foundry (chat and embeddings models)
- Azure AI Search, Azure Functions, Application Insights (planned)
- xUnit

## Current state

| Folder | Content |
| --- | --- |
| `src/Core` | Shared logic (for now, the token cost calculation and the cosine similarity) |
| `src/Playground` | Console app with two demos: `chat` calls the chat model, `embed` compares sentences with embeddings |
| `tests/Unit` | xUnit tests |

## Run the playground

The app signs in with your Azure account, so there is no API key to store.

1. Sign in: `az login`
2. Give your account the role "Cognitive Services OpenAI User" on your Azure OpenAI resource (Azure portal, Access control, or `az role assignment create`).
3. Store the endpoint of your resource (it ends with `/openai/v1/`) in the user secrets:

```bash
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://<your-resource>.openai.azure.com/openai/v1/" --project src/Playground
```

4. Run the chat demo, with an optional question:

```bash
dotnet run --project src/Playground -- chat "What does a software tester do?"
```

The app asks the question with several reasoning levels, with and without a temperature, and prints the answer, the number of tokens, the time and the estimated cost for each call. With this model, a temperature is only accepted when reasoning is set to "none".

5. Run the embeddings demo (it needs a deployment named `embed`):

```bash
dotnet run --project src/Playground -- embed
```

The app turns six sentences into vectors (three about .NET and tests, three about other topics) and prints every pair of sentences with its cosine similarity, from closest to farthest. A score close to 1 means the same meaning, close to 0 means unrelated. It also compares one sentence written in English and in French. Example of the top of the ranking:

```text
Vector size: 1536 numbers

0.574  "A unit test checks that a small piece of C# code behaves as expected."  <->  "xUnit is a testing framework for .NET."
0.402  "The .NET SDK builds and runs the application from the command line."  <->  "xUnit is a testing framework for .NET."
...
0.037  "The .NET SDK builds and runs the application from the command line."  <->  "Simmer the tomato sauce for twenty minutes before adding the basil."
```

Compare the order of the pairs rather than the exact values, and never compare vectors made by two different models.

## Tests

```bash
dotnet test AskBilal.slnx
```

## Next steps

- A search index over the site content (splitting the texts, storing the vectors in Azure AI Search)
- `POST /api/ask` and `POST /api/fit` as Azure Functions
- Evaluation suite that blocks a release when answers get worse

## License

MIT
