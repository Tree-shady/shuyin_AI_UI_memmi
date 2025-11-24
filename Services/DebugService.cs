using AIChatAssistant.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System;
using System.Text;
using System.Text.Json;

namespace AIChatAssistant.Services
{
    /// <summary>
    /// 调试日志服务实现类
    /// </summary>
    public class DebugService : IDebugService
    {
        // 单例实例
        private static readonly DebugService _instance = new DebugService();
        
        // 日志集合
        private readonly ObservableCollection<DebugLog> _logs;
        
        // 最大日志条目数
        private int _maxLogEntries = 1000;
        
        // 线程锁对象
        private readonly object _lock = new object();

        /// <summary>
        /// 单例实例
        /// </summary>
        public static DebugService Instance => _instance;

        /// <summary>
        /// 所有日志的集合
        /// </summary>
        public ObservableCollection<DebugLog> Logs => _logs;

        /// <summary>
        /// 最大日志条目数限制
        /// </summary>
        public int MaxLogEntries 
        { 
            get => _maxLogEntries; 
            set 
            { 
                if (value > 0)
                {
                    _maxLogEntries = value;
                    // 如果当前日志数量超过新的限制，清理旧日志
                    EnsureMaxEntries();
                }
            } 
        }

        /// <summary>
        /// 日志添加事件
        /// </summary>
        public event System.EventHandler<DebugLog> LogAdded;

        /// <summary>
        /// 私有构造函数（单例模式）
        /// </summary>
        private DebugService()
        {
            _logs = new ObservableCollection<DebugLog>();
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public void LogInfo(string source, string message)
        {
            AddLog(new DebugLog(LogLevel.Info, source, message));
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public void LogDebug(string source, string message)
        {
            AddLog(new DebugLog(LogLevel.Debug, source, message));
        }
        
        /// <summary>
        /// 记录调试日志（带请求和响应参数）
        /// </summary>
        public void LogDebug(string source, string message, string requestParams, string responseParams)
        {
            AddLog(new DebugLog(LogLevel.Debug, source, message, requestParams, responseParams));
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public void LogWarning(string source, string message)
        {
            AddLog(new DebugLog(LogLevel.Warning, source, message));
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public void LogError(string source, string message)
        {
            AddLog(new DebugLog(LogLevel.Error, source, message));
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        public void LogException(string source, string message, System.Exception exception)
        {
            AddLog(new DebugLog(LogLevel.Error, source, message, exception));
        }

        /// <summary>
        /// 清空所有日志
        /// </summary>
        public void ClearLogs()
        {
            lock (_lock)
            {
                // 记录清空日志的操作
                var clearLog = new DebugLog(LogLevel.Info, "DebugService", "日志已清空");
                
                // 在UI线程上执行清空操作
                if (Application.MessageLoop)
                {
                    if (Application.OpenForms.Count > 0)
                        Application.OpenForms[0].Invoke(() =>
                        {
                            _logs.Clear();
                            // 添加清空日志的记录
                            _logs.Insert(0, clearLog);
                            // 触发日志添加事件
                            LogAdded?.Invoke(this, clearLog);
                        });
                }
                else
                {
                    if (Application.OpenForms.Count > 0)
                        Application.OpenForms[0].BeginInvoke(() =>
                        {
                            _logs.Clear();
                            // 添加清空日志的记录
                            _logs.Insert(0, clearLog);
                            // 触发日志添加事件
                            LogAdded?.Invoke(this, clearLog);
                        });
                }
            }
        }

        /// <summary>
        /// 根据日志级别过滤日志
        /// </summary>
        public List<DebugLog> GetFilteredLogs(List<LogLevel> levels)
        {
            lock (_lock)
            {
                return _logs.Where(log => levels.Contains(log.Level)).ToList();
            }
        }

        /// <summary>
        /// 添加日志到集合
        /// </summary>
        private void AddLog(DebugLog log)
        {
            lock (_lock)
            {
                try
                {
                    // 检查是否有打开的表单
                    if (Application.OpenForms.Count == 0)
                    {
                        // 如果没有UI上下文，直接添加到集合
                        _logs.Insert(0, log);
                        EnsureMaxEntries();
                        return;
                    }
                    
                    // 在UI线程上执行添加操作
                    if (Application.MessageLoop)
                    {
                        if (Application.OpenForms.Count > 0 && Application.OpenForms[0] != null)
                            Application.OpenForms[0].Invoke(() =>
                            {
                                _logs.Insert(0, log); // 新日志添加到开头
                                EnsureMaxEntries();
                                // 触发日志添加事件
                                LogAdded?.Invoke(this, log);
                            });
                    }
                    else
                    {
                        // 如果不是在UI线程，使用BeginInvoke异步执行
                        if (Application.OpenForms.Count > 0 && Application.OpenForms[0] != null)
                            Application.OpenForms[0].BeginInvoke(() =>
                            {
                                _logs.Insert(0, log);
                                EnsureMaxEntries();
                                LogAdded?.Invoke(this, log);
                            });
                    }
                }
                catch (Exception ex)
                {
                    // 记录添加日志失败的情况
                    Console.WriteLine($"添加日志失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 确保日志数量不超过最大值
        /// </summary>
        private void EnsureMaxEntries()
        {
            if (_logs.Count <= _maxLogEntries)
                return;
                
            // 一次性移除多余的日志，减少UI更新次数
            int removeCount = _logs.Count - _maxLogEntries;
            
            // 创建临时列表用于高效移除
            var logsToKeep = _logs.Take(_maxLogEntries).ToList();
            
            // 清空并重新添加，减少UI刷新次数
            _logs.Clear();
            foreach (var log in logsToKeep)
            {
                _logs.Add(log);
            }
        }
        
        /// <summary>
        /// 导出日志到文件
        /// </summary>
        /// <param name="filePath">导出文件路径</param>
        /// <param name="format">导出格式 (txt/json)</param>
        /// <returns>是否成功导出</returns>
        public bool ExportLogs(string filePath, string format = "txt")
        {
            try
            {
                List<DebugLog> logsCopy;
                
                // 复制日志集合，避免在导出过程中修改
                lock (_lock)
                {
                    logsCopy = _logs.ToList();
                }
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 根据格式导出
                if (!string.IsNullOrEmpty(format) && format.ToLower() == "json")
                {
                    // 导出为JSON格式
                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    string jsonContent = JsonSerializer.Serialize(logsCopy, jsonOptions);
                    File.WriteAllText(filePath, jsonContent);
                }
                else
                {
                    // 导出为文本格式
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("=== AI对话助手调试日志 ===");
                    sb.AppendLine($"导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    sb.AppendLine($"日志总数: {logsCopy.Count}");
                    sb.AppendLine("==================================\n");
                    
                    foreach (var log in logsCopy)
                    {
                        // 直接使用DebugLog的ToString()方法，保持格式一致性
                        sb.AppendLine(log.ToString());
                        sb.AppendLine(); // 空行分隔
                    }
                    
                    File.WriteAllText(filePath, sb.ToString());
                }
                
                // 记录导出日志操作
                LogInfo("DebugService", $"日志已成功导出到: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                // 记录导出失败的情况
                LogError("DebugService", $"导出日志失败: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 根据关键词搜索日志
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>匹配的日志列表</returns>
        public List<DebugLog> SearchLogs(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return _logs.ToList();
                
            lock (_lock)
            {
                return _logs.Where(log => 
                    log.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    log.Source.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (log.Exception?.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }
        }
    }
}