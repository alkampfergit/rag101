namespace Rag101.Api.Bootstrap;

public enum BootstrapSource
{
    Environment,
    CredentialStore,
}

/// <summary>
/// The Bootstrap connection is the MongoDB connection string: the one setting
/// the application needs before it can read any other, because all the other
/// settings are stored in MongoDB.
///
/// It is looked for in this order:
///   1. the RAG_MONGODB_CONNECTION environment variable
///   2. the operating system's credential store
/// When neither has it, the application is "not configured".
/// </summary>
public class BootstrapConnection
{
    public const string EnvironmentVariable = "RAG_MONGODB_CONNECTION";

    private readonly ICredentialStore _credentialStore;

    public BootstrapConnection(ICredentialStore credentialStore, string? environmentValue)
    {
        _credentialStore = credentialStore;

        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            Value = environmentValue;
            Source = BootstrapSource.Environment;
            return;
        }

        var stored = credentialStore.IsSupported ? credentialStore.Read() : null;
        if (!string.IsNullOrWhiteSpace(stored))
        {
            Value = stored;
            Source = BootstrapSource.CredentialStore;
        }
    }

    /// <summary>The connection string, or null when the application is not configured.</summary>
    public string? Value { get; private set; }

    public BootstrapSource? Source { get; private set; }

    public bool IsConfigured => Value is not null;

    /// <summary>
    /// A value that comes from the environment variable cannot be replaced
    /// from the UI, and on Linux there is nowhere to save one.
    /// </summary>
    public bool CanSave => Source != BootstrapSource.Environment && _credentialStore.IsSupported;

    /// <summary>
    /// Saves a new connection string in the credential store and starts using
    /// it immediately: no restart is needed.
    /// </summary>
    public void Save(string connectionString)
    {
        if (!CanSave)
        {
            throw new InvalidOperationException("The MongoDB connection cannot be saved from the application.");
        }

        _credentialStore.Write(connectionString);
        Value = connectionString;
        Source = BootstrapSource.CredentialStore;
    }
}
