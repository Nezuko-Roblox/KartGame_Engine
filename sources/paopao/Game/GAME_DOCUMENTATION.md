# Game 文件夹完整功能文档

## 概述

Game 文件夹包含了卡丁车游戏的核心游戏逻辑系统，实现了双重任务管理架构和智能道具分配机制。该系统通过策略模式、单例模式和模板方法模式，为游戏提供了实时效果管理、比赛事件调度、基于排名的道具分配和玩家状态管理等核心功能。

## 系统架构

### 核心设计原则
- **双重任务系统**: 分离实时效果和离散事件管理
- **基于排名的平衡**: 智能的道具分配机制
- **模板方法模式**: 统一的任务生命周期管理
- **策略模式**: 不同类型任务的灵活实现
- **事件驱动**: 消息系统的松耦合通信

### 系统组件分层
- **任务管理层**: 玩家效果和比赛事件的调度系统
- **道具管理层**: 智能道具生成和分配逻辑
- **游戏状态层**: 比赛状态和事件处理
- **数据管理层**: 道具元素和任务队列管理

## 核心任务管理系统

### 1. 玩家任务管理器 - PlayerJobManager.cs

#### 实时效果管理
**功能概述**: 管理临时效果和卡丁车属性修改的任务执行器

**任务管理架构**:
```csharp
public class PlayerJobManager
{
    private List<PlayerJob> jobList_;
    
    public PlayerJobManager()
    {
        this.jobList_ = new List<PlayerJob>();
    }
    
    // 添加新任务
    public void AddJob(PlayerJob job)
    {
        if (job != null)
        {
            job.Initialize();
            this.jobList_.Add(job);
        }
    }
    
    // 主更新循环
    public void Update()
    {
        // 更新所有活跃任务
        foreach (PlayerJob job in this.jobList_)
        {
            job.Update();
        }
        
        // 移除已完成的任务
        this.jobList_.RemoveAll(job => job.IsJobFinish());
    }
    
    // 清理所有任务
    public void ClearAllJobs()
    {
        this.jobList_.Clear();
    }
    
    // 获取当前任务数量
    public int GetActiveJobCount()
    {
        return this.jobList_.Count;
    }
    
    // 检查特定类型任务是否存在
    public bool HasJobOfType<T>() where T : PlayerJob
    {
        return this.jobList_.Exists(job => job is T);
    }
}
```

**高级任务管理功能**:
```csharp
public class AdvancedPlayerJobManager : PlayerJobManager
{
    private Dictionary<Type, List<PlayerJob>> jobsByType_;
    private float lastUpdateTime_;
    
    public AdvancedPlayerJobManager()
    {
        this.jobsByType_ = new Dictionary<Type, List<PlayerJob>>();
    }
    
    public override void AddJob(PlayerJob job)
    {
        base.AddJob(job);
        
        // 按类型分类管理
        Type jobType = job.GetType();
        if (!this.jobsByType_.ContainsKey(jobType))
        {
            this.jobsByType_[jobType] = new List<PlayerJob>();
        }
        this.jobsByType_[jobType].Add(job);
    }
    
    // 优先级任务处理
    public void UpdateWithPriority()
    {
        // 高优先级任务先执行
        var priorityJobs = this.jobList_.OrderByDescending(job => job.GetPriority()).ToList();
        
        foreach (PlayerJob job in priorityJobs)
        {
            if (!job.IsJobFinish())
            {
                job.Update();
            }
        }
        
        this.CleanupFinishedJobs();
    }
    
    // 取消特定类型的任务
    public void CancelJobsOfType<T>() where T : PlayerJob
    {
        this.jobList_.RemoveAll(job => job is T);
        
        Type jobType = typeof(T);
        if (this.jobsByType_.ContainsKey(jobType))
        {
            this.jobsByType_[jobType].Clear();
        }
    }
}
```

### 2. 抽象玩家任务基类 - PlayerJob.cs

#### 统一任务生命周期
**功能概述**: 所有玩家效果任务的抽象基类，定义标准生命周期

