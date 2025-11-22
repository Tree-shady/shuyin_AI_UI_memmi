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
                return JsonSerializer.Deserialize<ApiConfig>(json) ?? new ApiConfig();
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
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // 忽略保存错误
        }
    }
}