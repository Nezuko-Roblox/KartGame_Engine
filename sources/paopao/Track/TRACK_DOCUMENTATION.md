# Track 模块详细功能文档

## 概述

Track 模块是卡丁车游戏的赛道系统核心，负责赛道路径定义、通过点检测、圈数统计、排名计算、玩家重生等功能。该模块构建了一个完整的赛道管理系统，确保比赛的公平性和准确性。

## 模块结构

```
Track/
├── ApproachObject.cs          - 接近对象管理器
├── GoCourse.cs               - 赛道核心管理类
├── PassPlane.cs              - 通过平面定义
├── PassPlaneSequence.cs      - 通过平面序列管理
├── PassPlaneSequenceElement.cs - 通过平面序列元素
├── PassingLog.cs             - 通过记录日志
└── PassingLogElem.cs         - 通过记录元素
```

## 系统架构图

```
GoCourse (赛道核心)
    ├── PassPlane[] (通过点数组)
    ├── PassPlaneSequence (通过点序列)
    │   └── PassPlaneSequenceElement[] (序列元素)
    ├── SectionInfo[] (玩家区段信息)
    ├── PassingLog (通过记录)
    │   └── PassingLogElem[] (记录元素)
    └── ApproachObject[] (接近对象)
```

## 核心类详细分析

### 1. GoCourse.cs - 赛道核心管理类

**功能概述：**
赛道系统的核心管理类，负责整个比赛过程中的路径检测、圈数统计、排名计算、重生机制等功能。

**核心数据结构：**
```csharp
public class GoCourse
{
    public PassPlane[] passPlane_;                    // 通过平面数组
    public PassPlaneSequence passPlaneSequence_;     // 通过平面序列
    public SectionInfo[] sectionInfo_;               // 玩家区段信息（最多6个玩家）
    public int maxLap_ = 2;                          // 最大圈数
    public RegenInfo[] startInfos_;                   // 起始位置信息
    
    private float[,] passPlaneTrackDistanceDictionary; // 通过点间距离字典
    private float[] passPlaneTrackDistance;           // 通过点到起点的距离
}
```

#### 1.1 赛道初始化和解析

**文本格式解析：**
```csharp
public void ParsePlaneInfo(TextAsset planeInfo)
{
    StringReader reader = new StringReader(planeInfo.text);
    if (reader != null)
    {
        // 读取通过点数量
        string line = reader.ReadLine();
        int planeCount = int.Parse(line);
        this.passPlane_ = new PassPlane[planeCount];
        
        // 逐个解析通过点
        for (int i = 0; i < planeCount; i++)
        {
            line = reader.ReadLine();
            if (line != null)
            {
                this.passPlane_[i] = new PassPlane(line);
            }
        }
    }
    reader.Close();
}
```

**二进制格式解析：**
```csharp
public void ParsePlaneInfo(BinaryAsset planeInfo)
{
    BinaryReader reader = new BinaryReader(new MemoryStream(planeInfo.content_));
    if (reader != null)
    {
        int currentPos = 0;
        int totalLength = (int)reader.BaseStream.Length;
        
        reader.ReadInt16(); // 版本号
        currentPos += 2;
        
        int planeCount = (int)reader.ReadInt16();
        currentPos += 2;
        
        this.passPlane_ = new PassPlane[planeCount];
        for (int i = 0; i < planeCount; i++)
        {
            this.passPlane_[i] = new PassPlane(ref reader);
            currentPos += 72; // 每个通过点72字节
        }
        
        reader.Close();
    }
}
```

#### 1.2 起始位置生成系统

