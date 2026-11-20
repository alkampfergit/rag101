# Quickstart: validating the application shell

## Prerequisites

- .NET 10 SDK and Node.js 22 or later
- A MongoDB instance and an Elasticsearch 9.x instance you can reach
- Access to a language model (OpenAI, Azure AI Inference, or an OpenAI-compatible endpoint)

## Build and run

```
dotnet run --project src/Rag101.Api
```

The first build installs the frontend packages and builds the frontend into `src/Rag101.Api/wwwroot`. Open the URL printed in the console.

## Tests

```
dotnet test
```

Unit tests always run. Integration tests run only when these are set, and are skipped otherwise:

| Variable | Needed for |
|---|---|
| `RAG_MONGODB_CONNECTION` | MongoDB tests |
| `RAG_TEST_ELASTICSEARCH_URL` | Elasticsearch tests |
| `RAG_TEST_ELASTICSEARCH_USER`, `RAG_TEST_ELASTICSEARCH_PASSWORD` | optional, when Elasticsearch needs authentication |

## Validation scenarios

Each maps to a user story in [spec.md](./spec.md). API shapes are in [contracts/api.md](./contracts/api.md).

### 1. First run (User Story 1)

1. Make sure `RAG_MONGODB_CONNECTION` is not set and no stored connection exists (macOS: `security delete-generic-password -s rag101 -a mongodb-connection`).
2. Start the application and open it. **Expected**: the first-run page.
3. Enter a wrong connection string and press "Test". **Expected**: "Unreachable" with a reason.
4. Enter the right one, test, and save. **Expected**: the Settings page, with MongoDB reachable.
5. Restart the application. **Expected**: the Settings page directly.
6. Stop it, start it with `RAG_MONGODB_CONNECTION` set. **Expected**: the Settings page says the connection comes from the environment variable.

### 2. Settings (User Story 2)

1. Enter Elasticsearch and language model settings and save.
2. Restart. **Expected**: the same values, with the password and API key shown as stored but not readable.
3. Save again without retyping the secrets. **Expected**: status checks still succeed.
4. In the browser's network tab, inspect `GET /api/settings`. **Expected**: no secret in the response.

### 3. Status (User Story 3)

1. With correct settings. **Expected**: MongoDB and Elasticsearch reachable, each with a version.
2. Change the Elasticsearch password to a wrong one and save. **Expected**: Elasticsearch unreachable with a reason; MongoDB still reachable.
3. Press "Test" for the language model. **Expected**: reachable, with a short reply.
4. Reload the page and watch the network tab. **Expected**: no call to `/api/status/language-model`.

### 4. Reset data (User Story 4)

1. Create a `documents` collection in the application's database and an index named `rag101-test`; also create an index named `other-index`.
2. Press "Reset data" and confirm. **Expected**: the collection and `rag101-test` are gone; `other-index` and all settings remain.
