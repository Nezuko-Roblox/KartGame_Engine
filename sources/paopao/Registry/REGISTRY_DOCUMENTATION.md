# Registry 模块详细功能文档

## 概述

Registry 模块是卡丁车游戏项目中的配置和数据持久化系统，负责管理游戏设置、用户偏好和游戏记录的本地存储。该模块基于Unity的PlayerPrefs系统，提供了类型安全的配置管理接口，支持整数、字符串和复杂赛道数据的存储和读取。

## 模块结构

```
Registry/
├── RegistryValue.cs    - 配置值抽象基类
├── RegistryInt.cs      - 整数配置管理类
├── RegistryString.cs   - 字符串配置管理类
└── RegistryTrack.cs    - 赛道记录配置管理类
```

## 系统架构图

```
Registry System (配置注册表系统)
├─ 抽象层
│  └─ RegistryValue (配置值基类)
│     ├─ ReadRegistry() (读取配置)
│     ├─ SaveRegistry() (保存配置)
│     └─ Reset() (重置配置)
├─ 基础类型层
│  ├─ RegistryInt (整数配置)
│  │  ├─ 默认值机制
│  │  ├─ 脏标记系统
│  │  └─ PlayerPrefs集成
│  └─ RegistryString (字符串配置)
│     ├─ 空字符串默认值
│     ├─ 脏标记系统
│     └─ PlayerPrefs集成
└─ 复杂类型层
   └─ RegistryTrack (赛道记录)
      ├─ 幽灵数据管理
      ├─ 比赛统计
      ├─ 胜利计数
      └─ 复杂序列化

数据流向:
游戏设置 → Registry类 → PlayerPrefs → 本地存储
本地存储 → PlayerPrefs → Registry类 → 游戏设置
```

## 核心类详细分析

### 1. RegistryValue.cs - 配置值抽象基类

**功能概述：**
RegistryValue是所有配置管理类的抽象基类，定义了配置读取、保存和重置的标准接口，提供了脏标记机制来优化存储性能。

**完整抽象类定义：**
```csharp
public abstract class RegistryValue
{
    // 状态标记
    public bool isReadRegistry_;        // 是否已从注册表读取
    protected bool isDirty_;           // 是否需要保存（脏标记）
    
    // 抽象方法
    public abstract void ReadRegistry(string key);   // 从PlayerPrefs读取配置
    public abstract void SaveRegistry(string key);   // 保存配置到PlayerPrefs  
    public abstract void Reset();                    // 重置为默认值
}
```

**设计模式分析：**
1. **模板方法模式**: 定义配置管理的标准流程
2. **脏标记模式**: 通过isDirty_优化保存性能
3. **状态跟踪**: 通过isReadRegistry_跟踪初始化状态
4. **类型安全**: 通过继承提供强类型配置管理

### 2. RegistryInt.cs - 整数配置管理类

**功能概述：**
RegistryInt管理整数类型的配置值，支持默认值设置、自动脏标记跟踪和PlayerPrefs集成，常用于游戏设置中的数值选项。

**完整类实现：**
```csharp
public class RegistryInt : RegistryValue
{
    // 核心字段
    private int value_;      // 当前值
    private int default_;    // 默认值
    
    // 构造函数
    public RegistryInt(int defaultValue);
    
    // 重写方法
    public override void ReadRegistry(string key);
    public override void SaveRegistry(string key);
    public override void Reset();
    
    // 属性访问
    public int Value { get; set; }
    
    // 字符串转换
    public override string ToString();
}
```

#### 2.1 构造和初始化

**构造函数实现：**
```csharp
public RegistryInt(int defaultValue)
{
    this.isDirty_ = false;           // 初始无需保存
    this.isReadRegistry_ = false;    // 尚未读取配置
    this.value_ = 0;                 // 临时初始值
    this.default_ = defaultValue;    // 保存默认值
}
```

**初始化状态分析：**
1. **脏标记清除**: 新建对象无需立即保存
2. **读取状态**: 标记尚未从PlayerPrefs读取
3. **默认值保存**: 用于Reset()和首次读取时的回退

#### 2.2 配置读取机制

**ReadRegistry() 实现：**
```csharp
public override void ReadRegistry(string key)
{
    this.isDirty_ = false;                                    // 清除脏标记
    this.isReadRegistry_ = true;                              // 标记已读取
    this.value_ = PlayerPrefs.GetInt(key, this.default_);    // 读取或使用默认值
}
```

