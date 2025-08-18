# Network 文件夹完整功能文档

## 概述

Network 文件夹包含了卡丁车游戏的完整多人网络系统，实现了客户端-服务器架构的实时同步机制。该系统支持跨平台网络连接、可靠的状态同步、WiFi直连和HTTP通信，为游戏提供了专业级的多人游戏体验和网络通信基础设施。

## 系统架构

### 核心设计原则
- **客户端-服务器架构**: 权威服务器和客户端预测
- **实时同步**: 高频率的游戏状态同步
- **跨平台支持**: 统一的网络接口和平台特定实现
- **可靠性保证**: 连接恢复和错误处理机制
- **性能优化**: 二进制序列化和时间同步算法

### 网络层次架构
- **传输层** (`Session/`): 底层连接管理和数据传输
- **协议层** (`Packets/`): 数据包定义和序列化系统
- **控制层** (`Controllers/`): 网络实体控制和管理
- **应用层** (`WiFi/`, `Request`): 游戏特定的网络功能

## 核心网络管理系统

### 1. 网络管理器 - NetworkManager.cs

#### 中央网络协调器
**功能概述**: 单例网络管理器，负责数据包路由、时间同步和会话生命周期管理

**核心管理架构**:
```csharp
public class NetworkManager : MonoBehaviour, PacketHandler
{
    private static NetworkManager instance_;
    private Queue<Packet> packetQueue_;
    private Session currentSession_;
    
    // 时间同步系统
    private int serverTimeOffset_;
    private float lastSyncTime_;
    private bool isTimeSync_;
    
    public static NetworkManager Instance
    {
        get
        {
            if (instance_ == null)
            {
                GameObject networkObject = new GameObject("NetworkManager");
                instance_ = networkObject.AddComponent<NetworkManager>();
                DontDestroyOnLoad(networkObject);
            }
            return instance_;
        }
    }
}
```

**数据包队列管理**:
```csharp
private void Update()
{
    // 处理积压的数据包队列
    while (this.packetQueue_.Count > 0)
    {
        Packet packet = this.packetQueue_.Dequeue();
        this.ProcessPacket(packet);
    }
    
    // 定期时间同步
    if (Time.time - this.lastSyncTime_ > TIME_SYNC_INTERVAL)
    {
        this.RequestTimeSync();
    }
}

public void HandlePacket(Packet packet)
{
    // 检查游戏阶段是否准备好处理数据包
    if (!this.IsGameStageReady())
    {
        this.packetQueue_.Enqueue(packet);
        return;
    }
    
    this.ProcessPacket(packet);
}

private void ProcessPacket(Packet packet)
{
    // 基于数据包类型的分发
    switch (packet.GetPacketType())
    {
        case PacketType.GameKart:
            this.HandleGameKartPacket((GameKartPacket)packet);
            break;
            
        case PacketType.TimeSync:
            this.HandleTimeSyncPacket((TimeSyncPacket)packet);
            break;
            
        case PacketType.GameControl:
            this.HandleGameControlPacket((GameControlPacket)packet);
            break;
            
        default:
            Debug.LogWarning($"Unknown packet type: {packet.GetPacketType()}");
            break;
    }
}
```

**精确时间同步算法**:
```csharp
private void HandleTimeSyncPacket(TimeSyncPacket packet)
{
    int currentTick = MonoBehaiourExConst.GetTick();
    int roundTripTime = currentTick - packet.clientSendTick;
    
    // 计算服务器时间偏移
    int serverOffset = packet.serverTick - packet.clientSendTick - (roundTripTime / 2);
    
    // 网络抖动平滑处理
    if (roundTripTime < 6) // 低延迟情况下的平滑
    {
        this.serverTimeOffset_ = (this.serverTimeOffset_ + serverOffset) / 2;
    }
    else
    {
        this.serverTimeOffset_ = serverOffset;
    }
    
    this.isTimeSync_ = true;
    
    Debug.Log($"Time sync: RTT={roundTripTime}, Offset={this.serverTimeOffset_}");
}

public int GetServerTime()
{
    if (!this.isTimeSync_)
        return MonoBehaiourExConst.GetTick();
    
    return MonoBehaiourExConst.GetTick() + this.serverTimeOffset_;
}
```

### 2. 会话管理系统 - Session架构

#### Session.cs - 基础会话类
**功能概述**: 抽象基类定义网络会话的标准接口和生命周期

