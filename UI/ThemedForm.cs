// UI/ThemedForm.cs
using System.Drawing;
using System.Windows.Forms;
using AIChatAssistant.Services;

namespace AIChatAssistant.UI;

public class ThemedForm : Form
{
    public ThemedForm()
    {
        // 启用双缓冲
        this.DoubleBuffered = true;
        
        // 设置样式减少闪烁
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
               ControlStyles.UserPaint | 
               ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw, true);
        
        // 确保应用样式
        this.UpdateStyles();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        
        // 绘制自定义边框，避免白色边框
        using (var pen = new Pen(GetBorderColor(), 1))
        {
            e.Graphics.DrawRectangle(pen, 
                new Rectangle(0, 0, this.Width - 1, this.Height - 1));
        }
    }

    private Color GetBorderColor()
    {
        // 简化边框颜色获取，不再依赖于ThemeManager
        return SystemColors.WindowFrame;
    }
}