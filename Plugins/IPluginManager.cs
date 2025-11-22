// Plugins/IPluginManager.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins;

/// <summary>
/// 插件管理器接口
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// 已加载的插件列表
    /// </summary>
    IReadOnlyList<IPlugin> LoadedPlugins { get; }
    
    /// <summary>
    /// 初始化插件管理器
    /// </summary>
    /// <param name="context">插件上下文</param>
    void Initialize(PluginContext context);
    
    /// <summary>
    /// 加载插件目录中的所有插件
    /// </summary>
    /// <param name="pluginsDirectory">插件目录路径</param>
    void LoadPlugins(string pluginsDirectory);
    
    /// <summary>
    /// 卸载指定插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    bool UnloadPlugin(string pluginId);
    
    /// <summary>
    /// 启用或禁用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <param name="enabled">是否启用</param>
    bool SetPluginEnabled(string pluginId, bool enabled);
    
    /// <summary>
    /// 启用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    bool EnablePlugin(string pluginId);
    
    /// <summary>
    /// 禁用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    bool DisablePlugin(string pluginId);
    
    /// <summary>
    /// 查找插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>找到的插件或null</returns>
    IPlugin? GetPlugin(string pluginId);
    
    /// <summary>
    /// 处理消息，分发给合适的插件
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="conversationId">会话ID</param>
    /// <returns>处理结果，如果没有插件处理则返回null</returns>
    Task<PluginResult> ProcessMessageAsync(string message, string conversationId);
    
    /// <summary>
    /// 清理所有插件资源
    /// </summary>
    void Dispose();
}