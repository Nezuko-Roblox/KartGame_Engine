# Core 文件夹完整功能文档

## 概述

Core 文件夹是卡丁车游戏的核心系统模块，包含游戏的基础架构、初始化流程、场景管理和阶段控制系统。该模块采用状态机模式管理游戏的各个阶段，支持单人游戏、多人联网、UI导航和资源管理等核心功能。

## 核心文件详细分析

### 1. InitScene.cs - 游戏初始化场景控制器
**文件位置**: `/Core/InitScene.cs`
**功能概述**: 负责游戏启动时的设备验证和初始场景加载

**关键代码分析**:
```csharp
private void Start()
{
    Debug.Log(">>>>>> jaeduk > Start()" + NativeHelper.buildType);
    this.armFlag = 0;
    
    // 针对韩国电信商的ARM服务验证
    if (NativeHelper.buildType == "SKT" || NativeHelper.buildType == "LGT")
    {
        using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
        {
            int num = androidJavaClass.CallStatic<int>("checkArmService", new object[0]);
        }
    }
    else
    {
        this.armServiceVerified();
    }
}

private void Update()
{
    if (this.armFlag == 0) return;
    
    // 设备兼容性检查 - 过滤老旧iOS设备
    if (Array.IndexOf<iPhoneGeneration>(InitScene.IGNORED_GENERATIONS, iPhoneSettings.generation) >= 0)
    {
        GUIUtil.Localize(this.notsupported_.GetComponent<GUITexture>());
        this.notsupported_.SetActiveRecursively(true);
    }
    else
    {
        // 2.5秒延迟后进入主加载场景
        float realtimeSinceStartup = Time.realtimeSinceStartup;
        if ((double)realtimeSinceStartup > this.lastInterval + 2.5)
        {
            KartManager.Instance.parameter_.Stage = StageType.NONE;
            Application.LoadLevel("track_loading");
        }
        else
        {
            this.replayLogo_.SetActiveRecursively(true);
        }
    }
}

// 不支持的设备列表
private static iPhoneGeneration[] IGNORED_GENERATIONS = new iPhoneGeneration[]
{
    iPhoneGeneration.iPhone,        // iPhone 1代
    iPhoneGeneration.iPhone3G,      // iPhone 3G
    iPhoneGeneration.iPodTouch1Gen, // iPod Touch 1代
    iPhoneGeneration.iPodTouch2Gen  // iPod Touch 2代
};
```

**详细功能**:
- 执行平台特定的ARM服务验证（韩国电信商）
- 检查设备兼容性，过滤不支持的iOS设备
- 管理启动logo显示和场景切换时机
- 提供2.5秒的启动缓冲时间

### 2. Initialization.cs - 基础初始化容器
**文件位置**: `/Core/Initialization.cs`
**功能概述**: 预留的初始化扩展点

**关键代码**:
```csharp
public class Initialization : MonoBehaviour
{
    private void Start()
    {
        // 当前为空实现，预留给未来的初始化需求
    }
}
```

**设计目的**: 为未来的全局初始化逻辑提供扩展点

### 3. StaticOption.cs - 静态游戏配置管理器
**文件位置**: `/Core/StaticOption.cs`
**功能概述**: 管理游戏的静态配置选项

**关键代码分析**:
```csharp
public class StaticOption
{
    // 桌面版可用赛道列表
    private static string[] TRACKNAME_DESKTOP = new string[] { 
        "track_ccao",       // 测试赛道1
        "track_screenshot"  // 截图赛道
    };
    
    private static int trackIndex_ = 0;          // 当前赛道索引
    public static int startPosition_ = 0;        // 起始位置配置
    
    // 获取当前赛道名称
    public static string GetTrackName()
    {
        return StaticOption.TRACKNAME_DESKTOP[StaticOption.trackIndex_];
    }
    
    // 循环切换到下一个赛道
    public static void IncreaseTrackIndex()
    {
        StaticOption.trackIndex_ = (StaticOption.trackIndex_ + 1) % StaticOption.TRACKNAME_DESKTOP.Length;
    }
    
    // 获取加载场景名称
    public static string GetLoadStageName()
    {
        return "track_loader";
    }
}
```

**详细功能**:
- 管理桌面版本的可用赛道配置
- 提供赛道循环切换功能
- 定义标准加载场景名称
- 管理起始位置参数

### 4. StaticVariable.cs - 全局静态变量存储
**文件位置**: `/Core/StaticVariable.cs`
**功能概述**: 存储全局共享的静态变量

**关键代码**:
```csharp
public class StaticVariable
{
    // 最后选择的赛道ID，byte.MaxValue表示未选择
    public static byte LAST_SELECTED_TRACK = byte.MaxValue;
}
```

**设计模式**: 简单的全局状态存储，使用特殊值表示未初始化状态

## Stages 子文件夹 - 阶段管理系统详细分析

### 核心基础设施

#### 1. StageController.cs - 阶段控制器核心
**文件位置**: `/Core/Stages/StageController.cs`
**功能概述**: 游戏阶段管理的中央控制器，采用单例模式

**关键代码分析**:
```csharp
public class StageController : MonoBehaviour
{
    // 单例实现 - 自动创建和持久化
    public static StageController Instance
    {
        get
        {
            if (StageController.instance_ == null)
            {
                GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/stage_controller"));
                if (gameObject != null)
                {
                    StageController.instance_ = gameObject.GetComponent<StageController>();
                    if (StageController.instance_ != null)
                    {
                        UnityEngine.Object.DontDestroyOnLoad(StageController.instance_);
                    }
                }
            }
            return StageController.instance_;
        }
    }
    
    // 阶段切换主函数
    public void ChangeStage(StageType nextStage)
    {
        this.inputAuthority_ = 0;                                    // 锁定用户输入
        this.nextStage_ = nextStage;                                // 设置目标阶段
        this.stageControllerState_ = StageControllerState.FADE_OUT; // 开始淡出动画
        this.StartFade(this.FADE_OUT_END_COLOR, this.DEFAULT_FADE_TIME);
        this.FadeOutBgm(this.nextStage_);                          // 淡出背景音乐
    }
    
    // 淡入淡出效果实现
    public void StartFade(Color newScreenOverlayColor, float fadeDuration)
    {
        if (fadeDuration <= 0f)
        {
            // 立即切换
            this.targetScreenOverlayColor_ = newScreenOverlayColor;
            this.currentScreenOverlayColor_ = this.targetScreenOverlayColor_;
            this.SetScreenOverlayColor(this.currentScreenOverlayColor_);
        }
        else
        {
            // 渐变切换
            this.targetScreenOverlayColor_ = newScreenOverlayColor;
            this.deltaColor_ = (this.targetScreenOverlayColor_ - this.currentScreenOverlayColor_) / fadeDuration;
        }
    }
    
    // BGM管理 - 根据阶段类型自动切换音乐
    private string[] BGM_ARRAY = new string[]
    {
        string.Empty,   // NONE
        "title",        // MAIN
        "single",       // SINGLE_ITEM
        "single",       // SINGLE_SPEED
        "multi",        // WIFI
        "multi",        // WAITROOM_HOST
        "multi",        // WAITROOM_CLIENT
        "shop",         // GARAGE
        string.Empty,   // GAME
        string.Empty,   // GAME_WIFI
        "title",        // INFO
        string.Empty,   // LOADING_GAME
        string.Empty,   // LOADING_DEFAULT
        "title",        // MAIN_LOADING
        "shop"          // STORE
    };
}
```

**核心功能**:
- 管理所有游戏阶段的生命周期和转换
- 提供平滑的淡入淡出视觉效果
- 自动管理背景音乐切换
- 控制用户输入权限
- 处理阶段间的同步和数据传递

#### 2. StageType.cs - 阶段类型枚举
**文件位置**: `/Core/Stages/StageType.cs`
**功能概述**: 定义所有可用的游戏阶段类型

**完整枚举定义**:
```csharp
public enum StageType
{
    NONE,           // 0 - 无阶段/初始状态
    MAIN,           // 1 - 主菜单
    SINGLE_ITEM,    // 2 - 单人道具模式
    SINGLE_SPEED,   // 3 - 单人竞速模式
    WIFI,           // 4 - 无线网络模式选择
    WAITROOM_HOST,  // 5 - 房间主机等待
    WAITROOM_CLIENT,// 6 - 房间客户端等待
    GARAGE,         // 7 - 车库/自定义界面
    GAME,           // 8 - 单人游戏中
    GAME_WIFI,      // 9 - 多人网络游戏中
    INFO,           // 10 - 信息/帮助页面
    LOADING_GAME,   // 11 - 游戏内容加载
    LOADING_DEFAULT,// 12 - 默认加载页面
    MAIN_LOADING,   // 13 - 主程序加载
    STORE,          // 14 - 商店/内购页面
    GAMECENTER      // 15 - 游戏中心集成
}
```

#### 3. StageControllerState.cs - 阶段控制器状态
**文件位置**: `/Core/Stages/StageControllerState.cs`
**功能概述**: 定义阶段切换时的淡入淡出状态

**状态枚举**:
```csharp
public enum StageControllerState
{
    NO_FADE,     // 无淡入淡出效果
    FADE_IN,     // 淡入进行中
    FADE_OUT     // 淡出进行中
}
```

