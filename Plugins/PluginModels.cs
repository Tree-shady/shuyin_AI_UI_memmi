// Plugins/PluginModels.cs
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins;

/// <summary>
/// 插件信息
/// </summary>
public class PluginInfo
{
    /// <summary>
    /// 插件ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 插件名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 插件描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 插件版本
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// 插件作者
    /// </summary>
    public string Author { get; set; } = string.Empty;
    
    /// <summary>
    /// 触发词列表
    /// </summary>
    public List<string> TriggerWords { get; set; } = new List<string>();
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 插件上下文，提供插件运行所需的环境和服务
/// </summary>
public class PluginContext
{
    /// <summary>
    /// AI服务
    /// </summary>
    public IAiService AiService { get; set; } = null!;
    
    /// <summary>
    /// 会话服务
    /// </summary>
    public IConversationService ConversationService { get; set; } = null!;
    
    /// <summary>
    /// 插件配置字典
    /// </summary>
    public Dictionary<string, string> Configuration { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// 插件处理结果
/// </summary>
public class PluginResult
{
    /// <summary>
    /// 是否处理成功
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// 处理结果消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否拦截消息（不继续交给AI处理）
    /// </summary>
    public bool IsHandled { get; set; }
    
    /// <summary>
    /// 错误信息（如果有）
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 创建成功的处理结果
    /// </summary>
    /// <param name="message">结果消息</param>
    /// <param name="isHandled">是否拦截消息</param>
    /// <returns>插件处理结果</returns>
    public static PluginResult Success(string message, bool isHandled = true)
    {
        return new PluginResult
        {
            IsSuccess = true,
            Message = message,
            IsHandled = isHandled
        };
    }
    
    /// <summary>
    /// 创建失败的处理结果
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    /// <returns>插件处理结果</returns>
    public static PluginResult Failure(string errorMessage)
    {
        return new PluginResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            IsHandled = false
        };
    }
    
    /// <summary>
    /// 创建未处理的结果
    /// </summary>
    /// <returns>插件处理结果</returns>
    public static PluginResult NotHandled()
    {
        return new PluginResult
        {
            IsSuccess = true,
            IsHandled = false
        };
    }
}