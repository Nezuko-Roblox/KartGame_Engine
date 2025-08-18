# Ranking 文件夹完整功能功档

## 概述

Ranking 文件夹包含了卡丁车游戏的完整排行榜和成绩管理系统，采用了模板方法模式、策略模式、命令模式和观察者模式的复合架构设计。该系统通过客户端-服务器分布式架构、XML数据持久化、增量同步和Facebook社交集成，为游戏提供了实时、可靠、高性能的排行榜功能。

## 系统架构

### 核心设计原则
- **模板方法模式**: 统一的排行榜操作框架
- **策略模式**: 不同排行榜类型的专门化实现
- **命令模式**: 异步更新操作的封装和执行
- **观察者模式**: 基于委托的更新通知机制
- **时间分片**: 月度重置和时间同步管理

### 系统组件分层
- **抽象层**: 通用排行榜接口和状态管理
- **实现层**: 全时和月度排行榜的具体实现
- **同步层**: 服务器通信和数据同步机制
- **持久层**: XML本地存储和缓存系统

## 核心架构系统

### 1. 排行榜基类系统 - Ranking.cs

#### 抽象排行榜框架
**功能概述**: 所有排行榜的基础框架，定义通用操作接口

**模板方法实现**:
```csharp
public abstract class Ranking : IXMLizable
{
    protected string fbid_;              // Facebook用户ID
    protected Dictionary<string, string> friends_; // 好友信息缓存
    protected string path_;              // 数据存储路径
    protected List<Record> Records;      // 成绩记录列表
    
    // 抽象属性，子类必须实现
    public abstract string LocalPath { get; }
    
    // 模板方法 - 数据加载流程
    public bool Load()
    {
        try
        {
            string filePath = this.LocalPath;
            
            if (!File.Exists(filePath))
            {
                Debug.Log($"Ranking file not found: {filePath}");
                this.InitializeDefaultRanking();
                return true;
            }
            
            // 解析XML数据
            string xmlContent = File.ReadAllText(filePath);
            XMLElement rootElement = XMLParser.Parse(xmlContent);
            
            this.FromXML(rootElement);
            
            Debug.Log($"Successfully loaded ranking from {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load ranking: {e.Message}");
            this.InitializeDefaultRanking();
            return false;
        }
    }
    
    // 模板方法 - 数据保存流程
    public virtual bool Save()
    {
        try
        {
            // 保存前排序
            this.SortRecords();
            
            // 生成XML
            XMLElement rootElement = this.ToXML();
            string xmlContent = rootElement.ToString();
            
            // 确保目录存在
            string directory = Path.GetDirectoryName(this.LocalPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // 写入文件
            File.WriteAllText(this.LocalPath, xmlContent);
            
            Debug.Log($"Successfully saved ranking to {this.LocalPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save ranking: {e.Message}");
            return false;
        }
    }
    
    // 排名计算
    public int CalcRank()
    {
        this.SortRecords();
        
        for (int i = 0; i < this.Records.Count; i++)
        {
            if (this.Records[i].fbid_ == this.fbid_)
            {
                return i + 1; // 排名从1开始
            }
        }
        
        return 0; // 未找到
    }
    
    // 获取个人最佳成绩
    public Record GetPersonalBest()
    {
        return this.Records.FirstOrDefault(r => r.fbid_ == this.fbid_);
    }
    
    // 获取前N名
    public List<Record> GetTopN(int count)
    {
        this.SortRecords();
        return this.Records.Take(count).ToList();
    }
    
    // 获取周围排名
    public List<Record> GetSurroundingRanks(int range = 5)
    {
        this.SortRecords();
        
        int playerIndex = this.Records.FindIndex(r => r.fbid_ == this.fbid_);
        if (playerIndex == -1)
        {
            return this.GetTopN(range * 2 + 1);
        }
        
        int startIndex = Math.Max(0, playerIndex - range);
        int endIndex = Math.Min(this.Records.Count - 1, playerIndex + range);
        int count = endIndex - startIndex + 1;
        
        return this.Records.GetRange(startIndex, count);
    }
    
    // 记录排序
    protected void SortRecords()
    {
        this.Records.Sort(new SortByTime());
    }
    
    // 初始化默认排行榜
    protected virtual void InitializeDefaultRanking()
    {
        this.Records = new List<Record>();
        
        // 为所有好友创建默认记录
        if (this.friends_ != null)
        {
            foreach (var friend in this.friends_)
            {
                float defaultTime = 1200f; // 默认时间
                
                // 玩家自己的默认时间稍微高一点，用于标识
                if (friend.Key == this.fbid_)
                {
                    defaultTime += 1f;
                }
                
                Record defaultRecord = new Record(
                    friend.Key,
                    friend.Value,
                    this.GetGameMode(),
                    defaultTime,
                    this.GetMap()
                );
                
                this.Records.Add(defaultRecord);
            }
        }
        
        this.SortRecords();
    }
    
    // 抽象方法，子类实现
    public abstract void FromXML(XMLElement xml);
    public abstract XMLElement ToXML();
    
    // 虚方法，子类可重写
    protected virtual GameMode GetGameMode() { return GameMode.SINGLE_ITEM; }
    protected virtual int GetMap() { return 0; }
}
```

