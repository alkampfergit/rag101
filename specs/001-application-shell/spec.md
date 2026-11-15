# Feature Specification: Application shell and configuration

**Feature Branch**: `001-application-shell`

**Created**: 2026-11-15

**Status**: Draft

**Input**: The runnable state of Lesson `s0-l0` (Prerequisites and configuration): the application starts, serves its frontend, lets the user configure it, and reports whether its dependencies are reachable. No Ingestion and no search.

Terms in capitals are defined in `GLOSSARY.md`. Constraints on technology and structure are in `.specify/memory/constitution.md` and are not repeated here.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - First run: provide the Bootstrap connection (Priority: P1)

A student has cloned the repository and started the application for the first time. Nothing is configured. They open the application in the browser and are shown a first-run page with a single field for the MongoDB connection string. They paste it, test it, and save it. The application becomes usable without a restart.

**Why this priority**: every other setting is stored in MongoDB, so nothing else can work until this exists.

**Independent Test**: start the application on a machine with no Bootstrap connection configured, complete the first-run page, and confirm the application leaves "not configured" mode and still has the connection after a restart.

**Acceptance Scenarios**:

1. **Given** no Bootstrap connection is available, **When** the application starts, **Then** it starts successfully in "not configured" mode and every page of the frontend leads to the first-run page.
2. **Given** the first-run page, **When** the user enters a connection string and tests it, **Then** they are told whether MongoDB was reached, with the error message if it was not.
3. **Given** a connection string that tested successfully on macOS or Windows, **When** the user saves it, **Then** it is stored in the operating system's credential store, the application leaves "not configured" mode without a restart, and the Settings page is shown.
4. **Given** a saved Bootstrap connection, **When** the application is restarted, **Then** it starts configured and the first-run page is not shown.
5. **Given** the `RAG_MONGODB_CONNECTION` environment variable is set, **When** the application starts, **Then** that value is used even if a different one is in the credential store, and the first-run page is not shown.
6. **Given** the application runs on Linux with no environment variable set, **When** the user opens it, **Then** the first-run page explains that the `RAG_MONGODB_CONNECTION` environment variable must be set and offers no field to save a value.

---

### User Story 2 - Configure Elasticsearch and the language model (Priority: P2)

With the Bootstrap connection in place, the student opens the Settings page and enters how to reach Elasticsearch and which language model to use. The values are saved and survive a restart.

**Why this priority**: the Lessons that follow need both, and this is where the viewer learns how configuration works in the course.

**Independent Test**: enter Elasticsearch and language model settings, restart the application, and confirm the Settings page shows the saved values with secrets masked.

**Acceptance Scenarios**:

1. **Given** the Settings page, **When** the user enters an Elasticsearch URL, user and password and saves, **Then** the values are stored in MongoDB and shown again after a restart.
2. **Given** the Settings page, **When** the user ticks "No authentication" for Elasticsearch, **Then** the user and password fields are disabled and not required.
3. **Given** an Elasticsearch instance with a self-signed certificate, **When** the user ticks "Accept self-signed certificate" and saves, **Then** the application connects without a certificate error.
4. **Given** the Settings page, **When** the user chooses a language model provider (OpenAI, Azure AI Inference, or OpenAI-compatible endpoint), **Then** only the fields that provider needs are shown: an endpoint URL where applicable, an API key, and a model name.
5. **Given** saved settings that include a password or API key, **When** the Settings page is displayed, **Then** the secret is masked and is never sent back to the browser in full.
6. **Given** a saved secret, **When** the user saves the settings without retyping it, **Then** the stored secret is kept.

---

### User Story 3 - See whether everything is reachable (Priority: P3)

The student wants to know that the setup works before starting the first real Lesson. The Settings page shows a status for MongoDB, Elasticsearch and the language model.

**Why this priority**: it is the end state of the setup Lesson and the starting check for every later recording.

**Independent Test**: with correct settings, all three statuses show as reachable; break one setting and confirm only that status fails, with a readable reason.

**Acceptance Scenarios**:

1. **Given** correct settings, **When** the Settings page is opened, **Then** MongoDB and Elasticsearch each show as reachable, with the server version.
2. **Given** a wrong Elasticsearch password, **When** the status is checked, **Then** Elasticsearch shows as unreachable with the reason, and the MongoDB status is unaffected.
3. **Given** language model settings, **When** the user presses "Test" for the language model, **Then** one minimal request is sent and the result is shown: reachable with the model's reply, or the error returned.
4. **Given** the Settings page, **When** it is opened or refreshed, **Then** no request is sent to the language model unless the user presses "Test".
5. **Given** Elasticsearch or the language model has not been configured yet, **When** the Settings page is opened, **Then** its status reads "not configured", not as an error.

---

### User Story 4 - Reset data (Priority: P4)

Before a demo, or when moving from one Lesson to another, the student wants a known empty state without losing the configuration.

**Why this priority**: there is no data to reset in this Lesson, but every later Lesson page starts with this action, so it must exist from the first tag.

**Independent Test**: create data in the application's data collections and search indices, use "Reset data", and confirm they are gone while all settings remain.

**Acceptance Scenarios**:

