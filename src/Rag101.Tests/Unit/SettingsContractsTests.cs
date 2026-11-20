using System.Text.Json;
using Rag101.Api.Settings;

namespace Rag101.Tests.Unit;

public class SettingsContractsTests
{
    private static AppSettings StoredSettings() => new()
    {
        Elasticsearch = new ElasticsearchSettings { Url = "https://es:9200", User = "elastic", Password = "es-secret" },
        LanguageModel = new LanguageModelSettings { Model = "a-model", ApiKey = "llm-secret" },
    };

    private static SettingsUpdate Update(string? password, string? apiKey) => new(
        new ElasticsearchUpdate("https://es:9200", "elastic", password, false, false),
        new LanguageModelUpdate(LanguageModelProvider.OpenAi, "", apiKey, "a-model"));

    [Fact]
    public void A_null_secret_keeps_the_stored_value()
    {
        var result = Update(password: null, apiKey: null).ApplyTo(StoredSettings());

        Assert.Equal("es-secret", result.Elasticsearch.Password);
        Assert.Equal("llm-secret", result.LanguageModel.ApiKey);
    }

    [Fact]
    public void An_empty_secret_clears_the_stored_value()
    {
        var result = Update(password: "", apiKey: "").ApplyTo(StoredSettings());

        Assert.Equal("", result.Elasticsearch.Password);
        Assert.Equal("", result.LanguageModel.ApiKey);
    }

    [Fact]
    public void A_new_secret_replaces_the_stored_value()
    {
        var result = Update(password: "new-es", apiKey: "new-llm").ApplyTo(StoredSettings());

        Assert.Equal("new-es", result.Elasticsearch.Password);
        Assert.Equal("new-llm", result.LanguageModel.ApiKey);
    }

    [Fact]
    public void The_response_says_a_secret_exists_without_containing_it()
    {
        var response = SettingsResponse.From(StoredSettings());

        Assert.True(response.Elasticsearch.HasPassword);
        Assert.True(response.LanguageModel.HasApiKey);

        var json = JsonSerializer.Serialize(response);
        Assert.DoesNotContain("es-secret", json);
        Assert.DoesNotContain("llm-secret", json);
    }

    [Theory]
    [InlineData(LanguageModelProvider.OpenAi, "", "a-model", true)]
    [InlineData(LanguageModelProvider.OpenAi, "", "", false)]
    [InlineData(LanguageModelProvider.OpenAiCompatible, "", "a-model", false)]
    [InlineData(LanguageModelProvider.OpenAiCompatible, "http://localhost:1234/v1", "a-model", true)]
    [InlineData(LanguageModelProvider.AzureAiInference, "", "a-model", false)]
    public void Language_model_is_configured_only_with_what_its_provider_needs(
        LanguageModelProvider provider, string endpoint, string model, bool expected)
    {
        var settings = new LanguageModelSettings { Provider = provider, Endpoint = endpoint, Model = model };

        Assert.Equal(expected, settings.IsConfigured);
    }
}
