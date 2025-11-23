// Services/TrayIconService.cs
using System.Windows.Forms;
using System.Drawing;
using System;

namespace AIChatAssistant.Services;

public class TrayIconService : IDisposable
{
    private NotifyIcon? _trayIcon;
    private ContextMenuStrip? _contextMenu;
    private Form? _mainForm;
    private bool _isDisposed = false;

    // 事件定义
    public event EventHandler? ShowMainFormRequested;
    public event EventHandler? ExitRequested;
    public event EventHandler? NewConversationRequested;
    public event EventHandler? ManageConversationsRequested;

    public TrayIconService(Form mainForm)
    {
        _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
        InitializeTrayIcon();
    }

    private void InitializeTrayIcon()
    {
        // 创建系统托盘图标
        _trayIcon = new NotifyIcon
        {
            Text = "AI对话助手",
            Visible = false, // 初始不可见，在需要时显示
            Icon = CreateDefaultIcon() // 创建默认图标
        };

        // 创建上下文菜单
        _contextMenu = new ContextMenuStrip();
        
        // 添加菜单项
        var showMenuItem = _contextMenu.Items.Add("显示主窗口");
        showMenuItem.Click += (sender, e) => ShowMainForm();

        var newConvMenuItem = _contextMenu.Items.Add("新建对话");
        newConvMenuItem.Click += (sender, e) => OnNewConversationRequested();

        var manageConvMenuItem = _contextMenu.Items.Add("管理对话");
        manageConvMenuItem.Click += (sender, e) => OnManageConversationsRequested();

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = _contextMenu.Items.Add("退出");
        exitMenuItem.Click += (sender, e) => OnExitRequested();

        // 设置托盘图标的上下文菜单
        _trayIcon.ContextMenuStrip = _contextMenu;

        // 双击托盘图标显示主窗口
        _trayIcon.DoubleClick += (sender, e) => ShowMainForm();
    }

    /// <summary>
    /// 创建一个简单的默认图标
    /// </summary>
    private Icon CreateDefaultIcon()
    {
        try
        {
            // 创建一个简单的图标
            using (var bmp = new Bitmap(16, 16))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    // 填充背景
                    g.FillRectangle(Brushes.Blue, 0, 0, 16, 16);
                    // 绘制白色 "AI" 文字
                    using (var font = new Font("Arial", 7, FontStyle.Bold))
                    {
                        g.DrawString("AI", font, Brushes.White, 1, 2);
                    }
                }
                // 转换为图标
                return Icon.FromHandle(bmp.GetHicon());
            }
        }
        catch
        {
            // 如果创建自定义图标失败，返回默认图标
            return SystemIcons.Application;
        }
    }

    /// <summary>
    /// 显示系统托盘图标
    /// </summary>
    public void ShowTrayIcon()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = true;
        }
    }

    /// <summary>
    /// 隐藏系统托盘图标
    /// </summary>
    public void HideTrayIcon()
    {
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
        }
    }

    /// <summary>
    /// 显示主窗口
    /// </summary>
    public void ShowMainForm()
    {
        if (_mainForm != null)
        {
            _mainForm.Show();
            _mainForm.WindowState = FormWindowState.Normal;
            _mainForm.Activate();
        }
        
        // 触发事件通知订阅者
        ShowMainFormRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 发送托盘通知
    /// </summary>
    /// <param name="title">通知标题</param>
    /// <param name="message">通知消息</param>
    /// <param name="timeout">通知显示时间（毫秒）</param>
    public void ShowNotification(string title, string message, int timeout = 3000)
    {
        if (_trayIcon != null && _trayIcon.Visible)
        {
            _trayIcon.BalloonTipTitle = title;
            _trayIcon.BalloonTipText = message;
            _trayIcon.ShowBalloonTip(timeout);
        }
    }

    /// <summary>
    /// 触发新建对话事件
    /// </summary>
    private void OnNewConversationRequested()
    {
        NewConversationRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 触发管理对话事件
    /// </summary>
    private void OnManageConversationsRequested()
    {
        ManageConversationsRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 触发退出事件
    /// </summary>
    private void OnExitRequested()
    {
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 释放托管资源
                if (_trayIcon != null)
                {
                    _trayIcon.Visible = false;
                    _trayIcon.Dispose();
                }
                
                if (_contextMenu != null)
                {
                    _contextMenu.Dispose();
                }
            }

            _isDisposed = true;
        }
    }

    ~TrayIconService()
    {
        Dispose(false);
    }
}