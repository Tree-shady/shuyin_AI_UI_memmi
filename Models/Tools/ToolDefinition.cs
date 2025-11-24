// Models/Tools/ToolDefinition.cs
using System;
using System.Collections.Generic;

namespace AIChatAssistant.Models.Tools;

public enum ToolType
{
    Search,
    File,
    Calculator,
    Code,
    Other
}

public class ToolDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public ToolType Type { get; set; }
    public List<string> TriggerKeywords { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string ExecuteMethod { get; set; } = "";
}