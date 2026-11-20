import type { ReactNode } from "react";
import type { DependencyStatus } from "./api";

// The few visual building blocks shared by the two pages.

export const inputClass =
  "w-full rounded border border-slate-300 bg-white px-3 py-2 text-sm disabled:bg-slate-100 disabled:text-slate-400";

export const buttonClass =
  "rounded bg-slate-800 px-4 py-2 text-sm font-medium text-white hover:bg-slate-700 disabled:opacity-50";

export const secondaryButtonClass =
  "rounded border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-100 disabled:opacity-50";

export function Card({ title, children }: { title: string; children: ReactNode }) {
  return (
    <section className="mb-6 rounded-lg border border-slate-200 bg-white p-5">
      <h2 className="mb-4 text-lg font-semibold">{title}</h2>
      {children}
    </section>
  );
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="mb-3 block">
      <span className="mb-1 block text-sm font-medium text-slate-700">{label}</span>
      {children}
    </label>
  );
}

const statusText = {
  Reachable: "Reachable",
  Unreachable: "Unreachable",
  NotConfigured: "Not configured",
};

const statusColor = {
  Reachable: "bg-green-100 text-green-800",
  Unreachable: "bg-red-100 text-red-800",
  NotConfigured: "bg-slate-100 text-slate-600",
};

export function StatusLine({ name, status }: { name: string; status: DependencyStatus | null }) {
  return (
    <div className="flex items-start gap-3 py-2">
      <span className="w-36 shrink-0 text-sm font-medium">{name}</span>
      {status === null ? (
        <span className="text-sm text-slate-500">Checking…</span>
      ) : (
        <>
          <span className={`shrink-0 rounded px-2 py-0.5 text-xs font-medium ${statusColor[status.state]}`}>
            {statusText[status.state]}
          </span>
          <span className="break-all text-sm text-slate-600">{status.detail}</span>
        </>
      )}
    </div>
  );
}
