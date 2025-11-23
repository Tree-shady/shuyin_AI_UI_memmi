// UI/ProviderManagerForm.cs
using System;
using System.Windows.Forms;
using AIChatAssistant.Models;
using AIChatAssistant.Services;

namespace AIChatAssistant.UI;

public partial class ProviderManagerForm : ThemedForm
{
    private readonly IProviderConfigService _providerService;
    private AiProviderConfig? _currentProvider;
    
    public ProviderManagerForm()
    {
        InitializeComponent();
        _providerService = new ProviderConfigService();
        InitializeUI();
        LoadProviders();
    }
    
    private void InitializeUI()
    {
        this.Text = "API供应商管理";
        this.Width = 800;
        this.Height = 600;
        
        // 左侧供应商列表
        var providerListLabel = new Label
        {
            Text = "供应商列表:",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        
        dgvProviders = new DataGridView
        {
            Dock = DockStyle.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false
        };
        
        // 添加列
        dgvProviders.Columns.Add(new DataGridViewTextBoxColumn
        { HeaderText = "名称", DataPropertyName = "Name", Width = 150 });
        dgvProviders.Columns.Add(new DataGridViewTextBoxColumn
        { HeaderText = "类型", DataPropertyName = "ProviderType", Width = 100 });
        dgvProviders.Columns.Add(new DataGridViewCheckBoxColumn
        { HeaderText = "默认", DataPropertyName = "IsDefault", Width = 60 });
        
        dgvProviders.SelectionChanged += DgvProviders_SelectionChanged;
        
        var providerPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 350
        };
        providerPanel.Controls.Add(dgvProviders);
        providerPanel.Controls.Add(providerListLabel);
        
        // 按钮组
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.LeftToRight,
            Height = 40,
            Padding = new Padding(10)
        };
        
        btnAdd = new Button { Text = "添加", Width = 80 };
        btnEdit = new Button { Text = "编辑", Width = 80, Enabled = false };
        btnDelete = new Button { Text = "删除", Width = 80, Enabled = false };
        btnSetDefault = new Button { Text = "设为默认", Width = 80, Enabled = false };
        
        btnAdd.Click += BtnAdd_Click;
        btnEdit.Click += BtnEdit_Click;
        btnDelete.Click += BtnDelete_Click;
        btnSetDefault.Click += BtnSetDefault_Click;
        
        buttonPanel.Controls.Add(btnAdd);
        buttonPanel.Controls.Add(btnEdit);
        buttonPanel.Controls.Add(btnDelete);
        buttonPanel.Controls.Add(btnSetDefault);
        
        providerPanel.Controls.Add(buttonPanel);
        
