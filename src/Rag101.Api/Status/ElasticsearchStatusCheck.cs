using Rag101.Api.Data;
using Rag101.Api.Settings;

namespace Rag101.Api.Status;

public static class ElasticsearchStatusCheck
{
    /// <summary>Asks Elasticsearch for its version, the simplest request there is.</summary>
    public static async Task<DependencyStatus> CheckAsync(ElasticsearchSettings settings)
    {
        if (!settings.IsConfigured)
        {
            return DependencyStatus.NotConfigured();
        }

        try
        {
            var client = ElasticsearchClientFactory.Create(settings);
            var info = await client.InfoAsync();

            if (info.IsValidResponse)
            {
                return DependencyStatus.Reachable($"Elasticsearch {info.Version.Number}");
            }

            // Either Elasticsearch answered with an error (wrong password),
            // or the request never arrived (wrong address, certificate refused).
            var reason = info.ElasticsearchServerError?.Error?.Reason
                ?? info.ApiCallDetails?.OriginalException?.Message
                ?? $"Elasticsearch answered with HTTP status {info.ApiCallDetails?.HttpStatusCode}.";
            return DependencyStatus.Unreachable(reason);
        }
        catch (Exception exception)
        {
            return DependencyStatus.Unreachable(exception.Message);
        }
    }
}
