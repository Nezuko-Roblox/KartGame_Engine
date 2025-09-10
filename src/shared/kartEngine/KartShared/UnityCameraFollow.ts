import { RunService, Workspace } from "@rbxts/services";
import { UnityVector3, Mathf } from "./UnityMath";

interface GoPlayKart {
    m_KartRealVelocity?: UnityVector3;
}

export interface RigidbodyWalker {
    goPlayKart_?: GoPlayKart;
}

export class UnityCameraFollow {
    // 相机设置
    private camera: Camera;
    private target?: Model | BasePart;
    private rigidbodyWalker?: RigidbodyWalker;
    
    // Unity风格的相机参数
    private height: number = 2.0;
    private distance: number = 10.0;
    private heightDamping: number = 8.0;  // 增加高度跟随速度
    private rotationDamping: number = 15.0;  // 增加旋转跟随速度
    private followSpeed: number = 8.0;  // 增加位置跟随速度
    private lookAhead: number = 2.0;
    
    // 内部状态
    private currentHeight: number = 0;
    private wantedHeight: number = 0;
    
    // 连接
    private connection?: RBXScriptConnection;

    constructor(camera?: Camera, target?: Model | BasePart, rigidbodyWalker?: RigidbodyWalker) {
        this.camera = camera || (Workspace.CurrentCamera as Camera);
        this.target = target;
        this.rigidbodyWalker = rigidbodyWalker;
    }

    // 开始相机跟随
    Start(): void {
        if (this.connection) {
            this.connection.Disconnect();
        }
        
        this.connection = RunService.Heartbeat.Connect((deltaTime) => {
            this.UpdateCamera(deltaTime);
        });
    }

    // 停止相机跟随
    Stop(): void {
        if (this.connection) {
            this.connection.Disconnect();
            this.connection = undefined;
        }
    }

    // Unity风格的相机更新逻辑
    private UpdateCamera(deltaTime: number): void {
        if (!this.target) {
            return;
        }
        
        // 获取目标位置和方向?        
        const targetPosition = this.GetTargetPosition();
        const targetForward = this.GetTargetForward();
        const targetVelocity = this.GetTargetVelocity();
        
        // 检查获取的数据是否有效
        if (!targetPosition || !targetForward) {
            return;
        }
        
        // 计算相机方向 - 优先使用车头朝向，响应更快
        const cameraDirection = targetForward;
        
        // 计算期望高度
        this.wantedHeight = targetPosition.Y + this.height;
        this.currentHeight = this.Lerp(this.currentHeight, this.wantedHeight, this.heightDamping * deltaTime);
        
        // 计算相机位置（在目标后方）
        const backwardDirection = new UnityVector3(-cameraDirection.X, -cameraDirection.Y, -cameraDirection.Z);
        let desiredPosition = new UnityVector3(
            targetPosition.X + backwardDirection.X * this.distance,
            targetPosition.Y + backwardDirection.Y * this.distance,
            targetPosition.Z + backwardDirection.Z * this.distance
        );
        desiredPosition = new UnityVector3(desiredPosition.X, this.currentHeight, desiredPosition.Z);
        
        // 应用平滑跟随
        const currentPos = this.RobloxToUnityVector3(this.camera.CFrame.Position);
        const smoothedPosition = this.LerpVector3(currentPos, desiredPosition, this.followSpeed * deltaTime);
        
        // 转换为Roblox坐标系并设置相机
        const robloxCameraPos = this.UnityToRobloxVector3(smoothedPosition);
        const robloxTargetPos = this.UnityToRobloxVector3(targetPosition);
        
        // 确保转换成功（应该是Roblox Vector3 userdata类型）
        if (!robloxCameraPos || !robloxTargetPos) {
            warn("相机位置转换失败!");
            return;
        }
        
        // 使用简单的CFrame.lookAt方式，如果失败则使用备用方案
        const [success, cameraCFrame] = pcall(() => {
            return CFrame.lookAt(robloxCameraPos, robloxTargetPos);
        });
        
        if (success) {
            this.camera.CFrame = cameraCFrame as CFrame;
        } else {
            // 备用方案：手动构造CFrame
            warn("CFrame.lookAt失败，使用备用方案");
            const lookDirection = robloxTargetPos.sub(robloxCameraPos).Unit;
            this.camera.CFrame = new CFrame(robloxCameraPos, robloxCameraPos.add(lookDirection));
        }
    }

