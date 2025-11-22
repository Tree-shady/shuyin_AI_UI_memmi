// Plugins/PluginManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins;

/// <summary>
/// 插件管理器实现
/// </summary>
public class PluginManager : IPluginManager, IDisposable
{
    private readonly List<IPlugin> _plugins = new List<IPlugin>();
    private readonly List<Assembly> _loadedAssemblies = new List<Assembly>();
    private PluginContext? _context;
    private bool _isDisposed = false;
    
    /// <summary>
    /// 已加载的插件列表
    /// </summary>
    public IReadOnlyList<IPlugin> LoadedPlugins => _plugins.AsReadOnly();
    
    /// <summary>
    /// 初始化插件管理器
    /// </summary>
    /// <param name="context">插件上下文</param>
    public void Initialize(PluginContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    /// <summary>
    /// 加载插件目录中的所有插件
    /// </summary>
    /// <param name="pluginsDirectory">插件目录路径</param>
    public void LoadPlugins(string pluginsDirectory)
    {
        if (!Directory.Exists(pluginsDirectory))
        {
            Directory.CreateDirectory(pluginsDirectory);
            return;
        }
        
        try
        {
            var dllFiles = Directory.GetFiles(pluginsDirectory, "*.dll");
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    // 加载程序集
                    var assembly = Assembly.LoadFrom(dllFile);
                    _loadedAssemblies.Add(assembly);
                    
                    // 查找实现IPlugin接口的类型
                    var pluginTypes = assembly.GetTypes()?
                        .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract) ?? Enumerable.Empty<Type>();
                    
                    foreach (var pluginType in pluginTypes)
                    {
                        try
                        {
                            // 创建插件实例
                            var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                            
                            // 初始化插件
                            if (plugin != null)
                            {
                                if (_context != null)
                                {
                                    plugin.Initialize(_context);
                                }
                                else
                                {
                                    throw new InvalidOperationException("插件上下文未初始化");
                                }
                                
                                // 添加到插件列表
                                _plugins.Add(plugin);
                                Console.WriteLine($"成功加载插件: {plugin.Info.Name} (v{plugin.Info.Version})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"创建插件实例失败: {pluginType.Name}, 错误: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载插件DLL失败: {dllFile}, 错误: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载插件时发生错误: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 卸载指定插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    public bool UnloadPlugin(string pluginId)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Info.Id == pluginId);
        if (plugin == null)
            return false;
        
        try
        {
            plugin.Dispose();
            _plugins.Remove(plugin);
            Console.WriteLine($"插件 {plugin.Info.Name} 已卸载");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"卸载插件 {pluginId} 失败: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 启用或禁用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <param name="enabled">是否启用</param>
    public bool SetPluginEnabled(string pluginId, bool enabled)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Info.Id == pluginId);
        if (plugin == null)
            return false;
        
        plugin.Info.IsEnabled = enabled;
        plugin.IsEnabled = enabled;
        Console.WriteLine($"插件 {plugin.Info.Name} {(enabled ? "已启用" : "已禁用")}");
        return true;
    }
    
    /// <summary>
    /// 启用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    public bool EnablePlugin(string pluginId)
    {
        return SetPluginEnabled(pluginId, true);
    }
    
    /// <summary>
    /// 禁用插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    public bool DisablePlugin(string pluginId)
    {
        return SetPluginEnabled(pluginId, false);
    }
    
    /// <summary>
    /// 查找插件
    /// </summary>
    /// <param name="pluginId">插件ID</param>
    /// <returns>找到的插件或null</returns>
    public IPlugin? GetPlugin(string pluginId)
    {
        return _plugins.FirstOrDefault(p => p.Info.Id == pluginId);
    }
    
    /// <summary>
    /// 处理消息，分发给合适的插件
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="conversationId">会话ID</param>
    /// <returns>处理结果，如果没有插件处理则返回null</returns>
    public async Task<PluginResult> ProcessMessageAsync(string message, string conversationId)
    {
        if (string.IsNullOrEmpty(message))
            return PluginResult.NotHandled();
        
        // 过滤出已启用的插件
        var enabledPlugins = _plugins.Where(p => p != null && p.Info != null && p.Info.IsEnabled).ToList();
        
        // 首先尝试找到匹配触发词的插件
        foreach (var plugin in enabledPlugins)
        {
            if (plugin.Info != null && plugin.Info.TriggerWords != null && plugin.Info.TriggerWords.Any(trigger => message.Contains(trigger, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var result = await plugin.ProcessMessageAsync(message, conversationId);
                    if (result.IsHandled)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"插件 {plugin.Info.Name} 处理消息时发生错误: {ex.Message}");
                    continue;
                }
            }
        }
        
        // 如果没有匹配触发词的插件，让所有插件都有机会处理
        foreach (var plugin in enabledPlugins)
        {
            try
            {
                var result = await plugin.ProcessMessageAsync(message, conversationId);
                if (result.IsHandled)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插件 {plugin.Info.Name} 处理消息时发生错误: {ex.Message}");
                continue;
            }
        }
        
        // 没有插件处理消息
        return PluginResult.NotHandled();
    }
    
    /// <summary>
    /// 清理所有插件资源
    /// </summary>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            foreach (var plugin in _plugins)
            {
                try
                {
                    plugin.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"释放插件资源时发生错误: {ex.Message}");
                }
            }
            
            _plugins.Clear();
            _isDisposed = true;
        }
    }
}