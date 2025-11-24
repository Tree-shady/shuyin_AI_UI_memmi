// Services/MemoryService.cs
using AIChatAssistant.Models;
using System.Linq;
using System.Collections.Generic;

namespace AIChatAssistant.Services;

public class MemoryService
{
    /// <summary>
    /// 检索与当前输入相关的记忆
    /// </summary>
    /// <param name="input">用户输入</param>
    /// <param name="context">聊天上下文</param>
    /// <returns>相关记忆内容</returns>
    public string RetrieveRelevantMemory(string input, ChatContext context)
    {
        // 简单实现：从上下文中获取最近的对话历史作为记忆
        // 在实际应用中，这里可以实现更复杂的记忆检索逻辑
        if (context?.Messages == null || context.Messages.Count == 0)
        {
            return string.Empty;
        }

        var memoryBuilder = new System.Text.StringBuilder();
        var relevantMessages = context.Messages
            .Where(m => m.Role != "system")
            .OrderByDescending(m => m.Timestamp)
            .Take(5); // 获取最近的5条消息

        foreach (var message in relevantMessages.Reverse()) // 按时间正序排列
        {
            memoryBuilder.AppendLine($"{message.Role}: {message.Content}");
        }

        return memoryBuilder.ToString();
    }
}