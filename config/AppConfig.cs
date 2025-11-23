// Config/AppConfig.cs
using System;
using System.IO;
using System.Text.Json;
using AIChatAssistant.Models;

namespace AIChatAssistant.Config;

public static class AppConfig
{
    private static readonly string ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "config.json");

    public static ApiConfig LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<ApiConfig>(json, options) ?? new ApiConfig();
            }
        }
        catch
        {
            // 如果读取失败，返回默认配置
        }

        return new ApiConfig();
    }

    public static void SaveConfig(ApiConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions 
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {            // 忽略保存错误
        }
    }
    
    // 获取指定提供商的默认配置
    public static ApiConfig GetDefaultConfigForProvider(AiProvider provider)
    {
        var config = new ApiConfig { Provider = provider };
        
        switch (provider)
        {
            case AiProvider.OpenAI:
                config.BaseUrl = "https://api.openai.com/v1";
                config.Model = "gpt-3.5-turbo";
                break;
            case AiProvider.AzureOpenAI:
                config.BaseUrl = "https://your-resource-name.openai.azure.com";
                config.Model = "gpt-35-turbo";
                config.ApiVersion = "2023-05-15";
                break;
            case AiProvider.Claude:
                config.BaseUrl = "https://api.anthropic.com/v1";
                config.Model = "claude-3-haiku-20240307";
                break;
            case AiProvider.Gemini:
                config.BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
                config.Model = "gemini-1.5-flash";
                break;
            case AiProvider.CloudAPI:
            default:
                config.BaseUrl = "https://apis.iflow.cn/v1";
                config.Model = "gpt-3.5-turbo";
                break;
        }
        
        return config;
    }
}