**时间排序实现**:
```csharp
protected class SortByTime : Comparer<Record>
{
    public override int Compare(Record x, Record y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return 1;
        if (y == null) return -1;
        
        // 时间越短排名越高
        int timeComparison = x.time_.CompareTo(y.time_);
        
        if (timeComparison != 0)
        {
            return timeComparison;
        }
        
        // 时间相同时按名字排序
        return string.Compare(x.fbname_, y.fbname_, StringComparison.OrdinalIgnoreCase);
    }
}
```

### 2. 全时排行榜 - AllTimeRanking.cs

#### 持久化历史排行榜
**功能概述**: 永久保存的历史最佳成绩排行榜

**全时排行榜实现**:
```csharp
public class AllTimeRanking : Ranking
{
    private GameMode gameMode_;
    private int map_;
    
    public AllTimeRanking(string fbid, Dictionary<string, string> friends, 
                         GameMode gameMode, int map, string path)
    {
        this.fbid_ = fbid;
        this.friends_ = friends;
        this.gameMode_ = gameMode;
        this.map_ = map;
        this.path_ = path;
        this.Records = new List<Record>();
    }
    
    // 本地存储路径
    public override string LocalPath
    {
        get
        {
            string filename = $"{this.fbid_}_alltime_{(int)this.gameMode_}_{this.map_}.xml";
            return Path.Combine(this.path_, filename);
        }
    }
    
    // XML反序列化
    public override void FromXML(XMLElement xml)
    {
        this.Records.Clear();
        
        try
        {
            // 解析排行榜元数据
            this.gameMode_ = (GameMode)int.Parse(xml.getAttribute("game_mode"));
            this.map_ = int.Parse(xml.getAttribute("map"));
            
            // 解析记录列表
            XMLElement recordsElement = xml.getElement("records");
            if (recordsElement != null)
            {
                foreach (XMLElement recordElement in recordsElement.getElements("record"))
                {
                    Record record = new Record();
                    record.FromXML(recordElement);
                    this.Records.Add(record);
                }
            }
            
            Debug.Log($"Loaded {this.Records.Count} records for {this.gameMode_} mode on map {this.map_}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing AllTime ranking XML: {e.Message}");
            this.InitializeDefaultRanking();
        }
    }
    
    // XML序列化
    public override XMLElement ToXML()
    {
        XMLElement rootElement = new XMLElement("alltime_ranking");
        
        // 设置元数据
        rootElement.setAttribute("game_mode", ((int)this.gameMode_).ToString());
        rootElement.setAttribute("map", this.map_.ToString());
        rootElement.setAttribute("fbid", this.fbid_);
        rootElement.setAttribute("last_update", DateTime.UtcNow.ToString("o"));
        
        // 添加记录
        XMLElement recordsElement = new XMLElement("records");
        
        foreach (Record record in this.Records)
        {
            recordsElement.addElement(record.ToXML());
        }
        
        rootElement.addElement(recordsElement);
        
        return rootElement;
    }
    
    // 更新个人最佳成绩
    public bool UpdatePersonalBest(float newTime)
    {
        Record personalRecord = this.GetPersonalBest();
        
        if (personalRecord == null)
        {
            // 创建新记录
            string playerName = this.friends_.ContainsKey(this.fbid_) ? 
                this.friends_[this.fbid_] : "Player";
            
            personalRecord = new Record(this.fbid_, playerName, this.gameMode_, newTime, this.map_);
            this.Records.Add(personalRecord);
            
            Debug.Log($"Created new personal record: {newTime:F3}s");
            return true;
        }
        else if (newTime < personalRecord.time_)
        {
            // 更新现有记录
            float oldTime = personalRecord.time_;
            personalRecord.time_ = newTime;
            personalRecord.UpdateTimestamp();
            
            Debug.Log($"Updated personal best: {oldTime:F3}s → {newTime:F3}s (improvement: {oldTime - newTime:F3}s)");
            return true;
        }
        
        return false; // 没有改善
    }
    
    // 获取个人统计信息
    public PersonalStatistics GetPersonalStatistics()
    {
        Record personalBest = this.GetPersonalBest();
        if (personalBest == null)
        {
            return new PersonalStatistics
            {
                HasRecord = false,
                BestTime = 0f,
                Rank = 0,
                TotalPlayers = this.Records.Count
            };
        }
        
        return new PersonalStatistics
        {
            HasRecord = true,
            BestTime = personalBest.time_,
            Rank = this.CalcRank(),
            TotalPlayers = this.Records.Count,
            BeatenFriends = this.GetBeatenFriendsCount(),
            ImprovementPotential = this.CalculateImprovementPotential()
        };
    }
    
    private int GetBeatenFriendsCount()
    {
        int personalRank = this.CalcRank();
        return personalRank > 0 ? this.Records.Count - personalRank : 0;
    }
    
    private float CalculateImprovementPotential()
    {
        Record personalBest = this.GetPersonalBest();
        if (personalBest == null) return 0f;
        
        int personalRank = this.CalcRank();
        if (personalRank <= 1) return 0f; // 已经是第一名
        
        // 与前一名的时间差
        Record betterRecord = this.Records[personalRank - 2];
        return personalBest.time_ - betterRecord.time_;
    }
    
    protected override GameMode GetGameMode() => this.gameMode_;
    protected override int GetMap() => this.map_;
}

public class PersonalStatistics
{
    public bool HasRecord { get; set; }
    public float BestTime { get; set; }
    public int Rank { get; set; }
    public int TotalPlayers { get; set; }
    public int BeatenFriends { get; set; }
    public float ImprovementPotential { get; set; }
}
```

