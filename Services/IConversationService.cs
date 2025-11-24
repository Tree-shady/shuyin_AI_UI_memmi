// Services/IConversationService.cs
using AIChatAssistant.Models;
using System.Collections.Generic;
using System; // 添加IDisposable需要的命名空间

namespace AIChatAssistant.Services;

public interface IConversationService : IDisposable
{
    // 创建新会话
    Conversation CreateConversation();
    
    // 获取所有会话
    List<Conversation> GetAllConversations();
    
    // 通过ID获取会话
    Conversation? GetConversationById(string id);
    
    // 更新会话
    void UpdateConversation(Conversation conversation);
    
    // 删除会话
    bool DeleteConversation(string id);
    
    // 获取当前活动会话
    Conversation? GetActiveConversation();
    
    // 设置当前活动会话
    void SetActiveConversation(string conversationId);
    
    // 向会话添加消息
    void AddMessageToConversation(string conversationId, ChatMessage message);
    
    // 更新会话标题
    void UpdateConversationTitle(string conversationId, string newTitle);
    
    /// <summary>
    /// 立即保存所有对话
    /// </summary>
    void SaveAllConversations();
}