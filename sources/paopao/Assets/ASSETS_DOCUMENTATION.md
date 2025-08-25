# Assets 文件夹完整功能文档

## 概述

Assets 文件夹包含了卡丁车游戏的核心资产管理系统，提供了一个灵活的框架来管理不同类型的游戏资产（角色、卡丁车、赛道）。该系统支持解锁条件、购买机制、动态资产加载和进度跟踪等功能，采用数据驱动的设计理念，通过XML配置实现内容的灵活管理。

## 系统架构

### 核心设计原则
- **类型安全**: 通过枚举确保资产操作的类型安全性
- **可扩展性**: 虚拟方法和继承支持特化的资产类型
- **数据驱动**: XML配置允许无需代码修改即可更新内容
- **状态分离**: 定义与运行时状态分离，支持进度跟踪
- **模块化**: 每个组件具有单一、明确定义的职责

### 设计模式应用
- **模板方法模式**: 基类定义算法框架，子类实现具体细节
- **工厂方法模式**: 特化管理器创建相应的资产定义类型
- **单例模式**: 管理器实例确保全局唯一性
- **策略模式**: 不同解锁类型实现不同的解锁策略
- **状态模式**: 资产锁定状态的管理和转换

## 文件详细分析

### 1. AssetType.cs - 资产类型枚举
**文件位置**: `/Assets/AssetType.cs`
**功能概述**: 定义游戏中不同类型资产的分类标识符

**枚举定义**:
```csharp
public enum AssetType
{
    CHARACTER,  // 0 - 角色资产
    KART,       // 1 - 卡丁车资产
    TRACK,      // 2 - 赛道资产
    SIZE        // 3 - 数组大小标记
}
```

**使用场景**:
- 资产管理器的类型路由和分发
- 数组索引和大小计算
- 类型安全的资产操作
- UI系统中的分类显示

**架构决策**:
- 简单枚举设计确保清晰性和可维护性
- `SIZE` 值为常见模式，用于确定数组边界或迭代限制
- 遵循C#枚举的标准命名约定
- 支持未来新资产类型的扩展

### 2. AssetDefinition.cs - 资产定义基类
**文件位置**: `/Assets/AssetDefinition.cs`
**功能概述**: 定义个体游戏资产的属性、解锁条件和购买信息的基类

**解锁类型枚举**:
```csharp
public enum enLockType
{
    NONE,   // 始终解锁（免费资产）
    CASH,   // 通过购买解锁（付费资产）
    QUEST   // 通过完成任务解锁（进度资产）
}
```

**核心属性结构**:
```csharp
public class AssetDefinition
{
    protected string name_;                    // 资产标识符
    protected string[] productIDs_;           // 购买标识符数组
    private enLockType lockType_;             // 解锁方式类型
    private bool lock_ = true;                // 当前锁定状态
    private QuestBase quest_;                 // 关联的解锁任务
    private Rect questContents_;              // 任务UI定位信息
    private int questTitle_;                  // 任务标题标识符
    private int id_ = -1;                     // 唯一资产ID
    private int listIdx_ = -1;                // 显示顺序索引
    
    // 核心属性访问器
    public string Name { get { return this.name_; } }
    public bool Lock { get { return this.lock_; } set { this.lock_ = value; } }
    public enLockType LockType { get { return this.lockType_; } }
    public QuestBase Quest { get { return this.quest_; } }
    public int Id { get { return this.id_; } set { this.id_ = value; } }
    public int ListIdx { get { return this.listIdx_; } }
    public string[] ProductIDs { get { return this.productIDs_; } }
}
```

