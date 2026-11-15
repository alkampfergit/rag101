namespace Rag101.Api.Bootstrap;

/// <summary>
/// Used on Linux, where the only supported source for the connection string
/// is the RAG_MONGODB_CONNECTION environment variable.
/// </summary>
public class UnsupportedCredentialStore : ICredentialStore
{
    public bool IsSupported => false;

    public string? Read() => null;

    public void Write(string connectionString) =>
        throw new NotSupportedException("This operating system has no supported credential store.");
}
