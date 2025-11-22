// Models/ApiConfig.cs
namespace AIChatAssistant.Models;

public class ApiConfig
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-3.5-turbo";
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0.7;
}