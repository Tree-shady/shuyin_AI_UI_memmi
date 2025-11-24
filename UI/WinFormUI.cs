// UI/WinFormUI.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using AIChatAssistant.Models;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Plugins;
using AIChatAssistant.Services;
using System.Collections.ObjectModel;

namespace AIChatAssistant.UI;

public partial class WinFormUI : Form
{
    private readonly IAiService _aiService;
    private readonly IConversationService _conversationService;
    private readonly IPluginManager _pluginManager;
    private RichTextBox? _chatBox;
    private TextBox? _inputBox;
    private Button? _sendButton;
    private Button? _clearButton;
    private Button? _newConversationButton;
    private Button? _listConversationsButton;
    private Button? _configButton;
    private Button? _pluginsButton;
    private Button? _providerManagerButton;
    private Button? _toggleDebugPanelButton;
    private ComboBox? _conversationComboBox;
    private TrayIconService? _trayIconService;
    
    // 调试面板相关控件
        private Panel? _debugPanel;
        private DataGridView? _debugLogGrid; // 保留以避免破坏其他代码
        private RichTextBox? _debugLogTextBox;
        private Button? _clearDebugLogButton;
        private Button? _exportLogButton;
        private TextBox? _searchLogTextBox;
        private Label? _logCountLabel;
        private CheckBox? _infoLogCheckbox;
        private CheckBox? _debugLogCheckbox;
        private CheckBox? _warningLogCheckbox;
        private CheckBox? _errorLogCheckbox;
    
    // 调试面板状态
    private bool _debugPanelVisible = false;
    private bool _isAnimating = false;
    private const int DEBUG_PANEL_WIDTH = 400;

    public WinFormUI(IAiService aiService, IConversationService conversationService, IPluginManager pluginManager)
    {
        _aiService = aiService;
        _conversationService = conversationService;
        _pluginManager = pluginManager;
        
        // 初始化时创建一个新会话
        _conversationService.CreateConversation();
        
        InitializeComponent();
        SetupUI();
        InitializeTrayIcon();
        UpdateConversationComboBox();
    }

