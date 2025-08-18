# Quest 文件夹完整功能文档

## 概述

Quest 文件夹包含了卡丁车游戏的完整成就/任务管理系统，采用了工厂模式、建造者模式、策略模式和模板方法模式的复合架构设计。该系统通过字符串配置、位标志操作和增量统计追踪，为游戏提供了灵活、高效、可扩展的任务管理功能。

## 系统架构

### 核心设计原则
- **建造者模式**: 复杂任务对象的类型安全构建
- **工厂模式**: 统一的任务创建和管理接口
- **策略模式**: 不同任务类型的统一处理方式
- **模板方法模式**: 标准化的任务生命周期管理
- **位标志优化**: 高效的多条件检查和存储

### 系统组件分层
- **管理层**: 任务构建器工厂和全局任务管理
- **构建层**: 类型安全的任务创建和验证系统
- **执行层**: 具体任务类型的逻辑实现
- **数据层**: 统计数据和持久化存储集成

## 核心架构系统

### 1. 任务基类架构 - QuestBase.cs

#### 通用任务框架
**功能概述**: 所有任务的抽象基类，定义标准进度追踪模式

**模板方法实现**:
```csharp
public class QuestBase
{
    protected int max_;      // 目标值
    protected int current_;  // 当前进度
    
    // 模板方法 - 刷新进度
    public virtual void Refresh()
    {
        // 子类实现具体的进度计算逻辑
        this.CalculateCurrentProgress();
    }
    
    // 任务完成检查
    public virtual bool IsComplete()
    {
        return this.current_ >= this.max_;
    }
    
    // 获取目标值
    public int GetGoal()
    {
        return this.max_;
    }
    
    // 获取当前进度
    public int GetCurrent()
    {
        return this.current_;
    }
    
    // 获取完成百分比
    public float GetProgressPercentage()
    {
        if (this.max_ <= 0) return 0f;
        return Mathf.Clamp01((float)this.current_ / (float)this.max_);
    }
    
    // 抽象方法，子类必须实现
    protected abstract void CalculateCurrentProgress();
    
    // 虚方法，子类可选重写
    protected virtual void OnQuestComplete()
    {
        Debug.Log($"Quest completed: {this.GetType().Name}");
    }
    
    protected virtual void OnProgressUpdate(int oldProgress, int newProgress)
    {
        if (newProgress > oldProgress)
        {
            Debug.Log($"Quest progress: {newProgress}/{this.max_}");
        }
    }
}
```

**高级任务基类扩展**:
```csharp
public abstract class AdvancedQuestBase : QuestBase
{
    protected DateTime startTime_;
    protected DateTime? completionTime_;
    protected bool hasTimeLimit_;
    protected TimeSpan timeLimit_;
    
    public AdvancedQuestBase(bool hasTimeLimit = false, TimeSpan timeLimit = default)
    {
        this.startTime_ = DateTime.Now;
        this.hasTimeLimit_ = hasTimeLimit;
        this.timeLimit_ = timeLimit;
    }
    
    public override bool IsComplete()
    {
        // 检查时间限制
        if (this.hasTimeLimit_ && !this.IsWithinTimeLimit())
        {
            return false;
        }
        
        bool isComplete = base.IsComplete();
        
        if (isComplete && !this.completionTime_.HasValue)
        {
            this.completionTime_ = DateTime.Now;
            this.OnQuestComplete();
        }
        
        return isComplete;
    }
    
    private bool IsWithinTimeLimit()
    {
        return DateTime.Now - this.startTime_ <= this.timeLimit_;
    }
    
    public TimeSpan GetRemainingTime()
    {
        if (!this.hasTimeLimit_) return TimeSpan.MaxValue;
        
        TimeSpan elapsed = DateTime.Now - this.startTime_;
        return this.timeLimit_ - elapsed;
    }
    
    public bool IsExpired()
    {
        return this.hasTimeLimit_ && !this.IsWithinTimeLimit();
    }
}
```

### 2. 建造者工厂系统 - QuestBuilderManager.cs

#### 类型安全的任务创建
**功能概述**: 单例工厂管理器，通过字符串配置创建类型安全的任务实例

