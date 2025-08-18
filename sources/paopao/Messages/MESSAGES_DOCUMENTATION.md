# Messages 模块详细功能文档

## 概述

Messages 模块是卡丁车游戏项目中的消息传递系统，负责游戏内各组件间的通信和事件传递。该模块实现了一套基于类型的消息系统，支持参数化消息传递、消息工厂模式和统一的错误信息管理，为游戏的模块化架构提供了核心的通信基础设施。

## 模块结构

```
Messages/
├── MonoBehaviourMessage.cs         - 消息基类
├── MonoBehaviourMessage1Param.cs   - 单参数消息泛型类
├── MonoBehaviourMessage2Param.cs   - 双参数消息泛型类
├── MonoBehaviourMessageFactory.cs  - 消息工厂管理器
├── BlackBarMessage.cs              - 黑条显示消息
├── WarpMessage.cs                  - 传送消息
└── ErrorMessages.cs                - 错误信息常量定义
```

## 系统架构图

```
Messages System (消息系统架构)
├─ 消息基础层
│  ├─ MonoBehaviourMessage (消息基类)
│  ├─ MonoBehaviourMessage1Param<T> (单参数泛型)
│  └─ MonoBehaviourMessage2Param<T1,T2> (双参数泛型)
├─ 消息管理层
│  └─ MonoBehaviourMessageFactory (消息工厂)
│     ├─ 单例模式
│     ├─ 消息池管理
│     └─ 类型索引映射
├─ 具体消息层
│  ├─ BlackBarMessage (UI黑条控制)
│  └─ WarpMessage (空间传送)
└─ 错误处理层
   └─ ErrorMessages (本地化错误信息)

消息流向:
发送方 → MessageFactory → Message对象 → 
 → 接收方
```

## 核心类详细分析

### 1. MonoBehaviourMessage.cs - 消息基类

**功能概述：**
MonoBehaviourMessage是所有游戏消息的基类，定义了消息系统的基础结构，包含消息类型标识符，为整个消息传递系统提供统一的接口。

**完整基类实现：**
```csharp
public class MonoBehaviourMessage
{
    // 消息类型标识
    public MonoBehaviourMessageType type_;
    
    // 默认构造函数
    public MonoBehaviourMessage();
    
    // 带类型参数的构造函数
    public MonoBehaviourMessage(MonoBehaviourMessageType type);
}
```

#### 1.1 构造函数实现

**默认构造函数：**
```csharp
public MonoBehaviourMessage()
{
    // 创建无类型的基础消息
}
```

**类型化构造函数：**
```csharp
public MonoBehaviourMessage(MonoBehaviourMessageType type)
{
    this.type_ = type;  // 设置消息类型
}
```

#### 1.2 设计模式分析

**基类设计原则：**
1. **类型安全**: 通过枚举类型确保消息类型的正确性
2. **扩展性**: 为所有具体消息类提供统一的基础
3. **简洁性**: 最小化基类复杂度，专注于类型管理
4. **多态性**: 支持消息的多态处理和统一管理

### 2. MonoBehaviourMessage1Param.cs - 单参数消息泛型类

**功能概述：**
MonoBehaviourMessage1Param是支持单个参数的泛型消息类，允许在消息中携带一个强类型的参数，广泛用于需要传递单一数据的场景。

**完整泛型实现：**
```csharp
public class MonoBehaviourMessage1Param<T> : MonoBehaviourMessage
{
    // 泛型参数
    public T param_;
    
    // 构造函数
    public MonoBehaviourMessage1Param(MonoBehaviourMessageType type) : base(type);
    
    // 参数初始化方法
    public MonoBehaviourMessage1Param<T> Initialize(T param);
}
```

#### 2.1 参数管理

**Initialize() 方法实现：**
```csharp
public MonoBehaviourMessage1Param<T> Initialize(T param)
{
    this.param_ = param;  // 设置参数值
    return this;          // 返回自身，支持链式调用
}
```

**链式调用支持：**
```csharp
// 使用示例
var message = new MonoBehaviourMessage1Param<bool>(MonoBehaviourMessageType.SHOW_UI)
    .Initialize(true);
```

#### 2.2 类型安全特性

**泛型类型优势：**
1. **编译期检查**: 确保参数类型正确性
2. **性能优化**: 避免装箱拆箱操作
3. **智能提示**: IDE能够提供完整的类型信息
4. **重构支持**: 类型更改能够被工具自动发现

#### 2.3 应用场景示例

**常见使用模式：**
```csharp
// UI显示控制
var showUIMessage = new MonoBehaviourMessage1Param<bool>(MonoBehaviourMessageType.SHOW_UI)
    .Initialize(false);

// 道具获取通知
var getItemMessage = new MonoBehaviourMessage1Param<GameItem>(MonoBehaviourMessageType.GET_ITEM)
    .Initialize(GameItem.SPEED_BOOST);

// 目标到达通知
var goalMessage = new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.GOAL_IN)
    .Initialize(playerIndex);

// 数值更新消息
var scoreMessage = new MonoBehaviourMessage1Param<float>(MonoBehaviourMessageType.UPDATE_SCORE)
    .Initialize(newScore);
```

### 3. MonoBehaviourMessage2Param.cs - 双参数消息泛型类

**功能概述：**
MonoBehaviourMessage2Param支持两个不同类型参数的消息传递，适用于需要传递多个相关数据的复杂场景。

