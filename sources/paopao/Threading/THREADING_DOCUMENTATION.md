# Threading 模块详细功能文档

## 概述

Threading 模块是卡丁车游戏项目中的应用生命周期管理系统，主要负责处理应用的前台/后台切换事件，管理系统级的状态变化，确保游戏在不同应用状态下的正确行为。虽然名为Threading，但实际上是一个应用状态管理模块。

## 模块结构

```
Threading/
├── BackgroundDelegate.cs     - 后台事件委托定义
└── BackgroundNotifier.cs     - 后台通知管理器
```

## 系统架构图

```
应用生命周期事件
        ↓
BackgroundNotifier (事件分发中心)
        ↓
┌─────────────────────────────────────────┐
│                事件处理                  │
├─────────────────────────────────────────┤
│ WillResignActive() - 即将失去焦点        │
│ DidEnterBackground() - 已进入后台        │
│ DidBecomeActive() - 已变为活动状态       │
│ WillEnterForeground() - 即将进入前台     │
└─────────────────────────────────────────┘
        ↓
┌─────────────────────────────────────────┐
│              系统响应                    │
├─────────────────────────────────────────┤
│ StageController - 阶段控制器             │
│ KartManager - 卡丁车管理器               │
│ KartOptions - 游戏选项（音效等）         │
│ BackgroundDelegate - 自定义回调          │
└─────────────────────────────────────────┘
```

## 核心类详细分析

### 1. BackgroundDelegate.cs - 后台事件委托定义

**功能概述：**
定义用于后台事件处理的委托类型，提供标准的回调接口。

**完整定义：**
```csharp
public delegate void BackgroundDelegate();
```

**用途说明：**
- **类型**: 无参数、无返回值的委托
- **用途**: 定义应用状态切换时的回调函数签名
- **优势**: 提供类型安全的事件处理机制
- **应用场景**: 
  - 游戏暂停/恢复逻辑
  - 资源管理和释放
  - 网络连接状态处理
  - 音效系统控制

**使用示例：**
```csharp
// 注册后台事件处理
BackgroundNotifier.resign_ += OnGameWillResign;
BackgroundNotifier.active_ += OnGameDidBecomeActive;

// 事件处理函数
private void OnGameWillResign()
{
    // 保存游戏状态
    SaveGameProgress();
    // 暂停音效
    AudioManager.Pause();
}

private void OnGameDidBecomeActive()
{
    // 恢复游戏状态
    ResumeGameProgress();
    // 恢复音效
    AudioManager.Resume();
}
```

### 2. BackgroundNotifier.cs - 后台通知管理器

**功能概述：**
应用生命周期事件的核心管理器，负责监听系统级状态变化并分发给相关模块。

**核心数据结构：**
```csharp
public class BackgroundNotifier
{
    public static BackgroundDelegate resign_;   // 失去焦点时的回调委托
    public static BackgroundDelegate active_;   // 获得焦点时的回调委托
}
```

#### 2.1 应用状态管理接口

**WillResignActive() - 即将失去活动状态：**
```csharp
public static void WillResignActive()
{
    // 当前实现为空，预留给将来扩展
    // 可以在这里添加即将失去焦点时的预处理逻辑
}
```

**应用场景：**
- 游戏即将被其他应用覆盖时
- 用户即将切换到其他应用时
- 系统级弹窗（如电话）即将出现时

**典型用途：**
```csharp
public static void WillResignActive()
{
    // 预处理逻辑示例
    if (GameManager.IsInGame())
    {
        GameManager.PrepareToPause();
    }
    
    // 保存关键数据
    DataManager.SaveCriticalData();
    
    // 减少CPU使用
    PerformanceManager.ReduceFrameRate();
}
```

#### 2.2 后台状态处理

**DidEnterBackground() - 已进入后台状态：**
```csharp
public static void DidEnterBackground()
{
    // 通知阶段控制器
    if (StageController.IsInstantiated())
    {
        StageController.Instance.WillResignActive();
    }
    
    // 执行自定义后台回调
    if (BackgroundNotifier.resign_ != null)
    {
        BackgroundNotifier.resign_();
    }
    
    // 通知卡丁车管理器
    KartManager.DidEnterBackground();
}
```

**处理流程分析：**

1. **阶段控制器处理**：
   ```csharp
   if (StageController.IsInstantiated())
   {
       StageController.Instance.WillResignActive();
   }
   ```
   - 检查阶段控制器是否已实例化
   - 调用阶段控制器的后台处理方法
   - 确保游戏状态正确保存

