# RAG 101 Constitution

RAG 101 is a video course and the application the course builds. The code exists to be explained on screen, so these principles put teaching value ahead of engineering generality. Terms in capitals are defined in `GLOSSARY.md`.

## Core Principles

### I. A Lesson teaches one or two concepts

Each Lesson has stated teaching goals in `docs/course-outline.md`. Code that does not serve those goals does not enter the Lesson. If a change is needed but teaches nothing (a refactoring, a dependency update), it is kept as small as possible and named in the Lesson page's "What changed" section.

### II. Code is written to be read on screen

Explicit beats clever. No abstraction, interface, helper or configuration option is added until a Lesson needs it. A reader who knows C# and web APIs but not RAG must be able to follow any file top to bottom. Prefer one obvious code path over a general one.

### III. MongoDB is the source of truth

Every Document, Chunk and piece of Metadata is stored in MongoDB. The Elasticsearch indices are a projection that can be deleted and rebuilt from MongoDB at any time. See `docs/adr/0001-mongodb-is-the-source-of-truth.md`.

### IV. No settings in files

The only setting read from outside the database is the Bootstrap connection. Everything else is stored in MongoDB and edited from the Settings page. No secret and no environment-specific value is ever committed. See `docs/adr/0002-bootstrap-connection-and-settings-in-mongodb.md`.

### V. One vocabulary

The terms in `GLOSSARY.md` are used as written in code, API routes, UI labels, specs and Lesson pages. A new domain concept is added to the glossary before it is named in code.

### VI. Every Lesson runs from a clean clone

At every Lesson tag, a clean clone builds with one command and runs as one process on one port, with the API serving the frontend. A Lesson is not finished until its Lesson page exists and its demo has been run from a clean clone.

### VII. Tests check real behaviour

Pure logic (text cleaning, chunking) has unit tests. Behaviour that depends on MongoDB or Elasticsearch is tested against real instances, never mocks. The language model is always replaced by a fake in tests, because its output is slow, costly and not repeatable.

## Technology Constraints

- **Backend**: ASP.NET Core on .NET 10, one API project (`src/Rag101.Api`) until a Lesson needs a split.
- **Frontend**: React with TypeScript, built by Vite into the API's `wwwroot`. The Vite dev server is not used. The frontend calls the API with relative URLs. There is no login.
- **Primary database**: MongoDB.
- **Search**: Elasticsearch 9.x through the official `Elastic.Clients.Elasticsearch` package.
- **Language models**: the Cisharpai library, supporting OpenAI, Azure AI Inference and any OpenAI-compatible endpoint. `Cisharpai.Rag` is not used in Section 1; chunking and retrieval are written by hand so they can be explained.
- **Supported platforms**: macOS and Windows fully; Linux through the `RAG_MONGODB_CONNECTION` environment variable only.
- **Infrastructure**: MongoDB and Elasticsearch are provided by whoever runs the application. The repository ships no container or compose files.
- **Language**: all code, documentation and Lesson pages are in English.

## Workflow

- The process for producing a Lesson is in `docs/production-guide.md`.
- Specs are an incremental series and are not tied one-to-one to Lessons. Spec Kit is a production aid; it may be dropped without amending these principles.
- A Lesson is tagged (`s<section>-l<lesson>`, for example `s1-l2`) when its code is finished. The tag may be moved until the video is published. After publication it is frozen, and defects are recorded as Errata on the Lesson page and fixed on `master` only.

## Governance

This constitution takes precedence over other guidance in the repository. A spec or plan that conflicts with a principle must either be changed or justify the exception in writing. Amendments are made by editing this file, updating the version below, and updating any ADR or guide the change affects.

**Version**: 1.0.0 | **Ratified**: 2026-11-15 | **Last Amended**: 2026-11-15
