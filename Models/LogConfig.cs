using System;
using System.IO;

namespace AIChatAssistant.Models
{
    /// <summary>
    /// 日志系统配置类
    /// </summary>
    public class LogConfig
    {
        /// <summary>
        /// 最小日志记录级别
        /// </summary>
        public LogLevel MinLogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// 最大内存中日志条目数
        /// </summary>
        public int MaxLogEntries { get; set; } = 1000;

        /// <summary>
        /// 是否启用文件日志
        /// </summary>
        public bool EnableFileLogging { get; set; } = false;

        /// <summary>
        /// 日志文件目录
        /// </summary>
        public string LogDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AI_UI_memmi", "Logs");

        /// <summary>
        /// 单个日志文件最大大小（MB）
        /// </summary>
        public int MaxFileSizeInMB { get; set; } = 5;

        /// <summary>
        /// 最大保留日志文件数量
        /// </summary>
        public int MaxFileCount { get; set; } = 5;

        /// <summary>
        /// 日志批处理大小
        /// </summary>
        public int BatchSize { get; set; } = 50;

        /// <summary>
        /// 日志输出格式
        /// </summary>
        public LogOutputFormat OutputFormat { get; set; } = LogOutputFormat.Console;
        
        /// <summary>
        /// 是否启用控制台输出
        /// </summary>
        public bool EnableConsoleOutput { get; set; } = true;

        // 保持向后兼容性的属性
        public long MaxFileSizeBytes { get { return (long)MaxFileSizeInMB * 1024 * 1024; } set { MaxFileSizeInMB = (int)(value / (1024 * 1024)); } }
        public int MaxLogFiles { get { return MaxFileCount; } set { MaxFileCount = value; } }

        /// <summary>
        /// 获取默认配置
        /// </summary>
        public static LogConfig GetDefaultConfig()
        {
            return new LogConfig
            {
                EnableConsoleOutput = true
            };
        }

        /// <summary>
        /// 获取详细配置（记录更多信息）
        /// </summary>
        public static LogConfig GetVerboseConfig()
        {
            return new LogConfig
            {
                MinLogLevel = LogLevel.Trace,
                MaxLogEntries = 2000,
                EnableFileLogging = true,
                OutputFormat = LogOutputFormat.Json,
                EnableConsoleOutput = true
            };
        }

        /// <summary>
        /// 获取简洁配置（仅记录必要信息）
        /// </summary>
        public static LogConfig GetMinimalConfig()
        {
            return new LogConfig
            {
                MinLogLevel = LogLevel.Warning,
                MaxLogEntries = 500,
                EnableFileLogging = false,
                EnableConsoleOutput = true
            };
        }
    }

    /// <summary>
    /// 日志输出格式枚举
    /// </summary>
    public enum LogOutputFormat
    {
        /// <summary>
        /// 默认格式
        /// </summary>
        Default,
        /// <summary>
        /// 详细格式
        /// </summary>
        Detailed,
        /// <summary>
        /// 简洁格式
        /// </summary>
        Compact,
        /// <summary>
        /// JSON格式（机器可读）
        /// </summary>
        Json,
        /// <summary>
        /// 控制台格式（人类可读）
        /// </summary>
        Console
    }
}