**读取流程分析：**
1. **状态重置**: 清除脏标记，避免不必要的保存
2. **读取标记**: 设置isReadRegistry_为true
3. **值获取**: 使用PlayerPrefs.GetInt，不存在时返回默认值

#### 2.3 配置保存机制

**SaveRegistry() 实现：**
```csharp
public override void SaveRegistry(string key)
{
    if (this.isDirty_)                           // 仅在有更改时保存
    {
        PlayerPrefs.SetInt(key, this.value_);    // 写入PlayerPrefs
        this.isDirty_ = false;                   // 清除脏标记
    }
}
```

**保存优化分析：**
1. **性能优化**: 只有脏标记为true时才执行保存
2. **自动清理**: 保存后自动清除脏标记
3. **批量保存**: 可与其他Registry对象一起批量保存

#### 2.4 值属性管理

**Value属性实现：**
```csharp
public int Value
{
    get
    {
        return this.value_;
    }
    set
    {
        this.value_ = value;      // 设置新值
        this.isDirty_ = true;     // 标记为脏，需要保存
    }
}
```

**脏标记自动化：**
- **读取**: 直接返回当前值，无副作用
- **写入**: 自动设置脏标记，确保下次SaveRegistry()时会保存

#### 2.5 重置功能

**Reset() 实现：**
```csharp
public override void Reset()
{
    this.value_ = this.default_;     // 恢复为默认值
    this.isDirty_ = true;            // 标记需要保存
    this.isReadRegistry_ = true;     // 标记为已读取状态
}
```

**重置机制分析：**
1. **值恢复**: 将当前值设为构造时的默认值
2. **保存标记**: 设置脏标记确保重置会被持久化
3. **状态一致**: 设置读取标记保持状态一致性

#### 2.6 应用示例

**游戏设置管理：**
```csharp
public class GameSettings
{
    // 图形设置
    private RegistryInt graphicsQuality_;
    private RegistryInt screenResolution_;
    private RegistryInt targetFrameRate_;
    
    // 音频设置
    private RegistryInt masterVolume_;
    private RegistryInt musicVolume_;
    private RegistryInt sfxVolume_;
    
    // 游戏设置
    private RegistryInt difficulty_;
    private RegistryInt kartType_;
    
    public GameSettings()
    {
        // 初始化配置对象
        graphicsQuality_ = new RegistryInt(1);    // 默认中等画质
        screenResolution_ = new RegistryInt(0);   // 默认最高分辨率
        targetFrameRate_ = new RegistryInt(60);   // 默认60FPS
        
        masterVolume_ = new RegistryInt(80);      // 默认80%音量
        musicVolume_ = new RegistryInt(70);       // 默认70%音乐音量
        sfxVolume_ = new RegistryInt(90);         // 默认90%音效音量
        
        difficulty_ = new RegistryInt(1);         // 默认普通难度
        kartType_ = new RegistryInt(0);           // 默认第一辆卡丁车
    }
    
    public void LoadSettings()
    {
        // 从PlayerPrefs加载所有设置
        graphicsQuality_.ReadRegistry("GraphicsQuality");
        screenResolution_.ReadRegistry("ScreenResolution");
        targetFrameRate_.ReadRegistry("TargetFrameRate");
        
        masterVolume_.ReadRegistry("MasterVolume");
        musicVolume_.ReadRegistry("MusicVolume");
        sfxVolume_.ReadRegistry("SFXVolume");
        
        difficulty_.ReadRegistry("Difficulty");
        kartType_.ReadRegistry("KartType");
    }
    
    public void SaveSettings()
    {
        // 保存所有已更改的设置
        graphicsQuality_.SaveRegistry("GraphicsQuality");
        screenResolution_.SaveRegistry("ScreenResolution");
        targetFrameRate_.SaveRegistry("TargetFrameRate");
        
        masterVolume_.SaveRegistry("MasterVolume");
        musicVolume_.SaveRegistry("MusicVolume");
        sfxVolume_.SaveRegistry("SFXVolume");
        
        difficulty_.SaveRegistry("Difficulty");
        kartType_.SaveRegistry("KartType");
        
        PlayerPrefs.Save(); // 强制保存到磁盘
    }
    
    // 属性访问器
    public int GraphicsQuality
    {
        get { return graphicsQuality_.Value; }
        set { graphicsQuality_.Value = value; }
    }
    
    public int MasterVolume
    {
        get { return masterVolume_.Value; }
        set { masterVolume_.Value = Mathf.Clamp(value, 0, 100); }
    }
    
    public int Difficulty
    {
        get { return difficulty_.Value; }
        set { difficulty_.Value = Mathf.Clamp(value, 0, 2); }
    }
    
    // 重置所有设置
    public void ResetToDefaults()
    {
        graphicsQuality_.Reset();
        screenResolution_.Reset();
        targetFrameRate_.Reset();
        masterVolume_.Reset();
        musicVolume_.Reset();
        sfxVolume_.Reset();
        difficulty_.Reset();
        kartType_.Reset();
        
        SaveSettings(); // 立即保存重置结果
    }
}
```