**会话状态管理**:
```csharp
public abstract class Session
{
    public enum SessionMode
    {
        SERVER,  // 主机模式，具有游戏状态权威
        CLIENT   // 客户端模式，接收状态更新
    }
    
    public enum SessionState
    {
        AVAILABLE,   // 可连接状态
        CONNECTING,  // 连接建立中
        CONNECTED    // 已连接状态
    }
    
    protected SessionMode mode_;
    protected SessionState state_;
    protected PacketHandler packetHandler_;
    
    // 抽象接口，平台特定实现
    public abstract bool StartServer(int port);
    public abstract bool ConnectToServer(string address, int port);
    public abstract void SendPacket(Packet packet, SendDataMode mode);
    public abstract void Disconnect();
    
    // 通用会话管理
    protected virtual void OnConnectionEstablished()
    {
        this.state_ = SessionState.CONNECTED;
        Debug.Log($"Session connected in {this.mode_} mode");
    }
    
    protected virtual void OnDataReceived(byte[] data)
    {
        try
        {
            Packet packet = PacketFactory.DeserializePacket(data);
            this.packetHandler_?.HandlePacket(packet);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to process received data: {e.Message}");
        }
    }
}
```

#### AndroidSession.cs - Android平台实现
**功能概述**: Android特定的网络实现，通过JNI桥接原生网络功能

**JNI桥接系统**:
```csharp
public class AndroidSession : Session
{
    private const string ANDROID_NETWORK_CLASS = "com.unity3d.plugins.AndroidNetwork";
    private AndroidJavaObject networkHelper_;
    
    public override bool StartServer(int port)
    {
        try
        {
            using (AndroidJavaClass networkClass = new AndroidJavaClass(ANDROID_NETWORK_CLASS))
            {
                this.networkHelper_ = networkClass.CallStatic<AndroidJavaObject>("getInstance");
                
                bool success = this.networkHelper_.Call<bool>("startServer", port);
                
                if (success)
                {
                    this.mode_ = SessionMode.SERVER;
                    this.state_ = SessionState.CONNECTED;
                    
                    // 启动数据接收线程
                    this.StartReceiveLoop();
                }
                
                return success;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start Android server: {e.Message}");
            return false;
        }
    }
    
    public override bool ConnectToServer(string address, int port)
    {
        try
        {
            bool success = this.networkHelper_.Call<bool>("connectToServer", address, port);
            
            if (success)
            {
                this.mode_ = SessionMode.CLIENT;
                this.state_ = SessionState.CONNECTED;
                this.StartReceiveLoop();
            }
            
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to server: {e.Message}");
            return false;
        }
    }
    
    private void StartReceiveLoop()
    {
        // 在后台线程中持续接收数据
        Thread receiveThread = new Thread(() =>
        {
            while (this.state_ == SessionState.CONNECTED)
            {
                try
                {
                    byte[] data = this.networkHelper_.Call<byte[]>("receiveData", 5000); // 5秒超时
                    
                    if (data != null && data.Length > 0)
                    {
                        // 切换到主线程处理数据包
                        UnityMainThreadDispatcher.Instance.Enqueue(() => this.OnDataReceived(data));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Receive error: {e.Message}");
                    break;
                }
            }
        });
        
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
}
```

### 3. 数据包系统 - Packets架构

#### PacketFactory.cs - 数据包工厂
**功能概述**: 集中化的数据包创建、序列化和类型管理系统

**类型注册系统**:
```csharp
public static class PacketFactory
{
    // 数据包类型注册表
    private static readonly Type[] packetTypes_ = new Type[]
    {
        typeof(GameKartPacket),        // 0
        typeof(TimeSyncPacket),        // 1
        typeof(PlayerPacket),          // 2
        typeof(GameControlPacket),     // 3
        typeof(GameResultPacket),      // 4
        typeof(ItemPacket),            // 5
        typeof(GuardEffectPacket),     // 6
        typeof(ShieldEffectPacket),    // 7
        typeof(DestroyBananaPacket),   // 8
        typeof(UserLeaveNoticePacket), // 9
        typeof(AppVersionPacket),      // 10
        typeof(GameParamPacket),       // 11
        typeof(PlayerResultPacket),    // 12
        typeof(ServerStatePacket),     // 13
        typeof(LastReceivedPacket)     // 14
    };
    
    private static readonly Dictionary<Type, int> typeToStamp_;
    
    static PacketFactory()
    {
        typeToStamp_ = new Dictionary<Type, int>();
        
        for (int i = 0; i < packetTypes_.Length; i++)
        {
            typeToStamp_[packetTypes_[i]] = i;
        }
    }
}
```

