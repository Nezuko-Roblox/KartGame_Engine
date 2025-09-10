// NetworkManager.server.ts
// 简化的服务器端网络管理器 - 只负责同步远程赛车状态
// 客户端直接使用RigidbodyFPSWalker进行本地物理计算

const RunService = game.GetService("RunService");
const ReplicatedStorage = game.GetService("ReplicatedStorage");
const Players = game.GetService("Players");

// 网络配置常量
const NETWORK_CONFIG = {
    SYNC_RATE: 20,              // 状态同步频率(Hz) - 20Hz
    MAX_PLAYERS: 8,             // 最大玩家数量
};

interface InputDataMessage {
    horizontal?: number;
    vertical?: number;
    drift?: boolean;
    boost?: boolean;
}

interface PositionDataMessage {
    position?: Vector3;
    rotation?: Vector3;
    velocity?: Vector3;
}

interface PlayerInputData {
    playerId: number;
    playerName: string;
    horizontal: number;  // 水平输入 (-1 到 1)
    vertical: number;    // 垂直输入 (-1 到 1)
    drift: boolean;      // 是否按住漂移键
    boost: boolean;      // 是否按住加速键
    timestamp: number;
}

interface PlayerPositionData {
    playerId: number;
    position: Vector3;
    rotation: Vector3;
    velocity: Vector3;
    timestamp: number;
}

interface PlayerInfo {
    userId: number;
    name: string;
    joinTime: number;
}

// 创建网络通信文件夹和事件
function createNetworkInfrastructure(): Folder {
    const existingFolder = ReplicatedStorage.FindFirstChild("NetworkRemotes") as Folder;
    if (existingFolder) {
        return existingFolder;
    }
    
    const remotes = new Instance("Folder");
    remotes.Name = "NetworkRemotes";
    remotes.Parent = ReplicatedStorage;
    
    // 创建远程事件
    const events = {
        // 玩家输入同步
        PlayerInput: new Instance("RemoteEvent"),      // 客户端发送自己的输入
        AllPlayersInput: new Instance("RemoteEvent"),  // 服务器广播所有玩家的输入
        
        // 位置同步
        PlayerPosition: new Instance("RemoteEvent"),   // 客户端发送自己的位置
        AllPlayersPosition: new Instance("RemoteEvent"), // 服务器广播所有玩家的位置
        
        // 玩家管理
        PlayerJoined: new Instance("RemoteEvent"),     // 玩家加入通知
        PlayerLeft: new Instance("RemoteEvent"),       // 玩家离开通知
    };
    
    for (const [name, event] of pairs(events)) {
        event.Name = name;
        event.Parent = remotes;
    }
    
    return remotes;
}

export class NetworkManager {
    private remotes: Folder;
    private playerInputs: Map<number, PlayerInputData> = new Map();
    private playerPositions: Map<number, PlayerPositionData> = new Map();
    private connectedPlayers: Map<Player, PlayerInfo> = new Map();
    
    // 同步相关
    private syncAccumulator: number = 0;
    private positionSyncAccumulator: number = 0;

    constructor() {
        // 初始化网络基础设施
        this.remotes = createNetworkInfrastructure();
        
        // 初始化连接
        this.setupConnections();
    }

    private setupConnections(): void {
        // 玩家加入处理
        Players.PlayerAdded.Connect((player) => {
            this.onPlayerJoin(player);
        });
        
        // 玩家离开处理
        Players.PlayerRemoving.Connect((player) => {
            this.onPlayerLeave(player);
        });
        
        // 接收客户端输入
        (this.remotes.FindFirstChild("PlayerInput") as RemoteEvent).OnServerEvent.Connect((player, inputData) => {
            this.updatePlayerInput(player, inputData);
        });
        
        // 接收客户端位置
        (this.remotes.FindFirstChild("PlayerPosition") as RemoteEvent).OnServerEvent.Connect((player, positionData) => {
            this.updatePlayerPosition(player, positionData);
        });
        
        // 主同步循环
        RunService.Heartbeat.Connect((deltaTime) => {
            this.networkSync(deltaTime);
            this.positionSync(deltaTime);
        });
    }