**完整双参数实现：**
```csharp
public class MonoBehaviourMessage2Param<T1, T2> : MonoBehaviourMessage
{
    // 双泛型参数
    public T1 lparam_;   // 左参数（第一参数）
    public T2 rparam_;   // 右参数（第二参数）
    
    // 构造函数
    public MonoBehaviourMessage2Param(MonoBehaviourMessageType type) : base(type);
    
    // 双参数初始化方法
    public MonoBehaviourMessage2Param<T1, T2> Initialize(T1 lparam, T2 rparam);
}
```

#### 3.1 双参数管理

**Initialize() 方法实现：**
```csharp
public MonoBehaviourMessage2Param<T1, T2> Initialize(T1 lparam, T2 rparam)
{
    this.lparam_ = lparam;  // 设置第一参数
    this.rparam_ = rparam;  // 设置第二参数
    return this;            // 返回自身，支持链式调用
}
```

#### 3.2 复合数据传递

**双参数应用示例：**
```csharp
// 角色动画控制
var animMessage = new MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>(
    MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION)
    .Initialize(CharacterAnimation.VICTORY, WrapMode.Once);

// 道具应用
var itemMessage = new MonoBehaviourMessage2Param<GameItem, ItemParam>(
    MonoBehaviourMessageType.ITEM)
    .Initialize(GameItem.SPEED_BOOST, itemParameters);

// 排名显示
var rankingMessage = new MonoBehaviourMessage2Param<int, RankingType>(
    MonoBehaviourMessageType.SHOW_RANKING)
    .Initialize(playerRank, RankingType.WEEKLY);

// 新记录通知
var recordMessage = new MonoBehaviourMessage2Param<float, bool>(
    MonoBehaviourMessageType.NEW_RECORD)
    .Initialize(lapTime, isPersonalBest);
```

### 4. MonoBehaviourMessageFactory.cs - 消息工厂管理器

**功能概述：**
MonoBehaviourMessageFactory是消息系统的核心管理器，实现了单例模式和对象池模式，负责创建、管理和分发所有类型的消息对象，优化内存使用和性能。

**完整工厂实现：**
```csharp
public class MonoBehaviourMessageFactory
{
    // 单例实例
    public static MonoBehaviourMessageFactory instance_;
    public static MonoBehaviourMessageFactory Instance { get; }
    
    // 消息对象池
    private MonoBehaviourMessage[] message_;
    
    // 核心方法
    public void Initialize();
    public bool IsInitialized();
    public MonoBehaviourMessage GetMessage(MonoBehaviourMessageType type);
}
```

#### 4.1 单例模式实现

**Instance属性实现：**
```csharp
public static MonoBehaviourMessageFactory Instance
{
    get
    {
        if (MonoBehaviourMessageFactory.instance_ == null)
        {
            MonoBehaviourMessageFactory.instance_ = new MonoBehaviourMessageFactory();
        }
        return MonoBehaviourMessageFactory.instance_;
    }
}
```

#### 4.2 消息池初始化

**Initialize() 完整实现：**
```csharp
public void Initialize()
{
    this.message_ = new MonoBehaviourMessage[]
    {
        // UI控制消息
        new BlackBarMessage(),
        new MonoBehaviourMessage1Param<bool>(MonoBehaviourMessageType.SHOW_UI),
        
        // 相机控制消息
        new ChangeCameraMessage(),
        
        // 动画控制消息
        new MonoBehaviourMessage2Param<CharacterAnimation, WrapMode>(
            MonoBehaviourMessageType.CHANGE_CHARACTER_ANIMATION),
        new MonoBehaviourMessage2Param<KartBodyAnimation, KartAnimationOption>(
            MonoBehaviourMessageType.CHANGE_KART_ANIMATION),
        
        // 游戏事件消息
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.GOAL_IN),
        new MonoBehaviourMessage(MonoBehaviourMessageType.RESET),
        new MonoBehaviourMessage(MonoBehaviourMessageType.PAUSE),
        new MonoBehaviourMessage(MonoBehaviourMessageType.RESUME),
        new MonoBehaviourMessage(MonoBehaviourMessageType.RACE_OVER),
        
        // 道具系统消息
        new MonoBehaviourMessage2Param<GameItem, ItemParam>(MonoBehaviourMessageType.ITEM),
        new MonoBehaviourMessage2Param<GameItem, ApplyItemParam>(MonoBehaviourMessageType.APPLY_ITEM),
        new MonoBehaviourMessage1Param<GameItem>(MonoBehaviourMessageType.GET_ITEM),
        new UpdateGUIItemSlotMessge(),
        
        // 传送和位置消息
        new WarpMessage(),
        new MonoBehaviourMessage2Param<int, int>(MonoBehaviourMessageType.ENTER_USER_SECTION),
        
        // 结果和排名消息
        new MonoBehaviourMessage2Param<bool, bool>(MonoBehaviourMessageType.SHOW_RESULT),
        new MonoBehaviourMessage2Param<float, bool>(MonoBehaviourMessageType.NEW_RECORD),
        new MonoBehaviourMessage2Param<int, RankingType>(MonoBehaviourMessageType.SHOW_RANKING),
        new MonoBehaviourMessage(MonoBehaviourMessageType.UPDATE_RANKING),
        
        // 网络和多人游戏消息
        new MonoBehaviourMessage1Param<GameParamPacket>(MonoBehaviourMessageType.UPDATE_WAITROOM),
        new MonoBehaviourMessage2Param<int, Peer[]>(MonoBehaviourMessageType.UPDATE_WIFI_ROOM_LIST),
        
        // UI界面控制消息
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.UPDATE_SHOPLIST),
        new MonoBehaviourMessage1Param<string>(MonoBehaviourMessageType.UPDATE_STORE_ITEM_INFO),
        new MonoBehaviourMessage2Param<AssetType, int>(MonoBehaviourMessageType.SHOW_INFO),
        
        // 教程系统消息
        new MonoBehaviourMessage(MonoBehaviourMessageType.SHOW_TUTORIAL),
        new MonoBehaviourMessage(MonoBehaviourMessageType.SHOW_TUTORIAL2),
        new MonoBehaviourMessage(MonoBehaviourMessageType.SHOW_TUTORIAL_MULTI),
        
        // 弹窗消息
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.NETWORK_JOIN_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.FACEBOOK_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.RANKING_LOADING_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.WAITING_PLAYERS_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.FB_LOGIN_POPUP_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.PATCH_SUMMARY_POPUP_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.RESTORE_PURCHASES_POPUP_MESSAGE),
        new MonoBehaviourMessage1Param<int>(MonoBehaviourMessageType.PURCHASE_CONFIRM_POPUP_MESSAGE),
        
        // 其他系统消息
        new MonoBehaviourMessage2Param<int, int>(MonoBehaviourMessageType.SIMPLE_MESSAGE),
        new MonoBehaviourMessage1Param<StageType>(MonoBehaviourMessageType.CHANGE_SCENE),
        new MonoBehaviourMessage2Param<int, int>(MonoBehaviourMessageType.ITEM_TO_CTRL),
        new MonoBehaviourMessage1Param<GameStageCommand>(MonoBehaviourMessageType.GAMESTAGE_COMMAND),
        new MonoBehaviourMessage1Param<SoundController.FxType>(MonoBehaviourMessageType.PLAY_SOUND)
    };
}
```

