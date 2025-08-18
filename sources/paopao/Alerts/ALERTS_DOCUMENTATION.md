# Alerts 文件夹完整功能文档

## 概述

Alerts 文件夹包含了卡丁车游戏的通知管理系统，负责跟踪和管理各种游戏元素（卡丁车、角色、赛道、内容包）的状态变化，并在适当时机向玩家显示提醒。该系统采用状态模式和工厂模式设计，具有持久化存储、版本控制和可扩展性等特性。

## 系统架构

### 核心设计模式
- **状态模式 (State Pattern)**: 通过枚举值管理物品状态
- **工厂模式 (Factory Pattern)**: 集中创建和管理警报状态实例
- **单例模式 (Singleton Pattern)**: 确保工厂类的全局唯一性
- **模板方法模式 (Template Method)**: 基类定义通用算法框架
- **策略模式 (Strategy Pattern)**: 不同警报类型实现不同行为

### 系统特性
- **持久化存储**: 使用Unity PlayerPrefs跨会话保存状态
- **版本控制**: 自动检测和应用配置更新
- **XML配置**: 外部配置文件支持灵活的默认设置
- **类型安全**: 强类型枚举确保状态管理安全性

## 文件详细分析

### 1. AlertState.cs - 警报状态抽象基类
**文件位置**: `/Alerts/AlertState.cs`
**功能概述**: 定义警报状态管理的核心抽象框架

**核心状态枚举**:
```csharp
public enum StateEnum
{
    LOCKED_UPDATED,     // 已锁定但有更新 - 触发警报
    LOCKED_READ,        // 已锁定且已读 - 不触发警报
    UNLOCKED_UPDATED,   // 已解锁且有更新 - 触发警报
    UNLOCKED_READ       // 已解锁且已读 - 不触发警报
}
```

**警报触发逻辑**:
```csharp
public bool DisplayAlert(string id)
{
    AlertState.StateEnum state = this.GetState(id);
    
    // 只有UPDATED状态才会触发警报显示
    foreach (AlertState.StateEnum stateEnum in this.ALERT_STATES)
    {
        if (stateEnum == state)
            return true;
    }
    return false;
}

// 定义触发警报的状态类型
private static AlertState.StateEnum[] ALERT_STATES = new AlertState.StateEnum[]
{
    AlertState.StateEnum.LOCKED_UPDATED,
    AlertState.StateEnum.UNLOCKED_UPDATED
};
```

**状态管理核心方法**:
```csharp
// 获取指定ID的状态
public StateEnum GetState(string id)
{
    if (this.state_.ContainsKey(id))
    {
        return this.state_[id];
    }
    return StateEnum.LOCKED_READ; // 默认状态
}

// 设置指定ID的状态
public void SetState(string id, StateEnum state)
{
    this.state_[id] = state;
}

// 提交状态变化到持久化存储
public void Commit()
{
    this.cache_.Save(this.state_);
}
```

**事件处理框架**:
```csharp
public enum EventType
{
    CLICK,   // 点击事件 - 标记为已读
    UNLOCK,  // 解锁事件 - 解锁新内容
    RIDE     // 使用事件 - 标记为已体验（仅限赛道）
}

// 抽象方法 - 子类必须实现
public abstract void Click(string id, bool commit);
public abstract void Unlock(string id);
public abstract void Ride(string id);
public abstract void CheckUnlockedItems();
```

**设计特点**:
- 使用字典存储ID到状态的映射，支持任意类型的游戏物品
- 状态转换遵循严格的规则，防止非法状态变化
- 提供批量操作的commit机制，提高性能

### 2. AlertStateCache.cs - 警报状态缓存管理器
**文件位置**: `/Alerts/AlertStateCache.cs`
**功能概述**: 负责警报状态的持久化存储和序列化

**数据序列化格式**:
```csharp
// 存储格式: "id1:state1,id2:state2,id3:state3"
public void Save(Dictionary<string, AlertState.StateEnum> state)
{
    StringBuilder stringBuilder = new StringBuilder();
    bool first = true;
    
    foreach (string key in state.Keys)
    {
        if (!first)
        {
            stringBuilder.Append(",");
        }
        
        stringBuilder.Append(key);
        stringBuilder.Append(":");
        stringBuilder.Append((int)state[key]); // 将枚举转换为整数
        first = false;
    }
    
    // 使用类型特定的键存储到PlayerPrefs
    PlayerPrefs.SetString(this.key_, stringBuilder.ToString());
}
```

