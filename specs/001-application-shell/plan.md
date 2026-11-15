# Implementation Plan: Application shell and configuration

**Branch**: `001-application-shell` | **Date**: 2026-11-15 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-application-shell/spec.md`

## Summary

Build the runnable state of Lesson `s0-l0`: one ASP.NET Core process that serves a React frontend and a small JSON API on the same port. The application resolves its Bootstrap connection from an environment variable or the OS credential store, keeps every other setting in MongoDB, reports the reachability of MongoDB, Elasticsearch and the language model, and offers a "Reset data" action. There is no Ingestion and no search.

## Technical Context

**Language/Version**: C# on .NET 10 (backend); TypeScript 5 with React 19 (frontend)

**Primary Dependencies**: ASP.NET Core minimal APIs; `MongoDB.Driver` 3.x; `Elastic.Clients.Elasticsearch` 9.x; `Cisharpai`, `Cisharpai.OpenAi`, `Cisharpai.Azure` 0.4.0; Vite, React, Tailwind CSS

**Storage**: MongoDB (settings, later Documents); Elasticsearch (search indices, none created yet); macOS Keychain / Windows Credential Manager (Bootstrap connection only)

**Testing**: xUnit. Unit tests for pure logic; integration tests against real MongoDB and Elasticsearch, skipped when the test environment variables are absent; the language model is faked

**Target Platform**: macOS and Windows developer machines; Linux with the environment variable only

**Project Type**: web application — one backend project that serves the built frontend

**Performance Goals**: none beyond a responsive UI; status checks time out after 5 seconds so the Settings page never hangs

**Constraints**: single process, single port, no login, no settings in files, frontend built into `wwwroot` by `dotnet build`

**Scale/Scope**: one user on one machine; two pages (first run, Settings); eight API endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Result |
|---|---|---|
| I. One or two concepts per Lesson | The feature contains only what the setup Lesson shows: configuration and status | Pass |
| II. Read on screen | One class per responsibility, no interfaces except where a test fake needs one, no router or state library in the frontend | Pass |
| III. MongoDB is the source of truth | Nothing is written to Elasticsearch in this feature; "Reset data" deletes from both | Pass |
| IV. No settings in files | `appsettings.json` holds logging defaults only; the Bootstrap connection comes from the environment or credential store; the rest is in MongoDB | Pass |
| V. One vocabulary | "Bootstrap connection", "Settings", "Document" used in code, routes and UI | Pass |
| VI. Runs from a clean clone | `dotnet run --project src/Rag101.Api` builds the frontend and serves everything on one port | Pass |
| VII. Tests check real behaviour | No mocks of MongoDB or Elasticsearch; the language model test uses a fake | Pass |

Re-checked after Phase 1 design: no violations. One interface is introduced, `ICredentialStore`, because the platform implementations differ and the bootstrap resolution order is unit-tested with an in-memory store. This is a real need of this Lesson, not speculative generality.

## Project Structure

### Documentation (this feature)

```text
specs/001-application-shell/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── api.md
├── checklists/
│   └── requirements.md
└── tasks.md             # created by /speckit-tasks
```

### Source Code (repository root)

```text
Rag101.slnx
src/
├── Rag101.Api/
│   ├── Program.cs                     # wiring and all endpoint mappings
│   ├── Bootstrap/
│   │   ├── BootstrapConnection.cs     # resolves and holds the Bootstrap connection
│   │   ├── ICredentialStore.cs
│   │   ├── MacKeychainCredentialStore.cs
│   │   ├── WindowsCredentialStore.cs
│   │   └── UnsupportedCredentialStore.cs
│   ├── Settings/
│   │   ├── AppSettings.cs             # the stored settings document
│   │   ├── SettingsStore.cs           # read and write in MongoDB
│   │   └── SettingsContracts.cs       # what the browser sends and receives
│   ├── Status/
│   │   ├── DependencyStatus.cs
│   │   ├── MongoDbStatusCheck.cs
│   │   ├── ElasticsearchStatusCheck.cs
│   │   └── LanguageModelStatusCheck.cs
│   ├── Data/
│   │   ├── MongoDatabaseProvider.cs   # the application's database
│   │   ├── ElasticsearchClientFactory.cs
│   │   ├── LanguageModelClientFactory.cs
│   │   └── DataReset.cs               # "Reset data"
│   └── wwwroot/                       # built frontend, not committed
└── Rag101.Tests/
    ├── Unit/
    └── Integration/
web/
├── index.html
├── vite.config.ts
├── package.json
└── src/
    ├── main.tsx
    ├── App.tsx
    ├── api.ts
    ├── FirstRunPage.tsx
    └── SettingsPage.tsx
```

**Structure Decision**: one backend project plus one test project, and the frontend in `web/`, as fixed by the constitution. Folders inside the API are named after what the Lesson explains (Bootstrap, Settings, Status, Data), not after technical layers.

## Complexity Tracking

No violations to justify.
