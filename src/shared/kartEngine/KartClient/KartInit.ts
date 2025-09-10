// KartInit.client.ts
// 客户端赛车初始化脚本（本地使用RigidbodyFPSWalker + 网络同步远程玩家）
import { Players, Workspace, RunService, CollectionService } from "@rbxts/services";
import { RigidbodyFPSWalker } from "../KartMove/RigidbodyFPSWalker";
import { UnityCameraFollow } from "../KartShared/UnityCameraFollow";

// 尝试加载网络管理器
let ClientNetworkManagerClass: unknown = undefined;
let clientNetworkManager: unknown = undefined;
let networkEnabled = false;

// 检查是否存在网络模块
let success: boolean;
let err: unknown;
[success, err] = pcall(() => {
    // ClientNetworkManagerClass = require(KartEngine.KartClient.ClientNetworkManager)
    // 注释掉网络管理器的导入，因为还未完全转换
    networkEnabled = false; // 暂时设为 false
});

print("版本：0.2");
if (!success) {
    warn("[KartInit] 网络模块未找到，使用单机模式:", err);
}

const player = Players.LocalPlayer;

// 等待角色和赛车模型
function waitForKartModel(): Model | undefined {
    const kartModel = Workspace.WaitForChild("Kart") as Model | undefined;  // 改为查找名为Kart的Model（注意大写）
    if (kartModel) {
        // 确保是Model类型
        if (!kartModel.IsA("Model")) {
            warn("KartController: kart不是Model，是", (kartModel as unknown as { ClassName: string }).ClassName);
            return undefined;
        }
        
        // 检查并设置PrimaryPart
        if (kartModel.PrimaryPart) {
            // PrimaryPart已存在
        } else {
            // 等待名为"PrimaryPart"的Part加载
            const primaryPart = kartModel.WaitForChild("PrimaryPart", 10) as BasePart | undefined;
            if (primaryPart && primaryPart.IsA("BasePart")) {
                kartModel.PrimaryPart = primaryPart;
            } else {
                warn("KartController: 等待PrimaryPart超时或Part不是BasePart");
            }
        }
        
        return kartModel;
    }
    warn("KartController: 找不到赛车模型");
    return undefined;
}

// 保护和禁用角色控制的函数
function protectCharacter(character: Model): void {
    if (!character) {
        return;
    }
    
    // 获取角色的Humanoid
    const humanoid = character.WaitForChild("Humanoid") as Humanoid;
    
    // 设置角色不死
    humanoid.MaxHealth = math.huge;  // 无限生命值
    humanoid.Health = math.huge;     // 当前生命值设为无限    
    // 禁用角色的默认移动
    humanoid.WalkSpeed = 0;
    humanoid.JumpPower = 0;
    humanoid.JumpHeight = 0;
    humanoid.AutoRotate = false;
    
    // 防止角色受到伤害
    humanoid.HealthChanged.Connect(() => {
        if (humanoid.Health < humanoid.MaxHealth) {
            humanoid.Health = humanoid.MaxHealth; // 立即恢复满血
        }
    });
    
    // 防止角色死亡
    humanoid.Died.Connect(() => {
        // 如果意外死亡，立即恢复
        humanoid.Health = humanoid.MaxHealth;
        humanoid.ChangeState(Enum.HumanoidStateType.Running);
    });
}

// 主要的赛车初始化函数
function initializeKart(): void {
    // 现在总是使用本地的Kart模型，不再等待服务器分配
    let kartModel = waitForKartModel();
    
    if (!kartModel) {
        print("[KartInit] 等待赛车模型...");
        let connection: RBXScriptConnection | undefined;
        connection = RunService.Heartbeat.Connect(() => {
            kartModel = waitForKartModel();
            if (kartModel) {
                connection!.Disconnect();
                initializeKart(); // 递归调用
            }
        });
        return;
    }
    
    // 给赛车添加Player标签
    CollectionService.AddTag(kartModel, "Player");
    
    // 等待玩家角色
    const character = (player.Character || player.CharacterAdded.Wait()) as unknown as Model;
    
    // 保护角色（不掉血、不重生、禁用控制）
    protectCharacter(character);
    
    // 创建RigidbodyFPSWalker（本地赛车直接使用它进行物理计算）
    const rigidbodyWalker = new RigidbodyFPSWalker(kartModel);
    
    print("[KartInit] 本地赛车使用RigidbodyFPSWalker进行物理计算");
    
    // 设置Unity风格的相机跟随
    const camera = Workspace.CurrentCamera;
    if (camera) {
        camera.CameraType = Enum.CameraType.Scriptable;
        
        // 创建Unity风格的相机跟随控制器
        const cameraFollow = new UnityCameraFollow(camera, kartModel, rigidbodyWalker);
        
        // 设置相机参数（可根据需要调整）
        cameraFollow.SetDistance(12.0);  // 跟随距离
        cameraFollow.SetHeight(6.0);    // 相机高度
        cameraFollow.SetDamping(8.0, 25.0);  // 高度阻尼, 旋转阻尼 - 大幅提高旋转响应速度
        
        // 开始相机跟随
        cameraFollow.Start();
        
        // 将相机控制器保存到rigidbodyWalker中，方便后续访问
        if (rigidbodyWalker) {
            (rigidbodyWalker as RigidbodyFPSWalker & { cameraFollow?: UnityCameraFollow }).cameraFollow = cameraFollow;
        }
    }
    
    // 网络模式下设置远程玩家同步
    if (networkEnabled && ClientNetworkManagerClass) {
        // 创建网络管理器实例用于同步远程玩家
        const NetworkManagerClass = ClientNetworkManagerClass as new () => {
            setupLocalKart: (kartModel: Model, rigidbodyWalker: RigidbodyFPSWalker) => void;
        };
        clientNetworkManager = new NetworkManagerClass();
        (clientNetworkManager as { setupLocalKart: (kartModel: Model, rigidbodyWalker: RigidbodyFPSWalker) => void }).setupLocalKart(kartModel, rigidbodyWalker);
        print("[KartInit] 网络同步已启用 - 本地使用RigidbodyFPSWalker，远程玩家状态同步");
    }
}

// 启动初始化
if (networkEnabled) {
    print("[KartInit] 客户端脚本已加载（网络模式 - 本地物理计算）");
} else {
    print("[KartInit] 客户端脚本已加载（单机模式）");
}

initializeKart();

// 返回空对象以满足 require
export default {};
