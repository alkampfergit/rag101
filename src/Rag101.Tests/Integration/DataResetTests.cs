using MongoDB.Bson;
using MongoDB.Driver;
using Rag101.Api.Data;
using Rag101.Api.Settings;

namespace Rag101.Tests.Integration;

[Collection("Integration")]
public class DataResetTests
{
    [MongoDbAndElasticsearchFact]
    public async Task Reset_deletes_data_and_indices_but_keeps_settings_and_foreign_indices()
    {
        var provider = TestEnvironment.DatabaseProvider();
        var database = provider.GetDatabase();
        var store = new SettingsStore(provider);
        var elasticsearchSettings = TestEnvironment.ElasticsearchSettings();
        var elasticsearch = ElasticsearchClientFactory.Create(elasticsearchSettings);

        const string ownedIndex = "rag101-reset-test";
        const string foreignIndex = "other-reset-test";

        // Arrange: settings, one Document, one index of ours, one that is not ours.
        await store.SaveAsync(new AppSettings { Elasticsearch = elasticsearchSettings });
        await database.GetCollection<BsonDocument>("documents").InsertOneAsync(new BsonDocument("title", "a book"));
        await elasticsearch.Indices.DeleteAsync(ownedIndex);
        await elasticsearch.Indices.DeleteAsync(foreignIndex);
        Assert.True((await elasticsearch.Indices.CreateAsync(ownedIndex)).IsValidResponse);
        Assert.True((await elasticsearch.Indices.CreateAsync(foreignIndex)).IsValidResponse);

        try
        {
            var result = await new DataReset(provider, store).ResetAsync();

            Assert.Empty(result.Failed);
            Assert.Contains("MongoDB collection documents", result.Deleted);
            Assert.Contains($"Elasticsearch index {ownedIndex}", result.Deleted);

            var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();
            Assert.DoesNotContain("documents", collections);
            Assert.Contains(SettingsStore.CollectionName, collections);
            Assert.Equal(elasticsearchSettings.Url, (await store.LoadAsync()).Elasticsearch.Url);

            Assert.False((await elasticsearch.Indices.ExistsAsync(ownedIndex)).Exists);
            Assert.True((await elasticsearch.Indices.ExistsAsync(foreignIndex)).Exists);
        }
        finally
        {
            await elasticsearch.Indices.DeleteAsync(foreignIndex);
        }
    }

    [MongoDbFact]
    public async Task Reset_without_elasticsearch_settings_still_deletes_the_collections()
    {
        var provider = TestEnvironment.DatabaseProvider();
        var database = provider.GetDatabase();
        var store = new SettingsStore(provider);
        await store.SaveAsync(new AppSettings());
        await database.GetCollection<BsonDocument>("documents").InsertOneAsync(new BsonDocument("title", "a book"));

        var result = await new DataReset(provider, store).ResetAsync();

        Assert.Empty(result.Failed);
        Assert.Equal(["MongoDB collection documents"], result.Deleted);
    }
}
