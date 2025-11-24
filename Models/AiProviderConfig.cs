// Models/AiProviderConfig.cs
namespace AIChatAssistant.Models;

public class AiProviderConfig
{
    public string Id { get; set; } = System.Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public AiProvider ProviderType { get; set; } = AiProvider.OpenAI;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = "";
    public string DefaultModel { get; set; } = "gpt-3.5-turbo";
    public bool IsDefault { get; set; } = false;
    
    // 特定供应商的配置
    public string? AzureDeploymentId { get; set; } = null;
    public string? ApiVersion { get; set; } = "2023-05-15";
    
    // 提供ApiConfig转换，用于兼容现有服务
    public ApiConfig ToApiConfig()
    {
        return new ApiConfig
        {
            ApiKey = ApiKey,
            BaseUrl = BaseUrl,
            Model = DefaultModel,
            Provider = ProviderType,
            AzureDeploymentId = AzureDeploymentId,
            ApiVersion = ApiVersion,
            SelectedProviderId = this.Id // 将当前供应商的ID设置为选中的供应商ID
        };
    }
}