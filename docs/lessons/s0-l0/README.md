# s0-l0 — Prerequisites and configuration

| | |
|---|---|
| **Tag** | `s0-l0` |
| **Video** | _link, added when published_ |
| **Previous Lesson** | none |

## Teaching goals

- What the course builds and what you need to follow it.
- How the application is configured: the Bootstrap connection, and every other setting stored in MongoDB.

## Prerequisites

- .NET 10 SDK
- Node.js 22 or later
- A MongoDB instance
- An Elasticsearch 9.x instance
- Access to a language model: OpenAI, Azure AI Inference, or any OpenAI-compatible endpoint (LiteLLM, llama.cpp, LM Studio)

The application does not start MongoDB or Elasticsearch for you. If you have neither, these commands start one of each on your machine, without authentication. They are examples for a course, not a production setup.

With Docker:

```
docker run -d --name rag101-mongo -p 27017:27017 mongo:8

docker run -d --name rag101-es -p 9200:9200 -m 3g \
  -e discovery.type=single-node -e xpack.security.enabled=false \
  docker.elastic.co/elasticsearch/elasticsearch:9.5.5
```

With Apple `container` (macOS):

```
container system start

container run -d --name rag101-mongo -p 27017:27017 docker.io/library/mongo:8

container run -d --name rag101-es -p 9200:9200 --memory 3g \
  -e discovery.type=single-node -e xpack.security.enabled=false \
  docker.elastic.co/elasticsearch/elasticsearch:9.5.5
```

With these, the MongoDB connection string is `mongodb://localhost:27017` and the Elasticsearch URL is `http://localhost:9200` with "No authentication" ticked.

## Run this Lesson

```
git checkout s0-l0
dotnet run --project src/Rag101.Api
```

The first build installs the frontend packages and builds the frontend, so it takes longer. Open `http://localhost:5101`.

## 1. The problem

A RAG application depends on three things outside itself: a database, a search engine and a language model. Before any of the interesting parts, it has to know where they are, and you have to know they answer.

## 2. The concept

**One setting outside, everything else inside.** The application needs exactly one value before it can start: where MongoDB is. This is the Bootstrap connection. It is looked for in this order:

1. the `RAG_MONGODB_CONNECTION` environment variable
2. the operating system's credential store: the Keychain on macOS, the Credential Manager on Windows

On Linux only the environment variable is supported.

Every other setting (Elasticsearch, the language model) is stored in MongoDB and edited from the Settings page. No setting is ever written in a file of the repository, so nothing secret can be committed by mistake.

**Two stores, two roles.** MongoDB is the primary database: it will hold every Document. Elasticsearch is the search engine: it will hold a copy of the data shaped for searching, which can always be rebuilt from MongoDB.

**One language model interface.** The application talks to the language model through the Cisharpai library, which gives the same interface for OpenAI, Azure AI Inference and any OpenAI-compatible endpoint.

## 3. What changed

Everything is new. Show these, in this order:

- `src/Rag101.Api/Program.cs` — the whole API on one page: what is registered, and the eight routes. The frontend is served from the same process.
- `src/Rag101.Api/Bootstrap/BootstrapConnection.cs` — the resolution order: environment variable, then credential store, then "not configured".
- `src/Rag101.Api/Bootstrap/MacKeychainCredentialStore.cs` — how the Keychain is read and written, with the same `security` commands you can type in a terminal. `WindowsCredentialStore.cs` does the same for Windows.
- `src/Rag101.Api/Settings/AppSettings.cs` — the single settings document stored in MongoDB.
- `src/Rag101.Api/Settings/SettingsContracts.cs` — why the browser never receives a password or an API key.
- `src/Rag101.Api/Status/` — the three checks. MongoDB and Elasticsearch are checked when the page opens; the language model only on request, because that call costs something.
- `src/Rag101.Api/Data/LanguageModelClientFactory.cs` — one settings object becomes a Cisharpai client for the chosen provider.
- `src/Rag101.Api/Data/DataReset.cs` — "Reset data" and how it knows what it is allowed to delete.
- `web/src/` — two pages, `FirstRunPage.tsx` and `SettingsPage.tsx`, and `api.ts` with every call to the API.

## 4. Demo

1. Start the application with nothing configured and open it. **Expected**: the first-run page, asking for the MongoDB connection string.
2. Enter `mongodb://localhost:9999` and press "Test". **Expected**: "Unreachable", with "No answer from MongoDB at localhost:9999 within 5 seconds."
3. Enter the right connection string, press "Test", then "Save". **Expected**: the Settings page, with MongoDB "Reachable" and its version, and Elasticsearch "Not configured".
4. In a terminal on macOS, run `security find-generic-password -s rag101 -a mongodb-connection -w`. **Expected**: the connection string you saved.
5. Enter the Elasticsearch URL, tick "No authentication" if it has none, and press "Save settings". **Expected**: Elasticsearch "Reachable" with its version.
6. Choose the language model provider, enter its endpoint, key and model, and save. Press "Test". **Expected**: "Reachable", with the model's one-word reply. The exact reply varies.
7. Stop and start the application. **Expected**: the Settings page directly, with the saved values and the secrets shown as stored but not readable.
8. Press "Reset data" and confirm. **Expected**: "Deleted: MongoDB collection documents", and the settings unchanged.

### A note on model names

With the OpenAI and OpenAI-compatible providers, a model whose name starts with `gpt-5` is called through OpenAI's newer Responses API. Local servers such as llama.cpp and LM Studio may not implement it. If you use a proxy or a local server, give the model a name that does not start with `gpt-5`.

## Known limits

- The application does nothing with documents yet: there is no Ingestion and no search.
- Passwords and API keys are stored in MongoDB in plain text. Anyone who can read the database can read them. This is acceptable for a course application and is not a pattern for production.
- The connection string is passed to the `security` tool as an argument on macOS, so it is briefly visible to other processes of the same user.

## Errata

_None._
