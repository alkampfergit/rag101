import { useEffect, useState } from "react";
import { type BootstrapInfo, getBootstrap } from "./api";
import FirstRunPage from "./FirstRunPage";
import SettingsPage from "./SettingsPage";

// The application has two pages. Which one is shown depends on a single
// question to the API: do you have a Bootstrap connection?
export default function App() {
  const [bootstrap, setBootstrap] = useState<BootstrapInfo | null>(null);
  const [error, setError] = useState("");

  const load = () =>
    getBootstrap()
      .then(setBootstrap)
      .catch((e: Error) => setError(e.message));

  useEffect(() => {
    load();
  }, []);

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto max-w-3xl px-4 py-4">
          <h1 className="text-xl font-semibold">RAG 101</h1>
        </div>
      </header>
      <main className="mx-auto max-w-3xl px-4 py-6">
        {error && <p className="text-red-700">{error}</p>}
        {bootstrap && !bootstrap.configured && (
          <FirstRunPage bootstrap={bootstrap} onSaved={load} />
        )}
        {bootstrap && bootstrap.configured && <SettingsPage bootstrap={bootstrap} />}
      </main>
    </div>
  );
}