**随机起始位置生成：**
```csharp
public List<int> GenerateStartInfos()
{
    List<int> randomList;
    FiaUtil.GetRandomList(0, 5, out randomList); // 生成0-5的随机排列
    this.GenerateStartInfos(randomList);
    return randomList;
}

public void GenerateStartInfos(List<int> randomList)
{
    this.startInfos_ = new RegenInfo[6];
    
    // 获取起点和终点的重生信息
    RegenInfo startRegen = this.GetRegenInfo(0);
    RegenInfo endRegen = this.GetRegenInfo(this.passPlane_.Length - 1);
    
    // 计算横向偏移向量
    Vector3 lateralOffset = Vector3.Cross(startRegen.direction_, Vector3.up);
    
    // 根据玩家数量调整间距
    float interval = (randomList.Count <= 4) ? 4f : 2.6f;
    
    // 计算起始中心位置
    Vector3 centerPos = (startRegen.position_ + endRegen.position_) / 2f + 
                       lateralOffset * interval * -((float)randomList.Count * 0.5f - 0.5f);
    
    // 为每个玩家分配起始位置
    int playerIndex = 0;
    foreach (int slotIndex in randomList)
    {
        this.startInfos_[playerIndex] = new RegenInfo();
        this.startInfos_[playerIndex].direction_ = startRegen.direction_;
        this.startInfos_[playerIndex].position_ = centerPos + (float)slotIndex * interval * lateralOffset;
        playerIndex++;
    }
}
```

#### 1.3 实时位置跟踪和通过点检测

**核心更新循环：**
```csharp
public void Update()
{
    for (int kartIndex = 0; kartIndex < 6; kartIndex++)
    {
        if (this.sectionInfo_[kartIndex] != null && !this.IsKartGoalIn(kartIndex))
        {
            GameObject kart = this.sectionInfo_[kartIndex].kart_.m_kart;
            Vector3 currentPos = kart.transform.position;
            
            // 初始化上一帧位置
            if (this.sectionInfo_[kartIndex].lastKartPos_ == Vector3.zero)
            {
                this.sectionInfo_[kartIndex].lastKartPos_ = currentPos;
            }
            
            // 检查移动距离是否足够触发检测
            if (Vector3.Distance(this.sectionInfo_[kartIndex].lastKartPos_, currentPos) >= 0.5f)
            {
                this.ProcessPassPlaneDetection(kartIndex, currentPos);
                this.UpdateKartDistance(kartIndex);
                this.CheckForReset(kartIndex, currentPos);
                
                this.sectionInfo_[kartIndex].lastKartPos_ = currentPos;
            }
        }
    }
    this.CalcRank(); // 计算所有玩家排名
}
```

**通过点检测算法：**
```csharp
private void ProcessPassPlaneDetection(int kartIndex, Vector3 currentPos)
{
    bool detected = false;
    int currentPlane = this.sectionInfo_[kartIndex].latestPassingPlane_;
    int[] nextPlanes;
    
    // 根据当前状态确定要检测的通过点
    if (currentPlane == -1 || this.sectionInfo_[kartIndex].passCorrect_)
    {
        nextPlanes = this.passPlaneSequence_.GetNext(currentPlane);
    }
    else
    {
        nextPlanes = this.passPlaneSequence_.GetNext(this.passPlaneSequence_.GetPrev(currentPlane)[0]);
    }
    
    // 检测前进方向的通过点
    for (int i = 0; i < nextPlanes.Length && !detected; i++)
    {
        int planeIndex = nextPlanes[i];
        int passingResult = this.CheckPlanePassing(
            this.passPlane_[planeIndex], 
            this.sectionInfo_[kartIndex].lastKartPos_, 
            currentPos
        );
        
        if (passingResult > 0) // 正向通过
        {
            this.ProcessCorrectPassing(kartIndex, planeIndex);
            detected = true;
        }
    }
    
    // 如果没有检测到前进，检查后退方向
    if (!detected)
    {
        this.ProcessReversePassing(kartIndex, currentPos);
    }
}
```

#### 1.4 圈数和排名计算系统

**圈数更新逻辑：**
```csharp
private void ProcessCorrectPassing(int kartIndex, int planeIndex)
{
    this.sectionInfo_[kartIndex].latestPassingPlane_ = planeIndex;
    this.sectionInfo_[kartIndex].passCorrect_ = true;
    
    // 检查是否为正确方向的连续通过
    if (this.passPlaneSequence_.IsNextPlane(
        this.sectionInfo_[kartIndex].lastCorrectDirectionPassingPlane_, 
        planeIndex) || 
        this.passPlaneSequence_.IsExclusivePlane(
        this.sectionInfo_[kartIndex].lastCorrectDirectionPassingPlane_, 
        planeIndex))
    {
        // 处理第一圈的开始
        if (this.sectionInfo_[kartIndex].lap_ == 0 && 
            this.passPlaneSequence_.IsFirstPlane(planeIndex))
        {
            this.sectionInfo_[kartIndex].lap_++;
            this.sectionInfo_[kartIndex].lastCorrectDirectionPassingPlane_ = planeIndex;
        }
        else
        {
            this.sectionInfo_[kartIndex].lastCorrectDirectionPassingPlane_ = planeIndex;
            
            // 检查是否完成一圈
            if (this.passPlaneSequence_.IsLastPlane(planeIndex))
            {
                // 记录圈数时间
                this.sectionInfo_[kartIndex].lapTime_[this.sectionInfo_[kartIndex].lap_ - 1] = 
                    KartManager.Instance.GetPlayTime();
                
                this.sectionInfo_[kartIndex].lap_++;
                this.sectionInfo_[kartIndex].lastCorrectDirectionPassingPlane_ = -1;
            }
        }
    }
}
```

