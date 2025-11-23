// Services/ThemeManager.cs
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AIChatAssistant.Services;

public class ThemeManager
{
    public enum Theme
    {
        Light,
        Dark,
        Blue,
        Green
    }

    private Theme _currentTheme = Theme.Light;

    public void ApplyTheme(Control control, Theme theme)
    {
        _currentTheme = theme;
        
        // 先挂起布局
        control.SuspendLayout();
        
        // 递归应用主题
        ApplyThemeRecursive(control, theme);
        
        // 恢复布局并强制重绘
        control.ResumeLayout(false);
        control.Refresh();
        
        // 确保窗体边框也更新
        if (control is Form form)
        {
            UpdateFormBorder(form, theme);
        }
    }

    private void ApplyThemeRecursive(Control control, Theme theme)
    {
        if (control == null) return;

        // 对于非Form控件，使用反射安全地设置双缓冲（避免直接访问受保护属性）
        if (!(control is Form)) // 因为Form类型已经在ThemedForm中处理了双缓冲
        {
            try
            {
                // 使用反射获取并设置DoubleBuffered属性
                var doubleBufferedProperty = typeof(Control).GetProperty("DoubleBuffered", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (doubleBufferedProperty != null)
                {
                    doubleBufferedProperty.SetValue(control, true, null);
                }
                
                // 同样设置ControlStyles.OptimizedDoubleBuffer
                var setStyleMethod = typeof(Control).GetMethod("SetStyle", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.NonPublic);
                
                if (setStyleMethod != null)
                {
                    setStyleMethod.Invoke(control, new object[] {
                        ControlStyles.OptimizedDoubleBuffer | 
                        ControlStyles.AllPaintingInWmPaint | 
                        ControlStyles.UserPaint,
                        true
                    });
                }
                
                // 调用UpdateStyles确保样式应用
                var updateStylesMethod = typeof(Control).GetMethod("UpdateStyles", 
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public);
                
                if (updateStylesMethod != null)
                {
                    updateStylesMethod.Invoke(control, null);
                }
            }
            catch {}
        }
        
        // 刷新控件以确保样式应用
        control.Refresh();

        // 根据主题设置颜色
        var colors = GetThemeColors(theme);
        
        control.BackColor = colors.BackColor;
        control.ForeColor = colors.ForeColor;

        // 特殊处理某些控件
        switch (control)
        {
            case Panel panel:
                panel.BorderStyle = BorderStyle.None;
                break;
                
            case GroupBox groupBox:
                groupBox.ForeColor = colors.ForeColor;
                break;
                
            case Button button:
                button.BackColor = colors.ControlColor;
                button.ForeColor = colors.ForeColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                break;
                
            case TextBox textBox:
                textBox.BackColor = colors.ControlColor;
                textBox.ForeColor = colors.ForeColor;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;
                
            case RichTextBox richTextBox:
                richTextBox.BackColor = colors.ControlColor;
                richTextBox.ForeColor = colors.ForeColor;
                break;
                
            case ListBox listBox:
                listBox.BackColor = colors.ControlColor;
                listBox.ForeColor = colors.ForeColor;
                break;
                
            case ComboBox comboBox:
                comboBox.BackColor = colors.ControlColor;
                comboBox.ForeColor = colors.ForeColor;
                break;
        }

        // 递归处理子控件
        foreach (Control childControl in control.Controls)
        {
            ApplyThemeRecursive(childControl, theme);
        }
    }

    private (Color BackColor, Color ForeColor, Color ControlColor) GetThemeColors(Theme theme)
    {
        return theme switch
        {
            Theme.Dark => (Color.FromArgb(45, 45, 48), Color.White, Color.FromArgb(63, 63, 70)),
            Theme.Blue => (Color.FromArgb(240, 245, 255), Color.FromArgb(30, 30, 30), Color.White),
            Theme.Green => (Color.FromArgb(240, 255, 245), Color.FromArgb(30, 30, 30), Color.White),
            _ => (SystemColors.Window, SystemColors.WindowText, SystemColors.Control)
        };
    }

    private void UpdateFormBorder(Form form, Theme theme)
    {
        // 更新窗体边框颜色
        form.Refresh();
        
        // 强制重绘非客户区（边框）
        NativeMethods.SetWindowPos(form.Handle, IntPtr.Zero, 0, 0, 0, 0,
            NativeMethods.SWP_FRAMECHANGED | NativeMethods.SWP_NOMOVE | 
            NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER);
    }

    internal Color GetThemeColors(int currentTheme)
    {
        throw new NotImplementedException();
    }
}

internal static class NativeMethods
{
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_FRAMECHANGED = 0x0020;

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, 
        int X, int Y, int cx, int cy, uint uFlags);
}