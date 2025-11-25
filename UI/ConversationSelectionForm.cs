// UI/ConversationSelectionForm.cs
using System;
using System.Windows.Forms;
using AIChatAssistant.Services;

namespace AIChatAssistant.UI;

public class ConversationSelectionForm : Form
{
    private readonly IConversationService _conversationService;
    private Label _messageLabel;
    private Button _continueButton;
    private Button _newConversationButton;
    private Button _cancelButton;
    
    public bool ShouldContinueLastConversation { get; private set; } = true;
    public bool IsCancelled { get; private set; } = false;
    
    public ConversationSelectionForm(IConversationService conversationService)
    {
        _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
        
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        Text = "选择操作";
        Size = new Size(350, 180);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        
        // 消息标签
        _messageLabel = new Label
        {
            Text = "检测到之前的对话，您想要：",
            Location = new Point(20, 20),
            Size = new Size(300, 40),
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        };
        
        // 继续上次对话按钮
        _continueButton = new Button
        {
            Text = "继续上次对话",
            Location = new Point(20, 70),
            Size = new Size(140, 30)
        };
        _continueButton.Click += ContinueButton_Click;
        
        // 新建对话按钮
        _newConversationButton = new Button
        {
            Text = "新建对话",
            Location = new Point(180, 70),
            Size = new Size(140, 30)
        };
        _newConversationButton.Click += NewConversationButton_Click;
        
        // 取消按钮
        _cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(130, 110),
            Size = new Size(80, 30)
        };
        _cancelButton.Click += CancelButton_Click;
        
        // 添加控件
        Controls.Add(_messageLabel);
        Controls.Add(_continueButton);
        Controls.Add(_newConversationButton);
        Controls.Add(_cancelButton);
    }
    
    private void ContinueButton_Click(object? sender, EventArgs e)
    {
        ShouldContinueLastConversation = true;
        DialogResult = DialogResult.OK;
        Close();
    }
    
    private void NewConversationButton_Click(object? sender, EventArgs e)
    {
        ShouldContinueLastConversation = false;
        DialogResult = DialogResult.OK;
        Close();
    }
    
    private void CancelButton_Click(object? sender, EventArgs e)
    {
        IsCancelled = true;
        DialogResult = DialogResult.Cancel;
        Close();
    }
}