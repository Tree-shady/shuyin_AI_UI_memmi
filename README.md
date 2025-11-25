# AI 聊天助手

一个功能完善的AI聊天助手应用，支持多会话管理，提供命令行和图形界面两种交互方式。

目前支持同步交互模式。
用户发出消息后，应用会将消息发送给AI供应商的API，等待AI模型的响应。响应返回后，应用会将响应内容显示在用户界面上。故而会有延迟，用户需要等待一段时间才能看到AI的回复。后续用户可以继续发送消息，应用会将所有消息发送给AI供应商的API，等待AI模型的响应。响应返回后，应用会将响应内容显示在用户界面上。

目前已预留插件扩展系统框架，等待后续实现计算器和天气查询插件等功能扩展。流式输出模式正在开发中，未来将支持实时显示AI响应内容。

## 功能特性

- **多会话管理**：创建、查看、切换和删除不同的对话会话，支持跨会话数据持久化
- **双界面支持**：提供命令行界面(CLI)和Windows窗体图形界面(GUI)，满足不同使用场景
- **多供应商AI交互**：与OpenAI、Azure OpenAI、Claude、Gemini和云API服务进行无缝交互
- **高级供应商管理**：支持自定义、添加、编辑、删除和设置默认供应商配置，满足多样化的AI服务需求
- **细粒度配置管理**：自定义API参数，如API密钥、基础URL、模型选择、最大令牌数和温度值等
- **完整对话历史记录**：保存和管理对话历史，支持会话标题自动生成和手动修改
- **系统托盘功能**：支持最小化到系统托盘，保持程序在后台运行不占用任务栏空间，提供便捷的上下文菜单
- **现代化启动界面**：应用启动时显示的启动界面，提供平滑的动画效果和加载状态显示
- **插件扩展系统**：框架已预留，等待后续实现计算器和天气查询插件等功能扩展

## 技术栈

- **开发语言**：C#
- **框架**：.NET 8.0
- **UI框架**：Windows Forms
- **依赖管理**：NuGet

## 项目结构

```
AIChatAssistant/           # 主项目目录
├── Models/                # 数据模型
│   ├── ApiConfig.cs       # API配置模型
│   ├── AiProviderConfig.cs # AI供应商配置模型
│   ├── ChatMessage.cs     # 聊天消息模型
│   └── Conversation.cs    # 会话模型
├── Services/              # 服务层
│   ├── AiServiceFactory.cs # AI服务工厂
│   ├── AzureOpenAiService.cs # Azure OpenAI服务实现
│   ├── ClaudeService.cs   # Claude服务实现
│   ├── GeminiService.cs   # Gemini服务实现
│   ├── IAiService.cs      # AI服务接口
│   ├── IConversationService.cs # 会话管理服务接口
│   ├── ProviderConfigService.cs # 供应商配置服务
│   ├── ConversationService.cs # 会话管理服务实现
│   ├── SummaryService.cs  # 摘要服务
│   ├── ThemeManager.cs    # 主题管理服务
│   └── TrayIconService.cs  # 系统托盘服务
├── UI/                    # 用户界面
│   ├── ConsoleUI.cs       # 命令行界面实现
│   ├── WinFormUI.cs       # 图形界面实现
│   ├── MultiChatForm.cs   # 多聊天窗体
│   ├── ProviderManagerForm.cs # 供应商管理界面
│   ├── SplashScreen.cs    # 启动界面实现
│   └── ThemedForm.cs      # 主题化窗体基类
├── Plugins/               # 插件系统
│   ├── IPlugin.cs         # 插件接口
│   ├── IPluginManager.cs  # 插件管理器接口
│   ├── PluginBase.cs      # 插件基类
│   ├── PluginManager.cs   # 插件管理器实现
│   ├── PluginModels.cs    # 插件模型
│   ├── CalculatorPlugin.cs # 计算器插件
│   └── WeatherPlugin.cs   # 天气插件
├── config/                # 配置文件
│   └── AppConfig.cs       # 应用配置
├── Program.cs             # 程序入口
└── AIChatAssistant.csproj # 项目文件
```

## 安装指南

### 前置要求

- .NET 8.0 SDK
- Visual Studio 2022 或更高版本（推荐）

### 构建项目

1. 克隆或下载项目代码
2. 使用Visual Studio打开解决方案
3. 还原NuGet包
4. 构建解决方案

或者使用命令行：

```bash
cd AIChatAssistant
dotnet restore
dotnet build
```

## 使用说明

### 启动应用

**命令行界面模式**：
```bash
dotnet run
```

**图形界面模式**：
```bash
dotnet run --gui
```

### 命令行界面(CLI)使用