    private void InitializeComponent()
    {
        Text = "AI对话助手";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        
        // 添加Resize事件处理
        Resize += new EventHandler(WinFormUI_Resize);
        FormClosing += WinFormUI_FormClosing;

        // 会话选择下拉框
        _conversationComboBox = new ComboBox
        {
            Location = new Point(10, 10),
            Size = new Size(550, 25),
            Font = new Font("Microsoft YaHei", 9),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // 聊天显示区域
        _chatBox = new RichTextBox
        {
            Location = new Point(10, 40),
            Size = new Size(760, 370),
            ReadOnly = true,
            BackColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        // 输入框
        _inputBox = new TextBox
        {
            Location = new Point(10, 420),
            Font = new Font("Microsoft YaHei", 10),
            Multiline = true,
            Size = new Size(600, 70), // 减小高度以增加与底部按钮的间距
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        // 发送按钮
        _sendButton = new Button
        {
            Location = new Point(620, 420),
            Size = new Size(150, 35),
            Text = "发送",
            BackColor = Color.DodgerBlue,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        // 清空对话按钮
        _clearButton = new Button
        {
            Location = new Point(620, 465),
            Size = new Size(150, 35),
            Text = "清空对话",
            BackColor = Color.LightGray,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        // 新建对话按钮
        _newConversationButton = new Button
        {
            Location = new Point(570, 10),
            Size = new Size(90, 25),
            Text = "新建对话",
            BackColor = Color.Green,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 9),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        // 对话列表按钮
        _listConversationsButton = new Button
        {
            Location = new Point(670, 10),
            Size = new Size(90, 25),
            Text = "管理对话",
            BackColor = Color.Purple,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 9),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        // API配置按钮
        _configButton = new Button
        {
            Location = new Point(620, 510),
            Size = new Size(150, 35),
            Text = "API配置",
            BackColor = Color.Orange,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        
        // 插件管理按钮
        _pluginsButton = new Button
        {
            Location = new Point(150, 510),
            Size = new Size(150, 35),
            Text = "插件管理",
            BackColor = Color.Purple,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };

        // 供应商管理按钮
        _providerManagerButton = new Button
        {
            Location = new Point(310, 510),
            Size = new Size(150, 35),
            Text = "供应商管理",
            BackColor = Color.Teal,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };
        
        // 调试面板切换按钮
        _toggleDebugPanelButton = new Button
        {
            Location = new Point(470, 510),
            Size = new Size(140, 35),
            Text = "显示调试",
            BackColor = Color.Indigo,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
            Visible = false // 隐藏调试按钮，默认不显示调试信息框
        };

        Controls.AddRange(new Control[] { 
            _conversationComboBox, _chatBox, _inputBox, _sendButton, _clearButton, 
            _newConversationButton, _listConversationsButton, _configButton, _pluginsButton, _providerManagerButton, _toggleDebugPanelButton 
        });
    }

    private void InitializeTrayIcon()
    {
        try
        {
            // 初始化托盘图标服务
            _trayIconService = new TrayIconService(this);
            
            // 订阅托盘事件
            _trayIconService.ShowMainFormRequested += (sender, e) => ShowMainForm();
            _trayIconService.ExitRequested += (sender, e) => ExitApplication();
            _trayIconService.NewConversationRequested += (sender, e) => CreateNewConversation();
            _trayIconService.ManageConversationsRequested += (sender, e) => OnManageConversationsRequested();
            
            // 默认不显示托盘图标，只在需要时显示
        }
        catch (Exception ex)
        {
            MessageBox.Show($"初始化系统托盘失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void OnManageConversationsRequested()
    {
        // 实现管理对话逻辑
        MessageBox.Show("管理对话功能尚未实现", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void SetupUI()
    {
        // 添加null检查以避免空引用异常
        if (_sendButton != null)
            _sendButton.Click += async (s, e) => await SendMessage();
            
        if (_providerManagerButton != null)
            _providerManagerButton.Click += (s, e) => ShowProviderManager();
        if (_clearButton != null)
            _clearButton.Click += (s, e) => ClearConversation();
        if (_configButton != null)
            _configButton.Click += (s, e) => ShowConfigDialog();
        if (_inputBox != null)
            _inputBox.KeyDown += async (s, e) =>
            {
                // Enter键发送消息
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    // 阻止默认的换行行为
                    e.SuppressKeyPress = true;
                    await SendMessage();
                }
                // Shift+Enter保持默认的换行行为
            };
        if (_newConversationButton != null)
            _newConversationButton.Click += (s, e) => CreateNewConversation();
        if (_listConversationsButton != null)
            _listConversationsButton.Click += (s, e) => ShowConversationManager();
        if (_conversationComboBox != null)
            _conversationComboBox.SelectedIndexChanged += (s, e) => OnConversationSelected();
        if (_pluginsButton != null)
            _pluginsButton.Click += (s, e) => ShowPluginsManager();
            
        if (_toggleDebugPanelButton != null)
            _toggleDebugPanelButton.Click += async (s, e) => await ToggleDebugPanel();
            
        // 订阅日志服务事件
        DebugService.Instance.LogAdded += OnLogAdded;
        
        // 添加欢迎消息
        AddMessageToChat("系统", "欢迎使用AI对话助手！", Color.Blue);
        
        // 初始化调试面板
        InitializeDebugPanel();
        
        // 记录应用启动日志
        DebugService.Instance.LogInfo("UI", "应用启动成功");
    }

    private async Task SendMessage()
    {
        // 默认使用流式发送消息
        await SendMessageWithStreamingAsync();
    }

    private async Task SendMessageWithStreamingAsync()
    {
        // 添加null检查
        if (_inputBox == null || _sendButton == null)
            return;

        var message = _inputBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(message))
            return;

        // 显示用户消息
        AddMessageToChat("你", message, Color.DarkGreen);
        _inputBox.Clear();

        // 只禁用发送按钮防止重复发送，保留输入框的可用性
        _sendButton.Enabled = false;
        
        // 获取当前活动会话
        var activeConversation = _conversationService.GetActiveConversation();
        if (activeConversation == null)
        {
            AddMessageToChat("系统", "错误: 当前没有活动对话", Color.Red);
            _sendButton.Enabled = true;
            _inputBox.Focus();
            return;
        }

        // 添加用户消息到对话历史
        var userMessage = new ChatMessage { Role = "user", Content = message };
        _conversationService.AddMessageToConversation(activeConversation.Id, userMessage);
        
        // 用于存储AI回复的完整内容
        StringBuilder fullResponse = new StringBuilder();
        // 用于跟踪AI消息是否已经添加到聊天框
        bool aiMessageStarted = false;
        // 保存AI消息在聊天框中的起始位置
        int aiMessageStartPos = 0;

        try
        {
            // 使用非流式API发送消息
            string aiResponse = await _aiService.SendMessageAsync(message, activeConversation.Messages, activeConversation.Id);
            
            // 在UI线程上更新UI
            if (InvokeRequired)
            {
                Invoke((Action)(() =>
                {
                    if (_chatBox != null)
                    {
                        _chatBox.SuspendLayout();
                        try
                        {
                            _chatBox.SelectionStart = _chatBox.TextLength;
                            _chatBox.SelectionColor = Color.DarkBlue;
                            _chatBox.AppendText("AI: " + aiResponse + "\n\n");
                            _chatBox.ScrollToCaret();
                            _chatBox.SelectionColor = _chatBox.ForeColor;
                        }
                        finally
                        {
                            _chatBox.ResumeLayout(true);
                        }
                    }
                    fullResponse.Append(aiResponse);
                }));
            }
            else
            {
                if (_chatBox != null)
                {
                    _chatBox.SuspendLayout();
                    try
                    {
                        _chatBox.SelectionStart = _chatBox.TextLength;
                        _chatBox.SelectionColor = Color.DarkBlue;
                        _chatBox.AppendText("AI: " + aiResponse + "\n\n");
                        _chatBox.ScrollToCaret();
                        _chatBox.SelectionColor = _chatBox.ForeColor;
                    }
                    finally
                    {
                        _chatBox.ResumeLayout(true);
                    }
                }
                fullResponse.Append(aiResponse);
            }

            // 保存完整的AI回复到对话历史
            var assistantMessage = new ChatMessage { Role = "assistant", Content = fullResponse.ToString() };
            _conversationService.AddMessageToConversation(activeConversation.Id, assistantMessage);
            
            // 更新对话下拉框
            UpdateConversationComboBox();
        }
        catch (Exception ex)
        {
            AddMessageToChat("系统", $"错误: {ex.Message}", Color.Red);
        }
        finally
        {
            // 恢复发送按钮的可用性
            _sendButton.Enabled = true;
            _inputBox.Focus(); // 确保输入框仍然保持焦点
        }
    }

    private void UpdateStreamingResponse(string chunk, StringBuilder fullResponse, ref bool aiMessageStarted, ref int aiMessageStartPos)
    {
        if (_chatBox == null)
            return;

        // 暂停布局更新，减少重绘次数
        _chatBox.SuspendLayout();
        try
        {
            // 如果是AI消息的第一个块
            if (!aiMessageStarted)
            {
                aiMessageStarted = true;
                aiMessageStartPos = _chatBox.TextLength;
                _chatBox.SelectionStart = aiMessageStartPos;
                _chatBox.SelectionColor = Color.DarkBlue;
                _chatBox.AppendText("AI: ");
            }
            else
            {
                // 对于后续块，确保从正确位置开始并使用正确的颜色
                _chatBox.SelectionStart = _chatBox.TextLength;
                _chatBox.SelectionColor = Color.DarkBlue;
            }

            // 添加新的文本块
            _chatBox.AppendText(chunk);
            fullResponse.Append(chunk);
            
            // 重置选择颜色，避免影响后续文本
            _chatBox.SelectionStart = _chatBox.TextLength;
            _chatBox.SelectionColor = _chatBox.ForeColor;
        }
        finally
        {
            // 恢复布局更新
            _chatBox.ResumeLayout(true);
            // 滚动到末尾，确保用户能看到最新内容
            _chatBox.ScrollToCaret();
        }
    }

    private void AddMessageToChat(string sender, string message, Color color)
    {
        // 添加null检查
        if (_chatBox == null)
            return;
            
        // 暂停布局更新，减少重绘次数
        _chatBox.SuspendLayout();
        
        try
        {
            _chatBox.SelectionStart = _chatBox.TextLength;
            _chatBox.SelectionColor = color;
            _chatBox.AppendText($"{sender}: {message}\n\n");
        }
        finally
        {
            // 恢复布局更新
            _chatBox.ResumeLayout(true);
            // 只在恢复布局后滚动到末尾，避免多次滚动操作
            _chatBox.ScrollToCaret();
        }
    }
    
    private void WinFormUI_FormClosing(object? sender, FormClosingEventArgs e)
    {
        // 如果是用户点击关闭按钮，显示选择框
        if (e.CloseReason == CloseReason.UserClosing && _trayIconService != null)
        {
            // 显示确认对话框，让用户选择是关闭软件还是最小化到托盘
            var result = MessageBox.Show(
                "请选择操作：\n\n最小化到系统托盘 - 点击'是'\n完全退出程序 - 点击'否'",
                "确认操作",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                // 用户选择最小化到托盘
                // 显示托盘图标
                _trayIconService.ShowTrayIcon();
                
                // 隐藏窗口而不是关闭
                this.Hide();
                
                // 显示通知
                _trayIconService.ShowNotification("AI对话助手", "程序已最小化到系统托盘", 2000);
                
                // 取消关闭事件
                e.Cancel = true;
            }
            else if (result == DialogResult.Cancel)
            {
                // 用户取消操作，保持程序打开
                e.Cancel = true;
            }
            // 如果用户选择DialogResult.No，则不取消关闭事件，程序会正常退出
        }
        else
        {
            // 其他情况（如任务管理器强制关闭）则正常退出
            _trayIconService?.Dispose();
        }
    }

    private void ShowMainForm()
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
    }
    
    private void ExitApplication()
    {
        // 释放托盘图标资源
        _trayIconService?.Dispose();
        Application.Exit();
    }

    private void ClearConversation()
    {
        // 添加null检查
        if (_chatBox == null)
            return;
            
        // 获取当前活动会话
        var activeConversation = _conversationService.GetActiveConversation();
        if (activeConversation != null)
        {
            _chatBox.Clear();
            activeConversation.Messages.Clear();
            AddMessageToChat("系统", "当前对话历史已清空", Color.Blue);
        }
    }

    private void InitializeDebugPanel()
        {
            // 创建调试面板 - 命令行样式
            _debugPanel = new Panel
            {
                Location = new Point(Width, 0),
                Size = new Size(DEBUG_PANEL_WIDTH, Height),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom // 锚定到顶部、右侧和底部
            };
            
            // 创建日志过滤选项面板 - 命令行样式
            var filterPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(DEBUG_PANEL_WIDTH - 20, 70), // 增加高度以容纳所有控件
                BackColor = Color.Black
            };
            
            // 创建搜索面板 - 命令行样式
            var searchPanel = new Panel
            {
                Location = new Point(10, 90), // 调整位置避免遮挡
                Size = new Size(DEBUG_PANEL_WIDTH - 20, 40),
                BackColor = Color.Black
            };
            
            // 创建日志级别复选框 - 命令行样式
            _infoLogCheckbox = new CheckBox
            {
                Location = new Point(10, 5),
                Text = "信息",
                Checked = true,
                ForeColor = Color.Cyan,
                Font = new Font("Consolas", 9),
                BackColor = Color.Transparent
            };
            
            _debugLogCheckbox = new CheckBox
            {
                Location = new Point(80, 5),
                Text = "调试",
                Checked = true,
                ForeColor = Color.Green,
                Font = new Font("Consolas", 9),
                BackColor = Color.Transparent
            };
            
            _warningLogCheckbox = new CheckBox
            {
                Location = new Point(150, 5),
                Text = "警告",
                Checked = true,
                ForeColor = Color.Yellow,
                Font = new Font("Consolas", 9),
                BackColor = Color.Transparent
            };
            
            _errorLogCheckbox = new CheckBox
            {
                Location = new Point(220, 5),
                Text = "错误",
                Checked = true,
                ForeColor = Color.Red,
                Font = new Font("Consolas", 9),
                BackColor = Color.Transparent
            };
            
            // 创建导出日志按钮 - 命令行样式（调整位置避免与清空按钮重叠）
            _exportLogButton = new Button
            {
                Location = new Point(10, 35),
                Size = new Size(70, 30),
                Text = "导出",
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                FlatStyle = FlatStyle.Flat
            };
            
            _exportLogButton.Click += (s, e) => ExportDebugLogs();
            
            // 创建清空日志按钮 - 命令行样式（调整位置避免遮挡）
            _clearDebugLogButton = new Button
            {
                Location = new Point(90, 35),
                Size = new Size(70, 30),
                Text = "清空",
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                FlatStyle = FlatStyle.Flat
            };
            
            _clearDebugLogButton.Click += (s, e) => 
            {
                DebugService.Instance.ClearLogs();
                UpdateLogDisplay();
            };
        
        // 为日志级别复选框添加事件处理
        _infoLogCheckbox.CheckedChanged += (s, e) => UpdateLogDisplay();
        _debugLogCheckbox.CheckedChanged += (s, e) => UpdateLogDisplay();
        _warningLogCheckbox.CheckedChanged += (s, e) => UpdateLogDisplay();
        _errorLogCheckbox.CheckedChanged += (s, e) => UpdateLogDisplay();
        
        // 创建搜索标签 - 命令行样式
        var searchLabel = new Label
        {
            Location = new Point(0, 10),
            Size = new Size(30, 20),
            Text = "搜索:",
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Consolas", 9)
        };
        
        // 创建搜索文本框 - 命令行样式
        _searchLogTextBox = new TextBox
        {
            Location = new Point(40, 8),
            Size = new Size(220, 20),
            PlaceholderText = "输入关键词搜索日志...",
            BackColor = Color.Black,
            ForeColor = Color.White,
            Font = new Font("Consolas", 9),
            BorderStyle = BorderStyle.FixedSingle
        };
        
        _searchLogTextBox.TextChanged += (s, e) => UpdateLogDisplay();
        
        // 创建日志计数标签 - 命令行样式
        _logCountLabel = new Label
        {
            Location = new Point(270, 10),
            Size = new Size(100, 20),
            Text = "日志: 0",
            TextAlign = ContentAlignment.MiddleRight,
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Consolas", 9)
        };
        
        // 添加搜索面板控件
        searchPanel.Controls.AddRange(new Control[] {
            searchLabel, _searchLogTextBox, _logCountLabel
        });
        
        // 添加过滤面板控件
        filterPanel.Controls.AddRange(new Control[] {
            _infoLogCheckbox, _debugLogCheckbox, _warningLogCheckbox, _errorLogCheckbox, _exportLogButton, _clearDebugLogButton
        });
        
        // 创建日志数据网格视图 - 命令行样式
        _debugLogGrid = new DataGridView
        {
            Location = new Point(10, 110),
            Size = new Size(DEBUG_PANEL_WIDTH - 20, Height - 130),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToOrderColumns = false,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            // 优化渲染
            EnableHeadersVisualStyles = false,
            // 设置列头样式 - 命令行风格
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            },
            // 设置行高和默认样式 - 命令行字体
            RowTemplate = {
                Height = 25,
                DefaultCellStyle = {
                    Font = new Font("Consolas", 9),
                    BackColor = Color.Black,
                    ForeColor = Color.White
                }
            },
            // 命令行背景色
            BackgroundColor = Color.Black,
            // 无边框风格
            BorderStyle = BorderStyle.None,
            // 列标题可见性
            ColumnHeadersVisible = true,
            // 启用列排序
        };
        
        // 添加列
        _debugLogGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Timestamp",
            HeaderText = "时间",
            DataPropertyName = "Timestamp",
            Width = 100,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "HH:mm:ss.fff"
            }
        });
        
        _debugLogGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Level",
            HeaderText = "级别",
            DataPropertyName = "Level",
            Width = 60
        });
        
        _debugLogGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Source",
            HeaderText = "来源",
            DataPropertyName = "Source",
            Width = 80
        });
        