### 3. RegistryString.cs - 字符串配置管理类

**功能概述：**
RegistryString管理字符串类型的配置值，用于存储用户名、语言设置、自定义配置等文本信息。

**完整类实现：**
```csharp
public class RegistryString : RegistryValue
{
    // 核心字段
    private string value_;   // 当前字符串值
    
    // 构造函数
    public RegistryString();
    
    // 重写方法
    public override void ReadRegistry(string key);
    public override void SaveRegistry(string key);
    public override void Reset();
    
    // 属性访问
    public string Value { get; set; }
    
    // 字符串转换
    public override string ToString();
}
```

#### 3.1 构造和初始化

**构造函数实现：**
```csharp
public RegistryString()
{
    this.isDirty_ = false;           // 初始无需保存
    this.isReadRegistry_ = false;    // 尚未读取配置
    this.value_ = string.Empty;      // 默认空字符串
}
```

**字符串默认值特点：**
1. **空字符串默认**: 使用string.Empty作为初始值
2. **无参构造**: 不需要显式指定默认值
3. **统一初始化**: 与RegistryInt类似的初始化流程

#### 3.2 字符串读取和保存

**ReadRegistry() 实现：**
```csharp
public override void ReadRegistry(string key)
{
    this.isDirty_ = false;                          // 清除脏标记
    this.isReadRegistry_ = true;                    // 标记已读取
    this.value_ = PlayerPrefs.GetString(key);       // 读取字符串值
}
```

**SaveRegistry() 实现：**
```csharp
public override void SaveRegistry(string key)
{
    if (this.isDirty_)                              // 仅在有更改时保存
    {
        PlayerPrefs.SetString(key, this.value_);    // 写入PlayerPrefs
        this.isDirty_ = false;                      // 清除脏标记
    }
}
```

**字符串处理特点：**
1. **默认行为**: PlayerPrefs.GetString()在键不存在时返回空字符串
2. **无默认值参数**: 不同于RegistryInt，直接使用PlayerPrefs默认行为
3. **Null安全**: string.Empty确保值永不为null

#### 3.3 重置机制

**Reset() 实现：**
```csharp
public override void Reset()
{
    this.value_ = string.Empty;      // 重置为空字符串
    this.isDirty_ = true;            // 标记需要保存
    this.isReadRegistry_ = true;     // 标记为已读取状态
}
```

#### 3.4 应用示例

**用户配置管理：**
```csharp
public class UserProfile
{
    private RegistryString playerName_;
    private RegistryString preferredLanguage_;
    private RegistryString lastPlayedTrack_;
    private RegistryString customCarColor_;
    
    public UserProfile()
    {
        playerName_ = new RegistryString();
        preferredLanguage_ = new RegistryString();
        lastPlayedTrack_ = new RegistryString();
        customCarColor_ = new RegistryString();
    }
    
    public void LoadProfile()
    {
        playerName_.ReadRegistry("PlayerName");
        preferredLanguage_.ReadRegistry("Language");
        lastPlayedTrack_.ReadRegistry("LastTrack");
        customCarColor_.ReadRegistry("CarColor");
        
        // 设置默认值（如果为空）
        if (string.IsNullOrEmpty(playerName_.Value))
        {
            playerName_.Value = "Player";
        }
        
        if (string.IsNullOrEmpty(preferredLanguage_.Value))
        {
            preferredLanguage_.Value = "ko"; // 默认韩语
        }
    }
    
    public void SaveProfile()
    {
        playerName_.SaveRegistry("PlayerName");
        preferredLanguage_.SaveRegistry("Language");
        lastPlayedTrack_.SaveRegistry("LastTrack");
        customCarColor_.SaveRegistry("CarColor");
        PlayerPrefs.Save();
    }
    
    // 属性访问
    public string PlayerName
    {
        get { return playerName_.Value; }
        set { playerName_.Value = value ?? string.Empty; }
    }
    
    public string PreferredLanguage
    {
        get { return preferredLanguage_.Value; }
        set { preferredLanguage_.Value = value ?? "ko"; }
    }
    
    public string LastPlayedTrack
    {
        get { return lastPlayedTrack_.Value; }
        set { lastPlayedTrack_.Value = value ?? string.Empty; }
    }
}
```