2. **自定义回调执行**：
   ```csharp
   if (BackgroundNotifier.resign_ != null)
   {
       BackgroundNotifier.resign_();
   }
   ```
   - 执行所有注册的后台回调函数
   - 允许各模块进行自定义的后台处理
   - 支持多个回调的链式调用

3. **卡丁车管理器通知**：
   ```csharp
   KartManager.DidEnterBackground();
   ```
   - 专门处理卡丁车相关的后台逻辑
   - 可能包括物理模拟暂停、AI行为调整等

#### 2.3 前台激活处理

**DidBecomeActive() - 已变为活动状态：**
```csharp
public static void DidBecomeActive()
{
    // iOS平台特殊处理：检查iPod音乐播放状态
    if (Application.platform == RuntimePlatform.IPhonePlayer)
    {
        KartOptions.Instance.Bgm = !iOSUtil.IsIPodPlaying();
    }
    
    // 通知阶段控制器
    if (StageController.IsInstantiated())
    {
        StageController.Instance.DidBecomeActive();
    }
    
    // 执行自定义激活回调
    if (BackgroundNotifier.active_ != null)
    {
        BackgroundNotifier.active_();
    }
}
```

**处理流程详解：**

1. **平台特定音频处理**：
   ```csharp
   if (Application.platform == RuntimePlatform.IPhonePlayer)
   {
       KartOptions.Instance.Bgm = !iOSUtil.IsIPodPlaying();
   }
   ```
   - **平台检测**: 仅在iOS设备上执行
   - **音频冲突处理**: 检查用户是否在播放iPod音乐
   - **智能音效控制**: 如果用户在听音乐，自动关闭游戏背景音乐
   - **用户体验优化**: 避免音频冲突，提供更好的用户体验

2. **阶段控制器恢复**：
   ```csharp
   if (StageController.IsInstantiated())
   {
       StageController.Instance.DidBecomeActive();
   }
   ```
   - 恢复游戏阶段的正常状态
   - 重新激活游戏逻辑处理
   - 恢复渲染和更新循环

3. **自定义激活回调**：
   ```csharp
   if (BackgroundNotifier.active_ != null)
   {
       BackgroundNotifier.active_();
   }
   ```
   - 执行所有注册的激活回调
   - 允许各模块恢复其活动状态
   - 支持复杂的恢复逻辑

#### 2.4 前台准备处理

**WillEnterForeground() - 即将进入前台：**
```csharp
public static void WillEnterForeground()
{
    KartManager.WillEnterForeground();
}
```

**功能说明：**
- **预备处理**: 在完全激活前进行准备工作
- **卡丁车管理器通知**: 让卡丁车系统提前准备
- **性能优化**: 可以在此阶段预加载资源

**扩展用途：**
```csharp
public static void WillEnterForeground()
{
    // 卡丁车管理器准备
    KartManager.WillEnterForeground();
    
    // 扩展功能示例
    ResourceManager.PreloadCriticalAssets();
    NetworkManager.ReestablishConnections();
    PhysicsManager.WarmUpSimulation();
}
```

## 应用生命周期状态图

```
应用启动
    ↓
[Active] ←──→ [Will Resign Active]
    ↓               ↓
[Background] ←──→ [Will Enter Foreground]
    ↓               ↓
[Terminated]    [Active]

状态转换说明：
Active → Will Resign Active: 即将失去焦点
Will Resign Active → Background: 完全进入后台
Background → Will Enter Foreground: 即将返回前台
Will Enter Foreground → Active: 完全激活
```

## 事件处理最佳实践

### 1. 注册回调的正确方式

**推荐方式：**
```csharp
public class GameModule : MonoBehaviour
{
    private void Start()
    {
        // 注册事件处理
        BackgroundNotifier.resign_ += OnGameResign;
        BackgroundNotifier.active_ += OnGameActive;
    }
    
    private void OnDestroy()
    {
        // 清理事件注册，避免内存泄漏
        BackgroundNotifier.resign_ -= OnGameResign;
        BackgroundNotifier.active_ -= OnGameActive;
    }
    
    private void OnGameResign()
    {
        // 游戏失去焦点时的处理
        PauseGame();
        SaveProgress();
    }
    
    private void OnGameActive()
    {
        // 游戏获得焦点时的处理
        ResumeGame();
        RefreshUI();
    }
}
```

### 2. 模块化的状态管理