    // 获取目标位置
    private GetTargetPosition(): UnityVector3 | undefined {
        if (!this.target) {
            return undefined;
        }
        
        if (this.target.IsA("Model")) {
            const primaryPart = (this.target as Model).PrimaryPart || (this.target as Model).FindFirstChildOfClass("BasePart");
            if (primaryPart) {
                return this.RobloxToUnityVector3(primaryPart.Position);
            } else {
                return UnityVector3.zero;
            }
        } else {
            return this.RobloxToUnityVector3((this.target as BasePart).Position);
        }
    }

    // 获取目标前进方向
    private GetTargetForward(): UnityVector3 | undefined {
        if (!this.target) {
            return undefined;
        }
        
        if (this.target.IsA("Model")) {
            const primaryPart = (this.target as Model).PrimaryPart || (this.target as Model).FindFirstChildOfClass("BasePart");
            if (primaryPart) {
                // 使用 -LookVector 作为前进方向，与Unity逻辑保持一致
                const lookVec = primaryPart.CFrame.LookVector;
                return this.RobloxToUnityVector3(new Vector3(-lookVec.X, -lookVec.Y, -lookVec.Z));
            } else {
                return new UnityVector3(0, 0, -1);
            }
        } else {
            // 使用 -LookVector 作为前进方向，与Unity逻辑保持一致
            const lookVec = (this.target as BasePart).CFrame.LookVector;
            return this.RobloxToUnityVector3(new Vector3(-lookVec.X, -lookVec.Y, -lookVec.Z));
        }
    }

    // 获取目标速度
    private GetTargetVelocity(): UnityVector3 {
        if (this.rigidbodyWalker && this.rigidbodyWalker.goPlayKart_) {
            const kartVelocity = this.rigidbodyWalker.goPlayKart_.m_KartRealVelocity;
            if (kartVelocity) {
                return kartVelocity;
            }
        }
        return UnityVector3.zero;
    }

    // 坐标系转换函数
    private RobloxToUnityVector3(robloxVec3: Vector3): UnityVector3 {
        if (!robloxVec3) {
            return UnityVector3.zero;
        }
        // 从 Roblox Vector3 转换为 Unity Vector3 实例
        return new UnityVector3(robloxVec3.X, robloxVec3.Y, robloxVec3.Z);
    }

    private UnityToRobloxVector3(unityVec3: UnityVector3): Vector3 {
        if (!unityVec3) {
            return new Vector3(0, 0, 0);
        }
        
        // 从 Unity Vector3 转换为 Roblox Vector3
        return new Vector3(unityVec3.X, unityVec3.Y, unityVec3.Z);
    }

    // 设置相机参数
    SetDistance(distance: number): void {
        this.distance = distance;
    }

    SetHeight(height: number): void {
        this.height = height;
    }

    SetDamping(heightDamping: number, rotationDamping: number): void {
        this.heightDamping = heightDamping;
        this.rotationDamping = rotationDamping;
    }

    SetTarget(target: Model | BasePart): void {
        this.target = target;
    }

    // 辅助数学函数
    private Lerp(a: number, b: number, t: number): number {
        const clampedT = math.min(math.max(t, 0), 1);
        return a + (b - a) * clampedT;
    }

    private LerpVector3(a: UnityVector3, b: UnityVector3, t: number): UnityVector3 {
        if (!a || !b) {
            return a || b || UnityVector3.zero;
        }
        
        const clampedT = math.min(math.max(t, 0), 1);
        return new UnityVector3(
            a.X + (b.X - a.X) * clampedT,
            a.Y + (b.Y - a.Y) * clampedT,
            a.Z + (b.Z - a.Z) * clampedT
        );
    }

    // 清理资源
    Destroy(): void {
        this.Stop();
    }
}