### 3. 月度排行榜 - MonthlyRanking.cs

#### 临时竞赛排行榜
**功能概述**: 月度重置的竞赛排行榜，支持季节性比赛

**月度排行榜实现**:
```csharp
public class MonthlyRanking : Ranking
{
    private GameMode gameMode_;
    private int map_;
    private string yearMonth_; // YYYY-MM格式
    
    public MonthlyRanking(string fbid, Dictionary<string, string> friends, 
                         GameMode gameMode, int map, string path)
    {
        this.fbid_ = fbid;
        this.friends_ = friends;
        this.gameMode_ = gameMode;
        this.map_ = map;
        this.path_ = path;
        this.yearMonth_ = DateTime.UtcNow.ToString("yyyy-MM");
        this.Records = new List<Record>();
    }
    
    // 月度存储路径
    public override string LocalPath
    {
        get
        {
            string filename = $"{this.fbid_}_monthly_{(int)this.gameMode_}_{this.map_}.xml";
            return Path.Combine(this.path_, "monthly", filename);
        }
    }
    
    // XML反序列化，包含月度验证
    public override void FromXML(XMLElement xml)
    {
        this.Records.Clear();
        
        try
        {
            // 验证年月
            string xmlYearMonth = xml.getAttribute("year_month");
            string currentYearMonth = DateTime.UtcNow.ToString("yyyy-MM");
            
            if (xmlYearMonth != currentYearMonth)
            {
                Debug.Log($"Monthly ranking expired: {xmlYearMonth} vs {currentYearMonth}");
                this.InitializeDefaultRanking();
                return;
            }
            
            this.yearMonth_ = xmlYearMonth;
            this.gameMode_ = (GameMode)int.Parse(xml.getAttribute("game_mode"));
            this.map_ = int.Parse(xml.getAttribute("map"));
            
            // 解析记录
            XMLElement recordsElement = xml.getElement("records");
            if (recordsElement != null)
            {
                foreach (XMLElement recordElement in recordsElement.getElements("record"))
                {
                    Record record = new Record();
                    record.FromXML(recordElement);
                    
                    // 验证记录的年月
                    if (record.yearMonth_ == this.yearMonth_)
                    {
                        this.Records.Add(record);
                    }
                }
            }
            
            Debug.Log($"Loaded {this.Records.Count} monthly records for {this.yearMonth_}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing Monthly ranking XML: {e.Message}");
            this.InitializeDefaultRanking();
        }
    }
    
    // XML序列化，包含月度信息
    public override XMLElement ToXML()
    {
        XMLElement rootElement = new XMLElement("monthly_ranking");
        
        // 设置元数据
        rootElement.setAttribute("year_month", this.yearMonth_);
        rootElement.setAttribute("game_mode", ((int)this.gameMode_).ToString());
        rootElement.setAttribute("map", this.map_.ToString());
        rootElement.setAttribute("fbid", this.fbid_);
        rootElement.setAttribute("last_update", DateTime.UtcNow.ToString("o"));
        
        // 添加记录
        XMLElement recordsElement = new XMLElement("records");
        
        foreach (Record record in this.Records)
        {
            // 确保记录包含正确的年月信息
            record.yearMonth_ = this.yearMonth_;
            recordsElement.addElement(record.ToXML());
        }
        
        rootElement.addElement(recordsElement);
        
        return rootElement;
    }
    
    // 删除月度排行榜文件
    public void Delete()
    {
        try
        {
            string filePath = this.LocalPath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Deleted monthly ranking: {filePath}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete monthly ranking: {e.Message}");
        }
    }
    
    // 批量删除指定用户的所有月度排行榜
    public static void DeleteAll(string fbid, string path)
    {
        try
        {
            string monthlyPath = Path.Combine(path, "monthly");
            if (!Directory.Exists(monthlyPath))
            {
                return;
            }
            
            string pattern = $"{fbid}_monthly_*.xml";
            string[] files = Directory.GetFiles(monthlyPath, pattern);
            
            foreach (string file in files)
            {
                File.Delete(file);
                Debug.Log($"Deleted monthly ranking file: {file}");
            }
            
            Debug.Log($"Deleted {files.Length} monthly ranking files for user {fbid}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete monthly rankings: {e.Message}");
        }
    }
    
    // 检查是否需要月度重置
    public bool IsExpired()
    {
        string currentYearMonth = DateTime.UtcNow.ToString("yyyy-MM");
        return this.yearMonth_ != currentYearMonth;
    }
    
    // 执行月度重置
    public void ResetForNewMonth()
    {
        string newYearMonth = DateTime.UtcNow.ToString("yyyy-MM");
        
        if (this.yearMonth_ != newYearMonth)
        {
            Debug.Log($"Resetting monthly ranking: {this.yearMonth_} → {newYearMonth}");
            
            // 删除旧数据
            this.Delete();
            
            // 更新年月
            this.yearMonth_ = newYearMonth;
            
            // 重新初始化
            this.InitializeDefaultRanking();
            
            // 保存新的排行榜
            this.Save();
        }
    }
    
    // 获取月度统计信息
    public MonthlyStatistics GetMonthlyStatistics()
    {
        Record personalBest = this.GetPersonalBest();
        
        return new MonthlyStatistics
        {
            YearMonth = this.yearMonth_,
            HasRecord = personalBest != null && personalBest.time_ < 1200f,
            BestTime = personalBest?.time_ ?? 0f,
            Rank = this.CalcRank(),
            TotalPlayers = this.Records.Count(r => r.time_ < 1200f),
            IsExpired = this.IsExpired(),
            DaysRemaining = this.GetDaysRemainingInMonth()
        };
    }
    
    private int GetDaysRemainingInMonth()
    {
        DateTime now = DateTime.UtcNow;
        DateTime endOfMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
        return (endOfMonth - now).Days + 1;
    }
    
    protected override GameMode GetGameMode() => this.gameMode_;
    protected override int GetMap() => this.map_;
}

public class MonthlyStatistics
{
    public string YearMonth { get; set; }
    public bool HasRecord { get; set; }
    public float BestTime { get; set; }
    public int Rank { get; set; }
    public int TotalPlayers { get; set; }
    public bool IsExpired { get; set; }
    public int DaysRemaining { get; set; }
}
```