        _debugLogGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Message",
            HeaderText = "消息",
            DataPropertyName = "Message",
            Width = 150
        });
        
        // 添加请求参数列
        _debugLogGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "RequestParams",
            HeaderText = "请求参数",
            DataPropertyName = "RequestParams",
            Width = 150
        });
        
        // 添加响应参数列
        _debugLogGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ResponseParams",
            HeaderText = "响应参数",
            DataPropertyName = "ResponseParams",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        
        // 设置数据源
        _debugLogGrid.DataSource = DebugService.Instance.Logs;
        
        // 添加日志级别颜色格式化事件 - 命令行终端风格
        _debugLogGrid.CellFormatting += (s, e) =>
        {
            // 先设置默认的黑底白字样式
            _debugLogGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Black;
            _debugLogGrid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.White;
            
            // 为不同级别设置不同颜色
            if (e.ColumnIndex == _debugLogGrid.Columns["Level"].Index && e.Value != null)
            {
                switch (e.Value.ToString())
                {
                    case "Info":
                        _debugLogGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Cyan;
                        break;
                    case "Debug":
                        _debugLogGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Green;
                        break;
                    case "Warning":
                        _debugLogGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Yellow;
                        break;
                    case "Error":
                        _debugLogGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.ForeColor = Color.Red;
                        // 错误行可以使用高亮背景
                        _debugLogGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(30, 0, 0);
                        break;
                }
            }
        };
        
        // 创建命令行风格的日志文本框（替换DataGridView）
        _debugLogTextBox = new RichTextBox
        {
            Location = new Point(10, 140), // 调整位置避免与其他控件重叠
            Size = new Size(DEBUG_PANEL_WIDTH - 20, Height - 160),
            ReadOnly = true,
            BackColor = Color.Black,
            ForeColor = Color.White,
            Font = new Font("Consolas", 10),
            BorderStyle = BorderStyle.None,
            Multiline = true,
            ScrollBars = RichTextBoxScrollBars.Both,
            WordWrap = false, // 命令行通常不自动换行
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom // 锚定到所有边缘
        };
        
        // 添加调试面板控件
        _debugPanel.Controls.Add(filterPanel);
        _debugPanel.Controls.Add(searchPanel);
        _debugPanel.Controls.Add(_debugLogTextBox);
        
        // 添加调试面板到主窗口，并确保它总是在最上层
        Controls.Add(_debugPanel);
        _debugPanel.BringToFront(); // 确保调试面板显示在最前面，不被其他控件遮挡
        
        // 初始添加一条测试日志
        DebugService.Instance.LogInfo("UI", "调试面板初始化完成");
        DebugService.Instance.LogDebug("UI", "应用启动，初始化调试功能");
    }
    
    private async Task ToggleDebugPanel()
    {        // 防止动画重复触发
        if (_isAnimating || _debugPanel == null || _toggleDebugPanelButton == null || _chatBox == null)
            return;
            
        _isAnimating = true;
        // 强制保持调试面板隐藏，不允许切换显示状态
        _debugPanelVisible = false;
        _debugPanel.Visible = false;
        _isAnimating = false;
        return; // 直接返回，不执行任何动画或显示逻辑
        
        // 平滑动画参数
        const int animationSpeed = 5; // 每步移动的像素数
        const int animationDelay = 5; // 每步延迟的毫秒数
        
        // 保存原始锚定设置
        AnchorStyles originalChatBoxAnchor = _chatBox.Anchor;
        
        // 临时移除Right锚定，避免与手动调整宽度冲突
        _chatBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
        
        // 确保调试面板在动画开始时可见
        if (_debugPanelVisible)
        {
            _debugPanel.Visible = true;
            _debugPanel.BringToFront(); // 确保调试面板显示在最前面
            _toggleDebugPanelButton.Text = "隐藏调试";
            // 设置调试面板位置和大小 - 调整Top位置，避免覆盖顶部按钮区域
            _debugPanel.Size = new Size(DEBUG_PANEL_WIDTH, Height - 40); // 留出顶部40像素空间给按钮
            _debugPanel.Location = new Point(Width - DEBUG_PANEL_WIDTH, 40); // 从顶部40像素开始显示
            
            // 确保顶部按钮保持在最前面
            if (_newConversationButton != null)
                _newConversationButton.BringToFront();
            if (_listConversationsButton != null)
                _listConversationsButton.BringToFront();
            if (_conversationComboBox != null)
                _conversationComboBox.BringToFront();
        }
        else
        {
            _toggleDebugPanelButton.Text = "显示调试";
        }
        
        // 定义目标宽度变量 - 确保在整个方法作用域内可见
        int targetChatBoxWidth = 0;
        int targetDebugPanelLeft = 0;
        
        try
        {
            // 计算目标位置和大小 - 使用固定的边距值
            const int LEFT_MARGIN = 20; // 左侧边距
            const int TOP_BUTTON_HEIGHT = 40; // 顶部按钮区域高度
            
            // 调整目标聊天框宽度 - 确保不影响顶部按钮区域
            targetChatBoxWidth = _debugPanelVisible ? Width - DEBUG_PANEL_WIDTH - LEFT_MARGIN : Width - LEFT_MARGIN;
            
            // 确保调试面板大小适配
            int targetDebugPanelHeight = _debugPanelVisible ? Height - TOP_BUTTON_HEIGHT : 0;
            
            int currentChatBoxWidth = _chatBox.Width;
            int step = _debugPanelVisible ? -animationSpeed : animationSpeed;
            targetDebugPanelLeft = _debugPanelVisible ? Width - DEBUG_PANEL_WIDTH : Width;
            int currentDebugPanelLeft = _debugPanel.Left;
            int panelStep = _debugPanelVisible ? -animationSpeed : animationSpeed;
            
            // 执行平滑动画
            while ((_debugPanelVisible && _chatBox.Width > targetChatBoxWidth) || 
                   (!_debugPanelVisible && _chatBox.Width < targetChatBoxWidth))
            {
                // 计算新宽度，确保不会超过目标值
                int newChatBoxWidth = _chatBox.Width + step;
                if (_debugPanelVisible && newChatBoxWidth < targetChatBoxWidth)
                    newChatBoxWidth = targetChatBoxWidth;
                else if (!_debugPanelVisible && newChatBoxWidth > targetChatBoxWidth)
                    newChatBoxWidth = targetChatBoxWidth;
                
                // 计算调试面板新位置和大小
                int newPanelLeft = _debugPanel.Left + panelStep;
                if (_debugPanelVisible && newPanelLeft < targetDebugPanelLeft)
                    newPanelLeft = targetDebugPanelLeft;
                else if (!_debugPanelVisible && newPanelLeft > targetDebugPanelLeft)
                    newPanelLeft = targetDebugPanelLeft;
                
                // 更新聊天框宽度 - 只调整宽度，不影响位置
                _chatBox.Width = newChatBoxWidth;
                
                // 更新调试面板位置和大小 - 确保不覆盖顶部按钮
                _debugPanel.Left = newPanelLeft;
                if (_debugPanelVisible)
                {
                    _debugPanel.Size = new Size(DEBUG_PANEL_WIDTH, targetDebugPanelHeight);
                    _debugPanel.Top = TOP_BUTTON_HEIGHT;
                }
                
                // 延迟一小段时间以创建动画效果
                await Task.Delay(animationDelay);
            }
            
            // 确保最终状态正确
            _chatBox.Width = targetChatBoxWidth;
            _debugPanel.Left = targetDebugPanelLeft;
            if (_debugPanelVisible)
            {
                _debugPanel.Size = new Size(DEBUG_PANEL_WIDTH, targetDebugPanelHeight);
                _debugPanel.Top = TOP_BUTTON_HEIGHT;
            }
            
            // 动画结束后，根据调试面板状态设置适当的锚定
            if (_debugPanelVisible)
            {
                // 显示调试面板时，聊天框不锚定右侧
                _chatBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
                // 调试面板锚定右侧和顶部(从按钮下方)/底部，避免覆盖顶部按钮
                _debugPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                // 再次确保顶部按钮保持在最前面
                if (_newConversationButton != null)
                    _newConversationButton.BringToFront();
                if (_listConversationsButton != null)
                    _listConversationsButton.BringToFront();
                if (_conversationComboBox != null)
                    _conversationComboBox.BringToFront();
            }
            else
            {
                // 隐藏调试面板时，恢复聊天框的所有锚定
                _chatBox.Anchor = originalChatBoxAnchor;
                _debugPanel.Visible = false;
            }
            
            // 记录调试信息
            DebugService.Instance.LogDebug("UI", $"调试面板状态: {(DebugPanelVisible ? "显示" : "隐藏")}");
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogError("UI", $"调试面板动画出错: {ex.Message}");
            // 出错时确保面板状态一致
            _debugPanel.Visible = _debugPanelVisible;
            _chatBox.Width = targetChatBoxWidth;
        }
        finally
        {
            _isAnimating = false;
        }
    }
    
    // 属性用于访问调试面板可见状态
    public bool DebugPanelVisible => _debugPanelVisible;
    
    private void OnLogAdded(object sender, DebugLog log)
    {
        // 异步更新日志显示，避免阻塞UI
        Task.Run(() => UpdateLogDisplay());
    }
    
    private void UpdateLogDisplay()
        {
            if (_debugLogTextBox == null || _infoLogCheckbox == null || _debugLogCheckbox == null || 
                _warningLogCheckbox == null || _errorLogCheckbox == null || _searchLogTextBox == null || _logCountLabel == null)
                return;
            
        // 确保在UI线程上执行
        if (_debugLogTextBox.InvokeRequired)
        {
            _debugLogTextBox.Invoke(new Action(UpdateLogDisplay));
            return;
        }
        
        try
        {
            // 获取过滤后的日志
            var filteredLogs = DebugService.Instance.Logs.Where(log => 
                (log.Level == LogLevel.Info && _infoLogCheckbox.Checked) ||
                (log.Level == LogLevel.Debug && _debugLogCheckbox.Checked) ||
                (log.Level == LogLevel.Warning && _warningLogCheckbox.Checked) ||
                (log.Level == LogLevel.Error && _errorLogCheckbox.Checked)
            ).ToList();
            
            // 应用搜索过滤
            string searchTerm = _searchLogTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                filteredLogs = filteredLogs.Where(log => 
                    (log.Message != null && log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (log.Source != null && log.Source.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (log.Timestamp.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (log.RequestParams != null && log.RequestParams.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (log.ResponseParams != null && log.ResponseParams.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }
            
            // 保存当前滚动位置
            int currentScrollPos = _debugLogTextBox.SelectionStart;
            bool wasAtEnd = currentScrollPos == _debugLogTextBox.Text.Length;
            
            // 清空文本框
            _debugLogTextBox.Clear();
            
            // 按顺序添加彩色日志到文本框
            foreach (var log in filteredLogs)
            {
                AddColoredLogToTextBox(log);
            }
            
            // 更新日志计数
            _logCountLabel.Text = $"日志: {filteredLogs.Count}";
            
            // 如果之前在末尾，则自动滚动到最新日志
            if (wasAtEnd && _debugLogTextBox.TextLength > 0)
            {
                _debugLogTextBox.SelectionStart = _debugLogTextBox.TextLength;
                _debugLogTextBox.ScrollToCaret();
            }
            else
            {
                // 否则恢复之前的滚动位置
                _debugLogTextBox.SelectionStart = Math.Min(currentScrollPos, _debugLogTextBox.TextLength);
            }
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常，避免影响主程序
            DebugService.Instance.LogError("UI", $"更新日志显示时出错: {ex.Message}");
        }
    }
    
    // 将彩色日志添加到RichTextBox的辅助方法
    private void AddColoredLogToTextBox(DebugLog log)
    {
        if (log == null || _debugLogTextBox == null)
            return;
        
        // 根据日志级别设置颜色
        Color logColor = Color.White;
        switch (log.Level)
        {
            case LogLevel.Info:
                logColor = Color.Cyan;
                break;
            case LogLevel.Debug:
                logColor = Color.Green;
                break;
            case LogLevel.Warning:
                logColor = Color.Yellow;
                break;
            case LogLevel.Error:
                logColor = Color.Red;
                break;
        }
        
        // 获取命令行格式的日志文本
        string logText = log.ToString();
        
        // 保存当前选择
        int startPos = _debugLogTextBox.TextLength;
        
        // 添加日志文本
        _debugLogTextBox.AppendText(logText + Environment.NewLine);
        
        // 设置颜色
        _debugLogTextBox.SelectionStart = startPos;
        _debugLogTextBox.SelectionLength = logText.Length;
        _debugLogTextBox.SelectionColor = logColor;
        
        // 重置颜色为默认值
        _debugLogTextBox.SelectionStart = _debugLogTextBox.TextLength;
        _debugLogTextBox.SelectionColor = Color.White;
    }
    
    private void ExportDebugLogs()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "导出调试日志",
                Filter = "文本文件 (*.txt)|*.txt|JSON文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = "txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileExtension = Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant();
                bool isJson = fileExtension == ".json";
                
                // 获取当前过滤后的日志
                var filteredLogs = DebugService.Instance.Logs.Where(log => 
                    (log.Level == LogLevel.Info && (_infoLogCheckbox?.Checked ?? false)) ||
                    (log.Level == LogLevel.Debug && (_debugLogCheckbox?.Checked ?? false)) ||
                    (log.Level == LogLevel.Warning && (_warningLogCheckbox?.Checked ?? false)) ||
                    (log.Level == LogLevel.Error && (_errorLogCheckbox?.Checked ?? false))
                ).ToList();
                
                // 应用搜索过滤
                string searchTerm = _searchLogTextBox?.Text.Trim() ?? string.Empty;
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredLogs = filteredLogs.Where(log => 
                        (log.Message != null && log.Message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                        (log.Source != null && log.Source.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                
                // 直接导出所有日志，不进行过滤
                DebugService.Instance.ExportLogs(saveFileDialog.FileName);
                
                // 显示导出成功提示
                MessageBox.Show($"日志成功导出到:\n{saveFileDialog.FileName}", 
                    "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            DebugService.Instance.LogError("WinFormUI", $"导出日志失败: {ex.Message}");
            MessageBox.Show($"导出日志时出错:\n{ex.Message}", 
                "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void ShowConfigDialog()
    {
        try
        {
            using var configForm = new ApiConfigForm(_aiService);
            configForm.ShowDialog();
        }
        catch (Exception ex)
        {
            // 捕获所有异常并显示友好的错误消息，而不是让程序崩溃
            MessageBox.Show($"打开配置对话框时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void WinFormUI_Resize(object? sender, EventArgs e)
    {
        // 当窗口最小化时，将其隐藏并显示在托盘
        if (WindowState == FormWindowState.Minimized && _trayIconService != null)
        {
            // 显示托盘图标
            _trayIconService.ShowTrayIcon();
            
            // 隐藏窗口
            this.Hide();
            
            // 显示通知
            _trayIconService.ShowNotification("AI对话助手", "程序已最小化到系统托盘", 2000);
            
            return; // 不需要调整控件大小
        }

        // 确保所有控件不为null
        if (_chatBox == null || _inputBox == null || _sendButton == null || _clearButton == null || _configButton == null ||
            _conversationComboBox == null || _newConversationButton == null || _listConversationsButton == null ||
            _pluginsButton == null || _providerManagerButton == null)
            return;
            
        // 对话选择框大小调整
        _conversationComboBox.Width = ClientSize.Width - 210; // 减去右侧按钮宽度和边距
        
        // 右侧按钮位置调整
        _newConversationButton.Left = ClientSize.Width - 190;
        _listConversationsButton.Left = ClientSize.Width - 100;
        
        // 聊天框大小调整
        _chatBox.Width = ClientSize.Width - 20; // 左右各留10像素边距
        _chatBox.Height = ClientSize.Height - 160; // 底部留出足够空间给其他控件
        
        // 输入框宽度调整
        _inputBox.Width = ClientSize.Width - 170; // 减去按钮宽度和边距
        _inputBox.Top = ClientSize.Height - 110; // 底部留出足够空间
        
        // 右侧底部按钮位置调整（发送、清空、配置）
        _sendButton.Left = ClientSize.Width - 160; // 右侧留出10像素边距
        _sendButton.Top = ClientSize.Height - 110;
        
        _clearButton.Left = ClientSize.Width - 160;
        _clearButton.Top = ClientSize.Height - 70;
        
        _configButton.Left = ClientSize.Width - 160;
        _configButton.Top = ClientSize.Height - 30;
        
        // 左侧底部按钮位置调整（插件管理、供应商管理）
        // 保持左侧按钮的相对位置，但确保它们在窗口底部之上
        _pluginsButton.Top = ClientSize.Height - 30;
        
        _providerManagerButton.Top = ClientSize.Height - 30;
        
        // 调试按钮位置调整（如果可见）
        if (_toggleDebugPanelButton != null && _toggleDebugPanelButton.Visible)
        {
            _toggleDebugPanelButton.Top = ClientSize.Height - 30;
        }
    }
    
    // 更新对话下拉框
    private void UpdateConversationComboBox()
    {
        if (_conversationComboBox == null)
            return;
            
        var conversations = _conversationService.GetAllConversations();
        var activeConversation = _conversationService.GetActiveConversation();
        
        _conversationComboBox.Items.Clear();
        
        foreach (var conversation in conversations)
        {
            _conversationComboBox.Items.Add(new ConversationItem(conversation));
        }
        
        // 选择当前活动对话
        if (activeConversation != null)
        {
            for (int i = 0; i < _conversationComboBox.Items.Count; i++)
            {
                if (_conversationComboBox.Items[i] is ConversationItem item && item.ConversationId == activeConversation.Id)
                {
                    _conversationComboBox.SelectedIndex = i;
                    break;
                }
            }
        }
    }
    
    // 当选择对话时切换到该对话
    private void OnConversationSelected()
    {
        if (_conversationComboBox == null || _conversationComboBox.SelectedItem == null)
            return;
            
        var selectedItem = _conversationComboBox.SelectedItem as ConversationItem;
        if (selectedItem == null)
            return;
            
        // 设置活动对话
        _conversationService.SetActiveConversation(selectedItem.ConversationId);
        
        // 刷新聊天显示
        LoadConversationHistory();
    }
    
    // 加载对话历史到聊天窗口
    private void LoadConversationHistory()
    {
        if (_chatBox == null)
            return;
            
        var activeConversation = _conversationService.GetActiveConversation();
        if (activeConversation == null)
            return;
            
        _chatBox.Clear();
        
        foreach (var message in activeConversation.Messages)
        {
            string sender = message.Role == "user" ? "你" : "AI";
            Color color = message.Role == "user" ? Color.DarkGreen : Color.DarkBlue;
            AddMessageToChat(sender, message.Content, color);
        }
    }
    
    // 创建新对话
    private void CreateNewConversation()
    {
        var newConversation = _conversationService.CreateConversation();
        UpdateConversationComboBox();
        _chatBox?.Clear();
        AddMessageToChat("系统", "已创建新对话", Color.Blue);
    }
    
    // 显示对话管理器
    private void ShowConversationManager()
    {
        using var managerForm = new ConversationManagerForm(_conversationService);
        if (managerForm.ShowDialog() == DialogResult.OK)
        {
            UpdateConversationComboBox();
            LoadConversationHistory();
        }
    }
    
    // 显示供应商管理器
    private void ShowProviderManager()
    {
        using var managerForm = new ProviderManagerForm();
        managerForm.ShowDialog();
    }
    
    // 显示插件管理器
    private void ShowPluginsManager()
    {
        // 显示插件管理窗口的逻辑
        Form pluginsManager = new Form
        {
            Text = "插件管理",
            Size = new Size(500, 400),
            StartPosition = FormStartPosition.CenterParent
        };
        
        // 创建一个ListView来显示所有插件
        ListView pluginsListView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        
        // 添加列
        pluginsListView.Columns.Add("插件名称", 150);
        pluginsListView.Columns.Add("版本", 60);
        pluginsListView.Columns.Add("描述", 200);
        pluginsListView.Columns.Add("状态", 60);
        
        // 填充插件列表
        foreach (var plugin in _pluginManager.LoadedPlugins)
        {
            ListViewItem item = new ListViewItem(plugin.Info.Name);
            item.SubItems.Add(plugin.Info.Version);
            item.SubItems.Add(plugin.Info.Description);
            item.SubItems.Add(plugin.IsEnabled ? "已启用" : "已禁用");
            item.Tag = plugin;
            pluginsListView.Items.Add(item);
        }
        
        // 添加启用/禁用按钮
        FlowLayoutPanel buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(5)
        };
        
        Button enableButton = new Button
        {
            Text = "启用",
            Width = 80,
            Margin = new Padding(5, 5, 5, 5)
        };
        
        Button disableButton = new Button
        {
            Text = "禁用",
            Width = 80,
            Margin = new Padding(5, 5, 5, 5)
        };
        
        // 启用按钮点击事件
        enableButton.Click += (s, e) =>
        {
            if (pluginsListView.SelectedItems.Count > 0)
            {
                var plugin = pluginsListView.SelectedItems[0].Tag as IPlugin;
                if (plugin != null)
                {
                    _pluginManager.EnablePlugin(plugin.Info.Name);
                    pluginsListView.SelectedItems[0].SubItems[3].Text = "已启用";
                    MessageBox.Show($"插件 '{plugin.Info.Name}' 已启用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        };
        
        // 禁用按钮点击事件
        disableButton.Click += (s, e) =>
        {
            if (pluginsListView.SelectedItems.Count > 0)
            {
                var plugin = pluginsListView.SelectedItems[0].Tag as IPlugin;
                if (plugin != null)
                {
                    _pluginManager.DisablePlugin(plugin.Info.Name);
                    pluginsListView.SelectedItems[0].SubItems[3].Text = "已禁用";
                    MessageBox.Show($"插件 '{plugin.Info.Name}' 已禁用", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        };
        
        // 添加按钮到面板
        buttonPanel.Controls.Add(enableButton);
        buttonPanel.Controls.Add(disableButton);
        
        // 添加控件到窗口
        pluginsManager.Controls.Add(pluginsListView);
        pluginsManager.Controls.Add(buttonPanel);
        
        // 显示窗口
        pluginsManager.ShowDialog();
    }
    
    // 对话项，用于下拉框显示
    private class ConversationItem
    {
        public string ConversationId { get; }
        public string Title { get; }
        
        public ConversationItem(Conversation conversation)
        {
            ConversationId = conversation.Id;
            Title = conversation.Title;
        }
        
        public override string ToString()
        {
            return Title;
        }
    }
}

// 对话管理窗体
public class ConversationManagerForm : Form
{
    private readonly IConversationService _conversationService;
    private readonly ListView _conversationListView;
    private readonly Button _deleteButton;
    private readonly Button _renameButton;
    private readonly Button _selectButton;
    private readonly Button _closeButton;
    
    public ConversationManagerForm(IConversationService conversationService)
    {
        _conversationService = conversationService;
        
        Text = "管理对话";
        Size = new Size(600, 400);
        StartPosition = FormStartPosition.CenterParent;
        
        // 初始化列表视图
        _conversationListView = new ListView
        {
            Location = new Point(10, 10),
            Size = new Size(560, 280),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };
        
        // 添加列
        _conversationListView.Columns.Add("ID", 100);
        _conversationListView.Columns.Add("标题", 200);
        _conversationListView.Columns.Add("创建时间", 120);
        _conversationListView.Columns.Add("消息数", 60);
        
        // 删除按钮
        _deleteButton = new Button
        {
            Location = new Point(10, 310),
            Size = new Size(100, 35),
            Text = "删除对话",
            BackColor = Color.Red,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 9)
        };
        
        // 重命名按钮
        _renameButton = new Button
        {
            Location = new Point(120, 310),
            Size = new Size(100, 35),
            Text = "重命名",
            BackColor = Color.Orange,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 9)
        };
        
        // 选择按钮
        _selectButton = new Button
        {
            Location = new Point(230, 310),
            Size = new Size(100, 35),
            Text = "选择对话",
            BackColor = Color.Green,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 9)
        };
        
        // 关闭按钮
        _closeButton = new Button
        {
            Location = new Point(470, 310),
            Size = new Size(100, 35),
            Text = "关闭",
            BackColor = Color.Gray,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 9)
        };
        
        // 添加控件
        Controls.AddRange(new Control[] {
            _conversationListView, _deleteButton, _renameButton, _selectButton, _closeButton
        });
        
        // 添加事件处理
        _deleteButton.Click += DeleteSelectedConversation;
        _renameButton.Click += RenameSelectedConversation;
        _selectButton.Click += SelectSelectedConversation;
        _closeButton.Click += (s, e) => Close();
        
        // 加载对话列表
        LoadConversations();
    }
    
    // 加载对话列表
    private void LoadConversations()
    {
        _conversationListView.Items.Clear();
        
        var conversations = _conversationService.GetAllConversations();
        var activeConversation = _conversationService.GetActiveConversation();
        
        foreach (var conversation in conversations)
        {
            var item = new ListViewItem(conversation.Id);
            item.SubItems.Add(conversation.Title);
            item.SubItems.Add(conversation.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
            item.SubItems.Add(conversation.Messages.Count.ToString());
            
            // 如果是活动对话，高亮显示
            if (activeConversation != null && conversation.Id == activeConversation.Id)
            {
                item.BackColor = Color.LightBlue;
            }
            
            _conversationListView.Items.Add(item);
        }
    }
    
    // 删除选中的对话
    private void DeleteSelectedConversation(object? sender, EventArgs e)
    {
        if (_conversationListView.SelectedItems.Count == 0)
        {
            MessageBox.Show("请选择要删除的对话", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var selectedItem = _conversationListView.SelectedItems[0];
        var conversationId = selectedItem.Text;
        
        var confirmResult = MessageBox.Show(
            $"确定要删除对话 '{selectedItem.SubItems[1].Text}' 吗？", 
            "确认删除", 
            MessageBoxButtons.YesNo, 
            MessageBoxIcon.Warning);
        
        if (confirmResult == DialogResult.Yes)
        {
            _conversationService.DeleteConversation(conversationId);
            LoadConversations();
        }
    }
    
    // 重命名选中的对话
    private void RenameSelectedConversation(object? sender, EventArgs e)
    {
        if (_conversationListView.SelectedItems.Count == 0)
        {
            MessageBox.Show("请选择要重命名的对话", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var selectedItem = _conversationListView.SelectedItems[0];
        var conversationId = selectedItem.Text;
        var currentTitle = selectedItem.SubItems[1].Text;
        
        var newTitle = Microsoft.VisualBasic.Interaction.InputBox(
            "请输入新的对话标题", 
            "重命名对话", 
            currentTitle);
        
        if (!string.IsNullOrWhiteSpace(newTitle))
        {
            _conversationService.UpdateConversationTitle(conversationId, newTitle);
            LoadConversations();
        }
    }
    
    // 选择对话
    private void SelectSelectedConversation(object? sender, EventArgs e)
    {
        if (_conversationListView.SelectedItems.Count == 0)
        {
            MessageBox.Show("请选择要切换的对话", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        var selectedItem = _conversationListView.SelectedItems[0];
        var conversationId = selectedItem.Text;
        
        _conversationService.SetActiveConversation(conversationId);
        DialogResult = DialogResult.OK;
        Close();
    }
}

// API配置窗体
public class ApiConfigForm : Form
{
    private readonly IAiService _aiService;
    private readonly ComboBox _providerComboBox;
    private readonly TextBox _apiKeyTextBox;
    private readonly TextBox _baseUrlTextBox;
    private readonly TextBox _modelTextBox;
    private readonly TextBox _azureDeploymentIdTextBox;
    private readonly TextBox _apiVersionTextBox;
    private readonly Panel _azureConfigPanel;
    private readonly NumericUpDown _maxTokensNumericUpDown;
    private readonly NumericUpDown _temperatureNumericUpDown;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;

    public ApiConfigForm(IAiService aiService)
    {
        _aiService = aiService;
        
        Text = "API配置";
        Size = new Size(500, 450);
        StartPosition = FormStartPosition.CenterParent;
        
        // 提供商选择
        var providerLabel = new Label { Text = "提供商:", Location = new Point(20, 20), Size = new Size(80, 25) };
        _providerComboBox = new ComboBox { Location = new Point(100, 20), Size = new Size(350, 25), DropDownStyle = ComboBoxStyle.DropDownList };
        _providerComboBox.Items.AddRange(Enum.GetNames(typeof(AiProvider)));
        
        var apiKeyLabel = new Label { Text = "API Key:", Location = new Point(20, 60), Size = new Size(80, 25) };
        _apiKeyTextBox = new TextBox { Location = new Point(100, 60), Size = new Size(350, 25), UseSystemPasswordChar = true };

        var baseUrlLabel = new Label { Text = "Base URL:", Location = new Point(20, 100), Size = new Size(80, 25) };
        _baseUrlTextBox = new TextBox { Location = new Point(100, 100), Size = new Size(350, 25) };

        var modelLabel = new Label { Text = "模型:", Location = new Point(20, 140), Size = new Size(80, 25) };
        _modelTextBox = new TextBox { Location = new Point(100, 140), Size = new Size(350, 25) };

        // Azure OpenAI 特定配置
        _azureConfigPanel = new Panel { Location = new Point(20, 180), Size = new Size(450, 80) };
        var azureDeploymentIdLabel = new Label { Text = "部署ID:", Location = new Point(0, 0), Size = new Size(80, 25) };
        _azureDeploymentIdTextBox = new TextBox { Location = new Point(100, 0), Size = new Size(350, 25) };
        var apiVersionLabel = new Label { Text = "API版本:", Location = new Point(0, 40), Size = new Size(80, 25) };
        _apiVersionTextBox = new TextBox { Location = new Point(100, 40), Size = new Size(350, 25) };
        _azureConfigPanel.Controls.AddRange(new Control[] { azureDeploymentIdLabel, _azureDeploymentIdTextBox, apiVersionLabel, _apiVersionTextBox });

        var maxTokensLabel = new Label { Text = "最大令牌数:", Location = new Point(20, 260), Size = new Size(80, 25) };
        _maxTokensNumericUpDown = new NumericUpDown { Location = new Point(100, 260), Size = new Size(350, 25), Minimum = 1, Maximum = 4000, Value = 1000 };

        var temperatureLabel = new Label { Text = "温度:", Location = new Point(20, 300), Size = new Size(80, 25) };
        _temperatureNumericUpDown = new NumericUpDown { Location = new Point(100, 300), Size = new Size(350, 25), Minimum = 0, Maximum = 2, DecimalPlaces = 1, Increment = 0.1M, Value = 0.7M };

        _saveButton = new Button { Text = "保存", Location = new Point(150, 340), Size = new Size(80, 30) };
        _cancelButton = new Button { Text = "取消", Location = new Point(250, 340), Size = new Size(80, 30) };

        Controls.AddRange(new Control[] {
            providerLabel, _providerComboBox,
            apiKeyLabel, _apiKeyTextBox,
            baseUrlLabel, _baseUrlTextBox,
            modelLabel, _modelTextBox,
            _azureConfigPanel,
            maxTokensLabel, _maxTokensNumericUpDown,
            temperatureLabel, _temperatureNumericUpDown,
            _saveButton, _cancelButton
        });
        
        // 添加提供商变更事件
        _providerComboBox.SelectedIndexChanged += ProviderComboBox_SelectedIndexChanged;
        
        // 加载现有配置 - 确保在所有控件初始化并添加到Controls集合后调用
        LoadExistingConfig();

        _saveButton.Click += SaveConfig;
        _cancelButton.Click += (s, e) => Close();
    }
    
    // 提供商变更时更新UI
    private void ProviderComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_providerComboBox.SelectedItem is string selectedProviderName && 
            Enum.TryParse<AiProvider>(selectedProviderName, out var selectedProvider))
        {
            // 根据提供商显示/隐藏Azure配置面板
            _azureConfigPanel.Visible = selectedProvider == AiProvider.AzureOpenAI;
            
            // 设置默认值
            var defaultConfig = AppConfig.GetDefaultConfigForProvider(selectedProvider);
            if (defaultConfig != null)
            {
                if (string.IsNullOrEmpty(_baseUrlTextBox.Text))
                    _baseUrlTextBox.Text = defaultConfig.BaseUrl;
                if (string.IsNullOrEmpty(_modelTextBox.Text))
                    _modelTextBox.Text = defaultConfig.Model;
                if (selectedProvider == AiProvider.AzureOpenAI)
                {
                    if (string.IsNullOrEmpty(_apiVersionTextBox.Text))
                        _apiVersionTextBox.Text = defaultConfig.ApiVersion ?? "2023-05-15";
                }
            }
        }
    }

    private void LoadExistingConfig()
    {
        try
        {
            var config = AppConfig.LoadConfig();
            if (config != null)
            {
                // 设置提供商选择
                if (_providerComboBox.Items.Contains(config.Provider.ToString()))
                {
                    _providerComboBox.SelectedItem = config.Provider.ToString();
                }
                
                // 显示或隐藏Azure配置面板
                _azureConfigPanel.Visible = config.Provider == AiProvider.AzureOpenAI;
                
                // 设置其他配置项
                _apiKeyTextBox.Text = config.ApiKey ?? "";
                _baseUrlTextBox.Text = config.BaseUrl ?? "";
                _modelTextBox.Text = config.Model ?? "";
                _azureDeploymentIdTextBox.Text = config.AzureDeploymentId ?? "";
                _apiVersionTextBox.Text = config.ApiVersion ?? "2023-05-15";
                _maxTokensNumericUpDown.Value = config.MaxTokens;
                _temperatureNumericUpDown.Value = (decimal)config.Temperature;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("加载配置失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void SaveConfig(object? sender, EventArgs e)
    {
        try
        {
            var config = new ApiConfig
            {
                Provider = Enum.TryParse<AiProvider>(_providerComboBox.SelectedItem as string ?? "OpenAI", out var provider) ? provider : AiProvider.OpenAI,
                ApiKey = _apiKeyTextBox?.Text ?? "",
                BaseUrl = _baseUrlTextBox?.Text ?? "",
                Model = _modelTextBox?.Text ?? "",
                AzureDeploymentId = _azureDeploymentIdTextBox?.Text ?? "",
                ApiVersion = _apiVersionTextBox?.Text ?? "",
                MaxTokens = _maxTokensNumericUpDown != null ? (int)_maxTokensNumericUpDown.Value : 1000,
                Temperature = _temperatureNumericUpDown != null ? (double)_temperatureNumericUpDown.Value : 0.7
            };

            // 保存配置到文件
            AppConfig.SaveConfig(config);

            // 更新AI服务配置 - 添加null检查
            if (_aiService != null && _aiService is OpenAiService openAiService)
            {
                try
                {
                    openAiService.UpdateConfig(config);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"更新AI服务配置时出错: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            // 调用Program类中的方法更新静态AI服务实例
            Program.UpdateAiService(config);

            MessageBox.Show("配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存配置时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    

}