// Models/HotkeySettings.cs
namespace AIChatAssistant.Models;

public class HotkeySettings
{
    public bool EnableGlobalHotkeys { get; set; } = true;
    public string ShowChatHotkey { get; set; } = "Alt+Shift+C";
    public string NewChatHotkey { get; set; } = "Alt+Shift+N";
    public string SendMessageHotkey { get; set; } = "Enter";
}
