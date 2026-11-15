namespace Rag101.Api.Bootstrap;

/// <summary>
/// The operating system's place for secrets: the Keychain on macOS,
/// the Credential Manager on Windows. It holds exactly one value for us,
/// the MongoDB connection string.
/// </summary>
public interface ICredentialStore
{
    /// <summary>False on operating systems where we have no store (Linux).</summary>
    bool IsSupported { get; }

    /// <summary>Returns the stored connection string, or null when nothing is stored.</summary>
    string? Read();

    void Write(string connectionString);
}
