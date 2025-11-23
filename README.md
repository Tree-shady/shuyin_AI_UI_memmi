# AI 聊天助手

一个功能完善的AI聊天助手应用，支持多会话管理，提供命令行和图形界面两种交互方式。

## 功能特性

- **多会话管理**：创建、查看、切换和删除不同的对话会话
- **双界面支持**：提供命令行界面(CLI)和Windows窗体图形界面(GUI)
- **AI交互**：与OpenAI或云API服务进行交互
- **配置管理**：自定义API参数，如最大令牌数和温度值
- **对话历史记录**：保存和管理对话历史
- **系统托盘功能**：支持最小化到系统托盘，保持程序在后台运行不占用任务栏空间

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
│   ├── ChatMessage.cs     # 聊天消息模型
│   └── Conversation.cs    # 会话模型
├── Services/              # 服务层
│   ├── IAiService.cs      # AI服务接口
│   ├── IConversationService.cs # 会话管理服务接口
│   └── TrayIconService.cs  # 系统托盘服务
├── UI/                    # 用户界面
│   ├── ConsoleUI.cs       # 命令行界面实现
│   └── WinFormUI.cs       # 图形界面实现
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

#### 界面元素
- 聊天显示区域：显示对话内容
- 消息输入框：输入问题
- 发送按钮：发送消息
- 会话管理下拉框：选择现有会话
- 新建对话按钮：创建新对话
- 管理对话按钮：打开会话管理窗口
- 清空对话按钮：清空当前对话历史
- 配置按钮：打开API配置窗口

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

应用支持以下API配置：

- **最大令牌数(MaxTokens)**：控制生成回复的长度
- **温度值(Temperature)**：控制回复的随机性（0.0-1.0）

配置会保存到本地配置文件中，下次启动时自动加载。

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

要添加新的AI服务提供商，需要：
1. 实现`IAiService`接口
2. 在`Program.cs`中注册新的服务实现

### 添加新功能

1. 在相应的模型中添加必要的数据结构
2. 在服务层实现业务逻辑
3. 更新UI层以支持新功能

## 注意事项

- 确保正确配置API参数以获得最佳体验
- 多会话管理功能允许在不同的对话上下文之间切换，保持对话的连贯性
- 图形界面仅在Windows系统上可用

## 许可证

MIT License

Copyright (c) 2025 Tree-shady

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.