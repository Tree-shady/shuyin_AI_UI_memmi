// Services/ClaudeService.cs
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;

namespace AIChatAssistant.Services;

public class ClaudeService : IAiService
{
    private readonly HttpClient _httpClient;
    private ApiConfig _config;
    private IPluginManager? _pluginManager;

    public ClaudeService(ApiConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
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
            
            // 构建Claude格式的消息列表
            var messages = conversationHistory.Select(msg => new
            {
                role = msg.Role switch
                {
                    "assistant" => "assistant",
                    _ => "user"
                },
                content = msg.Content
            }).ToList();

            // 添加当前消息
            messages.Add(new { role = "user", content = message });

            // Claude特定的请求体格式
            var requestBody = new
            {
                model = _config.Model, // Claude模型名称，如"claude-3-opus-20240229"
                messages = messages,
                max_tokens = _config.MaxTokens,
                temperature = _config.Temperature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Claude API端点，默认为https://api.anthropic.com/v1/messages
            string endpoint = string.IsNullOrEmpty(_config.BaseUrl) ? "https://api.anthropic.com/v1/messages" : $"{_config.BaseUrl}/messages";

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Claude API响应格式解析
            if (responseObject.TryGetProperty("content", out JsonElement contentArray) && 
                contentArray.GetArrayLength() > 0 &&
                contentArray[0].TryGetProperty("text", out JsonElement textElement))
            {
                return textElement.GetString() ?? "没有收到回复";
            }
            else
            {
                return $"错误: 收到的Claude API响应格式不符合预期。响应内容: {responseContent}";
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
        _httpClient.DefaultRequestHeaders.Remove("x-api-key");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
    }
}