**XML初始化系统**:
```csharp
public virtual bool Initialize(XMLElement elem)
{
    // 解析基本属性
    this.name_ = elem.getStringAttribute("name", string.Empty);
    if (string.IsNullOrEmpty(this.name_))
        return false;
    
    this.listIdx_ = elem.getIntAttribute("list_index", -1);
    this.lockType_ = (enLockType)elem.getIntAttribute("lock", 0);
    
    // 处理任务解锁类型
    if (this.lockType_ == enLockType.QUEST)
    {
        string questId = elem.getStringAttribute("quest", string.Empty);
        if (!string.IsNullOrEmpty(questId))
        {
            // 通过任务建造器创建任务实例
            this.quest_ = QuestBuilderManager.Instance.Build(questId);
        }
        
        // 解析任务UI布局配置
        string questContents = elem.getStringAttribute("quest_contents", string.Empty);
        if (!string.IsNullOrEmpty(questContents))
        {
            string[] coords = questContents.Split(new char[] { ',' });
            this.questContents_ = RectHelper.CreateRect(coords);
        }
        
        this.questTitle_ = elem.getIntAttribute("quest_title", -1);
    }
    // 处理购买解锁类型
    else if (this.lockType_ == enLockType.CASH)
    {
        string productIds = elem.getStringAttribute("productIDs", string.Empty);
        if (!string.IsNullOrEmpty(productIds))
        {
            this.productIDs_ = productIds.Split(new char[] { ',' });
        }
    }
    
    return true;
}
```

**产品ID映射系统**:
```csharp
public int ProductID
{
    get
    {
        if (this.productIDs_ == null || this.productIDs_.Length == 0)
            return -1;
        
        string productId = this.productIDs_[0];
        
        // 将产品字符串(p1-p9)映射到整数索引(0-8)
        switch (productId)
        {
            case "p1": return 0;
            case "p2": return 1;
            case "p3": return 2;
            case "p4": return 3;
            case "p5": return 4;
            case "p6": return 5;
            case "p7": return 6;
            case "p8": return 7;
            case "p9": return 8;
            default: return -1;
        }
    }
}
```

**虚拟方法框架**:
```csharp
// 获取主要资产路径 - 子类必须实现
public virtual string GetMainAsset()
{
    return string.Empty;
}

// 获取所有相关资产路径 - 子类可选实现
public virtual void GetAssets(ref List<string> list)
{
    // 默认只返回主资产
    string mainAsset = this.GetMainAsset();
    if (!string.IsNullOrEmpty(mainAsset))
    {
        list.Add(mainAsset);
    }
}
```

**设计模式应用**:
- **模板方法模式**: 虚拟方法允许子类定义资产特定行为
- **策略模式**: 不同锁定类型实现不同解锁策略
- **工厂模式**: 与QuestBuilderManager协作创建任务对象

**架构决策**:
- XML配置支持数据驱动的资产定义
- 锁定类型与锁定状态分离，支持运行时状态管理
- 支持多产品ID，实现灵活的货币化策略
- 任务集成提供基于游戏玩法的进度系统

### 3. AssetSelection.cs - 资产选择状态管理器
**文件位置**: `/Assets/AssetSelection.cs`
**功能概述**: 维护玩家当前在不同资产类型间的选择状态

**数据结构**:
```csharp
public class AssetSelection
{
    // 全局静态数组，存储当前选择的资产索引
    // 索引对应: [CHARACTER, KART, TRACK]
    public static int[] selection_ = new int[3];
    
    // 访问器方法
    public static int GetSelection(AssetType assetType)
    {
        return selection_[(int)assetType];
    }
    
    public static void SetSelection(AssetType assetType, int index)
    {
        selection_[(int)assetType] = index;
    }
}
```

**使用模式**:
```csharp
// 设置当前选择的角色
AssetSelection.SetSelection(AssetType.CHARACTER, 2);

// 获取当前选择的卡丁车
int selectedKart = AssetSelection.GetSelection(AssetType.KART);

// 直接数组访问（内部使用）
int selectedTrack = AssetSelection.selection_[AssetType.TRACK];
```

**设计特点**:
- **单例模式**: 静态数组确保全局可访问性
- **状态模式**: 维护当前选择状态
- **简单设计**: 反映其作为基本状态容器的角色