**工厂模式实现**:
```csharp
public class QuestBuilderManager
{
    private static QuestBuilderManager instance_;
    
    // 预注册的任务构建器数组
    private QuestBuilder[] builder_ = new QuestBuilder[]
    {
        new QuestRaceCountBuilder("racecount", 4),      // 比赛次数
        new QuestMedalBuilder("medal", 4),              // 奖牌成就
        new QuestRegistryFlagBuilder("registry", 2),    // 注册标志
        new QuestWinCountBuilder("wincount", 4),        // 胜利次数
        new QuestRaceCompleteBuilder("racecomplete", 4), // 比赛完成
        new QuestDifficultyBuilder("difficulty", 5),    // 难度挑战
        new QuestFriendBuilder("friend", 2),            // 社交好友
        new QuestTrackCountBuilder("trackcount", 2),    // 赛道计数
        new QuestCupCountBuilder("cupcount", 2),        // 杯赛计数
        new QuestMapCupCountBuilder("mapcupcount", 4),  // 地图杯赛
        new QuestMultiMapCupCountBuilder("multimapcupcount", 3) // 多地图杯赛
    };
    
    public static QuestBuilderManager Instance
    {
        get
        {
            if (instance_ == null)
            {
                instance_ = new QuestBuilderManager();
            }
            return instance_;
        }
    }
    
    // 主要的任务构建方法
    public QuestBase Build(string questDefinition)
    {
        if (string.IsNullOrEmpty(questDefinition))
        {
            Debug.LogWarning("Empty quest definition provided");
            return null;
        }
        
        // 解析任务定义字符串
        string[] parameters = questDefinition.Split(new char[] { '_' }, 
                                                   StringSplitOptions.RemoveEmptyEntries);
        
        if (parameters.Length < 2)
        {
            Debug.LogError($"Invalid quest definition format: {questDefinition}");
            return null;
        }
        
        // 查找匹配的构建器
        foreach (QuestBuilder builder in this.builder_)
        {
            if (builder.IsRightFormat(parameters))
            {
                try
                {
                    QuestBase quest = builder.Build(parameters);
                    if (quest != null)
                    {
                        Debug.Log($"Successfully built quest: {quest.GetType().Name}");
                        return quest;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error building quest from '{questDefinition}': {e.Message}");
                }
            }
        }
        
        Debug.LogWarning($"No suitable builder found for quest: {questDefinition}");
        return null;
    }
    
    // 批量构建任务
    public List<QuestBase> BuildMultiple(string[] questDefinitions)
    {
        List<QuestBase> quests = new List<QuestBase>();
        
        foreach (string definition in questDefinitions)
        {
            QuestBase quest = this.Build(definition);
            if (quest != null)
            {
                quests.Add(quest);
            }
        }
        
        return quests;
    }
    
    // 获取支持的任务类型列表
    public string[] GetSupportedQuestTypes()
    {
        List<string> types = new List<string>();
        foreach (QuestBuilder builder in this.builder_)
        {
            types.Add(builder.GetQuestType());
        }
        return types.ToArray();
    }
    
    // 验证任务定义格式
    public bool ValidateQuestDefinition(string questDefinition)
    {
        try
        {
            QuestBase quest = this.Build(questDefinition);
            return quest != null;
        }
        catch
        {
            return false;
        }
    }
}
```

### 3. 抽象建造者基类 - QuestBuilder.cs

#### 统一构建接口
**功能概述**: 所有任务构建器的抽象基类，定义标准的构建和验证流程

**建造者模式实现**:
```csharp
public abstract class QuestBuilder
{
    protected string key_;          // 任务类型标识符
    protected int paramCount_;      // 期望的参数数量
    
    protected QuestBuilder(string key, int paramCount)
    {
        this.key_ = key;
        this.paramCount_ = paramCount;
    }
    
    // 格式验证方法
    public virtual bool IsRightFormat(string[] parameters)
    {
        if (parameters == null || parameters.Length != this.paramCount_)
        {
            return false;
        }
        
        // 检查类型标识符
        if (!string.Equals(parameters[0], this.key_, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        
        // 子类可以重写此方法进行额外验证
        return this.ValidateParameters(parameters);
    }
    
    // 抽象构建方法
    public abstract QuestBase Build(string[] parameters);
    
    // 虚拟参数验证方法
    protected virtual bool ValidateParameters(string[] parameters)
    {
        return true;
    }
    
    // 获取任务类型
    public string GetQuestType()
    {
        return this.key_;
    }
    
    // 获取期望参数数量
    public int GetExpectedParameterCount()
    {
        return this.paramCount_;
    }
    
    // 工具方法 - 解析整数参数
    protected bool TryParseInt(string value, out int result)
    {
        return int.TryParse(value, out result);
    }
    
    // 工具方法 - 解析无符号长整型（位标志）
    protected bool TryParseULong(string value, out ulong result)
    {
        // 支持十六进制格式
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return ulong.TryParse(value.Substring(2), 
                                 NumberStyles.HexNumber, 
                                 CultureInfo.InvariantCulture, 
                                 out result);
        }
        
        // 标准十六进制格式
        return ulong.TryParse(value, NumberStyles.HexNumber, 
                             CultureInfo.InvariantCulture, out result);
    }
    
    // 工具方法 - 解析字节参数
    protected bool TryParseByte(string value, out byte result)
    {
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return byte.TryParse(value.Substring(2), 
                                NumberStyles.HexNumber, 
                                CultureInfo.InvariantCulture, 
                                out result);
        }
        
        return byte.TryParse(value, NumberStyles.HexNumber, 
                            CultureInfo.InvariantCulture, out result);
    }
}
```