#### 4.3 消息获取机制

**GetMessage() 实现：**
```csharp
public MonoBehaviourMessage GetMessage(MonoBehaviourMessageType type)
{
    return this.message_[(int)type];  // 通过枚举索引直接访问
}
```

**快速访问特性：**
1. **O(1)访问**: 通过数组索引实现常量时间访问
2. **预分配**: 避免运行时内存分配
3. **类型安全**: 枚举确保索引的有效性
4. **缓存友好**: 数组的连续内存布局

#### 4.4 消息使用模式

**标准消息发送流程：**
```csharp
public class MessageSender : MonoBehaviour
{
    public void SendShowUIMessage(bool isVisible)
    {
        // 1. 获取消息对象
        var message = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.SHOW_UI) as MonoBehaviourMessage1Param<bool>;
        
        // 2. 初始化参数
        message.Initialize(isVisible);
        
        // 3. 发送消息
        MonoBehaviourExCenter.Instance.BroadcastMessage(0, message);
    }
    
    public void SendItemMessage(GameItem item, ItemParam param)
    {
        var message = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.ITEM) as MonoBehaviourMessage2Param<GameItem, ItemParam>;
        
        message.Initialize(item, param);
        MonoBehaviourExCenter.Instance.SendMessage(targetId, message);
    }
    
    public void SendWarpMessage(Vector3 position, Quaternion rotation)
    {
        var message = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.WARP) as WarpMessage;
        
        message.Initialize(position, rotation, true, true);
        MonoBehaviourExCenter.Instance.SendMessage(kartId, message);
    }
}
```

### 5. BlackBarMessage.cs - 黑条显示消息

**功能概述：**
BlackBarMessage是专门用于控制游戏UI中黑条显示的消息类，支持平滑过渡和不同显示模式，常用于电影式的画面效果和UI过渡。

**完整实现：**
```csharp
public class BlackBarMessage : MonoBehaviourMessage
{
    // 显示控制参数
    public bool isShow_;             // 是否显示黑条
    public bool isSmooth_;           // 是否平滑过渡
    public bool isShowOnlyBlackBar_; // 是否仅显示黑条
    
    // 构造函数
    public BlackBarMessage() : base(MonoBehaviourMessageType.BLACK_BAR);
    
    // 参数初始化方法
    public BlackBarMessage Initialize(bool isShow, bool isSmooth, bool isShowOnlyBlackBar);
    
    // 字符串表示
    public override string ToString();
}
```

#### 5.1 参数初始化

**Initialize() 实现：**
```csharp
public BlackBarMessage Initialize(bool isShow, bool isSmooth, bool isShowOnlyBlackBar)
{
    this.isShow_ = isShow;                      // 设置显示状态
    this.isSmooth_ = isSmooth;                  // 设置过渡模式
    this.isShowOnlyBlackBar_ = isShowOnlyBlackBar; // 设置显示模式
    return this;                                // 返回自身支持链式调用
}
```

#### 5.2 调试支持

**ToString() 实现：**
```csharp
public override string ToString()
{
    return string.Format("[ {0}/{1}/{2} ]", this.isShow_, this.isSmooth_, this.isShowOnlyBlackBar_);
}
```

#### 5.3 应用场景

**黑条控制示例：**
```csharp
public class CinematicController : MonoBehaviour
{
    public void StartCinematicMode()
    {
        // 显示电影式黑条，平滑过渡
        var message = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.BLACK_BAR) as BlackBarMessage;
        
        message.Initialize(true, true, false);
        MonoBehaviourExCenter.Instance.SendMessage(0, 14, message);
    }
    
    public void EndCinematicMode()
    {
        // 隐藏黑条，平滑过渡
        var message = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.BLACK_BAR) as BlackBarMessage;
        
        message.Initialize(false, true, false);
        MonoBehaviourExCenter.Instance.SendMessage(0, 14, message);
    }
    
    public void ShowLoadingScreen()
    {
        // 显示纯黑屏，无平滑过渡
        var message = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.BLACK_BAR) as BlackBarMessage;
        
        message.Initialize(true, false, true);
        MonoBehaviourExCenter.Instance.SendMessage(0, 14, message);
    }
}
```

