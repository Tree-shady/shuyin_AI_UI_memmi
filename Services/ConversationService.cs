// Services/ConversationService.cs
using AIChatAssistant.Models;
using System.Collections.Generic;
using System.Linq;

namespace AIChatAssistant.Services;

public class ConversationService : IConversationService
{
    private readonly List<Conversation> _conversations = new List<Conversation>();
    private string? _activeConversationId = null;
    
    // 创建新会话
    public Conversation CreateConversation()
    {
        var conversation = new Conversation();
        _conversations.Add(conversation);
        _activeConversationId = conversation.Id;
        return conversation;
    }
    
    // 获取所有会话
    public List<Conversation> GetAllConversations()
    {
        return _conversations.OrderByDescending(c => c.LastModifiedAt).ToList();
    }
    
    // 通过ID获取会话
    public Conversation? GetConversationById(string id)
    {
        return _conversations.FirstOrDefault(c => c.Id == id);
    }
    
    // 更新会话
    public void UpdateConversation(Conversation conversation)
    {
        var existingConversation = GetConversationById(conversation.Id);
        if (existingConversation != null)
        {
            // 更新现有会话的属性
            existingConversation.Title = conversation.Title;
            existingConversation.LastModifiedAt = conversation.LastModifiedAt;
            existingConversation.Messages = conversation.Messages;
        }
    }
    
    // 删除会话
    public bool DeleteConversation(string id)
    {
        var conversation = GetConversationById(id);
        if (conversation != null)
        {
            _conversations.Remove(conversation);
            
            // 如果删除的是活动会话，则清除活动会话ID
            if (_activeConversationId == id)
            {
                _activeConversationId = null;
                // 如果还有其他会话，设置最新的一个为活动会话
                if (_conversations.Any())
                {
                    _activeConversationId = _conversations.OrderByDescending(c => c.LastModifiedAt).First().Id;
                }
            }
            return true;
        }
        return false;
    }
    
    // 获取当前活动会话
    public Conversation? GetActiveConversation()
    {
        if (_activeConversationId == null)
        {
            return null;
        }
        return GetConversationById(_activeConversationId);
    }
    
    // 设置当前活动会话
    public void SetActiveConversation(string conversationId)
    {
        var conversation = GetConversationById(conversationId);
        if (conversation != null)
        {
            _activeConversationId = conversationId;
        }
    }
    
    // 向会话添加消息
    public void AddMessageToConversation(string conversationId, ChatMessage message)
    {
        var conversation = GetConversationById(conversationId);
        if (conversation != null)
        {
            conversation.AddMessage(message);
        }
    }
    
    // 更新会话标题
    public void UpdateConversationTitle(string conversationId, string newTitle)
    {
        var conversation = GetConversationById(conversationId);
        if (conversation != null)
        {
            conversation.UpdateTitle(newTitle);
        }
    }
}