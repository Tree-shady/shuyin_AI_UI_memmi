// Services/ProviderConfigService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIChatAssistant.Models;

namespace AIChatAssistant.Services;

public interface IProviderConfigService
{
    List<AiProviderConfig> GetAllProviders();
    AiProviderConfig GetProviderById(string id);
    AiProviderConfig? GetDefaultProvider();
    void AddProvider(AiProviderConfig providerConfig);
    void UpdateProvider(AiProviderConfig providerConfig);
    void DeleteProvider(string id);
    void SetDefaultProvider(string id);
    void SaveProviders();
}

public class ProviderConfigService : IProviderConfigService
{
    private readonly string _configFilePath;
    private List<AiProviderConfig> _providers = new List<AiProviderConfig>();
    
    public ProviderConfigService(string? configFilePath = null)
    {
        // 获取应用程序数据目录，确保配置文件存储在安全位置
        string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AI_UI", "providers.json");
        _configFilePath = configFilePath ?? defaultPath;
        
        // 确保目录存在
        var directory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        LoadProviders();
        
        // 如果没有任何供应商配置，创建默认的提供商配置
        if (_providers.Count == 0)
        {
            AddDefaultProviders();
        }
    }
    
    private void AddDefaultProviders()
    {
        // 添加一些常用的默认提供商配置
        AddProvider(new AiProviderConfig
        {
            Id = Guid.NewGuid().ToString(),
            Name = "OpenAI",
            ProviderType = AiProvider.OpenAI,
            BaseUrl = "https://api.openai.com/v1",
            DefaultModel = "gpt-3.5-turbo",
            IsDefault = true
        });
        
        AddProvider(new AiProviderConfig
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Claude",
            ProviderType = AiProvider.Claude,
            BaseUrl = "https://api.anthropic.com",
            DefaultModel = "claude-3-opus-20240229",
            IsDefault = false
        });
        
        AddProvider(new AiProviderConfig
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Gemini",
            ProviderType = AiProvider.Gemini,
            BaseUrl = "https://generativelanguage.googleapis.com/v1",
            DefaultModel = "gemini-1.5-pro",
            IsDefault = false
        });
    }
    
    private void LoadProviders()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var jsonOptions = new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                
                var json = File.ReadAllText(_configFilePath);
                _providers = JsonSerializer.Deserialize<List<AiProviderConfig>>(json, jsonOptions) ?? new List<AiProviderConfig>();
            }
        }
        catch (Exception ex)
        {
            // 加载失败时记录错误并使用空列表
            Console.WriteLine($"加载供应商配置失败: {ex.Message}");
            _providers = new List<AiProviderConfig>();
        }
    }
    
    public void SaveProviders()
    {
        try
        {
            // 确保目录存在
            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var jsonOptions = new JsonSerializerOptions 
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
            
            var json = JsonSerializer.Serialize(_providers, jsonOptions);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            // 保存失败时抛出异常
            throw new Exception($"保存供应商配置失败: {ex.Message}", ex);
        }
    }
    
    public List<AiProviderConfig> GetAllProviders()
    {
        return new List<AiProviderConfig>(_providers);
    }
    
    public AiProviderConfig GetProviderById(string id)
    {
        var provider = _providers.Find(p => p.Id == id);
        if (provider == null)
        {
            throw new KeyNotFoundException($"Provider with id {id} not found");
        }
        return provider;
    }
    
    public AiProviderConfig? GetDefaultProvider()
    {
        return _providers.Find(p => p.IsDefault);
    }
    
    public void AddProvider(AiProviderConfig providerConfig)
    {
        // 如果设置为默认，取消其他默认设置
        if (providerConfig.IsDefault)
        {
            foreach (var provider in _providers)
            {
                provider.IsDefault = false;
            }
        }
        
        // 确保有唯一的Id
        if (string.IsNullOrEmpty(providerConfig.Id))
        {
            providerConfig.Id = System.Guid.NewGuid().ToString();
        }
        
        _providers.Add(providerConfig);
        SaveProviders();
        
        // 如果是默认供应商，同步更新config.json
        if (providerConfig.IsDefault)
        {
            try
            {
                // 获取API配置
                var apiConfig = providerConfig.ToApiConfig();
                // 确保SelectedProviderId设置为当前供应商ID
                apiConfig.SelectedProviderId = providerConfig.Id;
                // 使用AppConfig保存配置到config.json
                AIChatAssistant.Config.AppConfig.SaveConfig(apiConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"同步更新config.json失败: {ex.Message}");
            }
        }
    }
    
    public void UpdateProvider(AiProviderConfig providerConfig)
    {
        var index = _providers.FindIndex(p => p.Id == providerConfig.Id);
        if (index >= 0)
        {
            // 如果设置为默认，取消其他默认设置
            if (providerConfig.IsDefault)
            {
                foreach (var provider in _providers)
                {
                    provider.IsDefault = false;
                }
            }
            
            _providers[index] = providerConfig;
            SaveProviders();
            
            // 如果是默认供应商，同步更新config.json
            if (providerConfig.IsDefault)
            {
                try
                {
                    // 获取API配置
                    var apiConfig = providerConfig.ToApiConfig();
                    // 确保SelectedProviderId设置为当前供应商ID
                    apiConfig.SelectedProviderId = providerConfig.Id;
                    // 使用AppConfig保存配置到config.json
                    AIChatAssistant.Config.AppConfig.SaveConfig(apiConfig);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"同步更新config.json失败: {ex.Message}");
                }
            }
        }
    }
    
    public void DeleteProvider(string id)
    {
        var provider = _providers.Find(p => p.Id == id);
        if (provider != null)
        {
            bool wasDefault = provider.IsDefault;
            _providers.Remove(provider);
            
            // 如果删除的是默认提供商，设置第一个可用的为默认
            if (wasDefault && _providers.Count > 0)
            {
                _providers[0].IsDefault = true;
                
                // 同步更新config.json中的默认供应商配置
                try
                {
                    AIChatAssistant.Config.AppConfig.SaveConfig(_providers[0].ToApiConfig());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"同步更新config.json失败: {ex.Message}");
                }
            }
            
            SaveProviders();
        }
    }
    
    public void SetDefaultProvider(string id)
    {
        foreach (var provider in _providers)
        {
            provider.IsDefault = (provider.Id == id);
        }
        SaveProviders();
        
        // 同步更新config.json中的默认供应商配置
        var defaultProvider = GetDefaultProvider();
        if (defaultProvider != null)
        {
            try
            {
                // 获取API配置
                var apiConfig = defaultProvider.ToApiConfig();
                // 确保SelectedProviderId设置为当前供应商ID
                apiConfig.SelectedProviderId = id;
                // 使用AppConfig保存配置到config.json
                AIChatAssistant.Config.AppConfig.SaveConfig(apiConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"同步更新config.json失败: {ex.Message}");
            }
        }
    }
}