**高效序列化系统**:
```csharp
public static byte[] SerializePacket(Packet packet, SendDataMode mode)
{
    Type packetType = packet.GetType();
    
    if (!typeToStamp_.TryGetValue(packetType, out int typeStamp))
    {
        throw new ArgumentException($"Unknown packet type: {packetType.Name}");
    }
    
    using (MemoryStream stream = new MemoryStream())
    using (BinaryWriter writer = new BinaryWriter(stream))
    {
        // 写入数据包头
        writer.Write(typeStamp);           // 类型标识
        writer.Write((int)mode);           // 传输模式
        writer.Write(Environment.TickCount); // 时间戳
        
        // 写入数据包内容
        packet.Serialize(writer);
        
        return stream.ToArray();
    }
}

public static Packet DeserializePacket(byte[] data)
{
    using (MemoryStream stream = new MemoryStream(data))
    using (BinaryReader reader = new BinaryReader(stream))
    {
        // 读取数据包头
        int typeStamp = reader.ReadInt32();
        SendDataMode mode = (SendDataMode)reader.ReadInt32();
        int timestamp = reader.ReadInt32();
        
        // 验证类型标识
        if (typeStamp < 0 || typeStamp >= packetTypes_.Length)
        {
            throw new ArgumentException($"Invalid packet type stamp: {typeStamp}");
        }
        
        // 创建数据包实例
        Type packetType = packetTypes_[typeStamp];
        Packet packet = (Packet)Activator.CreateInstance(packetType);
        
        // 反序列化内容
        packet.Deserialize(reader);
        packet.SetTimestamp(timestamp);
        packet.SetSendMode(mode);
        
        return packet;
    }
}
```

#### GameKartPacket.cs - 游戏状态同步
**功能概述**: 实时卡丁车状态同步的核心数据包

**高频状态同步**:
```csharp
public class GameKartPacket : Packet
{
    // 基础运动状态
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public float speed;
    
    // 动画状态
    public CharacterAnimation characterAnimation;
    public KartBodyAnimation kartBodyAnimation;
    
    // 游戏状态
    public int rankingValue;
    public int kartIndex;
    public int serverTick;
    
    // 效果状态
    public bool hasShield;
    public bool isBoosting;
    public int itemState;
    
    public override void Serialize(BinaryWriter writer)
    {
        // 位置和旋转
        SerializeVector3(writer, this.position);
        SerializeQuaternion(writer, this.rotation);
        SerializeVector3(writer, this.velocity);
        writer.Write(this.speed);
        
        // 动画状态
        writer.Write((int)this.characterAnimation);
        writer.Write((int)this.kartBodyAnimation);
        
        // 游戏数据
        writer.Write(this.rankingValue);
        writer.Write(this.kartIndex);
        writer.Write(this.serverTick);
        
        // 效果状态（使用位标记压缩）
        byte effectFlags = 0;
        if (this.hasShield) effectFlags |= 0x01;
        if (this.isBoosting) effectFlags |= 0x02;
        writer.Write(effectFlags);
        writer.Write(this.itemState);
    }
    
    public override void Deserialize(BinaryReader reader)
    {
        // 读取位置和旋转
        this.position = DeserializeVector3(reader);
        this.rotation = DeserializeQuaternion(reader);
        this.velocity = DeserializeVector3(reader);
        this.speed = reader.ReadSingle();
        
        // 读取动画状态
        this.characterAnimation = (CharacterAnimation)reader.ReadInt32();
        this.kartBodyAnimation = (KartBodyAnimation)reader.ReadInt32();
        
        // 读取游戏数据
        this.rankingValue = reader.ReadInt32();
        this.kartIndex = reader.ReadInt32();
        this.serverTick = reader.ReadInt32();
        
        // 读取效果状态
        byte effectFlags = reader.ReadByte();
        this.hasShield = (effectFlags & 0x01) != 0;
        this.isBoosting = (effectFlags & 0x02) != 0;
        this.itemState = reader.ReadInt32();
    }
}
```

### 4. WiFi直连系统

#### WifiStage.cs - WiFi网络阶段
**功能概述**: WiFi直连功能的游戏阶段管理，支持房间发现和连接