### 6. WarpMessage.cs - 传送消息

**功能概述：**
WarpMessage专门用于处理游戏对象的瞬间传送，支持位置、旋转的设置以及物理状态的控制，是游戏中空间传送功能的核心消息。

**完整实现：**
```csharp
public class WarpMessage : MonoBehaviourMessage
{
    // 传送参数
    public Vector3 pos_;       // 目标位置
    public Quaternion rot_;    // 目标旋转
    public bool isFlush_;      // 是否清空状态
    public bool isResetVel_;   // 是否重置速度
    
    // 构造函数
    public WarpMessage() : base(MonoBehaviourMessageType.WARP);
    
    // 参数初始化方法
    public WarpMessage Initialize(Vector3 pos, Quaternion rot, bool flush, bool resetVel);
}
```

#### 6.1 传送参数管理

**Initialize() 实现：**
```csharp
public WarpMessage Initialize(Vector3 pos, Quaternion rot, bool flush, bool resetVel)
{
    this.pos_ = pos;          // 设置目标位置
    this.rot_ = rot;          // 设置目标旋转
    this.isFlush_ = flush;    // 设置状态清空标志
    this.isResetVel_ = resetVel; // 设置速度重置标志
    return this;              // 返回自身支持链式调用
}
```

#### 6.2 传送应用场景

**传送功能示例：**
```csharp
public class TeleportSystem : MonoBehaviour
{
    public void TeleportToCheckpoint(Vector3 checkpointPosition, Quaternion checkpointRotation)
    {
        var warpMessage = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.WARP) as WarpMessage;
        
        // 传送到检查点，重置速度但保持其他状态
        warpMessage.Initialize(checkpointPosition, checkpointRotation, false, true);
        MonoBehaviourExCenter.Instance.SendMessage(kartId, warpMessage);
    }
    
    public void RespawnPlayer(Vector3 spawnPoint)
    {
        var warpMessage = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.WARP) as WarpMessage;
        
        // 重生玩家，完全重置状态
        warpMessage.Initialize(spawnPoint, Quaternion.identity, true, true);
        MonoBehaviourExCenter.Instance.SendMessage(playerId, warpMessage);
    }
    
    public void TeleportToFinishLine()
    {
        var warpMessage = MonoBehaviourMessageFactory.Instance
            .GetMessage(MonoBehaviourMessageType.WARP) as WarpMessage;
        
        // 传送到终点，保持当前状态和速度
        warpMessage.Initialize(finishLinePosition, finishLineRotation, false, false);
        MonoBehaviourExCenter.Instance.SendMessage(kartId, warpMessage);
    }
}
```

### 7. ErrorMessages.cs - 错误信息常量定义

**功能概述：**
ErrorMessages类集中定义了游戏中所有的错误信息常量，支持本地化显示，为用户提供友好的错误提示和故障排除信息。

**完整错误信息定义：**
```csharp
public class ErrorMessages
{
    // 网络连接错误
    public const string SERVER_CONNECTION = "서버에 연결할 수 없습니다. 다시 시도해주세요.";
    public const string FAILED_CONNECTION__NETWORK = "네트워크에 연결할 수 없습니다.";
    public const string FAILED_CONNECTION__SERVER = "서버에 연결할 수 없습니다. 다시 시도해주세요.";
    public const string LOST_CONNECTION = "네트워크 연결이 끊겼습니다.";
    
    // 认证和登录错误
    public const string FIA_AUTH = "페이스북 로그인에 실패하였습니다. 로그아웃을 한 후 다시 로그인 해주세요.";
    public const string FAILED_CONNECTION__FACEBOOK = "페이스북에 로그인 할 수 없습니다.";
    public const string LOGGED_OUT = "페이스북에 로그인 하세요.";
    
    // 多人游戏错误
    public const string FAILED_CONNECTION__MULTIPLAY = "네트워크에 연결할 수 없습니다. 블루투스를 켜주세요.";
    public const string FAILED_CONNECTION__HOST = "들어갈 수 없는 방입니다. 새로 고침을 하고 다른 방을 선택해주세요.";
    public const string EVERYONE_LEFT = "다른 플레이어들의 네트워크 연결이 끊겼습니다.";
    
    // 系统和应用错误
    public const string BACKGROUNDING_TERMINATION = "게임이 백그라운드로 들어가면 접속이 끊깁니다.";
    public const string FAILED_TO_UPLOAD = "네트워크에 연결할 수 없습니다. 랭킹을 서버에 올리지 못했습니다.";
}
```

#### 7.1 错误分类分析

**网络相关错误：**
- **SERVER_CONNECTION**: 通用服务器连接失败
- **FAILED_CONNECTION__NETWORK**: 网络不可用
- **FAILED_CONNECTION__SERVER**: 服务器特定连接问题
- **LOST_CONNECTION**: 连接中断

**认证相关错误：**
- **FIA_AUTH**: Facebook认证失败
- **FAILED_CONNECTION__FACEBOOK**: Facebook连接问题
- **LOGGED_OUT**: 需要登录提示

**多人游戏错误：**
- **FAILED_CONNECTION__MULTIPLAY**: 蓝牙连接问题
- **FAILED_CONNECTION__HOST**: 房间连接失败
- **EVERYONE_LEFT**: 其他玩家离开

#### 7.2 本地化错误管理系统