## 具体任务类型实现

### 1. 比赛次数任务 - QuestRaceCount.cs

#### 跨赛道和模式的比赛统计
**功能概述**: 基于位标志的高效赛道和模式选择，统计玩家的比赛完成次数

**位标志架构**:
```csharp
public class QuestRaceCount : QuestBase
{
    private ulong trackFlag_;    // 赛道选择位标志
    private byte raceFlag_;      // 比赛模式位标志
    
    public QuestRaceCount(int max, ulong trackFlag, byte raceFlag)
    {
        this.max_ = max;
        this.trackFlag_ = trackFlag;
        this.raceFlag_ = raceFlag;
        this.current_ = 0;
    }
    
    protected override void CalculateCurrentProgress()
    {
        this.current_ = 0;
        
        // 获取统计数据
        Statistics statistics = Statistics.Instance;
        int assetCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        
        // 遍历所有赛道
        for (int trackIndex = 0; trackIndex < assetCount; trackIndex++)
        {
            // 检查赛道是否被选中（位标志检查）
            if (this.IsTrackSelected(trackIndex))
            {
                // 遍历所有比赛模式
                for (int modeIndex = 0; modeIndex < 4; modeIndex++)
                {
                    // 检查比赛模式是否被选中
                    if (this.IsRaceModeSelected(modeIndex))
                    {
                        // 累加比赛完成次数
                        this.current_ += statistics.GetRaceCompleteCount(trackIndex, modeIndex);
                    }
                }
            }
        }
    }
    
    // 高效的位标志检查
    private bool IsTrackSelected(int trackIndex)
    {
        if (trackIndex >= 64) return false; // 位标志限制
        
        ulong trackMask = 1UL << trackIndex;
        return (this.trackFlag_ & trackMask) != 0;
    }
    
    private bool IsRaceModeSelected(int modeIndex)
    {
        if (modeIndex >= 8) return false; // 字节位数限制
        
        byte modeMask = (byte)(1 << modeIndex);
        return (this.raceFlag_ & modeMask) != 0;
    }
    
    // 获取选中的赛道列表
    public List<int> GetSelectedTracks()
    {
        List<int> tracks = new List<int>();
        int assetCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        
        for (int i = 0; i < assetCount && i < 64; i++)
        {
            if (this.IsTrackSelected(i))
            {
                tracks.Add(i);
            }
        }
        
        return tracks;
    }
    
    // 获取选中的比赛模式列表
    public List<int> GetSelectedRaceModes()
    {
        List<int> modes = new List<int>();
        
        for (int i = 0; i < 8; i++)
        {
            if (this.IsRaceModeSelected(i))
            {
                modes.Add(i);
            }
        }
        
        return modes;
    }
}
```

**对应的构建器实现**:
```csharp
public class QuestRaceCountBuilder : QuestBuilder
{
    public QuestRaceCountBuilder() : base("racecount", 4)
    {
    }
    
    protected override bool ValidateParameters(string[] parameters)
    {
        // 验证最大值参数
        if (!this.TryParseInt(parameters[1], out int max) || max <= 0)
        {
            return false;
        }
        
        // 验证赛道标志参数
        if (!this.TryParseULong(parameters[2], out ulong trackFlag))
        {
            return false;
        }
        
        // 验证比赛模式标志参数
        if (!this.TryParseByte(parameters[3], out byte raceFlag))
        {
            return false;
        }
        
        return true;
    }
    
    public override QuestBase Build(string[] parameters)
    {
        int max = int.Parse(parameters[1]);
        ulong trackFlag = ulong.Parse(parameters[2], NumberStyles.HexNumber);
        byte raceFlag = byte.Parse(parameters[3], NumberStyles.HexNumber);
        
        return new QuestRaceCount(max, trackFlag, raceFlag);
    }
}
```

