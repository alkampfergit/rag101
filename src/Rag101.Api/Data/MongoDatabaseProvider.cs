using MongoDB.Driver;
using Rag101.Api.Bootstrap;

namespace Rag101.Api.Data;

/// <summary>
/// Gives access to the application's MongoDB database, connecting with the
/// Bootstrap connection. When the connection string changes (the first-run
/// page saved one), the next call connects with the new value.
/// </summary>
public class MongoDatabaseProvider(BootstrapConnection bootstrapConnection)
{
    public const string DefaultDatabaseName = "rag101";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private MongoClient? _client;
    private string? _clientConnectionString;

    public IMongoDatabase GetDatabase()
    {
        var connectionString = bootstrapConnection.Value
            ?? throw new InvalidOperationException("The MongoDB connection is not configured.");

        if (_client is null || _clientConnectionString != connectionString)
        {
            _client = CreateClient(connectionString);
            _clientConnectionString = connectionString;
        }

        // The database named in the connection string, or "rag101".
        var databaseName = new MongoUrl(connectionString).DatabaseName ?? DefaultDatabaseName;
        return _client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Creates a client that gives up after 5 seconds when the server cannot
    /// be reached. The driver's default is 30 seconds, too long for a status page.
    /// </summary>
    public static MongoClient CreateClient(string connectionString)
    {
        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.ServerSelectionTimeout = Timeout;
        settings.ConnectTimeout = Timeout;
        return new MongoClient(settings);
    }
}
