// Models/ChatMessage.cs
namespace AIChatAssistant.Models;

public class ChatMessage
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}