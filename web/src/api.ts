// Typed calls to the API. Every URL is relative: the API that answers
// is the same server that served this page.

export type StatusState = "Reachable" | "Unreachable" | "NotConfigured";

export interface DependencyStatus {
  state: StatusState;
  detail: string;
}

export interface BootstrapInfo {
  configured: boolean;
  source: "Environment" | "CredentialStore" | null;
  canSave: boolean;
}

export type LanguageModelProvider = "OpenAi" | "AzureAiInference" | "OpenAiCompatible";

// What the API returns: secrets are replaced by "has" flags.
export interface Settings {
  elasticsearch: {
    url: string;
    user: string;
    hasPassword: boolean;
    noAuthentication: boolean;
    acceptSelfSignedCertificate: boolean;
  };
  languageModel: {
    provider: LanguageModelProvider;
    endpoint: string;
    hasApiKey: boolean;
    model: string;
  };
}

// What the API accepts: a secret that is null keeps the stored value.
export interface SettingsUpdate {
  elasticsearch: {
    url: string;
    user: string;
    password: string | null;
    noAuthentication: boolean;
    acceptSelfSignedCertificate: boolean;
  };
  languageModel: {
    provider: LanguageModelProvider;
    endpoint: string;
    apiKey: string | null;
    model: string;
  };
}

export interface Status {
  mongoDb: DependencyStatus;
  elasticsearch: DependencyStatus;
}

export interface ResetResult {
  deleted: string[];
  failed: string[];
}

async function send<T>(method: string, url: string, body?: unknown): Promise<T> {
  const response = await fetch(url, {
    method,
    headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new Error(problem?.error ?? `The server answered ${response.status}.`);
  }

  return response.status === 204 ? (undefined as T) : await response.json();
}

export const getBootstrap = () => send<BootstrapInfo>("GET", "/api/bootstrap");

export const testBootstrap = (connectionString: string) =>
  send<DependencyStatus>("POST", "/api/bootstrap/test", { connectionString });

export const saveBootstrap = (connectionString: string) =>
  send<void>("PUT", "/api/bootstrap", { connectionString });

export const getSettings = () => send<Settings>("GET", "/api/settings");

export const saveSettings = (settings: SettingsUpdate) =>
  send<void>("PUT", "/api/settings", settings);

export const getStatus = () => send<Status>("GET", "/api/status");

export const testLanguageModel = () =>
  send<DependencyStatus>("POST", "/api/status/language-model");

export const resetData = () => send<ResetResult>("POST", "/api/reset-data");