**模板方法模式实现**:
```csharp
public abstract class PlayerJob
{
    protected bool isInitialized_;
    protected bool isFinished_;
    protected float startTime_;
    protected int priority_;
    
    // 模板方法 - 任务生命周期
    public void Initialize()
    {
        if (this.isInitialized_) return;
        
        this.startTime_ = Time.time;
        this.isInitialized_ = true;
        this.OnInitialize();
    }
    
    public void Update()
    {
        if (!this.isInitialized_ || this.isFinished_) return;
        
        this.OnUpdate();
        
        if (this.ShouldFinish())
        {
            this.OnFinish();
            this.isFinished_ = true;
        }
    }
    
    public bool IsJobFinish()
    {
        return this.isFinished_;
    }
    
    // 抽象方法，子类实现
    protected abstract void OnInitialize();
    protected abstract void OnUpdate();
    protected abstract bool ShouldFinish();
    
    // 虚方法，子类可选重写
    protected virtual void OnFinish() { }
    protected virtual int GetPriority() { return this.priority_; }
    
    // 工具方法
    protected float GetElapsedTime()
    {
        return Time.time - this.startTime_;
    }
    
    protected bool HasTimeElapsed(float duration)
    {
        return this.GetElapsedTime() >= duration;
    }
}
```

### 3. UFO攻击任务实现 - JobApplyUFO.cs

#### 临时物理效果任务
**功能概述**: UFO道具的临时拖拽效果实现，展示物理属性修改模式

**物理修改任务实现**:
```csharp
public class JobApplyUFO : PlayerJob
{
    private GoKart targetKart_;
    private float originalDragFactor_;
    private const float UFO_EFFECT_DURATION = 3f;
    private const float UFO_DRAG_MULTIPLIER = 4f;
    
    public JobApplyUFO(GoKart kart)
    {
        this.targetKart_ = kart;
        this.priority_ = 10; // 高优先级
    }
    
    protected override void OnInitialize()
    {
        if (this.targetKart_ == null)
        {
            this.isFinished_ = true;
            return;
        }
        
        // 保存原始拖拽系数
        this.originalDragFactor_ = this.targetKart_.GetDragFactor();
        
        // 应用UFO效果 - 增加拖拽力
        float newDragFactor = this.originalDragFactor_ * UFO_DRAG_MULTIPLIER;
        this.targetKart_.SetDragFactor(newDragFactor);
        
        // 播放UFO效果音效和视觉
        this.PlayUFOEffect();
        
        Debug.Log($"UFO effect applied to kart {this.targetKart_.GetKartIndex()}: drag {this.originalDragFactor_:F2} → {newDragFactor:F2}");
    }
    
    protected override void OnUpdate()
    {
        if (this.targetKart_ == null)
        {
            this.isFinished_ = true;
            return;
        }
        
        // 根据剩余时间调整效果强度
        float remainingTime = UFO_EFFECT_DURATION - this.GetElapsedTime();
        float effectStrength = remainingTime / UFO_EFFECT_DURATION;
        
        // 渐进式减弱效果
        float currentDrag = Mathf.Lerp(this.originalDragFactor_, 
                                      this.originalDragFactor_ * UFO_DRAG_MULTIPLIER, 
                                      effectStrength);
        this.targetKart_.SetDragFactor(currentDrag);
        
        // 更新视觉效果强度
        this.UpdateUFOVisualEffect(effectStrength);
    }
    
    protected override bool ShouldFinish()
    {
        return this.HasTimeElapsed(UFO_EFFECT_DURATION) || this.targetKart_ == null;
    }
    
    protected override void OnFinish()
    {
        if (this.targetKart_ != null)
        {
            // 恢复原始拖拽系数
            this.targetKart_.SetDragFactor(this.originalDragFactor_);
            
            // 停止UFO效果
            this.StopUFOEffect();
            
            Debug.Log($"UFO effect removed from kart {this.targetKart_.GetKartIndex()}: drag restored to {this.originalDragFactor_:F2}");
        }
    }
    
    private void PlayUFOEffect()
    {
        // 播放UFO攻击音效
        AudioSource ufoAudio = this.targetKart_.GetComponent<AudioSource>();
        if (ufoAudio != null)
        {
            ufoAudio.PlayOneShot(ResourceLoader.LoadAudioClip("ufo_attack"));
        }
        
        // 创建UFO视觉效果
        GameObject ufoEffect = ResourceLoader.LoadPrefab("UFO_Effect");
        if (ufoEffect != null)
        {
            ufoEffect.transform.SetParent(this.targetKart_.transform);
            ufoEffect.transform.localPosition = Vector3.up * 2f;
        }
    }
    
    private void UpdateUFOVisualEffect(float intensity)
    {
        // 更新粒子效果强度
        ParticleSystem[] particles = this.targetKart_.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particles)
        {
            if (ps.name.Contains("UFO"))
            {
                var emission = ps.emission;
                emission.rateOverTime = 50f * intensity;
            }
        }
    }
    
    private void StopUFOEffect()
    {
        // 移除UFO效果对象
        Transform ufoEffect = this.targetKart_.transform.Find("UFO_Effect");
        if (ufoEffect != null)
        {
            Destroy(ufoEffect.gameObject);
        }
    }
}
```

