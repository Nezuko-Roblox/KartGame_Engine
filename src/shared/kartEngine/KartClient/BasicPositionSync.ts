// BasicPositionSync.ts
// 使用影子追随算法的位置同步方案

import { RunService } from "@rbxts/services";

// 定义接口
interface RemotePlayer {
    kartModel: Model;
}

interface PositionData {
    X?: number;
    Y?: number;
    Z?: number;
}

interface RotationData {
    Y?: number;
}

interface NetworkState {
    position?: PositionData | Vector3;
    rotation?: RotationData | Vector3 | number;
}

// 配置参数（优化后）
const SYNC_CONFIG = {
    SHADOW_DISTANCE: 5,          // 影子跟随距离
    FOLLOW_SPEED: 50,            // 跟随速度（单位/秒）
    SNAP_DISTANCE: 100,          // 瞬移距离阈值
    SMOOTHING: 0.9,              // 平滑系数（0-1，越大越平滑）
    MAX_SPEED_MULTIPLIER: 2,    // 最大速度倍数（更平滑的加速）
    ROTATION_SMOOTHING: 0.08,   // 旋转平滑系数（降低以获得更平滑的旋转）
    ROTATION_SPEED_MAX: 180,    // 最大旋转速度（度/秒）
};

export class BasicPositionSync {
    private remotePlayer: RemotePlayer;
    private kartModel: Model;
    
    // 影子位置（网络接收的实际位置）
    private shadowPosition: Vector3 = new Vector3(0, 0, 0);
    private shadowRotation: number = 0;
    
    // 显示位置（平滑后的位置）
    private displayPosition: Vector3 = new Vector3(0, 0, 0);
    private displayRotation: number = 0;
    
    // 速度（用于平滑移动）
    private velocity: Vector3 = new Vector3(0, 0, 0);
    
    // 旋转速度（用于平滑旋转）
    private rotationVelocity: number = 0;  // 当前旋转速度

    constructor(remotePlayer: RemotePlayer) {
        this.remotePlayer = remotePlayer;
        this.kartModel = remotePlayer.kartModel;
        
        // 初始化位置
        if (this.kartModel && this.kartModel.PrimaryPart) {
            this.displayPosition = this.kartModel.PrimaryPart.Position;
            this.shadowPosition = this.displayPosition;
            
            // 设置为幽灵车
            for (const part of this.kartModel.GetDescendants()) {
                if (part.IsA("BasePart")) {
                    (part as BasePart).CanCollide = true;
                    (part as BasePart).CanTouch = true;
                    (part as BasePart).CanQuery = true;
                }
            }
        }
    }

    AddNetworkState(state: NetworkState): void {
        // 更新影子位置（网络接收的实际位置）
        if (state.position) {
            if (typeOf(state.position) === "table") {
                const posData = state.position as PositionData;
                this.shadowPosition = new Vector3(
                    posData.X || 0,
                    posData.Y || 0,
                    posData.Z || 0
                );
            } else {
                this.shadowPosition = state.position as Vector3;
            }
        }
        
        // 更新影子旋转
        if (state.rotation) {
            if (typeOf(state.rotation) === "table") {
                const rotData = state.rotation as RotationData;
                this.shadowRotation = rotData.Y || 0;
            } else if (typeOf(state.rotation) === "Vector3") {
                this.shadowRotation = (state.rotation as Vector3).Y;
            } else {
                this.shadowRotation = state.rotation as number;
            }
        }
    }

    Update(deltaTime: number): void {
        if (!this.kartModel || !this.kartModel.PrimaryPart) {
            return;
        }
        
        // 统一的弹簧-阻尼系统（无阶段切换）
        const toShadow = this.shadowPosition.sub(this.displayPosition);
        const distance = toShadow.Magnitude;
        
        // 根据距离决定处理方式
        if (distance > SYNC_CONFIG.SNAP_DISTANCE) {
            // 距离太远，直接瞬移
            this.displayPosition = this.shadowPosition;
            this.displayRotation = this.shadowRotation;
            this.velocity = new Vector3(0, 0, 0);
        } else {
            // 使用统一的弹簧-阻尼系统（避免阶段切换造成的卡顿）
            
            // 动态调整参数（基于距离）
            const distanceFactor = math.min(distance / 10, 1);  // 0到1的平滑过渡
            
            // 弹簧强度随距离增加（远处追得快，近处追得慢）
            const springStrength = 8.0 + distanceFactor * 12.0;  // 8-20的范围
            
            // 阻尼系数（临界阻尼）
            const criticalDamping = 2 * math.sqrt(springStrength);
            const dampingRatio = 0.9;  // 稍微欠阻尼，更快响应
            const damping = dampingRatio * criticalDamping;
            
            // 计算力
            const springForce = toShadow.mul(springStrength);
            const dampingForce = this.velocity.mul(-damping);
            
            // 计算加速度
            const acceleration = springForce.add(dampingForce);
            
            // 更新速度（带速度限制）
            this.velocity = this.velocity.add(acceleration.mul(deltaTime));
            
            // 动态速度限制（距离越远，允许的速度越大）
            const maxVelocity = 30 + distanceFactor * 70;  // 30-100的范围
            if (this.velocity.Magnitude > maxVelocity) {
                this.velocity = this.velocity.Unit.mul(maxVelocity);
            }
            
            // 更新位置
            this.displayPosition = this.displayPosition.add(this.velocity.mul(deltaTime));
            
            // 添加微小的阻尼，防止永远振荡
            if (distance < 0.5) {
                this.velocity = this.velocity.mul(0.98);
            }
        }
        
        // 3. 处理旋转（简单的弹簧-阻尼系统，无预测）
        let angleDiff = this.shadowRotation - this.displayRotation;
        
        // 处理360度边界（选择最短旋转路径）
        if (angleDiff > 180) {
            angleDiff = angleDiff - 360;
        } else if (angleDiff < -180) {
            angleDiff = angleDiff + 360;
        }
        
        // 使用二阶系统（弹簧-阻尼模型）进行平滑
        const springStrength = 12.0;  // 弹簧强度（响应速度）
        const damping = 0.8;          // 阻尼系数（临界阻尼）
        
        // 计算加速度
        const springForce = angleDiff * springStrength;
        const dampingForce = -this.rotationVelocity * damping * 2 * math.sqrt(springStrength);
        const rotationAcceleration = springForce + dampingForce;
        
        // 更新速度和位置
        this.rotationVelocity = this.rotationVelocity + rotationAcceleration * deltaTime;
        
        // 限制最大旋转速度
        const maxVelocity = 360;  // 度/秒
        if (math.abs(this.rotationVelocity) > maxVelocity) {
            this.rotationVelocity = (this.rotationVelocity / math.abs(this.rotationVelocity)) * maxVelocity;
        }
        
        // 更新显示旋转
        this.displayRotation = this.displayRotation + this.rotationVelocity * deltaTime;
        
        // 确保角度在 0-360 范围内
        while (this.displayRotation > 360) {
            this.displayRotation = this.displayRotation - 360;
        }
        while (this.displayRotation < 0) {
            this.displayRotation = this.displayRotation + 360;
        }
        
        // 4. 应用到模型
        this.kartModel.SetPrimaryPartCFrame(
            new CFrame(this.displayPosition).mul(
                CFrame.Angles(0, math.rad(this.displayRotation), 0)
            )
        );
    }

    Destroy(): void {
        // 清理
    }
}
