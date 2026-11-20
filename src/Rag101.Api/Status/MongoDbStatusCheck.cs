using MongoDB.Bson;
using MongoDB.Driver;
using Rag101.Api.Data;

namespace Rag101.Api.Status;

public static class MongoDbStatusCheck
{
    /// <summary>
    /// Connects with the given connection string and asks the server for its
    /// version. Takes the connection string as a parameter so that the
    /// first-run page can test one before it is saved.
    /// </summary>
    public static async Task<DependencyStatus> CheckAsync(string connectionString)
    {
        try
        {
            using var client = MongoDatabaseProvider.CreateClient(connectionString);
            var buildInfo = await client.GetDatabase("admin")
                .RunCommandAsync<BsonDocument>(new BsonDocument("buildInfo", 1));

            return DependencyStatus.Reachable($"MongoDB {buildInfo["version"].AsString}");
        }
        catch (MongoConfigurationException)
        {
            // The driver's message repeats the connection string, password
            // included, so we do not pass it on.
            return DependencyStatus.Unreachable("The connection string is not valid.");
        }
        catch (TimeoutException)
        {
            // The driver's message is a long dump of its internal state.
            // Server names are safe to show: they carry no credentials.
            var servers = string.Join(", ", new MongoUrl(connectionString).Servers);
            return DependencyStatus.Unreachable($"No answer from MongoDB at {servers} within 5 seconds.");
        }
        catch (Exception exception)
        {
            return DependencyStatus.Unreachable(exception.Message);
        }
    }
}