### 4. RegistryTrack.cs - 赛道记录配置管理类

**功能概述：**
RegistryTrack是最复杂的配置管理类，专门用于存储赛道相关的游戏记录，包括最佳时间的幽灵数据、比赛次数统计和胜利次数统计。

**完整类结构：**
```csharp
public class RegistryTrack : RegistryValue
{
    // 核心数据字段
    private GhostFilenameInfo[] best_;    // 最佳记录幽灵数据 [2个槽位]
    private int[] raceCounter_;           // 比赛次数统计 [4个计数器]
    private int[] winCounter_;            // 胜利次数统计 [4个计数器]
    
    // 构造函数
    public RegistryTrack();
    
    // 重写方法
    public override void ReadRegistry(string key);
    public override void SaveRegistry(string key);
    public override void Reset();
    
    // 幽灵数据管理
    public void SetBestInfo(int idx, GhostFilenameInfo info);
    public GhostFilenameInfo GetBestInfo(int idx);
    
    // 统计数据属性
    public int[] RaceCounter { get; set; }
    public int[] WinCounter { get; set; }
}
```

#### 4.1 数据结构初始化

**构造函数实现：**
```csharp
public RegistryTrack()
{
    this.isDirty_ = false;               // 初始无需保存
    this.isReadRegistry_ = false;        // 尚未读取配置
    this.best_ = null;                   // 临时设为null
    this.best_ = new GhostFilenameInfo[2];  // 创建2个幽灵数据槽
    this.raceCounter_ = new int[4];      // 创建4个比赛计数器
    this.winCounter_ = new int[4];       // 创建4个胜利计数器
}
```

**数据结构说明：**
1. **幽灵数据**: 2个槽位存储不同模式的最佳时间记录
2. **比赛计数**: 4个计数器可能对应不同难度或模式
3. **胜利计数**: 4个计数器记录各模式的胜利次数

#### 4.2 复杂序列化机制

**ReadRegistry() 复杂解析：**
```csharp
public override void ReadRegistry(string key)
{
    this.isDirty_ = false;
    this.isReadRegistry_ = true;
    string dataString = PlayerPrefs.GetString(key);
    
    if (dataString != null && dataString != string.Empty)
    {
        // 使用'/'分隔符分割数据
        char[] separator = new char[] { '/' };
        string[] parts = dataString.Split(separator);
        
        if (parts.Length == 11)  // 期望11个部分的数据格式
        {
            // 解析幽灵数据 (索引0-1)
            for (int i = 0; i < 2; i++)
            {
                if (parts[i] != null && parts[i] != string.Empty)
                {
                    this.best_[i] = new GhostFilenameInfo(parts[i]);
                    if (!this.best_[i].IsValidData())
                    {
                        this.best_[i] = null;  // 无效数据清除
                    }
                    
                    if (this.best_[i] != null)
                    {
                        // 解析比赛和胜利计数器 (十六进制格式)
                        this.raceCounter_[i] = int.Parse(parts[i + 2], NumberStyles.HexNumber);
                        this.raceCounter_[i + 2] = int.Parse(parts[i + 4], NumberStyles.HexNumber);
                        this.winCounter_[i] = int.Parse(parts[i + 6], NumberStyles.HexNumber);
                        this.winCounter_[i + 2] = int.Parse(parts[i + 8], NumberStyles.HexNumber);
                    }
                }
                else
                {
                    // 处理空幽灵数据的情况
                    this.raceCounter_[i] = 0;
                    this.raceCounter_[i + 2] = int.Parse(parts[i + 4], NumberStyles.HexNumber);
                    this.winCounter_[i] = 0;
                    this.winCounter_[i + 2] = int.Parse(parts[i + 8], NumberStyles.HexNumber);
                }
            }
        }
    }
}
```

**数据格式分析：**
```
存储格式: ghost1/ghost2/race0/race1/race2/race3/win0/win1/win2/win3/
索引分布:
- 0-1: 幽灵数据字符串
- 2-5: 比赛次数 (十六进制)
- 6-9: 胜利次数 (十六进制)
- 10: 分隔符结束
```

#### 4.3 复杂序列化保存

