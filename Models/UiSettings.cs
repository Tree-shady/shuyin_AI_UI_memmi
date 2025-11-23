// Models/UiSettings.cs
using System.Drawing;
using System.Windows.Forms;
using AIChatAssistant.Services;

namespace AIChatAssistant.Models;

public class UiSettings
{
    public ThemeManager.Theme CurrentTheme { get; set; } = ThemeManager.Theme.Light;
    public int FontSize { get; set; } = 12;
    public string FontFamily { get; set; } = "微软雅黑";
    public bool AutoStart { get; set; } = false;
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowTimestamps { get; set; } = true;
    public bool UseAnimations { get; set; } = true;
    public FormWindowState WindowState { get; set; } = FormWindowState.Normal;
    public Size WindowSize { get; set; } = new Size(1000, 700);
    public Point WindowLocation { get; set; } = new Point(100, 100);
}
