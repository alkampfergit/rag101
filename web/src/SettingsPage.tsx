import { useEffect, useState } from "react";
import {
  type BootstrapInfo,
  type DependencyStatus,
  type LanguageModelProvider,
  type ResetResult,
  type SettingsUpdate,
  type Status,
  getSettings,
  getStatus,
  resetData,
  saveSettings,
  testLanguageModel,
} from "./api";
import { buttonClass, Card, Field, inputClass, secondaryButtonClass, StatusLine } from "./ui";

export default function SettingsPage({ bootstrap }: { bootstrap: BootstrapInfo }) {
  const [form, setForm] = useState<SettingsUpdate | null>(null);
  // Secrets are never sent to the browser: we only know whether one is stored.
  const [stored, setStored] = useState({ password: false, apiKey: false });
  const [status, setStatus] = useState<Status | null>(null);
  const [languageModelStatus, setLanguageModelStatus] = useState<DependencyStatus | null>(null);
  const [testingLanguageModel, setTestingLanguageModel] = useState(false);
  const [resetResult, setResetResult] = useState<ResetResult | null>(null);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const loadSettings = async () => {
    const settings = await getSettings();
    setStored({
      password: settings.elasticsearch.hasPassword,
      apiKey: settings.languageModel.hasApiKey,
    });
    setForm({
      elasticsearch: { ...settings.elasticsearch, password: null },
      languageModel: { ...settings.languageModel, apiKey: null },
    });
  };

  // Checks MongoDB and Elasticsearch. The language model is not called here.
  const loadStatus = async () => {
    setStatus(null);
    setStatus(await getStatus());
  };

  useEffect(() => {
    loadSettings().catch((e: Error) => setError(e.message));
    loadStatus().catch((e: Error) => setError(e.message));
  }, []);

  if (form === null) {
    return <p className="text-sm text-red-700">{error || "Loading…"}</p>;
  }

  const es = form.elasticsearch;
  const lm = form.languageModel;
  const setEs = (change: Partial<SettingsUpdate["elasticsearch"]>) =>
    setForm({ ...form, elasticsearch: { ...es, ...change } });
  const setLm = (change: Partial<SettingsUpdate["languageModel"]>) =>
    setForm({ ...form, languageModel: { ...lm, ...change } });

  const save = async () => {
    setMessage("");
    setError("");
    try {
      await saveSettings(form);
      await loadSettings();
      setMessage("Settings saved.");
      setLanguageModelStatus(null);
      await loadStatus();
    } catch (e) {
      setError((e as Error).message);
    }
  };

  const testModel = async () => {
    setTestingLanguageModel(true);
    setLanguageModelStatus(
      await testLanguageModel().catch(
        (e: Error): DependencyStatus => ({ state: "Unreachable", detail: e.message }),
      ),
    );
    setTestingLanguageModel(false);
  };

  const reset = async () => {
    const confirmed = window.confirm(
      "Delete all Documents and search indices? Settings are kept. This cannot be undone.",
    );
    if (!confirmed) return;
    setResetResult(await resetData().catch((e: Error) => ({ deleted: [], failed: [e.message] })));
  };

  return (
    <>
      <Card title="Status">
        <StatusLine name="MongoDB" status={status?.mongoDb ?? null} />
        <StatusLine name="Elasticsearch" status={status?.elasticsearch ?? null} />
        <div className="flex items-start gap-3 py-2">
          <span className="w-36 shrink-0 text-sm font-medium">Language model</span>
          <button
            className={secondaryButtonClass}
            disabled={testingLanguageModel}
            onClick={testModel}
          >
            {testingLanguageModel ? "Testing…" : "Test"}
          </button>
        </div>
        {languageModelStatus && <StatusLine name="" status={languageModelStatus} />}
        <p className="mt-3 text-xs text-slate-500">
          MongoDB connection read from{" "}
          {bootstrap.source === "Environment"
            ? "the RAG_MONGODB_CONNECTION environment variable. It cannot be changed here."
            : "the operating system's credential store."}
        </p>
      </Card>

      <Card title="Elasticsearch">
        <Field label="URL">
          <input
            className={inputClass}
            placeholder="https://localhost:9200"
            value={es.url}
            onChange={(e) => setEs({ url: e.target.value })}
          />
        </Field>
        <label className="mb-3 flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={es.noAuthentication}
            onChange={(e) => setEs({ noAuthentication: e.target.checked })}
          />
          No authentication
        </label>
        <Field label="User">
          <input
            className={inputClass}
            disabled={es.noAuthentication}
            value={es.user}
            onChange={(e) => setEs({ user: e.target.value })}
          />
        </Field>
        <Field label="Password">
          <input
            type="password"
            className={inputClass}
            disabled={es.noAuthentication}
            placeholder={stored.password ? "•••••••• (stored, leave empty to keep)" : ""}
            value={es.password ?? ""}
            onChange={(e) => setEs({ password: e.target.value === "" ? null : e.target.value })}
          />
        </Field>
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={es.acceptSelfSignedCertificate}
            onChange={(e) => setEs({ acceptSelfSignedCertificate: e.target.checked })}
          />
          Accept self-signed certificate
        </label>
      </Card>

      <Card title="Language model">
        <Field label="Provider">
          <select
            className={inputClass}
            value={lm.provider}
            onChange={(e) => setLm({ provider: e.target.value as LanguageModelProvider })}
          >
            <option value="OpenAi">OpenAI</option>
            <option value="AzureAiInference">Azure AI Inference</option>
            <option value="OpenAiCompatible">OpenAI-compatible endpoint</option>
          </select>
        </Field>
        {lm.provider !== "OpenAi" && (
          <Field label="Endpoint URL">
            <input
              className={inputClass}
              placeholder={
                lm.provider === "OpenAiCompatible"
                  ? "http://localhost:1234/v1"
                  : "https://<resource>.services.ai.azure.com/models"
              }
              value={lm.endpoint}
              onChange={(e) => setLm({ endpoint: e.target.value })}
            />
          </Field>
        )}
        <Field label="API key">
          <input
            type="password"
            className={inputClass}
            placeholder={stored.apiKey ? "•••••••• (stored, leave empty to keep)" : ""}
            value={lm.apiKey ?? ""}
            onChange={(e) => setLm({ apiKey: e.target.value === "" ? null : e.target.value })}
          />
        </Field>
        <Field label="Model">
          <input
            className={inputClass}
            value={lm.model}
            onChange={(e) => setLm({ model: e.target.value })}
          />
        </Field>
      </Card>

      <div className="mb-6">
        <button className={buttonClass} onClick={save}>
          Save settings
        </button>
        {message && <span className="ml-3 text-sm text-green-700">{message}</span>}
        {error && <span className="ml-3 text-sm text-red-700">{error}</span>}
        <p className="mt-2 text-xs text-slate-500">
          Passwords and API keys are stored in MongoDB in plain text. This is a course
          application: do not use credentials you cannot afford to expose.
        </p>
      </div>

      <Card title="Reset data">
        <p className="mb-3 text-sm text-slate-600">
          Deletes all Documents and the search indices created by RAG 101. Settings are kept.
        </p>
        <button className={secondaryButtonClass} onClick={reset}>
          Reset data
        </button>
        {resetResult && (
          <ul className="mt-3 text-sm">
            {resetResult.deleted.map((line) => (
              <li key={line} className="text-green-700">
                Deleted: {line}
              </li>
            ))}
            {resetResult.failed.map((line) => (
              <li key={line} className="text-red-700">
                Failed: {line}
              </li>
            ))}
          </ul>
        )}
      </Card>
    </>
  );
}
