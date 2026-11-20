using Rag101.Api.Settings;

namespace Rag101.Tests.Integration;

[Collection("Integration")]
public class SettingsStoreTests
{
    private static async Task<SettingsStore> EmptyStoreAsync()
    {
        var provider = TestEnvironment.DatabaseProvider();
        await provider.GetDatabase().DropCollectionAsync(SettingsStore.CollectionName);
        return new SettingsStore(provider);
    }

    [MongoDbFact]
    public async Task Loading_before_anything_is_saved_returns_empty_settings()
    {
        var store = await EmptyStoreAsync();

        var settings = await store.LoadAsync();

        Assert.False(settings.Elasticsearch.IsConfigured);
        Assert.False(settings.LanguageModel.IsConfigured);
    }

    [MongoDbFact]
    public async Task Saved_settings_are_loaded_back()
    {
        var store = await EmptyStoreAsync();

        await store.SaveAsync(new AppSettings
        {
            Elasticsearch = new ElasticsearchSettings
            {
                Url = "https://es:9200", User = "elastic", Password = "es-secret", AcceptSelfSignedCertificate = true,
            },
            LanguageModel = new LanguageModelSettings
            {
                Provider = LanguageModelProvider.OpenAiCompatible,
                Endpoint = "http://localhost:1234/v1",
                ApiKey = "llm-secret",
                Model = "local-model",
            },
        });
        var loaded = await store.LoadAsync();

        Assert.Equal("https://es:9200", loaded.Elasticsearch.Url);
        Assert.Equal("es-secret", loaded.Elasticsearch.Password);
        Assert.True(loaded.Elasticsearch.AcceptSelfSignedCertificate);
        Assert.Equal(LanguageModelProvider.OpenAiCompatible, loaded.LanguageModel.Provider);
        Assert.Equal("local-model", loaded.LanguageModel.Model);
    }

    [MongoDbFact]
    public async Task Saving_again_without_secrets_keeps_them()
    {
        var store = await EmptyStoreAsync();
        await store.SaveAsync(new AppSettings
        {
            Elasticsearch = new ElasticsearchSettings { Url = "https://es:9200", Password = "es-secret" },
            LanguageModel = new LanguageModelSettings { Model = "a-model", ApiKey = "llm-secret" },
        });

        // What the Settings page sends when the user did not retype the secrets.
        var update = new SettingsUpdate(
            new ElasticsearchUpdate("https://other:9200", "", Password: null, false, false),
            new LanguageModelUpdate(LanguageModelProvider.OpenAi, "", ApiKey: null, "a-model"));
        await store.SaveAsync(update.ApplyTo(await store.LoadAsync()));
        var loaded = await store.LoadAsync();

        Assert.Equal("https://other:9200", loaded.Elasticsearch.Url);
        Assert.Equal("es-secret", loaded.Elasticsearch.Password);
        Assert.Equal("llm-secret", loaded.LanguageModel.ApiKey);
    }
}
