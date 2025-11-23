// UI/SplashScreen.cs
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System;
using System.IO;

namespace AIChatAssistant.UI;

/// <summary>
/// 启动界面类
/// 用于在应用程序启动时显示加载信息
/// </summary>
public class SplashScreen : Form
{
    private Label? _titleLabel;
    private Label? _statusLabel;
    private Label? _versionLabel;
    private int _progressValue;
    private Label? _progressLabel;
    private Panel? _progressBar;
    private Panel? _progressFill;
    private PictureBox? _iconPictureBox;
    private Label? _copyrightLabel; // 添加版权标签
    private System.Windows.Forms.Timer? _timer;

    /// <summary>
    /// 构造函数
    /// </summary>
    public SplashScreen()
    {
        // 设置启动界面的基本属性
        InitializeComponent();
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None; // 无边框窗口
        BackColor = Color.White;
        Size = new Size(520, 320);
        ShowInTaskbar = false; // 不在任务栏显示
        DoubleBuffered = true; // 启用双缓冲，减少闪烁
        
        // 添加圆角和阴影效果
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.White;
        
        // 添加淡出效果
        Opacity = 0;
        FadeIn();
    }
    
    /// <summary>
    /// 重写OnPaint方法，添加圆角边框效果
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        // 创建圆角矩形
        Rectangle bounds = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
        using (GraphicsPath path = new GraphicsPath())
        {
            int radius = 15;
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            
            // 设置窗口区域，实现圆角效果
            this.Region = new Region(path);
            
            // 绘制边框
            using (Pen pen = new Pen(Color.LightGray, 1))
            {
                e.Graphics.DrawPath(pen, path);
            }
        }
    }
    
    /// <summary>
    /// 设置启动界面的图标
    /// </summary>
    /// <param name="iconPath">图标文件路径</param>
    public void SetIcon(string iconPath)
    {
        if (_iconPictureBox != null && File.Exists(iconPath))
        {
            try
            {
                _iconPictureBox.Image = Image.FromFile(iconPath);
                _iconPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                _iconPictureBox.Visible = true;
            }
            catch (Exception)
            {
                // 图标加载失败，保持默认状态
                _iconPictureBox.Visible = false;
            }
        }
    }
    
    /// <summary>
    /// 设置启动界面的标题
    /// </summary>
    /// <param name="title">标题文本</param>
    public void SetTitle(string title)
    {
        if (_titleLabel != null)
        {
            _titleLabel.Text = title;
            // 自动调整位置使标题居中
            _titleLabel.Location = new Point(
                (this.ClientSize.Width - _titleLabel.Width) / 2,
                _titleLabel.Location.Y);
        }
    }

    /// <summary>
    /// 设置启动界面的状态文本
    /// </summary>
    /// <param name="status">状态文本</param>
    public void SetStatus(string status)
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = status;
            // 自动调整位置使状态文本居中
            _statusLabel.Location = new Point(
                (this.ClientSize.Width - _statusLabel.Width) / 2,
                _statusLabel.Location.Y);
        }
    }

    /// <summary>
    /// 设置启动界面的版本号
    /// </summary>
    /// <param name="version">版本号</param>
    public void SetVersion(string version)
    {
        if (_versionLabel != null)
        {
            _versionLabel.Text = $"版本 {version}";
        }
    }
    
    /// <summary>
    /// 设置启动界面的版权信息
    /// </summary>
    /// <param name="copyright">版权文本</param>
    public void SetCopyright(string copyright)
    {
        if (_copyrightLabel != null)
        {
            _copyrightLabel.Text = copyright;
        }
    }

    /// <summary>
    /// 初始化UI组件
    /// </summary>
    private void InitializeComponent()
    {
        // 创建图标控件
        _iconPictureBox = new PictureBox
        {
            Size = new Size(70, 70),
            Location = new Point(55, 75),
            Visible = true, // 默认显示
            BorderStyle = BorderStyle.None
        };
        
        // 创建标题标签 - 优化字体和颜色
        _titleLabel = new Label
        {
            Text = "AI 对话助手",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            AutoSize = true,
            Location = new Point(145, 80),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 创建状态标签 - 优化字体和颜色
        _statusLabel = new Label
        {
            Text = "正在初始化...",
            Font = new Font("Segoe UI", 9.75F),
            ForeColor = Color.FromArgb(64, 64, 64),
            AutoSize = true,
            Location = new Point(130, 130),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // 创建版本标签 - 优化字体和位置
        _versionLabel = new Label
        {
            Text = "版本 1.0.0",
            Font = new Font("Segoe UI", 8.25F),
            ForeColor = Color.Gray,
            Location = new Point(375, 260),
            Size = new Size(80, 16),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            TextAlign = ContentAlignment.MiddleRight
        };

        // 创建进度标签 - 保持原有功能
        _progressLabel = new Label
        {
            Text = "0%",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DarkGray,
            AutoSize = true,
            Location = new Point(450, 240),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };

        // 创建进度条背景 - 优化样式
        _progressBar = new Panel
        {
            Size = new Size(320, 12),
            Location = new Point(90, 170),
            BackColor = Color.FromArgb(230, 230, 230),
            BorderStyle = BorderStyle.None,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        // 创建进度填充 - 优化颜色
        _progressFill = new Panel
        {
            Size = new Size(0, 12),
            Location = new Point(0, 0),
            BackColor = Color.RoyalBlue
        };
        
        // 创建版权标签 - 新增
        _copyrightLabel = new Label
        {
            Text = "© 2023 AI 对话助手团队",
            Font = new Font("Segoe UI", 7.8F),
            ForeColor = Color.Gray,
            Location = new Point(55, 260),
            Size = new Size(200, 16),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };

        // 将进度填充添加到进度条背景中
        _progressBar.Controls.Add(_progressFill);

        // 添加所有控件到表单
        Controls.Add(_copyrightLabel); // 新增版权标签
        Controls.Add(_iconPictureBox);
        Controls.Add(_titleLabel);
        Controls.Add(_statusLabel);
        Controls.Add(_versionLabel);
        Controls.Add(_progressLabel);
        Controls.Add(_progressBar);

        // 创建计时器用于控制启动界面显示时间
        _timer = new System.Windows.Forms.Timer
        {
            Interval = 50
        };
        _timer.Tick += Timer_Tick;

        // 开始计时器
        _timer.Start();
    }

    /// <summary>
    /// 计时器事件处理
    /// </summary>
    private void Timer_Tick(object? sender, EventArgs e)
    {
        // 更新进度条
        _progressValue += 2;
        if (_progressValue > 100)
        {
            _progressValue = 100;
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            // 启动界面显示完成，淡出并关闭
            FadeOut();
        }

        // 更新进度显示
        if (_progressFill != null && _progressBar != null)
        {
            _progressFill.Width = (_progressBar.Width * _progressValue) / 100;
        }
        if (_progressLabel != null)
        {
            _progressLabel.Text = $"{_progressValue}%";
        }

        // 更新状态文本
        if (_statusLabel != null)
        {
            if (_progressValue <= 25)
                _statusLabel.Text = "正在初始化...";
            else if (_progressValue <= 50)
                _statusLabel.Text = "正在加载配置...";
            else if (_progressValue <= 75)
                _statusLabel.Text = "正在准备资源...";
            else
                _statusLabel.Text = "即将启动...";
        }
    }

    /// <summary>
    /// 淡入效果 - 使用更平滑的加速曲线
    /// </summary>
    private async void FadeIn()
    {
        double currentOpacity = 0;
        double acceleration = 0.02; // 加速度
        double currentStep = 0.02; // 初始步长
        
        while (currentOpacity < 1.0)
        {
            await Task.Delay(10); // 更短的延迟，使动画更流畅
            
            // 使用加速曲线
            currentStep += acceleration;
            if (currentStep > 0.1) // 限制最大步长
                currentStep = 0.1;
                
            currentOpacity += currentStep;
            if (currentOpacity > 1.0)
                currentOpacity = 1.0;
                
            Opacity = currentOpacity;
            
            // 确保UI更新
            Application.DoEvents();
        }
        Opacity = 1.0;
    }

    /// <summary>
    /// 淡出效果并关闭 - 使用平滑的减速曲线
    /// </summary>
    private async void FadeOut()
    {
        double currentOpacity = 1.0;
        double deceleration = 0.01; // 减速度
        double currentStep = 0.05; // 初始步长
        
        while (currentOpacity > 0)
        {
            await Task.Delay(10); // 更短的延迟，使动画更流畅
            
            // 使用减速曲线
            currentStep -= deceleration;
            if (currentStep < 0.02) // 限制最小步长
                currentStep = 0.02;
                
            currentOpacity -= currentStep;
            if (currentOpacity < 0)
                currentOpacity = 0;
                
            Opacity = currentOpacity;
            
            // 确保UI更新
            Application.DoEvents();
        }
        Close();
    }

    /// <summary>
    /// 防止用户通过按Alt+F4关闭启动界面
    /// </summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (Opacity > 0 && _progressValue < 100)
        {
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
