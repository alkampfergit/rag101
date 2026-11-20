using Rag101.Api.Settings;
using Rag101.Api.Status;

namespace Rag101.Tests.Integration;

[Collection("Integration")]
public class StatusCheckTests
{
    [MongoDbFact]
    public async Task MongoDb_is_reachable_and_reports_its_version()
    {
        var status = await MongoDbStatusCheck.CheckAsync(TestEnvironment.MongoDbConnection!);

        Assert.Equal(DependencyState.Reachable, status.State);
        Assert.StartsWith("MongoDB ", status.Detail);
    }

    [Fact]
    public async Task MongoDb_at_a_wrong_address_is_unreachable()
    {
        var status = await MongoDbStatusCheck.CheckAsync("mongodb://localhost:1");

        Assert.Equal(DependencyState.Unreachable, status.State);
        Assert.NotEqual("", status.Detail);
    }

    [Fact]
    public async Task An_invalid_connection_string_is_not_repeated_in_the_reason()
    {
        var status = await MongoDbStatusCheck.CheckAsync("not-a-connection-string-with-secret");

        Assert.Equal(DependencyState.Unreachable, status.State);
        Assert.DoesNotContain("secret", status.Detail);
    }

    [ElasticsearchFact]
    public async Task Elasticsearch_is_reachable_and_reports_its_version()
    {
        var status = await ElasticsearchStatusCheck.CheckAsync(TestEnvironment.ElasticsearchSettings());

        Assert.Equal(DependencyState.Reachable, status.State);
        Assert.StartsWith("Elasticsearch ", status.Detail);
    }

    [Fact]
    public async Task Elasticsearch_at_a_wrong_address_is_unreachable()
    {
        var settings = new ElasticsearchSettings { Url = "http://localhost:1", NoAuthentication = true };

        var status = await ElasticsearchStatusCheck.CheckAsync(settings);

        Assert.Equal(DependencyState.Unreachable, status.State);
        Assert.NotEqual("", status.Detail);
    }

    [Fact]
    public async Task Elasticsearch_without_a_url_is_not_configured()
    {
        var status = await ElasticsearchStatusCheck.CheckAsync(new ElasticsearchSettings());

        Assert.Equal(DependencyState.NotConfigured, status.State);
    }
}
