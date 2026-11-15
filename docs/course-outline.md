# Course outline

The ordered list of Sections and Lessons, with the teaching goals of each. This is the planning document for the course: a Lesson is added here first, before any code or spec is written for it.

Status values: **planned** (goals written), **coded** (tagged, not recorded), **recorded**, **published**.

## Section 0 — Setup

| Lesson | Tag | Title | Status |
|---|---|---|---|
| 0 | `s0-l0` | Prerequisites and configuration | planned |

### s0-l0 — Prerequisites and configuration

**Teaching goals**

- What the course builds and what you need to follow it: .NET 10, Node, a MongoDB instance, an Elasticsearch instance, and access to a language model.
- How the application is configured: the Bootstrap connection, and every other setting stored in MongoDB.

**Runnable state**: the application shell. It starts, serves the frontend, and the Settings page reports whether MongoDB, Elasticsearch and the language model are reachable. There is no Ingestion and no search.

**Known limits (lead-in to s1-l1)**: the application does nothing with documents yet.

## Section 1 — Unstructured data

| Lesson | Tag | Title | Status |
|---|---|---|---|
| 1 | `s1-l1` | Whole-document search and a first Answer | planned |
| 2 | `s1-l2` | Chunking | planned |
| 3 | `s1-l3` | Metadata | planned |

### s1-l1 — Whole-document search and a first Answer

**Teaching goals**

- Retrieval and generation are two separate steps.
- Lexical search (BM25) over the full text of each Document, with Fragments extracted by highlighting.

**Runnable state**: Ingestion reads the Corpus directory and stores each book as one Document with its entire text. The user enters Keywords and a Question in two separate fields. Keywords find Documents and produce Fragments; the Fragments and the Question go to the language model, which writes the Answer. The UI shows the Fragments next to the Answer.

**Known limits (lead-in to s1-l2)**: Fragments are short and cut around matching words with no regard for meaning, and relevance is scored per whole book.

### s1-l2 — Chunking

**Teaching goals**

- Why text is split into Chunks before it is indexed, and what a simple chunking strategy looks like.

**Runnable state**: Ingestion splits each Document into Chunks, and search runs over Chunks. Keywords and Question remain two separate fields, as in s1-l1.

**Known limits (lead-in to s1-l3)**: a Chunk knows nothing about the book it came from or what it is about.

### s1-l3 — Metadata

**Teaching goals**

- Enriching Documents and Chunks with Metadata, both read from the source and extracted by a language model (keywords for the whole Document and for each Chunk).
- Schema flexibility in MongoDB and Elasticsearch: dynamic fields with a type prefix (for example `s_keyword` for a string field) mapped by an Elasticsearch index template.

**Runnable state**: to be detailed in the second planning phase.

## Backlog

Ideas for later Sections, in no order and with no commitment:

- Embeddings and vector search
- Hybrid search and rank fusion
- Reranking
- Deriving the search query from the Question (a single input field)
- Evaluating retrieval and Answers
- Other source formats (PDF, EPUB, Office)
- Background Ingestion with progress reporting
- Structured data
- Adopting `Cisharpai.Rag` in place of hand-written pieces
