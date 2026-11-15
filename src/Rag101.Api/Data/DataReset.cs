using Rag101.Api.Settings;

namespace Rag101.Api.Data;

public record ResetResult(List<string> Deleted, List<string> Failed);

/// <summary>
/// "Reset data": deletes everything the application stored except its
/// settings. It deletes only what the application created, which it
/// recognises by name.
/// </summary>
public class DataReset(MongoDatabaseProvider databaseProvider, SettingsStore settingsStore)
{
    /// <summary>
    /// The MongoDB collections that hold data. The "settings" collection is
    /// deliberately not in this list. Later Lessons add to it.
    /// </summary>
    public static readonly string[] DataCollections = ["documents"];

    /// <summary>Every search index the application creates starts with this.</summary>
    public const string IndexPrefix = "rag101-";

    public async Task<ResetResult> ResetAsync()
    {
        var result = new ResetResult([], []);

        // The settings are read before anything is deleted: they say where
        // Elasticsearch is.
        var settings = await settingsStore.LoadAsync();

        await DeleteCollectionsAsync(result);
        await DeleteIndicesAsync(settings.Elasticsearch, result);

        return result;
    }

    private async Task DeleteCollectionsAsync(ResetResult result)
    {
        foreach (var collection in DataCollections)
        {
            try
            {
                await databaseProvider.GetDatabase().DropCollectionAsync(collection);
                result.Deleted.Add($"MongoDB collection {collection}");
            }
            catch (Exception exception)
            {
                result.Failed.Add($"MongoDB collection {collection}: {exception.Message}");
            }
        }
    }

    private static async Task DeleteIndicesAsync(ElasticsearchSettings settings, ResetResult result)
    {
        if (!settings.IsConfigured)
        {
            return;
        }

        try
        {
            var client = ElasticsearchClientFactory.Create(settings);

            // Elasticsearch refuses by default to delete indices by wildcard,
            // so we list the matching indices and delete them by name.
            var indices = await client.Indices.GetAsync($"{IndexPrefix}*");
            if (!indices.IsValidResponse)
            {
                result.Failed.Add($"Elasticsearch indices: {indices.ApiCallDetails?.OriginalException?.Message
                    ?? indices.ElasticsearchServerError?.Error?.Reason
                    ?? "the list of indices could not be read."}");
                return;
            }

            foreach (var indexName in indices.Indices.Keys)
            {
                var deletion = await client.Indices.DeleteAsync(indexName);
                if (deletion.IsValidResponse)
                {
                    result.Deleted.Add($"Elasticsearch index {indexName}");
                }
                else
                {
                    result.Failed.Add($"Elasticsearch index {indexName}: {deletion.ElasticsearchServerError?.Error?.Reason}");
                }
            }
        }
        catch (Exception exception)
        {
            result.Failed.Add($"Elasticsearch indices: {exception.Message}");
        }
    }
}