**排名计算算法：**
```csharp
private void CalcRank()
{
    for (int i = 0; i < 6; i++)
    {
        if (this.sectionInfo_[i] != null)
        {
            // 计算排名值
            if (this.IsKartGoalIn(i))
            {
                // 已完成比赛的玩家：基于完成时间排名
                this.sectionInfo_[i].rankValue = 
                    (double)((long)((this.maxLap_ + 1) * 1000) << 32) - 
                    (double)this.GetFinishTime(i);
            }
            else if (KartManager.Instance.parameter_.Stage == StageType.GAME_WIFI && 
                     i != KartManager.PLAYER_KART_IDX)
            {
                // 网络玩家：使用网络排名值
                this.sectionInfo_[i].rankValue = this.sectionInfo_[i].networkRankValue;
            }
            else
            {
                // 进行中的玩家：基于圈数和位置排名
                int currentLap = this.sectionInfo_[i].lap_;
                if (this.sectionInfo_[i].lastCorrectDirectionPassingPlane_ < 
                    this.sectionInfo_[i].latestPassingPlane_)
                {
                    currentLap--;
                }
                
                this.sectionInfo_[i].rankValue = 
                    (double)((long)(currentLap * 1000 + this.sectionInfo_[i].distancePlane_) << 32) + 
                    (double)this.sectionInfo_[i].distance_;
            }
            
            // 计算相对排名
            this.sectionInfo_[i].rank_ = 0;
            for (int j = 0; j < i; j++)
            {
                if (this.sectionInfo_[j] != null)
                {
                    if (this.sectionInfo_[i].rankValue <= this.sectionInfo_[j].rankValue)
                    {
                        this.sectionInfo_[i].rank_++;
                    }
                    else
                    {
                        this.sectionInfo_[j].rank_++;
                    }
                }
            }
        }
    }
}
```

#### 1.5 碰撞检测和重生系统

**碰撞检测：**
```csharp
private void CheckForReset(int kartIndex, Vector3 currentPos)
{
    PlayerType playerType = KartManager.Instance.parameter_.kart_[kartIndex].type_;
    
    // 只对AI和玩家进行碰撞检测（不包括幽灵和网络玩家）
    if ((playerType == PlayerType.AI || playerType == PlayerType.PLAYER) && 
        Physics.Linecast(this.sectionInfo_[kartIndex].lastKartPos_, currentPos, 4096))
    {
        this.sectionInfo_[kartIndex].needReset_ = true;
    }
}
```

**重生系统：**
```csharp
public void ResetKart(int kartIndex)
{
    // 获取重生位置信息
    RegenInfo regenInfo = this.GetRegenInfo(this.sectionInfo_[kartIndex].latestPassingPlane_);
    
    // 发送传送消息
    MonoBehaviourExCenter.Instance.SendMessage(
        0, 
        this.sectionInfo_[kartIndex].kart_.controller_.id_, 
        ((WarpMessage)MonoBehaviourMessageFactory.Instance.GetMessage(MonoBehaviourMessageType.WARP))
        .Initialize(
            regenInfo.position_, 
            Quaternion.LookRotation(regenInfo.direction_, Vector3.up), 
            true, 
            true
        )
    );
    
    // 更新通过状态
    int currentPlane = this.sectionInfo_[kartIndex].latestPassingPlane_;
    if (currentPlane != -1)
    {
        this.sectionInfo_[kartIndex].passCorrect_ = 
            this.CheckPlanePassing(
                this.passPlane_[currentPlane], 
                regenInfo.position_, 
                regenInfo.position_ + regenInfo.direction_
            ) <= 0;
        
        this.sectionInfo_[kartIndex].lastKartPos_ = regenInfo.position_;
    }
}
```