1. **Given** the Settings page, **When** the user chooses "Reset data", **Then** they are asked to confirm before anything is deleted.
2. **Given** the user confirms, **When** the reset completes, **Then** all Documents and all search indices created by the application are deleted, and all settings are unchanged.
3. **Given** Elasticsearch is unreachable, **When** the user confirms a reset, **Then** the user is told which part failed and which part was completed.
4. **Given** an Elasticsearch instance that also holds indices not created by this application, **When** a reset is performed, **Then** those indices are untouched.

---

### Edge Cases

- The Bootstrap connection is present but MongoDB is unreachable at startup: the application starts and shows the MongoDB status as unreachable with the reason; it does not show the first-run page and does not discard the stored connection.
- The environment variable is set and the user wants to change the connection: the Settings page states that the value comes from the environment variable and cannot be changed from the UI.
- The connection string contains credentials: it is never written to logs and never returned to the browser.
- The user opens a frontend URL directly (a deep link) or refreshes the page: the frontend loads and shows the right page.
- A request is made to an API route that needs MongoDB while the application is "not configured": it is refused with a clear "not configured" response, not a server error.

## Requirements *(mandatory)*

### Functional Requirements

**Start-up and serving**

- **FR-001**: The application MUST run as a single process on a single port, serving both the API and the frontend.
- **FR-002**: The frontend MUST reach the API on the same address it was loaded from.
- **FR-003**: The application MUST require no login.
- **FR-004**: A clean clone MUST build, including the frontend, with a single command.

**Bootstrap connection**

- **FR-005**: The application MUST resolve the Bootstrap connection in this order: the `RAG_MONGODB_CONNECTION` environment variable; otherwise the operating system credential store on macOS (Keychain) and Windows (Credential Manager); otherwise none.
- **FR-006**: When no Bootstrap connection is found, the application MUST start in "not configured" mode and present the first-run page.
- **FR-007**: The first-run page MUST allow the connection string to be tested before it is saved, and MUST save it to the operating system credential store.
- **FR-008**: On Linux the application MUST support only the environment variable and MUST say so on the first-run page.
- **FR-009**: Saving the Bootstrap connection MUST take effect without restarting the application.
- **FR-010**: The Bootstrap connection MUST never be written to a file in the repository, to logs, or sent to the browser.

**Settings**

- **FR-011**: All settings other than the Bootstrap connection MUST be stored in MongoDB and be editable from the Settings page.
- **FR-012**: Elasticsearch settings MUST include: URL, user, password, a "No authentication" option, and an "Accept self-signed certificate" option.
- **FR-013**: Language model settings MUST include a provider chosen from OpenAI, Azure AI Inference and OpenAI-compatible endpoint, with the endpoint URL, API key and model name that the chosen provider needs.
- **FR-014**: Changed settings MUST take effect without restarting the application.
- **FR-015**: Stored secrets MUST be masked when displayed and MUST NOT be returned to the browser in full.
- **FR-016**: The Settings page MUST state that secrets are stored in MongoDB in plain text.

**Status**

- **FR-017**: The Settings page MUST show a status for MongoDB and for Elasticsearch: reachable with server version, unreachable with reason, or not configured.
- **FR-018**: The language model status MUST be checked only when the user asks, by sending one minimal request, and MUST show the reply or the error.
- **FR-019**: A failure in one status check MUST NOT prevent the others from being shown.

**Reset data**

- **FR-020**: The Settings page MUST offer a "Reset data" action that, after confirmation, deletes all Documents and all search indices created by the application and keeps all settings.
- **FR-021**: "Reset data" MUST NOT delete anything in MongoDB or Elasticsearch that the application did not create.
- **FR-022**: "Reset data" MUST report what was deleted and what failed.

### Key Entities

- **Bootstrap connection**: the MongoDB connection string. Lives outside the database. Has a source (environment variable or credential store) that the UI can display.
- **Settings**: the single set of stored configuration values: Elasticsearch connection details and language model details.
- **Status**: the result of a reachability check for one dependency: state, detail (version or reason), and when it was checked. Not stored.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A student with MongoDB, Elasticsearch and a language model available goes from a fresh clone to a Settings page showing all three as reachable in under 10 minutes, following the Lesson page only.
- **SC-002**: The student edits no file and types no secret into a terminal during setup on macOS and Windows.
- **SC-003**: For each of the three dependencies, a wrong setting produces a message that names the dependency and the cause, without the student reading logs.
- **SC-004**: After "Reset data", the application holds no Documents and no search indices, and the Settings page shows the same values as before.
- **SC-005**: No secret appears in the repository, in application logs, or in any response sent to the browser.

## Assumptions

- The student provides MongoDB and Elasticsearch. The Lesson page gives example commands to start them; the application does not start or manage them.
- One person uses the application on their own machine. Concurrent users and access control are out of scope.
- There is one set of Settings. Profiles and multiple environments are out of scope.
- The names of the MongoDB database and of the search indices are fixed by the application, which is how "Reset data" knows what it owns.
- In this Lesson there are no Documents yet; "Reset data" is specified now so that later Lessons only add to what it deletes.
- Language model use in this Lesson is limited to the reachability test. Embedding models are not configured until a Lesson needs them.
- A model name that the OpenAI client would route to the Responses API (names beginning `gpt-5`) may not work against OpenAI-compatible servers; the Lesson page advises on model naming. The application does not validate it.
