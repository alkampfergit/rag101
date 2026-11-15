using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Rag101.Api.Settings;

namespace Rag101.Api.Data;

public static class ElasticsearchClientFactory
{
    /// <summary>
    /// Creates an Elasticsearch client from the stored settings. A new client
    /// is created each time one is needed, so a change in the settings is
    /// picked up immediately.
    /// </summary>
    public static ElasticsearchClient Create(ElasticsearchSettings settings)
    {
        var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Url))
            .RequestTimeout(TimeSpan.FromSeconds(5));

        if (!settings.NoAuthentication)
        {
            clientSettings = clientSettings.Authentication(
                new BasicAuthentication(settings.User, settings.Password));
        }

        if (settings.AcceptSelfSignedCertificate)
        {
            // Accept any certificate the server presents.
            clientSettings = clientSettings.ServerCertificateValidationCallback(
                (_, _, _, _) => true);
        }

        return new ElasticsearchClient(clientSettings);
    }
}
