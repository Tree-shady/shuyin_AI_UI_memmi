// Models/Agent/AgentDefinition.cs
using AIChatAssistant.Models.Tools;
using System;
using System.Collections.Generic;

namespace AIChatAssistant.Models.Agent;

public class AgentDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string SystemPrompt { get; set; } = "";
    public AgentCapability Capabilities { get; set; } = new();
    public AgentPersonality Personality { get; set; } = new();
    public List<ToolDefinition> Tools { get; set; } = new();
    public AgentConfig Config { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}

// Models/Agent/AgentCapability.cs
public class AgentCapability
{
    public bool CanSearchWeb { get; set; }
    public bool CanAccessFiles { get; set; }
    public bool CanExecuteCode { get; set; }
    public bool CanCallApis { get; set; }
    public bool CanRememberContext { get; set; }
    public int MaxContextLength { get; set; } = 10;
    public List<string> SupportedFormats { get; set; } = new() { "text", "markdown" };
}

// Models/Agent/AgentPersonality.cs
public class AgentPersonality
{
    public string Tone { get; set; } = "friendly"; // friendly, professional, humorous, etc.
    public string ResponseStyle { get; set; } = "detailed"; // concise, detailed, creative
    public string CommunicationStyle { get; set; } = "casual"; // formal, casual, technical
    public List<string> Specialties { get; set; } = new();
    public List<string> Limitations { get; set; } = new();
}