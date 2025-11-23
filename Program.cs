// Program.cs
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;
using AIChatAssistant.Services;
using AIChatAssistant.UI;
using System.Windows.Forms;

namespace AIChatAssistant;

class Program
{
    private static System.Threading.ManualResetEvent _exitEvent = new System.Threading.ManualResetEvent(false);
    
    [STAThread]
    static void Main(string[] args)
    {
        // 加载配置
        var config = AppConfig.LoadConfig();

        // 创建AI服务
        IAiService aiService = new OpenAiService(config);
        // 或者使用云API服务: IAiService aiService = new CloudApiService(config);
        
        // 创建会话管理服务
        IConversationService conversationService = new ConversationService();
        
        // 创建插件管理器
        IPluginManager pluginManager = new PluginManager();
        
        // 初始化插件上下文
        var pluginContext = new PluginContext
        {
            AiService = aiService,
            ConversationService = conversationService,
            Configuration = new Dictionary<string, string>()
        };
        
        // 添加配置到Configuration字典
        if (config != null)
        {
            pluginContext.Configuration["ApiKey"] = config.ApiKey;
            pluginContext.Configuration["Model"] = config.Model;
            pluginContext.Configuration["BaseUrl"] = config.BaseUrl;
        }
        
        // 初始化插件管理器
        pluginManager.Initialize(pluginContext);
        
        // 设置插件管理器到AI服务
        aiService.SetPluginManager(pluginManager);
        
        // 加载插件目录中的插件
        string pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        pluginManager.LoadPlugins(pluginsDirectory);

        // 检查命令行参数
        if (args.Length > 0 && (args[0] == "--console" || args[0] == "-c"))
        {
            // 命令行模式
            RunConsoleMode(aiService, conversationService, pluginManager);
            // 等待命令行模式退出信号
            _exitEvent.WaitOne();
        }
        else
        {
            // 图形界面模式
            RunGuiMode(aiService, conversationService, pluginManager);
        }
    }
    
    static void RunConsoleMode(IAiService aiService, IConversationService conversationService, IPluginManager pluginManager)
    {
        // 最简单的命令行模式实现，直接在控制台上输出信息
        System.Console.WriteLine("===================================");
        System.Console.WriteLine("AI 对话助手 - 命令行模式");
        System.Console.WriteLine("===================================");
        System.Console.WriteLine("命令行模式已成功启动!");
        System.Console.WriteLine("为解决控制台输出问题，这是一个简化版本。");
        System.Console.WriteLine();
        System.Console.WriteLine("此版本提供基本的命令行交互功能。");
        System.Console.WriteLine();
        System.Console.WriteLine("输入 'exit' 或按 Ctrl+C 退出程序");
        System.Console.WriteLine("===================================");
        
        try
        {
            // 创建一个简单的交互循环
            while (true)
            {
                System.Console.Write("\n> ");
                string input = System.Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                
                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    System.Console.WriteLine("正在退出...");
                    break;
                }
                
                // 基本响应
                System.Console.WriteLine($"助手: 收到您的输入 '{input}'");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"错误: {ex.Message}");
            System.Console.WriteLine("按任意键继续...");
            System.Console.ReadKey();
        }
        finally
        {
            _exitEvent.Set();
        }
    }

    static void RunGuiMode(IAiService aiService, IConversationService conversationService, IPluginManager pluginManager)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        // 显示启动界面
        using (var splashScreen = new SplashScreen())
        {
            // 显示启动界面（非阻塞方式）
            splashScreen.Show();
            
            // 执行一些初始化操作，同时保持UI响应
            // 使用DoEvents来确保启动界面能够正常显示和更新
            for (int i = 0; i < 20; i++)
            {
                System.Threading.Thread.Sleep(100); // 模拟初始化工作
                Application.DoEvents(); // 处理UI消息，确保启动界面更新
            }
        }
        
        // 启动界面关闭后，再启动主界面应用程序
        Application.Run(new WinFormUI(aiService, conversationService, pluginManager));
    }
}