**房间发现和连接流程**:
```csharp
public class WifiStage : GameStageBase
{
    private Session wifiSession_;
    private List<GameRoomInfo> discoveredRooms_;
    private bool isSearching_;
    private float lastDiscoveryTime_;
    
    public override void Enter()
    {
        base.Enter();
        
        this.InitializeWiFiSession();
        this.StartRoomDiscovery();
    }
    
    private void InitializeWiFiSession()
    {
        // 根据平台创建适当的会话
        if (Application.platform == RuntimePlatform.Android)
        {
            this.wifiSession_ = new AndroidSession();
        }
        else
        {
            this.wifiSession_ = new MockSession(); // 用于测试
        }
        
        this.wifiSession_.SetPacketHandler(NetworkManager.Instance);
    }
    
    private void StartRoomDiscovery()
    {
        this.isSearching_ = true;
        this.discoveredRooms_ = new List<GameRoomInfo>();
        
        // 开始广播扫描
        this.StartCoroutine(this.PerformRoomDiscovery());
    }
    
    private IEnumerator PerformRoomDiscovery()
    {
        while (this.isSearching_)
        {
            // 发送房间发现请求
            RoomDiscoveryPacket discoveryPacket = new RoomDiscoveryPacket
            {
                playerName = PlayerPrefs.GetString("PlayerName", "Player"),
                gameVersion = Application.version,
                searchTimestamp = Time.time
            };
            
            this.wifiSession_.BroadcastPacket(discoveryPacket);
            
            yield return new WaitForSeconds(1f); // 每秒扫描一次
        }
    }
    
    public void JoinRoom(GameRoomInfo roomInfo)
    {
        this.isSearching_ = false;
        
        // 验证版本兼容性
        if (!this.IsVersionCompatible(roomInfo.gameVersion))
        {
            this.ShowVersionMismatchDialog(roomInfo.gameVersion);
            return;
        }
        
        // 连接到选定房间
        bool connected = this.wifiSession_.ConnectToServer(roomInfo.ipAddress, roomInfo.port);
        
        if (connected)
        {
            // 发送加入请求
            JoinRoomPacket joinPacket = new JoinRoomPacket
            {
                playerName = PlayerPrefs.GetString("PlayerName"),
                characterId = PlayerPrefs.GetInt("SelectedCharacter"),
                kartId = PlayerPrefs.GetInt("SelectedKart")
            };
            
            this.wifiSession_.SendPacket(joinPacket, SendDataMode.RELIABLE);
            
            // 切换到等待房间阶段
            StageController.Instance.ChangeStage(StageType.WaitRoom);
        }
        else
        {
            this.ShowConnectionFailedDialog();
        }
    }
}
```

### 5. HTTP请求系统

#### Request.cs - HTTP请求基类
**功能概述**: 外部服务器通信的HTTP请求抽象基类

**异步HTTP通信**:
```csharp
public abstract class Request
{
    protected string url_;
    protected RequestDelegate onSuccess_;
    protected RequestDelegate onFailure_;
    protected WWW wwwRequest_;
    
    public void ExecuteAsync(string url, RequestDelegate onSuccess, RequestDelegate onFailure)
    {
        this.url_ = url;
        this.onSuccess_ = onSuccess;
        this.onFailure_ = onFailure;
        
        MonoBehaviour caller = NetworkManager.Instance;
        caller.StartCoroutine(this.PerformRequest());
    }
    
    private IEnumerator PerformRequest()
    {
        try
        {
            // 准备请求数据
            byte[] requestData = this.PrepareRequestData();
            Dictionary<string, string> headers = this.PrepareHeaders();
            
            // 创建WWW请求
            if (requestData != null)
            {
                this.wwwRequest_ = new WWW(this.url_, requestData, headers);
            }
            else
            {
                this.wwwRequest_ = new WWW(this.url_);
            }
            
            yield return this.wwwRequest_;
            
            // 处理响应
            if (string.IsNullOrEmpty(this.wwwRequest_.error))
            {
                Response response = this.ParseResponse(this.wwwRequest_.bytes);
                this.onSuccess_?.Invoke(response);
            }
            else
            {
                this.HandleError(this.wwwRequest_.error);
            }
        }
        catch (Exception e)
        {
            this.HandleError(e.Message);
        }
        finally
        {
            this.wwwRequest_?.Dispose();
        }
    }
    
    protected abstract byte[] PrepareRequestData();
    protected abstract Dictionary<string, string> PrepareHeaders();
    protected abstract Response ParseResponse(byte[] responseData);
    
    private void HandleError(string error)
    {
        Debug.LogError($"Request failed: {error}");
        
        RequestException exception = new RequestException(error);
        this.onFailure_?.Invoke(new ErrorResponse(exception));
    }
}
```

