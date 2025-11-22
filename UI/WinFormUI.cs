// UI/WinFormUI.cs
using AIChatAssistant.Config;
using AIChatAssistant.Models;
using AIChatAssistant.Services;
using System.Drawing;
using System.Windows.Forms;

namespace AIChatAssistant.UI;

public partial class WinFormUI : Form
{
    private readonly IAiService _aiService;
    private readonly List<ChatMessage> _conversationHistory;
    private RichTextBox? _chatBox;
    private TextBox? _inputBox;
    private Button? _sendButton;
    private Button? _clearButton;
    private Button? _configButton;

    public WinFormUI(IAiService aiService)
    {
        _aiService = aiService;
        _conversationHistory = new List<ChatMessage>();
        
        InitializeComponent();
        SetupUI();
    }

    private void InitializeComponent()
    {
        Text = "AI对话助手";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
        
        // 添加Resize事件处理
        Resize += new EventHandler(WinFormUI_Resize);

        _chatBox = new RichTextBox
        {
            Location = new Point(10, 10),
            Size = new Size(760, 400),
            ReadOnly = true,
            BackColor = Color.White,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        _inputBox = new TextBox
        {
            Location = new Point(10, 430),
            Font = new Font("Microsoft YaHei", 10),
            Multiline = true,
            Size = new Size(600, 80),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

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

        _clearButton = new Button
        {
            Location = new Point(620, 475),
            Size = new Size(150, 35),
            Text = "清空对话",
            BackColor = Color.LightGray,
            Font = new Font("Microsoft YaHei", 10),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

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

        Controls.AddRange(new Control[] { _chatBox, _inputBox, _sendButton, _clearButton, _configButton });
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

        // 禁用按钮防止重复发送
        _sendButton.Enabled = false;
        _inputBox.Enabled = false;

        try
        {
            // 发送消息并获取回复
            var response = await _aiService.SendMessageAsync(message, _conversationHistory);

            // 显示AI回复
            AddMessageToChat("AI", response, Color.DarkBlue);

            // 保存对话历史
            _conversationHistory.Add(new ChatMessage { Role = "user", Content = message });
            _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response });
        }
        catch (Exception ex)
        {
            AddMessageToChat("系统", $"错误: {ex.Message}", Color.Red);
        }
        finally
        {
            _sendButton.Enabled = true;
            _inputBox.Enabled = true;
            _inputBox.Focus();
        }
    }

    private void AddMessageToChat(string sender, string message, Color color)
    {
        // 添加null检查
        if (_chatBox == null)
            return;
            
        _chatBox.SelectionStart = _chatBox.TextLength;
        _chatBox.SelectionColor = color;
        _chatBox.AppendText($"{sender}: {message}\n\n");
        _chatBox.ScrollToCaret();
    }

    private void ClearConversation()
    {
        // 添加null检查
        if (_chatBox == null)
            return;
            
        _chatBox.Clear();
        _conversationHistory.Clear();
        AddMessageToChat("系统", "对话历史已清空", Color.Blue);
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
        // 确保所有控件不为null
        if (_chatBox == null || _inputBox == null || _sendButton == null || _clearButton == null || _configButton == null)
            return;
            
        // 聊天框大小调整
        _chatBox.Width = ClientSize.Width - 20; // 左右各留10像素边距
        _chatBox.Height = ClientSize.Height - 130; // 底部留出足够空间给其他控件
        
        // 输入框宽度调整
        _inputBox.Width = ClientSize.Width - 170; // 减去按钮宽度和边距
        _inputBox.Top = ClientSize.Height - 110; // 底部留出足够空间
        
        // 右侧按钮位置调整
        _sendButton.Left = ClientSize.Width - 160; // 右侧留出10像素边距
        _sendButton.Top = ClientSize.Height - 110;
        
        _clearButton.Left = ClientSize.Width - 160;
        _clearButton.Top = ClientSize.Height - 70;
        
        _configButton.Left = ClientSize.Width - 160;
        _configButton.Top = ClientSize.Height - 30;
    }
}

// API配置窗体
public class ApiConfigForm : Form
{
    private readonly IAiService _aiService;
    private readonly TextBox _apiKeyTextBox;
    private readonly TextBox _baseUrlTextBox;
    private readonly TextBox _modelTextBox;
    private readonly NumericUpDown _maxTokensNumericUpDown;
    private readonly NumericUpDown _temperatureNumericUpDown;
    private readonly Button _saveButton;
    private readonly Button _cancelButton;