        // 右侧详细信息
        var detailPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        
        var detailLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 8,
            AutoSize = true
        };
        
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        detailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        
        // 添加标签和输入框
        detailLayout.Controls.Add(new Label { Text = "名称:", Anchor = AnchorStyles.Right }, 0, 0);
        txtName = new TextBox { Dock = DockStyle.Fill };
        detailLayout.Controls.Add(txtName, 1, 0);
        
        detailLayout.Controls.Add(new Label { Text = "类型:", Anchor = AnchorStyles.Right }, 0, 1);
        cboProviderType = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        // 添加所有供应商类型
        foreach (var type in Enum.GetValues(typeof(AiProvider)))
        {
            cboProviderType.Items.Add(type);
        }
        detailLayout.Controls.Add(cboProviderType, 1, 1);
        
        detailLayout.Controls.Add(new Label { Text = "基础URL:", Anchor = AnchorStyles.Right }, 0, 2);
        txtBaseUrl = new TextBox { Dock = DockStyle.Fill };
        detailLayout.Controls.Add(txtBaseUrl, 1, 2);
        
        detailLayout.Controls.Add(new Label { Text = "API密钥:", Anchor = AnchorStyles.Right }, 0, 3);
        txtApiKey = new TextBox { Dock = DockStyle.Fill, PasswordChar = '*' };
        detailLayout.Controls.Add(txtApiKey, 1, 3);
        
        detailLayout.Controls.Add(new Label { Text = "默认模型:", Anchor = AnchorStyles.Right }, 0, 4);
        txtDefaultModel = new TextBox { Dock = DockStyle.Fill };
        detailLayout.Controls.Add(txtDefaultModel, 1, 4);
        
        detailLayout.Controls.Add(new Label { Text = "Azure部署ID:", Anchor = AnchorStyles.Right }, 0, 5);
        txtAzureDeploymentId = new TextBox { Dock = DockStyle.Fill };
        detailLayout.Controls.Add(txtAzureDeploymentId, 1, 5);
        
        detailLayout.Controls.Add(new Label { Text = "API版本:", Anchor = AnchorStyles.Right }, 0, 6);
        txtApiVersion = new TextBox { Dock = DockStyle.Fill };
        detailLayout.Controls.Add(txtApiVersion, 1, 6);
        
        detailLayout.Controls.Add(new Label { Text = "默认提供商:", Anchor = AnchorStyles.Right }, 0, 7);
        chkIsDefault = new CheckBox { Text = "设为默认API提供商" };
        detailLayout.Controls.Add(chkIsDefault, 1, 7);
        
        // 保存按钮
        btnSave = new Button { Text = "保存", Width = 100 };
        btnSave.Click += BtnSave_Click;
        
        detailPanel.Controls.Add(detailLayout);
        detailPanel.Controls.Add(new Panel { Height = 20, Dock = DockStyle.Top }); // 间隔
        detailPanel.Controls.Add(new Panel { Controls = { btnSave }, Dock = DockStyle.Top, Height = 40 });
        
        // 分割器
        var splitter = new Splitter { Dock = DockStyle.Left };
        
        // 添加到主窗口
        this.Controls.Add(detailPanel);
        this.Controls.Add(splitter);
        this.Controls.Add(providerPanel);
        
        // 初始禁用编辑功能
        DisableEditControls();
    }
    
    private void DisableEditControls()
    {
        txtName.Enabled = false;
        cboProviderType.Enabled = false;
        txtBaseUrl.Enabled = false;
        txtApiKey.Enabled = false;
        txtDefaultModel.Enabled = false;
        txtAzureDeploymentId.Enabled = false;
        txtApiVersion.Enabled = false;
        chkIsDefault.Enabled = false;
        btnSave.Enabled = false;
    }
    
    private void EnableEditControls()
    {
        txtName.Enabled = true;
        cboProviderType.Enabled = true;
        txtBaseUrl.Enabled = true;
        txtApiKey.Enabled = true;
        txtDefaultModel.Enabled = true;
        txtAzureDeploymentId.Enabled = true;
        txtApiVersion.Enabled = true;
        chkIsDefault.Enabled = true;
        btnSave.Enabled = true;
    }
    
    private void ClearEditControls()
    {
        txtName.Text = "";
        cboProviderType.SelectedIndex = 0;
        txtBaseUrl.Text = "";
        txtApiKey.Text = "";
        txtDefaultModel.Text = "";
        txtAzureDeploymentId.Text = "";
        txtApiVersion.Text = "2023-05-15";
        chkIsDefault.Checked = false;
        _currentProvider = null;
    }
    
    private void LoadProviders()
    {
        var providers = _providerService.GetAllProviders();
        dgvProviders.DataSource = null;
        dgvProviders.DataSource = providers;
    }
    
    private void LoadProviderDetails(AiProviderConfig provider)
    {
        _currentProvider = provider;
        txtName.Text = provider.Name;
        cboProviderType.SelectedItem = provider.ProviderType;
        txtBaseUrl.Text = provider.BaseUrl;
        txtApiKey.Text = provider.ApiKey;
        txtDefaultModel.Text = provider.DefaultModel;
        txtAzureDeploymentId.Text = provider.AzureDeploymentId ?? "";
        txtApiVersion.Text = provider.ApiVersion ?? "2023-05-15";
        chkIsDefault.Checked = provider.IsDefault;
    }
    
    private void DgvProviders_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvProviders.SelectedRows.Count > 0)
        {
            var provider = dgvProviders.SelectedRows[0].DataBoundItem as AiProviderConfig;
            if (provider != null)
            {
                LoadProviderDetails(provider);
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
                btnSetDefault.Enabled = !provider.IsDefault;
                DisableEditControls();
            }
        }
    }
    
    private void BtnAdd_Click(object sender, EventArgs e)
    {
        ClearEditControls();
        // 设置默认值
        txtBaseUrl.Text = "https://api.openai.com/v1";
        txtDefaultModel.Text = "gpt-3.5-turbo";
        EnableEditControls();
    }
    
    private void BtnEdit_Click(object sender, EventArgs e)
    {
        if (_currentProvider != null)
        {
            EnableEditControls();
        }
    }
    
    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (dgvProviders.SelectedRows.Count > 0)
        {
            var provider = dgvProviders.SelectedRows[0].DataBoundItem as AiProviderConfig;
            if (provider != null)
            {
                if (MessageBox.Show($"确定要删除供应商 '{provider.Name}' 吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        _providerService.DeleteProvider(provider.Id);
                        LoadProviders();
                        ClearEditControls();
                        DisableEditControls();
                        btnEdit.Enabled = false;
                        btnDelete.Enabled = false;
                        btnSetDefault.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
    
    private void BtnSetDefault_Click(object sender, EventArgs e)
    {
        if (dgvProviders.SelectedRows.Count > 0)
        {
            var provider = dgvProviders.SelectedRows[0].DataBoundItem as AiProviderConfig;
            if (provider != null)
            {
                _providerService.SetDefaultProvider(provider.Id);
                LoadProviders();
                LoadProviderDetails(provider);
                btnSetDefault.Enabled = false;
            }
        }
    }
    
    private void BtnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("请输入供应商名称", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        if (string.IsNullOrWhiteSpace(txtBaseUrl.Text))
        {
            MessageBox.Show("请输入基础URL", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        try
        {
            var isNew = (_currentProvider == null);
            
            if (isNew)
            {
                _currentProvider = new AiProviderConfig();
            }
            
            _currentProvider.Name = txtName.Text;
            _currentProvider.ProviderType = (AiProvider)cboProviderType.SelectedItem;
            _currentProvider.BaseUrl = txtBaseUrl.Text;
            _currentProvider.ApiKey = txtApiKey.Text;
            _currentProvider.DefaultModel = txtDefaultModel.Text;
            _currentProvider.AzureDeploymentId = string.IsNullOrWhiteSpace(txtAzureDeploymentId.Text) ? null : txtAzureDeploymentId.Text;
            _currentProvider.ApiVersion = string.IsNullOrWhiteSpace(txtApiVersion.Text) ? "2023-05-15" : txtApiVersion.Text;
            _currentProvider.IsDefault = chkIsDefault.Checked;
            
            if (isNew)
            {
                _providerService.AddProvider(_currentProvider);
            }
            else
            {
                _providerService.UpdateProvider(_currentProvider);
            }
            
            LoadProviders();
            DisableEditControls();
            
            // 选择刚保存的项目
            foreach (DataGridViewRow row in dgvProviders.Rows)
            {
                var provider = row.DataBoundItem as AiProviderConfig;
                if (provider != null && provider.Id == _currentProvider.Id)
                {
                    row.Selected = true;
                    dgvProviders.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    // 声明控件
    private DataGridView dgvProviders;
    private Button btnAdd, btnEdit, btnDelete, btnSetDefault, btnSave;
    private TextBox txtName, txtBaseUrl, txtApiKey, txtDefaultModel, txtAzureDeploymentId, txtApiVersion;
    private ComboBox cboProviderType;
    private CheckBox chkIsDefault;
}

// 设计器部分（简化版，实际使用时由WinForms设计器自动生成）
partial class ProviderManagerForm
{
    private System.ComponentModel.IContainer components = null;
    
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
    
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 600);
        this.Text = "API供应商管理";
    }
}