### 4. 比赛任务队列系统 - KartJobQueue.cs

#### 全局事件调度器
**功能概述**: 管理比赛事件序列的单例队列系统

**队列管理架构**:
```csharp
public class KartJobQueue
{
    private static KartJobQueue instance_;
    private LinkedList<KartJob> jobQueue_;
    private bool isProcessing_;
    
    public static KartJobQueue Instance
    {
        get
        {
            if (instance_ == null)
            {
                instance_ = new KartJobQueue();
            }
            return instance_;
        }
    }
    
    private KartJobQueue()
    {
        this.jobQueue_ = new LinkedList<KartJob>();
        this.isProcessing_ = false;
    }
    
    // 添加任务到队列
    public void AddJob(KartJob job)
    {
        if (job != null)
        {
            this.jobQueue_.AddLast(job);
        }
    }
    
    // 处理下一个任务
    public void ProcessNextJob()
    {
        if (this.isProcessing_ || this.jobQueue_.Count == 0)
            return;
        
        this.isProcessing_ = true;
        
        KartJob currentJob = this.jobQueue_.First.Value;
        this.jobQueue_.RemoveFirst();
        
        this.ExecuteJob(currentJob);
    }
    
    private void ExecuteJob(KartJob job)
    {
        try
        {
            switch (job.type)
            {
                case KartJobType.RACING_START:
                    this.HandleRacingStart(job);
                    break;
                    
                case KartJobType.RACING_OVER:
                    this.HandleRacingOver(job);
                    break;
                    
                case KartJobType.TIME_OVER:
                    this.HandleTimeOver(job);
                    break;
                    
                case KartJobType.FINISH_NOTICES:
                    this.HandleFinishNotices(job);
                    break;
                    
                default:
                    Debug.LogWarning($"Unknown job type: {job.type}");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error executing job {job.type}: {e.Message}");
        }
        finally
        {
            this.isProcessing_ = false;
            
            // 继续处理队列中的下一个任务
            if (this.jobQueue_.Count > 0)
            {
                this.ProcessNextJob();
            }
        }
    }
}
```

**比赛事件处理**:
```csharp
private void HandleRacingStart(KartJob job)
{
    // 开始比赛倒计时
    JobRacingStart racingStart = new JobRacingStart();
    racingStart.Execute();
    
    // 通知所有卡丁车准备开始
    KartManager.Instance.NotifyRaceStart();
    
    // 启动比赛计时器
    RaceTimer.Instance.StartRace();
    
    Debug.Log("Race started!");
}

private void HandleRacingOver(KartJob job)
{
    // 结束比赛
    JobRacingOver racingOver = new JobRacingOver();
    racingOver.Execute();
    
    // 计算最终排名
    var finalRankings = KartManager.Instance.CalculateFinalRankings();
    
    // 显示比赛结果
    GameResultManager.Instance.ShowResults(finalRankings);
    
    // 添加完成通知任务
    KartJob finishNotice = new KartJob(KartJobType.FINISH_NOTICES);
    this.AddJob(finishNotice);
    
    Debug.Log("Race finished!");
}

private void HandleTimeOver(KartJob job)
{
    // 时间耗尽处理
    JobTimeOver timeOver = new JobTimeOver();
    timeOver.Execute();
    
    // 强制结束比赛
    KartManager.Instance.ForceRaceEnd();
    
    // 显示超时消息
    UIManager.Instance.ShowTimeOverMessage();
    
    Debug.Log("Race time over!");
}

private void HandleFinishNotices(KartJob job)
{
    // 发送完成通知
    JobFinishNotices finishNotices = new JobFinishNotices();
    finishNotices.Execute();
    
    // 清理比赛资源
    this.CleanupRaceResources();
    
    Debug.Log("Finish notices sent!");
}
```