**扩展的错误管理器：**
```csharp
public static class LocalizedErrorMessages
{
    private static Dictionary<string, Dictionary<string, string>> localizedMessages_;
    private static string currentLanguage_ = "ko"; // 默认韩语
    
    static LocalizedErrorMessages()
    {
        InitializeMessages();
    }
    
    private static void InitializeMessages()
    {
        localizedMessages_ = new Dictionary<string, Dictionary<string, string>>();
        
        // 韩语消息 (原始)
        var koreanMessages = new Dictionary<string, string>
        {
            ["SERVER_CONNECTION"] = "서버에 연결할 수 없습니다. 다시 시도해주세요.",
            ["FIA_AUTH"] = "페이스북 로그인에 실패하였습니다. 로그아웃을 한 후 다시 로그인 해주세요.",
            ["BACKGROUNDING_TERMINATION"] = "게임이 백그라운드로 들어가면 접속이 끊깁니다.",
            ["EVERYONE_LEFT"] = "다른 플레이어들의 네트워크 연결이 끊겼습니다.",
            ["LOST_CONNECTION"] = "네트워크 연결이 끊겼습니다.",
            ["FAILED_TO_UPLOAD"] = "네트워크에 연결할 수 없습니다. 랭킹을 서버에 올리지 못했습니다.",
            ["FAILED_CONNECTION__NETWORK"] = "네트워크에 연결할 수 없습니다.",
            ["FAILED_CONNECTION__SERVER"] = "서버에 연결할 수 없습니다. 다시 시도해주세요.",
            ["FAILED_CONNECTION__FACEBOOK"] = "페이스북에 로그인 할 수 없습니다.",
            ["FAILED_CONNECTION__MULTIPLAY"] = "네트워크에 연결할 수 없습니다. 블루투스를 켜주세요.",
            ["FAILED_CONNECTION__HOST"] = "들어갈 수 없는 방입니다. 새로 고침을 하고 다른 방을 선택해주세요.",
            ["LOGGED_OUT"] = "페이스북에 로그인 하세요."
        };
        
        // 英语消息
        var englishMessages = new Dictionary<string, string>
        {
            ["SERVER_CONNECTION"] = "Cannot connect to server. Please try again.",
            ["FIA_AUTH"] = "Facebook login failed. Please logout and login again.",
            ["BACKGROUNDING_TERMINATION"] = "Connection will be lost when the game goes to background.",
            ["EVERYONE_LEFT"] = "Other players have disconnected.",
            ["LOST_CONNECTION"] = "Network connection lost.",
            ["FAILED_TO_UPLOAD"] = "Cannot connect to network. Failed to upload ranking to server.",
            ["FAILED_CONNECTION__NETWORK"] = "Cannot connect to network.",
            ["FAILED_CONNECTION__SERVER"] = "Cannot connect to server. Please try again.",
            ["FAILED_CONNECTION__FACEBOOK"] = "Cannot login to Facebook.",
            ["FAILED_CONNECTION__MULTIPLAY"] = "Cannot connect to network. Please turn on Bluetooth.",
            ["FAILED_CONNECTION__HOST"] = "Cannot join the room. Please refresh and select another room.",
            ["LOGGED_OUT"] = "Please login to Facebook."
        };
        
        // 中文消息
        var chineseMessages = new Dictionary<string, string>
        {
            ["SERVER_CONNECTION"] = "无法连接到服务器，请重试。",
            ["FIA_AUTH"] = "Facebook登录失败，请注销后重新登录。",
            ["BACKGROUNDING_TERMINATION"] = "游戏进入后台时连接将断开。",
            ["EVERYONE_LEFT"] = "其他玩家已断开连接。",
            ["LOST_CONNECTION"] = "网络连接已断开。",
            ["FAILED_TO_UPLOAD"] = "无法连接网络，上传排名失败。",
            ["FAILED_CONNECTION__NETWORK"] = "无法连接网络。",
            ["FAILED_CONNECTION__SERVER"] = "无法连接服务器，请重试。",
            ["FAILED_CONNECTION__FACEBOOK"] = "无法登录Facebook。",
            ["FAILED_CONNECTION__MULTIPLAY"] = "无法连接网络，请开启蓝牙。",
            ["FAILED_CONNECTION__HOST"] = "无法加入房间，请刷新并选择其他房间。",
            ["LOGGED_OUT"] = "请登录Facebook。"
        };
        
        localizedMessages_["ko"] = koreanMessages;
        localizedMessages_["en"] = englishMessages;
        localizedMessages_["zh"] = chineseMessages;
    }
    
    public static void SetLanguage(string languageCode)
    {
        if (localizedMessages_.ContainsKey(languageCode))
        {
            currentLanguage_ = languageCode;
        }
        else
        {
            Debug.LogWarning($"Language {languageCode} not supported, using Korean as default");
        }
    }
    
    public static string GetErrorMessage(string errorKey)
    {
        if (localizedMessages_.ContainsKey(currentLanguage_) &&
            localizedMessages_[currentLanguage_].ContainsKey(errorKey))
        {
            return localizedMessages_[currentLanguage_][errorKey];
        }
        
        // 回退到韩语
        if (localizedMessages_["ko"].ContainsKey(errorKey))
        {
            return localizedMessages_["ko"][errorKey];
        }
        
        return $"Error: {errorKey}";
    }
    
    // 便捷访问方法
    public static string ServerConnection => GetErrorMessage("SERVER_CONNECTION");
    public static string FiaAuth => GetErrorMessage("FIA_AUTH");
    public static string BackgroundingTermination => GetErrorMessage("BACKGROUNDING_TERMINATION");
    public static string EveryoneLeft => GetErrorMessage("EVERYONE_LEFT");
    public static string LostConnection => GetErrorMessage("LOST_CONNECTION");
    public static string FailedToUpload => GetErrorMessage("FAILED_TO_UPLOAD");
    public static string NetworkConnection => GetErrorMessage("FAILED_CONNECTION__NETWORK");
    public static string ServerConnectionSpecific => GetErrorMessage("FAILED_CONNECTION__SERVER");
    public static string FacebookConnection => GetErrorMessage("FAILED_CONNECTION__FACEBOOK");
    public static string MultiplayConnection => GetErrorMessage("FAILED_CONNECTION__MULTIPLAY");
    public static string HostConnection => GetErrorMessage("FAILED_CONNECTION__HOST");
    public static string LoggedOut => GetErrorMessage("LOGGED_OUT");
}
```