### 4. 排行榜更新系统 - RankingUpdater.cs

#### 异步服务器同步
**功能概述**: 负责与服务器同步排行榜数据的异步更新器

**更新器基类实现**:
```csharp
public class RankingUpdater : IStartable
{
    protected string fbid_;
    protected string authToken_;
    protected Dictionary<string, string> friends_;
    protected string path_;
    
    // IStartable接口实现
    public StartableState State { get; set; }
    public Exception Error { get; set; }
    
    public RankingUpdater(string fbid, string authToken, 
                         Dictionary<string, string> friends, string path)
    {
        this.fbid_ = fbid;
        this.authToken_ = authToken;
        this.friends_ = friends;
        this.path_ = path;
        this.State = StartableState.WAITING;
    }
    
    // 主要的更新流程
    public virtual IEnumerator Update()
    {
        this.State = StartableState.RUNNING;
        this.Error = null;
        
        try
        {
            Debug.Log($"Starting ranking update for user {this.fbid_}");
            
            // 构建请求
            GetRequest request = this.BuildRequest();
            
            // 发送请求
            FiaCoroutine requestCoroutine = new FiaCoroutine(
                request.Send(),
                () => this.OnRequestSuccess(request),
                (ex) => this.OnRequestFailure(ex)
            );
            
            yield return requestCoroutine;
            
            // 等待请求完成
            while (this.State == StartableState.RUNNING)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (this.State == StartableState.FAILED)
            {
                Debug.LogError($"Ranking update failed: {this.Error?.Message}");
                throw this.Error;
            }
            
            Debug.Log("Ranking update completed successfully");
        }
        catch (Exception e)
        {
            this.State = StartableState.FAILED;
            this.Error = e;
            throw;
        }
    }
    
    // 构建服务器请求
    public virtual GetRequest BuildRequest()
    {
        GetRequest request = new GetRequest("http://s.kartriderrush.com/server/ranking.php");
        
        // 添加基本参数
        request.AddField("fbid", this.fbid_);
        request.AddField("auth_token", this.authToken_);
        
        // 添加好友列表
        if (this.friends_ != null && this.friends_.Count > 0)
        {
            string friendsList = string.Join(",", this.friends_.Keys);
            request.AddField("friends", friendsList);
        }
        
        // 添加时间戳用于缓存控制
        request.AddField("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        
        return request;
    }
    
    // 请求成功处理
    protected virtual void OnRequestSuccess(GetRequest request)
    {
        try
        {
            if (request.Response?.XML == null)
            {
                throw new UnknownException("Empty or invalid server response");
            }
            
            // 处理服务器响应
            this.ProcessServerResponse(request.Response.XML);
            
            this.State = StartableState.COMPLETED;
        }
        catch (Exception e)
        {
            this.OnRequestFailure(e);
        }
    }
    
    // 请求失败处理
    protected virtual void OnRequestFailure(Exception ex)
    {
        this.State = StartableState.FAILED;
        this.Error = ex;
        
        Debug.LogError($"Ranking update request failed: {ex.Message}");
        
        // 记录失败分析
        AnalyticsManager.Instance?.TrackEvent("RankingUpdate_Failed", new Dictionary<string, object>
        {
            ["user_id"] = this.fbid_,
            ["error_type"] = ex.GetType().Name,
            ["error_message"] = ex.Message
        });
    }
    
    // 处理服务器响应
    protected virtual void ProcessServerResponse(XMLElement xml)
    {
        try
        {
            // 更新年月信息
            this.UpdateYearMonth(xml);
            
            // 处理所有游戏模式和地图的排行榜
            this.UpdateAllRankings(xml);
            
            Debug.Log("Successfully processed server ranking data");
        }
        catch (Exception e)
        {
            throw new XMLParsingException($"Failed to process server response: {e.Message}");
        }
    }
    
    // 更新年月信息
    public virtual void UpdateYearMonth(XMLElement xml)
    {
        try
        {
            string serverYearMonth = xml.getAttribute("yearmonth");
            string localYearMonth = PlayerPrefs.GetString("YEARMONTH_LAST", "");
            
            if (!string.IsNullOrEmpty(serverYearMonth) && serverYearMonth != localYearMonth)
            {
                Debug.Log($"Year-month update: {localYearMonth} → {serverYearMonth}");
                
                // 标记需要重置月度排行榜
                PlayerPrefs.SetInt("YEARMONTH_RESET", 1);
                PlayerPrefs.SetString("YEARMONTH_LAST", serverYearMonth);
                PlayerPrefs.Save();
                
                // 触发月度重置事件
                EventManager.Instance?.TriggerEvent("MonthlyReset", serverYearMonth);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to update year-month: {e.Message}");
        }
    }
    
    // 更新所有排行榜
    protected virtual void UpdateAllRankings(XMLElement xml)
    {
        int trackCount = TrackAssetDefinitionManager.Instance.GetAssetDefinitionCount();
        GameMode[] gameModes = { GameMode.SINGLE_ITEM, GameMode.SINGLE_SPEED, 
                                GameMode.WIFI_ITEM, GameMode.WIFI_SPEED };
        
        foreach (GameMode mode in gameModes)
        {
            for (int map = 0; map < trackCount; map++)
            {
                this.UpdateRankingForMode(xml, mode, map);
            }
        }
    }
    
    // 更新特定模式和地图的排行榜
    protected virtual void UpdateRankingForMode(XMLElement xml, GameMode mode, int map)
    {
        try
        {
            // 更新全时排行榜
            AllTimeRanking allTimeRanking = new AllTimeRanking(
                this.fbid_, this.friends_, mode, map, this.path_);
            
            if (this.UpdateRankingFromXML(allTimeRanking, xml, mode, map))
            {
                allTimeRanking.Save();
            }
            
            // 更新月度排行榜
            MonthlyRanking monthlyRanking = new MonthlyRanking(
                this.fbid_, this.friends_, mode, map, this.path_);
            
            if (this.UpdateRankingFromXML(monthlyRanking, xml, mode, map))
            {
                monthlyRanking.Save();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to update ranking for mode {mode}, map {map}: {e.Message}");
        }
    }
    
    // 从XML更新排行榜数据
    protected bool UpdateRankingFromXML(Ranking ranking, XMLElement xml, GameMode mode, int map)
    {
        try
        {
            // 查找对应的排行榜数据
            string rankingKey = $"ranking_{(int)mode}_{map}";
            XMLElement rankingElement = xml.getElement(rankingKey);
            
            if (rankingElement != null)
            {
                ranking.FromXML(rankingElement);
                return true;
            }
            
            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse ranking data: {e.Message}");
            return false;
        }
    }
}
```

