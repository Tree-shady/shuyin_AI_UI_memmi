// Services/ConversationService.cs
using AIChatAssistant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace AIChatAssistant.Services;

public class ConversationService : IConversationService
{
    private readonly List<Conversation> _conversations = new List<Conversation>();
    private string? _activeConversationId = null;
    private readonly string _conversationsFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    
    public ConversationService()
    {
        // 设置对话保存路径
        var appDataDir = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(appDataDir);
        _conversationsFilePath = Path.Combine(appDataDir, "conversations.json");
        
        // 加载已保存的对话
        LoadConversations();
    }
    
    /// <summary>
    /// 从文件加载对话
    /// </summary>
    private void LoadConversations()
    {
        try
        {
            if (File.Exists(_conversationsFilePath))
            {
                var json = File.ReadAllText(_conversationsFilePath);
                var savedData = JsonSerializer.Deserialize<ConversationData>(json, _jsonOptions);
                
                if (savedData != null)
                {
                    _conversations.Clear();
                    _conversations.AddRange(savedData.Conversations);
                    _activeConversationId = savedData.ActiveConversationId;
                }
            }
        }
        catch (Exception ex)
        {
            // 记录加载失败，但不影响程序运行
            DebugService.Instance.LogWarning("ConversationService", $"加载对话失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 保存对话到文件
    /// </summary>
    private void SaveConversations()
    {
        try
        {
            var data = new ConversationData
            {
                Conversations = _conversations.ToList(),
                ActiveConversationId = _activeConversationId
            };
            
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(_conversationsFilePath, json);
        }
        catch (Exception ex)
        {
            // 记录保存失败，但不影响程序运行
            DebugService.Instance.LogWarning("ConversationService", $"保存对话失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 用于序列化的对话数据类
    /// </summary>
    private class ConversationData
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        public string? ActiveConversationId { get; set; }
    }
    
    // 创建新会话
    public Conversation CreateConversation()
    {
        var conversation = new Conversation();
        _conversations.Add(conversation);
        _activeConversationId = conversation.Id;
        SaveConversations();
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
            SaveConversations();
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
            SaveConversations();
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
            SaveConversations();
        }
    }
    
    // 向会话添加消息
    public void AddMessageToConversation(string conversationId, ChatMessage message)
    {
        var conversation = GetConversationById(conversationId);
        if (conversation != null)
        {
            conversation.AddMessage(message);
            SaveConversations();
        }
    }
    
    // 更新会话标题
    public void UpdateConversationTitle(string conversationId, string newTitle)
    {
        var conversation = GetConversationById(conversationId);
        if (conversation != null)
        {
            conversation.UpdateTitle(newTitle);
            SaveConversations();
        }
    }
}