#### 7.3 错误处理集成

**统一错误处理系统：**
```csharp
public class ErrorHandler : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject errorPopup;
    public Text errorMessageText;
    public Button retryButton;
    public Button cancelButton;
    
    private System.Action onRetry_;
    private System.Action onCancel_;
    
    private void Start()
    {
        retryButton.onClick.AddListener(OnRetryClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        errorPopup.SetActive(false);
    }
    
    public void ShowError(string errorKey, System.Action onRetry = null, System.Action onCancel = null)
    {
        string errorMessage = LocalizedErrorMessages.GetErrorMessage(errorKey);
        ShowError(errorMessage, onRetry, onCancel);
    }
    
    public void ShowError(string errorMessage, System.Action onRetry = null, System.Action onCancel = null)
    {
        errorMessageText.text = errorMessage;
        onRetry_ = onRetry;
        onCancel_ = onCancel;
        
        // 根据是否有重试逻辑显示按钮
        retryButton.gameObject.SetActive(onRetry != null);
        
        errorPopup.SetActive(true);
        
        // 记录错误日志
        Debug.LogError($"Error displayed: {errorMessage}");
    }
    
    private void OnRetryClicked()
    {
        errorPopup.SetActive(false);
        onRetry_?.Invoke();
    }
    
    private void OnCancelClicked()
    {
        errorPopup.SetActive(false);
        onCancel_?.Invoke();
    }
    
    // 网络错误处理
    public void HandleNetworkError(System.Exception exception)
    {
        if (exception is System.Net.WebException)
        {
            ShowError("FAILED_CONNECTION__NETWORK", RetryNetworkConnection);
        }
        else
        {
            ShowError("SERVER_CONNECTION", RetryServerConnection);
        }
    }
    
    // Facebook认证错误处理
    public void HandleFacebookAuthError()
    {
        ShowError("FIA_AUTH", RetryFacebookLogin, ReturnToMainMenu);
    }
    
    // 多人游戏错误处理
    public void HandleMultiplayerError(string errorType)
    {
        switch (errorType)
        {
            case "bluetooth":
                ShowError("FAILED_CONNECTION__MULTIPLAY", CheckBluetoothSettings);
                break;
            case "host":
                ShowError("FAILED_CONNECTION__HOST", RefreshRoomList);
                break;
            case "players_left":
                ShowError("EVERYONE_LEFT", null, ReturnToLobby);
                break;
        }
    }
    
    private void RetryNetworkConnection()
    {
        // 重试网络连接逻辑
        NetworkManager.Instance.RetryConnection();
    }
    
    private void RetryServerConnection()
    {
        // 重试服务器连接逻辑
        ServerManager.Instance.Reconnect();
    }
    
    private void RetryFacebookLogin()
    {
        // 重试Facebook登录逻辑
        Facebook.Instance.Login();
    }
    
    private void ReturnToMainMenu()
    {
        // 返回主菜单
        SceneManager.LoadScene("MainMenu");
    }
    
    private void CheckBluetoothSettings()
    {
        // 引导用户检查蓝牙设置
        Application.OpenURL("app-settings:bluetooth");
    }
    
    private void RefreshRoomList()
    {
        // 刷新房间列表
        MultiplayerManager.Instance.RefreshRooms();
    }
    
    private void ReturnToLobby()
    {
        // 返回大厅
        SceneManager.LoadScene("Lobby");
    }
}
```

## 综合应用示例

### 1. 统一消息传递系统