**数据反序列化**:
```csharp
public Dictionary<string, AlertState.StateEnum> Load()
{
    Dictionary<string, AlertState.StateEnum> result = new Dictionary<string, AlertState.StateEnum>();
    
    string data = PlayerPrefs.GetString(this.key_, string.Empty);
    if (!string.IsNullOrEmpty(data))
    {
        string[] pairs = data.Split(',');
        foreach (string pair in pairs)
        {
            string[] keyValue = pair.Split(':');
            if (keyValue.Length == 2)
            {
                string id = keyValue[0];
                int stateValue = int.Parse(keyValue[1]);
                result[id] = (AlertState.StateEnum)stateValue;
            }
        }
    }
    
    return result;
}
```

**键命名策略**:
```csharp
// 构造函数设置特定类型的存储键
public AlertStateCache(AlertStateType type)
{
    this.key_ = string.Format("{0}_ALERT_STATE", type.ToString());
    // 例如: "KART_ALERT_STATE", "TRACK_ALERT_STATE"
}
```

**设计优势**:
- 简单的文本格式确保跨平台兼容性
- 类型特定的键避免不同警报类型间的数据冲突
- 轻量级序列化适合小规模数据集

### 3. AlertStateFactory.cs - 警报状态工厂
**文件位置**: `/Alerts/AlertStateFactory.cs`
**功能概述**: 单例工厂，负责创建和管理所有警报状态实例，支持版本控制和XML配置

**单例实现**:
```csharp
public static AlertStateFactory Instance
{
    get
    {
        if (AlertStateFactory.instance_ == null)
        {
            AlertStateFactory.instance_ = new AlertStateFactory();
        }
        return AlertStateFactory.instance_;
    }
}

private static AlertStateFactory instance_;
private AlertState[] alertStates_; // 缓存已创建的实例
```

**核心工厂方法**:
```csharp
public AlertState GetAlertState(AlertStateType type)
{
    // 懒加载模式 - 只在需要时创建
    if (this.alertStates_[(int)type] == null)
    {
        AlertStateCache alertStateCache = new AlertStateCache(type);
        
        // 加载已缓存的状态
        Dictionary<string, AlertState.StateEnum> cachedState = alertStateCache.Load();
        Dictionary<string, AlertState.StateEnum> defaultEvents = new Dictionary<string, AlertState.StateEnum>();
        
        // 版本检查和更新
        if (!this.UpdatedToCurrentVersion(type))
        {
            // 加载XML默认配置
            XMLElement xmlConfig = this.DefaultXML(type);
            if (xmlConfig != null)
            {
                defaultEvents = this.ParseXMLEvents(xmlConfig);
                // 合并默认配置到缓存状态
                this.MergeStates(cachedState, defaultEvents);
            }
            this.UpdateVersion(type);
        }
        
        // 创建类型特定的实例
        AlertState alertState = this.CreateAlertState(type, cachedState, defaultEvents, alertStateCache);
        
        // 检查新解锁的物品
        alertState.CheckUnlockedItems();
        alertState.Commit();
        
        this.alertStates_[(int)type] = alertState;
    }
    
    return this.alertStates_[(int)type];
}
```

**类型特定创建逻辑**:
```csharp
private AlertState CreateAlertState(AlertStateType type, 
                                   Dictionary<string, AlertState.StateEnum> state,
                                   Dictionary<string, AlertState.StateEnum> events,
                                   AlertStateCache cache)
{
    switch (type)
    {
        case AlertStateType.KART:
            return new KartAlertState(state, events, cache);
            
        case AlertStateType.CHARACTER:
            return new CharacterAlertState(state, events, cache);
            
        case AlertStateType.TRACK:
            return new TrackAlertState(state, events, cache);
            
        case AlertStateType.BUNDLE:
            return new BundleAlertState(state, events, cache);
            
        case AlertStateType.BUNDLE_INFO:
            return new BundleInfoAlertState(state, events, cache);
            
        default:
            throw new ArgumentException("Unknown AlertStateType: " + type);
    }
}
```

**版本控制系统**:
```csharp
private bool UpdatedToCurrentVersion(AlertStateType type)
{
    string versionKey = string.Format("{0}_ALERT_VERSION", type.ToString());
    string currentVersion = Application.version;
    string savedVersion = PlayerPrefs.GetString(versionKey, "0.0.0");
    
    return currentVersion == savedVersion;
}

private void UpdateVersion(AlertStateType type)
{
    string versionKey = string.Format("{0}_ALERT_VERSION", type.ToString());
    PlayerPrefs.SetString(versionKey, Application.version);
}
```