#### 4. MonoBehaviourStage.cs - 阶段基类
**文件位置**: `/Core/Stages/MonoBehaviourStage.cs`
**功能概述**: 所有游戏阶段的基础类

**关键实现**:
```csharp
public class MonoBehaviourStage : MonoBehaviourEx
{
    // 向StageController注册自己
    public override void RegistMonoBehaviour(int id)
    {
        base.RegistMonoBehaviour(id);
        StageController.Instance.RegistMonoBehaviour(id, this);
    }
    
    // 通知阶段切换准备完成
    public void ReadyToChangeScene()
    {
        StageController.Instance.ReadyToChangeScene(this.id_);
    }
    
    protected AudioSourceEx bgm_;  // 阶段专用背景音乐
}
```

#### 5. GameStageCommand.cs - 游戏阶段命令
**文件位置**: `/Core/Stages/GameStageCommand.cs`
**功能概述**: 定义游戏内可执行的控制命令

**命令枚举**:
```csharp
public enum GameStageCommand
{
    QUIT,      // 退出游戏
    RESTART,   // 重新开始
    PAUSE,     // 暂停游戏
    RESUME,    // 恢复游戏
    TUTORIAL,  // 显示教程1
    TUTORIAL2  // 显示教程2
}
```

### 网络系统基础

#### 6. NetStage.cs - 网络阶段接口
**文件位置**: `/Core/Stages/NetStage.cs`
**功能概述**: 定义网络功能阶段必须实现的接口

**接口定义**:
```csharp
public interface NetStage
{
    // 处理网络数据包
    bool ProcessPacket(Packet packet, string senderID);
    
    // 处理连接失败
    void ConnectionFailed(string serverID);
}
```

### 游戏核心阶段

#### 7. GameStageBase.cs - 游戏阶段基类
**文件位置**: `/Core/Stages/GameStageBase.cs`
**功能概述**: 所有实际游戏阶段的基础类，包含赛车游戏核心逻辑

**核心状态机**:
```csharp
public enum DriveState
{
    LOADING,        // 加载资源中
    READY,          // 准备开始（倒计时）
    DRIVING,        // 正在驾驶
    RACE_OVER,      // 比赛结束
    RESULT,         // 显示结果
    GO_FINAL_STAGE, // 返回主界面
    RESTART         // 重新开始比赛
}

public enum GameStageMode
{
    BEFORE_RACING,  // 比赛前
    RACING,         // 比赛中
    AFTER_RACING    // 比赛后
}
```

**关键方法分析**:
```csharp
// 初始化游戏环境
protected virtual void Awake()
{
    Time.timeScale = 1f;
    
    // 启动阶段控制器
    if (StageController.IsInstantiated())
    {
        StageController.Instance.BeginStage();
    }
    
    // 初始化核心管理器
    MonoBehaiourExConst.Initialize();
    CameraManager.Instance.Initialize();
    KartManager.Instance.InitGameData();
    
    // 根据游戏模式初始化道具系统
    if (KartManager.Instance.parameter_.gameMode_ == GameMode.SINGLE_ITEM)
    {
        this.InitializeItems();
    }
    
    // 加载赛道资源
    string mainAsset = TrackAssetDefinitionManager.Instance.GetMainAsset((int)KartManager.Instance.parameter_.track_);
    this.InitializeGoCourse(mainAsset);
    this.InitializeKarts(gameObject, mainAsset);
}

// 游戏状态更新逻辑
protected virtual void UpdateState(float tick)
{
    switch (this.driveState_)
    {
        case GameStageBase.DriveState.READY:
            // 倒计时和开始准备
            if (KartManager.Instance.DriveStartTime >= 1f && this.IsUpdateHorn(tick))
            {
                if (this.driveStartHorn_ == 3)
                {
                    KartManager.Instance.Stuck = false;
                    this.audioSource_[1].Play(); // 播放开始音效
                    this.driveState_ = GameStageBase.DriveState.DRIVING;
                    this.gameStageMode_ = GameStageBase.GameStageMode.RACING;
                }
                this.driveStartHorn_++;
            }
            break;
            
        case GameStageBase.DriveState.DRIVING:
            // 驾驶状态监控
            this.CheckKartReset();          // 检查卡丁车重置需求
            this.CheckStuckedReset(tick);   // 检查卡住状态
            
            // 检查完成条件
            if (KartManager.Instance.goCourse_.IsKartGoalIn(KartManager.PLAYER_KART_IDX))
            {
                // 处理比赛完成逻辑
                this.kartDriveTime_ = tick - KartManager.Instance.DriveStartTime;
                this.isWinner_ = KartManager.Instance.goCourse_.GetMyRank() <= 0;
                this.driveState_ = GameStageBase.DriveState.RACE_OVER;
                this.RaceOverSetting();
            }
            break;
            
        case GameStageBase.DriveState.RACE_OVER:
            // 比赛结束处理
            if (this.raceOverTime_ > 0f && this.raceOverTime_ <= tick)
            {
                this.ProcessRaceResults();
            }
            break;
    }
}
```

**AI和卡丁车管理**:
```csharp
// AI信息生成 - 基于玩家表现调整AI难度
protected void GetAiRacingInfoByMiru(out List<GameStageBase.AiRacingInfo> racingInfoList)
{
    byte track_ = KartManager.Instance.parameter_.track_;
    TrackAssetDefinition trackAssetDefinition = TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)track_);
    GameMode gameMode_ = KartManager.Instance.parameter_.gameMode_;
    int winCount = KartOptions.Instance.GetWinCounter(track_)[(int)gameMode_];
    
    float targetTime;
    float timeRange;
    MedalType medalType;
    
    if (winCount == 0)
    {
        // 首次游戏 - 铜牌难度
        targetTime = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.BRONZE);
        timeRange = 2.5f;
        medalType = MedalType.BRONZE;
    }
    else if (winCount == 1)
    {
        // 第二次游戏 - 银牌难度
        targetTime = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.SILVER);
        timeRange = Mathf.Min(2.5f, (trackAssetDefinition.GetMedalTime(gameMode_, MedalType.BRONZE) - targetTime) / 2f);
        medalType = MedalType.SILVER;
    }
    else
    {
        // 高级玩家 - 金牌难度
        targetTime = trackAssetDefinition.GetMedalTime(gameMode_, MedalType.GOLD);
        timeRange = Mathf.Min(2.5f, (trackAssetDefinition.GetMedalTime(gameMode_, MedalType.SILVER) - targetTime) / 2f);
        medalType = MedalType.GOLD;
    }
    
    // 根据卡丁车性能调整AI时间
    List<AssetDefinition> assetDefinitionList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
    float playerLevel = ((KartAssetDefinition)assetDefinitionList[(int)KartManager.Instance.parameter_.kart_[KartManager.PLAYER_KART_IDX].body_]).Level;
    
    for (int i = 0; i < 6; i++)
    {
        if (KartManager.Instance.parameter_.kart_[i] != null && i != KartManager.PLAYER_KART_IDX)
        {
            KartAssetDefinition kartAssetDefinition = (KartAssetDefinition)assetDefinitionList[(int)KartManager.Instance.parameter_.kart_[i].body_];
            float levelDifference = playerLevel - kartAssetDefinition.Level;
            racingInfoList.Add(new GameStageBase.AiRacingInfo(
                AIControllerType.FIXED_DELTA, 
                targetTime + timeRange * i + levelDifference + FiaUtil.GetRandom(-0.5f, 0.5f + kartAssetDefinition.RandomRange), 
                i <= 4, 
                medalType
            ));
        }
    }
}
```

#### 8. GameStage.cs - 具体游戏实现阶段
**文件位置**: `/Core/Stages/GameStage.cs`
**功能概述**: 继承GameStageBase，实现具体的单人游戏逻辑

**任务队列处理系统**:
```csharp
protected void ProcessJobQueue()
{
    KartJob kartJob;
    while ((kartJob = KartJobQueue.Instance.Pop()) != null)
    {
        switch (kartJob.type_)
        {
            case KartJobType.RACING_START:
                // 比赛开始处理
                JobRacingStart jobRacingStart = (JobRacingStart)kartJob;
                KartManager.Instance.DriveStartTime = Time.time + jobRacingStart.GetStartTime();
                InGameStatistics.Instance.Initialize();
                this.gameInterface_.PlayAction("start123@action", KartManager.Instance.DriveStartTime - 3f);
                
                // 教程系统集成
                if (!KartOptions.Instance.IsQuestFlagOn(KartOptions.QuestFlag.TUTORIAL))
                {
                    base.Invoke("ShowTutorial", jobRacingStart.GetStartTime() - 4f);
                }
                break;
                
            case KartJobType.FINISH_NOTICES:
                // 完成通知处理
                JobFinishNotices jobFinishNotices = (JobFinishNotices)kartJob;
                KartManager.Instance.DriveEndTime = jobFinishNotices.GetDriveEndTime();
                if (this.driveState_ == GameStageBase.DriveState.DRIVING)
                {
                    this.retireCountHorn_ = 0;
                    this.retireCountTime_ = KartManager.Instance.DriveEndTime - 10f;
                    this.gameInterface_.PlayAction("finish_count@action", this.retireCountTime_);
                }
                break;
                
            case KartJobType.RACE_OVER:
                // 比赛强制结束
                JobRacingOver jobRacingOver = (JobRacingOver)kartJob;
                this.raceOverTime_ = jobRacingOver.GetRaceOverTime();
                if (this.driveState_ == GameStageBase.DriveState.DRIVING)
                {
                    this.driveState_ = GameStageBase.DriveState.RACE_OVER;
                    // 播放失败音效和动画
                    if (this.commonBgmSource_ != null)
                    {
                        this.commonBgmSource_.PlayOneShot(this.commonBgmClip_[2]); // 失败音乐
                    }
                }
                break;
        }
    }
}
```