### 2. 奖牌成就任务 - QuestMedal.cs

#### 复杂的成就验证系统
**功能概述**: 基于最佳成绩和时间限制的奖牌成就追踪

**奖牌类型和验证**:
```csharp
public enum MedalType : byte
{
    BRONZE = 0,   // 铜牌
    SILVER = 1,   // 银牌
    GOLD = 2,     // 金牌
    COMPLETE = 3  // 完成奖牌
}

public class QuestMedal : QuestBase
{
    private ulong trackFlag_;
    private byte raceFlag_;
    private MedalType medalType_;
    
    public QuestMedal(int max, ulong trackFlag, byte raceFlag, MedalType medalType)
    {
        this.max_ = max;
        this.trackFlag_ = trackFlag;
        this.raceFlag_ = raceFlag;
        this.medalType_ = medalType;
    }
    
    protected override void CalculateCurrentProgress()
    {
        this.current_ = 0;
        
        int assetCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        
        for (int trackIndex = 0; trackIndex < assetCount; trackIndex++)
        {
            if (this.IsTrackSelected(trackIndex))
            {
                // 获取赛道资产定义
                TrackAssetDefinition trackDef = TrackAssetDefinitionManager.Instance
                    .GetAssetDefinition(trackIndex);
                
                for (int modeIndex = 0; modeIndex < 4; modeIndex++)
                {
                    if (this.IsRaceModeSelected(modeIndex))
                    {
                        if (this.CheckMedalRequirement(trackIndex, modeIndex, trackDef))
                        {
                            this.current_++;
                        }
                    }
                }
            }
        }
    }
    
    private bool CheckMedalRequirement(int trackIndex, int modeIndex, 
                                      TrackAssetDefinition trackDef)
    {
        // 获取玩家最佳成绩
        GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(
            (byte)trackIndex, modeIndex);
        
        if (bestInfo == null)
        {
            return false; // 没有完成过此赛道
        }
        
        // 完成奖牌只需要有记录即可
        if (this.medalType_ == MedalType.COMPLETE)
        {
            return true;
        }
        
        // 检查时间是否达到奖牌要求
        if (trackDef?.MedalTime == null)
        {
            return false; // 没有设置奖牌时间
        }
        
        // 计算奖牌时间索引：[模式 * 3 + 奖牌类型]
        int medalTimeIndex = modeIndex * 3 + (int)this.medalType_;
        
        if (medalTimeIndex >= trackDef.MedalTime.Length)
        {
            return false;
        }
        
        float requiredTime = trackDef.MedalTime[medalTimeIndex];
        return bestInfo.finishTime_ <= requiredTime;
    }
    
    // 获取特定赛道和模式的奖牌状态
    public MedalType GetMedalStatus(int trackIndex, int modeIndex)
    {
        TrackAssetDefinition trackDef = TrackAssetDefinitionManager.Instance
            .GetAssetDefinition(trackIndex);
        GhostFilenameInfo bestInfo = KartOptions.Instance.GetBestInfo(
            (byte)trackIndex, modeIndex);
        
        if (bestInfo == null)
        {
            return MedalType.COMPLETE; // 没有完成
        }
        
        if (trackDef?.MedalTime == null)
        {
            return MedalType.COMPLETE; // 只有完成奖牌
        }
        
        // 从金牌开始检查，向下降级
        for (int medal = (int)MedalType.GOLD; medal >= (int)MedalType.BRONZE; medal--)
        {
            int timeIndex = modeIndex * 3 + medal;
            if (timeIndex < trackDef.MedalTime.Length)
            {
                if (bestInfo.finishTime_ <= trackDef.MedalTime[timeIndex])
                {
                    return (MedalType)medal;
                }
            }
        }
        
        return MedalType.COMPLETE; // 完成但未达到任何奖牌时间
    }
}
```

### 3. 难度挑战任务 - QuestDifficulty.cs

#### 复杂条件逻辑系统
**功能概述**: 支持AND/OR逻辑的复杂难度挑战验证

