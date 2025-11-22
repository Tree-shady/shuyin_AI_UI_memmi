// CalculatorPlugin.cs
using AIChatAssistant.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins
{
    /// <summary>
    /// 计算器插件示例
    /// 处理简单的数学计算请求
    /// </summary>
    public class CalculatorPlugin : PluginBase
    {
        // 触发关键词列表
        private readonly List<string> _triggerKeywords = new List<string>
        {
            "计算", "加", "减", "乘", "除", "等于", "求和", "总和", 
            "calculate", "plus", "minus", "multiply", "divide", "add", "subtract", "result"
        };
        
        // 数学运算符
        private readonly Dictionary<string, string> _operators = new Dictionary<string, string>
        {
            { "加", "+" },
            { "减", "-" },
            { "乘", "*" },
            { "除", "/" },
            { "plus", "+" },
            { "minus", "-" },
            { "multiply", "*" },
            { "divide", "/" },
            { "add", "+" },
            { "subtract", "-" }
        };

        /// <summary>
        /// 初始化插件信息
        /// </summary>
        /// <returns>插件信息</returns>
        protected override PluginInfo InitializePluginInfo()
        {
            return new PluginInfo
            {
                Name = "CalculatorPlugin",
                Version = "1.0.0",
                Description = "计算器插件，支持简单的数学计算",
                Author = "AIChatAssistant",
                TriggerWords = _triggerKeywords
            };
        }

        /// <summary>
        /// 处理消息的主要方法
        /// </summary>
        /// <param name="message">用户输入的消息</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>插件处理结果</returns>
        public override async Task<PluginResult> ProcessMessageAsync(string message, string conversationId)
        {            
            // 检查是否包含触发关键词
            if (!ContainsAnyKeyword(message, _triggerKeywords))
            {
                // 不包含触发关键词，不处理此消息
                return await Task.FromResult(new PluginResult { IsHandled = false });
            }

            try
            {
                // 尝试提取并计算数学表达式
                double result = CalculateExpression(message);
                
                return await Task.FromResult(new PluginResult
                    {
                        IsHandled = true,
                        Message = $"计算结果：{result}\n\n(计算由CalculatorPlugin插件提供)"
                    });
            }
            catch (Exception ex)
            {
                return await Task.FromResult(new PluginResult
                    {
                        IsHandled = true,
                        Message = $"计算失败：{ex.Message}\n请尝试输入格式如'3 + 5'或'10除以2'的计算表达式"
                    });
            }
        }

        /// <summary>
        /// 从消息中提取并计算数学表达式
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <returns>计算结果</returns>
        private double CalculateExpression(string message)
        {
            // 替换中文字符为对应的运算符
            string normalizedMessage = message.ToLower();
            foreach (var op in _operators)
            {
                normalizedMessage = normalizedMessage.Replace(op.Key, op.Value);
            }

            // 尝试匹配基本的数学表达式格式
            // 例如: "3 + 5" 或 "10 * 2"
            Match match = Regex.Match(normalizedMessage, @"([\d.]+)\s*([+\-*/])\s*([\d.]+)");
            
            if (match.Success)
            {
                double num1 = double.Parse(match.Groups[1].Value);
                string op = match.Groups[2].Value;
                double num2 = double.Parse(match.Groups[3].Value);
                
                return Calculate(num1, op, num2);
            }
            
            // 尝试匹配更复杂的表达式，例如求和、总和等
            if (normalizedMessage.Contains("求和") || normalizedMessage.Contains("总和") || 
                normalizedMessage.Contains("sum") || normalizedMessage.Contains("total"))
            {
                // 提取所有数字
                MatchCollection numMatches = Regex.Matches(normalizedMessage, @"[\d.]+");
                if (numMatches.Count >= 2)
                {
                    double sum = 0;
                    foreach (Match numMatch in numMatches)
                    {
                        sum += double.Parse(numMatch.Value);
                    }
                    return sum;
                }
            }
            
            throw new ArgumentException("无法识别的数学表达式格式");
        }

        /// <summary>
        /// 执行基本的数学运算
        /// </summary>
        /// <param name="num1">第一个操作数</param>
        /// <param name="op">运算符</param>
        /// <param name="num2">第二个操作数</param>
        /// <returns>计算结果</returns>
        private double Calculate(double num1, string op, double num2)
        {
            switch (op)
            {
                case "+":
                    return num1 + num2;
                case "-":
                    return num1 - num2;
                case "*":
                    return num1 * num2;
                case "/":
                    if (Math.Abs(num2) < 1e-10)
                        throw new DivideByZeroException("除数不能为零");
                    return num1 / num2;
                default:
                    throw new ArgumentException($"不支持的运算符: {op}");
            }
        }
        
        /// <summary>
        /// 检查消息是否包含任何关键词
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="keywords">关键词列表</param>
        /// <returns>是否包含</returns>
        private bool ContainsAnyKeyword(string message, List<string> keywords)
        {
            // 特殊处理：如果消息只包含数字和运算符，直接视为计算请求
            if (Regex.IsMatch(message, @"^[\d\s+\-*/().]+$"))
            {
                return true;
            }
            
            // 检查关键词
            foreach (string keyword in keywords)
            {
                if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}