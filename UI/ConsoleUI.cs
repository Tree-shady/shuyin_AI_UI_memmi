// UI/ConsoleUI.cs
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;
using AIChatAssistant.Services;

namespace AIChatAssistant.UI;

public class ConsoleUI
{
    private readonly IAiService _aiService;
    private readonly IConversationService _conversationService;
    private readonly IPluginManager _pluginManager;

    public ConsoleUI(IAiService aiService, IConversationService conversationService, IPluginManager pluginManager)
    {
        _aiService = aiService;
        _conversationService = conversationService;
        _pluginManager = pluginManager;
        
        // 初始化时创建一个新会话
        _conversationService.CreateConversation();
    }

    public async Task RunAsync()
    {
        try
        {
            Console.WriteLine("=== AI对话助手 - 命令行模式 ===");
            Console.WriteLine("输入 'quit' 退出，'clear' 清空对话历史，'config' 配置API");
            Console.WriteLine("'new' 创建新对话，'list' 查看对话列表，'select id' 切换对话，'delete id' 删除对话");
            Console.WriteLine("'plugins' 显示已加载插件，'plugin [name] [enable/disable]' 启用/禁用插件");

            while (true)
            {
                try
                {
                    Console.Write("\n你: ");
                    var input = Console.ReadLine();

                    if (input == null) // 处理用户按下Ctrl+Z或Ctrl+C的情况
                        break;

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (input.ToLower() == "quit")
                        break;

                    if (input.ToLower() == "clear")
                    {
                        var currentConversation = _conversationService.GetActiveConversation();
                        if (currentConversation != null)
                        {
                            currentConversation.Messages.Clear();
                            Console.WriteLine("当前对话历史已清空");
                        }
                        else
                        {
                            Console.WriteLine("没有活动对话");
                        }
                        continue;
                    }
                    
                    if (input.ToLower() == "plugins")
                    {
                        ShowLoadedPlugins();
                        continue;
                    }
                    
                    if (input.ToLower().StartsWith("plugin "))
                    {
                        HandlePluginCommand(input.ToLower());
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

                    // 获取当前活动会话
                    var activeConversation = _conversationService.GetActiveConversation();
                    if (activeConversation == null)
                    {
                        Console.WriteLine("错误: 当前没有活动对话");
                        continue;
                    }
                    
                    // 发送消息并获取回复
                    var response = await _aiService.SendMessageAsync(input, activeConversation.Messages, activeConversation.Id);

                    loadingTask.Dispose(); // 停止加载动画

                    Console.WriteLine($"\rAI: {response}");

                    // 保存对话历史
                    var userMessage = new ChatMessage { Role = "user", Content = input };
                    var assistantMessage = new ChatMessage { Role = "assistant", Content = response };
                    
                    _conversationService.AddMessageToConversation(activeConversation.Id, userMessage);
                    _conversationService.AddMessageToConversation(activeConversation.Id, assistantMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n错误: {ex.Message}");
                    // 记录详细错误信息
                    Console.WriteLine($"详细错误: {ex.GetType().Name}");
                    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n致命错误: {ex.Message}");
            Console.WriteLine($"请按任意键退出...");
            Console.ReadKey();
            throw; // 重新抛出异常以便上层捕获
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
    
    private void ListConversations()
    {
        var conversations = _conversationService.GetAllConversations();
        if (conversations.Count == 0)
        {
            Console.WriteLine("没有对话记录");
            return;
        }
        
        Console.WriteLine("对话列表:");
        Console.WriteLine("---------------------------");
        foreach (var conversation in conversations)
        {
            var isActive = _conversationService.GetActiveConversation()?.Id == conversation.Id;
            var status = isActive ? "[活跃]" : "[非活跃]";
            Console.WriteLine($"{status} ID: {conversation.Id}");
            Console.WriteLine($"  标题: {conversation.Title}");
            Console.WriteLine($"  创建时间: {conversation.CreatedAt}");
            Console.WriteLine($"  最后修改: {conversation.LastModifiedAt}");
            Console.WriteLine($"  消息数: {conversation.Messages.Count}");
            Console.WriteLine("---------------------------");
        }
    }
    
    private void ShowLoadedPlugins()
    {
        Console.WriteLine("已加载的插件：");
        Console.WriteLine("{0,-30} {1,-10} {2}", "插件名称", "状态", "描述");
        Console.WriteLine(new string('-', 60));
        
        foreach (var plugin in _pluginManager.LoadedPlugins)
        {
            string status = plugin.IsEnabled ? "已启用" : "已禁用";
            Console.WriteLine("{0,-30} {1,-10} {2}", plugin.Info.Name, status, plugin.Info.Description);
        }
    }

    private void HandlePluginCommand(string command)
    {
        var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            Console.WriteLine("用法: plugin [插件名称] [enable/disable]");
            return;
        }
        
        string pluginName = parts[1];
        string action = parts[2];
        
        if (action == "enable")
        {
            bool success = _pluginManager.EnablePlugin(pluginName);
            Console.WriteLine(success ? $"插件 '{pluginName}' 已启用" : $"找不到插件 '{pluginName}'");
        }
        else if (action == "disable")
        {
            bool success = _pluginManager.DisablePlugin(pluginName);
            Console.WriteLine(success ? $"插件 '{pluginName}' 已禁用" : $"找不到插件 '{pluginName}'");
        }
        else
        {
            Console.WriteLine("操作无效，请使用 'enable' 或 'disable'");
        }
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