// Models/Conversation.cs
using System.Collections.Generic;
using System;namespace AIChatAssistant.Models;

public class Conversation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "新对话";  
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastModifiedAt { get; set; } = DateTime.Now;
    public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    
    public void AddMessage(ChatMessage message)
    {
        Messages.Add(message);
        LastModifiedAt = DateTime.Now;
        
        // 如果是第一条用户消息，使用它作为对话标题
        if (Messages.Count == 1 && message.Role == "user" && message.Content.Length > 0)
        {
            Title = message.Content.Length > 50 
                ? message.Content.Substring(0, 50) + "..." 
                : message.Content;
        }
    }
    
    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
        LastModifiedAt = DateTime.Now;
    }
}