**AI记录系统**:
```csharp
// AI行为记录 - 用于生成AI行为数据
protected override void OnPlayRaceOver()
{
    // 保存AI记录数据
    if (this.aiRecord_ != null)
    {
        string timeStamp = DateTime.Now.ToString("MMddHHmmss");
        for (int i = 0; i < this.aiRecord_.Length; i++)
        {
            if (this.aiRecord_[i] != null)
            {
                string fileName = string.Format("ai_{0}_{1}_{2}_{3}_{4}.bin", 
                    TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)KartManager.Instance.parameter_.track_).Name,
                    timeStamp,
                    i,
                    (i != 0) ? this.aiStartSection_[i - 1][this.aiStartSection_[i - 1].Count - 1] : StaticOption.startPosition_,
                    this.aiStartSection_[i][this.aiStartSection_[i].Count - 1]
                );
                this.aiRecord_[i].SerializeToBin(fileName);
            }
        }
    }
}
```

### 网络多人游戏阶段

#### 9. NetGameStage.cs - 网络游戏阶段
**文件位置**: `/Core/Stages/NetGameStage.cs`
**功能概述**: 管理网络多人游戏的同步和通信

**网络同步系统**:
```csharp
public class NetGameStage : GameStageBase, NetStage
{
    // 动态同步频率调整
    private void UpdateSyncRate()
    {
        int peerCount = NetworkManager.Instance.GetActivePeerCount();
        if (peerCount <= 3)
        {
            this.syncRate_ = 4; // 4fps for small groups
        }
        else
        {
            this.syncRate_ = 6; // 6fps for larger groups
        }
    }
    
    // 游戏状态同步
    private void SendGameKartPacket()
    {
        if (Time.time > this.lastSyncTime_ + (1f / this.syncRate_))
        {
            GoPlayKart playKart = KartManager.Instance.goPlayKart_;
            if (playKart != null)
            {
                GameKartPacket packet = new GameKartPacket();
                packet.Initialize(
                    playKart.transform.position,
                    playKart.transform.rotation,
                    playKart.GetVelocity(),
                    playKart.GetCurrentAnimation()
                );
                NetworkManager.Instance.BroadcastPacket(packet);
                this.lastSyncTime_ = Time.time;
            }
        }
    }
    
    // 网络数据包处理
    public bool ProcessPacket(Packet packet, string senderID)
    {
        switch (packet.GetPacketType())
        {
            case PacketType.GAME_KART:
                // 处理其他玩家的卡丁车状态
                GameKartPacket kartPacket = (GameKartPacket)packet;
                this.UpdateRemoteKart(senderID, kartPacket);
                return true;
                
            case PacketType.GAME_CONTROL:
                // 处理游戏控制命令
                GameControlPacket controlPacket = (GameControlPacket)packet;
                this.ProcessGameControl(controlPacket);
                return true;
                
            case PacketType.ITEM:
                // 处理道具使用
                ItemPacket itemPacket = (ItemPacket)packet;
                this.ProcessItemUsage(itemPacket);
                return true;
                
            default:
                return false;
        }
    }
}
```

### 加载和过渡阶段

#### 10. LoadingStage.cs - 主加载阶段
**文件位置**: `/Core/Stages/LoadingStage.cs`
**功能概述**: 游戏启动时的资源初始化和社交系统集成

**核心初始化流程**:
```csharp
protected override void Start()
{
    StageController.Instance.SetBgm(StageType.MAIN_LOADING);
    Time.timeScale = 1f;
    base.Start();
    this.RegistMonoBehaviour(512);
    
    // 防止屏幕变暗
    iPhoneSettings.screenCanDarken = false;
    
    // 初始化资源定义管理器
    TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
    KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
    CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
    
    // 初始化核心系统
    MaterialManager.Instance.Initialize();
    KartOptions.Instance.LoadRegistry();
    MonoBehaviourMessageFactory.Instance.Initialize();
    Statistics.Instance.Initialize();
    
    // 控制器配置 - 根据设备类型设置默认控制器
    if (KartOptions.Instance.Controller == -1)
    {
        KartOptions.Instance.Controller = (!Env.IsIPad) ? 2 : 3;
    }
    iOSController.Instance.Type = (iOSControllerType)KartOptions.Instance.Controller;
    
    // 内购系统初始化
    MockFiaStore.Inst.LoadPurchasedProductList();
    base.StartCoroutine(MockFiaStore.Inst.RequestProductInfo());
    
    // 刷新资源定义
    CharacterAssetDefinitionManager.Instance.Refresh();
    KartAssetDefinitionManager.Instance.Refresh();
    TrackAssetDefinitionManager.Instance.Refresh();
    
    // 初始化卡丁车预览器
    GUIKartViewer.Instance.ChangeKartCharacter((byte)KartOptions.instance_.Kart, (byte)KartOptions.instance_.Character);
    GUIKartViewer.Instance.Hide();
    
    // Facebook集成处理
    this.InitializeFacebookIntegration();
}

private void InitializeFacebookIntegration()
{
    if (Env.IsDesktop)
    {
        // 桌面版使用Mock Facebook
        this.fb_ = MockFacebook.Inst;
        if (!this.fb_.LoggedIn)
        {
            this.updating_ |= 2;
            FiaCoroutine fiaCoroutine = new FiaCoroutine(this.fb_.Login(), new OnSuccess(this.LoginSuccess), new OnFailure(this.LoginFailure));
            base.StartCoroutine(fiaCoroutine);
        }
        else
        {
            this.UpdateFriendDictAndRanking();
        }
    }
    else
    {
        // 移动设备Facebook集成
        this.fb_ = Facebook.Inst;
        if (this.fb_.LoggedIn)
        {
            this.UpdateRanking();
        }
        else if (this.fb_.FBID != null && FiaAuth.AuthToken != null)
        {
            this.fb_.LoadFriendXML();
            if (Env.IsConnectedToInternet)
            {
                this.UpdateFriendDictAndRanking();
            }
        }
    }
}
```

#### 11. GameLoadingStage.cs - 游戏内容加载阶段
**文件位置**: `/Core/Stages/GameLoadingStage.cs`
**功能概述**: 显示加载屏幕和游戏提示

**UI和动画系统**:
```csharp
protected override void Start()
{
    base.Start();
    this.RegistMonoBehaviour(513);
    
    // 随机选择显示提示（13个可用提示）
    int randomTip = Random.Range(0, 13);
    this.currentTipIndex_ = randomTip;
    
    // 设置提示文本和背景
    this.SetTipVisibility(randomTip, true);
    
    // 启动加载动画（12帧循环动画）
    this.StartLoadingAnimation();
}

private void StartLoadingAnimation()
{
    this.animationFrame_ = 0;
    this.InvokeRepeating("UpdateLoadingAnimation", 0f, 0.1f); // 10fps动画
}

private void UpdateLoadingAnimation()
{
    // 更新加载转圈动画
    if (this.loadingRenderer_ != null)
    {
        Material currentMaterial = this.loadingMaterials_[this.animationFrame_];
        this.loadingRenderer_.material = currentMaterial;
        this.animationFrame_ = (this.animationFrame_ + 1) % 12; // 12帧循环
    }
}
```

#### 12. BetweenSceneStage.cs - 场景间过渡阶段
**文件位置**: `/Core/Stages/BetweenSceneStage.cs`
**功能概述**: 处理场景间的平滑过渡

**异步加载系统**:
```csharp
public class BetweenSceneStage : MonoBehaviourStage
{
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(514);
        
        // 启动进度动画
        this.InvokeRepeating("ProgressAnimation", 0f, 0.1f);
        
        // 延迟后开始加载下一个场景
        base.Invoke("LoadNextStage", 1.5f);
    }
    
    private void LoadNextStage()
    {
        // 异步加载下一个场景
        base.StartCoroutine(this.LoadNextStageCoroutine());
    }
    
    private IEnumerator LoadNextStageCoroutine()
    {
        // 设置目标场景
        KartManager.Instance.parameter_.Stage = GameLoadingStageStaticVariable.nextStage_;
        
        // 异步加载场景
        yield return Application.LoadLevelAdditiveAsync(KartDefine.STAGE_SCNE_NAME[(int)GameLoadingStageStaticVariable.nextStage_]);
        
        // 清理和完成
        base.CancelInvoke("ProgressAnimation");
        UnityEngine.Object.Destroy(this.cam_.gameObject);
    }
    
    private void ProgressAnimation()
    {
        // 更新进度条动画
        this.progressValue_ += 0.05f;
        if (this.progressValue_ > 1f)
        {
            this.progressValue_ = 0f;
        }
        // 更新UI显示
        this.UpdateProgressBar(this.progressValue_);
    }
}
```

#### 13. GameLoadingStageStaticVariable.cs - 场景转换数据
**文件位置**: `/Core/Stages/GameLoadingStageStaticVariable.cs`
**功能概述**: 在加载阶段间共享状态数据

