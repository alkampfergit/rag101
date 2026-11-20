using Cisharpai;
using Cisharpai.Models;
using Rag101.Api.Data;
using Rag101.Api.Settings;

namespace Rag101.Api.Status;

/// <summary>
/// Checks the language model by sending it one very small request. Unlike the
/// other checks this one costs something, so it runs only when asked.
/// </summary>
public class LanguageModelStatusCheck(LanguageModelClientFactory clientFactory)
{
    public async Task<DependencyStatus> CheckAsync(LanguageModelSettings settings)
    {
        if (!settings.IsConfigured)
        {
            return DependencyStatus.NotConfigured();
        }

        var result = clientFactory.Create(settings);
        if (!result.IsSuccess)
        {
            return DependencyStatus.Unreachable(result.ErrorMessage ?? "The client could not be created.");
        }

        return await CheckAsync(result.Client!, settings.Model);
    }

    public static async Task<DependencyStatus> CheckAsync(IChatCompletionClient client, string model)
    {
        var request = new ChatCompletionRequest(
            Messages: [new LlmMessage(LlmRole.User, "Reply with the single word: ready")],
            Model: model,
            MaxTokens: 50);

        try
        {
            // Cisharpai does not throw for errors returned by the provider:
            // it reports them in the response.
            var response = await client.GetChatCompletionAsync(request);

            return response.IsSuccess
                ? DependencyStatus.Reachable($"{response.Model} replied: {response.Content.Trim()}")
                : DependencyStatus.Unreachable(response.ErrorMessage ?? "The language model returned an error.");
        }
        catch (Exception exception)
        {
            // Network failures (wrong address, server not running) do throw.
            return DependencyStatus.Unreachable(exception.Message);
        }
    }
}
