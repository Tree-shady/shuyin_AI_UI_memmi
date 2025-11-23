// Services/GeminiService.cs
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;

namespace AIChatAssistant.Services;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private ApiConfig _config;
    private IPluginManager? _pluginManager;

    public GeminiService(ApiConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
    }
    
    public void SetPluginManager(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public async Task<string> SendMessageAsync(string message, List<ChatMessage> conversationHistory, string? conversationId = null)
    {
        try
        {
            // 首先尝试通过插件处理消息
            if (_pluginManager != null)
            {
                var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
                if (pluginResult != null && pluginResult.IsHandled)
                {
                    return pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage;
                }
            }
            
            // 构建Gemini格式的消息列表
            var geminiMessages = new List<object>();
            
            foreach (var msg in conversationHistory)
            {
                geminiMessages.Add(new
                {
                    role = msg.Role switch
                    {
                        "assistant" => "model",
                        _ => "user"
                    },
                    parts = new[] {
                        new { text = msg.Content }
                    }
                });
            }

            // 添加当前消息
            geminiMessages.Add(new
            {
                role = "user",
                parts = new[] {
                    new { text = message }
                }
            });

            // Gemini特定的请求体格式
            var requestBody = new
            {
                contents = geminiMessages,
                generationConfig = new
                {
                    maxOutputTokens = _config.MaxTokens,
                    temperature = _config.Temperature
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gemini API端点，默认为https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent
            string modelName = string.IsNullOrEmpty(_config.Model) ? "gemini-1.5-flash" : _config.Model;
            string endpoint = string.IsNullOrEmpty(_config.BaseUrl) 
                ? $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent" 
                : $"{_config.BaseUrl}/{modelName}:generateContent";

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Gemini API响应格式解析
            if (responseObject.TryGetProperty("candidates", out JsonElement candidatesElement) && 
                candidatesElement.GetArrayLength() > 0 &&
                candidatesElement[0].TryGetProperty("content", out JsonElement contentElement) &&
                contentElement.TryGetProperty("parts", out JsonElement partsElement) &&
                partsElement.GetArrayLength() > 0 &&
                partsElement[0].TryGetProperty("text", out JsonElement textElement))
            {
                return textElement.GetString() ?? "没有收到回复";
            }
            else
            {
                return $"错误: 收到的Gemini API响应格式不符合预期。响应内容: {responseContent}";
            }
        }
        catch (Exception ex)
        {
            return $"错误: {ex.Message}";
        }
    }

    public void UpdateConfig(ApiConfig config)
    {
        _config = config;
        _httpClient.DefaultRequestHeaders.Remove("x-goog-api-key");
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _config.ApiKey);
    }
}