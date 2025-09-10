// ClientNetworkManager.client.ts
// 简化的客户端网络管理器
// 本地赛车直接使用RigidbodyFPSWalker，只同步远程玩家状态
const Players = game.GetService("Players");
const ReplicatedStorage = game.GetService("ReplicatedStorage");
const Workspace = game.GetService("Workspace");
const RunService = game.GetService("RunService");

// 获取 KartEngine
const KartEngine = ReplicatedStorage.WaitForChild("TS").WaitForChild("kartEngine");

const player = Players.LocalPlayer;

// 类型声明
interface KartManagerModule {
    MAX_KART: number;
    Instance: {
        SetKart: (index: number, kart: unknown, controller: unknown, options: unknown) => unknown;
        goKart_: (unknown | undefined)[];
        goKartCount_: number;
    };
}

interface GoPlayKartBuilderModule {
    default: new() => {
        Build(): unknown;
    };
}

interface BasicPositionSyncModule {
    default: new(data: unknown) => {
        AddNetworkState: (state: unknown) => void;
        Update: (deltaTime: number) => void;
    };
}

interface Controller {
    gameObject?: Model;
    transform?: BasePart;
    kartIndex_?: number;
    StopAllCallbacks?: () => void;
}

interface GoPlayKart {
    m_KartWLVel?: Vector3;
}

interface RigidbodyWalker {
    goPlayKart_?: GoPlayKart;
}

interface RemotePlayerData {
    userId: number;
    name: string;
    kartModel: Model;
    kartIndex: number;
    goKart: unknown;
    controller: Controller;
    syncManager?: {
        AddNetworkState: (state: unknown) => void;
        Update: (deltaTime: number) => void;
    };
}

interface PositionData {
    playerId: number;
    position: Vector3;
    rotation: Vector3;
    velocity: Vector3;
}

interface PlayerData {
    playerId: number;
    playerName: string;
}

export class ClientNetworkManager {
    private remotes!: Folder;
    
    // 本地数据
    private localRigidbodyWalker?: RigidbodyWalker;    // 本地RigidbodyFPSWalker
    private localKartModel?: Model;        // 本地赛车模型
    
    // 远程玩家数据
    private remotePlayers: { [userId: number]: RemotePlayerData } = {};
    private nextRemoteKartIndex: number = 2;       // 远程玩家从索引2开始（1是本地玩家）
    
    // 同步相关（只同步位置，不同步输入）
    private positionSendRate: number = 20;         // 位置发送频率(Hz) - 20Hz 高频率    
    private positionSendAccumulator: number = 0;
    
    // 帧率统计
    private sendFrameCount: number = 0;            // 发送帧计数
    private recvFrameCount: number = 0;            // 接收帧计数
    private lastFrameReportTime: number = tick();  // 上次报告时间

    constructor() {
        // 等待网络基础设施
        this.remotes = ReplicatedStorage.WaitForChild("NetworkRemotes") as Folder;
        
        // 初始化连接
        this.setupConnections();
    }

    private setupConnections(): void {
        // 等待新的远程事件
        this.remotes.WaitForChild("AllPlayersInput", 5);
        this.remotes.WaitForChild("PlayerInput", 5);
        this.remotes.WaitForChild("AllPlayersPosition", 5);
        this.remotes.WaitForChild("PlayerPosition", 5);
        
        // 接收所有玩家的输入
        const allPlayersInput = this.remotes.FindFirstChild("AllPlayersInput") as RemoteEvent;
        if (allPlayersInput) {
            allPlayersInput.OnClientEvent.Connect((allInputs: any[]) => {
                this.processAllPlayersInput(allInputs);
            });
        } else {
            warn("[ClientNetworkManager] AllPlayersInput 事件未找到");
        }
        
        // 接收所有玩家的位置（用于校正）
        const allPlayersPosition = this.remotes.FindFirstChild("AllPlayersPosition") as RemoteEvent;
        if (allPlayersPosition) {
            allPlayersPosition.OnClientEvent.Connect((allPositions: PositionData[]) => {
                this.processAllPlayersPosition(allPositions);
            });
        }
        
        // 玩家加入事件
        const playerJoined = this.remotes.FindFirstChild("PlayerJoined") as RemoteEvent;
        playerJoined.OnClientEvent.Connect((playerData: PlayerData) => {
            this.onRemotePlayerJoin(playerData);
        });
        
        // 玩家离开事件
        const playerLeft = this.remotes.FindFirstChild("PlayerLeft") as RemoteEvent;
        playerLeft.OnClientEvent.Connect((userId: number) => {
            this.onRemotePlayerLeave(userId);
        });
        
        // 定期发送本地位置（不再发送输入）
        RunService.Heartbeat.Connect((deltaTime) => {
            this.sendLocalPosition(deltaTime);
            this.updateRemotePositions(deltaTime);  // 平滑位置更新
        });
    }