**逻辑操作符实现**:
```csharp
public enum enOperator : byte
{
    AND = 0,  // 所有条件都必须满足
    OR = 1    // 任一条件满足即可
}

public class QuestDifficulty : QuestBase
{
    private ulong trackFlag_;
    private byte raceFlag_;
    private enOperator operator_;
    private int minWins_;
    
    public QuestDifficulty(ulong trackFlag, byte raceFlag, enOperator op, int minWins)
    {
        this.trackFlag_ = trackFlag;
        this.raceFlag_ = raceFlag;
        this.operator_ = op;
        this.minWins_ = minWins;
        this.max_ = 0; // 动态计算
    }
    
    protected override void CalculateCurrentProgress()
    {
        this.current_ = 0;
        this.max_ = 0; // 重新计算目标
        
        int assetCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        
        for (int trackIndex = 0; trackIndex < assetCount; trackIndex++)
        {
            if (this.IsTrackSelected(trackIndex))
            {
                if (this.operator_ == enOperator.OR)
                {
                    this.ProcessORLogic(trackIndex);
                }
                else
                {
                    this.ProcessANDLogic(trackIndex);
                }
            }
        }
    }
    
    private void ProcessORLogic(int trackIndex)
    {
        // OR逻辑：每个赛道只要有任一模式达到胜利要求即可
        this.max_++;
        
        bool trackCompleted = false;
        
        for (int modeIndex = 0; modeIndex < 4; modeIndex++)
        {
            if (this.IsRaceModeSelected(modeIndex))
            {
                int wins = Statistics.Instance.GetWinCount(trackIndex, modeIndex);
                if (wins >= this.minWins_)
                {
                    trackCompleted = true;
                    break; // 只要有一个模式满足即可
                }
            }
        }
        
        if (trackCompleted)
        {
            this.current_++;
        }
    }
    
    private void ProcessANDLogic(int trackIndex)
    {
        // AND逻辑：每个赛道的所有选中模式都必须达到胜利要求
        List<int> selectedModes = this.GetSelectedRaceModes(trackIndex);
        
        this.max_ += selectedModes.Count;
        
        foreach (int modeIndex in selectedModes)
        {
            int wins = Statistics.Instance.GetWinCount(trackIndex, modeIndex);
            if (wins >= this.minWins_)
            {
                this.current_++;
            }
        }
    }
    
    private List<int> GetSelectedRaceModes(int trackIndex)
    {
        List<int> modes = new List<int>();
        
        for (int i = 0; i < 4; i++)
        {
            if (this.IsRaceModeSelected(i))
            {
                modes.Add(i);
            }
        }
        
        return modes;
    }
    
    // 获取详细进度信息
    public Dictionary<int, Dictionary<int, int>> GetDetailedProgress()
    {
        var progress = new Dictionary<int, Dictionary<int, int>>();
        int assetCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        
        for (int trackIndex = 0; trackIndex < assetCount; trackIndex++)
        {
            if (this.IsTrackSelected(trackIndex))
            {
                progress[trackIndex] = new Dictionary<int, int>();
                
                for (int modeIndex = 0; modeIndex < 4; modeIndex++)
                {
                    if (this.IsRaceModeSelected(modeIndex))
                    {
                        int wins = Statistics.Instance.GetWinCount(trackIndex, modeIndex);
                        progress[trackIndex][modeIndex] = wins;
                    }
                }
            }
        }
        
        return progress;
    }
}
```

### 4. 社交集成任务 - QuestFriend.cs

#### Facebook集成的好友系统
**功能概述**: 集成Facebook API的社交好友数量追踪

