// UI/MultiChatForm.cs
using System.Windows.Forms;
using System.Collections.Generic;
using AIChatAssistant.Models;

public partial class MultiChatForm : Form
{
    private TabControl _chatTabs = new TabControl(); // 初始化字段
    private Dictionary<TabPage, Conversation> _tabSessions = new Dictionary<TabPage, Conversation>(); // 使用Conversation代替ChatSession

    public MultiChatForm()
    {
        // 简单构造函数，不依赖InitializeComponent
    }

    public void AddNewChatTab()
    {
        // 实现多标签对话的占位方法
    }
}