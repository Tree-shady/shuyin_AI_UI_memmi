// Services/AzureOpenAiService.cs
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;

namespace AIChatAssistant.Services;

public class AzureOpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private ApiConfig _config;
    private IPluginManager? _pluginManager;

    public AzureOpenAiService(ApiConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
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
            
            var messages = conversationHistory.Select(msg => new
            {
                role = msg.Role,
                content = msg.Content
            }).ToList();

            // 添加当前消息
            messages.Add(new { role = "user", content = message });

            var requestBody = new
            {
                messages = messages,
                max_tokens = _config.MaxTokens,
                temperature = _config.Temperature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 构建Azure OpenAI特定的URL格式
            string endpoint = string.IsNullOrEmpty(_config.AzureDeploymentId)
                ? $"{_config.BaseUrl}/openai/deployments/{_config.Model}/chat/completions?api-version={_config.ApiVersion}"
                : $"{_config.BaseUrl}/openai/deployments/{_config.AzureDeploymentId}/chat/completions?api-version={_config.ApiVersion}";

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (responseObject.TryGetProperty("choices", out JsonElement choicesElement) && 
                choicesElement.GetArrayLength() > 0 &&
                choicesElement[0].TryGetProperty("message", out JsonElement messageElement) &&
                messageElement.TryGetProperty("content", out JsonElement contentElement))
            {
                return contentElement.GetString() ?? "没有收到回复";
            }
            else
            {
                return $"错误: 收到的Azure OpenAI API响应格式不符合预期。响应内容: {responseContent}";
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
        _httpClient.DefaultRequestHeaders.Remove("api-key");
        _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
    }
}