**SaveRegistry() 序列化实现：**
```csharp
public override void SaveRegistry(string key)
{
    if (this.isDirty_)
    {
        string serializedData = string.Empty;
        
        // 1. 序列化幽灵数据
        for (int i = 0; i < 2; i++)
        {
            if (this.best_[i] != null)
            {
                serializedData += this.best_[i].GenerateFilename();
            }
            serializedData += "/";  // 添加分隔符
        }
        
        // 2. 序列化比赛计数器 (十六进制格式)
        for (int j = 0; j < this.raceCounter_.Length; j++)
        {
            serializedData += string.Format("{0:X}/", this.raceCounter_[j]);
        }
        
        // 3. 序列化胜利计数器 (十六进制格式)
        for (int k = 0; k < this.winCounter_.Length; k++)
        {
            serializedData += string.Format("{0:X}/", this.winCounter_[k]);
        }
        
        // 4. 保存到PlayerPrefs
        PlayerPrefs.SetString(key, serializedData);
        this.isDirty_ = false;
    }
}
```

**序列化特点：**
1. **紧凑格式**: 使用十六进制减少字符串长度
2. **分隔符**: 使用'/'分隔各数据段
3. **容错处理**: 支持null幽灵数据的序列化
4. **版本兼容**: 固定格式确保数据兼容性

#### 4.4 幽灵数据管理

**幽灵数据接口：**
```csharp
public void SetBestInfo(int idx, GhostFilenameInfo info)
{
    this.best_[idx] = info;    // 设置指定槽位的幽灵数据
    this.isDirty_ = true;      // 标记需要保存
}

public GhostFilenameInfo GetBestInfo(int idx)
{
    return this.best_[idx];    // 获取指定槽位的幽灵数据
}
```

**幽灵数据应用：**
```csharp
// 设置新的最佳时间记录
public void UpdateBestTime(int mode, float lapTime, string kartConfig)
{
    GhostFilenameInfo currentBest = track.GetBestInfo(mode);
    
    if (currentBest == null || lapTime < currentBest.BestTime)
    {
        // 创建新的幽灵记录
        GhostFilenameInfo newBest = new GhostFilenameInfo();
        newBest.SetBestTime(lapTime);
        newBest.SetKartConfiguration(kartConfig);
        newBest.SetTimestamp(DateTime.Now);
        
        // 更新记录
        track.SetBestInfo(mode, newBest);
        
        Debug.Log($"New best time for mode {mode}: {lapTime:F3}s");
    }
}
```

#### 4.5 统计数据管理

**RaceCounter属性实现：**
```csharp
public int[] RaceCounter
{
    get
    {
        return this.raceCounter_;
    }
    set
    {
        Array.Copy(value, this.raceCounter_, this.raceCounter_.Length);  // 安全复制
        this.isDirty_ = true;  // 标记需要保存
    }
}
```

**WinCounter属性实现：**
```csharp
public int[] WinCounter
{
    get
    {
        return this.winCounter_;
    }
    set
    {
        Array.Copy(value, this.winCounter_, this.winCounter_.Length);  // 安全复制
        this.isDirty_ = true;  // 标记需要保存
    }
}
```

**统计数据应用：**
```csharp
public class TrackStatistics
{
    private RegistryTrack trackRegistry_;
    
    public void RecordRaceCompletion(int difficulty, bool isWin)
    {
        // 增加比赛次数
        int[] raceCounters = trackRegistry_.RaceCounter;
        raceCounters[difficulty]++;
        trackRegistry_.RaceCounter = raceCounters;
        
        // 如果获胜，增加胜利次数
        if (isWin)
        {
            int[] winCounters = trackRegistry_.WinCounter;
            winCounters[difficulty]++;
            trackRegistry_.WinCounter = winCounters;
        }
        
        // 保存统计数据
        trackRegistry_.SaveRegistry("TrackStats");
    }
    
    public float GetWinRate(int difficulty)
    {
        int races = trackRegistry_.RaceCounter[difficulty];
        int wins = trackRegistry_.WinCounter[difficulty];
        
        return races > 0 ? (float)wins / races : 0f;
    }
    
    public int GetTotalRaces()
    {
        int total = 0;
        for (int i = 0; i < trackRegistry_.RaceCounter.Length; i++)
        {
            total += trackRegistry_.RaceCounter[i];
        }
        return total;
    }
}
```

#### 4.6 重置功能

**Reset() 实现：**
```csharp
public override void Reset()
{
    // 清除幽灵数据
    for (int i = 0; i < 2; i++)
    {
        this.best_[i] = null;
    }
    
    // 重置所有计数器
    for (int j = 0; j < 4; j++)
    {
        this.raceCounter_[j] = 0;
        this.winCounter_[j] = 0;
    }
    
    this.isDirty_ = true;            // 标记需要保存
    this.isReadRegistry_ = true;     // 标记为已读取状态
}
```