**架构考虑**:
- 静态存储便于简单性和全局访问
- 固定大小数组假设稳定的资产类型枚举
- 数组大小对应`AssetType.SIZE`（3种类型）

### 4. AssetDefinitionManager.cs - 资产定义管理器基类
**文件位置**: `/Assets/AssetDefinitionManager.cs`
**功能概述**: 资产定义集合的抽象基础管理器，处理XML初始化、解锁状态管理和资产查询

**核心数据管理**:
```csharp
public abstract class AssetDefinitionManager
{
    protected List<AssetDefinition> assets_ = new List<AssetDefinition>();
    private bool isInitialized_ = false;
    
    // 抽象工厂方法 - 子类必须实现
    public virtual AssetDefinition CreateAssetDefinition()
    {
        return null;
    }
    
    // 基本属性访问
    public List<AssetDefinition> GetAssetDefinitionList()
    {
        return this.assets_;
    }
    
    public AssetDefinition GetAssetDefinition(int idx)
    {
        if (idx >= 0 && idx < this.assets_.Count)
            return this.assets_[idx];
        return null;
    }
}
```

**XML初始化系统**:
```csharp
public void Initialize(TextAsset assetDefinition)
{
    if (this.isInitialized_)
        return;
    
    if (assetDefinition != null)
    {
        StringReader reader = new StringReader(assetDefinition.text);
        XMLElement rootElement = new XMLElement();
        rootElement.parseFromReader(reader);
        
        // 处理每个资产定义
        foreach (XMLElement childElement in rootElement.getChildren())
        {
            AssetDefinition asset = this.CreateAssetDefinition();
            if (asset != null && asset.Initialize(childElement))
            {
                this.assets_.Add(asset);
                asset.Id = this.assets_.Count - 1; // 自动分配顺序ID
            }
        }
        reader.Close();
    }
    
    this.isInitialized_ = true;
}
```

**锁定状态管理系统**:
```csharp
// 恢复所有资产到默认锁定状态
public void RestoreLock()
{
    foreach (AssetDefinition asset in this.assets_)
    {
        asset.Lock = (asset.LockType != AssetDefinition.enLockType.NONE);
    }
}

// 刷新解锁状态并返回新解锁的资产列表
public void Refresh(out List<int> questUnlocked, out List<int> cashUnlocked)
{
    questUnlocked = new List<int>();
    cashUnlocked = new List<int>();
    
    for (int index = 0; index < this.assets_.Count; index++)
    {
        AssetDefinition asset = this.assets_[index];
        
        if (asset.Lock)
        {
            switch (asset.LockType)
            {
                case AssetDefinition.enLockType.NONE:
                    // 免费资产应该始终解锁
                    asset.Lock = false;
                    break;
                    
                case AssetDefinition.enLockType.CASH:
                    // 检查是否有任何产品ID已被购买
                    if (asset.ProductIDs != null)
                    {
                        foreach (string productId in asset.ProductIDs)
                        {
                            if (FiaStore.Inst.UnlockedProductList?.Contains(productId) == true)
                            {
                                asset.Lock = false;
                                cashUnlocked.Add(index);
                                break; // 只需要一个产品ID解锁即可
                            }
                        }
                    }
                    break;
                    
                case AssetDefinition.enLockType.QUEST:
                    // 检查关联任务是否完成
                    if (asset.Quest != null)
                    {
                        asset.Quest.Refresh();
                        if (asset.Quest.IsComplete())
                        {
                            asset.Lock = false;
                            questUnlocked.Add(index);
                        }
                    }
                    break;
            }
        }
    }
}

// 调用刷新但不返回解锁列表的便捷方法
public void Refresh()
{
    List<int> questUnlocked, cashUnlocked;
    this.Refresh(out questUnlocked, out cashUnlocked);
}
```

