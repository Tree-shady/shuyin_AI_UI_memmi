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
            Size = new Size(220, 120),
            Margin = new Padding(5),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        
        var nameLabel = new Label
        {
            Text = agent.Name,
            Location = new Point(10, 10),
            Font = new Font("微软雅黑", 10, FontStyle.Bold)
        };
        
        var descLabel = new Label
        {
            Text = agent.Description,
            Location = new Point(10, 35),
            Size = new Size(200, 40),
            ForeColor = Color.FromArgb(100, 100, 100),
            Font = new Font("微软雅黑", 8)
        };
        
        var selectButton = new Button
        {
            Text = "选择",
            Location = new Point(10, 80),
            Size = new Size(80, 25)
        };
        
        selectButton.Click += (s, e) => SelectAgent(agent);
        
        card.Controls.AddRange(new Control[] { nameLabel, descLabel, selectButton });
        
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

    public AgentCreationDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "创建智能体";
        this.Size = new Size(500, 600);
        
        // 创建表单控件
        CreateFormControls();
    }

    private void CreateFormControls()
    {
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 2
        };
        
        _nameTextBox = new TextBox { Dock = DockStyle.Fill };
        _descTextBox = new TextBox { Dock = DockStyle.Fill };
        _typeComboBox = new ComboBox { Dock = DockStyle.Fill };
        
        // 添加类型选项
        _typeComboBox.Items.AddRange(Enum.GetNames(typeof(AgentType)));
        
        mainPanel.Controls.Add(new Label { Text = "名称:", TextAlign = ContentAlignment.MiddleRight });
        mainPanel.Controls.Add(_nameTextBox);
        
        mainPanel.Controls.Add(new Label { Text = "描述:", TextAlign = ContentAlignment.MiddleRight });
        mainPanel.Controls.Add(_descTextBox);
        
        this.Controls.Add(mainPanel);
    }
}