**消息管理器：**
```csharp
public class GameMessageManager : MonoBehaviour
{
    public static GameMessageManager Instance { get; private set; }
    
    [Header("Message Settings")]
    public bool enableMessageLogging = false;
    public bool enableMessageQueue = true;
    public int maxQueueSize = 100;
    
    // 消息队列
    private Queue<QueuedMessage> messageQueue_;
    private bool isProcessingQueue_;
    
    private struct QueuedMessage
    {
        public int targetId;
        public int channelId;
        public MonoBehaviourMessage message;
        public float delay;
        public float timestamp;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMessageSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeMessageSystem()
    {
        // 初始化消息工厂
        if (!MonoBehaviourMessageFactory.Instance.IsInitialized())
        {
            MonoBehaviourMessageFactory.Instance.Initialize();
        }
        
        messageQueue_ = new Queue<QueuedMessage>();
        isProcessingQueue_ = false;
        
        // 启动消息队列处理
        if (enableMessageQueue)
        {
            StartCoroutine(ProcessMessageQueue());
        }
    }
    
    private void Update()
    {
        // 处理延迟消息
        ProcessDelayedMessages();
    }
    
    // 立即发送消息
    public void SendMessage(int targetId, MonoBehaviourMessageType messageType)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType);
        SendMessageInternal(targetId, 0, message);
    }
    
    public void SendMessage<T>(int targetId, MonoBehaviourMessageType messageType, T param)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType) as MonoBehaviourMessage1Param<T>;
        message.Initialize(param);
        SendMessageInternal(targetId, 0, message);
    }
    
    public void SendMessage<T1, T2>(int targetId, MonoBehaviourMessageType messageType, T1 param1, T2 param2)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType) as MonoBehaviourMessage2Param<T1, T2>;
        message.Initialize(param1, param2);
        SendMessageInternal(targetId, 0, message);
    }
    
    // 延迟发送消息
    public void SendMessageDelayed(int targetId, MonoBehaviourMessageType messageType, float delay)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType);
        QueueMessage(targetId, 0, message, delay);
    }
    
    public void SendMessageDelayed<T>(int targetId, MonoBehaviourMessageType messageType, T param, float delay)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType) as MonoBehaviourMessage1Param<T>;
        message.Initialize(param);
        QueueMessage(targetId, 0, message, delay);
    }
    
    // 广播消息
    public void BroadcastMessage(MonoBehaviourMessageType messageType)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType);
        MonoBehaviourExCenter.Instance.BroadcastMessage(0, message);
        
        if (enableMessageLogging)
        {
            Debug.Log($"Broadcast message: {messageType}");
        }
    }
    
    public void BroadcastMessage<T>(MonoBehaviourMessageType messageType, T param)
    {
        var message = MonoBehaviourMessageFactory.Instance.GetMessage(messageType) as MonoBehaviourMessage1Param<T>;
        message.Initialize(param);
        MonoBehaviourExCenter.Instance.BroadcastMessage(0, message);
        
        if (enableMessageLogging)
        {
            Debug.Log($"Broadcast message: {messageType} with param: {param}");
        }
    }
    
    private void SendMessageInternal(int targetId, int channelId, MonoBehaviourMessage message)
    {
        MonoBehaviourExCenter.Instance.SendMessage(targetId, channelId, message);
        
        if (enableMessageLogging)
        {
            Debug.Log($"Sent message: {message.type_} to target: {targetId}");
        }
    }
    
    private void QueueMessage(int targetId, int channelId, MonoBehaviourMessage message, float delay)
    {
        if (!enableMessageQueue)
        {
            Debug.LogWarning("Message queue is disabled");
            return;
        }
        
        if (messageQueue_.Count >= maxQueueSize)
        {
            Debug.LogWarning("Message queue is full, dropping oldest message");
            messageQueue_.Dequeue();
        }
        
        var queuedMessage = new QueuedMessage
        {
            targetId = targetId,
            channelId = channelId,
            message = message,
            delay = delay,
            timestamp = Time.time
        };
        
        messageQueue_.Enqueue(queuedMessage);
    }
    
    private void ProcessDelayedMessages()
    {
        if (!enableMessageQueue || isProcessingQueue_) return;
        
        isProcessingQueue_ = true;
        
        var tempQueue = new Queue<QueuedMessage>();
        
        while (messageQueue_.Count > 0)
        {
            var queuedMessage = messageQueue_.Dequeue();
            
            if (Time.time >= queuedMessage.timestamp + queuedMessage.delay)
            {
                // 时间到了，发送消息
                SendMessageInternal(queuedMessage.targetId, queuedMessage.channelId, queuedMessage.message);
            }
            else
            {
                // 时间未到，重新加入队列
                tempQueue.Enqueue(queuedMessage);
            }
        }
        
        // 将未到时间的消息重新加入队列
        while (tempQueue.Count > 0)
        {
            messageQueue_.Enqueue(tempQueue.Dequeue());
        }
        
        isProcessingQueue_ = false;
    }
    
    private IEnumerator ProcessMessageQueue()
    {
        while (true)
        {
            ProcessDelayedMessages();
            yield return new WaitForSeconds(0.1f); // 每0.1秒处理一次队列
        }
    }
    
    // 清空消息队列
    public void ClearMessageQueue()
    {
        messageQueue_.Clear();
        Debug.Log("Message queue cleared");
    }
    
    // 获取队列状态
    public int GetQueueSize()
    {
        return messageQueue_.Count;
    }
    
    public bool IsQueueFull()
    {
        return messageQueue_.Count >= maxQueueSize;
    }
}
```

### 2. 消息性能监控