**XML配置解析**:
```csharp
private XMLElement DefaultXML(AlertStateType type)
{
    string xmlFileName = string.Format("alert_{0}_default", type.ToString().ToLower());
    TextAsset xmlAsset = (TextAsset)Resources.Load(xmlFileName);
    
    if (xmlAsset != null)
    {
        XMLElement xmlElement = new XMLElement();
        xmlElement.Parse(xmlAsset.text);
        return xmlElement;
    }
    
    return null;
}

private Dictionary<string, AlertState.StateEnum> ParseXMLEvents(XMLElement xml)
{
    Dictionary<string, AlertState.StateEnum> events = new Dictionary<string, AlertState.StateEnum>();
    
    XMLElement eventsElement = xml.GetElement("events");
    if (eventsElement != null)
    {
        foreach (XMLElement eventElement in eventsElement.GetElements("event"))
        {
            string id = eventElement.GetAttribute("id");
            string stateStr = eventElement.GetAttribute("state");
            
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(stateStr))
            {
                AlertState.StateEnum state = (AlertState.StateEnum)Enum.Parse(typeof(AlertState.StateEnum), stateStr);
                events[id] = state;
            }
        }
    }
    
    return events;
}
```

### 4. AlertStateType.cs - 警报状态类型枚举
**文件位置**: `/Alerts/AlertStateType.cs`
**功能概述**: 定义系统中所有可用的警报状态类型

**类型定义**:
```csharp
public enum AlertStateType
{
    KART,        // 0 - 卡丁车相关警报
    CHARACTER,   // 1 - 角色相关警报  
    TRACK,       // 2 - 赛道相关警报
    BUNDLE,      // 3 - 内容包相关警报
    BUNDLE_INFO, // 4 - 内容包信息警报
    SIZE         // 5 - 数组大小标记（非实际类型）
}
```

**使用场景**:
- 工厂模式中的类型分发
- 数组索引和大小计算
- 配置文件命名和版本控制键生成
- 类型安全的警报管理

### 5. BaseKartCharacterAlertState.cs - 卡丁车角色警报基类
**文件位置**: `/Alerts/BaseKartCharacterAlertState.cs`
**功能概述**: 为卡丁车和角色警报提供通用行为的抽象基类

**通用点击处理**:
```csharp
public override void Click(string id, bool commit)
{
    bool stateChanged = false;
    AlertState.StateEnum currentState = base.GetState(id);
    
    switch (currentState)
    {
        case AlertState.StateEnum.LOCKED_UPDATED:
            // 已锁定但有更新 -> 已锁定且已读
            base.SetState(id, AlertState.StateEnum.LOCKED_READ);
            stateChanged = true;
            break;
            
        case AlertState.StateEnum.UNLOCKED_UPDATED:
            // 已解锁且有更新 -> 已解锁且已读
            base.SetState(id, AlertState.StateEnum.UNLOCKED_READ);
            stateChanged = true;
            break;
            
        default:
            // 其他状态不需要处理
            break;
    }
    
    // 如果状态发生变化且需要立即提交，则保存到持久化存储
    if (stateChanged && commit)
    {
        base.Commit();
    }
}
```

**通用解锁处理**:
```csharp
public override void Unlock(string id)
{
    AlertState.StateEnum currentState = base.GetState(id);
    
    switch (currentState)
    {
        case AlertState.StateEnum.LOCKED_READ:
        case AlertState.StateEnum.LOCKED_UPDATED:
            // 任何锁定状态 -> 已解锁且有更新（触发警报）
            base.SetState(id, AlertState.StateEnum.UNLOCKED_UPDATED);
            break;
            
        default:
            // 已经解锁的物品不需要重复处理
            break;
    }
}
```

**空实现的Ride方法**:
```csharp
public override void Ride(string id)
{
    // 卡丁车和角色不支持"使用"操作
    // 只有赛道才有Ride的概念
}
```

**设计意图**:
- 消除卡丁车和角色警报状态间的代码重复
- 提供一致的用户交互行为
- 为未来扩展相似类型的警报预留基础架构

### 6. BundleAlertState.cs - 内容包警报状态
**文件位置**: `/Alerts/BundleAlertState.cs`
**功能概述**: 处理游戏内容包相关的警报状态管理