    setupLocalKart(kartModel: Model, rigidbodyWalker: RigidbodyWalker): void {
        this.localKartModel = kartModel;
        this.localRigidbodyWalker = rigidbodyWalker;
    }

    private collectLocalInput(): any {
        // 从Unity Input系统获取输入
        const UnityInput = require(KartEngine.FindFirstChild("KartShared")!.FindFirstChild("UnityInput") as ModuleScript) as {
            Input: {
                GetAxis: (axisName: string) => number;
                GetKey: (keyCode: Enum.KeyCode) => boolean;
            };
        };
        const Input = UnityInput.Input;
        
        const input = {
            horizontal: Input.GetAxis("Horizontal"),  // -1 到 1
            vertical: Input.GetAxis("Vertical"),      // -1 到 1
            drift: Input.GetKey(Enum.KeyCode.LeftShift),
            boost: Input.GetKey(Enum.KeyCode.Space)
        };
        
        return input;
    }

    // 发送本地位置到服务器
    private sendLocalPosition(deltaTime: number): void {
        if (!this.localKartModel) {
            return;
        }
        
        this.positionSendAccumulator = this.positionSendAccumulator + deltaTime;
        
        const sendInterval = 1 / this.positionSendRate;
        if (this.positionSendAccumulator < sendInterval) {
            return;
        }
        
        // 防止累积积压，直接重置而不是累加
        this.positionSendAccumulator = 0;
        
        // 收集位置信息
        const position = this.localKartModel.PrimaryPart?.Position || new Vector3(0, 0, 0);
        const positionData = {
            position: position,
            rotation: this.localKartModel.PrimaryPart?.Orientation || new Vector3(0, 0, 0),
            velocity: this.localRigidbodyWalker?.goPlayKart_?.m_KartWLVel || new Vector3(0, 0, 0)
        };
        
        // 更新帧计数
        this.sendFrameCount = this.sendFrameCount + 1;
        
        // 发送到服务器
        const playerPosition = this.remotes.FindFirstChild("PlayerPosition") as RemoteEvent;
        if (playerPosition) {
            playerPosition.FireServer(positionData);
        }
    }

    // 处理所有玩家的位置（位置校正）
    private processAllPlayersPosition(allPositions: PositionData[]): void {
        // 正常处理所有位置更新
        for (const positionData of allPositions) {
            if (positionData.playerId !== player.UserId) {
                this.updateRemotePosition(positionData);
            }
        }
    }

    // 更新远程玩家位置（使用NetworkSyncManager）
    private updateRemotePosition(positionData: PositionData): void {
        const remotePlayer = this.remotePlayers[positionData.playerId];
        if (!remotePlayer) {
            return;
        }
        
        // 使用NetworkSyncManager处理位置同步
        if (remotePlayer.syncManager) {
            remotePlayer.syncManager.AddNetworkState(positionData);
        }
    }

    // 更新所有远程玩家（每帧调用）
    private updateRemotePositions(deltaTime: number): void {
        // 使用 pairs() 函数遍历表
        for (const [userId, remotePlayer] of pairs(this.remotePlayers)) {
            if (remotePlayer && remotePlayer.syncManager) {
                remotePlayer.syncManager.Update(deltaTime);
            }
        }
    }

    private onRemotePlayerJoin(playerData: PlayerData): void {
        if (playerData.playerId === player.UserId) {
            return; // 忽略自己
        }
        
        // 创建远程玩家的赛车模型
        this.createRemoteKart(playerData);
    }

    private processAllPlayersInput(allInputs: any[]): void {
        // 不再处理输入，只同步位置
        // 输入同步已废弃
    }

