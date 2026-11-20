using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Rag101.Api.Settings;

/// <summary>
/// Every setting except the Bootstrap connection. Stored as a single
/// document in the "settings" collection of MongoDB.
/// </summary>
public class AppSettings
{
    public const string DocumentId = "settings";

    [BsonId]
    public string Id { get; set; } = DocumentId;

    public ElasticsearchSettings Elasticsearch { get; set; } = new();

    public LanguageModelSettings LanguageModel { get; set; } = new();
}

public class ElasticsearchSettings
{
    public string Url { get; set; } = "";

    public string User { get; set; } = "";

    /// <summary>Stored in plain text: this is a course application.</summary>
    public string Password { get; set; } = "";

    public bool NoAuthentication { get; set; }

    /// <summary>
    /// Elasticsearch turns on TLS with a self-signed certificate by default,
    /// which a client normally refuses.
    /// </summary>
    public bool AcceptSelfSignedCertificate { get; set; }

    public bool IsConfigured => Url != "";
}

public enum LanguageModelProvider
{
    OpenAi,
    AzureAiInference,
    OpenAiCompatible,
}

public class LanguageModelSettings
{
    [BsonRepresentation(BsonType.String)]
    public LanguageModelProvider Provider { get; set; } = LanguageModelProvider.OpenAi;

    /// <summary>Not used for OpenAi, which has a well-known address.</summary>
    public string Endpoint { get; set; } = "";

    /// <summary>Stored in plain text: this is a course application.</summary>
    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "";

    public bool IsConfigured =>
        Model != "" && (Provider == LanguageModelProvider.OpenAi || Endpoint != "");
}