    private onPlayerJoin(player: Player): void {
        // 添加到连接列表
        this.connectedPlayers.set(player, {
            userId: player.UserId,
            name: player.Name,
            joinTime: tick()
        });
        
        // 初始化玩家输入
        this.playerInputs.set(player.UserId, {
            playerId: player.UserId,
            playerName: player.Name,
            horizontal: 0,  // 水平输入 (-1 到 1)
            vertical: 0,    // 垂直输入 (-1 到 1)
            drift: false,   // 是否按住漂移键
            boost: false,   // 是否按住加速键
            timestamp: tick()
        });
        
        // 初始化玩家位置
        this.playerPositions.set(player.UserId, {
            playerId: player.UserId,
            position: new Vector3(0, 0, 0),
            rotation: new Vector3(0, 0, 0),
            velocity: new Vector3(0, 0, 0),
            timestamp: tick()
        });
        
        // 向新玩家发送所有已存在玩家的信息
        for (const [existingUserId, existingInput] of this.playerInputs) {
            if (existingUserId !== player.UserId) {
                // 向新玩家发送每个已存在玩家的信息
                (this.remotes.FindFirstChild("PlayerJoined") as RemoteEvent).FireClient(player, {
                    playerId: existingInput.playerId,
                    playerName: existingInput.playerName
                });
            }
        }
        
        // 向所有其他客户端通知新玩家加入
        for (const otherPlayer of Players.GetPlayers()) {
            if (otherPlayer !== player) {
                (this.remotes.FindFirstChild("PlayerJoined") as RemoteEvent).FireClient(otherPlayer, {
                    playerId: player.UserId,
                    playerName: player.Name
                });
            }
        }
    }

    private onPlayerLeave(player: Player): void {
        const userId = player.UserId;
        
        // 清理数据
        this.connectedPlayers.delete(player);
        this.playerInputs.delete(userId);
        this.playerPositions.delete(userId);
        
        // 通知其他客户端
        (this.remotes.FindFirstChild("PlayerLeft") as RemoteEvent).FireAllClients(userId);
    }

    private updatePlayerInput(player: Player, inputData: unknown): void {
        // 验证数据
        if (!inputData) {
            return;
        }
        
        const data = inputData as InputDataMessage;
        
        // 更新玩家输入
        const userId = player.UserId;
        this.playerInputs.set(userId, {
            playerId: userId,
            playerName: player.Name,
            horizontal: data.horizontal || 0,
            vertical: data.vertical || 0,
            drift: data.drift || false,
            boost: data.boost || false,
            timestamp: tick()
        });
    }

    private networkSync(deltaTime: number): void {
        this.syncAccumulator = this.syncAccumulator + deltaTime;
        
        const syncInterval = 1 / NETWORK_CONFIG.SYNC_RATE;
        if (this.syncAccumulator < syncInterval) {
            return;
        }
        
        // 防止累积积压，直接重置
        this.syncAccumulator = 0;
        
        // 收集所有玩家输入
        const allInputs: PlayerInputData[] = [];
        
        for (const [userId, input] of this.playerInputs) {
            allInputs.push(input);
        }
        
        // 广播给所有客户端（包括发送者自己，用于权威服务器模式）
        if (allInputs.size() > 0) {
            (this.remotes.FindFirstChild("AllPlayersInput") as RemoteEvent).FireAllClients(allInputs);
        }
    }

    // 更新玩家位置
    private updatePlayerPosition(player: Player, positionData: unknown): void {
        if (!positionData) {
            return;
        }
        
        const data = positionData as PositionDataMessage;
        const userId = player.UserId;
        const position = data.position || new Vector3(0, 0, 0);
        
        // 调试输出：服务端接收到的位置（注释掉，太频繁）
        // print(string.format("[Server-Recv] 玩家:%s 位置:(%.2f, %.2f, %.2f)", 
        //     player.Name, position.X, position.Y, position.Z))
        
        this.playerPositions.set(userId, {
            playerId: userId,
            position: position,
            rotation: data.rotation || new Vector3(0, 0, 0),
            velocity: data.velocity || new Vector3(0, 0, 0),
            timestamp: tick()
        });
    }

    // 位置同步（固定频率，优化数据）
    private positionSync(deltaTime: number): void {
        this.positionSyncAccumulator = this.positionSyncAccumulator + deltaTime;
        
        // 固定20Hz同步频率
        const syncInterval = 0.05;
        
        if (this.positionSyncAccumulator < syncInterval) {
            return;
        }
        
        // 防止累积积压，直接重置
        this.positionSyncAccumulator = 0;
        
        // 收集所有玩家位置
        const allPositions: {
            playerId: number;
            position: Vector3;
            rotation: Vector3;
        }[] = [];
        for (const [userId, positionData] of this.playerPositions) {
            // 简化数据格式，减少网络开销
            const simplifiedData = {
                playerId: userId,
                position: positionData.position,
                rotation: new Vector3(0, positionData.rotation.Y || 0, 0)  // 只发送Y轴旋转
            };
            allPositions.push(simplifiedData);
        }
        
        // 广播给所有客户端
        if (allPositions.size() > 0) {
            (this.remotes.FindFirstChild("AllPlayersPosition") as RemoteEvent).FireAllClients(allPositions);
        }
    }
}

// 创建并启动网络管理器
const networkManager = new NetworkManager();

export default networkManager;