## 智能道具管理系统

### 1. 游戏道具管理器 - GameItemManager.cs

#### 基于排名的道具分配
**功能概述**: 单例道具管理器，实现智能的排名平衡道具分配算法

**概率矩阵系统**:
```csharp
public class GameItemManager
{
    private static GameItemManager instance_;
    
    // 道具概率矩阵 [玩家数量-2][道具类型][排名]
    private float[,,] ITEM_PROBABILITY = new float[5, 10, 6]
    {
        // 2玩家模式
        {
            // BOOSTER概率: [1st, 2nd, 3rd, 4th, 5th, 6th]
            { 0.1f, 0.3f, 0.0f, 0.0f, 0.0f, 0.0f },
            // BANANA概率
            { 0.3f, 0.2f, 0.0f, 0.0f, 0.0f, 0.0f },
            // UFO概率
            { 0.2f, 0.4f, 0.0f, 0.0f, 0.0f, 0.0f },
            // 其他道具...
        },
        // 3-6玩家模式的概率矩阵...
    };
    
    public static GameItemManager Instance
    {
        get
        {
            if (instance_ == null)
            {
                instance_ = new GameItemManager();
            }
            return instance_;
        }
    }
}
```

**智能道具生成算法**:
```csharp
public GameItem GenerateItem(int kartIndex)
{
    // 验证比赛状态
    if (!this.IsValidRaceState(kartIndex))
    {
        return GameItem.BOOSTER; // 默认道具
    }
    
    // 获取卡丁车排名和玩家数量
    int rank = KartManager.Instance.goCourse_.GetRank(kartIndex);
    int playerCount = KartManager.Instance.GetGoKartCount();
    
    // 边界检查
    if (playerCount < 2 || playerCount > 6 || rank < 1 || rank > 6)
    {
        return this.GetRandomItem();
    }
    
    // 基于概率矩阵选择道具
    return this.SelectItemByProbability(playerCount, rank);
}

private GameItem SelectItemByProbability(int playerCount, int rank)
{
    int playerIndex = playerCount - 2; // 转换为数组索引
    int rankIndex = rank - 1;          // 转换为数组索引
    
    float randomValue = Random.value;
    float cumulativeProbability = 0f;
    
    // 遍历所有道具类型
    for (int itemType = 0; itemType < 10; itemType++)
    {
        cumulativeProbability += this.ITEM_PROBABILITY[playerIndex, itemType, rankIndex];
        
        if (randomValue <= cumulativeProbability)
        {
            return (GameItem)itemType;
        }
    }
    
    // 降级处理
    return GameItem.BOOSTER;
}

private bool IsValidRaceState(int kartIndex)
{
    // 检查比赛是否活跃
    if (KartManager.Instance.goCourse_ == null)
        return false;
    
    // 检查卡丁车是否已完成比赛
    if (KartManager.Instance.goCourse_.IsGoal(kartIndex))
        return false;
    
    // 检查比赛时间是否有效
    if (RaceTimer.Instance.IsTimeUp())
        return false;
    
    return true;
}
```