#### 1.6 逆行检测系统

**逆行检测算法：**
```csharp
public bool IsWrongWay()
{
    GoKart playerKart = this.sectionInfo_[KartManager.PLAYER_KART_IDX].kart_;
    int currentPlane = this.sectionInfo_[KartManager.PLAYER_KART_IDX].latestPassingPlane_;
    
    // 基础条件检查
    if (playerKart == null) return false;
    if (playerKart.m_KartWLVel.magnitude <= 5f) return false; // 速度太低不判断
    if (currentPlane == -1) return false; // 没有通过点信息
    
    // 获取正确行驶方向
    Vector3 correctDirection = this.passPlane_[currentPlane].toNextPlane_.normalized;
    
    // 检查车头方向和速度方向是否都与正确方向相反
    bool headingWrong = Vector3.Dot(playerKart.m_kart.transform.forward, correctDirection) <= -0.5f;
    bool velocityWrong = Vector3.Dot(playerKart.m_KartWLVel.normalized, correctDirection) <= -0.5f;
    
    return headingWrong && velocityWrong;
}
```

### 2. PassPlane.cs - 通过平面定义

**功能概述：**
定义赛道中的通过检测平面，每个平面由两个三角形组成，用于检测卡丁车是否通过特定区域。

**数据结构：**
```csharp
public class PassPlane
{
    public const int TOKEN_NO = 18;              // 数据标记数量
    private const int VERTEX_NO = 4;             // 顶点数量
    
    public Vector3[] pt1_ = new Vector3[3];      // 第一个三角形的3个顶点
    public Vector3[] pt2_ = new Vector3[3];      // 第二个三角形的3个顶点
    public Vector3 normal_;                      // 平面法向量
    public Vector3 toNextPlane_;                 // 指向下一个平面的方向
    public Vector3 centerPos_ = Vector3.zero;   // 平面中心位置
    
    private bool isRegenPosSetting_;             // 重生位置是否已设置
    public Vector3 regenPos_ = Vector3.zero;    // 重生位置
    private string toString_;                    // 字符串表示
}
```

**初始化和解析：**
```csharp
private void Initialize(float[] tokenF)
{
    if (tokenF.Length != 18) return;
    
    // 设置两个三角形的顶点
    this.SetVector(ref this.pt1_, tokenF, 0, 1, 2);  // 顶点0,1,2
    this.SetVector(ref this.pt2_, tokenF, 0, 2, 3);  // 顶点0,2,3
    
    // 设置法向量和方向向量
    this.SetVector(ref this.normal_, tokenF, 4);
    this.SetVector(ref this.toNextPlane_, tokenF, 5);
    
    // 计算中心位置（四个顶点的平均值）
    for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            this.centerPos_[j] += tokenF[i * 3 + j];
        }
    }
    this.centerPos_ /= 4f;
}

public void SetVector(ref Vector3[] vertices, float[] data, int v1, int v2, int v3)
{
    int[] indices = { v1, v2, v3 };
    for (int i = 0; i < 3; i++)
    {
        this.SetVector(ref vertices[i], data, indices[i]);
    }
}

public void SetVector(ref Vector3 vector, float[] data, int index)
{
    int startIdx = index * 3;
    for (int i = 0; i < 3; i++)
    {
        vector[i] = data[startIdx + i];
    }
}
```

**重生位置计算：**
```csharp
public Vector3 GetRegenPos()
{
    if (this.isRegenPosSetting_)
    {
        return this.regenPos_;
    }
    
    // 使用射线检测找到地面位置
    RaycastHit hitInfo;
    float maxDistance = Mathf.Abs(this.centerPos_.y - this.pt1_[0].y);
    
    if (Physics.Raycast(this.centerPos_, -Vector3.up, out hitInfo, maxDistance, 256))
    {
        this.regenPos_ = hitInfo.point; // 使用射线命中点
    }
    else
    {
        this.regenPos_ = this.centerPos_; // 使用中心位置作为备选
    }
    
    this.isRegenPosSetting_ = true;
    return this.regenPos_;
}

public bool GetRegenPos(ref Vector3 regenPos)
{
    RaycastHit hitInfo;
    float maxDistance = Mathf.Abs(this.centerPos_.y - this.pt1_[0].y);
    
    if (Physics.Raycast(this.centerPos_, -Vector3.up, out hitInfo, maxDistance, 256))
    {
        regenPos = hitInfo.point;
        return true; // 成功找到地面
    }
    
    regenPos = this.centerPos_;
    return false; // 使用默认位置
}
```