**特殊的解锁行为**:
```csharp
public override void Unlock(string id)
{
    AlertState.StateEnum currentState = base.GetState(id);
    
    if (currentState == AlertState.StateEnum.LOCKED_READ || 
        currentState == AlertState.StateEnum.LOCKED_UPDATED)
    {
        // 内容包解锁后直接设置为已读状态，不触发警报
        // 这与卡丁车/角色不同，它们解锁后会设置为UNLOCKED_UPDATED
        base.SetState(id, AlertState.StateEnum.UNLOCKED_READ);
        
        if (Debug.isDebugBuild)
        {
            Debug.Log(string.Format("Bundle {0} unlocked and marked as read", id));
        }
    }
}
```

**空实现的检查方法**:
```csharp
public override void CheckUnlockedItems()
{
    // 内容包的解锁状态由外部系统管理
    // 不需要自动检查解锁状态
}
```

**继承的标准行为**:
- **Click处理**: 继承自AlertState基类的标准实现
- **Ride操作**: 空实现（内容包不支持使用操作）

**设计决策**:
- 内容包解锁后不显示"新内容"警报，因为它们通常是批量内容
- 依赖外部系统（如内购管理器）来触发解锁事件
- 简化的状态管理适合内容包的特殊需求

### 7. TrackAlertState.cs - 赛道警报状态
**文件位置**: `/Alerts/TrackAlertState.cs`
**功能概述**: 管理赛道相关的警报状态，支持完整的交互模型

**资产管理器集成**:
```csharp
public override void CheckUnlockedItems()
{
    // 刷新赛道资产定义
    TrackAssetDefinitionManager.Instance.Refresh();
    
    // 获取所有赛道定义
    List<AssetDefinition> trackList = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
    
    foreach (AssetDefinition track in trackList)
    {
        TrackAssetDefinition trackDef = (TrackAssetDefinition)track;
        
        // 检查赛道是否已解锁
        if (!trackDef.Lock)
        {
            // 自动解锁新开放的赛道
            this.Unlock(trackDef.Id);
        }
    }
}
```

**完整的事件处理**:
```csharp
// 继承标准点击行为
public override void Click(string id, bool commit)
{
    // 使用基类的标准实现
    // LOCKED_UPDATED -> LOCKED_READ
    // UNLOCKED_UPDATED -> UNLOCKED_READ
    base.Click(id, commit);
}

// 继承标准解锁行为  
public override void Unlock(string id)
{
    // 使用基类的标准实现
    // LOCKED_* -> UNLOCKED_UPDATED
    base.Unlock(id);
}

// 赛道特有的使用行为
public override void Ride(string id)
{
    AlertState.StateEnum currentState = base.GetState(id);
    
    switch (currentState)
    {
        case AlertState.StateEnum.UNLOCKED_UPDATED:
            // 首次游玩解锁的赛道 -> 标记为已读
            base.SetState(id, AlertState.StateEnum.UNLOCKED_READ);
            break;
            
        case AlertState.StateEnum.LOCKED_UPDATED:
            // 游玩锁定但有更新的赛道 -> 标记为已读
            base.SetState(id, AlertState.StateEnum.LOCKED_READ);
            break;
            
        default:
            // 其他状态不需要处理
            break;
    }
}
```

**三种交互模式**:
1. **查看 (Click)**: 浏览赛道信息，标记为已读
2. **解锁 (Unlock)**: 赛道变为可游玩状态
3. **游玩 (Ride)**: 实际进行比赛，除"新赛道"提醒消

**设计特点**:
- 最复杂的交互模型，支持所有三种事件类型
- 与资产管理系统深度集成，自动检测解锁状态
- 提供渐进式的用户体验（查看->解锁->游玩）

### 8. KartAlertState.cs 和 CharacterAlertState.cs - 具体实现类
**文件位置**: `/Alerts/KartAlertState.cs`, `/Alerts/CharacterAlertState.cs`
**功能概述**: 继承BaseKartCharacterAlertState的具体实现

**KartAlertState实现**:
```csharp
public class KartAlertState : BaseKartCharacterAlertState
{
    public KartAlertState(Dictionary<string, AlertState.StateEnum> state,
                         Dictionary<string, AlertState.StateEnum> events,
                         AlertStateCache cache) : base(state, events, cache)
    {
    }
    
    public override void CheckUnlockedItems()
    {
        // 刷新卡丁车资产定义
        KartAssetDefinitionManager.Instance.Refresh();
        
        // 获取所有卡丁车定义
        List<AssetDefinition> kartList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        foreach (AssetDefinition kart in kartList)
        {
            KartAssetDefinition kartDef = (KartAssetDefinition)kart;
            
            // 检查卡丁车是否已解锁
            if (!kartDef.Lock)
            {
                this.Unlock(kartDef.Id);
            }
        }
    }
}
```

