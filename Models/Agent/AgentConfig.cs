// Models/Agent/AgentConfig.cs
namespace AIChatAssistant.Models.Agent;

/// <summary>
/// 智能体配置
/// </summary>
public class AgentConfig
{
    /// <summary>
    /// 温度参数
    /// </summary>
    public float Temperature { get; set; } = 0.7f;
    
    /// <summary>
    /// 最大令牌数
    /// </summary>
    public int MaxTokens { get; set; } = 2000;
    
    /// <summary>
    /// 是否启用流式输出
    /// </summary>
    public bool EnableStreaming { get; set; } = true;
    
    /// <summary>
    /// 是否启用工具调用
    /// </summary>
    public bool EnableTools { get; set; } = true;
    
    /// <summary>
    /// 是否启用记忆
    /// </summary>
    public bool EnableMemory { get; set; } = true;
    
    /// <summary>
    /// 上下文窗口大小
    /// </summary>
    public int ContextWindowSize { get; set; } = 10;
    
    /// <summary>
    /// 自定义设置
    /// </summary>
    public System.Collections.Generic.Dictionary<string, object> CustomSettings { get; set; } = new System.Collections.Generic.Dictionary<string, object>();
}