// WeatherPlugin.cs
using AIChatAssistant.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIChatAssistant.Plugins
{
    /// <summary>
    /// 天气查询插件示例
    /// 当用户提问包含天气相关关键词时，返回模拟的天气信息
    /// </summary>
    public class WeatherPlugin : PluginBase
    {
        // 触发关键词列表
        private readonly List<string> _triggerKeywords = new List<string>
        {
            "天气", "温度", "temperature", "weather", "气温", "下雨", "晴天", "多云"
        };
        
        // 支持的城市列表
        private readonly List<string> _supportedCities = new List<string>
        {
            "北京", "上海", "广州", "深圳", "杭州", "南京", "成都", "武汉",
            "beijing", "shanghai", "guangzhou", "shenzhen", "hangzhou", "nanjing", "chengdu", "wuhan"
        };

        /// <summary>
        /// 初始化插件信息
        /// </summary>
        /// <returns>插件信息</returns>
        protected override PluginInfo InitializePluginInfo()
        {
            return new PluginInfo
            {
                Name = "WeatherPlugin",
                Version = "1.0.0",
                Description = "天气查询插件，支持查询主要城市的天气信息",
                Author = "AIChatAssistant",
                TriggerWords = _triggerKeywords
            };
        }

        /// <summary>
        /// 处理消息的主要方法
        /// </summary>
        /// <param name="message">用户输入的消息</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>插件处理结果</returns>
        public override Task<PluginResult> ProcessMessageAsync(string message, string conversationId)
        {            
            // 检查是否包含触发关键词
            if (!ContainsAnyKeyword(message, _triggerKeywords))
            {
                // 不包含触发关键词，不处理此消息
                return Task.FromResult(new PluginResult { IsHandled = false });
            }

            // 尝试提取城市名称
            string city = ExtractCityFromMessage(message);
            
            if (string.IsNullOrEmpty(city))
            {
                // 未找到城市名称
                return Task.FromResult(new PluginResult
                {
                    IsHandled = true,
                    Message = "请告诉我您想查询哪个城市的天气信息。"
                });
            }

            // 模拟天气数据生成
            string weatherInfo = GenerateWeatherInfo(city);
            
            return Task.FromResult(new PluginResult
            {
                IsHandled = true,
                Message = weatherInfo
            });
        }

        /// <summary>
        /// 从消息中提取城市名称
        /// </summary>
        /// <param name="message">用户消息</param>
        /// <returns>提取的城市名称，如果未找到则返回null</returns>
        private string ExtractCityFromMessage(string message)
        {
            // 检查是否包含支持的城市名称
            foreach (string city in _supportedCities)
            {
                if (message.Contains(city, StringComparison.OrdinalIgnoreCase))
                {
                    return city;
                }
            }
            
            return string.Empty;
        }

        /// <summary>
        /// 生成模拟的天气信息
        /// </summary>
        /// <param name="city">城市名称</param>
        /// <returns>格式化的天气信息</returns>
        private string GenerateWeatherInfo(string city)
        {            
            // 生成随机温度（15-35度之间）
            Random rand = new Random();
            int temperature = rand.Next(15, 36);
            
            // 天气状况列表
            string[] weatherConditions = { "晴天", "多云", "阴天", "小雨", "中雨", "大雨", "雷阵雨", "晴间多云" };
            string condition = weatherConditions[rand.Next(weatherConditions.Length)];
            
            // 风向列表
            string[] windDirections = { "东风", "南风", "西风", "北风", "东北风", "东南风", "西北风", "西南风" };
            string windDirection = windDirections[rand.Next(windDirections.Length)];
            
            // 风力等级
            int windForce = rand.Next(1, 7);
            
            // 湿度
            int humidity = rand.Next(30, 91);
            
            return $"【{city}天气信息】\n" +
                   $"当前天气：{condition}\n" +
                   $"温度：{temperature}°C\n" +
                   $"风向：{windDirection} {windForce}级\n" +
                   $"湿度：{humidity}%\n" +
                   $"更新时间：{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}\n\n" +
                   "(天气数据由WeatherPlugin插件提供)";
        }
        
        /// <summary>
        /// 检查消息是否包含任何关键词
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="keywords">关键词列表</param>
        /// <returns>是否包含</returns>
        private bool ContainsAnyKeyword(string message, List<string> keywords)
        {
            foreach (string keyword in keywords)
            {
                if (message.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}