**数据结构**:
```csharp
public class GameLoadingStageStaticVariable
{
    public static StageType prevStage_;  // 来源阶段
    public static StageType nextStage_;  // 目标阶段
}
```

### 单人游戏模式阶段

#### 14. SingleModeStage.cs - 单人模式配置阶段
**文件位置**: `/Core/Stages/SingleModeStage.cs`
**功能概述**: 配置单人游戏的难度和AI对手

**AI难度系统**:
```csharp
protected override void Start()
{
    base.Start();
    this.RegistMonoBehaviour(515);
    
    // 获取玩家历史表现
    byte selectedTrack = KartOptions.Instance.Track;
    GameMode gameMode = (KartOptions.Instance.GameMode != 0) ? GameMode.SINGLE_ITEM : GameMode.SINGLE_SPEED;
    
    GhostFilenameInfo bestRecord = KartOptions.Instance.GetBestInfo(selectedTrack, (int)gameMode);
    TrackAssetDefinition trackDefinition = TrackAssetDefinitionManager.Instance.GetAssetDefinition((int)selectedTrack);
    
    if (bestRecord != null && trackDefinition != null)
    {
        float[] medalTimes = trackDefinition.GetMedalTime(gameMode);
        
        // 根据最佳记录选择AI难度
        List<AssetDefinition> availableKarts;
        if (bestRecord.finishTime_ <= medalTimes[0]) // 金牌水平
        {
            // 使用高级AI和高性能卡丁车
            KartAssetDefinitionManager.Instance.GetRandomAsset(4, Random.Range(3, 6), ref availableKarts);
        }
        else if (bestRecord.finishTime_ <= medalTimes[1]) // 银牌水平
        {
            // 使用中级AI和中等性能卡丁车
            KartAssetDefinitionManager.Instance.GetRandomAsset(2, Random.Range(2, 5), ref availableKarts);
        }
        else // 铜牌或以下水平
        {
            // 使用初级AI和低性能卡丁车
            KartAssetDefinitionManager.Instance.GetRandomAsset(0, Random.Range(1, 4), ref availableKarts);
        }
        
        // 分配AI卡丁车和角色
        this.AssignAIKartsAndCharacters(availableKarts);
    }
    
    // 验证赛道资源
    this.ValidateTrackAssets(selectedTrack);
    
    // 启动游戏
    this.StartSinglePlayerGame();
}

private void AssignAIKartsAndCharacters(List<AssetDefinition> availableKarts)
{
    List<AssetDefinition> availableCharacters = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
    
    for (int i = 0; i < 6; i++)
    {
        if (i != KartManager.PLAYER_KART_IDX) // 跳过玩家位置
        {
            // 随机选择AI卡丁车
            AssetDefinition randomKart = availableKarts[Random.Range(0, availableKarts.Count)];
            
            // 随机选择AI角色
            AssetDefinition randomCharacter = availableCharacters[Random.Range(0, availableCharacters.Count)];
            
            // 分配给AI
            KartManager.Instance.parameter_.kart_[i] = new AssetSelection();
            KartManager.Instance.parameter_.kart_[i].body_ = (byte)randomKart.Index;
            KartManager.Instance.parameter_.kart_[i].character_ = (byte)randomCharacter.Index;
            KartManager.Instance.parameter_.kart_[i].type_ = PlayerType.AI;
        }
    }
}
```

### 网络多人游戏阶段

#### 15. BaseWaitRoomStage.cs - 等待房间基类
**文件位置**: `/Core/Stages/BaseWaitRoomStage.cs`
**功能概述**: 所有网络等待房间的抽象基类

**网络同步系统**:
```csharp
public abstract class BaseWaitRoomStage : MonoBehaviourStage, NetStage
{
    protected Dictionary<string, Peer> peers_ = new Dictionary<string, Peer>();
    protected GameParamPacket gameParam_;
    protected bool isHost_;
    
    // 创建玩家数据包
    protected PlayerPacket CreatePlayerPacket()
    {
        PlayerPacket packet = new PlayerPacket();
        packet.fbid_ = Facebook.Inst.FBID;
        packet.name_ = Facebook.Inst.Name;
        packet.kart_ = (byte)KartOptions.Instance.Kart;
        packet.character_ = (byte)KartOptions.Instance.Character;
        packet.ready_ = this.isReady_;
        return packet;
    }
    
    // 创建游戏参数数据包
    protected GameParamPacket CreateGameParamPacket()
    {
        GameParamPacket packet = new GameParamPacket();
        packet.track_ = KartOptions.Instance.Track;
        packet.gameMode_ = (GameMode)KartOptions.Instance.GameMode;
        packet.maxLap_ = 3;
        
        // 生成随机起始位置
        List<int> startPositions;
        FiaUtil.GetRandomList(0, this.peers_.Count - 1, out startPositions);
        packet.startPositions_ = startPositions;
        
        return packet;
    }
    
    // 网络数据包处理
    public virtual bool ProcessPacket(Packet packet, string senderID)
    {
        switch (packet.GetPacketType())
        {
            case PacketType.PLAYER:
                PlayerPacket playerPacket = (PlayerPacket)packet;
                this.UpdatePeerInfo(senderID, playerPacket);
                this.RefreshPeerList();
                return true;
                
            case PacketType.GAME_PARAM:
                GameParamPacket gameParamPacket = (GameParamPacket)packet;
                if (!this.isHost_)
                {
                    this.gameParam_ = gameParamPacket;
                    this.UpdateGameParameters();
                }
                return true;
                
            case PacketType.USER_LEAVE_NOTICE:
                UserLeaveNoticePacket leavePacket = (UserLeaveNoticePacket)packet;
                this.RemovePeer(senderID);
                this.RefreshPeerList();
                return true;
                
            default:
                return false;
        }
    }
    
    // 抽象方法 - 子类必须实现
    public abstract void SetReady(bool ready);
    public abstract void Ready();
    public abstract void LoadResources();
    public abstract void UpdateStatus();
}
```

#### 16. WaitRoomStage.cs - 等待房间实现
**文件位置**: `/Core/Stages/WaitRoomStage.cs`
**功能概述**: 具体的网络等待房间实现

**房间管理系统**:
```csharp
public class WaitRoomStage : BaseWaitRoomStage
{
    private bool allPlayersReady_ = false;
    private float readyCheckTimer_ = 0f;
    
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(517);
        
        // 初始化房间状态
        this.isHost_ = NetworkManager.Instance.IsServer();
        this.isReady_ = false;
        
        // 如果是主机，创建游戏参数
        if (this.isHost_)
        {
            this.gameParam_ = this.CreateGameParamPacket();
            NetworkManager.Instance.BroadcastPacket(this.gameParam_);
        }
        
        // 广播自己的玩家信息
        PlayerPacket playerPacket = this.CreatePlayerPacket();
        NetworkManager.Instance.BroadcastPacket(playerPacket);
        
        // 启动UI更新
        this.InvokeRepeating("UpdateRoomUI", 0f, 0.5f);
    }
    
    protected override void Update()
    {
        base.Update();
        
        // 定期检查是否所有玩家都准备好了
        this.readyCheckTimer_ += Time.deltaTime;
        if (this.readyCheckTimer_ > 1f) // 每秒检查一次
        {
            this.CheckAllPlayersReady();
            this.readyCheckTimer_ = 0f;
        }
    }
    
    private void CheckAllPlayersReady()
    {
        bool allReady = true;
        int playerCount = 0;
        
        foreach (KeyValuePair<string, Peer> peerPair in this.peers_)
        {
            playerCount++;
            if (!peerPair.Value.ready_)
            {
                allReady = false;
                break;
            }
        }
        
        // 至少需要2名玩家且所有人都准备好
        if (allReady && playerCount >= 2 && this.isHost_)
        {
            this.StartNetworkGame();
        }
    }
    
    private void StartNetworkGame()
    {
        // 广播游戏开始消息
        GameControlPacket startPacket = new GameControlPacket();
        startPacket.command_ = GameControlCommand.START_GAME;
        NetworkManager.Instance.BroadcastPacket(startPacket);
        
        // 切换到网络游戏阶段
        StageController.Instance.ChangeStage(StageType.GAME_WIFI);
    }
    
    // 实现抽象方法
    public override void SetReady(bool ready)
    {
        this.isReady_ = ready;
        
        // 广播准备状态更新
        PlayerPacket packet = this.CreatePlayerPacket();
        NetworkManager.Instance.BroadcastPacket(packet);
        
        this.UpdateStatus();
    }
    
    public override void Ready()
    {
        this.SetReady(true);
    }
    
    public override void LoadResources()
    {
        // 预加载游戏资源
        string trackAssetName = TrackAssetDefinitionManager.Instance.GetMainAsset((int)this.gameParam_.track_);
        ResourceLoader.Instance.PreloadAsset(trackAssetName);
    }
    
    public override void UpdateStatus()
    {
        // 更新房间状态UI
        this.UpdatePlayerList();
        this.UpdateReadyButton();
        this.UpdateStartButton();
    }
}
```

#### 17. GameLobbyStage.cs - 游戏大厅阶段
**文件位置**: `/Core/Stages/GameLobbyStage.cs`
**功能概述**: 网络游戏的服务器浏览和连接界面

