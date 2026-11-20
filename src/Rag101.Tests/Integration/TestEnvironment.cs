using MongoDB.Driver;
using Rag101.Api.Bootstrap;
using Rag101.Api.Data;
using Rag101.Api.Settings;

namespace Rag101.Tests.Integration;

// Integration tests run against a real MongoDB and a real Elasticsearch.
// They find them through environment variables and are skipped, not failed,
// when a variable is missing.

public static class TestEnvironment
{
    public const string DatabaseName = "rag101-tests";

    public static string? MongoDbConnection =>
        Environment.GetEnvironmentVariable(BootstrapConnection.EnvironmentVariable);

    public static string? ElasticsearchUrl =>
        Environment.GetEnvironmentVariable("RAG_TEST_ELASTICSEARCH_URL");

    /// <summary>A provider for the test database, never the real one.</summary>
    public static MongoDatabaseProvider DatabaseProvider()
    {
        var url = new MongoUrlBuilder(MongoDbConnection);

        // Naming a database in a connection string can also change where the
        // user is looked up. So the database the driver authenticates against
        // with the original string is stated before the name is replaced.
        // With mongodb+srv it may come from DNS, which is why the driver is asked.
        var credential = MongoClientSettings.FromConnectionString(MongoDbConnection).Credential;
        if (credential is not null)
        {
            url.AuthenticationSource = credential.Source;
        }
        url.DatabaseName = DatabaseName;

        return new MongoDatabaseProvider(new BootstrapConnection(new UnsupportedCredentialStore(), url.ToString()));
    }

    public static ElasticsearchSettings ElasticsearchSettings()
    {
        var user = Environment.GetEnvironmentVariable("RAG_TEST_ELASTICSEARCH_USER");
        return new ElasticsearchSettings
        {
            Url = ElasticsearchUrl!,
            User = user ?? "",
            Password = Environment.GetEnvironmentVariable("RAG_TEST_ELASTICSEARCH_PASSWORD") ?? "",
            NoAuthentication = user is null,
            AcceptSelfSignedCertificate = true,
        };
    }
}

/// <summary>A test that needs MongoDB.</summary>
public class MongoDbFactAttribute : FactAttribute
{
    public MongoDbFactAttribute()
    {
        if (TestEnvironment.MongoDbConnection is null)
        {
            Skip = $"Set {BootstrapConnection.EnvironmentVariable} to run this test.";
        }
    }
}

/// <summary>A test that needs Elasticsearch.</summary>
public class ElasticsearchFactAttribute : FactAttribute
{
    public ElasticsearchFactAttribute()
    {
        if (TestEnvironment.ElasticsearchUrl is null)
        {
            Skip = "Set RAG_TEST_ELASTICSEARCH_URL to run this test.";
        }
    }
}

/// <summary>A test that needs both.</summary>
public class MongoDbAndElasticsearchFactAttribute : FactAttribute
{
    public MongoDbAndElasticsearchFactAttribute()
    {
        if (TestEnvironment.MongoDbConnection is null || TestEnvironment.ElasticsearchUrl is null)
        {
            Skip = $"Set {BootstrapConnection.EnvironmentVariable} and RAG_TEST_ELASTICSEARCH_URL to run this test.";
        }
    }
}
