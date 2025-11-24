// Services/ToolManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AIChatAssistant.Models;
using AIChatAssistant.Models.Tools;

namespace AIChatAssistant.Services;

public class ToolManager
{
    private readonly Dictionary<string, ITool> _registeredTools = new();

    public ToolManager()
    {
        RegisterBuiltInTools();
    }

    private void RegisterBuiltInTools()
    {
        RegisterTool(new WebSearchTool());
        RegisterTool(new CodeExecutorTool());
    }

    public void RegisterTool(ITool tool)
    {
        _registeredTools[tool.GetType().Name] = tool;
    }

    public async Task<ToolResult> ExecuteToolAsync(ToolDefinition toolDef, string input, ChatContext context)
    {
        try
        {
            if (_registeredTools.TryGetValue(toolDef.ExecuteMethod, out var tool))
            {
                return await tool.ExecuteAsync(input, context);
            }
            return new ToolResult { Success = false, ErrorMessage = $"工具未找到: {toolDef.ExecuteMethod}" };
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogError("ToolManager", $"执行工具失败: {toolDef.Name} - {ex.Message}");
            return new ToolResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public List<ToolDefinition> GetAvailableTools()
    {
        return _registeredTools.Values.Select(t => t.GetDefinition()).ToList();
    }
}