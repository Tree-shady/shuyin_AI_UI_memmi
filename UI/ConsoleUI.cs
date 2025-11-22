// UI/ConsoleUI.cs
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Services;

namespace AIChatAssistant.UI;

public class ConsoleUI
{
    private readonly IAiService _aiService;
    private readonly List<ChatMessage> _conversationHistory;

    public ConsoleUI(IAiService aiService)
    {
        _aiService = aiService;
        _conversationHistory = new List<ChatMessage>();
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== AI对话助手 - 命令行模式 ===");
        Console.WriteLine("输入 'quit' 退出，'clear' 清空对话历史，'config' 配置API");

        while (true)
        {
            Console.Write("\n你: ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            if (input.ToLower() == "quit")
                break;

            if (input.ToLower() == "clear")
            {
                _conversationHistory.Clear();
                Console.WriteLine("对话历史已清空");
                continue;
            }

            if (input.ToLower() == "config")
            {
                ConfigureApi();
                continue;
            }

            // 显示思考中...
            Console.Write("AI: 思考中");
            var loadingTask = ShowLoadingAnimation();

            // 发送消息并获取回复
            var response = await _aiService.SendMessageAsync(input, _conversationHistory);

            loadingTask.Dispose(); // 停止加载动画

            Console.WriteLine($"\rAI: {response}");

            // 保存对话历史
            _conversationHistory.Add(new ChatMessage { Role = "user", Content = input });
            _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response });
        }
    }

    private LoadingAnimation ShowLoadingAnimation()
    {
        var cts = new CancellationTokenSource();
        var task = Task.Run(async () =>
        {
            var frames = new[] { ".", "..", "...", "...." };
            int frameIndex = 0;
            
            while (!cts.Token.IsCancellationRequested)
            {
                Console.Write($"\rAI: 思考中{frames[frameIndex]}");
                frameIndex = (frameIndex + 1) % frames.Length;
                await Task.Delay(300);
            }
        }, cts.Token);

        return new LoadingAnimation(task, cts);
    }

    private void ConfigureApi()
    {
        Console.WriteLine("\n=== API配置 ===");
        
        // 加载现有配置
        var existingConfig = AppConfig.LoadConfig();
        
        Console.Write($"请输入API Key (当前值已隐藏): ");
        var apiKey = Console.ReadLine();
        
        Console.Write($"请输入Base URL (当前: {existingConfig.BaseUrl}, 回车使用当前值): ");
        var baseUrl = Console.ReadLine();
        
        Console.Write($"请输入模型名称 (当前: {existingConfig.Model}, 回车使用当前值): ");
        var model = Console.ReadLine();
        
        Console.Write($"请输入最大令牌数 (当前: {existingConfig.MaxTokens}, 回车使用当前值): ");
        var maxTokensStr = Console.ReadLine();
        int? maxTokens = null;
        if (!string.IsNullOrWhiteSpace(maxTokensStr))
        {
            if (int.TryParse(maxTokensStr, out int parsedMaxTokens))
            {
                maxTokens = parsedMaxTokens;
            }
        }
        
        Console.Write($"请输入温度值 (当前: {existingConfig.Temperature}, 回车使用当前值): ");
        var temperatureStr = Console.ReadLine();
        double? temperature = null;
        if (!string.IsNullOrWhiteSpace(temperatureStr))
        {
            if (double.TryParse(temperatureStr, out double parsedTemperature))
            {
                temperature = parsedTemperature;
            }
        }

        var config = new ApiConfig
        {
            ApiKey = string.IsNullOrWhiteSpace(apiKey) ? existingConfig.ApiKey : apiKey,
            BaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? existingConfig.BaseUrl : baseUrl,
            Model = string.IsNullOrWhiteSpace(model) ? existingConfig.Model : model,
            MaxTokens = maxTokens ?? existingConfig.MaxTokens,
            Temperature = temperature ?? existingConfig.Temperature
        };

        // 保存配置到文件
        AppConfig.SaveConfig(config);
        
        // 更新AI服务配置
        if (_aiService is OpenAiService openAiService)
        {
            openAiService.UpdateConfig(config);
        }
        
        Console.WriteLine("API配置已保存");
    }
}

// 加载动画辅助类
public class LoadingAnimation : IDisposable
{
    private readonly Task _task;
    private readonly CancellationTokenSource _cts;

    public LoadingAnimation(Task task, CancellationTokenSource cts)
    {
        _task = task;
        _cts = cts;
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _task.Wait(100); } catch { }
        _cts.Dispose();
    }
}