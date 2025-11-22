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
            RunConsoleMode(aiService, conversationService, pluginManager).Wait();
        }
        else
        {
            // 图形界面模式
            RunGuiMode(aiService, conversationService, pluginManager);
        }
    }

    static async Task RunConsoleMode(IAiService aiService, IConversationService conversationService, IPluginManager pluginManager)
    {
        var consoleUi = new ConsoleUI(aiService, conversationService, pluginManager);
        await consoleUi.RunAsync();
    }

    static void RunGuiMode(IAiService aiService, IConversationService conversationService, IPluginManager pluginManager)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WinFormUI(aiService, conversationService, pluginManager));
    }
}