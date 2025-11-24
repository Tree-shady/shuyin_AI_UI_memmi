// UI/AgentSelectedEventArgs.cs
using AIChatAssistant.Models.Agent;

namespace AIChatAssistant.UI;

/// <summary>
/// 智能体选择事件参数
/// </summary>
public class AgentSelectedEventArgs : System.EventArgs
{
    /// <summary>
    /// 选中的智能体
    /// </summary>
    public AgentDefinition SelectedAgent { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="agent">选中的智能体定义</param>
    public AgentSelectedEventArgs(AgentDefinition agent)
    {
        SelectedAgent = agent;
    }
}