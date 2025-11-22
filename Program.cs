// Program.cs
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Services;
using AIChatAssistant.UI;

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

        // 检查命令行参数
        if (args.Length > 0 && (args[0] == "--console" || args[0] == "-c"))
        {
            // 命令行模式
            RunConsoleMode(aiService).Wait();
        }
        else
        {
            // 图形界面模式
            RunGuiMode(aiService);
        }
    }

    static async Task RunConsoleMode(IAiService aiService)
    {
        var consoleUi = new ConsoleUI(aiService);
        await consoleUi.RunAsync();
    }

    static void RunGuiMode(IAiService aiService)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new WinFormUI(aiService));
    }
}