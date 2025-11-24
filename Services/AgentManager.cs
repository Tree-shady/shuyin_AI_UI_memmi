// Services/AgentManager.cs
using System.Text.Json;
using AIChatAssistant.Models.Agent;
using System.IO;
using AIChatAssistant.Models;
using AIChatAssistant.Models.Tools;

namespace AIChatAssistant.Services;

// 智能体类型枚举
public enum AgentType
{
    General,
    CodeExpert,
    CreativeWriter,
    DataAnalyst
}

public class AgentManager
{
    private readonly string _agentsDirectory;
    private Dictionary<string, AgentDefinition> _agents = new();
    private AgentDefinition _currentAgent;

    public AgentManager()
    {
        _agentsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Agents");
        Directory.CreateDirectory(_agentsDirectory);
        
        LoadBuiltInAgents();
        LoadCustomAgents();
    }

    public List<AgentDefinition> GetAvailableAgents()
    {
        return _agents.Values.ToList();
    }

    public void SetCurrentAgent(string agentId)
    {
        if (_agents.TryGetValue(agentId, out var agent))
        {
            _currentAgent = agent;
            DebugService.Instance.LogInfo("AgentManager", $"切换智能体: {agent.Name}");
        }
    }

    public AgentDefinition GetCurrentAgent()
    {
        return _currentAgent ?? CreateDefaultAgent();
    }

    private AgentDefinition CreateDefaultAgent()
    {
        return new AgentDefinition
        {
            Id = "default_agent",
            Name = "默认助手",
            Description = "默认智能助手",
            SystemPrompt = "你是一个有帮助的AI助手，请友好、详细地回答用户的问题。",
            Tools = new List<ToolDefinition>()
        };
    }

    private void LoadBuiltInAgents()
    {
        var builtInAgents = new[]
        {
            new AgentDefinition
            {
                Id = "general_assistant",
                Name = "通用助手",
                Description = "适用于日常对话和问题解答",
                SystemPrompt = "你是一个有帮助的AI助手，请友好、详细地回答用户的问题。",
                Personality = new AgentPersonality
                {
                    Tone = "friendly",
                    ResponseStyle = "detailed",
                    CommunicationStyle = "casual",
                    Specialties = new List<string> { "日常对话", "问题解答", "信息查询" }
                }
            },
            new AgentDefinition
            {
                Id = "code_expert",
                Name = "代码专家",
                Description = "专门处理编程和技术问题",
                SystemPrompt = "你是一个专业的编程助手，擅长代码编写、调试和技术问题解答。",
                Capabilities = new AgentCapability
                {
                    CanExecuteCode = true,
                    CanRememberContext = true
                },
                Personality = new AgentPersonality
                {
                    Tone = "professional",
                    ResponseStyle = "technical",
                    CommunicationStyle = "technical",
                    Specialties = new List<string> { "代码编写", "调试帮助", "技术方案" }
                }
            },
            new AgentDefinition
            {
                Id = "creative_writer",
                Name = "创意写手",
                Description = "擅长创意写作和内容创作",
                SystemPrompt = "你是一个创意写手，擅长故事创作、文案写作和内容生成。",
                Personality = new AgentPersonality
                {
                    Tone = "creative",
                    ResponseStyle = "creative",
                    CommunicationStyle = "casual",
                    Specialties = new List<string> { "故事创作", "文案写作", "内容生成" }
                }
            },
            new AgentDefinition
            {
                Id = "data_analyst",
                Name = "数据分析师",
                Description = "擅长数据处理、分析和可视化",
                SystemPrompt = "你是一个数据分析专家，擅长解释数据、分析趋势和制作可视化图表。",
                Capabilities = new AgentCapability
                {
                    CanAccessFiles = true,
                    CanRememberContext = true
                },
                Personality = new AgentPersonality
                {
                    Tone = "analytical",
                    ResponseStyle = "structured",
                    CommunicationStyle = "technical",
                    Specialties = new List<string> { "数据分析", "趋势预测", "可视化" }
                }
            }
        };

        foreach (var agent in builtInAgents)
        {
            _agents[agent.Id] = agent;
        }
    }

    private void LoadCustomAgents()
    {
        try
        {
            var agentFiles = Directory.GetFiles(_agentsDirectory, "*.json");
            foreach (var file in agentFiles)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var agent = JsonSerializer.Deserialize<AgentDefinition>(json);
                    if (agent != null)
                    {
                        _agents[agent.Id] = agent;
                    }
                }
                catch (Exception ex)
                {
                    DebugService.Instance.LogError("AgentManager", $"加载自定义智能体文件失败: {file} - {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogError("AgentManager", $"加载自定义智能体失败: {ex.Message}");
        }
    }

    public void SaveCustomAgent(AgentDefinition agent)
    {
        try
        {
            var filePath = Path.Combine(_agentsDirectory, $"{agent.Id}.json");
            var json = JsonSerializer.Serialize(agent, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
            _agents[agent.Id] = agent;
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogError("AgentManager", $"保存自定义智能体失败: {ex.Message}");
        }
    }

    public AgentDefinition CreateAgentFromTemplate(string name, string description, 
        AgentType type = AgentType.General)
    {
        var template = GetAgentTemplate(type);
        return new AgentDefinition
        {
            Name = name,
            Description = description,
            SystemPrompt = template.SystemPrompt,
            Capabilities = template.Capabilities,
            Personality = template.Personality
        };
    }

    private AgentDefinition GetAgentTemplate(AgentType type)
    {
        switch (type)
        {
            case AgentType.CodeExpert:
                return new AgentDefinition
                {
                    SystemPrompt = "你是一个专业的编程助手，擅长代码编写、调试和技术问题解答。",
                    Capabilities = new AgentCapability { CanExecuteCode = true, CanRememberContext = true },
                    Personality = new AgentPersonality
                    {
                        Tone = "professional",
                        ResponseStyle = "technical",
                        CommunicationStyle = "technical",
                        Specialties = new List<string> { "代码编写", "调试帮助", "技术方案" }
                    }
                };
            case AgentType.CreativeWriter:
                return new AgentDefinition
                {
                    SystemPrompt = "你是一个创意写手，擅长故事创作、文案写作和内容生成。",
                    Personality = new AgentPersonality
                    {
                        Tone = "creative",
                        ResponseStyle = "creative",
                        CommunicationStyle = "casual",
                        Specialties = new List<string> { "故事创作", "文案写作", "内容生成" }
                    }
                };
            case AgentType.DataAnalyst:
                return new AgentDefinition
                {
                    SystemPrompt = "你是一个数据分析专家，擅长解释数据、分析趋势和制作可视化图表。",
                    Capabilities = new AgentCapability { CanAccessFiles = true, CanRememberContext = true },
                    Personality = new AgentPersonality
                    {
                        Tone = "analytical",
                        ResponseStyle = "structured",
                        CommunicationStyle = "technical",
                        Specialties = new List<string> { "数据分析", "趋势预测", "可视化" }
                    }
                };
            case AgentType.General:
            default:
                return new AgentDefinition
                {
                    SystemPrompt = "你是一个有帮助的AI助手，请友好、详细地回答用户的问题。",
                    Personality = new AgentPersonality
                    {
                        Tone = "friendly",
                        ResponseStyle = "detailed",
                        CommunicationStyle = "casual",
                        Specialties = new List<string> { "日常对话", "问题解答", "信息查询" }
                    }
                };
        }
    }
}
