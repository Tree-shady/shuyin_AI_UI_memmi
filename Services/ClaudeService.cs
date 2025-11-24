// Services/ClaudeService.cs
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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
        DebugService.Instance.LogInfo("ClaudeService", "服务初始化完成");
    }
    
    public void SetPluginManager(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
        DebugService.Instance.LogDebug("ClaudeService", "插件管理器已设置");
    }

    public async Task<string> SendMessageAsync(string message, List<ChatMessage> conversationHistory, string? conversationId = null)
    {
        DebugService.Instance.LogDebug("ClaudeService", $"开始发送消息，会话ID: {conversationId ?? "default"}");
        try
        {
            // 首先尝试通过插件处理消息
            if (_pluginManager != null)
            {
                DebugService.Instance.LogDebug("ClaudeService", "尝试通过插件处理消息");
                var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
                if (pluginResult != null && pluginResult.IsHandled)
                {
                    DebugService.Instance.LogInfo("ClaudeService", "消息由插件处理");
                    return pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage;
                }
            }
            
            DebugService.Instance.LogDebug("ClaudeService", "准备发送消息到Claude API");
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
            DebugService.Instance.LogDebug("ClaudeService", $"发送消息数量: {messages.Count}");

            // Claude特定的请求体格式
            var requestBody = new
            {
                model = _config.Model, // Claude模型名称，如"claude-3-opus-20240229"
                messages = messages,
                max_tokens = _config.MaxTokens,
                temperature = _config.Temperature
            };
            
            DebugService.Instance.LogDebug("ClaudeService", $"参数: MaxTokens={_config.MaxTokens}, Temperature={_config.Temperature}");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Claude API端点，默认为https://api.anthropic.com/v1/messages
            string endpoint = string.IsNullOrEmpty(_config.BaseUrl) ? "https://api.anthropic.com/v1/messages" : $"{_config.BaseUrl}/messages";
            DebugService.Instance.LogInfo("ClaudeService", $"调用Claude API，端点: {endpoint}");
            // 记录请求参数
            DebugService.Instance.LogDebug("ClaudeService", "发送API请求", json, string.Empty);

            var response = await _httpClient.PostAsync(endpoint, content);
            DebugService.Instance.LogDebug("ClaudeService", $"API响应状态: {response.StatusCode}");
            
            // 获取响应内容并记录
            var responseContent = await response.Content.ReadAsStringAsync();
            DebugService.Instance.LogDebug("ClaudeService", "收到API响应", string.Empty, responseContent);
            
            response.EnsureSuccessStatusCode();

            DebugService.Instance.LogDebug("ClaudeService", "获取API响应内容成功");
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Claude API响应格式解析
            if (responseObject.TryGetProperty("content", out JsonElement contentArray) && 
                contentArray.GetArrayLength() > 0 &&
                contentArray[0].TryGetProperty("text", out JsonElement textElement))
            {
                string result = textElement.GetString() ?? "没有收到回复";
                DebugService.Instance.LogInfo("ClaudeService", "成功获取API响应内容");
            return result;
        }
        else
        {
            DebugService.Instance.LogWarning("ClaudeService", "API响应格式不符合预期");
            return $"错误: 收到的Claude API响应格式不符合预期。响应内容: {responseContent}";
        }
    }
    catch (Exception ex)
    {
        DebugService.Instance.LogException("ClaudeService", "发送消息失败", ex);
        return $"错误: {ex.Message}";
    }
}

public void UpdateConfig(ApiConfig config)
{
    DebugService.Instance.LogInfo("ClaudeService", "更新API配置");
    _config = config;
    _httpClient.DefaultRequestHeaders.Remove("x-api-key");
    _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.ApiKey);
}

public async Task StreamMessageAsync(string message, List<ChatMessage> conversationHistory, Action<string> onContentReceived, string? conversationId = null)
{
    DebugService.Instance.LogDebug("ClaudeService", $"开始流式发送消息，会话ID: {conversationId ?? "default"}");
    try
    {
        // 首先尝试通过插件处理消息
        if (_pluginManager != null)
        {
            DebugService.Instance.LogDebug("ClaudeService", "尝试通过插件处理消息");
            var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
            if (pluginResult != null && pluginResult.IsHandled)
            {
                DebugService.Instance.LogInfo("ClaudeService", "消息由插件处理");
                onContentReceived(pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage);
                return;
            }
        }
        
        DebugService.Instance.LogDebug("ClaudeService", "准备流式发送消息到Claude API");
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
        DebugService.Instance.LogDebug("ClaudeService", $"发送消息数量: {messages.Count}");

        // Claude特定的请求体格式，添加stream参数
        var requestBody = new
        {
            model = _config.Model,
            messages = messages,
            max_tokens = _config.MaxTokens,
            temperature = _config.Temperature,
            stream = true // 启用流式响应
        };
        
        DebugService.Instance.LogDebug("ClaudeService", $"参数: MaxTokens={_config.MaxTokens}, Temperature={_config.Temperature}");

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Claude API端点
        string endpoint = string.IsNullOrEmpty(_config.BaseUrl) ? "https://api.anthropic.com/v1/messages" : $"{_config.BaseUrl}/messages";
        DebugService.Instance.LogInfo("ClaudeService", $"调用Claude API (流式)，端点: {endpoint}");
        // 记录请求参数
        DebugService.Instance.LogDebug("ClaudeService", "发送API请求 (流式)", json, string.Empty);

        // 发送请求并处理流式响应
        using (var response = await _httpClient.PostAsync(endpoint, content, CancellationToken.None))
        {
            response.EnsureSuccessStatusCode();
            DebugService.Instance.LogDebug("ClaudeService", "开始接收流式响应");
            
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var reader = new StreamReader(stream))
            {
                StringBuilder fullResponse = new StringBuilder();
                string line;
                
                while (!reader.EndOfStream)
                {
                    line = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    
                    // 移除 "data: " 前缀
                    if (line.StartsWith("data: "))
                    {
                        line = line[6..];
                    }
                    
                    // 处理完成信号
                    if (line == "[DONE]")
                    {
                        break;
                    }
                    
                    try
                    {
                        var responseObject = JsonSerializer.Deserialize<JsonElement>(line);
                        
                        // 处理中间响应或最终响应
                        if (responseObject.TryGetProperty("type", out JsonElement typeElement))
                        {
                            string responseType = typeElement.GetString() ?? "";
                            
                            // 处理内容更新
                            if (responseType == "content_block_delta" &&
                                responseObject.TryGetProperty("delta", out JsonElement deltaElement) &&
                                deltaElement.TryGetProperty("text", out JsonElement textElement))
                            {
                                string newContent = textElement.GetString() ?? "";
                                if (!string.IsNullOrEmpty(newContent))
                                {
                                    fullResponse.Append(newContent);
                                    onContentReceived(newContent);
                                }
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        DebugService.Instance.LogError("ClaudeService", $"解析流式响应出错: {ex.Message}");
                    }
                }
                
                DebugService.Instance.LogDebug("ClaudeService", "流式响应接收完成");
            }
        }
    }
    catch (Exception ex)
    {
        DebugService.Instance.LogError("ClaudeService", $"流式请求失败: {ex.Message}");
        onContentReceived($"错误: {ex.Message}");
    }
}
}