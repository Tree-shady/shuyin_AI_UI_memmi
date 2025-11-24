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
        DebugService.Instance.LogInfo("GeminiService", "服务初始化完成");
    }
    
    public void SetPluginManager(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        DebugService.Instance.LogDebug("GeminiService", "插件管理器已设置");
    }

    public async Task<string> SendMessageAsync(string message, List<ChatMessage> conversationHistory, string? conversationId = null)
    {
        DebugService.Instance.LogDebug("GeminiService", $"开始发送消息，会话ID: {conversationId ?? "default"}");
        try
        {
            // 首先尝试通过插件处理消息
            if (_pluginManager != null)
            {
                DebugService.Instance.LogDebug("GeminiService", "尝试通过插件处理消息");
                var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
                if (pluginResult != null && pluginResult.IsHandled)
                {
                    DebugService.Instance.LogInfo("GeminiService", "消息由插件处理");
                    return pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage;
                }
            }
            
            DebugService.Instance.LogDebug("GeminiService", "准备发送消息到Gemini API");
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
            DebugService.Instance.LogDebug("GeminiService", $"发送消息数量: {geminiMessages.Count}");

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
            
            DebugService.Instance.LogDebug("GeminiService", $"参数: MaxTokens={_config.MaxTokens}, Temperature={_config.Temperature}");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Gemini API端点，默认为https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent
            string modelName = string.IsNullOrEmpty(_config.Model) ? "gemini-1.5-flash" : _config.Model;
            string endpoint = string.IsNullOrEmpty(_config.BaseUrl) 
                ? $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent" 
                : $"{_config.BaseUrl}/{modelName}:generateContent";
            
            DebugService.Instance.LogInfo("GeminiService", $"调用Gemini API，端点: {endpoint}");
            // 记录请求参数
            DebugService.Instance.LogDebug("GeminiService", "发送API请求", json, string.Empty);

            var response = await _httpClient.PostAsync(endpoint, content);
            DebugService.Instance.LogDebug("GeminiService", $"API响应状态: {response.StatusCode}");
            
            // 获取响应内容并记录
            var responseContent = await response.Content.ReadAsStringAsync();
            DebugService.Instance.LogDebug("GeminiService", "收到API响应", string.Empty, responseContent);
            
            response.EnsureSuccessStatusCode();

            DebugService.Instance.LogDebug("GeminiService", "获取API响应内容成功");
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Gemini API响应格式解析
            if (responseObject.TryGetProperty("candidates", out JsonElement candidatesElement) && 
                candidatesElement.GetArrayLength() > 0 &&
                candidatesElement[0].TryGetProperty("content", out JsonElement contentElement) &&
                contentElement.TryGetProperty("parts", out JsonElement partsElement) &&
                partsElement.GetArrayLength() > 0 &&
                partsElement[0].TryGetProperty("text", out JsonElement textElement))
            {
                string result = textElement.GetString() ?? "没有收到回复";
                DebugService.Instance.LogInfo("GeminiService", "成功获取API响应内容");
                return result;
            }
            else
            {
                DebugService.Instance.LogWarning("GeminiService", "API响应格式不符合预期");
                return $"错误: 收到的Gemini API响应格式不符合预期。响应内容: {responseContent}";
            }
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogException("GeminiService", "发送消息失败", ex);
            return $"错误: {ex.Message}";
        }
    }

    public void UpdateConfig(ApiConfig config)
    {
        DebugService.Instance.LogInfo("GeminiService", "更新API配置");
        _config = config;
        _httpClient.DefaultRequestHeaders.Remove("x-goog-api-key");
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _config.ApiKey);
    }
}