## 性能优化和可靠性特性

### 1. 网络性能优化

#### 数据压缩和序列化优化
```csharp
public static class NetworkOptimizer
{
    // 向量压缩序列化
    public static void SerializeCompressedVector3(BinaryWriter writer, Vector3 vector)
    {
        // 使用16位精度减少带宽
        writer.Write((short)(vector.x * 100f));
        writer.Write((short)(vector.y * 100f));
        writer.Write((short)(vector.z * 100f));
    }
    
    // 四元数压缩
    public static void SerializeCompressedQuaternion(BinaryWriter writer, Quaternion quaternion)
    {
        // 找到最大分量，只传输其他三个分量
        int maxIndex = GetMaxQuaternionComponentIndex(quaternion);
        byte header = (byte)((maxIndex << 6) | 0x3F); // 6位用于索引，其余预留
        
        writer.Write(header);
        
        for (int i = 0; i < 4; i++)
        {
            if (i != maxIndex)
            {
                writer.Write((short)(quaternion[i] * 32767f));
            }
        }
    }
    
    // 状态差异传输
    public static byte[] CreateDeltaPacket(GameKartPacket previous, GameKartPacket current)
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            byte changeFlags = 0;
            
            // 检查位置变化
            if (Vector3.Distance(previous.position, current.position) > 0.01f)
            {
                changeFlags |= 0x01;
            }
            
            // 检查旋转变化
            if (Quaternion.Angle(previous.rotation, current.rotation) > 0.1f)
            {
                changeFlags |= 0x02;
            }
            
            // 检查动画变化
            if (previous.characterAnimation != current.characterAnimation)
            {
                changeFlags |= 0x04;
            }
            
            writer.Write(changeFlags);
            
            // 只写入变化的数据
            if ((changeFlags & 0x01) != 0)
            {
                SerializeCompressedVector3(writer, current.position);
            }
            
            if ((changeFlags & 0x02) != 0)
            {
                SerializeCompressedQuaternion(writer, current.rotation);
            }
            
            if ((changeFlags & 0x04) != 0)
            {
                writer.Write((byte)current.characterAnimation);
            }
            
            return stream.ToArray();
        }
    }
}
```

### 2. 连接可靠性保障

#### 自动重连机制
```csharp
public class ConnectionManager
{
    private int reconnectAttempts_;
    private const int MAX_RECONNECT_ATTEMPTS = 5;
    private const float RECONNECT_DELAY = 2f;
    
    public void HandleConnectionLoss()
    {
        if (this.reconnectAttempts_ < MAX_RECONNECT_ATTEMPTS)
        {
            this.reconnectAttempts_++;
            
            float delay = RECONNECT_DELAY * Mathf.Pow(2, this.reconnectAttempts_ - 1); // 指数退避
            
            NetworkManager.Instance.StartCoroutine(this.AttemptReconnect(delay));
        }
        else
        {
            this.OnReconnectFailed();
        }
    }
    
    private IEnumerator AttemptReconnect(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        bool reconnected = NetworkManager.Instance.GetCurrentSession().Reconnect();
        
        if (reconnected)
        {
            this.reconnectAttempts_ = 0;
            this.OnReconnectSuccess();
        }
        else
        {
            this.HandleConnectionLoss();
        }
    }
}
```

## 总结

Network系统提供了一个完整、可靠的多人游戏网络框架，具有以下核心优势：

### 技术优势
1. **实时同步**: 高频率、低延迟的游戏状态同步
2. **跨平台支持**: 统一接口和平台特定实现
3. **时间同步**: 精确的客户端-服务器时间同步算法
4. **数据压缩**: 优化的序列化和带宽使用
5. **连接可靠性**: 自动重连和错误恢复机制

### 架构特点
- **分层设计**: 清晰的传输、协议和应用层分离
- **工厂模式**: 类型安全的数据包创建和管理
- **策略模式**: 可靠和不可靠传输模式
- **观察者模式**: 事件驱动的网络状态通知

该网络系统为卡丁车游戏提供了企业级的多人游戏基础设施，支持复杂的实时同步需求，同时保持了出色的性能和跨平台兼容性。