// Services/AgentExecutor.cs
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AIChatAssistant.Models;
using AIChatAssistant.Models.Agent;
using AIChatAssistant.Models.Tools;

namespace AIChatAssistant.Services;

public class AgentExecutor
{
    private readonly AgentManager _agentManager;
    private readonly ToolManager _toolManager;
    private readonly MemoryService _memoryService;

    public AgentExecutor(AgentManager agentManager, ToolManager toolManager, MemoryService memoryService)
    {
        _agentManager = agentManager;
        _toolManager = toolManager;
        _memoryService = memoryService;
    }

    public async Task<AgentResponse> ExecuteAsync(string userInput, ChatContext context)
    {
        var agent = _agentManager.GetCurrentAgent();
        DebugService.Instance.LogDebug("AgentExecutor", $"智能体 {agent.Name} 处理输入: {userInput}");

        // 1. 预处理和意图识别
        var processedInput = PreprocessInput(userInput, agent);
        
        // 2. 工具调用决策
        var toolResults = await ExecuteToolsAsync(processedInput, agent, context);
        
        // 3. 构建增强的提示词
        var enhancedPrompt = BuildEnhancedPrompt(processedInput, agent, toolResults, context);
        
        // 4. 调用AI服务
        var aiResponse = await CallAiServiceAsync(enhancedPrompt, agent);
        
        // 5. 后处理和记忆存储
        var finalResponse = PostprocessResponse(aiResponse, agent, context);
        
        return finalResponse;
    }

    private async Task<List<ToolResult>> ExecuteToolsAsync(string input, AgentDefinition agent, ChatContext context)
    {
        var results = new List<ToolResult>();
        
        foreach (var tool in agent.Tools)
        {
            if (ShouldUseTool(input, tool))
            {
                var result = await _toolManager.ExecuteToolAsync(tool, input, context);
                results.Add(result);
            }
        }
        
        return results;
    }

    private string BuildEnhancedPrompt(string input, AgentDefinition agent, 
        List<ToolResult> toolResults, ChatContext context)
    {
        var promptBuilder = new StringBuilder();
        
        // 系统提示词
        promptBuilder.AppendLine(agent.SystemPrompt);
        
        // 添加工具结果
        if (toolResults.Any())
        {
            promptBuilder.AppendLine("工具执行结果:");
            promptBuilder.AppendLine(string.Join("\n", toolResults.Select(r => r.Result)));
        }
        
        // 添加上下文记忆
        if (agent.Capabilities?.CanRememberContext ?? false)
        {
            var memoryContext = _memoryService.RetrieveRelevantMemory(input, context);
            if (!string.IsNullOrEmpty(memoryContext))
            {
                promptBuilder.AppendLine("相关记忆:");
                promptBuilder.AppendLine(memoryContext);
            }
        }
        
        // 用户输入
        promptBuilder.AppendLine($"用户: {input}");
        
        return promptBuilder.ToString();
    }

    private bool ShouldUseTool(string input, ToolDefinition tool)
    {
        // 基于关键词、语义相似度等决策是否使用工具
        return input.ToLower().Contains(tool.TriggerKeywords.FirstOrDefault()?.ToLower() ?? "");
    }

    private string PreprocessInput(string userInput, AgentDefinition agent)
    {
        // 简单的预处理逻辑
        return userInput.Trim();
    }

    private async Task<string> CallAiServiceAsync(string prompt, AgentDefinition agent)
    {
        // 模拟AI服务调用
        // 实际实现中应该调用具体的AI服务提供商API
        await Task.Delay(100);
        return "这是一个模拟的AI响应。在实际应用中，这里会调用真实的AI服务。";
    }

    private AgentResponse PostprocessResponse(string aiResponse, AgentDefinition agent, ChatContext context)
    {
        // 简单实现，不使用SaveMemory
        
        return new AgentResponse
        {
            Content = aiResponse,
            AgentName = agent.Name
        };
    }
}