**社交平台适配**:
```csharp
public class QuestFriend : QuestBase
{
    public QuestFriend(int max)
    {
        this.max_ = max;
        this.current_ = 0;
    }
    
    protected override void CalculateCurrentProgress()
    {
        // 检查是否有特殊的好友任务标志
        if (KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.FACEBOOK_FRIEND))
        {
            this.current_ = this.max_; // 直接完成任务
            return;
        }
        
        // 根据平台获取Facebook实例
        Facebook facebook = this.GetPlatformFacebook();
        
        if (!facebook.LoggedIn)
        {
            this.current_ = 0;
            return;
        }
        
        // 获取好友数量（排除自己）
        int friendCount = facebook.FriendDict.Count - 1;
        this.current_ = Mathf.Clamp(friendCount, 0, this.max_);
    }
    
    private Facebook GetPlatformFacebook()
    {
        // 根据运行平台选择适当的Facebook实现
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
            case RuntimePlatform.Android:
                return Facebook.Inst; // 真实Facebook API
                
            default:
                return MockFacebook.Inst; // 模拟Facebook（用于编辑器测试）
        }
    }
    
    // 获取好友信息详情
    public List<FacebookFriendInfo> GetFriendsList()
    {
        Facebook facebook = this.GetPlatformFacebook();
        List<FacebookFriendInfo> friends = new List<FacebookFriendInfo>();
        
        if (facebook.LoggedIn && facebook.FriendDict != null)
        {
            foreach (var friendPair in facebook.FriendDict)
            {
                if (friendPair.Key != facebook.UserId) // 排除自己
                {
                    friends.Add(new FacebookFriendInfo
                    {
                        Id = friendPair.Key,
                        Name = friendPair.Value.Name,
                        ProfilePictureUrl = friendPair.Value.ProfilePictureUrl
                    });
                }
            }
        }
        
        return friends;
    }
    
    // 尝试邀请更多好友
    public void InviteMoreFriends()
    {
        Facebook facebook = this.GetPlatformFacebook();
        
        if (facebook.LoggedIn)
        {
            facebook.ShowRequestDialog("邀请好友一起玩卡丁车游戏！", 
                                      new List<string>(), // 空列表表示向所有好友发送
                                      null, // 可选的过滤器
                                      this.OnInviteResult);
        }
    }
    
    private void OnInviteResult(string result)
    {
        Debug.Log($"Friend invite result: {result}");
        
        // 重新刷新好友数量
        this.Refresh();
    }
}

public class FacebookFriendInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ProfilePictureUrl { get; set; }
}
```

### 5. 注册标志任务 - QuestRegistryFlag.cs

#### 游戏进度标志追踪
**功能概述**: 追踪玩家完成的教程、功能解锁等里程碑事件

**标志系统实现**:
```csharp
public class QuestRegistryFlag : QuestBase
{
    private KartOptions.QuestFlag targetFlag_;
    
    public QuestRegistryFlag(int max, KartOptions.QuestFlag flag)
    {
        this.max_ = max;
        this.targetFlag_ = flag;
        this.current_ = 0;
    }
    
    protected override void CalculateCurrentProgress()
    {
        // 检查特定标志是否已设置
        bool flagIsSet = KartOptions.Instance.IsQuestFlagOn(this.targetFlag_);
        this.current_ = flagIsSet ? this.max_ : 0;
    }
    
    // 手动设置标志
    public void SetFlag()
    {
        KartOptions.Instance.SetQuestFlag(this.targetFlag_, true);
        this.Refresh();
        
        if (this.IsComplete())
        {
            this.OnQuestComplete();
        }
    }
    
    // 清除标志
    public void ClearFlag()
    {
        KartOptions.Instance.SetQuestFlag(this.targetFlag_, false);
        this.Refresh();
    }
    
    // 获取标志描述
    public string GetFlagDescription()
    {
        return this.targetFlag_ switch
        {
            KartOptions.QuestFlag.FACEBOOK_LOGIN => "Facebook登录",
            KartOptions.QuestFlag.FACEBOOK_PUBLISHING => "Facebook分享",
            KartOptions.QuestFlag.FACEBOOK_FRIEND => "Facebook好友",
            KartOptions.QuestFlag.CHANCHAN_BOOSTER => "使用加速道具",
            KartOptions.QuestFlag.TUTORIAL => "完成教程",
            KartOptions.QuestFlag.TUTORIAL2 => "完成高级教程",
            KartOptions.QuestFlag.TUTORIAL_MULTI => "完成多人教程",
            KartOptions.QuestFlag.TOUCH_CONTROLLER => "使用触摸控制",
            KartOptions.QuestFlag.ACCELEROMETER_CONTROLLER => "使用重力控制",
            KartOptions.QuestFlag.GAMEPAD_CONTROLLER => "使用手柄控制",
            _ => "未知标志"
        };
    }
    
    // 获取相关的成就奖励
    public QuestReward GetCompletionReward()
    {
        return this.targetFlag_ switch
        {
            KartOptions.QuestFlag.TUTORIAL => new QuestReward 
            { 
                Coins = 100, 
                Description = "完成基础教程奖励" 
            },
            KartOptions.QuestFlag.FACEBOOK_LOGIN => new QuestReward 
            { 
                Coins = 200, 
                Description = "Facebook登录奖励" 
            },
            KartOptions.QuestFlag.FACEBOOK_FRIEND => new QuestReward 
            { 
                Coins = 500, 
                Description = "社交好友奖励" 
            },
            _ => new QuestReward { Coins = 50, Description = "基础完成奖励" }
        };
    }
}

public class QuestReward
{
    public int Coins { get; set; }
    public string Description { get; set; }
    public List<string> UnlockedItems { get; set; } = new List<string>();
    public List<string> UnlockedTracks { get; set; } = new List<string>();
}
```

