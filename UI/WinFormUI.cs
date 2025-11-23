// UI/WinFormUI.cs
using System;
using System.Collections.Generic;
using System.Drawing;
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
    private Button? _configButton;
    private Button? _newConversationButton;
    private Button? _listConversationsButton;
    private Button? _pluginsButton;
    private ComboBox? _conversationComboBox;
    private TrayIconService? _trayIconService;

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
            Location = new Point(10, 430),
            Font = new Font("Microsoft YaHei", 10),
            Multiline = true,
            Size = new Size(600, 80),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        // 发送按钮
        _sendButton = new Button
        {
            Location = new Point(620, 430),
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
            Location = new Point(620, 475),
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
            Location = new Point(620, 520),
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
            Location = new Point(460, 520),
            Size = new Size(150, 35),
            Text = "插件管理",
            BackColor = Color.Purple,
            ForeColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        Controls.AddRange(new Control[] { 
            _conversationComboBox, _chatBox, _inputBox, _sendButton, _clearButton, 
            _newConversationButton, _listConversationsButton, _configButton, _pluginsButton 
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

        // 添加欢迎消息
        AddMessageToChat("系统", "欢迎使用AI对话助手！", Color.Blue);
    }

    private async Task SendMessage()
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
        // 移除对输入框的禁用，让用户可以继续输入

        try
        {
            // 获取当前活动会话
            var activeConversation = _conversationService.GetActiveConversation();
            if (activeConversation == null)
            {
                AddMessageToChat("系统", "错误: 当前没有活动对话", Color.Red);
                return;
            }

            // 异步发送消息并获取回复
            var response = await _aiService.SendMessageAsync(message, activeConversation.Messages, activeConversation.Id);

            // 显示AI回复
            AddMessageToChat("AI", response, Color.DarkBlue);

            // 保存对话历史
            var userMessage = new ChatMessage { Role = "user", Content = message };
            var assistantMessage = new ChatMessage { Role = "assistant", Content = response };
            
            _conversationService.AddMessageToConversation(activeConversation.Id, userMessage);
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
            // 不需要重新启用输入框，因为我们没有禁用它
            _inputBox.Focus(); // 确保输入框仍然保持焦点
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
            _conversationComboBox == null || _newConversationButton == null || _listConversationsButton == null)
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
        
        // 底部按钮位置调整
        _sendButton.Left = ClientSize.Width - 160; // 右侧留出10像素边距
        _sendButton.Top = ClientSize.Height - 110;
        
        _clearButton.Left = ClientSize.Width - 160;
        _clearButton.Top = ClientSize.Height - 70;
        
        _configButton.Left = ClientSize.Width - 160;
        _configButton.Top = ClientSize.Height - 30;
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