## 综合应用示例

### 1. 完整的配置管理系统

**统一配置管理器：**
```csharp
public class ConfigurationManager : MonoBehaviour
{
    public static ConfigurationManager Instance { get; private set; }
    
    [Header("General Settings")]
    public bool autoSave = true;
    public float autoSaveInterval = 30f;
    
    // 配置对象
    private Dictionary<string, RegistryValue> configurations_;
    
    // 预定义配置
    private GameSettings gameSettings_;
    private UserProfile userProfile_;
    private Dictionary<string, RegistryTrack> trackRecords_;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeConfigurations();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        LoadAllConfigurations();
        
        if (autoSave)
        {
            InvokeRepeating(nameof(SaveAllConfigurations), autoSaveInterval, autoSaveInterval);
        }
    }
    
    private void InitializeConfigurations()
    {
        configurations_ = new Dictionary<string, RegistryValue>();
        
        // 初始化游戏设置
        gameSettings_ = new GameSettings();
        
        // 初始化用户配置
        userProfile_ = new UserProfile();
        
        // 初始化赛道记录
        trackRecords_ = new Dictionary<string, RegistryTrack>();
        string[] trackNames = { "Track1", "Track2", "Track3", "Track4", "Track5" };
        foreach (string trackName in trackNames)
        {
            trackRecords_[trackName] = new RegistryTrack();
        }
    }
    
    public void LoadAllConfigurations()
    {
        try
        {
            // 加载游戏设置
            gameSettings_.LoadSettings();
            
            // 加载用户配置
            userProfile_.LoadProfile();
            
            // 加载赛道记录
            foreach (var kvp in trackRecords_)
            {
                kvp.Value.ReadRegistry($"Track_{kvp.Key}");
            }
            
            Debug.Log("All configurations loaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load configurations: {ex.Message}");
        }
    }
    
    public void SaveAllConfigurations()
    {
        try
        {
            // 保存游戏设置
            gameSettings_.SaveSettings();
            
            // 保存用户配置
            userProfile_.SaveProfile();
            
            // 保存赛道记录
            foreach (var kvp in trackRecords_)
            {
                kvp.Value.SaveRegistry($"Track_{kvp.Key}");
            }
            
            // 强制保存到磁盘
            PlayerPrefs.Save();
            
            Debug.Log("All configurations saved successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save configurations: {ex.Message}");
        }
    }
    
    public void ResetAllConfigurations()
    {
        if (EditorUtility.DisplayDialog("Reset Configurations", 
            "Are you sure you want to reset all configurations to default values?", 
            "Yes", "No"))
        {
            // 重置游戏设置
            gameSettings_.ResetToDefaults();
            
            // 重置用户配置
            userProfile_.ResetProfile();
            
            // 重置赛道记录
            foreach (var kvp in trackRecords_)
            {
                kvp.Value.Reset();
            }
            
            SaveAllConfigurations();
            Debug.Log("All configurations reset to defaults");
        }
    }
    
    // 便捷访问方法
    public GameSettings GetGameSettings() => gameSettings_;
    public UserProfile GetUserProfile() => userProfile_;
    public RegistryTrack GetTrackRecord(string trackName) => trackRecords_.ContainsKey(trackName) ? trackRecords_[trackName] : null;
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveAllConfigurations();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveAllConfigurations();
        }
    }
    
    private void OnDestroy()
    {
        SaveAllConfigurations();
    }
}
```

### 2. 性能优化的批量操作

