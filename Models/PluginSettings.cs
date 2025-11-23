// Models/PluginSettings.cs
using System.Collections.Generic;

namespace AIChatAssistant.Models;

public class PluginSettings
{
    public bool EnablePlugins { get; set; } = true;
    public Dictionary<string, bool> EnabledPlugins { get; set; } = new();
    public Dictionary<string, object> PluginConfigs { get; set; } = new();
}
