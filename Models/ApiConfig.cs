// Models/ApiConfig.cs
namespace AIChatAssistant.Models;

public enum AiProvider
{
    OpenAI,
    CloudAPI,
    AzureOpenAI,
    Claude,
    Gemini
}

public class ApiConfig
{
    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-3.5-turbo";
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0.7;
    public AiProvider Provider { get; set; } = AiProvider.OpenAI;
    
    // Azure OpenAI 特定配置
    public string? AzureDeploymentId { get; set; } = null;
    public string? ApiVersion { get; set; } = "2023-05-15";
    
    // 保持所选供应商的ID
    public string? SelectedProviderId { get; set; } = null;
    
    // 从AiProviderConfig创建ApiConfig的构造函数
    public ApiConfig(AiProviderConfig providerConfig)
    {
        ApiKey = providerConfig.ApiKey;
        BaseUrl = providerConfig.BaseUrl;
        Model = providerConfig.DefaultModel;
        Provider = providerConfig.ProviderType;
        AzureDeploymentId = providerConfig.AzureDeploymentId;
        ApiVersion = providerConfig.ApiVersion;
    }
    
    // 默认构造函数
    public ApiConfig()
    {
    }
}