**高级道具分配策略**:
```csharp
public class AdvancedGameItemManager : GameItemManager
{
    [Header("平衡配置")]
    public float rubberBandStrength = 1.5f;    // 橡皮筋效应强度
    public bool enableDynamicBalancing = true; // 动态平衡
    public float leadAdvantageThreshold = 10f; // 领先优势阈值
    
    public override GameItem GenerateItem(int kartIndex)
    {
        if (this.enableDynamicBalancing)
        {
            return this.GenerateBalancedItem(kartIndex);
        }
        
        return base.GenerateItem(kartIndex);
    }
    
    private GameItem GenerateBalancedItem(int kartIndex)
    {
        GoKart kart = KartManager.Instance.GetKart(kartIndex);
        if (kart == null) return GameItem.BOOSTER;
        
        // 计算与领先者的距离差
        float distanceToLeader = this.CalculateDistanceToLeader(kart);
        
        // 动态调整道具概率
        GameItem baseItem = base.GenerateItem(kartIndex);
        
        if (distanceToLeader > this.leadAdvantageThreshold)
        {
            // 大幅落后，提供强力道具
            return this.UpgradeItem(baseItem);
        }
        else if (distanceToLeader < -this.leadAdvantageThreshold)
        {
            // 大幅领先，降级道具
            return this.DowngradeItem(baseItem);
        }
        
        return baseItem;
    }
    
    private float CalculateDistanceToLeader(GoKart kart)
    {
        GoKart leader = KartManager.Instance.GetLeadingKart();
        if (leader == null || leader == kart) return 0f;
        
        // 基于赛道进度计算距离
        float kartProgress = CourseManager.Instance.GetRaceProgress(kart);
        float leaderProgress = CourseManager.Instance.GetRaceProgress(leader);
        
        return leaderProgress - kartProgress;
    }
    
    private GameItem UpgradeItem(GameItem baseItem)
    {
        return baseItem switch
        {
            GameItem.BANANA => GameItem.UFO,           // 香蕉升级为UFO
            GameItem.WATER_FLY => GameItem.WATER_MISSILE, // 水弹升级为导弹
            GameItem.BOOSTER => GameItem.DEVIL,        // 加速升级为恶魔
            _ => baseItem
        };
    }
    
    private GameItem DowngradeItem(GameItem baseItem)
    {
        return baseItem switch
        {
            GameItem.UFO => GameItem.BANANA,           // UFO降级为香蕉
            GameItem.WATER_MISSILE => GameItem.WATER_FLY, // 导弹降级为水弹
            GameItem.DEVIL => GameItem.BOOSTER,        // 恶魔降级为加速
            _ => baseItem
        };
    }
}
```

### 2. 道具元素数据 - GameItemElem.cs

#### 道具使用记录
**功能概述**: 简单的道具使用时间记录数据结构

**数据结构设计**:
```csharp
public class GameItemElem
{
    public int useTick;    // 使用时间戳
    
    public GameItemElem()
    {
        this.useTick = 0;
    }
    
    public GameItemElem(int tick)
    {
        this.useTick = tick;
    }
    
    // 获取使用时间（秒）
    public float GetUseTime()
    {
        return this.useTick / 60f; // 假设60FPS
    }
    
    // 检查是否在指定时间内使用
    public bool WasUsedWithin(float seconds)
    {
        int currentTick = MonoBehaiourExConst.GetTick();
        int timeDiff = currentTick - this.useTick;
        return (timeDiff / 60f) <= seconds;
    }
    
    // 检查道具是否过期
    public bool IsExpired(float timeoutSeconds)
    {
        return !this.WasUsedWithin(timeoutSeconds);
    }
}
```

## 比赛任务实现

### 1. 比赛开始任务 - JobRacingStart.cs

#### 倒计时和准备逻辑
**功能概述**: 处理比赛开始的倒计时和初始化

