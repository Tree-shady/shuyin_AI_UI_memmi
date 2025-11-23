// Services/AiServiceFactory.cs
using AIChatAssistant.Models;
using System;

namespace AIChatAssistant.Services;

public static class AiServiceFactory
{
    private static IProviderConfigService _providerConfigService = new ProviderConfigService();
    
    // 设置ProviderConfigService，便于测试和依赖注入
    public static void SetProviderConfigService(IProviderConfigService service)
    {
        _providerConfigService = service;
    }
    
    public static IAiService CreateAiService(ApiConfig config)
    {
        // 默认创建逻辑
        return CreateServiceForProvider(config);
    }
    
    // 通过供应商ID创建AI服务
    public static IAiService CreateAiService(string providerId, string apiKey)
    {
        try
        {
            var customProvider = _providerConfigService.GetProviderById(providerId);
            if (customProvider != null)
            {
                // 使用自定义供应商配置创建服务
                var customConfig = new ApiConfig(customProvider)
                {
                    ApiKey = apiKey
                };
                return CreateServiceForProvider(customConfig);
            }
        }
        catch (Exception ex)
        {
            // 如果自定义供应商配置出错，使用默认逻辑
            Console.WriteLine($"Error loading custom provider: {ex.Message}");
        }
        
        // 如果找不到自定义供应商，使用默认的OpenAI配置
        return CreateServiceForProvider(new ApiConfig { ApiKey = apiKey });
    }
    
    private static IAiService CreateServiceForProvider(ApiConfig config)
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