**系统模块响应示例：**
```csharp
// 音频管理器
public class AudioManager
{
    public static void OnApplicationResign()
    {
        // 保存当前音量状态
        SaveVolumeSettings();
        // 暂停所有音效
        PauseAllSounds();
        // 释放音频资源
        ReleaseAudioResources();
    }
    
    public static void OnApplicationActive()
    {
        // 恢复音频资源
        RestoreAudioResources();
        // 恢复音量状态
        RestoreVolumeSettings();
        // 恢复适当的音效
        ResumeAppropiateSounds();
    }
}

// 网络管理器
public class NetworkManager
{
    public static void OnApplicationResign()
    {
        // 保存网络状态
        SaveConnectionState();
        // 暂停非关键网络请求
        PauseNonCriticalRequests();
        // 发送心跳包
        SendKeepAlivePacket();
    }
    
    public static void OnApplicationActive()
    {
        // 检查网络连接
        CheckNetworkConnectivity();
        // 恢复网络请求
        ResumeNetworkRequests();
        // 同步数据
        SynchronizeData();
    }
}
```

### 3. 错误处理和容错机制

**安全的事件处理：**
```csharp
public static void SafeExecuteDelegate(BackgroundDelegate del)
{
    if (del != null)
    {
        try
        {
            del();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Background event execution failed: {ex.Message}");
            // 记录错误但不中断其他处理
        }
    }
}

// 在BackgroundNotifier中使用
public static void DidEnterBackground()
{
    if (StageController.IsInstantiated())
    {
        try
        {
            StageController.Instance.WillResignActive();
        }
        catch (Exception ex)
        {
            Debug.LogError($"StageController resign failed: {ex.Message}");
        }
    }
    
    SafeExecuteDelegate(BackgroundNotifier.resign_);
    
    try
    {
        KartManager.DidEnterBackground();
    }
    catch (Exception ex)
    {
        Debug.LogError($"KartManager background transition failed: {ex.Message}");
    }
}
```

## 性能考虑和优化

### 1. 资源管理优化

**后台资源释放策略：**
```csharp
public class ResourceOptimizer
{
    public static void OnEnterBackground()
    {
        // 释放大型纹理资源
        TextureManager.ReleaseLargeTextures();
        
        // 清理音频缓存
        AudioCache.ClearNonEssentialCache();
        
        // 减少物理模拟精度
        Physics.bounceThreshold = 10f;
        Physics.sleepThreshold = 0.5f;
        
        // 降低渲染质量
        QualitySettings.pixelLightCount = 1;
        QualitySettings.shadows = ShadowQuality.Disable;
    }
    
    public static void OnBecomeActive()
    {
        // 恢复高质量设置
        RestoreQualitySettings();
        
        // 重新加载关键资源
        TextureManager.ReloadCriticalTextures();
        
        // 恢复物理精度
        RestorePhysicsSettings();
    }
}
```

### 2. 数据同步优化

**智能数据保存：**
```csharp
public class DataSyncManager
{
    private static bool hasUnsavedChanges = false;
    
    public static void OnApplicationResign()
    {
        if (hasUnsavedChanges)
        {
            // 只在有未保存更改时才执行保存
            SaveGameData();
            hasUnsavedChanges = false;
        }
        
        // 保存关键设置
        SaveCriticalSettings();
    }
    
    public static void MarkDataDirty()
    {
        hasUnsavedChanges = true;
    }
}
```

## 平台特定处理

### 1. iOS平台优化

**iOS音频集成：**
```csharp
public static class iOSAudioHandler
{
    public static void HandleApplicationActivation()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // 检查系统音频状态
            bool isIPodPlaying = iOSUtil.IsIPodPlaying();
            bool hasHeadphones = iOSUtil.HasHeadphones();
            
            // 智能音频策略
            if (isIPodPlaying)
            {
                // 用户在听音乐，关闭游戏音乐
                KartOptions.Instance.Bgm = false;
                KartOptions.Instance.Sfx = true; // 保留音效
            }
            else if (hasHeadphones)
            {
                // 有耳机但没播放音乐，启用全音频
                KartOptions.Instance.Bgm = true;
                KartOptions.Instance.Sfx = true;
            }
            else
            {
                // 使用扬声器，根据用户设置
                RestoreUserAudioPreferences();
            }
        }
    }
}
```

### 2. Android平台优化