    public ApiConfigForm(IAiService aiService)
    {
        _aiService = aiService;
        
        Text = "API配置";
        Size = new Size(500, 350);
        StartPosition = FormStartPosition.CenterParent;
        
        var apiKeyLabel = new Label { Text = "API Key:", Location = new Point(20, 20), Size = new Size(80, 25) };
        _apiKeyTextBox = new TextBox { Location = new Point(100, 20), Size = new Size(350, 25), UseSystemPasswordChar = true };

        var baseUrlLabel = new Label { Text = "Base URL:", Location = new Point(20, 60), Size = new Size(80, 25) };
        _baseUrlTextBox = new TextBox { Location = new Point(100, 60), Size = new Size(350, 25) };

        var modelLabel = new Label { Text = "模型:", Location = new Point(20, 100), Size = new Size(80, 25) };
        _modelTextBox = new TextBox { Location = new Point(100, 100), Size = new Size(350, 25) };

        var maxTokensLabel = new Label { Text = "最大令牌数:", Location = new Point(20, 140), Size = new Size(80, 25) };
        _maxTokensNumericUpDown = new NumericUpDown { Location = new Point(100, 140), Size = new Size(350, 25), Minimum = 1, Maximum = 4000, Value = 1000 };

        var temperatureLabel = new Label { Text = "温度:", Location = new Point(20, 180), Size = new Size(80, 25) };
        _temperatureNumericUpDown = new NumericUpDown { Location = new Point(100, 180), Size = new Size(350, 25), Minimum = 0, Maximum = 2, DecimalPlaces = 1, Increment = 0.1M, Value = 0.7M };

        _saveButton = new Button { Text = "保存", Location = new Point(150, 220), Size = new Size(80, 30) };
        _cancelButton = new Button { Text = "取消", Location = new Point(250, 220), Size = new Size(80, 30) };

        Controls.AddRange(new Control[] {
            apiKeyLabel, _apiKeyTextBox,
            baseUrlLabel, _baseUrlTextBox,
            modelLabel, _modelTextBox,
            maxTokensLabel, _maxTokensNumericUpDown,
            temperatureLabel, _temperatureNumericUpDown,
            _saveButton, _cancelButton
        });
        
        // 加载现有配置 - 确保在所有控件初始化并添加到Controls集合后调用
        LoadExistingConfig();

        _saveButton.Click += SaveConfig;
        _cancelButton.Click += (s, e) => Close();
    }

    private void SaveConfig(object? sender, EventArgs e)
    {
        try
        {
            var config = new ApiConfig
            {
                ApiKey = _apiKeyTextBox?.Text ?? "",
                BaseUrl = _baseUrlTextBox?.Text ?? "",
                Model = _modelTextBox?.Text ?? "",
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

            MessageBox.Show("配置已保存", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存配置时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void LoadExistingConfig()
    {
        try
        {
            // 从文件加载配置
            var config = AppConfig.LoadConfig();
            
            // 添加null检查
            if (config != null)
            {
                // 设置控件值，添加null检查避免空引用异常
                if (_apiKeyTextBox != null)
                    _apiKeyTextBox.Text = config.ApiKey ?? "";
                if (_baseUrlTextBox != null)
                    _baseUrlTextBox.Text = config.BaseUrl ?? "";
                if (_modelTextBox != null)
                    _modelTextBox.Text = config.Model ?? "";
                if (_maxTokensNumericUpDown != null)
                    _maxTokensNumericUpDown.Value = config.MaxTokens;
                if (_temperatureNumericUpDown != null)
                    _temperatureNumericUpDown.Value = (decimal)config.Temperature;
            }
        }
        catch (Exception ex)
        {
            // 捕获并显示错误，但不中断程序流程
            System.Diagnostics.Debug.WriteLine($"加载配置时出错: {ex.Message}");
        }
    }
}