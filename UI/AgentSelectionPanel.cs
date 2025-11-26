// UI/AgentSelectionPanel.cs
using AIChatAssistant.Models.Agent;
using AIChatAssistant.Services;
using System.Windows.Forms;
using System.Drawing;

namespace AIChatAssistant.UI;

public partial class AgentSelectionPanel : UserControl
{
    private FlowLayoutPanel _agentsFlowPanel;
    private AgentManager _agentManager;

    public AgentSelectionPanel()
    {
        InitializeComponent();
        _agentManager = new AgentManager();
        LoadAgents();
    }

    private void InitializeComponent()
    {
        this.Dock = DockStyle.Left;
        this.Width = 250;
        this.BackColor = Color.FromArgb(245, 245, 245);
        
        _agentsFlowPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        
        var titleLabel = new Label
        {
            Text = "智能体选择",
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("微软雅黑", 12, FontStyle.Bold)
        };
        
        this.Controls.Add(_agentsFlowPanel);
        this.Controls.Add(titleLabel);
    }

    private void LoadAgents()
    {
        var agents = _agentManager.GetAvailableAgents();
        
        foreach (var agent in agents)
        {
            var agentCard = CreateAgentCard(agent);
            _agentsFlowPanel.Controls.Add(agentCard);
        }
    }

    private Panel CreateAgentCard(AgentDefinition agent)
    {
        var card = new Panel
        {
            Size = new Size(220, 180),
            Margin = new Padding(5),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(10)
        };
        
        // 智能体头像（使用首字母作为默认头像）
        var avatarPanel = new Panel
        {
            Location = new Point(10, 10),
            Size = new Size(40, 40),
            BackColor = Color.FromArgb(64, 158, 255),
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        
        var avatarLabel = new Label
        {
            Text = agent.Name.Substring(0, 1).ToUpper(),
            Location = new Point(0, 0),
            Size = new Size(40, 40),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("微软雅黑", 14, FontStyle.Bold),
            ForeColor = Color.White
        };
        avatarPanel.Controls.Add(avatarLabel);
        
        var nameLabel = new Label
        {
            Text = agent.Name,
            Location = new Point(60, 10),
            Size = new Size(150, 20),
            Font = new Font("微软雅黑", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 33, 33)
        };
        
        var descLabel = new Label
        {
            Text = agent.Description,
            Location = new Point(10, 60),
            Size = new Size(200, 60),
            ForeColor = Color.FromArgb(100, 100, 100),
            Font = new Font("微软雅黑", 9),
            AutoEllipsis = true,
            MaximumSize = new Size(200, 60)
        };
        
        // 功能标签
        var specialtiesFlowPanel = new FlowLayoutPanel
        {
            Location = new Point(10, 125),
            Size = new Size(200, 25),
            AutoScroll = false,
            WrapContents = true
        };
        
        if (agent.Personality?.Specialties != null && agent.Personality.Specialties.Any())
        {
            foreach (var specialty in agent.Personality.Specialties.Take(3)) // 最多显示3个标签
            {
                var tagLabel = new Label
                {
                    Text = specialty,
                    Padding = new Padding(5, 2, 5, 2),
                    Margin = new Padding(2),
                    BackColor = Color.FromArgb(230, 240, 255),
                    ForeColor = Color.FromArgb(64, 158, 255),
                    Font = new Font("微软雅黑", 7),
                    BorderStyle = BorderStyle.FixedSingle
                };
                specialtiesFlowPanel.Controls.Add(tagLabel);
            }
        }
        
        var selectButton = new Button
        {
            Text = "选择",
            Location = new Point(10, 150),
            Size = new Size(200, 25),
            BackColor = Color.FromArgb(64, 158, 255),
            ForeColor = Color.White,
            Font = new Font("微软雅黑", 9, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat
        };
        
        // 设置FlatAppearance属性（只读属性，不能直接赋值）
        selectButton.FlatAppearance.BorderSize = 0;
        selectButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 137, 255);
        selectButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(33, 133, 255);
        
        selectButton.Click += (s, e) => SelectAgent(agent);
        
        card.Controls.AddRange(new Control[] { avatarPanel, nameLabel, descLabel, specialtiesFlowPanel, selectButton });
        
        // 添加悬停效果
        card.MouseEnter += (s, e) => 
        {
            card.BackColor = Color.FromArgb(245, 248, 255);
        };
        card.MouseLeave += (s, e) => 
        {
            card.BackColor = Color.White;
        };
        
        return card;
    }

    private void SelectAgent(AgentDefinition agent)
    {
        _agentManager.SetCurrentAgent(agent.Id);
        OnAgentSelected?.Invoke(this, new AgentSelectedEventArgs(agent));
    }

    public event EventHandler<AgentSelectedEventArgs> OnAgentSelected;
}

// UI/AgentCreationDialog.cs
public partial class AgentCreationDialog : Form
{
    private TextBox _nameTextBox;
    private TextBox _descTextBox;
    private ComboBox _typeComboBox;
    private RichTextBox _promptTextBox;
    private CheckBox _canExecuteCodeCheckBox;
    private CheckBox _canRememberContextCheckBox;
    private Button _saveButton;
    private Button _cancelButton;
    