**服务器发现系统**:
```csharp
public class GameLobbyStage : MonoBehaviourStage
{
    private List<ServerInfo> availableServers_ = new List<ServerInfo>();
    private bool isSearching_ = false;
    private float searchTimer_ = 0f;
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(518);
        
        // 初始化网络管理器
        NetworkManager.Instance.Initialize();
        
        // 开始搜索可用服务器
        this.StartServerSearch();
        
        // 随机化开发者名称用于测试
        this.RandomizeDeveloperName();
    }
    
    private void StartServerSearch()
    {
        this.isSearching_ = true;
        this.searchTimer_ = 0f;
        
        if (Env.IsDesktop)
        {
            // 桌面版使用模拟服务器
            NetworkManager.Instance.StartMockServerDiscovery();
        }
        else
        {
            // 移动设备使用真实网络发现
            AndroidNetwork.Instance.StartServerDiscovery();
        }
        
        // 启动服务器列表更新
        this.InvokeRepeating("UpdateServerList", 0f, 2f);
    }
    
    private void UpdateServerList()
    {
        List<ServerInfo> discoveredServers = NetworkManager.Instance.GetDiscoveredServers();
        
        // 更新服务器列表
        this.availableServers_.Clear();
        this.availableServers_.AddRange(discoveredServers);
        
        // 更新UI显示
        this.RefreshServerListUI();
    }
    
    private void RandomizeDeveloperName()
    {
        // 为开发测试随机化玩家名称
        string[] developerNames = new string[]
        {
            "sublee", "jaeduk", "miru", "kaiser", "nexon_dev", "kart_tester"
        };
        
        string randomName = developerNames[Random.Range(0, developerNames.Length)];
        if (Facebook.Inst != null)
        {
            Facebook.Inst.SetDeveloperName(randomName);
        }
    }
    
    // UI事件处理
    public void OnHostGameClicked()
    {
        // 创建新游戏房间
        NetworkManager.Instance.StartServer();
        StageController.Instance.ChangeStage(StageType.WAITROOM_HOST);
    }
    
    public void OnJoinGameClicked(int serverIndex)
    {
        if (serverIndex < this.availableServers_.Count)
        {
            ServerInfo selectedServer = this.availableServers_[serverIndex];
            
            // 连接到选中的服务器
            NetworkManager.Instance.ConnectToServer(selectedServer);
            StageController.Instance.ChangeStage(StageType.WAITROOM_CLIENT);
        }
    }
    
    public void OnRefreshClicked()
    {
        // 重新搜索服务器
        this.StartServerSearch();
    }
}
```

### UI和菜单阶段

#### 18. MainMenuStage.cs - 主菜单阶段
**文件位置**: `/Core/Stages/MainMenuStage.cs`
**功能概述**: 游戏主菜单界面管理

**简洁实现**:
```csharp
public class MainMenuStage : MonoBehaviourStage
{
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(516); // 主菜单专用ID
    }
    
    public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
    {
        if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
        {
            // 准备切换到其他场景
            base.ReadyToChangeScene();
        }
    }
}
```

#### 19. GarageStage.cs - 车库自定义阶段
**文件位置**: `/Core/Stages/GarageStage.cs`
**功能概述**: 车辆和角色自定义界面（多人模式）

**网络集成实现**:
```csharp
public class GarageStage : BaseWaitRoomStage
{
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(519);
        
        // 初始化自定义界面
        this.InitializeCustomizationUI();
    }
    
    // 实现基类抽象方法（最小实现）
    public override void SetReady(bool ready)
    {
        this.isReady_ = ready;
    }
    
    public override void Ready()
    {
        this.isReady_ = true;
    }
    
    public override void LoadResources()
    {
        // 预加载自定义资源
    }
    
    public override void UpdateStatus()
    {
        // 更新自定义状态
    }
}
```

#### 20. InfoStage.cs - 信息展示阶段
**文件位置**: `/Core/Stages/InfoStage.cs`
**功能概述**: 显示游戏信息、帮助和制作人员名单

**简单信息显示**:
```csharp
public class InfoStage : MonoBehaviourStage
{
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(520);
    }
    
    public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
    {
        if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
        {
            base.ReadyToChangeScene();
        }
    }
}
```

#### 21. StoreStage.cs - 商店阶段
**文件位置**: `/Core/Stages/StoreStage.cs`
**功能概述**: 内购商店和道具购买界面

**内购系统集成**:
```csharp
public class StoreStage : MonoBehaviourStage
{
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(521);
        
        // 检查是否需要恢复购买
        if (PlayerPrefs.GetInt("RESTORED_PURCHASE", 0) == 0)
        {
            PlayerPrefs.SetInt("RESTORED_PURCHASE", 1);
            
            // 显示恢复购买弹窗
            MonoBehaviourMessage message = MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.SHOW_RESTORE_PURCHASES_POPUP);
            MonoBehaviourExCenter.Instance.SendMessage(0, 522, message);
        }
        
        // 初始化商店界面
        this.InitializeStoreUI();
    }
    
    private void InitializeStoreUI()
    {
        // 加载商店商品列表
        List<Product> productList = FiaStore.Inst.ProductInfoList;
        
        // 创建商品UI元素
        foreach (Product product in productList)
        {
            this.CreateProductUI(product);
        }
    }
    
    public override void ReceiveMessage(int senderId, MonoBehaviourMessage msg)
    {
        if (msg.type_ == MonoBehaviourMessageType.CHANGE_SCENE)
        {
            base.ReadyToChangeScene();
        }
        else if (msg.type_ == MonoBehaviourMessageType.PURCHASE_COMPLETED)
        {
            // 处理购买完成
            this.OnPurchaseCompleted();
        }
    }
    
    private void OnPurchaseCompleted()
    {
        // 刷新界面状态
        this.RefreshStoreUI();
        
        // 保存购买状态
        KartOptions.Instance.SaveRegistry();
    }
}
```

### 开发和测试阶段

#### 22. LoadStage.cs - 开发加载阶段
**文件位置**: `/Core/Stages/LoadStage.cs`
**功能概述**: 提供完整的开发时配置和测试功能

**开发者工具集成**:
```csharp
public class LoadStage : MonoBehaviourStage
{
    private int selectedTrack_ = 0;
    private int selectedKart_ = 0;
    private int selectedCharacter_ = 0;
    private int selectedController_ = 2;
    
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(523);
        
        // 初始化开发者界面
        this.InitializeDeveloperUI();
        
        // 加载所有可用资源
        this.LoadAllAssetDefinitions();
        
        // 设置默认配置
        this.SetDefaultConfiguration();
    }
    
    private void InitializeDeveloperUI()
    {
        // 创建赛道选择界面
        this.CreateTrackSelectionUI();
        
        // 创建卡丁车选择界面
        this.CreateKartSelectionUI();
        
        // 创建角色选择界面
        this.CreateCharacterSelectionUI();
        
        // 创建控制器配置界面
        this.CreateControllerConfigUI();
        
        // 创建AI记录管理界面
        this.CreateAIRecordManagementUI();
        
        // 创建Ghost记录界面
        this.CreateGhostRecordUI();
    }
    
    private void CreateTrackSelectionUI()
    {
        List<AssetDefinition> trackList = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        for (int i = 0; i < trackList.Count; i++)
        {
            TrackAssetDefinition track = (TrackAssetDefinition)trackList[i];
            
            // 创建赛道按钮
            GameObject trackButton = this.CreateUIButton(track.Name, i);
            trackButton.GetComponent<Button>().onClick.AddListener(() => this.OnTrackSelected(i));
        }
    }
    
    private void CreateKartSelectionUI()
    {
        List<AssetDefinition> kartList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        for (int i = 0; i < kartList.Count; i++)
        {
            KartAssetDefinition kart = (KartAssetDefinition)kartList[i];
            
            // 创建卡丁车预览
            GameObject kartPreview = this.CreateKartPreview(kart, i);
            kartPreview.GetComponent<Button>().onClick.AddListener(() => this.OnKartSelected(i));
        }
    }
    
    private void CreateControllerConfigUI()
    {
        // iOS控制器配置
        string[] controllerNames = new string[]
        {
            "Tilt + Touch",      // 0
            "Tilt + Buttons",    // 1  
            "Touch Only",        // 2
            "iPad Touch",        // 3
            "External Controller" // 4
        };
        
        for (int i = 0; i < controllerNames.Length; i++)
        {
            GameObject controllerButton = this.CreateUIButton(controllerNames[i], i);
            controllerButton.GetComponent<Button>().onClick.AddListener(() => this.OnControllerSelected(i));
        }
    }
    
    // AI记录文件管理
    private void CreateAIRecordManagementUI()
    {
        // 列出所有AI记录文件
        string[] aiRecordFiles = Directory.GetFiles(Application.persistentDataPath, "ai_*.bin");
        
        foreach (string filePath in aiRecordFiles)
        {
            string fileName = Path.GetFileName(filePath);
            
            // 创建记录项UI
            GameObject recordItem = this.CreateRecordItem(fileName);
            
            // 添加播放按钮
            Button playButton = recordItem.transform.Find("PlayButton").GetComponent<Button>();
            playButton.onClick.AddListener(() => this.PlayAIRecord(filePath));
            
            // 添加删除按钮
            Button deleteButton = recordItem.transform.Find("DeleteButton").GetComponent<Button>();
            deleteButton.onClick.AddListener(() => this.DeleteAIRecord(filePath));
        }
    }
    
    // 启动游戏配置
    public void OnStartGameClicked()
    {
        // 应用选择的配置
        KartOptions.Instance.Track = (byte)this.selectedTrack_;
        KartOptions.Instance.Kart = this.selectedKart_;
        KartOptions.Instance.Character = this.selectedCharacter_;
        KartOptions.Instance.Controller = this.selectedController_;
        
        // 保存配置
        KartOptions.Instance.SaveRegistry();
        
        // 启动游戏
        StageController.Instance.ChangeStage(StageType.GAME);
    }
}
```