```csharp
public class JobRacingStart
{
    private const float COUNTDOWN_DURATION = 10f;
    private float countdownTimer_;
    private bool isExecuting_;
    
    public void Execute()
    {
        if (this.isExecuting_) return;
        
        this.isExecuting_ = true;
        this.countdownTimer_ = COUNTDOWN_DURATION;
        
        // 开始倒计时协程
        MonoBehaviour caller = KartManager.Instance;
        caller.StartCoroutine(this.CountdownCoroutine());
    }
    
    private IEnumerator CountdownCoroutine()
    {
        // 准备阶段
        this.PrepareRaceStart();
        
        while (this.countdownTimer_ > 0f)
        {
            // 更新倒计时显示
            this.UpdateCountdownDisplay((int)Mathf.Ceil(this.countdownTimer_));
            
            yield return new WaitForSeconds(1f);
            this.countdownTimer_ -= 1f;
        }
        
        // 比赛正式开始
        this.OnRaceStart();
        this.isExecuting_ = false;
    }
    
    private void PrepareRaceStart()
    {
        // 重置所有卡丁车到起始位置
        KartManager.Instance.ResetAllKartsToStart();
        
        // 禁用输入直到倒计时结束
        InputManager.Instance.SetInputEnabled(false);
        
        // 播放准备音效
        AudioManager.Instance.PlaySound("race_prepare");
        
        // 显示倒计时UI
        UIManager.Instance.ShowCountdown(true);
    }
    
    private void UpdateCountdownDisplay(int seconds)
    {
        UIManager.Instance.UpdateCountdownNumber(seconds);
        
        // 播放倒计时音效
        if (seconds <= 3 && seconds > 0)
        {
            AudioManager.Instance.PlaySound($"countdown_{seconds}");
        }
    }
    
    private void OnRaceStart()
    {
        // 启用输入
        InputManager.Instance.SetInputEnabled(true);
        
        // 开始计时
        RaceTimer.Instance.StartRace();
        
        // 播放开始音效
        AudioManager.Instance.PlaySound("race_start");
        
        // 隐藏倒计时UI
        UIManager.Instance.ShowCountdown(false);
        
        // 通知所有系统比赛开始
        MessageBroadcaster.Instance.SendMessage(new RaceStartMessage());
        
        Debug.Log("Race officially started!");
    }
}
```

## 性能优化和最佳实践

### 1. 高效任务处理
```csharp
public class OptimizedPlayerJobManager
{
    private List<PlayerJob> activeJobs_;
    private List<PlayerJob> jobsToRemove_;
    private float lastUpdateTime_;
    private const float UPDATE_INTERVAL = 1f / 60f;
    
    public void OptimizedUpdate()
    {
        float currentTime = Time.unscaledTime;
        
        // 固定频率更新
        if (currentTime - this.lastUpdateTime_ < UPDATE_INTERVAL)
            return;
        
        // 批量处理
        this.jobsToRemove_.Clear();
        
        for (int i = 0; i < this.activeJobs_.Count; i++)
        {
            PlayerJob job = this.activeJobs_[i];
            job.Update();
            
            if (job.IsJobFinish())
            {
                this.jobsToRemove_.Add(job);
            }
        }
        
        // 批量移除已完成的任务
        foreach (PlayerJob job in this.jobsToRemove_)
        {
            this.activeJobs_.Remove(job);
        }
        
        this.lastUpdateTime_ = currentTime;
    }
}
```

### 2. 道具概率缓存
```csharp
public static class ItemProbabilityCache
{
    private static Dictionary<int, GameItem[]> probabilityCache_ = new Dictionary<int, GameItem[]>();
    
    public static GameItem GetCachedItem(int playerCount, int rank)
    {
        int cacheKey = (playerCount << 8) | rank;
        
        if (!probabilityCache_.ContainsKey(cacheKey))
        {
            probabilityCache_[cacheKey] = GenerateProbabilityArray(playerCount, rank);
        }
        
        GameItem[] items = probabilityCache_[cacheKey];
        int randomIndex = Random.Range(0, items.Length);
        
        return items[randomIndex];
    }
    
    private static GameItem[] GenerateProbabilityArray(int playerCount, int rank)
    {
        // 预计算概率数组，避免运行时计算
        List<GameItem> items = new List<GameItem>();
        
        // 根据概率添加道具到数组中
        // 概率高的道具在数组中出现次数更多
        
        return items.ToArray();
    }
}
```

## 总结

Game系统提供了一个完整、智能的游戏逻辑框架，具有以下核心优势：

### 技术优势
1. **双重任务架构**: 分离实时效果和离散事件的清晰管理
2. **智能平衡机制**: 基于排名的动态道具分配算法
3. **模板方法模式**: 统一的任务生命周期和扩展机制
4. **事件驱动设计**: 松耦合的消息通信系统
5. **性能优化**: 高效的更新循环和缓存策略

### 架构特点
- **策略模式**: 不同类型任务的灵活实现
- **单例模式**: 全局状态管理和资源协调
- **观察者模式**: 事件驱动的状态通知
- **工厂模式**: 任务和道具的动态创建

该游戏逻辑系统为卡丁车游戏提供了专业级的玩法管理基础，支持复杂的比赛流程、平衡的道具分配和流畅的效果处理，确保了公平竞争和出色的游戏体验。