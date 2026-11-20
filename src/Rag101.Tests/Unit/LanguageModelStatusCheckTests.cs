using Cisharpai;
using Cisharpai.Models;
using Cisharpai.Testing;
using Microsoft.Extensions.DependencyInjection;
using Rag101.Api.Data;
using Rag101.Api.Settings;
using Rag101.Api.Status;

namespace Rag101.Tests.Unit;

// The language model is always a fake in tests: a real one is slow,
// costs money and does not give the same reply twice.
public class LanguageModelStatusCheckTests
{
    [Fact]
    public async Task The_reply_of_the_model_becomes_the_detail()
    {
        var client = new FakeChatCompletionClient
        {
            DefaultResponse = new ChatCompletionResponse("ready", "fake-model", 5, 1),
        };

        var status = await LanguageModelStatusCheck.CheckAsync(client, "fake-model");

        Assert.Equal(DependencyState.Reachable, status.State);
        Assert.Contains("ready", status.Detail);
    }

    [Fact]
    public async Task An_error_from_the_provider_means_unreachable()
    {
        var client = new FakeChatCompletionClient
        {
            DefaultResponse = ChatCompletionResponse.Error("401 invalid api key"),
        };

        var status = await LanguageModelStatusCheck.CheckAsync(client, "fake-model");

        Assert.Equal(DependencyState.Unreachable, status.State);
        Assert.Equal("401 invalid api key", status.Detail);
    }

    [Fact]
    public async Task Empty_settings_mean_not_configured_and_no_request_is_sent()
    {
        var services = new ServiceCollection();
        services.AddCisharpaiClientFactory();
        var cisharpaiFactory = services.BuildServiceProvider().GetRequiredService<ICisharpaiClientFactory>();
        var check = new LanguageModelStatusCheck(new LanguageModelClientFactory(cisharpaiFactory));

        var status = await check.CheckAsync(new LanguageModelSettings());

        Assert.Equal(DependencyState.NotConfigured, status.State);
    }
}
