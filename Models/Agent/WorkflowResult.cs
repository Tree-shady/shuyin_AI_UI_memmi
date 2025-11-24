// Models/Agent/WorkflowResult.cs
using System.Collections.Generic;
using System;

namespace AIChatAssistant.Models.Agent;

/// <summary>
/// 工作流执行结果
/// </summary>
public class WorkflowResult
{
    /// <summary>
    /// 最终结果
    /// </summary>
    public string FinalResult { get; set; } = "";
    
    /// <summary>
    /// 子任务结果列表
    /// </summary>
    public List<SubTaskResult> SubResults { get; set; } = new List<SubTaskResult>();
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 执行时间
    /// </summary>
    public TimeSpan ExecutionTime { get; set; }
}