## 数据集成和持久化

### 1. 统计系统集成 - Statistics.cs

#### 实时数据收集
**功能概述**: 与游戏核心统计系统的无缝集成

```csharp
public class Statistics
{
    private static Statistics instance_;
    
    // 二维数组存储：[赛道索引][比赛模式索引] = 计数
    private int[][] raceCompleteCounter_;  // 比赛完成次数
    private int[][] winCounter_;           // 胜利次数
    
    public static Statistics Instance
    {
        get
        {
            if (instance_ == null)
            {
                instance_ = new Statistics();
                instance_.Initialize();
            }
            return instance_;
        }
    }
    
    // 初始化统计数据
    private void Initialize()
    {
        int trackCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        
        this.raceCompleteCounter_ = new int[trackCount][];
        this.winCounter_ = new int[trackCount][];
        
        for (int i = 0; i < trackCount; i++)
        {
            this.raceCompleteCounter_[i] = new int[4]; // 4种比赛模式
            this.winCounter_[i] = new int[4];
            
            this.LoadTrackStatistics(i);
        }
    }
    
    // 从持久化存储加载数据
    private void LoadTrackStatistics(int trackIndex)
    {
        for (int modeIndex = 0; modeIndex < 4; modeIndex++)
        {
            // 从KartOptions加载持久化数据
            this.raceCompleteCounter_[trackIndex][modeIndex] = 
                KartOptions.Instance.GetRaceCompleteCount(trackIndex, modeIndex);
            
            this.winCounter_[trackIndex][modeIndex] = 
                KartOptions.Instance.GetWinCount(trackIndex, modeIndex);
        }
    }
    
    // 比赛完成后更新统计
    public void RaceComplete(int trackIndex, int modeIndex, bool isWinner)
    {
        if (!this.IsValidIndices(trackIndex, modeIndex))
        {
            Debug.LogWarning($"Invalid track/mode indices: {trackIndex}/{modeIndex}");
            return;
        }
        
        // 更新比赛完成计数
        this.raceCompleteCounter_[trackIndex][modeIndex]++;
        
        // 更新胜利计数
        if (isWinner)
        {
            this.winCounter_[trackIndex][modeIndex]++;
        }
        
        // 保存到持久化存储
        this.SaveTrackStatistics(trackIndex, modeIndex);
        
        // 通知任务系统刷新
        QuestNotificationCenter.Instance.NotifyStatisticsUpdate(trackIndex, modeIndex);
    }
    
    // 保存统计数据
    private void SaveTrackStatistics(int trackIndex, int modeIndex)
    {
        KartOptions.Instance.SetRaceCompleteCount(trackIndex, modeIndex, 
            this.raceCompleteCounter_[trackIndex][modeIndex]);
        
        KartOptions.Instance.SetWinCount(trackIndex, modeIndex, 
            this.winCounter_[trackIndex][modeIndex]);
    }
    
    // 获取比赛完成次数
    public int GetRaceCompleteCount(int trackIndex, int modeIndex)
    {
        if (!this.IsValidIndices(trackIndex, modeIndex))
            return 0;
        
        return this.raceCompleteCounter_[trackIndex][modeIndex];
    }
    
    // 获取胜利次数
    public int GetWinCount(int trackIndex, int modeIndex)
    {
        if (!this.IsValidIndices(trackIndex, modeIndex))
            return 0;
        
        return this.winCounter_[trackIndex][modeIndex];
    }
    
    // 获取胜率
    public float GetWinRate(int trackIndex, int modeIndex)
    {
        int races = this.GetRaceCompleteCount(trackIndex, modeIndex);
        if (races == 0) return 0f;
        
        int wins = this.GetWinCount(trackIndex, modeIndex);
        return (float)wins / (float)races;
    }
    
    // 获取总统计信息
    public GlobalStatistics GetGlobalStatistics()
    {
        var stats = new GlobalStatistics();
        
        for (int i = 0; i < this.raceCompleteCounter_.Length; i++)
        {
            for (int j = 0; j < this.raceCompleteCounter_[i].Length; j++)
            {
                stats.TotalRaces += this.raceCompleteCounter_[i][j];
                stats.TotalWins += this.winCounter_[i][j];
            }
        }
        
        stats.OverallWinRate = stats.TotalRaces > 0 ? 
            (float)stats.TotalWins / (float)stats.TotalRaces : 0f;
        
        return stats;
    }
    
    private bool IsValidIndices(int trackIndex, int modeIndex)
    {
        return trackIndex >= 0 && trackIndex < this.raceCompleteCounter_.Length &&
               modeIndex >= 0 && modeIndex < 4;
    }
}

public class GlobalStatistics
{
    public int TotalRaces { get; set; }
    public int TotalWins { get; set; }
    public float OverallWinRate { get; set; }
    
    public Dictionary<int, int> FavoriteTrack { get; set; } = new Dictionary<int, int>();
    public Dictionary<int, int> FavoriteMode { get; set; } = new Dictionary<int, int>();
}
```