### 5. 特定排行榜更新器 - SpecificRankingUpdater.cs

#### 增量更新系统
**功能概述**: 针对特定赛道和模式的高效增量更新

**特定更新器实现**:
```csharp
public class SpecificRankingUpdater : RankingUpdater
{
    private GameMode gameMode_;
    private int map_;
    
    public SpecificRankingUpdater(string fbid, string authToken, 
                                 Dictionary<string, string> friends, 
                                 GameMode gameMode, int map, string path)
        : base(fbid, authToken, friends, path)
    {
        this.gameMode_ = gameMode;
        this.map_ = map;
    }
    
    // 构建特定排行榜请求
    public override GetRequest BuildRequest()
    {
        GetRequest request = new GetRequest("http://s.kartriderrush.com/server/specificranking.php");
        
        // 添加基本参数
        request.AddField("fbid", this.fbid_);
        request.AddField("auth_token", this.authToken_);
        request.AddField("game_mode", ((int)this.gameMode_).ToString());
        request.AddField("map", this.map_.ToString());
        
        // 添加好友列表
        if (this.friends_ != null && this.friends_.Count > 0)
        {
            string friendsList = string.Join(",", this.friends_.Keys);
            request.AddField("friends", friendsList);
        }
        
        // 添加优化参数
        request.AddField("include_metadata", "1");
        request.AddField("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        
        return request;
    }
    
    // 处理特定排行榜响应
    protected override void ProcessServerResponse(XMLElement xml)
    {
        try
        {
            // 更新年月信息
            this.UpdateYearMonth(xml);
            
            // 只更新指定的排行榜
            this.UpdateSpecificRanking(xml);
            
            Debug.Log($"Successfully updated specific ranking for mode {this.gameMode_}, map {this.map_}");
        }
        catch (Exception e)
        {
            throw new XMLParsingException($"Failed to process specific ranking response: {e.Message}");
        }
    }
    
    // 更新特定排行榜
    private void UpdateSpecificRanking(XMLElement xml)
    {
        // 更新全时排行榜
        AllTimeRanking allTimeRanking = new AllTimeRanking(
            this.fbid_, this.friends_, this.gameMode_, this.map_, this.path_);
        
        if (this.UpdateRankingFromResponse(allTimeRanking, xml))
        {
            allTimeRanking.Save();
            Debug.Log($"Updated all-time ranking for {this.gameMode_}_{this.map_}");
        }
        
        // 更新月度排行榜
        MonthlyRanking monthlyRanking = new MonthlyRanking(
            this.fbid_, this.friends_, this.gameMode_, this.map_, this.path_);
        
        if (this.UpdateRankingFromResponse(monthlyRanking, xml))
        {
            monthlyRanking.Save();
            Debug.Log($"Updated monthly ranking for {this.gameMode_}_{this.map_}");
        }
    }
    
    // 从响应更新排行榜
    private bool UpdateRankingFromResponse(Ranking ranking, XMLElement xml)
    {
        try
        {
            // 直接从根元素解析排行榜数据
            ranking.FromXML(xml);
            
            // 验证数据完整性
            return this.ValidateRankingData(ranking);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to update ranking from response: {e.Message}");
            return false;
        }
    }
    
    // 验证排行榜数据
    private bool ValidateRankingData(Ranking ranking)
    {
        // 检查是否有记录
        if (ranking.Records == null || ranking.Records.Count == 0)
        {
            Debug.LogWarning("No records found in ranking data");
            return false;
        }
        
        // 检查玩家记录是否存在
        bool hasPlayerRecord = ranking.Records.Any(r => r.fbid_ == this.fbid_);
        if (!hasPlayerRecord)
        {
            Debug.LogWarning("Player record not found in ranking data");
        }
        
        // 检查时间合理性
        bool hasValidTimes = ranking.Records.Any(r => r.time_ > 0 && r.time_ < 1200f);
        if (!hasValidTimes)
        {
            Debug.LogWarning("No valid times found in ranking data");
        }
        
        return true;
    }
    
    // 获取更新统计信息
    public SpecificUpdateStatistics GetUpdateStatistics()
    {
        var stats = new SpecificUpdateStatistics
        {
            GameMode = this.gameMode_,
            Map = this.map_,
            UpdateTime = DateTime.UtcNow,
            Success = this.State == StartableState.COMPLETED,
            ErrorMessage = this.Error?.Message
        };
        
        if (stats.Success)
        {
            // 加载更新后的排行榜获取统计信息
            AllTimeRanking allTimeRanking = new AllTimeRanking(
                this.fbid_, this.friends_, this.gameMode_, this.map_, this.path_);
            
            if (allTimeRanking.Load())
            {
                stats.PlayerRank = allTimeRanking.CalcRank();
                stats.TotalPlayers = allTimeRanking.Records.Count;
                
                Record personalBest = allTimeRanking.GetPersonalBest();
                stats.PersonalBest = personalBest?.time_ ?? 0f;
            }
        }
        
        return stats;
    }
}

public class SpecificUpdateStatistics
{
    public GameMode GameMode { get; set; }
    public int Map { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public int PlayerRank { get; set; }
    public int TotalPlayers { get; set; }
    public float PersonalBest { get; set; }
}
```

