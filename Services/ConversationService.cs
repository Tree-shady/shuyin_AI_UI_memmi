// Services/ConversationService.cs
using AIChatAssistant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using AIChatAssistant.Services; // 添加对DebugService的引用

namespace AIChatAssistant.Services;

public class ConversationService : IConversationService, IDisposable
{
    private readonly List<Conversation> _conversations = new List<Conversation>();
    private string? _activeConversationId = null;
    private readonly string _conversationsFilePath;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false, // 关闭缩进以提高性能
        PropertyNameCaseInsensitive = true,
        MaxDepth = 64 // 添加最大深度限制
    };
    private readonly SemaphoreSlim _fileAccessLock = new SemaphoreSlim(1, 1); // 用于文件操作的线程锁
    private bool _isDisposed = false;
    private Task? _saveTask = null;
    private CancellationTokenSource? _saveCancellationTokenSource;
    private const int SaveDelayMs = 300; // 防抖延迟，避免频繁保存
    
    public ConversationService()
    {
        // 设置对话保存路径
        // 使用AppData目录而不是程序目录，避免权限问题
        var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AIChatAssistant", "data");
        _conversationsFilePath = Path.Combine(appDataDir, "conversations.json");
        
        // 确保目录存在
        Directory.CreateDirectory(appDataDir);
        
        // 加载已保存的对话
        // 异步加载但不等待完成，避免启动时阻塞
        _ = LoadConversationsAsync().ContinueWith(task =>
        {
            if (task.IsFaulted && task.Exception != null)
            {
                DebugService.Instance.LogWarning("ConversationService", $"异步加载对话异常: {task.Exception.Message}");
            }
        });
    }
    
    /// <summary>
    /// 从文件加载对话
    /// </summary>
    private async Task LoadConversationsAsync()
    {
        try
        {
            await _fileAccessLock.WaitAsync();
            
            if (File.Exists(_conversationsFilePath))
            {
                // 使用异步读取文件以避免阻塞UI线程
                var json = await File.ReadAllTextAsync(_conversationsFilePath);
                var savedData = JsonSerializer.Deserialize<ConversationData>(json, _jsonOptions);
                
                if (savedData != null)
                {
                    // 确保在锁内修改集合
                    lock (_conversations)
                    {
                        _conversations.Clear();
                        _conversations.AddRange(savedData.Conversations);
                        _activeConversationId = savedData.ActiveConversationId;
                    }
                    
                    DebugService.Instance.LogInfo("ConversationService", $"成功加载{savedData.Conversations.Count}个对话");
                }
            }
        }
        catch (Exception ex)
        {
            // 记录更详细的异常信息
            DebugService.Instance.LogWarning("ConversationService", $"加载对话失败: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            _fileAccessLock.Release();
        }
    }
    
    /// <summary>
    /// 保存对话到文件
    /// </summary>
    private async Task SaveConversationsAsync()
    {
        if (_isDisposed)
            return;
            
        try
        {
            await _fileAccessLock.WaitAsync();
            
            // 创建副本以避免在序列化时锁定集合
            List<Conversation> conversationsCopy;
            string? activeConversationIdCopy;
            
            lock (_conversations)
            {
                conversationsCopy = _conversations.ToList();
                activeConversationIdCopy = _activeConversationId;
            }
            
            var data = new ConversationData
            {
                Conversations = conversationsCopy,
                ActiveConversationId = activeConversationIdCopy
            };
            
            // 序列化到内存流可以提高性能
            using (var memoryStream = new System.IO.MemoryStream())
            {
                await JsonSerializer.SerializeAsync(memoryStream, data, _jsonOptions);
                memoryStream.Position = 0;
                
                // 确保目录存在
                var directory = Path.GetDirectoryName(_conversationsFilePath);
                if (directory != null)
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 先写入临时文件，再替换，避免文件损坏
                var tempFilePath = _conversationsFilePath + ".tmp";
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await memoryStream.CopyToAsync(fileStream);
                }
                
                // 如果原文件存在，先删除
                if (File.Exists(_conversationsFilePath))
                {
                    File.Delete(_conversationsFilePath);
                }
                
                // 将临时文件重命名为目标文件
                File.Move(tempFilePath, _conversationsFilePath);
            }
            
            DebugService.Instance.LogDebug("ConversationService", $"成功保存{conversationsCopy.Count}个对话");
        }
        catch (Exception ex)
        {
            // 记录更详细的异常信息
            DebugService.Instance.LogWarning("ConversationService", $"保存对话失败: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            _fileAccessLock.Release();
        }
    }
    
    /// <summary>
    /// 用于序列化的对话数据类
    /// </summary>
    private class ConversationData
    {
        public List<Conversation> Conversations { get; set; } = new List<Conversation>();
        public string? ActiveConversationId { get; set; }
    }
    
    // 创建新会话
    public Conversation CreateConversation()
    {
        var conversation = new Conversation();
        
        lock (_conversations)
        {
            _conversations.Add(conversation);
            _activeConversationId = conversation.Id;
        }
        
        QueueSave();
        return conversation;
    }
    
    // 获取所有会话
    public List<Conversation> GetAllConversations()
    {
        lock (_conversations)
        {
            return _conversations.OrderByDescending(c => c.LastModifiedAt).ToList();
        }
    }
    
    // 通过ID获取会话
    public Conversation? GetConversationById(string id)
    {
        lock (_conversations)
        {
            return _conversations.FirstOrDefault(c => c.Id == id);
        }
    }
    
    // 更新会话
    public void UpdateConversation(Conversation conversation)
    {
        lock (_conversations)
        {
            var existingConversation = _conversations.FirstOrDefault(c => c.Id == conversation.Id);
            if (existingConversation != null)
            {
                // 更新现有会话的属性
                existingConversation.Title = conversation.Title;
                existingConversation.LastModifiedAt = conversation.LastModifiedAt;
                existingConversation.Messages = conversation.Messages;
            }
        }
        
        QueueSave();
    }
    
    // 删除会话
    public bool DeleteConversation(string id)
    {
        bool result = false;
        
        lock (_conversations)
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                _conversations.Remove(conversation);
                
                // 如果删除的是活动会话，则清除活动会话ID
                if (_activeConversationId == id)
                {
                    _activeConversationId = null;
                    // 如果还有其他会话，设置最新的一个为活动会话
                    if (_conversations.Any())
                    {
                        _activeConversationId = _conversations.OrderByDescending(c => c.LastModifiedAt).First().Id;
                    }
                }
                result = true;
            }
        }
        
        if (result)
        {
            QueueSave();
        }
        
        return result;
    }
    
    // 批量删除会话
    public bool DeleteConversations(IEnumerable<string> conversationIds)
    {
        if (conversationIds == null || !conversationIds.Any())
            return false;
            
        bool result = false;
        
        lock (_conversations)
        {
            var idsToDelete = new HashSet<string>(conversationIds);
            var conversationsToDelete = _conversations.Where(c => idsToDelete.Contains(c.Id)).ToList();
            
            if (conversationsToDelete.Count > 0)
            {
                // 删除匹配的会话
                foreach (var conversation in conversationsToDelete)
                {
                    _conversations.Remove(conversation);
                }
                
                // 如果删除的会话中包含当前活动会话，则重新设置活动会话
                if (idsToDelete.Contains(_activeConversationId))
                {
                    _activeConversationId = null;
                    // 如果还有其他会话，设置最新的一个为活动会话
                    if (_conversations.Any())
                    {
                        _activeConversationId = _conversations.OrderByDescending(c => c.LastModifiedAt).First().Id;
                    }
                }
                
                result = true;
            }
        }
        
        if (result)
        {
            QueueSave();
        }
        
        return result;
    }
    
    // 获取当前活动会话
    public Conversation? GetActiveConversation()
    {
        if (_activeConversationId == null)
        {
            return null;
        }
        
        lock (_conversations)
        {
            return _conversations.FirstOrDefault(c => c.Id == _activeConversationId);
        }
    }
    
    // 设置当前活动会话
    public void SetActiveConversation(string conversationId)
    {
        lock (_conversations)
        {
            if (_conversations.Any(c => c.Id == conversationId))
            {
                _activeConversationId = conversationId;
                QueueSave();
            }
        }
    }
    
    // 向会话添加消息
    public void AddMessageToConversation(string conversationId, ChatMessage message)
    {
        bool updated = false;
        
        lock (_conversations)
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation != null)
            {
                conversation.AddMessage(message);
                updated = true;
            }
        }
        
        if (updated)
        {
            QueueSave();
        }
    }
    
    // 更新会话标题
    public void UpdateConversationTitle(string conversationId, string newTitle)
    {
        bool updated = false;
        
        lock (_conversations)
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation != null)
            {
                conversation.UpdateTitle(newTitle);
                updated = true;
            }
        }
        
        if (updated)
        {
            QueueSave();
        }
    }
    
    /// <summary>
    /// 队列保存操作，实现防抖功能
    /// </summary>
    private void QueueSave()
    {
        // 取消之前的保存任务
        if (_saveCancellationTokenSource != null)
        {
            _saveCancellationTokenSource.Cancel();
            _saveCancellationTokenSource.Dispose();
        }
        
        // 创建新的取消令牌
        _saveCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _saveCancellationTokenSource.Token;
        
        // 延迟执行保存操作
        _saveTask = Task.Delay(SaveDelayMs, cancellationToken).ContinueWith(async _ =>
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await SaveConversationsAsync();
            }
        }, cancellationToken);
    }
    
    /// <summary>
    /// 立即保存所有对话（同步）
    /// </summary>
    public void SaveAllConversations()
    {
        // 同步调用异步方法
        SaveConversationsAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// 实现IDisposable接口
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// 清理资源
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // 取消任何挂起的保存任务
                if (_saveCancellationTokenSource != null)
                {
                    _saveCancellationTokenSource.Cancel();
                    _saveCancellationTokenSource.Dispose();
                }
                
                // 等待保存任务完成
                if (_saveTask != null && !_saveTask.IsCompleted)
                {
                    try
                    {
                        _saveTask.Wait(1000); // 等待最多1秒
                    }
                    catch (Exception ex)
                    {
                        DebugService.Instance.LogWarning("ConversationService", $"等待保存任务完成时出错: {ex.Message}");
                    }
                }
                
                // 释放信号量
                _fileAccessLock.Dispose();
            }
            
            _isDisposed = true;
        }
    }
    
    /// <summary>
    /// 析构函数
    /// </summary>
    ~ConversationService()
    {
        Dispose(false);
    }
}