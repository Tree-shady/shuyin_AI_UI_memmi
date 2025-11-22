// Plugins/IPlugin.cs
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins;

/// <summary>
/// 插件接口，所有插件必须实现此接口
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// 插件信息
    /// </summary>
    PluginInfo Info { get; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// 初始化插件
    /// </summary>
    /// <param name="context">插件上下文</param>
    void Initialize(PluginContext context);
    
    /// <summary>
    /// 处理用户消息
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="conversationId">会话ID</param>
    /// <returns>处理结果</returns>
    Task<PluginResult> ProcessMessageAsync(string message, string conversationId);
    
    /// <summary>
    /// 清理插件资源
    /// </summary>
    void Dispose();
}