### 3. PassPlaneSequence.cs - 通过平面序列管理

**功能概述：**
管理通过平面之间的连接关系，支持分支和合并路径，用于复杂赛道布局。

**核心数据结构：**
```csharp
public class PassPlaneSequence
{
    private PassPlaneSequenceElement[] sequence_;  // 序列元素数组
    private int[] startPassingPlane_;              // 起始通过点
    private int[] endPassingPlane_;                // 结束通过点
}
```

**简单序列构造（环形赛道）：**
```csharp
public PassPlaneSequence(int length)
{
    this.sequence_ = new PassPlaneSequenceElement[length];
    
    // 创建环形连接：每个点连接前一个和后一个
    for (int i = 0; i < length; i++)
    {
        int prevIndex = (i - 1 + length) % length;  // 前一个点（环形）
        int nextIndex = (i + 1) % length;           // 后一个点（环形）
        
        this.sequence_[i] = new PassPlaneSequenceElement(prevIndex, nextIndex);
    }
    
    this.startPassingPlane_ = new int[1] { 0 };          // 起点为0
    this.endPassingPlane_ = new int[] { length - 1 };    // 终点为最后一个
    
    this.GenerateBranchId(); // 生成分支ID
}
```

**复杂序列构造（支持分支）：**
```csharp
public PassPlaneSequence(TextAsset sequenceInfo)
{
    StringReader reader = new StringReader(sequenceInfo.text);
    if (reader != null)
    {
        string line = reader.ReadLine();
        int elementCount = int.Parse(line);
        this.sequence_ = new PassPlaneSequenceElement[elementCount];
        
        for (int i = 0; i < elementCount; i++)
        {
            line = reader.ReadLine();
            if (line != null)
            {
                char[] separators = { ' ', '\t' };
                string[] tokens = line.Split(separators);
                
                if (tokens.Length == 3)
                {
                    int index = int.Parse(tokens[0]);
                    
                    // 解析前驱和后继节点
                    char[] slashSeparator = { '/' };
                    string[] prevNodes = tokens[1].Split(slashSeparator, StringSplitOptions.RemoveEmptyEntries);
                    string[] nextNodes = tokens[2].Split(slashSeparator, StringSplitOptions.RemoveEmptyEntries);
                    
                    this.sequence_[index] = new PassPlaneSequenceElement(prevNodes, nextNodes);
                }
            }
        }
    }
    reader.Close();
    
    this.startPassingPlane_ = new int[1] { 0 };
    this.endPassingPlane_ = new int[] { this.sequence_.Length - 1 };
    this.GenerateBranchId();
}
```

**分支ID生成系统：**
```csharp
private void GenerateBranchIdRecursive(int planeIndex, BranchIdElem[] elements, int elementCount)
{
    PassPlaneSequenceElement element = this.sequence_[planeIndex];
    
    // 避免重复处理
    if (element.branchId_ != null) return;
    
    // 处理合并点
    if (element.IsMerge())
    {
        elementCount--;
    }
    
    // 创建分支ID
    element.branchId_ = new BranchId(elements, elementCount);
    
    // 处理分支点
    if (element.IsBranch())
    {
        elements[elementCount].node_ = (byte)planeIndex;
        elementCount++;
    }
    
    // 递归处理后继节点
    for (int i = 0; i < element.Next.Length; i++)
    {
        if (element.IsBranch())
        {
            elements[elementCount - 1].branch_ = (byte)element.Next[i];
        }
        this.GenerateBranchIdRecursive(element.Next[i], elements, elementCount);
    }
}
```

