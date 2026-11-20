# MongoDB is the source of truth; the Elasticsearch index is a rebuildable projection

Every Document, Chunk and piece of Metadata is stored in MongoDB first. Elasticsearch holds only a search projection of that data, and it must always be possible to delete the indices and rebuild them from MongoDB.

We chose this over indexing straight into Elasticsearch because the course changes the search side repeatedly (chunking strategy, mappings, later embeddings), and each change should be a re-index, not a re-ingestion of the source files.

## Consequences

- Nothing may exist only in Elasticsearch. A field that is searchable must be derivable from what MongoDB holds.
- Ingestion writes twice, so a Lesson may show the two stores briefly out of step.