#### 23. TestStage.cs - 资源测试阶段
**文件位置**: `/Core/Stages/TestStage.cs`
**功能概述**: 测试角色资源加载和装配

**资源测试系统**:
```csharp
public class TestStage : MonoBehaviourStage
{
    private int currentCharacterIndex_ = 0;
    private GameObject testCharacter_;
    
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(524);
        
        // 开始角色测试
        this.StartCharacterTest();
    }
    
    private void StartCharacterTest()
    {
        // 获取所有可用角色
        List<AssetDefinition> characterList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        if (characterList.Count > 0)
        {
            this.LoadTestCharacter(this.currentCharacterIndex_);
            
            // 每3秒切换到下一个角色
            this.InvokeRepeating("NextCharacter", 3f, 3f);
        }
    }
    
    private void LoadTestCharacter(int characterIndex)
    {
        // 清理之前的测试角色
        if (this.testCharacter_ != null)
        {
            UnityEngine.Object.DestroyImmediate(this.testCharacter_);
        }
        
        List<AssetDefinition> characterList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
        if (characterIndex < characterList.Count)
        {
            CharacterAssetDefinition character = (CharacterAssetDefinition)characterList[characterIndex];
            
            // 加载角色资源
            GameObject characterPrefab = ResourceLoader.Instance.LoadCharacterAsset(character.MainAsset);
            if (characterPrefab != null)
            {
                this.testCharacter_ = UnityEngine.Object.Instantiate(characterPrefab);
                this.testCharacter_.transform.position = Vector3.zero;
                
                // 测试骨骼映射
                this.TestBoneMapping();
                
                // 测试动画控制器
                this.TestAnimationController();
                
                // 输出测试结果
                Debug.Log(string.Format("Character Test: {0} - Loaded Successfully", character.Name));
            }
            else
            {
                Debug.LogError(string.Format("Character Test: {0} - Failed to Load", character.Name));
            }
        }
    }
    
    private void TestBoneMapping()
    {
        if (this.testCharacter_ != null)
        {
            // 查找关键骨骼
            Transform[] bones = this.testCharacter_.GetComponentsInChildren<Transform>();
            
            string[] requiredBones = new string[]
            {
                "Bip01 Pelvis", "Bip01 Spine", "Bip01 Neck", "Bip01 Head",
                "Bip01 L UpperArm", "Bip01 L Forearm", "Bip01 L Hand",
                "Bip01 R UpperArm", "Bip01 R Forearm", "Bip01 R Hand",
                "Bip01 L Thigh", "Bip01 L Calf", "Bip01 L Foot",
                "Bip01 R Thigh", "Bip01 R Calf", "Bip01 R Foot"
            };
            
            int foundBones = 0;
            foreach (string boneName in requiredBones)
            {
                Transform bone = this.FindBoneByName(bones, boneName);
                if (bone != null)
                {
                    foundBones++;
                }
            }
            
            float boneCompleteness = (float)foundBones / requiredBones.Length;
            Debug.Log(string.Format("Bone Mapping: {0:P0} complete ({1}/{2} bones found)", 
                boneCompleteness, foundBones, requiredBones.Length));
        }
    }
    
    private void TestAnimationController()
    {
        Animation animation = this.testCharacter_.GetComponent<Animation>();
        if (animation != null)
        {
            // 测试所有动画剪辑
            foreach (AnimationState animationState in animation)
            {
                Debug.Log(string.Format("Animation Clip: {0} - Length: {1:F2}s", 
                    animationState.name, animationState.length));
            }
            
            // 播放默认动画
            if (animation.GetClipCount() > 0)
            {
                animation.Play();
            }
        }
    }
    
    private void NextCharacter()
    {
        List<AssetDefinition> characterList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
        this.currentCharacterIndex_ = (this.currentCharacterIndex_ + 1) % characterList.Count;
        this.LoadTestCharacter(this.currentCharacterIndex_);
    }
}
```

#### 24. FacebookTestStage.cs - Facebook集成测试
**文件位置**: `/Core/Stages/FacebookTestStage.cs`
**功能概述**: 测试Facebook社交功能集成

**社交功能测试**:
```csharp
public class FacebookTestStage : MonoBehaviourStage
{
    private Facebook facebook_;
    private bool isLoggedIn_ = false;
    
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(525);
        
        // 初始化Facebook
        this.facebook_ = Facebook.Inst;
        
        // 创建测试UI
        this.CreateTestUI();
        
        // 检查登录状态
        this.CheckLoginStatus();
    }
    
    private void CreateTestUI()
    {
        // 登录/登出按钮
        GameObject loginButton = this.CreateUIButton("Login/Logout", 0);
        loginButton.GetComponent<Button>().onClick.AddListener(this.OnLoginLogoutClicked);
        
        // 发布消息按钮
        GameObject postButton = this.CreateUIButton("Post Message", 1);
        postButton.GetComponent<Button>().onClick.AddListener(this.OnPostMessageClicked);
        
        // 下载头像按钮
        GameObject avatarButton = this.CreateUIButton("Download Avatar", 2);
        avatarButton.GetComponent<Button>().onClick.AddListener(this.OnDownloadAvatarClicked);
        
        // 屏幕方向测试按钮
        GameObject orientationButton = this.CreateUIButton("Test Orientation", 3);
        orientationButton.GetComponent<Button>().onClick.AddListener(this.OnTestOrientationClicked);
        
        // 好友列表按钮
        GameObject friendsButton = this.CreateUIButton("Fetch Friends", 4);
        friendsButton.GetComponent<Button>().onClick.AddListener(this.OnFetchFriendsClicked);
    }
    
    private void OnLoginLogoutClicked()
    {
        if (this.isLoggedIn_)
        {
            // 登出
            FiaCoroutine logoutCoroutine = new FiaCoroutine(
                this.facebook_.Logout(),
                new OnSuccess(this.OnLogoutSuccess),
                new OnFailure(this.OnLogoutFailure)
            );
            this.StartCoroutine(logoutCoroutine);
        }
        else
        {
            // 登录
            FiaCoroutine loginCoroutine = new FiaCoroutine(
                this.facebook_.Login(),
                new OnSuccess(this.OnLoginSuccess),
                new OnFailure(this.OnLoginFailure)
            );
            this.StartCoroutine(loginCoroutine);
        }
    }
    
    private void OnPostMessageClicked()
    {
        if (this.isLoggedIn_)
        {
            string message = "Testing Facebook integration from Kart Game!";
            string link = "https://kartriderrush.nexon.com";
            string picture = "https://kartriderrush.nexon.com/images/logo.png";
            
            FiaCoroutine postCoroutine = new FiaCoroutine(
                this.facebook_.Post(message, link, picture),
                new OnSuccess(this.OnPostSuccess),
                new OnFailure(this.OnPostFailure)
            );
            this.StartCoroutine(postCoroutine);
        }
        else
        {
            Debug.Log("Please login to Facebook first");
        }
    }
    
    private void OnDownloadAvatarClicked()
    {
        if (this.isLoggedIn_)
        {
            string avatarUrl = string.Format("https://graph.facebook.com/{0}/picture?type=large", this.facebook_.FBID);
            
            FiaCoroutine downloadCoroutine = new FiaCoroutine(
                this.facebook_.DownloadProfileImage(avatarUrl),
                new OnSuccessWith<Texture2D>(this.OnAvatarDownloaded),
                new OnFailure(this.OnAvatarDownloadFailed)
            );
            this.StartCoroutine(downloadCoroutine);
        }
    }
    
    private void OnTestOrientationClicked()
    {
        // 测试屏幕方向切换
        if (ScreenController.Instance.IsDisplayingLandscapeLeft())
        {
            ScreenController.Instance.ForceDisplayingOrientation(ScreenController.Orientation.LANDSCAPE_RIGHT);
        }
        else
        {
            ScreenController.Instance.ForceDisplayingOrientation(ScreenController.Orientation.LANDSCAPE_LEFT);
        }
    }
    
    private void OnFetchFriendsClicked()
    {
        if (this.isLoggedIn_)
        {
            FiaCoroutine friendsCoroutine = new FiaCoroutine(
                this.facebook_.FetchFriends(),
                new OnSuccess(this.OnFriendsSuccess),
                new OnFailure(this.OnFriendsFailure)
            );
            this.StartCoroutine(friendsCoroutine);
        }
    }
    
    // 回调函数
    private void OnLoginSuccess()
    {
        this.isLoggedIn_ = true;
        Debug.Log("Facebook Login Success: " + this.facebook_.Name);
    }
    
    private void OnLoginFailure(Exception ex)
    {
        Debug.LogError("Facebook Login Failed: " + ex.Message);
    }
    
    private void OnLogoutSuccess()
    {
        this.isLoggedIn_ = false;
        Debug.Log("Facebook Logout Success");
    }
    
    private void OnPostSuccess()
    {
        Debug.Log("Facebook Post Success");
    }
    
    private void OnAvatarDownloaded(Texture2D avatar)
    {
        Debug.Log("Avatar Downloaded: " + avatar.width + "x" + avatar.height);
        
        // 显示头像
        GameObject avatarDisplay = GameObject.Find("AvatarDisplay");
        if (avatarDisplay != null)
        {
            avatarDisplay.GetComponent<Renderer>().material.mainTexture = avatar;
        }
    }
}
```

