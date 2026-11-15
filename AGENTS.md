# RAG 101

A video course on Retrieval-Augmented Generation and the application the course builds, Lesson by Lesson. The code is teaching material: it is read on screen in a video.

## Read first

- `GLOSSARY.md` — the vocabulary. Use these terms exactly in code, routes, UI labels and docs. Add a new domain term here before naming it in code.
- `.specify/memory/constitution.md` — the principles and technology constraints. They apply to every change.
- `docs/course-outline.md` — which Lesson is being built and its teaching goals.
- `docs/production-guide.md` — the steps for producing a Lesson, and the tag rules.
- `docs/adr/` — decisions that must not be "fixed" without discussion.

## Rules that are easy to break

- Add only what the current Lesson's teaching goals need. No helpers, interfaces, options or generality "for later".
- Never put a setting or secret in a file. The MongoDB connection string comes from `RAG_MONGODB_CONNECTION` or the OS credential store; everything else is stored in MongoDB.
- Nothing may exist only in Elasticsearch. Write to MongoDB first.
- The API serves the built frontend from `wwwroot` on the same port. Do not add a dev-server proxy, CORS, or absolute API URLs.
- Do not use `Cisharpai.Rag` in Section 1. Use the Cisharpai chat client only.
- Do not mock MongoDB or Elasticsearch in tests. Always fake the language model in tests.
- Never move a tag whose video is published. Never create or move a tag without being asked.

## Layout

```
src/Rag101.Api/     ASP.NET Core API, serves the frontend from wwwroot
src/Rag101.Tests/   unit and integration tests
web/                React + TypeScript frontend, built by Vite into the API's wwwroot
corpus/             sample source files for Ingestion
docs/               course outline, production guide, Lesson pages, ADRs
specs/              Spec Kit specs (incremental, not one per Lesson)
```

Directories are created when first needed.
