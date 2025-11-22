// Services/IConversationService.cs
using AIChatAssistant.Models;
using System.Collections.Generic;

namespace AIChatAssistant.Services;

public interface IConversationService
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
}