**路径查询接口：**
```csharp
public int[] GetNext(int passPlaneIndex)
{
    if (!MathHelper.IsBetweenIE(passPlaneIndex, 0, this.sequence_.Length))
    {
        return this.startPassingPlane_; // 返回起始点
    }
    return this.sequence_[passPlaneIndex].Next;
}

public int[] GetPrev(int passPlaneIndex)
{
    if (!MathHelper.IsBetweenIE(passPlaneIndex, 0, this.sequence_.Length))
    {
        return this.endPassingPlane_; // 返回结束点
    }
    return this.sequence_[passPlaneIndex].Prev;
}

public bool IsNextPlane(int prevPlane, int nextPlane)
{
    if (prevPlane == -1)
    {
        return this.IsFirstPlane(nextPlane);
    }
    
    int[] nextPlanes = this.sequence_[prevPlane].Next;
    foreach (int plane in nextPlanes)
    {
        if (nextPlane == plane) return true;
    }
    return false;
}
```

### 4. PassPlaneSequenceElement.cs - 通过平面序列元素

**功能概述：**
表示序列中的单个元素，包含前驱和后继节点信息，支持分支和合并判断。

**核心实现：**
```csharp
public class PassPlaneSequenceElement
{
    private int[] prev_;          // 前驱节点数组
    private int[] next_;          // 后继节点数组
    public BranchId branchId_;    // 分支标识
    
    // 构造函数 - 简单连接
    public PassPlaneSequenceElement(int prev, int next)
    {
        this.prev_ = new int[] { prev };
        this.next_ = new int[] { next };
    }
    
    // 构造函数 - 复杂连接
    public PassPlaneSequenceElement(string[] prevNodes, string[] nextNodes)
    {
        if (prevNodes.Length != 0 && nextNodes.Length != 0)
        {
            this.SetArray(prevNodes, out this.prev_);
            this.SetArray(nextNodes, out this.next_);
        }
    }
    
    // 字符串数组转整数数组
    public void SetArray(string[] stringArray, out int[] intArray)
    {
        intArray = new int[stringArray.Length];
        for (int i = 0; i < stringArray.Length; i++)
        {
            int.TryParse(stringArray[i], out intArray[i]);
        }
    }
    
    // 判断是否为分支点（多个后继）
    public bool IsBranch()
    {
        return this.next_ != null && this.next_.Length > 1;
    }
    
    // 判断是否为合并点（多个前驱）
    public bool IsMerge()
    {
        return this.prev_ != null && this.prev_.Length > 1;
    }
}
```

### 5. ApproachObject.cs - 接近对象管理器

**功能概述：**
管理可接近的游戏对象（如道具箱、加速带等），包含状态管理和动画控制。

**状态枚举：**
```csharp
public enum ApproachState
{
    NONE,        // 无状态
    ACTIVE,      // 激活状态
    EXPLORE,     // 探索中（动画播放中）
    EXPLORED,    // 已探索
    DESTROYED    // 已销毁
}

public enum ApproachAnim
{
    IDLE,        // 闲置动画
    EXPLORE      // 探索动画
}
```

**核心实现：**
```csharp
public class ApproachObject
{
    private GameObject root_;           // 根对象
    private GameObject idle_;           // 闲置状态对象
    private GameObject touched_;        // 触碰状态对象
    private GameObject touchedBubble_;  // 触碰气泡效果
    
    public ApproachObject(GameObject obj)
    {
        this.root_ = obj;
        this.ObjectSetting();
    }
    
    // 自动设置子对象引用
    private void ObjectSetting()
    {
        if (this.root_ == null) return;
        
        foreach (Transform child in this.root_.transform)
        {
            switch (child.name)
            {
                case "idle":
                    this.idle_ = child.gameObject;
                    break;
                case "touched":
                    this.touched_ = child.gameObject;
                    break;
                case "touched_bubble":
                    this.touchedBubble_ = child.gameObject;
                    break;
            }
        }
    }
    
    // 获取当前状态
    public ApproachState GetApproachState()
    {
        if (this.root_ == null) return ApproachState.NONE;
        if (!this.root_.active) return ApproachState.DESTROYED;
        
        if (this.idle_.active)
        {
            return ApproachState.ACTIVE;
        }
        
        if (this.touched_.active)
        {
            return this.touched_.animation.isPlaying ? 
                   ApproachState.EXPLORE : 
                   ApproachState.EXPLORED;
        }
        
        return ApproachState.DESTROYED;
    }
    
    // 设置状态
    public void SetApproachState(ApproachState state)
    {
        switch (state)
        {
            case ApproachState.ACTIVE:
                this.root_.active = true;
                this.idle_.SetActiveRecursively(true);
                this.touched_.SetActiveRecursively(false);
                this.touchedBubble_.SetActiveRecursively(false);
                break;
                
            case ApproachState.EXPLORE:
                this.root_.active = true;
                this.idle_.SetActiveRecursively(false);
                this.touched_.SetActiveRecursively(true);
                this.touchedBubble_.SetActiveRecursively(true);
                break;
                
            case ApproachState.DESTROYED:
                this.root_.SetActiveRecursively(false);
                break;
        }
    }
}
```