    private AgentManager _agentManager;

    public AgentCreationDialog()
    {
        InitializeComponent();
        _agentManager = new AgentManager();
    }

    private void InitializeComponent()
    {
        this.Text = "创建智能体";
        this.Size = new Size(500, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        // 创建表单控件
        CreateFormControls();
    }

    private void CreateFormControls()
    {
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 8,
            ColumnCount = 2,
            Padding = new Padding(10),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            RowStyles = {
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Absolute, 40),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.Absolute, 50)
            },
            ColumnStyles = {
                new ColumnStyle(SizeType.Absolute, 100),
                new ColumnStyle(SizeType.Percent, 100)
            }
        };
        
        // 名称
        var nameLabel = new Label { Text = "名称:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
        _nameTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };
        mainPanel.Controls.Add(nameLabel, 0, 0);
        mainPanel.Controls.Add(_nameTextBox, 1, 0);
        
        // 描述
        var descLabel = new Label { Text = "描述:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
        _descTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(5) };
        mainPanel.Controls.Add(descLabel, 0, 1);
        mainPanel.Controls.Add(_descTextBox, 1, 1);
        
        // 类型
        var typeLabel = new Label { Text = "类型:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
        _typeComboBox = new ComboBox { Dock = DockStyle.Fill, Margin = new Padding(5), DropDownStyle = ComboBoxStyle.DropDownList };
        _typeComboBox.Items.AddRange(Enum.GetNames(typeof(AgentType)));
        _typeComboBox.SelectedIndex = 0;
        mainPanel.Controls.Add(typeLabel, 0, 2);
        mainPanel.Controls.Add(_typeComboBox, 1, 2);
        
        // 执行代码能力
        var executeCodeLabel = new Label { Text = "执行代码:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
        _canExecuteCodeCheckBox = new CheckBox { Dock = DockStyle.Fill, Margin = new Padding(5), Text = "允许执行代码" };
        mainPanel.Controls.Add(executeCodeLabel, 0, 3);
        mainPanel.Controls.Add(_canExecuteCodeCheckBox, 1, 3);
        
        // 记忆上下文能力
        var rememberContextLabel = new Label { Text = "记忆上下文:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
        _canRememberContextCheckBox = new CheckBox { Dock = DockStyle.Fill, Margin = new Padding(5), Text = "允许记忆上下文" };
        mainPanel.Controls.Add(rememberContextLabel, 0, 4);
        mainPanel.Controls.Add(_canRememberContextCheckBox, 1, 4);
        
        // 系统提示词
        var promptLabel = new Label { Text = "系统提示词:", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
        _promptTextBox = new RichTextBox { Dock = DockStyle.Fill, Margin = new Padding(5), Multiline = true, ScrollBars = RichTextBoxScrollBars.Vertical };
        mainPanel.Controls.Add(promptLabel, 0, 5);
        mainPanel.Controls.Add(_promptTextBox, 1, 5);
        
        // 按钮面板
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(5)
        };
        
        _saveButton = new Button { Text = "保存", Margin = new Padding(5), Size = new Size(80, 30) };
        _cancelButton = new Button { Text = "取消", Margin = new Padding(5), Size = new Size(80, 30) };
        
        _saveButton.Click += SaveButton_Click;
        _cancelButton.Click += CancelButton_Click;
        
        buttonPanel.Controls.Add(_saveButton);
        buttonPanel.Controls.Add(_cancelButton);
        mainPanel.Controls.Add(new Label(), 0, 6);
        mainPanel.Controls.Add(buttonPanel, 1, 6);
        
        this.Controls.Add(mainPanel);
    }
    
    private void SaveButton_Click(object sender, EventArgs e)
    {
        // 验证输入
        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            MessageBox.Show("请输入智能体名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(_promptTextBox.Text))
        {
            MessageBox.Show("请输入系统提示词", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        
        // 创建智能体定义
        var agentDefinition = new AgentDefinition
        {
            Id = Guid.NewGuid().ToString(),
            Name = _nameTextBox.Text,
            Description = _descTextBox.Text,
            SystemPrompt = _promptTextBox.Text,
            Capabilities = new AgentCapability
            {
                CanExecuteCode = _canExecuteCodeCheckBox.Checked,
                CanRememberContext = _canRememberContextCheckBox.Checked
            },
            Personality = new AgentPersonality
            {
                Tone = "professional",
                ResponseStyle = "detailed",
                CommunicationStyle = "technical",
                Specialties = new List<string> { "通用助手" }
            }
        };
        
        // 保存智能体
        _agentManager.SaveCustomAgent(agentDefinition);
        
        MessageBox.Show("智能体创建成功", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        this.DialogResult = DialogResult.OK;
        this.Close();
    }
    
    private void CancelButton_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Cancel;
        this.Close();
    }
}