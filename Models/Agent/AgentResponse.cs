// Models/Agent/AgentResponse.cs
namespace AIChatAssistant.Models.Agent;

/// <summary>
/// 智能体响应
/// </summary>
public class AgentResponse
{
    /// <summary>
    /// 响应内容
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// 智能体ID
    /// </summary>
    public string AgentId { get; set; }
    
    /// <summary>
    /// 智能体名称
    /// </summary>
    public string AgentName { get; set; }
    
    /// <summary>
    /// 是否需要调用工具
    /// </summary>
    public bool RequiresToolCall { get; set; }
    
    /// <summary>
    /// 工具调用信息
    /// </summary>
    public ToolCallInfo ToolCallInfo { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public string Status { get; set; } = "success";
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }
}

/// <summary>
/// 工具调用信息
/// </summary>
public class ToolCallInfo
{
    /// <summary>
    /// 工具ID
    /// </summary>
    public string ToolId { get; set; }
    
    /// <summary>
    /// 工具名称
    /// </summary>
    public string ToolName { get; set; }
    
    /// <summary>
    /// 工具参数
    /// </summary>
    public System.Collections.Generic.Dictionary<string, object> Parameters { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
}