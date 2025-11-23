// Services/AiServiceFactory.cs
using AIChatAssistant.Models;

namespace AIChatAssistant.Services;

public static class AiServiceFactory
{
    public static IAiService CreateAiService(ApiConfig config)
    {
        return config.Provider switch
        {
            AiProvider.OpenAI => new OpenAiService(config),
            AiProvider.CloudAPI => new CloudApiService(config),
            AiProvider.AzureOpenAI => new AzureOpenAiService(config),
            AiProvider.Claude => new ClaudeService(config),
            AiProvider.Gemini => new GeminiService(config),
            _ => new OpenAiService(config) // 默认为 OpenAI
        };
    }
}