#### 基本命令
- `quit`：退出应用
- `clear`：清空当前对话历史
- `config`：配置API参数

#### 会话管理命令
- `new`：创建新对话
- `list`：查看所有对话列表
- `select id`：切换到指定ID的对话
- `delete id`：删除指定ID的对话

### 图形界面(GUI)使用

#### 启动界面
- 应用启动时会显示现代化的启动界面
- 启动界面包含应用标题、加载状态、进度条和版本信息
- 提供平滑的淡入淡出动画效果
- 圆角设计，简洁美观的视觉效果

#### 主界面元素
- 聊天显示区域：显示对话内容
- 消息输入框：输入问题
- 发送按钮：发送消息
- 会话管理下拉框：选择现有会话
- 新建对话按钮：创建新对话
- 管理对话按钮：打开会话管理窗口
- 清空对话按钮：清空当前对话历史
- 配置按钮：打开API配置窗口
- 供应商管理按钮：打开供应商管理窗口

#### 多聊天窗体
- 支持同时打开多个聊天窗口
- 独立管理不同的对话会话
- 可拖拽调整窗口位置和大小

#### 系统托盘功能
- **最小化到托盘**：点击窗口最小化按钮或关闭按钮时，程序会隐藏到系统托盘
- **从托盘恢复**：双击系统托盘图标或右键菜单选择"显示主窗口"可恢复程序窗口
- **托盘菜单**：右键点击托盘图标可访问以下功能：
  - 显示主窗口
  - 新建对话
  - 管理对话
  - 退出程序
- **托盘通知**：程序最小化到托盘时会显示通知提示

## 配置说明

## 配置说明

### API配置

应用支持以下API配置：

- **API密钥(ApiKey)**：访问AI服务所需的密钥
- **基础URL(BaseUrl)**：AI服务的API端点
- **模型(Model)**：使用的AI模型
- **最大令牌数(MaxTokens)**：控制生成回复的长度
- **温度值(Temperature)**：控制回复的随机性（0.0-1.0）
- **API版本(ApiVersion)**：（Azure OpenAI专用）API版本
- **部署ID(AzureDeploymentId)**：（Azure OpenAI专用）部署ID

### 供应商管理

应用提供了强大且灵活的供应商管理功能，允许用户全面控制AI服务的使用方式：

- **添加自定义供应商**：创建新的AI服务供应商配置，支持多种供应商类型
- **编辑现有供应商**：修改已有的供应商配置信息，灵活调整参数设置
- **删除供应商**：移除不需要的供应商配置，保持配置列表整洁
- **设置默认供应商**：指定默认使用的供应商配置，简化日常使用
- **供应商列表管理**：清晰展示所有已配置的供应商，支持快速查找和选择

#### 支持的供应商类型

| 供应商名称 | API端点示例 | 支持的模型 | 特殊配置项 |
|------------|-------------|------------|------------|
| OpenAI | https://api.openai.com/v1 | gpt-4o, gpt-4-turbo, gpt-3.5-turbo | ApiKey, BaseUrl, Model |
| Azure OpenAI | https://{your-resource}.openai.azure.com/ | gpt-4o, gpt-4-turbo, gpt-3.5-turbo | ApiKey, BaseUrl, ApiVersion, DeploymentId |
| Claude | https://api.anthropic.com/v1 | claude-3-opus-20240229, claude-3-sonnet-20240229 | ApiKey, Model |
| Gemini | https://generativelanguage.googleapis.com | gemini-1.5-pro, gemini-1.5-flash | ApiKey, Model |
| CloudAPI | https://api.example.com | 自定义模型 | ApiKey, BaseUrl, Model |

#### 供应商配置示例

**OpenAI配置示例：**
```json
{
  "ProviderId": "openai-default",
  "ProviderType": "OpenAI",
  "Name": "OpenAI默认",
  "ApiKey": "sk-...",
  "BaseUrl": "https://api.openai.com/v1",
  "Model": "gpt-4o"
}
```

**Azure OpenAI配置示例：**
```json
{
  "ProviderId": "azure-openai",
  "ProviderType": "AzureOpenAI",
  "Name": "Azure OpenAI",
  "ApiKey": "...",
  "BaseUrl": "https://your-resource.openai.azure.com/",
  "ApiVersion": "2024-02-01",
  "DeploymentId": "your-deployment-id"
}
```

**Claude配置示例：**
```json
{
  "ProviderId": "claude-default",
  "ProviderType": "Claude",
  "Name": "Claude默认",
  "ApiKey": "sk-ant-...",
  "Model": "claude-3-sonnet-20240229"
}
```

