// Services/IAiService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;

namespace AIChatAssistant.Services;

public interface IAiService
{
    Task<string> SendMessageAsync(string message, List<ChatMessage> conversationHistory, string? conversationId = null);
    Task StreamMessageAsync(string message, List<ChatMessage> conversationHistory, Action<string> onContentReceived, string? conversationId = null);
    void UpdateConfig(ApiConfig config);
    void SetPluginManager(IPluginManager pluginManager);
}