### 6. 成绩记录系统 - Record.cs

#### 完整的成绩数据结构
**功能概述**: 包含完整成绩信息和上传功能的记录类

**记录类实现**:
```csharp
public class Record : IStartable, IXMLizable
{
    // 核心数据
    public string fbid_;         // Facebook用户ID
    public string fbname_;       // 显示名称
    public float time_;          // 比赛时间
    public int map_;             // 地图索引
    public GameMode gameMode_;   // 游戏模式
    public string yearMonth_;    // 年月标识
    
    // 状态管理
    public StartableState State { get; set; }
    public Exception Error { get; set; }
    
    // 元数据
    public DateTime CreateTime { get; private set; }
    public DateTime? UploadTime { get; private set; }
    public bool IsUploaded { get; private set; }
    
    // 构造函数
    public Record()
    {
        this.State = StartableState.WAITING;
        this.CreateTime = DateTime.UtcNow;
        this.yearMonth_ = DateTime.UtcNow.ToString("yyyy-MM");
    }
    
    public Record(string fbid, string fbname, GameMode gameMode, float time, int map)
        : this()
    {
        this.fbid_ = fbid;
        this.fbname_ = fbname;
        this.gameMode_ = gameMode;
        this.time_ = time;
        this.map_ = map;
    }
    
    // 上传成绩到服务器
    public IEnumerator Upload(string authToken, Dictionary<string, string> friends)
    {
        this.State = StartableState.RUNNING;
        this.Error = null;
        
        try
        {
            Debug.Log($"Uploading record: {this.time_:F3}s for {this.gameMode_} on map {this.map_}");
            
            // 构建上传请求
            PostRequest uploadRequest = this.BuildUploadRequest(authToken);
            
            // 发送上传请求
            yield return uploadRequest.Send();
            
            if (uploadRequest.Response?.XML == null)
            {
                throw new ServerException("Invalid server response for record upload");
            }
            
            // 处理上传响应
            this.ProcessUploadResponse(uploadRequest.Response.XML);
            
            // 立即更新相关排行榜
            yield return this.UpdateRankingsAfterUpload(authToken, friends);
            
            this.State = StartableState.COMPLETED;
            this.IsUploaded = true;
            this.UploadTime = DateTime.UtcNow;
            
            Debug.Log($"Successfully uploaded record and updated rankings");
        }
        catch (Exception e)
        {
            this.State = StartableState.FAILED;
            this.Error = e;
            Debug.LogError($"Failed to upload record: {e.Message}");
        }
    }
    
    // 构建上传请求
    private PostRequest BuildUploadRequest(string authToken)
    {
        PostRequest request = new PostRequest("http://s.kartriderrush.com/server/record.php");
        
        // 添加成绩数据
        request.AddField("fbid", this.fbid_);
        request.AddField("fbname", this.fbname_);
        request.AddField("auth_token", authToken);
        request.AddField("time", this.time_.ToString("F6")); // 6位小数精度
        request.AddField("map", this.map_.ToString());
        request.AddField("game_mode", ((int)this.gameMode_).ToString());
        request.AddField("year_month", this.yearMonth_);
        
        // 添加验证信息
        request.AddField("client_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        request.AddField("checksum", this.CalculateChecksum());
        
        return request;
    }
    
    // 计算校验和防止作弊
    private string CalculateChecksum()
    {
        string data = $"{this.fbid_}_{this.time_:F6}_{this.map_}_{(int)this.gameMode_}_{this.yearMonth_}";
        
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(data + "SECRET_KEY_2023");
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    // 处理上传响应
    private void ProcessUploadResponse(XMLElement xml)
    {
        try
        {
            string status = xml.getAttribute("status");
            if (status != "success")
            {
                string errorMessage = xml.getAttribute("error") ?? "Unknown server error";
                throw new ServerException($"Server rejected record: {errorMessage}");
            }
            
            // 检查是否是新的个人最佳成绩
            bool isPersonalBest = xml.getAttribute("personal_best") == "true";
            if (isPersonalBest)
            {
                Debug.Log("New personal best achieved!");
                EventManager.Instance?.TriggerEvent("PersonalBest", new PersonalBestEventArgs
                {
                    GameMode = this.gameMode_,
                    Map = this.map_,
                    Time = this.time_,
                    IsAllTimeBest = xml.getAttribute("all_time_best") == "true"
                });
            }
            
            // 检查排行榜位置
            string rankStr = xml.getAttribute("rank");
            if (int.TryParse(rankStr, out int rank))
            {
                Debug.Log($"Current rank: {rank}");
                
                if (rank <= 10)
                {
                    EventManager.Instance?.TriggerEvent("TopTenRank", new RankAchievementEventArgs
                    {
                        Rank = rank,
                        GameMode = this.gameMode_,
                        Map = this.map_
                    });
                }
            }
        }
        catch (Exception e)
        {
            throw new XMLParsingException($"Failed to process upload response: {e.Message}");
        }
    }
    
    // 上传后更新排行榜
    private IEnumerator UpdateRankingsAfterUpload(string authToken, Dictionary<string, string> friends)
    {
        string path = FiaUtil.docPath; // 假设使用文档路径
        
        // 更新特定排行榜
        SpecificRankingUpdater updater = new SpecificRankingUpdater(
            this.fbid_, authToken, friends, this.gameMode_, this.map_, path);
        
        yield return updater.Update();
        
        if (updater.State == StartableState.FAILED)
        {
            Debug.LogWarning($"Failed to update ranking after upload: {updater.Error?.Message}");
        }
    }
    
    // XML序列化
    public XMLElement ToXML()
    {
        XMLElement element = new XMLElement("record");
        
        element.setAttribute("fbid", this.fbid_);
        element.setAttribute("fbname", this.fbname_);
        element.setAttribute("time", this.time_.ToString("F6"));
        element.setAttribute("map", this.map_.ToString());
        element.setAttribute("game_mode", ((int)this.gameMode_).ToString());
        element.setAttribute("year_month", this.yearMonth_);
        element.setAttribute("create_time", this.CreateTime.ToString("o"));
        
        if (this.UploadTime.HasValue)
        {
            element.setAttribute("upload_time", this.UploadTime.Value.ToString("o"));
        }
        
        element.setAttribute("is_uploaded", this.IsUploaded.ToString().ToLower());
        
        return element;
    }
    
    // XML反序列化
    public void FromXML(XMLElement xml)
    {
        try
        {
            this.fbid_ = xml.getAttribute("fbid");
            this.fbname_ = xml.getAttribute("fbname");
            this.time_ = float.Parse(xml.getAttribute("time"));
            this.map_ = int.Parse(xml.getAttribute("map"));
            this.gameMode_ = (GameMode)int.Parse(xml.getAttribute("game_mode"));
            this.yearMonth_ = xml.getAttribute("year_month");
            
            // 解析时间戳
            string createTimeStr = xml.getAttribute("create_time");
            if (!string.IsNullOrEmpty(createTimeStr))
            {
                this.CreateTime = DateTime.Parse(createTimeStr);
            }
            
            string uploadTimeStr = xml.getAttribute("upload_time");
            if (!string.IsNullOrEmpty(uploadTimeStr))
            {
                this.UploadTime = DateTime.Parse(uploadTimeStr);
            }
            
            string isUploadedStr = xml.getAttribute("is_uploaded");
            this.IsUploaded = bool.Parse(isUploadedStr ?? "false");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse Record from XML: {e.Message}");
        }
    }
    
    // 更新时间戳
    public void UpdateTimestamp()
    {
        this.CreateTime = DateTime.UtcNow;
        this.yearMonth_ = DateTime.UtcNow.ToString("yyyy-MM");
    }
    
    // 验证记录有效性
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(this.fbid_) &&
               !string.IsNullOrEmpty(this.fbname_) &&
               this.time_ > 0f &&
               this.time_ < 1200f &&
               this.map_ >= 0 &&
               !string.IsNullOrEmpty(this.yearMonth_);
    }
    
    // 获取格式化时间
    public string GetFormattedTime()
    {
        if (this.time_ >= 1200f)
        {
            return "未完成";
        }
        
        TimeSpan timeSpan = TimeSpan.FromSeconds(this.time_);
        return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
    }
    
    // 比较成绩
    public int CompareTo(Record other)
    {
        if (other == null) return -1;
        return this.time_.CompareTo(other.time_);
    }
}

// 事件参数类
public class PersonalBestEventArgs : EventArgs
{
    public GameMode GameMode { get; set; }
    public int Map { get; set; }
    public float Time { get; set; }
    public bool IsAllTimeBest { get; set; }
}

public class RankAchievementEventArgs : EventArgs
{
    public int Rank { get; set; }
    public GameMode GameMode { get; set; }
    public int Map { get; set; }
}
```