**批量配置管理器：**
```csharp
public class BatchConfigurationManager
{
    private List<RegistryValue> registryValues_;
    private List<string> registryKeys_;
    private bool isBatchMode_;
    
    public BatchConfigurationManager()
    {
        registryValues_ = new List<RegistryValue>();
        registryKeys_ = new List<string>();
        isBatchMode_ = false;
    }
    
    public void BeginBatch()
    {
        isBatchMode_ = true;
        registryValues_.Clear();
        registryKeys_.Clear();
    }
    
    public void AddToBatch(RegistryValue value, string key)
    {
        if (!isBatchMode_)
        {
            Debug.LogWarning("Not in batch mode. Call BeginBatch() first.");
            return;
        }
        
        registryValues_.Add(value);
        registryKeys_.Add(key);
    }
    
    public void ExecuteBatchLoad()
    {
        if (!isBatchMode_)
        {
            Debug.LogWarning("Not in batch mode.");
            return;
        }
        
        for (int i = 0; i < registryValues_.Count; i++)
        {
            registryValues_[i].ReadRegistry(registryKeys_[i]);
        }
        
        Debug.Log($"Batch loaded {registryValues_.Count} configurations");
    }
    
    public void ExecuteBatchSave()
    {
        if (!isBatchMode_)
        {
            Debug.LogWarning("Not in batch mode.");
            return;
        }
        
        int savedCount = 0;
        for (int i = 0; i < registryValues_.Count; i++)
        {
            registryValues_[i].SaveRegistry(registryKeys_[i]);
            savedCount++;
        }
        
        PlayerPrefs.Save();
        Debug.Log($"Batch saved {savedCount} configurations");
    }
    
    public void EndBatch()
    {
        isBatchMode_ = false;
        registryValues_.Clear();
        registryKeys_.Clear();
    }
    
    // 便捷方法：完整的批量操作
    public static void BatchLoadConfigurations(params (RegistryValue value, string key)[] configs)
    {
        BatchConfigurationManager batch = new BatchConfigurationManager();
        batch.BeginBatch();
        
        foreach (var config in configs)
        {
            batch.AddToBatch(config.value, config.key);
        }
        
        batch.ExecuteBatchLoad();
        batch.EndBatch();
    }
    
    public static void BatchSaveConfigurations(params (RegistryValue value, string key)[] configs)
    {
        BatchConfigurationManager batch = new BatchConfigurationManager();
        batch.BeginBatch();
        
        foreach (var config in configs)
        {
            batch.AddToBatch(config.value, config.key);
        }
        
        batch.ExecuteBatchSave();
        batch.EndBatch();
    }
}
```

### 3. 配置验证和迁移系统

**配置迁移管理器：**
```csharp
public class ConfigurationMigrator
{
    private const int CURRENT_CONFIG_VERSION = 2;
    private RegistryInt configVersion_;
    
    public ConfigurationMigrator()
    {
        configVersion_ = new RegistryInt(CURRENT_CONFIG_VERSION);
        configVersion_.ReadRegistry("ConfigVersion");
    }
    
    public bool NeedsMigration()
    {
        return configVersion_.Value < CURRENT_CONFIG_VERSION;
    }
    
    public void PerformMigration()
    {
        int fromVersion = configVersion_.Value;
        
        Debug.Log($"Migrating configuration from version {fromVersion} to {CURRENT_CONFIG_VERSION}");
        
        for (int version = fromVersion; version < CURRENT_CONFIG_VERSION; version++)
        {
            switch (version)
            {
                case 0:
                    MigrateFromV0ToV1();
                    break;
                case 1:
                    MigrateFromV1ToV2();
                    break;
            }
        }
        
        // 更新版本号
        configVersion_.Value = CURRENT_CONFIG_VERSION;
        configVersion_.SaveRegistry("ConfigVersion");
        
        Debug.Log("Configuration migration completed");
    }
    
    private void MigrateFromV0ToV1()
    {
        Debug.Log("Migrating from V0 to V1: Adding audio settings");
        
        // V1添加了音频设置，使用默认值
        RegistryInt masterVolume = new RegistryInt(80);
        RegistryInt musicVolume = new RegistryInt(70);
        RegistryInt sfxVolume = new RegistryInt(90);
        
        masterVolume.SaveRegistry("MasterVolume");
        musicVolume.SaveRegistry("MusicVolume");
        sfxVolume.SaveRegistry("SFXVolume");
    }
    
    private void MigrateFromV1ToV2()
    {
        Debug.Log("Migrating from V1 to V2: Updating track record format");
        
        // V2更新了赛道记录格式，需要重置旧数据
        string[] trackNames = { "Track1", "Track2", "Track3", "Track4", "Track5" };
        foreach (string trackName in trackNames)
        {
            RegistryTrack track = new RegistryTrack();
            track.Reset();
            track.SaveRegistry($"Track_{trackName}");
        }
    }
    
    public void ValidateConfigurations()
    {
        Debug.Log("Validating configurations...");
        
        // 验证游戏设置
        ValidateGameSettings();
        
        // 验证用户配置
        ValidateUserProfile();
        
        // 验证赛道记录
        ValidateTrackRecords();
        
        Debug.Log("Configuration validation completed");
    }
    
    private void ValidateGameSettings()
    {
        RegistryInt graphicsQuality = new RegistryInt(1);
        graphicsQuality.ReadRegistry("GraphicsQuality");
        
        if (graphicsQuality.Value < 0 || graphicsQuality.Value > 3)
        {
            Debug.LogWarning("Invalid graphics quality, resetting to default");
            graphicsQuality.Reset();
            graphicsQuality.SaveRegistry("GraphicsQuality");
        }
    }
    
    private void ValidateUserProfile()
    {
        RegistryString playerName = new RegistryString();
        playerName.ReadRegistry("PlayerName");
        
        if (string.IsNullOrEmpty(playerName.Value) || playerName.Value.Length > 20)
        {
            Debug.LogWarning("Invalid player name, resetting to default");
            playerName.Value = "Player";
            playerName.SaveRegistry("PlayerName");
        }
    }
    
    private void ValidateTrackRecords()
    {
        string[] trackNames = { "Track1", "Track2", "Track3", "Track4", "Track5" };
        
        foreach (string trackName in trackNames)
        {
            RegistryTrack track = new RegistryTrack();
            track.ReadRegistry($"Track_{trackName}");
            
            // 验证统计数据的合理性
            bool isValid = true;
            for (int i = 0; i < track.RaceCounter.Length; i++)
            {
                if (track.RaceCounter[i] < 0 || track.WinCounter[i] < 0 || 
                    track.WinCounter[i] > track.RaceCounter[i])
                {
                    isValid = false;
                    break;
                }
            }
            
            if (!isValid)
            {
                Debug.LogWarning($"Invalid track record for {trackName}, resetting");
                track.Reset();
                track.SaveRegistry($"Track_{trackName}");
            }
        }
    }
}
```