**CharacterAlertState实现**:
```csharp
public class CharacterAlertState : BaseKartCharacterAlertState
{
    public CharacterAlertState(Dictionary<string, AlertState.StateEnum> state,
                              Dictionary<string, AlertState.StateEnum> events,
                              AlertStateCache cache) : base(state, events, cache)
    {
    }
    
    public override void CheckUnlockedItems()
    {
        // 刷新角色资产定义
        CharacterAssetDefinitionManager.Instance.Refresh();
        
        // 获取所有角色定义
        List<AssetDefinition> characterList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        foreach (AssetDefinition character in characterList)
        {
            CharacterAssetDefinition charDef = (CharacterAssetDefinition)character;
            
            // 检查角色是否已解锁
            if (!charDef.Lock)
            {
                this.Unlock(charDef.Id);
            }
        }
    }
}
```

**共同特点**:
- 继承基类的所有事件处理行为
- 实现特定的资产管理器集成
- 自动检测新解锁的内容
- 维护与游戏主要资产系统的同步

## 系统关系图

### 类层次结构
```
AlertState (抽象基类)
├── BaseKartCharacterAlertState (抽象基类)
│   ├── KartAlertState (卡丁车警报)
│   └── CharacterAlertState (角色警报)
├── TrackAlertState (赛道警报)
└── BundleAlertState (内容包警报)
```

### 依赖关系
- **AlertStateFactory** → 创建所有AlertState实例
- **AlertStateCache** → 被所有AlertState实例用于持久化
- **AssetDefinitionManagers** → 被具体实现用于解锁检查
- **XMLElement** → 用于配置解析
- **PlayerPrefs** → 用于持久化存储

## 关键架构决策

### 1. **基于字符串的ID系统**
- **优势**: 支持任意类型的游戏资产，灵活性高
- **适用性**: 可以处理动态生成的内容和DLC
- **扩展性**: 新增资产类型无需修改核心代码

### 2. **枚举状态管理**
- **类型安全**: 编译时检查，避免无效状态
- **性能优化**: 整数比较，高效的状态检查
- **可维护性**: 清晰的状态定义和转换规则

### 3. **XML外部配置**
- **可维护性**: 无需代码修改即可调整默认行为
- **本地化支持**: 支持不同地区的不同配置
- **版本控制**: 配合版本系统实现渐进式更新

### 4. **懒加载模式**
- **性能优化**: 只在需要时创建AlertState实例
- **内存效率**: 避免预创建所有类型的实例
- **启动优化**: 减少游戏启动时间

### 5. **批量操作支持**
- **性能考虑**: commit参数支持批量状态更新
- **事务性**: 确保相关状态同时更新
- **用户体验**: 避免频繁的存储操作

## 使用示例

### 基本使用模式
```csharp
// 获取赛道警报状态管理器
AlertState trackAlerts = AlertStateFactory.Instance.GetAlertState(AlertStateType.TRACK);

// 检查是否需要显示警报
if (trackAlerts.DisplayAlert("track_001"))
{
    // 显示新赛道警报UI
    ShowNewTrackAlert("track_001");
}

// 用户点击查看赛道
trackAlerts.Click("track_001", true);

// 用户解锁新赛道
trackAlerts.Unlock("track_002");

// 用户游玩赛道
trackAlerts.Ride("track_001");
```

### 批量操作模式
```csharp
AlertState kartAlerts = AlertStateFactory.Instance.GetAlertState(AlertStateType.KART);

// 批量处理多个卡丁车点击，最后一次性提交
kartAlerts.Click("kart_001", false);
kartAlerts.Click("kart_002", false);
kartAlerts.Click("kart_003", true); // 最后一个操作提交所有变化
```

## 总结

Alerts系统提供了一个功能完整、设计良好的通知管理框架，具有以下优势：

1. **可扩展性**: 清晰的继承层次和接口定义支持新警报类型
2. **持久化**: 可靠的跨会话状态保存机制
3. **性能优化**: 懒加载、批量操作和高效的状态管理
4. **版本控制**: 自动处理游戏更新带来的配置变化
5. **类型安全**: 强类型系统防止状态管理错误
6. **灵活配置**: XML配置支持运营需求

该系统为游戏的用户体验提供了重要支撑，确保玩家及时了解新内容和状态变化，同时保持高效的运行性能。