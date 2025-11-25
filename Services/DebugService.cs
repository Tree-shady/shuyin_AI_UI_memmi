using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIChatAssistant.Models;
using System.Collections.Concurrent;

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
        
        // 缩进字符串常量
        private const string IndentString = "  ";
        // 最大日志条目数
        private int _maxLogEntries = 1000;
        
        // 线程锁对象
        private readonly object _lock = new object();
        
        // 文件日志相关字段
        private bool _isFileLoggingEnabled = false;
        private string _logDirectory = string.Empty;
        private int _maxFileSizeInMB = 10;
        private int _maxFileCount = 5;
        private string _currentLogFilePath = string.Empty;
        private long _currentFileSize = 0;
        private StreamWriter _logWriter = null;
        private Thread _fileWriteThread = null;
        private BlockingCollection<string> _logQueue = null;
        
        // 日志级别过滤器
        private LogLevel _minLogLevel = LogLevel.Info;
        
        // 批处理相关
        private int _batchSize = 10; // 可配置的批处理大小
        private List<string> _logBatch = new List<string>();
        private int _batchCounter = 0;
        private readonly object _batchLock = new object();
        
        // 日志输出格式
        private AIChatAssistant.Models.LogOutputFormat _outputFormat = AIChatAssistant.Models.LogOutputFormat.Default;
        
        // 是否启用终端输出
        private bool _isConsoleOutputEnabled = true;

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
                {                    _maxLogEntries = value;
                    // 如果当前日志数量超过新的限制，清理旧日志
                    EnsureMaxEntries();
                }
            } 
        }
        
        /// <summary>
        /// 最小日志记录级别
        /// </summary>
        public LogLevel MinLogLevel
        { 
            get => _minLogLevel; 
            set 
            { 
                _minLogLevel = value;
                LogInfo("DebugService", $"日志级别已设置为: {value}");
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
            _logQueue = new BlockingCollection<string>();
            _logBatch = new List<string>(_batchSize);
            
            // 启动日志写入线程
            _fileWriteThread = new Thread(WriteLogsToFileLoop)
            {
                IsBackground = true,
                Name = "LogFileWriter"
            };
            _fileWriteThread.Start();
        }
        
        /// <summary>
        /// 应用日志配置
        /// </summary>
        /// <param name="config">日志配置对象</param>
        public void ApplyConfig(AIChatAssistant.Models.LogConfig config)
        {            if (config == null)
                throw new ArgumentNullException(nameof(config));
                
            lock (_lock)
            {                // 应用日志级别配置
                _minLogLevel = config.MinLogLevel;
                
                // 应用日志条目数限制
                _maxLogEntries = config.MaxLogEntries;
                EnsureMaxEntries();
                
                // 应用批处理大小
                _batchSize = config.BatchSize;
                lock (_batchLock)
                {                    // 调整批处理列表大小
                    _logBatch.Capacity = _batchSize;
                }
                
                // 应用输出格式
                _outputFormat = config.OutputFormat;
                
                // 应用终端输出配置
                _isConsoleOutputEnabled = config.EnableConsoleOutput;
                
                // 应用文件日志配置
                _logDirectory = config.LogDirectory;
                _maxFileSizeInMB = config.MaxFileSizeInMB;
                _maxFileCount = config.MaxFileCount;
                
                // 根据配置启用或禁用文件日志
                if (config.EnableFileLogging != _isFileLoggingEnabled)
                {                    if (config.EnableFileLogging)
                        EnableFileLogging(config.LogDirectory, _maxFileSizeInMB, _maxFileCount);
                    else
                        DisableFileLogging();
                }
                
                LogInfo("DebugService", "日志配置已应用");
            }
        }
        
        /// <summary>
        /// 获取当前日志配置
        /// </summary>
        /// <returns>当前日志配置对象</returns>
        public AIChatAssistant.Models.LogConfig GetCurrentConfig()
        {            lock (_lock)
            {                return new AIChatAssistant.Models.LogConfig
                {                    MinLogLevel = _minLogLevel,
                    MaxLogEntries = _maxLogEntries,
                    EnableFileLogging = _isFileLoggingEnabled,
                    LogDirectory = _logDirectory,
                    MaxFileSizeInMB = _maxFileSizeInMB,
                    MaxFileCount = _maxFileCount,
                    BatchSize = _batchSize,
                    OutputFormat = _outputFormat,
                    EnableConsoleOutput = _isConsoleOutputEnabled
                };
            }
        }
        
        ~DebugService()
        {            // 确保所有批处理日志都被写入
            FlushBatch();
            DisableFileLogging();
            _logQueue?.CompleteAdding();
            _fileWriteThread?.Join(1000);
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
        public void LogTrace(string source, string message)
        {            AddLog(new DebugLog(LogLevel.Trace, source, message));
        }
        
        public void LogDebug(string source, string message)
        {            AddLog(new DebugLog(LogLevel.Debug, source, message));
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
        {            AddLog(new DebugLog(LogLevel.Error, source, message));
        }
        
        public void LogFatal(string source, string message)
        {            AddLog(new DebugLog(LogLevel.Fatal, source, message));
        }

        /// <summary>
        /// 记录异常日志
        /// </summary>
        public void LogException(string source, string message, System.Exception exception)
        {            AddLog(new DebugLog(LogLevel.Error, source, message, exception));
        }
        
        public void LogFatalException(string source, string message, System.Exception exception)
        {            AddLog(new DebugLog(LogLevel.Fatal, source, message, exception));
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
        {            // 日志级别过滤 - 如果日志级别低于最小级别则直接返回
            if (log.Level < _minLogLevel)
                return;
            
            // 获取格式化的日志
            string formattedLog = GetFormattedLog(log);
            
            // 如果启用了终端输出，将日志输出到控制台
            if (_isConsoleOutputEnabled)
            {                try
                {                    // 根据日志级别设置控制台颜色
                    ConsoleColor originalColor = Console.ForegroundColor;
                    switch (log.Level)
                    {                        case LogLevel.Error:
                        case LogLevel.Fatal:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case LogLevel.Warning:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        case LogLevel.Info:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case LogLevel.Debug:
                        case LogLevel.Trace:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                    }
                    
                    // 输出格式化的日志到控制台
                    Console.WriteLine(formattedLog);
                    
                    // 恢复原始控制台颜色
                    Console.ForegroundColor = originalColor;
                }
                catch (Exception)
                {                    // 忽略控制台输出异常，确保应用程序正常运行
                }
            }
            
            // 先将日志添加到队列中用于文件写入
            if (_isFileLoggingEnabled && _logQueue != null && !_logQueue.IsAddingCompleted)
            {                try
                {                    // 使用批处理方式添加到队列
                    AddToBatch(formattedLog);
                }
                catch (Exception)
                {                    // 如果队列已满或已完成添加，则忽略
                }
            }
            
            lock (_lock)
            {                try
                {                    // 检查是否有打开的表单
                    if (Application.OpenForms.Count == 0)
                    {                        // 如果没有UI上下文，直接添加到集合
                        _logs.Insert(0, log);
                        EnsureMaxEntries();
                        return;
                    }
                    
                    // 在UI线程上执行添加操作
                    if (Application.MessageLoop)
                    {                        if (Application.OpenForms.Count > 0 && Application.OpenForms[0] != null)
                            Application.OpenForms[0].Invoke(() =>
                            {                                // 使用高效的插入方式
                                _logs.Insert(0, log);
                                EnsureMaxEntries();
                                // 触发日志添加事件
                                LogAdded?.Invoke(this, log);
                            });
                    }
                    else
                    {                        // 如果不是在UI线程，使用BeginInvoke异步执行
                        if (Application.OpenForms.Count > 0 && Application.OpenForms[0] != null)
                            Application.OpenForms[0].BeginInvoke(() =>
                            {                                _logs.Insert(0, log);
                                EnsureMaxEntries();
                                LogAdded?.Invoke(this, log);
                            });
                    }
                }
                catch (Exception ex)
                {                    // 记录添加日志失败的情况
                    Console.WriteLine($"添加日志失败: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 将日志添加到批处理队列
        /// </summary>
        private void AddToBatch(string logEntry)
        {            lock (_batchLock)
            {                _logBatch.Add(logEntry);
                _batchCounter++;
                
                // 当批处理达到阈值时，批量添加到队列
                if (_batchCounter >= _batchSize)
                {                    FlushBatch();
                }
            }
        }
        
        /// <summary>
        /// 刷新批处理队列
        /// </summary>
        private void FlushBatch()
        {            lock (_batchLock)
            {                if (_logBatch.Count == 0)
                    return;
                
                // 批量添加到队列
                foreach (var entry in _logBatch)
                {                    try
                    {                        _logQueue.Add(entry);
                    }
                    catch (Exception)
                    {                        // 忽略添加失败的情况
                    }
                }
                
                // 清空批处理队列
                _logBatch.Clear();
                _batchCounter = 0;
            }
        }
        
        /// <summary>
        /// 启用文件日志输出
        /// </summary>
        public void EnableFileLogging(string logDirectory, int maxFileSizeInMB = 10, int maxFileCount = 5)
        {            try
            {                lock (_lock)
                {
                    // 先禁用现有的文件日志
                    DisableFileLogging();
                    
                    // 设置文件日志参数
                    _logDirectory = logDirectory;
                    _maxFileSizeInMB = maxFileSizeInMB;
                    _maxFileCount = maxFileCount;
                    
                    // 确保日志目录存在
                    if (!Directory.Exists(logDirectory))
                    {                        Directory.CreateDirectory(logDirectory);
                    }
                    
                    // 创建新的日志文件
                    CreateNewLogFile();
                    
                    // 启用文件日志
                    _isFileLoggingEnabled = true;
                    
                    // 记录日志
                    LogInfo("DebugService", $"文件日志已启用，目录: {logDirectory}");
                }
            }
            catch (Exception ex)
            {                Console.WriteLine($"启用文件日志失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 禁用文件日志输出
        /// </summary>
        public void DisableFileLogging()
        {            lock (_lock)
            {                _isFileLoggingEnabled = false;
                
                // 刷新批处理队列中的所有日志
                FlushBatch();
                
                // 关闭并释放日志写入器
                if (_logWriter != null)
                {                    try
                    {                        _logWriter.Flush();
                        _logWriter.Close();
                    }
                    catch (Exception)
                    {                        // 忽略关闭时的异常
                    }
                    finally
                    {                        _logWriter = null;
                    }
                }
                
                _currentLogFilePath = string.Empty;
                _currentFileSize = 0;
            }
        }
        
        /// <summary>
        /// 创建新的日志文件
        /// </summary>
        private void CreateNewLogFile()
        {            // 生成文件名：AI_Chat_Assistant_yyyyMMdd_HHmmss.log
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"AI_Chat_Assistant_{timestamp}.log";
            _currentLogFilePath = Path.Combine(_logDirectory, fileName);
            
            // 创建日志写入器
            _logWriter = new StreamWriter(_currentLogFilePath, true, Encoding.UTF8);
            _currentFileSize = 0;
            
            // 写入日志文件头部
            _logWriter.WriteLine("==============================================");
            _logWriter.WriteLine($"AI 对话助手日志文件");
            _logWriter.WriteLine($"创建时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _logWriter.WriteLine("==============================================");
            _logWriter.Flush();
            
            // 清理旧日志文件
            CleanupOldLogFiles();
        }
        
        /// <summary>
        /// 清理旧的日志文件
        /// </summary>
        private void CleanupOldLogFiles()
        {            try
            {                // 获取所有日志文件并按创建时间排序
                var logFiles = Directory.GetFiles(_logDirectory, "AI_Chat_Assistant_*.log")
                    .Select(f => new { Path = f, CreationTime = File.GetCreationTime(f) })
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();
                
                // 删除超出数量限制的旧文件
                for (int i = _maxFileCount; i < logFiles.Count; i++)
                {                    try
                    {                        File.Delete(logFiles[i].Path);
                    }
                    catch (Exception)
                    {                        // 忽略删除失败的文件
                    }
                }
            }
            catch (Exception)
            {                // 忽略清理过程中的异常
            }
        }
        
        /// <summary>
        /// 日志写入循环线程方法
        /// </summary>
        private void WriteLogsToFileLoop()
        {            try
            {                foreach (var logEntry in _logQueue.GetConsumingEnumerable())
                {                    if (!_isFileLoggingEnabled || _logWriter == null)
                        continue;
                    
                    lock (_lock)
                    {                        try
                        {                            // 检查文件大小是否需要滚动
                            if (_currentFileSize >= _maxFileSizeInMB * 1024 * 1024)
                            {
                                CreateNewLogFile();
                            }
                            
                            // 写入日志条目
                            _logWriter.WriteLine(logEntry);
                            _logWriter.WriteLine(); // 空行分隔
                            _logWriter.Flush();
                            
                            // 更新文件大小估计
                            _currentFileSize += logEntry.Length * 2; // 粗略估计UTF-8编码后的大小
                        }
                        catch (Exception)
                        {                            // 忽略写入失败的异常
                        }
                    }
                }
            }
            catch (Exception)
            {                // 忽略线程异常
            }
        }
        
        /// <summary>
        /// 获取格式化的日志字符串
        /// </summary>
        /// <param name="log">日志对象</param>
        /// <returns>格式化后的日志字符串</returns>
        private string GetFormattedLog(DebugLog log)
        {            switch (_outputFormat)
            {                case LogOutputFormat.Detailed:
                    // 详细格式，包含所有信息
                    StringBuilder detailed = new StringBuilder();
                    detailed.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level,-7}] [{log.Source}] {log.Message}");
                    if (log.EventId > 0)
                    {                        detailed.AppendLine($"EventId: {log.EventId}");
                    }
                    if (log.Properties != null && log.Properties.Count > 0)
                    {
                        detailed.AppendLine("Properties:");
                        foreach (var prop in log.Properties)
                        {                            detailed.AppendLine($"  - {prop.Key}: {prop.Value}");
                        }
                    }
                    if (!string.IsNullOrEmpty(log.RequestParams))
                    {
                        detailed.AppendLine("Request:");
                        detailed.AppendLine(IndentString + log.RequestParams);
                    }
                    if (!string.IsNullOrEmpty(log.ResponseParams))
                    {
                        detailed.AppendLine("Response:");
                        detailed.AppendLine(IndentString + log.ResponseParams);
                    }
                    if (log.Exception != null)
                    {
                        detailed.AppendLine("Exception:");
                        detailed.AppendLine(IndentString + log.Exception.ToString());
                    }
                    return detailed.ToString();
                    
                case LogOutputFormat.Compact:
                    // 简洁格式，只包含基本信息
                    return $"[{log.Timestamp:HH:mm:ss}] [{log.Level}] {log.Message}";
                    
                case LogOutputFormat.Default:
                default:
                    // 默认格式
                    return log.ToString();
            }
        }

        /// <summary>
        /// 确保日志数量不超过最大值
        /// </summary>
        private void EnsureMaxEntries()
        {            if (_logs.Count <= _maxLogEntries)
                return;
                
            // 一次性移除多余的日志，减少UI更新次数
            int removeCount = _logs.Count - _maxLogEntries;
            
            // 优化：从末尾开始批量移除，避免每次移除都触发UI更新
            for (int i = 0; i < removeCount; i++)
            {                _logs.RemoveAt(_logs.Count - 1);
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
        {            if (string.IsNullOrEmpty(keyword))
                return _logs.ToList();
                
            lock (_lock)
            {                return _logs.Where(log => 
                    log.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    log.Source.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (log.Exception?.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }
        }
        

    }
}