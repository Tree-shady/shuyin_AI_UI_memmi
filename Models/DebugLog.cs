namespace AIChatAssistant.Models
{
    /// <summary>
    /// 调试日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Info,
        Debug,
        Warning,
        Error
    }

    /// <summary>
    /// 调试日志模型类
    /// </summary>
    public class DebugLog
    {
        /// <summary>
        /// 日志时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// 日志来源（组件或服务名称）
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 日志消息内容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 可选的异常信息
        /// </summary>
        public string Exception { get; set; }
        
        /// <summary>
        /// 请求参数信息
        /// </summary>
        public string RequestParams { get; set; }
        
        /// <summary>
        /// 响应参数信息
        /// </summary>
        public string ResponseParams { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DebugLog() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="source">日志来源</param>
        /// <param name="message">日志消息</param>
        public DebugLog(LogLevel level, string source, string message)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Source = source;
            Message = message;
        }
        
        /// <summary>
        /// 构造函数（带请求和响应参数）
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="source">日志来源</param>
        /// <param name="message">日志消息</param>
        /// <param name="requestParams">请求参数</param>
        /// <param name="responseParams">响应参数</param>
        public DebugLog(LogLevel level, string source, string message, string requestParams, string responseParams)
        {
            Timestamp = DateTime.Now;
            Level = level;
            Source = source;
            Message = message;
            RequestParams = requestParams;
            ResponseParams = responseParams;
        }

        /// <summary>
        /// 构造函数（带异常）
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="source">日志来源</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常信息</param>
        public DebugLog(LogLevel level, string source, string message, Exception exception)
            : this(level, source, message)
        {
            Exception = exception?.ToString();
        }

        /// <summary>
        /// 格式化日志为命令行样式字符串
        /// </summary>
        public override string ToString()
        {
            // 命令行样式：使用更简洁的格式，类似终端输出
            var result = $"[{Timestamp:HH:mm:ss}] {Level,-7} {Source}: {Message}";
            
            if (!string.IsNullOrEmpty(RequestParams))
            {
                // 请求参数使用缩进和前缀，类似命令行输出
                result += $"\n> REQ:\n{IndentString(RequestParams)}";
            }
                
            if (!string.IsNullOrEmpty(ResponseParams))
            {
                // 响应参数使用缩进和前缀，类似命令行输出
                result += $"\n> RES:\n{IndentString(ResponseParams)}";
            }
                
            if (!string.IsNullOrEmpty(Exception))
            {
                // 异常信息使用错误前缀
                result += $"\n> ERROR:\n{IndentString(Exception)}";
            }
                
            return result;
        }
        
        /// <summary>
        /// 缩进字符串，为多行文本添加前缀
        /// </summary>
        /// <param name="text">要缩进的文本</param>
        /// <returns>缩进后的文本</returns>
        private string IndentString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
                
            // 为每行添加缩进前缀，使输出更像命令行
            return string.Join("\n", text.Split(new[] { '\n' }, StringSplitOptions.None)
                .Select(line => $"  {line}"));
        }
    }
}