### 6. PassingLog.cs 和 PassingLogElem.cs - 通过记录系统

**功能概述：**
记录玩家通过各个检测点的详细信息，用于调试和分析。

**PassingLogElem - 记录元素：**
```csharp
public class PassingLogElem
{
    public Vector3 pos1_ = Vector3.zero;    // 起始位置
    public Vector3 pos2_ = Vector3.zero;    // 结束位置
    public int passingIndex_;               // 通过点索引
    public bool isCorrect_ = true;          // 是否正确通过
    public int resultPassing_;              // 通过结果（-1逆向，0未通过，1正向）
    
    public override string ToString()
    {
        return string.Format("{0} {1} {2} {3} {4} ", 
            this.passingIndex_,
            Vector3Helper.ToStringVector3(this.pos1_),
            Vector3Helper.ToStringVector3(this.pos2_),
            this.isCorrect_,
            this.resultPassing_
        );
    }
}
```

**PassingLog - 记录管理器：**
```csharp
public class PassingLog
{
    private static PassingLog instance_;
    private List<PassingLogElem> elems_ = new List<PassingLogElem>();
    
    public static PassingLog Instance
    {
        get
        {
            if (PassingLog.instance_ == null)
            {
                PassingLog.instance_ = new PassingLog();
            }
            return PassingLog.instance_;
        }
    }
    
    public void AddElem(PassingLogElem elem)
    {
        this.elems_.Add(elem);
    }
    
    public void Clear()
    {
        this.elems_.Clear();
    }
    
    public override string ToString()
    {
        string result = "\n";
        foreach (PassingLogElem elem in this.elems_)
        {
            result += elem.ToString() + "\n";
        }
        return result;
    }
}
```

## 系统工作流程

### 1. 赛道初始化流程
```
1. 解析通过点数据 (ParsePlaneInfo)
2. 创建通过点序列 (PassPlaneSequence)
3. 初始化距离表 (InitPassPlane)
4. 生成起始位置 (GenerateStartInfos)
5. 注册卡丁车 (SetKart)
```

### 2. 实时检测流程
```
每帧Update循环:
1. 检查卡丁车位置变化
2. 执行通过点检测算法
3. 更新圈数和通过状态
4. 计算排名
5. 检查重生需求
6. 记录调试日志
```

### 3. 通过点检测算法
```
1. 获取候选通过点列表
2. 使用射线-三角面相交检测
3. 判断通过方向（正向/逆向）
4. 更新玩家状态
5. 处理分支路径
6. 记录通过日志
```

## 性能优化策略

### 1. 距离计算优化
- 预计算通过点间距离
- 使用查找表避免重复计算
- 分块处理大型赛道

### 2. 检测频率优化
- 基于移动距离的条件检测
- 避免每帧处理静止对象
- 优化射线检测的层级掩码

### 3. 内存管理优化
- 复用PassingLogElem对象
- 限制日志记录数量
- 及时清理无效引用

## 扩展性设计

### 1. 支持复杂赛道布局
- 分支路径系统
- 合并点处理
- 多路径排名计算

### 2. 多人游戏支持
- 网络同步优化
- 独立的玩家状态管理
- 公平性保证机制

### 3. 调试和监控
- 详细的通过日志
- 可视化调试工具
- 性能监控接口

## 总结

Track模块构建了一个完整、可靠的赛道管理系统，具有以下特点：

1. **精确性**: 基于几何算法的精确通过点检测
2. **公平性**: 统一的排名计算和重生机制
3. **灵活性**: 支持复杂的分支赛道布局
4. **可扩展性**: 模块化设计支持功能扩展
5. **可调试性**: 完整的日志和监控系统

该系统为卡丁车游戏提供了坚实的赛道基础，确保了比赛的公平性和游戏体验的流畅性。