# Production guide

How a Lesson goes from an idea to a published video. The same steps apply to every Lesson. Terms in capitals are defined in `GLOSSARY.md`.

## The steps

### 1. Outline

Add the Lesson to `docs/course-outline.md`: its tag, title, one or two teaching goals, the runnable state it ends in, and the known limits that lead into the next Lesson. If you cannot state the goals in two bullets, the Lesson is too big; split it.

### 2. Code

Build the Lesson's runnable state on a branch and merge it into `master`.

- Specs are optional and incremental. A Lesson may be built from one spec, several, or none. When Spec Kit is used, the flow is `/speckit-specify`, `/speckit-plan`, `/speckit-tasks`, `/speckit-implement`.
- The constitution (`.specify/memory/constitution.md`) applies whether or not a spec is written. The two rules most often broken are: nothing enters the Lesson that its goals do not need, and no abstraction before a Lesson needs it.
- New domain terms go into `GLOSSARY.md` before they are named in code.

### 3. Lesson page

Create `docs/lessons/<tag>/README.md` from `docs/lessons/_template/README.md`. It is both the recording script and the written companion for viewers. The "What changed" section names the new code to show on screen, in prose; it is not a diff.

### 4. Slides (optional)

When the concept part of the video needs more than talking over code, add slides to the Lesson's folder (`docs/lessons/<tag>/`). Keep the source file, not only an export. Slides cover the concept only; code is always shown in the editor.

### 5. Rehearse

From a clean clone of `master`:

1. Build and start the application with the commands on the Lesson page.
2. Use "Reset data" on the Settings page, then ingest the Corpus.
3. Run every step of the demo script and compare with the expected results.

Fix the code or the Lesson page until the demo runs without surprises.

### 6. Tag

When the code for the Lesson is finished, tag the commit on `master`:

```
git tag -a s1-l2 -m "Section 1, Lesson 2: Chunking"
git push origin s1-l2
```

Set the Lesson's status to **coded** in the course outline.

### 7. Record

Go through the pre-recording checklist below, then record. The video has four parts, matching the Lesson page:

1. **The problem**: a limit of the previous Lesson.
2. **The concept**: explained without code, with slides if there are any.
3. **What changed**: a tour of the new code.
4. **The demo**: ending on a limit that motivates the next Lesson.

If recording reveals a defect, fix it, amend or add the commit, and move the tag (see the rules below). Set the status to **recorded**.

### 8. Publish

Publish the video, add its link to the Lesson page, and set the status to **published**. From this point the tag is frozen.

## Tag rules

- Tag names are `s<section>-l<lesson>`: `s0-l0`, `s1-l1`, `s1-l2`.
- A tag is created when the code for its Lesson is finished.
- **Before the video is published**, the tag may be moved to include a fix:

  ```
  git tag -f -a s1-l2 -m "Section 1, Lesson 2: Chunking"
  git push --force origin s1-l2
  ```

- **After the video is published**, the tag is never moved. A defect found later is fixed on `master` only and recorded as Errata on the Lesson page, with a description of the problem and the fixing commit.

## Pre-recording checklist

**State**

- [ ] Clean clone, checked out at the Lesson's tag
- [ ] Application built and started with the commands from the Lesson page
- [ ] Settings page shows MongoDB, Elasticsearch and the language model as reachable
- [ ] "Reset data" done, Corpus ingested
- [ ] Every demo step run once; retrieved Fragments or Chunks match the Lesson page

**Screen**

- [ ] Editor and terminal font size readable at video resolution
- [ ] Browser zoom set, bookmarks bar and unrelated tabs hidden
- [ ] Only the files named in "What changed" open in the editor
- [ ] Notifications off

**Content**

- [ ] Lesson page open on a second screen
- [ ] Slides ready, if the Lesson has any
- [ ] No secret visible: the Settings page masks keys and passwords, and no terminal history shows a connection string

What the language model writes varies between runs. Verify the retrieved text, which is repeatable, and treat the Answer shown on the Lesson page as an illustration.

## Evolving the course

- New ideas go into the Backlog section of the course outline. They become Lessons only when step 1 is done for them.
- A decision that is hard to reverse, surprising without context, and the result of a real trade-off gets an ADR in `docs/adr/`.
- If a step in this guide stops being useful, change the guide; do not skip the step silently.