**Android生命周期处理：**
```csharp
public static class AndroidLifecycleHandler
{
    public static void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 应用被暂停
            BackgroundNotifier.DidEnterBackground();
        }
        else
        {
            // 应用恢复
            BackgroundNotifier.DidBecomeActive();
        }
    }
    
    public static void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            BackgroundNotifier.WillEnterForeground();
        }
        else
        {
            BackgroundNotifier.WillResignActive();
        }
    }
}
```

## 调试和监控

### 1. 状态变化日志

**详细日志记录：**
```csharp
public static class ApplicationStateLogger
{
    public static void LogStateChange(string stateName, string details = "")
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string message = $"[{timestamp}] App State: {stateName}";
        
        if (!string.IsNullOrEmpty(details))
        {
            message += $" - {details}";
        }
        
        Debug.Log(message);
        
        // 记录到文件（发布版本中可选）
        #if DEVELOPMENT_BUILD
        FileLogger.WriteLog("app_state.log", message);
        #endif
    }
}

// 在BackgroundNotifier中使用
public static void DidEnterBackground()
{
    ApplicationStateLogger.LogStateChange("DidEnterBackground", 
        $"Stage: {StageController.CurrentStage}, Players: {KartManager.PlayerCount}");
    
    // 原有逻辑...
}
```

### 2. 性能监控

**状态切换性能追踪：**
```csharp
public static class PerformanceTracker
{
    public static void TrackStateTransition(string stateName, Action action)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            action();
        }
        finally
        {
            stopwatch.Stop();
            Debug.Log($"State transition '{stateName}' took {stopwatch.ElapsedMilliseconds}ms");
            
            // 记录性能异常
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                Debug.LogWarning($"Slow state transition detected: {stateName}");
            }
        }
    }
}
```

## 扩展和集成建议

### 1. 事件系统扩展

**增强的事件管理器：**
```csharp
public static class EnhancedBackgroundNotifier
{
    public static event Action<ApplicationState> OnStateChanged;
    
    public enum ApplicationState
    {
        Active,
        WillResignActive,
        Background,
        WillEnterForeground
    }
    
    private static ApplicationState currentState = ApplicationState.Active;
    
    public static ApplicationState CurrentState => currentState;
    
    private static void ChangeState(ApplicationState newState)
    {
        if (currentState != newState)
        {
            ApplicationState oldState = currentState;
            currentState = newState;
            
            OnStateChanged?.Invoke(newState);
            
            Debug.Log($"Application state changed: {oldState} → {newState}");
        }
    }
}
```

### 2. 模块自动注册

**自动化模块管理：**
```csharp
public interface IApplicationStateHandler
{
    void OnWillResignActive();
    void OnDidEnterBackground();
    void OnWillEnterForeground();
    void OnDidBecomeActive();
}

public static class ModuleRegistry
{
    private static List<IApplicationStateHandler> handlers = new List<IApplicationStateHandler>();
    
    public static void RegisterHandler(IApplicationStateHandler handler)
    {
        if (!handlers.Contains(handler))
        {
            handlers.Add(handler);
        }
    }
    
    public static void UnregisterHandler(IApplicationStateHandler handler)
    {
        handlers.Remove(handler);
    }
    
    public static void NotifyAllHandlers(Action<IApplicationStateHandler> action)
    {
        foreach (var handler in handlers)
        {
            try
            {
                action(handler);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Handler {handler.GetType().Name} failed: {ex.Message}");
            }
        }
    }
}
```

## 总结

Threading模块虽然代码量不大，但在游戏架构中扮演着重要的应用生命周期管理角色：

### 核心价值
1. **统一的状态管理**: 提供应用状态变化的统一入口
2. **模块化响应**: 支持各模块独立处理状态变化
3. **平台优化**: 特别是iOS平台的音频冲突处理
4. **资源管理**: 确保后台时合理释放和恢复资源

### 设计特点
1. **简洁性**: 接口简单明了，易于使用和扩展
2. **可靠性**: 通过委托机制确保事件处理的安全性
3. **灵活性**: 支持自定义回调和模块化处理
4. **性能友好**: 最小化后台资源消耗

### 扩展方向
1. **更细粒度的状态管理**: 支持更多应用状态类型
2. **性能监控集成**: 自动追踪状态切换性能
3. **错误恢复机制**: 更强的容错和恢复能力
4. **跨平台优化**: 针对不同平台的特殊处理

该模块为卡丁车游戏提供了稳定可靠的应用状态管理基础，确保了游戏在复杂的移动设备环境中的稳定运行。