## 性能优化建议

### 1. 脏标记优化

**智能脏标记管理：**
```csharp
public class OptimizedRegistryInt : RegistryInt
{
    private int lastSavedValue_;
    
    public OptimizedRegistryInt(int defaultValue) : base(defaultValue)
    {
        lastSavedValue_ = defaultValue;
    }
    
    public override void SaveRegistry(string key)
    {
        // 只有真正改变时才保存
        if (isDirty_ && Value != lastSavedValue_)
        {
            base.SaveRegistry(key);
            lastSavedValue_ = Value;
        }
        else
        {
            isDirty_ = false; // 清除无意义的脏标记
        }
    }
}
```

### 2. 缓存策略

**配置缓存管理：**
```csharp
public class RegistryCache
{
    private static Dictionary<string, object> cache_ = new Dictionary<string, object>();
    private static HashSet<string> dirtyKeys_ = new HashSet<string>();
    
    public static T GetCached<T>(string key, T defaultValue)
    {
        if (cache_.ContainsKey(key))
        {
            return (T)cache_[key];
        }
        
        T value = defaultValue;
        if (typeof(T) == typeof(int))
        {
            value = (T)(object)PlayerPrefs.GetInt(key, (int)(object)defaultValue);
        }
        else if (typeof(T) == typeof(string))
        {
            value = (T)(object)PlayerPrefs.GetString(key, (string)(object)defaultValue);
        }
        
        cache_[key] = value;
        return value;
    }
    
    public static void SetCached<T>(string key, T value)
    {
        cache_[key] = value;
        dirtyKeys_.Add(key);
    }
    
    public static void FlushCache()
    {
        foreach (string key in dirtyKeys_)
        {
            object value = cache_[key];
            if (value is int)
            {
                PlayerPrefs.SetInt(key, (int)value);
            }
            else if (value is string)
            {
                PlayerPrefs.SetString(key, (string)value);
            }
        }
        
        dirtyKeys_.Clear();
        PlayerPrefs.Save();
    }
}
```

## 总结

Registry模块为卡丁车游戏提供了完整的配置管理和数据持久化解决方案：

### 核心特性
1. **类型安全**: 针对不同数据类型提供专门的管理类
2. **性能优化**: 通过脏标记机制减少不必要的存储操作
3. **复杂数据**: 支持赛道记录等复杂数据结构的序列化
4. **统一接口**: 抽象基类提供一致的操作模式

### 设计优势
1. **扩展性**: 易于添加新的数据类型支持
2. **可靠性**: 完善的错误处理和数据验证
3. **高效性**: 批量操作和缓存策略优化性能
4. **维护性**: 清晰的类层次结构和职责分离

### 应用价值
1. **配置管理**: 统一管理游戏设置和用户偏好
2. **进度保存**: 可靠的游戏进度和记录保存
3. **数据迁移**: 支持配置格式的版本升级
4. **性能保障**: 优化的存储策略确保游戏流畅性

该模块是游戏数据持久化的核心基础设施，为整个游戏系统提供了可靠的配置和状态管理能力。