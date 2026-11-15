# API contract: Application shell and configuration

All routes are under `/api`, on the same origin as the frontend. Bodies are JSON with camelCase property names. There is no authentication.

Every route except those under `/api/bootstrap` answers `503` with `{ "error": "not_configured" }` while the application has no Bootstrap connection.

## Bootstrap

### `GET /api/bootstrap`

```json
{ "configured": true, "source": "CredentialStore", "canSave": true }
```

- `source`: `"Environment"`, `"CredentialStore"` or `null`.
- `canSave`: false on Linux, and false when the source is `"Environment"`.

### `POST /api/bootstrap/test`

Request: `{ "connectionString": "mongodb://..." }`

Response `200`: `{ "state": "Reachable", "detail": "MongoDB 8.0.4" }` or `{ "state": "Unreachable", "detail": "<reason>" }`

### `PUT /api/bootstrap`

Request: `{ "connectionString": "mongodb://..." }`

- `204`: tested, saved to the credential store, and now in use.
- `400` with `{ "error": "<reason>" }`: the connection test failed; nothing saved.
- `409` with `{ "error": "..." }`: saving is not possible (Linux, or the value comes from the environment variable).

## Settings

### `GET /api/settings`

```json
{
  "elasticsearch": {
    "url": "https://localhost:9200",
    "user": "elastic",
    "hasPassword": true,
    "noAuthentication": false,
    "acceptSelfSignedCertificate": true
  },
  "languageModel": {
    "provider": "OpenAiCompatible",
    "endpoint": "http://localhost:1234/v1",
    "hasApiKey": false,
    "model": "local-model"
  }
}
```

Secrets are never returned. Unset values are empty strings and false.

### `PUT /api/settings`

Same shape, with `password` and `apiKey` in place of `hasPassword` and `hasApiKey`. A secret that is `null` or absent keeps the stored value; an empty string clears it.

Response `204`.

## Status

### `GET /api/status`

```json
{
  "mongoDb": { "state": "Reachable", "detail": "MongoDB 8.0.4" },
  "elasticsearch": { "state": "NotConfigured", "detail": "" }
}
```

`state` is `"Reachable"`, `"Unreachable"` or `"NotConfigured"`. Never calls the language model.

### `POST /api/status/language-model`

Sends one minimal request to the configured language model.

Response `200`: `{ "state": "Reachable", "detail": "<the model's reply>" }`, or `Unreachable` with the error, or `NotConfigured`.

## Reset data

### `POST /api/reset-data`

Deletes the application's data collections and its search indices. Settings are kept. Confirmation is the frontend's responsibility.

```json
{
  "deleted": ["MongoDB collection documents", "Elasticsearch indices rag101-*"],
  "failed": []
}
```

Response is `200` even when part of the reset failed; `failed` then lists each part with its reason.
