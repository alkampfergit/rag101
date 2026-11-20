# Research: Application shell and configuration

## 1. Reading and writing the macOS Keychain

- **Decision**: call the system tool `/usr/bin/security` (`find-generic-password` and `add-generic-password -U`) with service `rag101` and account `mongodb-connection`.
- **Rationale**: about twenty lines, no native interop, and a viewer can run the same commands in a terminal to see the stored item.
- **Alternatives considered**: P/Invoke into Security.framework (correct but long and opaque on screen); a third-party credential library (a dependency to explain).
- **Known cost**: the value is passed as a process argument when saving, so it is briefly visible to other processes of the same user. Accepted for a course application.

## 2. Reading and writing the Windows Credential Manager

- **Decision**: P/Invoke `CredRead`, `CredWrite` and `CredFree` from `advapi32.dll`, generic credential with target name `rag101:mongodb-connection`.
- **Rationale**: Windows has no command-line tool that reads a stored secret back, so interop is the only dependency-free way.
- **Alternatives considered**: `cmdkey` (cannot read); DPAPI-protected file (a file, against principle IV).
- **Verification limit**: this code cannot be run on the development machine (macOS). It is compiled everywhere and must be checked on Windows before `s0-l0` is recorded.

## 3. Leaving "not configured" mode without a restart

- **Decision**: a singleton `BootstrapConnection` resolves the value at start-up and can be given a new value by the first-run endpoint. `MongoDatabaseProvider` creates its `MongoClient` from the current value and recreates it when the value changes.
- **Rationale**: one small class owns the rule "environment variable, then credential store, then none", which is the concept the Lesson explains.
- **Alternatives considered**: .NET configuration providers with reload tokens (indirect, hard to follow on screen).

## 4. Which database and which indices the application owns

- **Decision**: the database is the one named in the connection string, or `rag101` when none is named. Data collections are listed in one place (`DataReset`), and search indices use the fixed prefix `rag101-`. "Reset data" drops the listed collections and deletes indices matching `rag101-*`. The `settings` collection is never in the list.
- **Rationale**: fixed names are how the reset knows what it may delete (FR-021).
- **Alternatives considered**: dropping the whole database (would delete settings); a configurable prefix (a setting nobody needs yet).

## 5. Elasticsearch client lifetime

- **Decision**: `ElasticsearchClientFactory` builds a client from the stored Settings each time one is needed. "No authentication" omits credentials; "Accept self-signed certificate" disables server certificate validation for that client only.
- **Rationale**: settings changes apply immediately (FR-014) with no cache to invalidate. The cost of building a client per call is irrelevant at this scale.
- **Alternatives considered**: a singleton client rebuilt on settings change (more state, no benefit yet).

## 6. Creating a language model client from stored settings

- **Decision**: register `AddCisharpaiClientFactory().AddOpenAiSupport().AddAzureAiInferenceSupport()` and build a configuration record per call: `OpenAiClientConfiguration` for "OpenAI" (default base URL) and for "OpenAI-compatible endpoint" (`BaseUrl` from Settings), `AzureAiInferenceClientConfiguration` for "Azure AI Inference". Confirmed present in Cisharpai 0.4.0.
- **Rationale**: the factory exists for exactly this case, a provider chosen at run time.
- **Detail**: `ApiKey` is required by the library. Local servers often need none, so an empty key is sent as the placeholder `none` for the OpenAI-compatible provider only.
- **Detail**: the OpenAI client routes model names starting with `gpt-5` to the Responses API, which OpenAI-compatible servers may not implement. Not validated by the application; covered on the Lesson page.

## 7. Status checks

- **Decision**: MongoDB is checked with a `ping` and `buildInfo` command; Elasticsearch with its root info request; both with a 5 second timeout and both reported independently. The language model is checked only from its own endpoint, with one request asking for a one-word reply and a small token limit.
- **Rationale**: FR-017 to FR-019, and no cost from merely opening the page.

## 8. Building the frontend with `dotnet build`

- **Decision**: an MSBuild target in `Rag101.Api.csproj` runs `npm ci` when `web/node_modules` is missing and `npm run build` when frontend sources are newer than the built `index.html`. Vite writes to `src/Rag101.Api/wwwroot`, which is not committed.
- **Rationale**: FR-004, one command from a clean clone. Incremental inputs and outputs keep ordinary backend builds fast.
- **Alternatives considered**: committing the built frontend (stale builds, noisy history); a separate script (a second command to remember).

## 9. Frontend structure

- **Decision**: no router and no state library. `App` asks the API whether the application is configured and renders either the first-run page or the Settings page. The server falls back to `index.html` for any non-API path. Tailwind CSS through its Vite plugin.
- **Rationale**: two pages do not need routing. A router is added in the Lesson that adds a third page.

## 10. Secrets never reach the browser

- **Decision**: the settings read contract carries `hasPassword` and `hasApiKey` booleans instead of the values. The write contract carries nullable secrets, where null means "keep what is stored" and an empty string means "clear".
- **Rationale**: FR-015 and acceptance scenario 2.6, with a rule simple enough to unit test.

## 11. Integration tests and their configuration

- **Decision**: integration tests read `RAG_MONGODB_CONNECTION` and, for Elasticsearch, `RAG_TEST_ELASTICSEARCH_URL` (plus optional `RAG_TEST_ELASTICSEARCH_USER` and `RAG_TEST_ELASTICSEARCH_PASSWORD`). When a variable is absent the tests that need it are skipped, not failed. They use a database named `rag101-tests`.
- **Rationale**: principle VII, and CI can still run the unit tests with no services.
