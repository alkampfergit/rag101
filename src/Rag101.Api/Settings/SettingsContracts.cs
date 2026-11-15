namespace Rag101.Api.Settings;

// What travels between the browser and the API. These are separate from
// AppSettings for one reason: secrets must never be sent to the browser.

/// <summary>What the browser receives: "has" flags in place of secrets.</summary>
public record SettingsResponse(ElasticsearchResponse Elasticsearch, LanguageModelResponse LanguageModel)
{
    public static SettingsResponse From(AppSettings settings) => new(
        new ElasticsearchResponse(
            settings.Elasticsearch.Url,
            settings.Elasticsearch.User,
            HasPassword: settings.Elasticsearch.Password != "",
            settings.Elasticsearch.NoAuthentication,
            settings.Elasticsearch.AcceptSelfSignedCertificate),
        new LanguageModelResponse(
            settings.LanguageModel.Provider,
            settings.LanguageModel.Endpoint,
            HasApiKey: settings.LanguageModel.ApiKey != "",
            settings.LanguageModel.Model));
}

public record ElasticsearchResponse(
    string Url,
    string User,
    bool HasPassword,
    bool NoAuthentication,
    bool AcceptSelfSignedCertificate);

public record LanguageModelResponse(
    LanguageModelProvider Provider,
    string Endpoint,
    bool HasApiKey,
    string Model);

/// <summary>
/// What the browser sends. A secret that is null means "keep the one that is
/// stored"; an empty string means "clear it".
/// </summary>
public record SettingsUpdate(ElasticsearchUpdate Elasticsearch, LanguageModelUpdate LanguageModel)
{
    /// <summary>Builds the settings to store, starting from the ones stored now.</summary>
    public AppSettings ApplyTo(AppSettings stored) => new()
    {
        Elasticsearch = new ElasticsearchSettings
        {
            Url = Elasticsearch.Url.Trim(),
            User = Elasticsearch.User.Trim(),
            Password = Elasticsearch.Password ?? stored.Elasticsearch.Password,
            NoAuthentication = Elasticsearch.NoAuthentication,
            AcceptSelfSignedCertificate = Elasticsearch.AcceptSelfSignedCertificate,
        },
        LanguageModel = new LanguageModelSettings
        {
            Provider = LanguageModel.Provider,
            Endpoint = LanguageModel.Endpoint.Trim(),
            ApiKey = LanguageModel.ApiKey ?? stored.LanguageModel.ApiKey,
            Model = LanguageModel.Model.Trim(),
        },
    };
}

public record ElasticsearchUpdate(
    string Url,
    string User,
    string? Password,
    bool NoAuthentication,
    bool AcceptSelfSignedCertificate);

public record LanguageModelUpdate(
    LanguageModelProvider Provider,
    string Endpoint,
    string? ApiKey,
    string Model);