## 性能优化策略

### 1. 位操作优化
```csharp
public static class BitFlagOptimizations
{
    // 高效的多位检查
    public static bool AnyBitSet(ulong flags, ulong mask)
    {
        return (flags & mask) != 0;
    }
    
    public static bool AllBitsSet(ulong flags, ulong mask)
    {
        return (flags & mask) == mask;
    }
    
    // 计算设置的位数
    public static int CountSetBits(ulong value)
    {
        int count = 0;
        while (value != 0)
        {
            count += (int)(value & 1);
            value >>= 1;
        }
        return count;
    }
    
    // 获取设置的位索引列表
    public static List<int> GetSetBitIndices(ulong flags)
    {
        List<int> indices = new List<int>();
        
        for (int i = 0; i < 64; i++)
        {
            if ((flags & (1UL << i)) != 0)
            {
                indices.Add(i);
            }
        }
        
        return indices;
    }
}
```

### 2. 缓存和惰性求值
```csharp
public class OptimizedQuestManager
{
    private Dictionary<string, QuestBase> questCache_ = new Dictionary<string, QuestBase>();
    private Dictionary<QuestBase, DateTime> lastRefreshTime_ = new Dictionary<QuestBase, DateTime>();
    private TimeSpan cacheValidDuration_ = TimeSpan.FromMinutes(1);
    
    public QuestBase GetOrCreateQuest(string definition)
    {
        if (this.questCache_.TryGetValue(definition, out QuestBase cachedQuest))
        {
            // 检查缓存是否仍然有效
            if (this.IsCacheValid(cachedQuest))
            {
                return cachedQuest;
            }
        }
        
        // 创建新任务并加入缓存
        QuestBase quest = QuestBuilderManager.Instance.Build(definition);
        if (quest != null)
        {
            this.questCache_[definition] = quest;
            this.lastRefreshTime_[quest] = DateTime.Now;
        }
        
        return quest;
    }
    
    private bool IsCacheValid(QuestBase quest)
    {
        if (!this.lastRefreshTime_.TryGetValue(quest, out DateTime lastRefresh))
        {
            return false;
        }
        
        return DateTime.Now - lastRefresh < this.cacheValidDuration_;
    }
    
    public void RefreshQuest(QuestBase quest)
    {
        if (quest != null)
        {
            quest.Refresh();
            this.lastRefreshTime_[quest] = DateTime.Now;
        }
    }
    
    public void InvalidateCache()
    {
        this.questCache_.Clear();
        this.lastRefreshTime_.Clear();
    }
}
```

## 总结

Quest系统展现了卓越的软件工程实践，具有以下核心优势：

### 技术优势
1. **多重设计模式**: 工厂、建造者、策略、模板方法的完美融合
2. **位操作优化**: 高效的内存使用和快速的多条件检查
3. **类型安全构建**: 字符串配置与编译时类型检查的平衡
4. **惰性求值**: 按需计算和智能缓存策略
5. **平台适配**: 跨平台社交集成的优雅抽象

### 架构特点
- **可扩展性**: 新任务类型的添加只需实现对应的Builder/Quest类
- **可维护性**: 清晰的职责分离和统一的接口设计
- **高性能**: 位操作、缓存和增量更新的性能优化
- **灵活配置**: 字符串定义系统支持运行时任务配置
- **数据集成**: 与游戏统计和持久化系统的深度集成

该任务系统为卡丁车游戏提供了专业级的成就管理框架，支持复杂的多条件验证、高效的数据处理和灵活的扩展机制，确保了优秀的用户体验和系统性能。