**Gemini配置示例：**
```json
{
  "ProviderId": "gemini-default",
  "ProviderType": "Gemini",
  "Name": "Gemini默认",
  "ApiKey": "AIzaSy...",
  "Model": "gemini-1.5-pro"
}
```

配置会自动保存到应用程序数据目录的配置文件中，下次启动时自动加载，确保用户的设置持久保存。

## 会话管理

### 会话数据结构

每个会话包含以下信息：
- 唯一标识符(ID)
- 会话标题
- 创建时间
- 消息列表

### 数据持久化

应用会自动保存所有会话数据，确保在重启应用后仍然可以访问之前的对话历史。

## 开发指南

### 扩展AI服务

要添加新的AI服务提供商，需要按照以下步骤进行：
1. 在`Models/ApiConfig.cs`中的`AiProvider`枚举中添加新的供应商类型
2. 实现`IAiService`接口，创建新的服务类（例如：`NewProviderService.cs`）
3. 在`AiServiceFactory.cs`中添加新的服务创建逻辑，确保工厂能够根据供应商类型创建正确的服务实例
4. 在`ProviderConfigService.cs`中添加新供应商的默认配置
5. 更新UI组件以支持新的供应商选项

### 供应商配置服务开发

如果需要扩展供应商配置功能，可以参考以下组件：

- **ProviderConfigService.cs**：负责管理供应商配置的保存、加载和默认配置设置
- **ProviderManagerForm.cs**：提供供应商管理的用户界面
- **AiProviderConfig.cs**：定义供应商配置的数据模型

### 添加新功能

1. 在相应的模型中添加必要的数据结构
2. 在服务层实现业务逻辑
3. 更新UI层以支持新功能
4. 确保所有新功能都遵循现有的设计模式和代码风格

### 供应商配置的存储位置

供应商配置默认存储在应用程序数据目录中，使用JSON格式进行序列化和反序列化，确保配置数据的安全性和完整性。

## 注意事项

- 确保正确配置API参数以获得最佳体验，特别是API密钥和基础URL
- 多会话管理功能允许在不同的对话上下文之间切换，保持对话的连贯性
- 图形界面仅在Windows系统上可用，命令行界面可在所有支持.NET的平台上使用
- **API密钥安全**：请妥善保管您的API密钥，避免在公共场合分享或提交到代码仓库
- **供应商配置最佳实践**：
  - 为不同的使用场景创建多个供应商配置
  - 为每个配置使用有意义的名称，便于识别
  - 定期检查和更新配置，特别是API密钥等敏感信息
- **API调用限制**：不同的AI供应商可能有不同的API调用限制和速率限制，请遵守各供应商的服务条款
- **供应商切换**：切换供应商时，请注意不同供应商的API特性和模型能力可能有所不同，这可能导致对话质量和响应格式的差异

## 许可证

MIT License


## 常见问题解答

### 供应商管理相关问题

**Q: 如何切换不同的AI供应商？**

A: 点击主界面上的"供应商管理"按钮，在供应商管理窗口中选择要使用的供应商，点击"设为默认"按钮。也可以在API配置界面中选择不同的供应商配置。

**Q: 我可以添加多少个供应商配置？**

A: 理论上没有限制，您可以根据需要添加任意数量的供应商配置。

**Q: 供应商配置文件存储在哪里？**

A: 供应商配置默认存储在应用程序数据目录中，确保数据的安全性和隐私保护。

**Q: 切换供应商后，现有会话会受到影响吗？**

A: 切换供应商会影响新的API调用，但不会改变现有的会话历史记录。新的消息将使用新选择的供应商生成。

**Q: 如何获取各供应商的API密钥？**

A: 请访问各供应商的官方网站：

- OpenAI: https://platform.openai.com/api-keys
- Azure OpenAI: Azure门户中的认知服务
- Claude: https://console.anthropic.com/keys
- Gemini: https://console.cloud.google.com/apis/credentials

### 配置故障排除

**Q: API调用失败怎么办？**

A: 检查以下几点：

1. 确保API密钥正确且有效
2. 验证基础URL配置正确
3. 确认选择的模型受该供应商支持
4. 检查网络连接是否正常
5. 对于Azure OpenAI，确保API版本和部署ID正确

**Q: 如何优化不同供应商的性能？**

A: 根据不同供应商的特点调整以下参数：

- OpenAI: 调整温度值和最大令牌数以平衡响应质量和速度
- Azure OpenAI: 确保使用最新的API版本以获得最佳性能
- Claude: 利用其长上下文能力，适当增加上下文窗口大小
- Gemini: 根据任务类型选择合适的模型（Pro或Flash）

**Q: 是否支持自定义的API端点？**

A: 是的，您可以在供应商配置中自定义基础URL，支持使用代理或本地部署的API服务。