# Data model: Application shell and configuration

## Bootstrap connection

Not stored in MongoDB.

| Field | Type | Notes |
|---|---|---|
| Value | string | The MongoDB connection string. Never logged, never sent to the browser. |
| Source | `Environment`, `CredentialStore` or none | Where the value came from. Shown in the UI. |

**Resolution order**: `RAG_MONGODB_CONNECTION` environment variable, then the OS credential store (macOS, Windows), then none ("not configured").

**Credential store entry**: service `rag101`, account `mongodb-connection` (macOS Keychain); target `rag101:mongodb-connection` (Windows Credential Manager).

**Rules**

- A value from the environment cannot be replaced from the UI.
- A value is saved to the credential store only after a successful connection test.
- On Linux there is no credential store; saving is not offered.

## Settings

One document in the `settings` collection of the application's database, with `_id` = `"settings"`.

```text
settings
├── _id: "settings"
├── elasticsearch
│   ├── url: string
│   ├── user: string
│   ├── password: string            (plain text)
│   ├── noAuthentication: bool
│   └── acceptSelfSignedCertificate: bool
└── languageModel
    ├── provider: "OpenAi" | "AzureAiInference" | "OpenAiCompatible"
    ├── endpoint: string             (not used for OpenAi)
    ├── apiKey: string               (plain text)
    └── model: string
```

**Rules**

- A missing document, or a missing `elasticsearch` or `languageModel` part, means "not configured" for that dependency.
- Elasticsearch is configured when `url` is not empty.
- The language model is configured when `model` is not empty and, for `AzureAiInference` and `OpenAiCompatible`, `endpoint` is not empty.
- When `noAuthentication` is true, `user` and `password` are ignored.
- On save, a secret sent as null keeps the stored value; an empty string clears it.

## Status

Computed on request, never stored.

| Field | Type | Notes |
|---|---|---|
| State | `Reachable`, `Unreachable`, `NotConfigured` | |
| Detail | string | Server version when reachable; the reason when unreachable; the model's reply for the language model. |

## What the application owns

Used by "Reset data".

| Store | Owned | Never touched |
|---|---|---|
| MongoDB | data collections listed in `DataReset` (currently `documents`) | `settings`, any other collection or database |
| Elasticsearch | indices named `rag101-*` | every other index |

The application's database is the one named in the Bootstrap connection, or `rag101` when the connection string names none.