    private createRemoteKart(playerData: PlayerData): void {
        // 查找原始赛车模型
        const originalKart = Workspace.FindFirstChild("Kart") as Model;
        if (!originalKart) {
            warn("[ClientNetworkManager] 找不到原始Kart模型");
            return;
        }
        
        // 检查是否还有可用的赛车索引
        const KartManager = require(KartEngine.FindFirstChild("KartMove")!.FindFirstChild("KartManager") as ModuleScript) as KartManagerModule;
        if (this.nextRemoteKartIndex > KartManager.MAX_KART) {
            warn("[ClientNetworkManager] 达到最大赛车数量限制", KartManager.MAX_KART);
            return;
        }
        
        // 克隆赛车模型
        const remoteKart = originalKart.Clone();
        remoteKart.Name = playerData.playerName + "_RemoteKart";
        remoteKart.SetAttribute("PlayerId", playerData.playerId);
        remoteKart.SetAttribute("IsRemote", true);
        
        // 设置初始位置（默认生成位置），保持原始模板的方向
        const spawnPosition = new Vector3(355, 1, 84);  // 默认生成位置
        
        // 获取原始模板的旋转
        let originalRotation = new CFrame();
        if (originalKart.PrimaryPart) {
            originalRotation = originalKart.PrimaryPart.CFrame.sub(originalKart.PrimaryPart.Position);
        }
        
        if (remoteKart.PrimaryPart) {
            remoteKart.SetPrimaryPartCFrame(new CFrame(spawnPosition).mul(originalRotation));
        } else {
            // 如果没有PrimaryPart，尝试设置
            const primaryPart = remoteKart.FindFirstChild("PrimaryPart") as BasePart;
            if (primaryPart && primaryPart.IsA("BasePart")) {
                remoteKart.PrimaryPart = primaryPart;
                remoteKart.SetPrimaryPartCFrame(new CFrame(spawnPosition).mul(originalRotation));
            }
        }
        
        remoteKart.Parent = Workspace;
        
        // 分配kartIndex
        const kartIndex = this.nextRemoteKartIndex;
        this.nextRemoteKartIndex = this.nextRemoteKartIndex + 1;
        
        // 创建远程赛车的GoPlayKart实例并注册到KartManager
        const GoPlayKartBuilder = require(KartEngine.FindFirstChild("GameStage")!.FindFirstChild("GoPlayKartBuilder") as ModuleScript) as GoPlayKartBuilderModule;
        
        // 创建一个简单的控制器用于远程赛车（不需要物理模拟）
        // 保持与本地赛车一致的结构，包装在 gameObject 中
        const remoteController = {
            gameObject: remoteKart,
            transform: remoteKart.PrimaryPart,
            kartIndex_: kartIndex
        };
        
        // 使用KartManager注册远程赛车
        const goKart = KartManager.Instance.SetKart(kartIndex, new GoPlayKartBuilder.default(), remoteController, {});
        
        // 保存远程玩家数据
        const remotePlayerData: RemotePlayerData = {
            userId: playerData.playerId,
            name: playerData.playerName,
            kartModel: remoteKart,
            kartIndex: kartIndex,
            goKart: goKart,
            controller: remoteController
        };
        
        // 使用基础位置同步
        const BasicPositionSync = require(KartEngine.FindFirstChild("KartClient")!.FindFirstChild("BasicPositionSync") as ModuleScript) as BasicPositionSyncModule;
        remotePlayerData.syncManager = new BasicPositionSync.default(remotePlayerData);
        
        this.remotePlayers[playerData.playerId] = remotePlayerData;
    }

    private onRemotePlayerLeave(userId: number): void {
        if (userId === player.UserId) {
            return; // 忽略自己
        }
        
        const remotePlayer = this.remotePlayers[userId];
        if (remotePlayer) {
            // 清理KartManager中的赛车实例
            const KartManager = require(KartEngine.FindFirstChild("KartMove")!.FindFirstChild("KartManager") as ModuleScript) as KartManagerModule;
            if (remotePlayer.kartIndex) {
                // 清理KartManager中对应索引的赛车
                KartManager.Instance.goKart_[remotePlayer.kartIndex] = undefined;
                KartManager.Instance.goKartCount_ = KartManager.Instance.goKartCount_ - 1;
                
                // 回收索引，让下一个加入的玩家可以使用
                if (remotePlayer.kartIndex < this.nextRemoteKartIndex) {
                    this.nextRemoteKartIndex = remotePlayer.kartIndex;
                }
            }
            
            // 停止RigidbodyWalker的更新
            if (remotePlayer.controller && remotePlayer.controller.StopAllCallbacks) {
                remotePlayer.controller.StopAllCallbacks();
            }
            
            // 销毁赛车模型
            if (remotePlayer.kartModel) {
                remotePlayer.kartModel.Destroy();
            }
            
            // 清理数据
            delete this.remotePlayers[userId];
        }
    }
}
