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
        DebugService.Instance.LogInfo("AiServiceFactory", $"创建AI服务: {config.Provider}");
        return CreateServiceForProvider(config);
    }
    
    // 通过供应商ID创建AI服务
    public static IAiService CreateAiService(string providerId, string apiKey)
    {
        DebugService.Instance.LogDebug("AiServiceFactory", $"通过ID创建AI服务: ProviderId={providerId}");
        try
        {
            var customProvider = _providerConfigService.GetProviderById(providerId);
            if (customProvider != null)
            {
                DebugService.Instance.LogInfo("AiServiceFactory", $"找到自定义供应商: {customProvider.Name}");
                // 使用自定义供应商配置创建服务
                var customConfig = new ApiConfig(customProvider)
                {
                    ApiKey = "***" // 不要记录实际的API密钥
                };
                return CreateServiceForProvider(customConfig);
            }
            DebugService.Instance.LogWarning("AiServiceFactory", $"未找到自定义供应商: {providerId}");
        }
        catch (Exception ex)
        {
            // 如果自定义供应商配置出错，使用默认逻辑
            DebugService.Instance.LogException("AiServiceFactory", "加载自定义供应商失败", ex);
        }
        
        // 如果找不到自定义供应商，使用默认的OpenAI配置
        DebugService.Instance.LogInfo("AiServiceFactory", "使用默认OpenAI配置");
        return CreateServiceForProvider(new ApiConfig { ApiKey = "***" });
    }
    
    private static IAiService CreateServiceForProvider(ApiConfig config)
    {
        DebugService.Instance.LogDebug("AiServiceFactory", $"为供应商创建服务: {config.Provider}");
        try
        {
            // 根据供应商类型创建相应的服务实例
            switch (config.Provider)
            {
                case AiProvider.OpenAI:
                    return new OpenAiService(config);
                case AiProvider.CloudAPI:
                    return new CloudApiService(config);
                case AiProvider.AzureOpenAI:
                    return new AzureOpenAiService(config);
                case AiProvider.Claude:
                    return new ClaudeService(config);
                case AiProvider.Gemini:
                    return new GeminiService(config);
                default:
                    DebugService.Instance.LogWarning("AiServiceFactory", $"未知供应商类型，使用默认OpenAI: {config.Provider}");
                    return new OpenAiService(config);
            };
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogException("AiServiceFactory", "创建AI服务实例失败", ex);
            // 返回默认服务作为后备
            return new OpenAiService(new ApiConfig());
        }
    }
}