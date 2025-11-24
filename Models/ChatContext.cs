// Models/ChatContext.cs
using System.Collections.Generic;

namespace AIChatAssistant.Models;

/// <summary>
/// 聊天上下文
/// </summary>
public class ChatContext
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string ConversationId { get; set; }
    
    /// <summary>
    /// 消息历史
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    
    /// <summary>
    /// 用户信息
    /// </summary>
    public Dictionary<string, object> UserInfo { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// 会话元数据
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}