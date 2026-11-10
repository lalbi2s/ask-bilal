# ask-bilal

Backend of the assistant on my portfolio [bilalsaidi.dev](https://bilalsaidi.dev). It answers recruiters' questions about my background, with sources, and checks how well a job offer matches my profile. Answers are built only from my public content (retrieval-augmented generation), and the assistant is tested automatically.

Work in progress. Target release: November 2026.

## Stack

- C# and .NET 10
- Azure OpenAI in Microsoft Foundry (chat and embeddings models)
- Azure AI Search (hybrid search with the semantic ranker)
- Azure Functions, Application Insights (planned)
- xUnit

## Current state

| Folder | Content |
| --- | --- |
| `src/Core` | Shared logic: splitting texts, index schema, search, the ask service, token cost, cosine similarity |
| `src/Indexer` | Console app that fills the search index from the site content and the READMEs of public repositories |
| `src/Playground` | Console app to try things: `chat`, `embed`, and `ask` (temporary, it goes away when the API arrives) |
| `prompts/ask.txt` | Rules given to the chat model |
| `tests/Unit` | xUnit tests, with fake search and chat objects (no Azure account needed) |

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

## Build the search index

The indexer reads two kinds of content from a `content/` folder (ignored by Git):

- `content/knowledge.json`: the public texts of the site, generated at build time at `/ai/knowledge.json`. Copy that file from the site build.
- `content/readmes/*.md`: the README of each public repository, downloaded by the indexer.

```bash
dotnet user-secrets set "AzureSearch:Endpoint" "https://<your-search-service>.search.windows.net" --project src/Indexer
dotnet run --project src/Indexer -- fetch-readmes
dotnet run --project src/Indexer -- rebuild
```

`rebuild` cuts the texts into chunks of about 1,500 characters (200 characters of overlap, README files by `##` section), stops if a chunk looks like a phone number or a secret, turns each chunk into a vector, deletes the index, creates it again and uploads everything. The schema of the index is in `src/Core/ChunkIndex.cs`, so the index can always be recreated with this one command.

Like the model, the search service is reached without a key. Give your account the roles "Search Service Contributor" and "Search Index Data Contributor" on the service. The first run uses `az login` and the same user secrets as the playground.

## Ask a question

```bash
dotnet run --project src/Playground -- ask "What did Bilal do during his internship?"
dotnet run --project src/Playground -- ask --lang fr "Qu'a-t-il fait pendant son stage ?"
```

The question becomes a vector, then a hybrid search (words and vector) with the semantic ranker returns the 5 best chunks. If even the best one scores under the threshold, the answer is a polite refusal and the model is not called. Otherwise the model answers with the numbered chunks only and cites them as `[1]`, `[2]`. The command prints the answer, the tokens used and the sources.

`scores` prints the best score of a list of good and off-topic questions, to choose the threshold:

```bash
dotnet run --project src/Playground -- scores
```

## Tests

```bash
dotnet test AskBilal.slnx
```

## Next steps

- `POST /api/ask` and `POST /api/fit` as Azure Functions
- Evaluation suite that blocks a release when answers get worse

## License

MIT
