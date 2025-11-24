// Services/CloudApiService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;

namespace AIChatAssistant.Services;

public class CloudApiService : IAiService
{
    private ApiConfig _config;
    private IPluginManager? _pluginManager;

    public CloudApiService(ApiConfig config)
    {
        _config = config;
    }
    
    public void SetPluginManager(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public async Task<string> SendMessageAsync(string message, List<ChatMessage> conversationHistory, string? conversationId = null)
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
        
        // 这里可以实现其他云服务API的调用
        // 例如：Azure OpenAI、百度文心一言、阿里通义千问等
        
        await Task.Delay(100); // 模拟API调用延迟
        
        // 简单模拟回复
        return $"这是来自云API的回复: {message} (模拟回复)";
    }

    public async Task StreamMessageAsync(string message, List<ChatMessage> conversationHistory, Action<string> onContentReceived, string? conversationId = null)
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

        // 模拟流式输出
        string mockResponse = $"这是来自云API的流式回复: {message} (模拟回复)";
        
        // 逐字符发送模拟输出，模拟真实的流式响应
        foreach (char c in mockResponse)
        {
            await Task.Delay(20); // 模拟延迟
            onContentReceived(c.ToString());
        }
    }

    public void UpdateConfig(ApiConfig config)
    {
        _config = config;
    }
}