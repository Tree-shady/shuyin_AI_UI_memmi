// Models/Tools/ITool.cs
using System.Threading.Tasks;

namespace AIChatAssistant.Models.Tools;

public interface ITool
{
    ToolDefinition GetDefinition();
    Task<ToolResult> ExecuteAsync(string input, ChatContext context);
}