**资产检索和选择系统**:
```csharp
// 静态工厂方法 - 跨管理器统一访问
public static AssetDefinition GetAssetDefinition(AssetType assetType, int idx)
{
    AssetDefinitionManager manager = null;
    
    // 路由到相应的特化管理器
    switch (assetType)
    {
        case AssetType.TRACK:
            manager = TrackAssetDefinitionManager.Instance;
            break;
        case AssetType.KART:
            manager = KartAssetDefinitionManager.Instance;
            break;
        case AssetType.CHARACTER:
            manager = CharacterAssetDefinitionManager.Instance;
            break;
    }
    
    return manager?.GetAssetDefinition(idx);
}

// 随机资产选择（支持过滤）
public void GetRandomAsset(out byte assetIdx, bool isOnlyUnlocked, byte except)
{
    if (isOnlyUnlocked)
    {
        // 构建已解锁资产列表（排除except指定的资产）
        List<int> unlockedAssets = new List<int>();
        for (int index = 0; index < this.assets_.Count; index++)
        {
            if (!this.assets_[index].Lock && index != (int)except)
            {
                unlockedAssets.Add(index);
            }
        }
        
        if (unlockedAssets.Count > 0)
        {
            assetIdx = (byte)unlockedAssets[UnityEngine.Random.Range(0, unlockedAssets.Count)];
        }
        else
        {
            assetIdx = 0; // 默认返回第一个资产
        }
    }
    else
    {
        // 从所有资产中选择（排除except指定的资产）
        assetIdx = (byte)UnityEngine.Random.Range(0, this.assets_.Count);
        if (assetIdx >= except)
        {
            assetIdx += 1; // 跳过被排除的资产
            if (assetIdx >= this.assets_.Count)
                assetIdx = 0; // 环绕到开始
        }
    }
}

// 重载版本，提供更多过滤选项
public void GetRandomAsset(int level, int range, ref List<AssetDefinition> selectedAssets)
{
    selectedAssets.Clear();
    
    // 根据级别和范围选择合适的资产
    for (int i = 0; i < this.assets_.Count; i++)
    {
        AssetDefinition asset = this.assets_[i];
        
        // 这里可以根据具体的资产类型实现不同的选择逻辑
        // 例如，对于卡丁车可能会考虑性能级别
        if (!asset.Lock && this.IsAssetInRange(asset, level, range))
        {
            selectedAssets.Add(asset);
        }
    }
}

// 虚拟方法，子类可以重写以实现特定的范围检查逻辑
protected virtual bool IsAssetInRange(AssetDefinition asset, int level, int range)
{
    return true; // 默认实现接受所有资产
}
```

**资产排序和组织**:
```csharp
// 按列表索引顺序获取资产ID数组
public int[] GetAssetIdsByListIndexOrder()
{
    int count = this.assets_.Count;
    int[] orderedIds = new int[count];
    
    // 初始化为-1，表示未设置
    for (int i = 0; i < count; i++)
    {
        orderedIds[i] = -1;
    }
    
    // 根据ListIdx排序
    foreach (AssetDefinition asset in this.assets_)
    {
        if (asset.ListIdx >= 0 && asset.ListIdx < count)
        {
            orderedIds[asset.ListIdx] = asset.Id;
        }
    }
    
    return orderedIds;
}

// 获取主要资产路径
public string GetMainAsset(int idx)
{
    AssetDefinition asset = this.GetAssetDefinition(idx);
    return asset?.GetMainAsset() ?? string.Empty;
}

// 批量解锁所有资产（调试/作弊功能）
public void UnlockAll()
{
    foreach (AssetDefinition asset in this.assets_)
    {
        asset.Lock = false;
    }
}
```

**设计模式应用**:
- **模板方法模式**: `CreateAssetDefinition()`由子类重写
- **工厂方法模式**: 特化管理器创建适当的资产定义类型
- **单例模式**: 静态`GetAssetDefinition()`方法暗示单例管理器
- **观察者模式**: 锁定状态变化通过返回列表通知相关方

