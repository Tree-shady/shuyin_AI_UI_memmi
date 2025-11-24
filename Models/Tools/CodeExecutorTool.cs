// Models/Tools/CodeExecutorTool.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIChatAssistant.Models;
using AIChatAssistant.Models.Tools;

namespace AIChatAssistant.Models.Tools;

public class CodeExecutorTool : ITool
{
    public ToolDefinition GetDefinition()
    {
        return new ToolDefinition
        {
            Name = "代码执行",
            Description = "执行简单的C#代码片段",
            Type = ToolType.Code,
            TriggerKeywords = new List<string> { "计算", "代码", "执行", "运行" },
            ExecuteMethod = nameof(ExecuteAsync)
        };
    }

    public async Task<ToolResult> ExecuteAsync(string input, ChatContext context)
    {
        try
        {
            // 简化的代码执行实现（模拟）
            // 在实际应用中，这里应该使用安全的代码执行环境
            string result = "模拟代码执行结果"; // 模拟执行结果
            
            return new ToolResult
            {
                Success = true,
                ToolName = "代码执行",
                ToolId = "code_executor",
                Result = $"执行结果: {result}",
                ExecutionTime = TimeSpan.FromMilliseconds(300),
                Metadata = new Dictionary<string, object>
                {
                    ["execution_time"] = DateTime.Now,
                    ["result_type"] = "string"
                }
            };
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                Success = false,
                ToolName = "代码执行",
                ToolId = "code_executor",
                ErrorMessage = $"代码执行错误: {ex.Message}",
                ExecutionTime = TimeSpan.FromMilliseconds(100),
                Metadata = new Dictionary<string, object>
                {
                    ["error_type"] = "execution",
                    ["timestamp"] = DateTime.Now
                }
            };
        }
    }
}