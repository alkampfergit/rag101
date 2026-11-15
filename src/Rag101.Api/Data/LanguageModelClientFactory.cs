using Cisharpai;
using Cisharpai.Azure.AzureAiInference;
using Cisharpai.OpenAi;
using Rag101.Api.Settings;

namespace Rag101.Api.Data;

/// <summary>
/// Creates a chat client for the language model chosen in the settings.
/// Cisharpai gives the same IChatCompletionClient interface for every
/// provider; only the configuration differs.
/// </summary>
public class LanguageModelClientFactory(ICisharpaiClientFactory cisharpaiFactory)
{
    public CisharpaiClientFactoryResult<IChatCompletionClient> Create(LanguageModelSettings settings)
    {
        CisharpaiClientConfiguration configuration = settings.Provider switch
        {
            LanguageModelProvider.OpenAi => new OpenAiClientConfiguration
            {
                ApiKey = settings.ApiKey,
                DefaultModel = settings.Model,
            },

            // Same protocol as OpenAI, at a different address: LiteLLM,
            // llama.cpp, LM Studio. Local servers often need no key, but the
            // library requires one, so we send a placeholder.
            LanguageModelProvider.OpenAiCompatible => new OpenAiClientConfiguration
            {
                ApiKey = settings.ApiKey == "" ? "none" : settings.ApiKey,
                BaseUrl = settings.Endpoint,
                DefaultModel = settings.Model,
            },

            LanguageModelProvider.AzureAiInference => new AzureAiInferenceClientConfiguration
            {
                ApiKey = settings.ApiKey,
                Endpoint = settings.Endpoint,
                ModelId = settings.Model,
            },

            _ => throw new ArgumentOutOfRangeException(nameof(settings)),
        };

        return cisharpaiFactory.CreateChatCompletionClient(configuration);
    }
}
