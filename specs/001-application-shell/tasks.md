# Tasks: Application shell and configuration

**Input**: Design documents from `/specs/001-application-shell/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/api.md, quickstart.md

**Tests**: included, as required by constitution principle VII. Unit tests for pure logic; integration tests against real MongoDB and Elasticsearch, skipped when their environment variables are absent; the language model is faked.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: can run in parallel (different files, no dependency on an unfinished task)
- **[Story]**: the user story the task belongs to

## Phase 1: Setup

- [ ] T001 Create `Rag101.slnx` with `src/Rag101.Api` (ASP.NET Core empty web project, net10.0) and `src/Rag101.Tests` (xUnit, net10.0, references the API project)
- [ ] T002 Add package references in `src/Rag101.Api/Rag101.Api.csproj`: `MongoDB.Driver`, `Elastic.Clients.Elasticsearch`, `Cisharpai`, `Cisharpai.OpenAi`, `Cisharpai.Azure`
- [ ] T003 [P] Create the frontend in `web/` (Vite, React, TypeScript, Tailwind through its Vite plugin) with `vite.config.ts` building into `../src/Rag101.Api/wwwroot`
- [ ] T004 Add an MSBuild target to `src/Rag101.Api/Rag101.Api.csproj` that runs `npm ci` when `web/node_modules` is missing and `npm run build` when frontend sources are newer than `wwwroot/index.html`
- [ ] T005 [P] Ignore `src/Rag101.Api/wwwroot/` in `.gitignore`, and reduce `src/Rag101.Api/appsettings.json` to logging defaults only

## Phase 2: Foundational

**Purpose**: what every user story needs. No story can start before this phase is done.

- [ ] T006 In `src/Rag101.Api/Program.cs`: serve static files, fall back to `index.html` for non-API paths, serialize enums as strings, and create the `/api` route group
- [ ] T007 [P] Create `DependencyStatus` (state `Reachable`, `Unreachable`, `NotConfigured`, plus detail) in `src/Rag101.Api/Status/DependencyStatus.cs`
- [ ] T008 [P] Create `web/src/api.ts` with typed functions for every route in `contracts/api.md`, and `web/src/App.tsx` that asks `GET /api/bootstrap` and renders the first-run page or the Settings page

**Checkpoint**: `dotnet run --project src/Rag101.Api` serves a page on one port.

## Phase 3: User Story 1 - First run: provide the Bootstrap connection (P1) 🎯 MVP

**Goal**: the application starts unconfigured, accepts and stores the MongoDB connection string, and becomes usable without a restart.

**Independent Test**: quickstart scenario 1.

### Tests

- [ ] T009 [P] [US1] Unit tests for the resolution order (environment variable wins; then credential store; then none; a value from the environment cannot be replaced) using an in-memory credential store, in `src/Rag101.Tests/Unit/BootstrapConnectionTests.cs`

### Implementation

- [ ] T010 [P] [US1] Create `ICredentialStore` (read, write, `IsSupported`) in `src/Rag101.Api/Bootstrap/ICredentialStore.cs`
- [ ] T011 [P] [US1] Implement `MacKeychainCredentialStore` with `/usr/bin/security`, service `rag101`, account `mongodb-connection`, in `src/Rag101.Api/Bootstrap/MacKeychainCredentialStore.cs`
- [ ] T012 [P] [US1] Implement `WindowsCredentialStore` with `CredRead`/`CredWrite`/`CredFree`, target `rag101:mongodb-connection`, in `src/Rag101.Api/Bootstrap/WindowsCredentialStore.cs`
- [ ] T013 [P] [US1] Implement `UnsupportedCredentialStore` (Linux: reads nothing, `IsSupported` false) in `src/Rag101.Api/Bootstrap/UnsupportedCredentialStore.cs`
- [ ] T014 [US1] Implement `BootstrapConnection` (resolve at start-up from `RAG_MONGODB_CONNECTION` then the credential store; expose value, source, `CanSave`; accept a new value) in `src/Rag101.Api/Bootstrap/BootstrapConnection.cs`
- [ ] T015 [US1] Implement `MongoDatabaseProvider` (creates the client from the current Bootstrap connection, recreates it when the value changes; database from the connection string or `rag101`) in `src/Rag101.Api/Data/MongoDatabaseProvider.cs`
- [ ] T016 [US1] Implement `MongoDbStatusCheck` (ping and server version, 5 second timeout, usable with an arbitrary connection string for the first-run test) in `src/Rag101.Api/Status/MongoDbStatusCheck.cs`
- [ ] T017 [US1] Map `GET /api/bootstrap`, `POST /api/bootstrap/test`, `PUT /api/bootstrap` in `src/Rag101.Api/Program.cs`, and the filter that answers `503 not_configured` on every other `/api` route while unconfigured
- [ ] T018 [US1] Build `web/src/FirstRunPage.tsx`: one field, "Test" and "Save", the result of the test, and the Linux variant that explains the environment variable and offers no field

**Checkpoint**: quickstart scenario 1 passes.

## Phase 4: User Story 2 - Configure Elasticsearch and the language model (P2)

**Goal**: settings are edited in the UI, stored in MongoDB, and secrets never reach the browser.

**Independent Test**: quickstart scenario 2.

### Tests

- [ ] T019 [P] [US2] Unit tests for the secret rule ("a secret sent as null keeps the stored value; an empty string clears it") and for the read contract carrying `hasPassword`/`hasApiKey` and no secret, in `src/Rag101.Tests/Unit/SettingsContractsTests.cs`
- [ ] T020 [P] [US2] Integration test: save, load, and save-without-secrets against real MongoDB (database `rag101-tests`, skipped without `RAG_MONGODB_CONNECTION`), in `src/Rag101.Tests/Integration/SettingsStoreTests.cs`

### Implementation

- [ ] T021 [P] [US2] Create `AppSettings` with `ElasticsearchSettings` (url, user, password, noAuthentication, acceptSelfSignedCertificate) and `LanguageModelSettings` (provider `OpenAi` | `AzureAiInference` | `OpenAiCompatible`, endpoint, apiKey, model), `_id` = `"settings"`, in `src/Rag101.Api/Settings/AppSettings.cs`
- [ ] T022 [P] [US2] Create the read and write contracts and the mapping between them and `AppSettings` in `src/Rag101.Api/Settings/SettingsContracts.cs`
- [ ] T023 [US2] Implement `SettingsStore` (load the single document or an empty one; replace it on save) in `src/Rag101.Api/Settings/SettingsStore.cs`
- [ ] T024 [US2] Map `GET /api/settings` and `PUT /api/settings` in `src/Rag101.Api/Program.cs`
- [ ] T025 [US2] Build the settings form in `web/src/SettingsPage.tsx`: Elasticsearch fields with "No authentication" disabling user and password, "Accept self-signed certificate", language model provider selecting which fields are shown, masked secrets, and the plain-text warning (FR-016)

**Checkpoint**: quickstart scenario 2 passes.

## Phase 5: User Story 3 - See whether everything is reachable (P3)

**Goal**: the Settings page shows a status per dependency; the language model is tested only on demand.

**Independent Test**: quickstart scenario 3.

### Tests

- [ ] T026 [P] [US3] Integration tests for `ElasticsearchStatusCheck`: reachable with a version, unreachable with a wrong URL, not configured with empty settings (skipped without `RAG_TEST_ELASTICSEARCH_URL`), in `src/Rag101.Tests/Integration/ElasticsearchStatusCheckTests.cs`
- [ ] T027 [P] [US3] Unit tests for `LanguageModelStatusCheck` with a fake chat client: reply becomes the detail, an error becomes `Unreachable`, empty settings become `NotConfigured`, in `src/Rag101.Tests/Unit/LanguageModelStatusCheckTests.cs`

### Implementation

- [ ] T028 [P] [US3] Implement `ElasticsearchClientFactory` (client from Settings; no credentials when `noAuthentication`; certificate validation off when `acceptSelfSignedCertificate`) in `src/Rag101.Api/Data/ElasticsearchClientFactory.cs`
- [ ] T029 [P] [US3] Implement `LanguageModelClientFactory` (Cisharpai factory; `OpenAiClientConfiguration` for `OpenAi` and `OpenAiCompatible`, `AzureAiInferenceClientConfiguration` for `AzureAiInference`; empty key sent as `none` for `OpenAiCompatible`) in `src/Rag101.Api/Data/LanguageModelClientFactory.cs`
- [ ] T030 [US3] Implement `ElasticsearchStatusCheck` (root info request, 5 second timeout) in `src/Rag101.Api/Status/ElasticsearchStatusCheck.cs`
- [ ] T031 [US3] Implement `LanguageModelStatusCheck` (one minimal request, small token limit) in `src/Rag101.Api/Status/LanguageModelStatusCheck.cs`
- [ ] T032 [US3] Map `GET /api/status` (MongoDB and Elasticsearch, checked independently) and `POST /api/status/language-model` in `src/Rag101.Api/Program.cs`
- [ ] T033 [US3] Add the status panel to `web/src/SettingsPage.tsx`: MongoDB and Elasticsearch status loaded with the page and after each save, the Bootstrap connection source, and a "Test" button for the language model

**Checkpoint**: quickstart scenario 3 passes.

## Phase 6: User Story 4 - Reset data (P4)

**Goal**: a known empty state that keeps the configuration.

**Independent Test**: quickstart scenario 4.

### Tests

- [ ] T034 [P] [US4] Integration test: a `documents` collection and a `rag101-*` index are deleted, the `settings` collection and an unrelated index survive (skipped without the environment variables), in `src/Rag101.Tests/Integration/DataResetTests.cs`

### Implementation

- [ ] T035 [US4] Implement `DataReset` (drop the listed data collections, currently `documents`; delete indices `rag101-*`; report what was deleted and what failed, each part independent) in `src/Rag101.Api/Data/DataReset.cs`
- [ ] T036 [US4] Map `POST /api/reset-data` in `src/Rag101.Api/Program.cs`
- [ ] T037 [US4] Add "Reset data" with a confirmation step and the result to `web/src/SettingsPage.tsx`

**Checkpoint**: quickstart scenario 4 passes.

## Phase 7: Polish

- [ ] T038 [P] Add `.github/workflows/ci.yml`: build the solution (which builds the frontend) and run the tests with no services, so integration tests skip
- [ ] T039 [P] Write the Lesson page `docs/lessons/s0-l0/README.md` from the template, including example commands to start MongoDB and Elasticsearch with Docker and with Apple `container`, and the note on model naming
- [ ] T040 Check that no secret is logged or returned: review log statements and every response contract against FR-010 and FR-015
- [ ] T041 Run the quickstart scenarios end to end and record the result

## Dependencies

- Phase 1 → Phase 2 → user stories.
- US1 blocks US2, US3 and US4: all of them need the MongoDB database that the Bootstrap connection provides.
- US3 needs the Settings from US2. US4 needs `ElasticsearchClientFactory` from US3.
- Within a story: tests and `[P]` tasks first, then services, then endpoints, then UI.

## Parallel opportunities

- T003 and T005 alongside T001–T002.
- T010–T013 (four small files) together, with T009.
- T021 and T022 together, with T019–T020.
- T028 and T029 together, with T026–T027.

## Implementation strategy

MVP is User Story 1: an application that starts, asks for its connection and remembers it. Each later story adds one visible capability to the Settings page and can be demonstrated on its own.
