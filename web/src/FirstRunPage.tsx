import { useState } from "react";
import { type BootstrapInfo, type DependencyStatus, saveBootstrap, testBootstrap } from "./api";
import { buttonClass, Card, Field, inputClass, secondaryButtonClass, StatusLine } from "./ui";

// Shown while the application has no Bootstrap connection: the MongoDB
// connection string is the one setting needed before any other can be read.
export default function FirstRunPage({
  bootstrap,
  onSaved,
}: {
  bootstrap: BootstrapInfo;
  onSaved: () => void;
}) {
  const [connectionString, setConnectionString] = useState("");
  const [testResult, setTestResult] = useState<DependencyStatus | null>(null);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  // On Linux there is no credential store to save into.
  if (!bootstrap.canSave) {
    return (
      <Card title="MongoDB connection needed">
        <p className="mb-3 text-sm">
          On this operating system the MongoDB connection string is read only from an
          environment variable. Set it, then start the application again:
        </p>
        <pre className="rounded bg-slate-100 p-3 text-sm">
          export RAG_MONGODB_CONNECTION="mongodb://localhost:27017"
        </pre>
      </Card>
    );
  }

  const test = async () => {
    setBusy(true);
    setError("");
    setTestResult(
      await testBootstrap(connectionString).catch(
        (e: Error): DependencyStatus => ({ state: "Unreachable", detail: e.message }),
      ),
    );
    setBusy(false);
  };

  const save = async () => {
    setBusy(true);
    setError("");
    try {
      await saveBootstrap(connectionString);
      onSaved();
    } catch (e) {
      setError((e as Error).message);
    }
    setBusy(false);
  };

  return (
    <Card title="Welcome: connect to MongoDB">
      <p className="mb-4 text-sm text-slate-600">
        RAG 101 keeps all of its settings in MongoDB, so the first thing it needs is a MongoDB
        connection string. It is saved in your operating system's credential store, never in a
        file.
      </p>
      <Field label="MongoDB connection string">
        <input
          type="password"
          className={inputClass}
          placeholder="mongodb://localhost:27017"
          value={connectionString}
          onChange={(e) => setConnectionString(e.target.value)}
        />
      </Field>
      <div className="flex gap-2">
        <button className={secondaryButtonClass} disabled={busy || !connectionString} onClick={test}>
          Test
        </button>
        <button className={buttonClass} disabled={busy || !connectionString} onClick={save}>
          Save
        </button>
      </div>
      {testResult && <StatusLine name="MongoDB" status={testResult} />}
      {error && <p className="mt-3 text-sm text-red-700">{error}</p>}
    </Card>
  );
}