### 内存和性能测试阶段

#### 25. MemoryCheckStage.cs - 动态内存测试
**文件位置**: `/Core/Stages/MemoryCheckStage.cs`
**功能概述**: 测试运行时资源加载的内存使用情况

**内存分析系统**:
```csharp
public class MemoryCheckStage : MonoBehaviourStage
{
    private List<GameObject> instantiatedObjects_ = new List<GameObject>();
    private int currentTestIndex_ = 0;
    private float initialMemory_;
    private float peakMemory_;
    
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(526);
        
        // 记录初始内存使用
        this.initialMemory_ = this.GetCurrentMemoryUsage();
        this.peakMemory_ = this.initialMemory_;
        
        // 开始内存测试
        this.StartMemoryTest();
    }
    
    private void StartMemoryTest()
    {
        Debug.Log(string.Format("Memory Test Started - Initial Memory: {0:F2} MB", this.initialMemory_));
        
        // 启动测试循环
        this.InvokeRepeating("RunMemoryTest", 1f, 2f);
    }
    
    private void RunMemoryTest()
    {
        // 测试不同类型的资源加载
        switch (this.currentTestIndex_ % 4)
        {
            case 0:
                this.TestKartAssetLoading();
                break;
            case 1:
                this.TestCharacterAssetLoading();
                break;
            case 2:
                this.TestTrackAssetLoading();
                break;
            case 3:
                this.TestEffectAssetLoading();
                break;
        }
        
        this.currentTestIndex_++;
        
        // 更新内存统计
        this.UpdateMemoryStatistics();
        
        // 测试100次后停止
        if (this.currentTestIndex_ >= 100)
        {
            this.StopMemoryTest();
        }
    }
    
    private void TestKartAssetLoading()
    {
        List<AssetDefinition> kartList = KartAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        if (kartList.Count > 0)
        {
            int randomIndex = Random.Range(0, kartList.Count);
            KartAssetDefinition kart = (KartAssetDefinition)kartList[randomIndex];
            
            // 动态加载卡丁车资源
            GameObject kartObject = ResourceLoader.Instance.LoadKartAsset(kart.MainAsset);
            if (kartObject != null)
            {
                GameObject instantiated = UnityEngine.Object.Instantiate(kartObject);
                instantiated.transform.position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                this.instantiatedObjects_.Add(instantiated);
                
                Debug.Log(string.Format("Loaded Kart: {0} - Objects Count: {1}", kart.Name, this.instantiatedObjects_.Count));
            }
        }
    }
    
    private void TestCharacterAssetLoading()
    {
        List<AssetDefinition> characterList = CharacterAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        if (characterList.Count > 0)
        {
            int randomIndex = Random.Range(0, characterList.Count);
            CharacterAssetDefinition character = (CharacterAssetDefinition)characterList[randomIndex];
            
            // 动态加载角色资源
            GameObject characterObject = ResourceLoader.Instance.LoadCharacterAsset(character.MainAsset);
            if (characterObject != null)
            {
                GameObject instantiated = UnityEngine.Object.Instantiate(characterObject);
                instantiated.transform.position = new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f));
                this.instantiatedObjects_.Add(instantiated);
                
                Debug.Log(string.Format("Loaded Character: {0} - Objects Count: {1}", character.Name, this.instantiatedObjects_.Count));
            }
        }
    }
    
    private void TestTrackAssetLoading()
    {
        List<AssetDefinition> trackList = TrackAssetDefinitionManager.Instance.GetAssetDefinitionList();
        
        if (trackList.Count > 0)
        {
            int randomIndex = Random.Range(0, trackList.Count);
            TrackAssetDefinition track = (TrackAssetDefinition)trackList[randomIndex];
            
            // 加载赛道小件（不加载完整赛道以避免过多内存占用）
            GameObject trackProp = ResourceLoader.Instance.LoadTrackProp(track.MainAsset);
            if (trackProp != null)
            {
                GameObject instantiated = UnityEngine.Object.Instantiate(trackProp);
                instantiated.transform.position = new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                this.instantiatedObjects_.Add(instantiated);
                
                Debug.Log(string.Format("Loaded Track Prop: {0} - Objects Count: {1}", track.Name, this.instantiatedObjects_.Count));
            }
        }
    }
    
    private void TestEffectAssetLoading()
    {
        // 测试特效资源加载
        string[] effectNames = new string[]
        {
            "explosion_effect", "drift_effect", "speed_effect", "item_effect"
        };
        
        string randomEffect = effectNames[Random.Range(0, effectNames.Length)];
        GameObject effectObject = ResourceLoader.Instance.LoadEffectAsset(randomEffect);
        
        if (effectObject != null)
        {
            GameObject instantiated = UnityEngine.Object.Instantiate(effectObject);
            instantiated.transform.position = new Vector3(Random.Range(-5f, 5f), 2, Random.Range(-5f, 5f));
            this.instantiatedObjects_.Add(instantiated);
            
            Debug.Log(string.Format("Loaded Effect: {0} - Objects Count: {1}", randomEffect, this.instantiatedObjects_.Count));
        }
    }
    
    private void UpdateMemoryStatistics()
    {
        float currentMemory = this.GetCurrentMemoryUsage();
        
        if (currentMemory > this.peakMemory_)
        {
            this.peakMemory_ = currentMemory;
        }
        
        float memoryIncrease = currentMemory - this.initialMemory_;
        
        Debug.Log(string.Format("Memory Stats - Current: {0:F2} MB, Peak: {1:F2} MB, Increase: {2:F2} MB", 
            currentMemory, this.peakMemory_, memoryIncrease));
        
        // 如果内存增长过多，清理一些对象
        if (memoryIncrease > 100f) // 100MB限制
        {
            this.CleanupOldObjects();
        }
    }
    
    private float GetCurrentMemoryUsage()
    {
        return (float)System.GC.GetTotalMemory(false) / (1024f * 1024f); // 转换为MB
    }
    
    private void CleanupOldObjects()
    {
        int objectsToRemove = Mathf.Min(10, this.instantiatedObjects_.Count / 2);
        
        for (int i = 0; i < objectsToRemove; i++)
        {
            if (this.instantiatedObjects_.Count > 0)
            {
                GameObject objectToDestroy = this.instantiatedObjects_[0];
                this.instantiatedObjects_.RemoveAt(0);
                UnityEngine.Object.DestroyImmediate(objectToDestroy);
            }
        }
        
        // 强制垃圾回收
        System.GC.Collect();
        
        Debug.Log(string.Format("Cleaned up {0} objects - Remaining: {1}", objectsToRemove, this.instantiatedObjects_.Count));
    }
    
    private void StopMemoryTest()
    {
        this.CancelInvoke("RunMemoryTest");
        
        float finalMemory = this.GetCurrentMemoryUsage();
        float totalIncrease = finalMemory - this.initialMemory_;
        float peakIncrease = this.peakMemory_ - this.initialMemory_;
        
        Debug.Log("=== Memory Test Completed ===");
        Debug.Log(string.Format("Initial Memory: {0:F2} MB", this.initialMemory_));
        Debug.Log(string.Format("Final Memory: {0:F2} MB", finalMemory));
        Debug.Log(string.Format("Peak Memory: {0:F2} MB", this.peakMemory_));
        Debug.Log(string.Format("Total Increase: {0:F2} MB", totalIncrease));
        Debug.Log(string.Format("Peak Increase: {0:F2} MB", peakIncrease));
        Debug.Log(string.Format("Objects Created: {0}", this.currentTestIndex_));
        Debug.Log(string.Format("Objects Remaining: {0}", this.instantiatedObjects_.Count));
    }
}
```

#### 26. MemoryCheckStagePrefab.cs - 预制体内存测试
**文件位置**: `/Core/Stages/MemoryCheckStagePrefab.cs`
**功能概述**: 测试预制体资源的内存使用和性能

