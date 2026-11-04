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
| `src/Core` | Shared logic (for now, the token cost calculation) |
| `src/Playground` | Console app that calls the chat model and prints tokens and cost |
| `tests/Unit` | xUnit tests |

## Run the playground

The app signs in with your Azure account, so there is no API key to store.

1. Sign in: `az login`
2. Give your account the role "Cognitive Services OpenAI User" on your Azure OpenAI resource (Azure portal, Access control, or `az role assignment create`).
3. Store the endpoint of your resource (it ends with `/openai/v1/`) in the user secrets:

```bash
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://<your-resource>.openai.azure.com/openai/v1/" --project src/Playground
```

4. Run it, with an optional question:

```bash
dotnet run --project src/Playground -- "What does a software tester do?"
```

The app asks the question with several reasoning levels and prints the answer, the number of tokens, the time and the estimated cost for each call.

## Tests

```bash
dotnet test AskBilal.slnx
```

## Next steps

- Embeddings and a search index over the site content
- `POST /api/ask` and `POST /api/fit` as Azure Functions
- Evaluation suite that blocks a release when answers get worse

## License

MIT
