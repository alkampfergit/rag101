# One Bootstrap connection outside the database; every other setting inside MongoDB

The application needs exactly one setting before it can start: the MongoDB connection string. It is read from the `RAG_MONGODB_CONNECTION` environment variable if set, otherwise from the macOS Keychain or the Windows Credential Manager. On Linux only the environment variable is supported. If none is found, the application starts in a "not configured" mode and the first-run page asks for it.

All other settings (Elasticsearch URL and credentials, language model provider, endpoint, key and model) are stored in MongoDB and edited from the Settings page. No setting lives in `appsettings.json` or any other file in the repository.

We chose this so that a student never edits a file that could be committed with a secret in it, and so that configuration is changed from the UI on camera.

## Considered options

- **`appsettings.json` or .NET user secrets**: the conventional choice, rejected because it puts secrets in files and makes setup a text-editing exercise.
- **Encrypting the secrets stored in MongoDB**: rejected because the encryption key would be a second bootstrap secret.

## Consequences

- The Elasticsearch password and the language model API key are stored in MongoDB in plain text. This is acceptable for a course application and is stated on the setup Lesson page; it is not a pattern for production.
- Anyone who can read the MongoDB database can read those secrets.
