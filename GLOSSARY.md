# RAG 101

A video course on Retrieval-Augmented Generation, and the application that the course builds lesson by lesson.

## Language

### Course

**Section**:
A group of Lessons that share one theme, such as "Unstructured data".
_Avoid_: Module, chapter, part

**Lesson**:
One video together with the single runnable state of the application that the video explains.
_Avoid_: Version, V1, step, episode

**Lesson page**:
The written companion of a Lesson: its concepts, talking points, demo script and expected results.
_Avoid_: Script, notes, handout

**Course outline**:
The ordered list of Sections and Lessons with the teaching goals of each.
_Avoid_: Roadmap, syllabus, plan

**Errata**:
A note on a Lesson page describing a defect found after the Lesson was recorded.
_Avoid_: Hotfix, patch note

### Application

**Document**:
One ingested source file, such as a book.
_Avoid_: Book, record, file

**Corpus**:
The set of sample source files shipped with the course for ingestion.
_Avoid_: Dataset, library, samples

**Ingestion**:
Reading the source files of a directory and turning each one into a Document.
_Avoid_: Import, load, indexing

**Chunk**:
A contiguous piece of a Document's text, produced during Ingestion and searchable on its own.
_Avoid_: Segment, passage, split

**Fragment**:
A piece of text cut out of a search hit at query time because it contains the matching terms.
_Avoid_: Snippet, highlight, excerpt

**Keywords**:
The words a user types to find text, matched lexically.
_Avoid_: Query, search terms

**Question**:
The natural-language request a user wants answered from the retrieved text.
_Avoid_: Prompt, query

**Answer**:
The text the language model writes for a Question from the retrieved Fragments or Chunks.
_Avoid_: Response, completion, summary

**Metadata**:
Named values describing a Document or a Chunk, apart from its text.
_Avoid_: Properties, attributes, tags

**Bootstrap connection**:
The one setting the application needs before it can read any other: where its primary database is.
_Avoid_: Root config, master secret
