// Services/AzureOpenAiService.cs
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        DebugService.Instance.LogInfo("AzureOpenAiService", "服务初始化完成");
    }
    
    public void SetPluginManager(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        DebugService.Instance.LogDebug("AzureOpenAiService", "插件管理器已设置");
    }

    public async Task<string> SendMessageAsync(string message, List<ChatMessage> conversationHistory, string? conversationId = null)
    {
        DebugService.Instance.LogDebug("AzureOpenAiService", $"开始发送消息，会话ID: {conversationId ?? "default"}");
        try
        {
            // 首先尝试通过插件处理消息
            if (_pluginManager != null)
            {
                DebugService.Instance.LogDebug("AzureOpenAiService", "尝试通过插件处理消息");
                var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
                if (pluginResult != null && pluginResult.IsHandled)
                {
                    DebugService.Instance.LogInfo("AzureOpenAiService", "消息由插件处理");
                    return pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage;
                }
            }
            
            DebugService.Instance.LogDebug("AzureOpenAiService", "准备发送消息到Azure OpenAI API");
            var messages = conversationHistory.Select(msg => new
            {
                role = msg.Role,
                content = msg.Content
            }).ToList();

            // 添加当前消息
            messages.Add(new { role = "user", content = message });
            DebugService.Instance.LogDebug("AzureOpenAiService", $"发送消息数量: {messages.Count}");

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
            
            DebugService.Instance.LogInfo("AzureOpenAiService", $"调用Azure OpenAI API，端点: {endpoint}");
            DebugService.Instance.LogDebug("AzureOpenAiService", $"参数: MaxTokens={_config.MaxTokens}, Temperature={_config.Temperature}");
            // 记录请求参数
            DebugService.Instance.LogDebug("AzureOpenAiService", "发送API请求", json, string.Empty);

            var response = await _httpClient.PostAsync(endpoint, content);
            DebugService.Instance.LogDebug("AzureOpenAiService", $"API响应状态: {response.StatusCode}");
            
            // 获取响应内容并记录
            var responseContent = await response.Content.ReadAsStringAsync();
            DebugService.Instance.LogDebug("AzureOpenAiService", "收到API响应", string.Empty, responseContent);
            
            response.EnsureSuccessStatusCode();

            DebugService.Instance.LogDebug("AzureOpenAiService", "获取API响应内容成功");
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            if (responseObject.TryGetProperty("choices", out JsonElement choicesElement) && 
                choicesElement.GetArrayLength() > 0 &&
                choicesElement[0].TryGetProperty("message", out JsonElement messageElement) &&
                messageElement.TryGetProperty("content", out JsonElement contentElement))
            {
                string result = contentElement.GetString() ?? "没有收到回复";
                DebugService.Instance.LogInfo("AzureOpenAiService", "成功获取API响应内容");
                return result;
            }
            else
            {
                DebugService.Instance.LogWarning("AzureOpenAiService", "API响应格式不符合预期");
                return $"错误: 收到的Azure OpenAI API响应格式不符合预期。响应内容: {responseContent}";
            }
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogException("AzureOpenAiService", "发送消息失败", ex);
            return $"错误: {ex.Message}";
        }
    }

    public void UpdateConfig(ApiConfig config)
    {
        DebugService.Instance.LogInfo("AzureOpenAiService", "更新API配置");
        _config = config;
        _httpClient.DefaultRequestHeaders.Remove("api-key");
        _httpClient.DefaultRequestHeaders.Add("api-key", _config.ApiKey);
    }
    
    public async Task StreamMessageAsync(string message, List<ChatMessage> conversationHistory, Action<string> onContentReceived, string? conversationId = null)
    {
        DebugService.Instance.LogDebug("AzureOpenAiService", $"开始流式发送消息，会话ID: {conversationId ?? "default"}");
        try
        {
            // 首先尝试通过插件处理消息
            if (_pluginManager != null)
            {
                DebugService.Instance.LogDebug("AzureOpenAiService", "尝试通过插件处理消息");
                var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
                if (pluginResult != null && pluginResult.IsHandled)
                {
                    DebugService.Instance.LogInfo("AzureOpenAiService", "消息由插件处理");
                    onContentReceived(pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage);
                    return;
                }
            }
            
            DebugService.Instance.LogDebug("AzureOpenAiService", "准备流式发送消息到Azure OpenAI API");
            var messages = conversationHistory.Select(msg => new
            {
                role = msg.Role,
                content = msg.Content
            }).ToList();

            // 添加当前消息
            messages.Add(new { role = "user", content = message });
            DebugService.Instance.LogDebug("AzureOpenAiService", $"发送消息数量: {messages.Count}");

            var requestBody = new
            {
                messages = messages,
                max_tokens = _config.MaxTokens,
                temperature = _config.Temperature,
                stream = true // 启用流式响应
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // 构建Azure OpenAI特定的URL格式
            string endpoint = string.IsNullOrEmpty(_config.AzureDeploymentId)
                ? $"{_config.BaseUrl}/openai/deployments/{_config.Model}/chat/completions?api-version={_config.ApiVersion}"
                : $"{_config.BaseUrl}/openai/deployments/{_config.AzureDeploymentId}/chat/completions?api-version={_config.ApiVersion}";
            
            DebugService.Instance.LogInfo("AzureOpenAiService", $"调用Azure OpenAI API (流式)，端点: {endpoint}");
            DebugService.Instance.LogDebug("AzureOpenAiService", $"参数: MaxTokens={_config.MaxTokens}, Temperature={_config.Temperature}");
            // 记录请求参数
            DebugService.Instance.LogDebug("AzureOpenAiService", "发送API请求 (流式)", json, string.Empty);

            // 发送请求并处理流式响应
            using (var response = await _httpClient.PostAsync(endpoint, content, CancellationToken.None))
            {
                response.EnsureSuccessStatusCode();
                DebugService.Instance.LogDebug("AzureOpenAiService", "开始接收流式响应");
                
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    StringBuilder fullResponse = new StringBuilder();
                    string line;
                    
                    while (!reader.EndOfStream)
                    {
                        line = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(line) || line == "data: [DONE]")
                        {
                            continue;
                        }
                        
                        // 移除 "data: " 前缀
                        if (line.StartsWith("data: "))
                        {
                            line = line[6..];
                        }
                        
                        try
                        {
                            var responseObject = JsonSerializer.Deserialize<JsonElement>(line);
                            if (responseObject.TryGetProperty("choices", out JsonElement choicesElement) && 
                                choicesElement.GetArrayLength() > 0 &&
                                choicesElement[0].TryGetProperty("delta", out JsonElement deltaElement) &&
                                deltaElement.TryGetProperty("content", out JsonElement contentElement))
                            {
                                string newContent = contentElement.GetString() ?? "";
                                if (!string.IsNullOrEmpty(newContent))
                                {
                                    fullResponse.Append(newContent);
                                    onContentReceived(newContent);
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            DebugService.Instance.LogError("AzureOpenAiService", $"解析流式响应出错: {ex.Message}");
                        }
                    }
                    
                    DebugService.Instance.LogDebug("AzureOpenAiService", "流式响应接收完成");
                }
            }
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogError("AzureOpenAiService", $"流式请求失败: {ex.Message}");
            onContentReceived($"错误: {ex.Message}");
        }
    }
}