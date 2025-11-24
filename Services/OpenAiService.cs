// Services/OpenAiService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;

namespace AIChatAssistant.Services;

public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private ApiConfig _config;
    private IPluginManager? _pluginManager;

    public OpenAiService(ApiConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
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
                model = _config.Model,
                messages = messages,
                max_tokens = _config.MaxTokens,
                temperature = _config.Temperature
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_config.BaseUrl}/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // 添加键存在性检查，避免"The given key was not present in the dictionary"错误
            if (responseObject.TryGetProperty("choices", out JsonElement choicesElement) && 
                choicesElement.GetArrayLength() > 0 &&
                choicesElement[0].TryGetProperty("message", out JsonElement messageElement) &&
                messageElement.TryGetProperty("content", out JsonElement contentElement))
            {
                return contentElement.GetString() ?? "没有收到回复";
            }
            else
            {
                // 如果响应格式不符合预期，返回错误信息
                return $"错误: 收到的API响应格式不符合预期。响应内容: {responseContent}";
            }
        }
        catch (Exception ex)
        {
            return $"错误: {ex.Message}";
        }
    }

    public async Task StreamMessageAsync(string message, List<ChatMessage> conversationHistory, Action<string> onContentReceived, string? conversationId = null)
    {
        try
        {
            // 首先尝试通过插件处理消息
            if (_pluginManager != null)
            {
                var pluginResult = await _pluginManager.ProcessMessageAsync(message, conversationId ?? "default");
                if (pluginResult != null && pluginResult.IsHandled)
                {
                    onContentReceived(pluginResult.IsSuccess ? pluginResult.Message : pluginResult.ErrorMessage);
                    return;
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
                model = _config.Model,
                messages = messages,
                max_tokens = _config.MaxTokens,
                temperature = _config.Temperature,
                stream = true
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync($"{_config.BaseUrl}/chat/completions", content, CancellationToken.None);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var buffer = new StringBuilder();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(line) || line == "data: [DONE]")
                    continue;

                // 移除"data: "前缀
                if (line.StartsWith("data: "))
                {
                    line = line["data: ".Length..];
                    
                    try
                    {
                        var responseObject = JsonSerializer.Deserialize<JsonElement>(line);
                        if (responseObject.TryGetProperty("choices", out JsonElement choicesElement) &&
                            choicesElement.GetArrayLength() > 0 &&
                            choicesElement[0].TryGetProperty("delta", out JsonElement deltaElement) &&
                            deltaElement.TryGetProperty("content", out JsonElement contentElement))
                        {
                            var token = contentElement.GetString();
                            if (!string.IsNullOrEmpty(token))
                            {
                                buffer.Append(token);
                                // 每积累一定量的文本再输出，避免频繁更新UI
                                if (buffer.Length >= 10 || token.Contains(". ") || token.Contains("。 ") || token.Contains("! ") || token.Contains("！ ") || token.Contains("? ") || token.Contains("？ "))
                                {
                                    onContentReceived(buffer.ToString());
                                    buffer.Clear();
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // 忽略无法解析的行
                    }
                }
            }

            // 确保所有剩余的内容都被发送
            if (buffer.Length > 0)
            {
                onContentReceived(buffer.ToString());
            }
        }
        catch (Exception ex)
        {
            onContentReceived($"错误: {ex.Message}");
        }
    }

    public void UpdateConfig(ApiConfig config)
    {
        _config = config;
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
    }
}