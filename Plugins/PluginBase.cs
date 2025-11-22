// Plugins/PluginBase.cs
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins;

/// <summary>
/// 插件基类，提供通用功能实现
/// </summary>
public abstract class PluginBase : IPlugin
{
    /// <summary>
    /// 插件信息
    /// </summary>
    public PluginInfo Info { get; protected set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 插件上下文
    /// </summary>
    protected PluginContext Context { get; private set; } = new PluginContext();
    
    /// <summary>
    /// 构造函数
    /// </summary>
    protected PluginBase()
    {
        Info = InitializePluginInfo();
    }
    
    /// <summary>
    /// 初始化插件信息
    /// </summary>
    /// <returns>插件信息</returns>
    protected abstract PluginInfo InitializePluginInfo();
    
    /// <summary>
    /// 初始化插件
    /// </summary>
    /// <param name="context">插件上下文</param>
    public virtual void Initialize(PluginContext context)
    {
        // 确保Context不为null
        Context = context ?? new PluginContext();
        
        // 确保Configuration不为null
        if (Context.Configuration == null)
        {
            Context.Configuration = new Dictionary<string, string>();
        }
        
        OnInitialized();
    }
    
    /// <summary>
    /// 插件初始化后的回调
    /// </summary>
    protected virtual void OnInitialized()
    {
        // 由子类实现
    }
    
    /// <summary>
    /// 处理用户消息
    /// </summary>
    /// <param name="message">用户消息</param>
    /// <param name="conversationId">会话ID</param>
    /// <returns>处理结果</returns>
    public abstract Task<PluginResult> ProcessMessageAsync(string message, string conversationId);
    
    /// <summary>
    /// 清理插件资源
    /// </summary>
    public virtual void Dispose()
    {
        // 由子类实现
    }
    
    /// <summary>
    /// 检查消息是否包含任何触发词
    /// </summary>
    /// <param name="message">要检查的消息</param>
    /// <returns>是否包含触发词</returns>
    protected bool ContainsTriggerWord(string message)
    {
        if (string.IsNullOrEmpty(message) || Info?.TriggerWords == null || Info.TriggerWords.Count == 0)
            return false;
        
        return Info.TriggerWords.Any(trigger => message.Contains(trigger, System.StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值或默认值</returns>
    protected string GetConfigValue(string key, string? defaultValue = null)
    {
        if (Context?.Configuration != null && Context.Configuration.TryGetValue(key, out var value))
        {
            return value ?? string.Empty;
        }
        return defaultValue ?? string.Empty;
    }
    
    /// <summary>
    /// 获取布尔类型的配置值
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值或默认值</returns>
    protected bool GetConfigBool(string key, bool defaultValue = false)
    {
        var value = GetConfigValue(key);
        if (bool.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }
    
    /// <summary>
    /// 获取整数类型的配置值
    /// </summary>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值或默认值</returns>
    protected int GetConfigInt(string key, int defaultValue = 0)
    {
        var value = GetConfigValue(key);
        if (int.TryParse(value, out var result))
        {
            return result;
        }
        return defaultValue;
    }
}