**类关系**:
- `TrackAssetDefinitionManager`、`KartAssetDefinitionManager`、`CharacterAssetDefinitionManager`的基类
- 与`AssetDefinition`对象协作进行个体资产管理
- 与`FiaStore`集成进行购买验证
- 与Unity的`TextAsset`系统协作进行配置加载

**架构决策**:
- XML配置支持数据驱动内容管理
- 关注点分离：管理器处理集合，定义处理个体资产
- 静态工厂方法提供跨不同资产类型的统一访问
- 锁定状态管理与定义分离，支持运行时进度
- 随机选择方法支持AI和游戏玩法多样性

## 系统集成点

### 与其他系统的协作
- **任务系统**: 资产可通过完成任务解锁
- **商店系统**: 资产可通过应用内购买解锁
- **游戏状态**: 当前选择在全局范围内维护
- **UI系统**: 资产信息在菜单和选择界面中显示
- **内容管道**: XML定义作为Unity TextAssets加载

### 性能考虑
- 资产定义在初始化期间一次性加载
- 锁定状态更新在刷新操作期间批量处理
- 随机选择针对过滤和非过滤情况进行优化
- 索引访问模式确保高效检索

### 扩展性设计
- 新资产类型可添加到枚举中，并通过新管理器支持
- 新锁定类型可添加以支持不同进度机制
- 资产定义可扩展额外元数据
- 管理器可特化为资产类型特定功能

## 使用示例

### 基本资产管理
```csharp
// 初始化资产管理器
KartAssetDefinitionManager kartManager = KartAssetDefinitionManager.Instance;
kartManager.Initialize(Resources.Load<TextAsset>("kartdefinition"));

// 获取特定资产
AssetDefinition kart = kartManager.GetAssetDefinition(0);
string kartName = kart.Name;
bool isLocked = kart.Lock;

// 检查解锁状态更新
List<int> questUnlocked, cashUnlocked;
kartManager.Refresh(out questUnlocked, out cashUnlocked);

foreach (int idx in cashUnlocked)
{
    Debug.Log($"Kart {idx} unlocked by purchase!");
}
```

### 随机资产选择
```csharp
// 获取随机已解锁卡丁车（排除当前选择）
byte currentKart = (byte)AssetSelection.GetSelection(AssetType.KART);
byte randomKart;
kartManager.GetRandomAsset(out randomKart, true, currentKart);

// 设置新选择
AssetSelection.SetSelection(AssetType.KART, randomKart);
```

### 跨类型资产访问
```csharp
// 使用静态工厂方法访问任意类型资产
AssetDefinition character = AssetDefinitionManager.GetAssetDefinition(AssetType.CHARACTER, 2);
AssetDefinition track = AssetDefinitionManager.GetAssetDefinition(AssetType.TRACK, 5);

// 获取资产的主要文件路径
string characterAssetPath = character.GetMainAsset();
string trackAssetPath = track.GetMainAsset();
```

## 总结

Assets系统提供了一个功能全面、设计良好的资产管理框架，具有以下优势：

### 核心优势
1. **数据驱动**: XML配置支持无需代码修改的内容更新
2. **类型安全**: 强类型系统防止资产管理错误
3. **灵活解锁**: 支持多种解锁机制（免费、购买、任务）
4. **状态管理**: 运行时状态与静态定义清晰分离
5. **性能优化**: 批量操作和高效的查询机制
6. **可扩展性**: 清晰的继承层次支持新资产类型

### 架构特点
- **模块化设计**: 每个组件职责明确，易于维护
- **继承层次**: 支持资产类型特化的同时保持代码复用
- **工厂模式**: 统一的创建和访问接口
- **配置驱动**: 外部配置支持灵活的内容管理

该资产管理系统为游戏内容的组织、进度跟踪和货币化提供了坚实的技术基础，同时保持了良好的可维护性和扩展性。