**预制体性能测试**:
```csharp
public class MemoryCheckStagePrefab : MonoBehaviourStage
{
    public GameObject[] testPrefabs_;
    public Shader[] performanceShaders_;
    
    private List<GameObject> instantiatedPrefabs_ = new List<GameObject>();
    private int currentPrefabIndex_ = 0;
    private float startTime_;
    
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(527);
        
        this.startTime_ = Time.realtimeSinceStartup;
        
        // 开始预制体测试
        this.StartPrefabTest();
    }
    
    private void StartPrefabTest()
    {
        if (this.testPrefabs_ != null && this.testPrefabs_.Length > 0)
        {
            Debug.Log(string.Format("Prefab Test Started - {0} prefabs to test", this.testPrefabs_.Length));
            
            // 启动测试循环
            this.InvokeRepeating("TestNextPrefab", 0.5f, 0.5f);
        }
        else
        {
            Debug.LogError("No test prefabs assigned!");
        }
    }
    
    private void TestNextPrefab()
    {
        if (this.currentPrefabIndex_ < this.testPrefabs_.Length)
        {
            GameObject prefab = this.testPrefabs_[this.currentPrefabIndex_];
            
            if (prefab != null)
            {
                // 测试原始预制体
                this.TestPrefabInstantiation(prefab, false);
                
                // 测试优化版本（替换着色器）
                this.TestPrefabInstantiation(prefab, true);
            }
            
            this.currentPrefabIndex_++;
        }
        else
        {
            // 测试完成
            this.CompletePrefabTest();
        }
    }
    
    private void TestPrefabInstantiation(GameObject prefab, bool optimized)
    {
        float startTime = Time.realtimeSinceStartup;
        
        // 创建多个实例进行压力测试
        int instanceCount = 10;
        List<GameObject> instances = new List<GameObject>();
        
        for (int i = 0; i < instanceCount; i++)
        {
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.transform.position = new Vector3(
                Random.Range(-20f, 20f),
                Random.Range(0f, 5f),
                Random.Range(-20f, 20f)
            );
            
            if (optimized)
            {
                // 应用性能优化着色器
                this.ApplyPerformanceShaders(instance);
                instance.name = prefab.name + "_Optimized_" + i;
            }
            else
            {
                instance.name = prefab.name + "_Original_" + i;
            }
            
            instances.Add(instance);
            this.instantiatedPrefabs_.Add(instance);
        }
        
        float endTime = Time.realtimeSinceStartup;
        float instantiationTime = endTime - startTime;
        
        // 计算渲染性能
        float frameTime = this.MeasureFrameTime();
        
        Debug.Log(string.Format("Prefab Test: {0} ({1}) - Instantiation: {2:F3}s, Frame Time: {3:F3}ms, Instances: {4}",
            prefab.name,
            optimized ? "Optimized" : "Original",
            instantiationTime,
            frameTime,
            instanceCount
        ));
    }
    
    private void ApplyPerformanceShaders(GameObject gameObject)
    {
        if (this.performanceShaders_ != null && this.performanceShaders_.Length > 0)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material != null)
                {
                    // 选择合适的性能着色器
                    Shader performanceShader = this.SelectPerformanceShader(renderer.material.shader);
                    
                    if (performanceShader != null)
                    {
                        renderer.material.shader = performanceShader;
                    }
                }
            }
        }
    }
    
    private Shader SelectPerformanceShader(Shader originalShader)
    {
        // 根据原始着色器选择对应的性能版本
        string shaderName = originalShader.name.ToLower();
        
        if (shaderName.Contains("diffuse"))
        {
            return this.FindShaderByName("Mobile/Diffuse");
        }
        else if (shaderName.Contains("bumped"))
        {
            return this.FindShaderByName("Mobile/Bumped Diffuse");
        }
        else if (shaderName.Contains("transparent"))
        {
            return this.FindShaderByName("Mobile/Transparent Diffuse");
        }
        else
        {
            // 默认使用最简单的着色器
            return this.FindShaderByName("Mobile/Unlit (Supports Lightmap)");
        }
    }
    
    private Shader FindShaderByName(string shaderName)
    {
        foreach (Shader shader in this.performanceShaders_)
        {
            if (shader.name == shaderName)
            {
                return shader;
            }
        }
        return null;
    }
    
    private float MeasureFrameTime()
    {
        // 测量几帧的平均渲染时间
        float totalTime = 0f;
        int frameCount = 5;
        
        for (int i = 0; i < frameCount; i++)
        {
            float frameStart = Time.realtimeSinceStartup;
            
            // 等待一帧
            yield return null;
            
            float frameEnd = Time.realtimeSinceStartup;
            totalTime += (frameEnd - frameStart) * 1000f; // 转换为毫秒
        }
        
        return totalTime / frameCount;
    }
    
    private void CompletePrefabTest()
    {
        this.CancelInvoke("TestNextPrefab");
        
        float totalTime = Time.realtimeSinceStartup - this.startTime_;
        
        Debug.Log("=== Prefab Test Completed ===");
        Debug.Log(string.Format("Total Test Time: {0:F2} seconds", totalTime));
        Debug.Log(string.Format("Prefabs Tested: {0}", this.testPrefabs_.Length));
        Debug.Log(string.Format("Total Instances Created: {0}", this.instantiatedPrefabs_.Count));
        
        // 清理测试对象
        this.CleanupTestObjects();
    }
    
    private void CleanupTestObjects()
    {
        foreach (GameObject instance in this.instantiatedPrefabs_)
        {
            if (instance != null)
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
        
        this.instantiatedPrefabs_.Clear();
        
        // 强制垃圾回收
        System.GC.Collect();
        
        Debug.Log("Test objects cleaned up");
    }
}
```

### 消息ID定义文件

#### 27-30. 消息ID常量文件
**文件列表**:
- `LoadingStageMID.cs` - 加载阶段消息ID
- `LoadingStageMID2.cs` - 加载阶段消息ID（扩展）
- `SingleModeStageMID.cs` - 单人模式消息ID
- `WaitRoomStageMID.cs` - 等待房间消息ID

**设计目的**: 为不同阶段定义专用的消息标识符，确保消息路由的正确性

#### 31. DirectLoadStage.cs - 直接加载测试阶段
**文件位置**: `/Core/Stages/DirectLoadStage.cs`
**功能概述**: 提供绕过菜单系统的直接游戏启动功能

**快速启动系统**:
```csharp
public class DirectLoadStage : MonoBehaviourStage
{
    protected override void Start()
    {
        base.Start();
        this.RegistMonoBehaviour(528);
        
        // 直接初始化所有必要的管理器
        this.InitializeGameManagers();
        
        // 设置默认玩家配置
        this.SetupDefaultPlayer();
        
        // 加载默认资源
        this.LoadDefaultResources();
        
        // 直接启动游戏
        this.StartGameDirectly();
    }
    
    private void InitializeGameManagers()
    {
        // 快速初始化核心管理器
        TrackAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("trackdefinition"));
        KartAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("kartdefinition"));
        CharacterAssetDefinitionManager.Instance.Initialize((TextAsset)Resources.Load("characterdefinition"));
        MaterialManager.Instance.Initialize();
        
        // 跳过复杂的用户配置加载，使用默认值
        KartOptions.Instance.LoadDefaultSettings();
        
        MonoBehaviourMessageFactory.Instance.Initialize();
        Statistics.Instance.Initialize();
        
        Debug.Log("Direct Load: Core managers initialized");
    }
    
    private void SetupDefaultPlayer()
    {
        // 硬编码的开发者设置
        Facebook.Inst.SetDeveloperName("sublee");
        
        // 设置默认游戏配置
        KartOptions.Instance.Track = 0;     // 第一个可用赛道
        KartOptions.Instance.Kart = 0;      // 第一个可用卡丁车
        KartOptions.Instance.Character = 0; // 第一个可用角色
        KartOptions.Instance.GameMode = 0;  // 单人竞速模式
        
        Debug.Log("Direct Load: Default player configuration set");
    }
    
    private void LoadDefaultResources()
    {
        // 预加载默认赛道资源
        string defaultTrackAsset = TrackAssetDefinitionManager.Instance.GetMainAsset(0);
        ResourceLoader.Instance.PreloadAsset(defaultTrackAsset);
        
        // 预加载默认卡丁车资源
        string defaultKartAsset = KartAssetDefinitionManager.Instance.GetMainAsset(0);
        ResourceLoader.Instance.PreloadAsset(defaultKartAsset);
        
        Debug.Log("Direct Load: Default resources preloaded");
    }
    
    private void StartGameDirectly()
    {
        // 设置游戏参数
        KartManager.Instance.parameter_.gameMode_ = GameMode.SINGLE_SPEED;
        KartManager.Instance.parameter_.track_ = 0;
        KartManager.Instance.parameter_.maxLap_ = 3;
        
        // 直接切换到游戏阶段
        StageController.Instance.ChangeStage(StageType.GAME);
        
        Debug.Log("Direct Load: Game started directly");
    }
}
```

## 总结

Core文件夹实现了一个完整的游戏架构系统，具有以下特点：

### 1. **架构设计模式**
- **状态机模式**: StageController管理游戏状态转换
- **单例模式**: 核心管理器确保全局唯一实例
- **模板方法模式**: BaseWaitRoomStage定义网络房间的通用流程
- **观察者模式**: MonoBehaviourMessage系统实现组件间解耦通信
- **策略模式**: 不同平台和模式的具体实现策略

### 2. **功能完整性**
- **游戏生命周期管理**: 从启动到结束的完整流程控制
- **多人网络支持**: 完整的P2P网络游戏架构
- **资源管理系统**: 动态加载、预加载和内存优化
- **社交系统集成**: Facebook登录、好友、排行榜
- **开发工具支持**: 完整的调试和测试工具集

### 3. **平台兼容性**
- **多平台支持**: iOS、Android、桌面版
- **设备适配**: 不同设备的性能和控制器适配
- **网络环境适应**: 离线、在线模式的无缝切换

### 4. **性能优化**
- **内存管理**: 垃圾回收优化和资源释放
- **渲染优化**: 着色器级别的性能调优
- **网络优化**: 自适应同步频率和数据压缩

### 5. **可扩展性**
- **模块化设计**: 每个阶段独立实现，便于扩展
- **接口定义**: NetStage等接口支持功能扩展
- **配置系统**: 灵活的参数配置和用户设置

这个核心系统为整个卡丁车游戏提供了坚实的技术基础，支持复杂的游戏功能和未来的功能扩展。