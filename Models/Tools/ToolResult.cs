// Models/Tools/ToolResult.cs
using System;

namespace AIChatAssistant.Models.Tools;

/// <summary>
/// 工具执行结果
/// </summary>
public class ToolResult
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
    /// 执行结果
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// 执行时间
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }
    
    /// <summary>
    /// 元数据
    /// </summary>
    public System.Collections.Generic.Dictionary<string, object> Metadata { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
}