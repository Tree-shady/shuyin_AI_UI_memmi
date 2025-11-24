// Models/Agent/SubTaskResult.cs
namespace AIChatAssistant.Models.Agent;

/// <summary>
/// 子任务执行结果
/// </summary>
public class SubTaskResult
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public string TaskId { get; set; }
    
    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; }
    
    /// <summary>
    /// 执行结果
    /// </summary>
    public string Result { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// 完成时间
    /// </summary>
    public System.DateTime CompletionTime { get; set; } = System.DateTime.Now;
}