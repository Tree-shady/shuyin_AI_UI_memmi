// Models/AppSettings.cs
namespace AIChatAssistant.Models;

public class AppSettings
{
    public ApiConfig ApiConfig { get; set; } = new();
    public UiSettings UiSettings { get; set; } = new();
    public PluginSettings PluginSettings { get; set; } = new();
    public HotkeySettings HotkeySettings { get; set; } = new();
}
