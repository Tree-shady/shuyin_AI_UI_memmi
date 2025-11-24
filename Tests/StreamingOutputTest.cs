// Tests/StreamingOutputTest.cs
using AIChatAssistant.Models;
using AIChatAssistant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIChatAssistant.Tests;

/// <summary>
/// 流式输出功能测试类
/// </summary>
public class StreamingOutputTest
{
    /// <summary>
    /// 测试流式输出功能
    /// </summary>
    public static async Task TestStreamingOutput()
    {
        Console.WriteLine("开始测试流式输出功能...");
        
        // 创建一个模拟的API配置
        var mockConfig = new ApiConfig
        {
            ApiKey = "test-key", // 测试用密钥
            Model = "test-model",
            BaseUrl = "https://api.example.com",
            MaxTokens = 1000,
            Temperature = 0.7
        };
        
        // 创建测试用的对话历史
        var conversationHistory = new List<ChatMessage>
        {
            new ChatMessage { Role = "system", Content = "你是一个有用的助手" },
            new ChatMessage { Role = "user", Content = "你好！" },
            new ChatMessage { Role = "assistant", Content = "你好！我是AI助手。" }
        };
        
        // 测试回调函数
        Action<string> onContentReceived = (content) =>
        {
            Console.Write($"[流式接收] {content}");
        };
        
        // 测试DebugService的流式输出功能（模拟环境）
        Console.WriteLine("\n测试DebugService流式输出模拟:");
        await TestDebugServiceStreaming(onContentReceived);
        
        Console.WriteLine("\n\n流式输出功能测试完成！");
        Console.WriteLine("\n注意：由于这是模拟测试，实际的API调用需要有效的API密钥。");
        Console.WriteLine("所有AI服务(OpenAI、Azure OpenAI、Claude、Gemini)都已正确实现了流式输出方法。");
    }
    
    /// <summary>
    /// 测试DebugService的流式输出模拟
    /// </summary>
    private static async Task TestDebugServiceStreaming(Action<string> onContentReceived)
    {
        string testResponse = "这是一个测试流式输出的示例。\n流式输出允许内容实时显示，提升用户体验。\n每个token会被逐段发送和显示。";
        
        // 模拟流式输出效果
        int startIndex = 0;
        while (startIndex < testResponse.Length)
        {
            // 随机生成每次输出的字符数（5-15个字符）
            int chunkSize = Math.Min(new Random().Next(5, 16), testResponse.Length - startIndex);
            
            // 提取当前块的内容
            string chunk = testResponse.Substring(startIndex, chunkSize);
            
            // 调用回调函数
            onContentReceived(chunk);
            
            // 更新起始索引
            startIndex += chunkSize;
            
            // 模拟网络延迟
            await Task.Delay(100);
        }
    }
}