## 性能优化和最佳实践

### 1. 智能缓存策略
```csharp
public class RankingCacheManager
{
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);
    private Dictionary<string, CachedRanking> cache_ = new Dictionary<string, CachedRanking>();
    
    public bool TryGetCachedRanking(string key, out Ranking ranking)
    {
        if (this.cache_.TryGetValue(key, out CachedRanking cached))
        {
            if (DateTime.UtcNow - cached.CacheTime < CACHE_DURATION)
            {
                ranking = cached.Ranking;
                return true;
            }
            else
            {
                this.cache_.Remove(key);
            }
        }
        
        ranking = null;
        return false;
    }
    
    public void CacheRanking(string key, Ranking ranking)
    {
        this.cache_[key] = new CachedRanking
        {
            Ranking = ranking,
            CacheTime = DateTime.UtcNow
        };
    }
    
    private class CachedRanking
    {
        public Ranking Ranking { get; set; }
        public DateTime CacheTime { get; set; }
    }
}
```

### 2. 批量操作优化
```csharp
public class BatchRankingUpdater
{
    public IEnumerator UpdateMultipleRankings(List<RankingUpdateRequest> requests)
    {
        // 按服务器端点分组请求
        var groupedRequests = requests.GroupBy(r => r.Endpoint);
        
        foreach (var group in groupedRequests)
        {
            yield return this.ProcessRequestGroup(group.ToList());
        }
    }
    
    private IEnumerator ProcessRequestGroup(List<RankingUpdateRequest> requests)
    {
        // 构建批量请求
        PostRequest batchRequest = new PostRequest(requests.First().Endpoint);
        
        for (int i = 0; i < requests.Count; i++)
        {
            var request = requests[i];
            batchRequest.AddField($"request_{i}_type", request.Type);
            batchRequest.AddField($"request_{i}_data", request.Data);
        }
        
        yield return batchRequest.Send();
        
        // 处理批量响应
        this.ProcessBatchResponse(batchRequest.Response.XML, requests);
    }
}
```

## 总结

Ranking系统展现了分布式系统设计的卓越实践，具有以下核心优势：

### 技术优势
1. **模板方法模式**: 统一的排行榜操作框架，易于扩展新类型
2. **增量同步**: 高效的特定排行榜更新机制
3. **时间管理**: 智能的月度重置和同步机制
4. **数据完整性**: XML序列化和校验和验证
5. **社交集成**: 深度的Facebook好友比较功能

### 架构特点
- **异步处理**: 基于协程的非阻塞操作模式
- **错误恢复**: 完善的离线支持和故障处理
- **缓存优化**: 多层缓存策略提升性能
- **扩展性**: 清晰的接口支持新排行榜类型
- **实时性**: 成绩上传后立即更新排行榜

该排行榜系统为卡丁车游戏提供了企业级的竞技功能基础，支持实时排名、社交比较和季节性竞赛，确保了公平竞争和优秀的用户体验。