**消息性能分析器：**
```csharp
public class MessagePerformanceProfiler : MonoBehaviour
{
    [Header("Profiling Settings")]
    public bool enableProfiling = false;
    public int maxRecords = 1000;
    public float reportInterval = 5.0f;
    
    private Dictionary<MonoBehaviourMessageType, MessageStats> messageStats_;
    private List<MessageRecord> messageRecords_;
    private float lastReportTime_;
    
    private struct MessageStats
    {
        public int count;
        public float totalTime;
        public float maxTime;
        public float minTime;
        public float averageTime => count > 0 ? totalTime / count : 0;
    }
    
    private struct MessageRecord
    {
        public MonoBehaviourMessageType type;
        public float timestamp;
        public float processingTime;
        public int targetId;
    }
    
    private void Start()
    {
        if (enableProfiling)
        {
            messageStats_ = new Dictionary<MonoBehaviourMessageType, MessageStats>();
            messageRecords_ = new List<MessageRecord>();
            lastReportTime_ = Time.time;
        }
    }
    
    private void Update()
    {
        if (enableProfiling && Time.time - lastReportTime_ >= reportInterval)
        {
            GeneratePerformanceReport();
            lastReportTime_ = Time.time;
        }
    }
    
    public void RecordMessage(MonoBehaviourMessageType type, float processingTime, int targetId)
    {
        if (!enableProfiling) return;
        
        // 更新统计信息
        if (!messageStats_.ContainsKey(type))
        {
            messageStats_[type] = new MessageStats
            {
                count = 0,
                totalTime = 0,
                maxTime = 0,
                minTime = float.MaxValue
            };
        }
        
        var stats = messageStats_[type];
        stats.count++;
        stats.totalTime += processingTime;
        stats.maxTime = Mathf.Max(stats.maxTime, processingTime);
        stats.minTime = Mathf.Min(stats.minTime, processingTime);
        messageStats_[type] = stats;
        
        // 记录详细信息
        if (messageRecords_.Count >= maxRecords)
        {
            messageRecords_.RemoveAt(0);
        }
        
        messageRecords_.Add(new MessageRecord
        {
            type = type,
            timestamp = Time.time,
            processingTime = processingTime,
            targetId = targetId
        });
    }
    
    private void GeneratePerformanceReport()
    {
        Debug.Log("=== Message Performance Report ===");
        
        foreach (var kvp in messageStats_)
        {
            var type = kvp.Key;
            var stats = kvp.Value;
            
            Debug.Log($"{type}: Count={stats.count}, Avg={stats.averageTime:F4}ms, " +
                     $"Max={stats.maxTime:F4}ms, Min={stats.minTime:F4}ms");
        }
        
        // 找出最慢的消息类型
        var slowestType = messageStats_.OrderByDescending(kvp => kvp.Value.averageTime).FirstOrDefault();
        if (slowestType.Value.count > 0)
        {
            Debug.LogWarning($"Slowest message type: {slowestType.Key} ({slowestType.Value.averageTime:F4}ms avg)");
        }
        
        // 找出最频繁的消息类型
        var mostFrequentType = messageStats_.OrderByDescending(kvp => kvp.Value.count).FirstOrDefault();
        if (mostFrequentType.Value.count > 0)
        {
            Debug.Log($"Most frequent message type: {mostFrequentType.Key} ({mostFrequentType.Value.count} times)");
        }
    }
    
    public void ResetStatistics()
    {
        messageStats_.Clear();
        messageRecords_.Clear();
        Debug.Log("Message performance statistics reset");
    }
    
    public MessageStats GetMessageStats(MonoBehaviourMessageType type)
    {
        return messageStats_.ContainsKey(type) ? messageStats_[type] : default(MessageStats);
    }
}
```

## 性能优化建议

### 1. 消息池化优化

**对象池消息工厂：**
```csharp
public class PooledMessageFactory : MonoBehaviourMessageFactory
{
    private Dictionary<MonoBehaviourMessageType, Queue<MonoBehaviourMessage>> messagePools_;
    private const int INITIAL_POOL_SIZE = 10;
    
    public new void Initialize()
    {
        base.Initialize();
        messagePools_ = new Dictionary<MonoBehaviourMessageType, Queue<MonoBehaviourMessage>>();
        
        // 预分配常用消息类型的对象池
        PreallocateMessagePools();
    }
    
    private void PreallocateMessagePools()
    {
        var commonTypes = new MonoBehaviourMessageType[]
        {
            MonoBehaviourMessageType.SHOW_UI,
            MonoBehaviourMessageType.ITEM,
            MonoBehaviourMessageType.WARP,
            MonoBehaviourMessageType.BLACK_BAR
        };
        
        foreach (var type in commonTypes)
        {
            var pool = new Queue<MonoBehaviourMessage>();
            for (int i = 0; i < INITIAL_POOL_SIZE; i++)
            {
                var message = CreateMessageInstance(type);
                pool.Enqueue(message);
            }
            messagePools_[type] = pool;
        }
    }
    
    public new MonoBehaviourMessage GetMessage(MonoBehaviourMessageType type)
    {
        if (messagePools_.ContainsKey(type) && messagePools_[type].Count > 0)
        {
            return messagePools_[type].Dequeue();
        }
        
        return CreateMessageInstance(type);
    }
    
    public void ReturnMessage(MonoBehaviourMessage message)
    {
        var type = message.type_;
        
        // 重置消息状态
        ResetMessage(message);
        
        if (!messagePools_.ContainsKey(type))
        {
            messagePools_[type] = new Queue<MonoBehaviourMessage>();
        }
        
        messagePools_[type].Enqueue(message);
    }
    
    private MonoBehaviourMessage CreateMessageInstance(MonoBehaviourMessageType type)
    {
        // 根据类型创建对应的消息实例
        return base.GetMessage(type);
    }
    
    private void ResetMessage(MonoBehaviourMessage message)
    {
        // 重置消息到初始状态
        if (message is BlackBarMessage blackBar)
        {
            blackBar.Initialize(false, false, false);
        }
        else if (message is WarpMessage warp)
        {
            warp.Initialize(Vector3.zero, Quaternion.identity, false, false);
        }
    }
}
```

## 总结

Messages模块为卡丁车游戏提供了完整且高效的消息传递基础设施：

### 核心特性
1. **类型安全**: 基于泛型的强类型消息系统
2. **统一管理**: 工厂模式集中管理所有消息类型
3. **参数化**: 支持无参数、单参数、双参数消息
4. **本地化**: 统一的错误信息本地化支持

### 设计优势
1. **性能优化**: 对象池模式减少内存分配
2. **易于扩展**: 清晰的继承层次便于添加新消息类型
3. **调试友好**: 完善的日志和性能监控机制
4. **模块化**: 松耦合的消息传递促进模块化设计

### 应用价值
1. **系统通信**: 为游戏各系统提供标准化通信接口
2. **事件驱动**: 支持事件驱动的游戏架构
3. **错误处理**: 统一的错误信息管理和显示
4. **国际化**: 多语言错误信息支持

该模块是游戏架构的核心通信基础设施，为实现松耦合、高可维护性的游戏系统提供了重要支撑。