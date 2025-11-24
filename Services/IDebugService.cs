using AIChatAssistant.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AIChatAssistant.Services
{
    /// <summary>
    /// 调试日志服务接口
    /// </summary>
    public interface IDebugService
    {
        /// <summary>
        /// 所有日志的集合
        /// </summary>
        ObservableCollection<DebugLog> Logs { get; }

        /// <summary>
        /// 最大日志条目数限制
        /// </summary>
        int MaxLogEntries { get; set; }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="source">来源</param>
        /// <param name="message">消息</param>
        void LogInfo(string source, string message);

        /// <summary>
        /// 记录调试日志
        /// </summary>
        /// <param name="source">来源</param>
        /// <param name="message">消息</param>
        void LogDebug(string source, string message);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="source">来源</param>
        /// <param name="message">消息</param>
        void LogWarning(string source, string message);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="source">来源</param>
        /// <param name="message">消息</param>
        void LogError(string source, string message);

        /// <summary>
        /// 记录异常日志
        /// </summary>
        /// <param name="source">来源</param>
        /// <param name="message">消息</param>
        /// <param name="exception">异常</param>
        void LogException(string source, string message, System.Exception exception);

        /// <summary>
        /// 清空所有日志
        /// </summary>
        void ClearLogs();

        /// <summary>
        /// 根据日志级别过滤日志
        /// </summary>
        /// <param name="levels">要包含的日志级别</param>
        /// <returns>过滤后的日志列表</returns>
        List<DebugLog> GetFilteredLogs(List<LogLevel> levels);

        /// <summary>
        /// 日志添加事件
        /// </summary>
        event System.EventHandler<DebugLog> LogAdded;
    }
}