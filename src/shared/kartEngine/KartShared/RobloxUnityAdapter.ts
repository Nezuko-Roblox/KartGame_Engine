// Roblox到Unity的适配器，提供Unity原生API的等效实现
// TEMPORARY FIX: roblox-ts does not support getters and setters
// All getter/setter properties are temporarily commented out to allow compilation
// TODO: Convert all getter/setter to method calls
import { RunService } from "@rbxts/services";
import { UnityVector3, Quaternion } from "./UnityMath";

// 保留对Roblox原生Vector3的引用
const RobloxVector3 = Vector3;

// 前向声明
export interface GameObject {
    gameObject: Instance;
    name: string;
    transform: Transform;
    GetComponent(componentType: string): any;
    GetComponentInChildren(componentType: string): any;
    GetComponentsInChildren(componentType: string): any[];
    AddComponent(componentType: string): Instance;
    SetActiveRecursively(active: boolean): void;
    Find(name: string): Instance | undefined;
    GetRobloxObject(): Instance | undefined;
}

// 简化版的适配器类，移除所有 getter/setter
export class RigidbodyAdapter {
    public GameObject: GameObject;
    private _currentVelocity?: UnityVector3;
    private _bodyAngularVelocity?: BodyAngularVelocity;
    private _bodyVelocity?: any;
    private _bodyPosition?: any;
    private _useGravity?: boolean;
    private _isKinematic?: boolean;

    // Helper method to check if gameObject is valid
    private isGameObjectValid(): boolean {
        return !!(this.GameObject && this.GameObject.gameObject);
    }

    constructor(gameObject: GameObject) {
        this.GameObject = gameObject;
        
        // 检查gameObject是否有效
        if (!this.isGameObjectValid()) {
            warn("RigidbodyAdapter: gameObject or gameObject.gameObject is nil, skipping initialization");
            warn(`RigidbodyAdapter constructor stack trace: ${debug.traceback()}`);
            return;
        }
        
        // 模拟Unity的Kinematic Rigidbody：设置所有BasePart为Anchored
        this.setKinematicMode(true);
        
        // 缓存身体组件引用（主要用于兼容性）- 仅在gameObject有效时执行
        if (this.isGameObjectValid()) {
            this._bodyAngularVelocity = this.GameObject.gameObject.FindFirstChild("BodyAngularVelocity") as BodyAngularVelocity;
            // 不再使用BodyVelocity和BodyPosition - 完全通过Transform控制位置
            this._bodyVelocity = undefined;
            this._bodyPosition = undefined;
            
            // 设置子物体位置同步系统（模拟Unity刚体行为）
            this.setupChildSync();
        }
    }

    // 设置Kinematic模式（模拟Unity的isKinematic = true）
    private setKinematicMode(isKinematic: boolean): void {
        // 检查gameObject是否有效
        if (!this.isGameObjectValid()) {
            warn("setKinematicMode: gameObject is nil, skipping");
            return;
        }
        
        const setAnchored = (parent: Instance, anchored: boolean) => {
            for (const child of parent.GetChildren()) {
                if (child.IsA("BasePart")) {
                    (child as BasePart).Anchored = anchored;
                } else if (child.IsA("Model") || child.IsA("Folder")) {
                    setAnchored(child, anchored);
                }
            }
        };
        
        if (this.GameObject.gameObject.IsA("BasePart")) {
            (this.GameObject.gameObject as BasePart).Anchored = isKinematic;
        } else {
            setAnchored(this.GameObject.gameObject, isKinematic);
        }
    }

    // Unity刚体子物体自动跟随的模拟
    private setupChildSync(): void {
        // 检查gameObject是否有效
        if (!this.isGameObjectValid()) {
            warn("setupChildSync: gameObject is nil, skipping");
            return;
        }
        
        const gameObject = this.GameObject.gameObject;
        
        // 对于Model，不需要子物体同步，Model本身会处理
        if (gameObject.IsA("Model")) {
            return;
        }
        
        const children: BasePart[] = [];
        const childOffsets: Map<BasePart, Vector3> = new Map();
        
        // 收集所有子物体并计算初始相对位置
        for (const child of gameObject.GetChildren()) {
            if (child.IsA("BasePart") && child !== gameObject) {
                children.push(child as BasePart);
                const relativePos = (gameObject as BasePart).CFrame.Inverse().mul((child as BasePart).CFrame);
                childOffsets.set(child as BasePart, relativePos.Position);
            }
        }
        
        if (children.size() === 0) return;
        
        let lastCarCFrame = (gameObject as BasePart).CFrame;
        
        // 使用Heartbeat进行高频率同步
        const connection = RunService.Heartbeat.Connect(() => {
            const currentCarCFrame = (gameObject as BasePart).CFrame;
            
            if (currentCarCFrame !== lastCarCFrame) {
                for (const child of children) {
                    if (child.Parent && childOffsets.has(child)) {
                        const offset = childOffsets.get(child)!;
                        const newWorldPos = currentCarCFrame.PointToWorldSpace(offset);
                        child.Position = newWorldPos;
                    }
                }
                lastCarCFrame = currentCarCFrame;
            }
        });
        
        // 当对象被销毁时断开连接
        gameObject.AncestryChanged.Connect(() => {
            if (!gameObject.Parent) {
                connection.Disconnect();
            }
        });
    }

    // 将 getter/setter 转换为方法
    public getVelocity(): UnityVector3 {
        return this._currentVelocity || UnityVector3.zero;
    }

    public setVelocity(value: UnityVector3): void {
        this._currentVelocity = new UnityVector3(value.X, value.Y, value.Z);
    }

    public getMass(): number {
        if (!this.isGameObjectValid()) {
            warn("getMass: gameObject is nil, returning 0");
            return 0;
        }
        
        if (this.GameObject.gameObject.IsA("BasePart")) {
            return (this.GameObject.gameObject as BasePart).Mass;
        } else {
            let totalMass = 0;
            for (const part of this.GameObject.gameObject.GetDescendants()) {
                if (part.IsA("BasePart")) {
                    totalMass += (part as BasePart).Mass;
                }
            }
            return totalMass;
        }
    }

    public setMass(value: number): void {
        // 检查gameObject是否有效
        if (!this.isGameObjectValid()) {
            warn("setMass: gameObject is nil, skipping");
            return;
        }
        
        // 设置质量的实现
        if (this.GameObject.gameObject.IsA("BasePart")) {
            const basePart = this.GameObject.gameObject as BasePart;
            const currentProps = basePart.CustomPhysicalProperties || new PhysicalProperties(Enum.Material.Plastic);
            const volume = basePart.Size.X * basePart.Size.Y * basePart.Size.Z;
            const density = value / volume;
            const newProps = new PhysicalProperties(
                density, 
                currentProps.Friction, 
                currentProps.Elasticity,
                currentProps.FrictionWeight,
                currentProps.ElasticityWeight
            );
            basePart.CustomPhysicalProperties = newProps;
        }
    }

    public getPosition(): UnityVector3 {
        if (!this.isGameObjectValid()) {
            warn("getPosition: gameObject is nil, returning zero vector");
            return new UnityVector3(0, 0, 0);
        }
        
        if (this.GameObject.gameObject.IsA("BasePart")) {
            const pos = (this.GameObject.gameObject as BasePart).Position;
            return new UnityVector3(pos.X, pos.Y, pos.Z);
        } else if ((this.GameObject.gameObject as Model).PrimaryPart) {
            const pos = (this.GameObject.gameObject as Model).PrimaryPart!.Position;
            return new UnityVector3(pos.X, pos.Y, pos.Z);
        } else {
            const firstPart = this.GameObject.gameObject.FindFirstChildOfClass("BasePart");
            if (firstPart) {
                const pos = (firstPart as BasePart).Position;
                return new UnityVector3(pos.X, pos.Y, pos.Z);
            } else {
                return UnityVector3.zero;
            }
        }
    }
}

// 简化的 Transform 类，移除所有 getter/setter
export class Transform {
    public _robloxObject: Instance;
    public gameObject: GameObject;
    public right: UnityVector3;
    public forward: UnityVector3;
    public up: UnityVector3;

    constructor(robloxObject: Instance, gameObject: GameObject) {
        this._robloxObject = robloxObject;
        this.gameObject = gameObject;
        
        // 初始化方向向量
        let targetPart: BasePart | undefined;
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
        }

        if (targetPart) {
            const rightVector = targetPart.CFrame.RightVector;
            const lookVector = targetPart.CFrame.LookVector;
            const upVector = targetPart.CFrame.UpVector;
            
            this.right = new UnityVector3(rightVector.X, rightVector.Y, rightVector.Z);
            // Unity标准坐标系转换：Roblox LookVector(-Z) 转换为 Unity forward(+Z)
            // 严格按照 Lua 版本的逻辑取反
            this.forward = new UnityVector3(-lookVector.X, -lookVector.Y, -lookVector.Z);
            this.up = new UnityVector3(upVector.X, upVector.Y, upVector.Z);
        } else {
            // 默认值
            this.right = new UnityVector3(1, 0, 0);
            this.forward = UnityVector3.forward;
            this.up = UnityVector3.up;
        }
        
        // 初始化localPosition属性
        this.localPosition = this.getLocalPosition();
    }

    public getPosition(): UnityVector3 {
        if (this._robloxObject.IsA("BasePart")) {
            const pos = (this._robloxObject as BasePart).Position;
            return new UnityVector3(pos.X, pos.Y, pos.Z);
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            const pos = (this._robloxObject as Model).PrimaryPart!.Position;
            return new UnityVector3(pos.X, pos.Y, pos.Z);
        }
        return UnityVector3.zero;
    }

    public setPosition(value: UnityVector3): void {
        if (this._robloxObject.IsA("BasePart")) {
            (this._robloxObject as BasePart).Position = new RobloxVector3(value.X, value.Y, value.Z);
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            (this._robloxObject as Model).SetPrimaryPartCFrame(
                new CFrame(new RobloxVector3(value.X, value.Y, value.Z))
            );
        }
    }

    // 添加localPosition方法，模拟Unity的本地位置
    public getLocalPosition(): Vector3 {
        if (this._robloxObject.IsA("BasePart")) {
            const pos = (this._robloxObject as BasePart).Position;
            return new Vector3(pos.X, pos.Y, pos.Z);
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            const pos = (this._robloxObject as Model).PrimaryPart!.Position;
            return new Vector3(pos.X, pos.Y, pos.Z);
        }
        return new Vector3(0, 0, 0);
    }

    public setLocalPosition(value: Vector3): void {
        if (this._robloxObject.IsA("BasePart")) {
            (this._robloxObject as BasePart).Position = value;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            (this._robloxObject as Model).SetPrimaryPartCFrame(
                new CFrame(value)
            );
        }
    }

    // 直接提供localPosition属性，模拟Unity的直接属性访问
    public localPosition: Vector3;



    // 转换为方法形式，避免 getter/setter
    public getForward(): UnityVector3 {
        let targetPart: BasePart | undefined;
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
        }

        if (targetPart) {
            const lookVector = targetPart.CFrame.LookVector;
            // Unity标准坐标系转换：Roblox LookVector(-Z) 转换为 Unity forward(+Z)
            return new UnityVector3(-lookVector.X, -lookVector.Y, -lookVector.Z);
        }
        return UnityVector3.forward;
    }

    public getRight(): UnityVector3 {
        let targetPart: BasePart | undefined;
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
        }

        if (targetPart) {
            const rightVector = targetPart.CFrame.RightVector;
            return new UnityVector3(rightVector.X, rightVector.Y, rightVector.Z);
        }
        return new UnityVector3(1, 0, 0); // Vector3.right equivalent
    }

    public getUp(): UnityVector3 {
        let targetPart: BasePart | undefined;
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
        }

        if (targetPart) {
            const upVector = targetPart.CFrame.UpVector;
            return new UnityVector3(upVector.X, upVector.Y, upVector.Z);
        }
        return UnityVector3.up;
    }

    public getLocalRotation(): Quaternion {
        let targetPart: BasePart | undefined;
        let isModel = false;
        
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
            isModel = true;
        }

        if (targetPart) {
            // 将 Roblox CFrame 转换为 Unity Quaternion
            const [, , , xx, yx, zx, xy, yy, zy, xz, yz, zz] = targetPart.CFrame.GetComponents();
            
            // 使用矩阵到四元数的转换
            const trace = xx + yy + zz;
            let w, x, y, z;
            
            if (trace > 0) {
                const s = math.sqrt(trace + 1) * 2;
                w = 0.25 * s;
                x = (zy - yz) / s;
                y = (xz - zx) / s;
                z = (yx - xy) / s;
            } else if (xx > yy && xx > zz) {
                const s = math.sqrt(1 + xx - yy - zz) * 2;
                w = (zy - yz) / s;
                x = 0.25 * s;
                y = (xy + yx) / s;
                z = (xz + zx) / s;
            } else if (yy > zz) {
                const s = math.sqrt(1 + yy - xx - zz) / 2;
                w = (xz - zx) / s;
                x = (xy + yx) / s;
                y = 0.25 * s;
                z = (yz + zy) / s;
            } else {
                const s = math.sqrt(1 + zz - xx - yy) * 2;
                w = (yx - xy) / s;
                x = (xz + zx) / s;
                y = (yz + zy) / s;
                z = 0.25 * s;
            }
            
            return new Quaternion(x, y, z, w);
        }
        return Quaternion.identity;
    }

    public setLocalRotation(value: Quaternion): void {
        let targetPart: BasePart | undefined;
        let isModel = false;
        
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
            isModel = true;
        }

        if (targetPart) {
            // 将 Unity Quaternion 转换为 Roblox CFrame
            const currentPos = targetPart.Position;
            
            // 四元数到旋转矩阵的转换
            const x = value.X;
            const y = value.Y;
            const z = value.Z;
            const w = value.w;
            
            const xx = x * x;
            const yy = y * y;
            const zz = z * z;
            const xy = x * y;
            const xz = x * z;
            const yz = y * z;
            const wx = w * x;
            const wy = w * y;
            const wz = w * z;
            
            const m00 = 1 - 2 * (yy + zz);
            const m01 = 2 * (xy - wz);
            const m02 = 2 * (xz + wy);
            const m10 = 2 * (xy + wz);
            const m11 = 1 - 2 * (xx + zz);
            const m12 = 2 * (yz - wx);
            const m20 = 2 * (xz - wy);
            const m21 = 2 * (yz + wx);
            const m22 = 1 - 2 * (xx + yy);
            
            const newCFrame = new CFrame(
                currentPos.X, currentPos.Y, currentPos.Z,
                m00, m01, m02,
                m10, m11, m12,
                m20, m21, m22
            );
            
            if (isModel) {
                (this._robloxObject as Model).SetPrimaryPartCFrame(newCFrame);
            } else {
                targetPart.CFrame = newCFrame;
            }
        }
    }

    public RotateAroundLocal(axis: UnityVector3, angle: number): void {
        let targetPart: BasePart | undefined;
        let isModel = false;
        
        if (this._robloxObject.IsA("BasePart")) {
            targetPart = this._robloxObject as BasePart;
        } else if (this._robloxObject.IsA("Model") && (this._robloxObject as Model).PrimaryPart) {
            targetPart = (this._robloxObject as Model).PrimaryPart!;
            isModel = true;
        }

        if (targetPart) {
            const currentCFrame = targetPart.CFrame;
            const robloxAxis = new RobloxVector3(axis.X, axis.Y, axis.Z);
            const rotationCFrame = CFrame.fromAxisAngle(robloxAxis, angle);
            const newCFrame = currentCFrame.mul(rotationCFrame);
            
            if (isModel) {
                (this._robloxObject as Model).SetPrimaryPartCFrame(newCFrame);
            } else {
                targetPart.CFrame = newCFrame;
            }
        }
    }
}

// 简化的其他适配器类，移除 getter/setter
export class ColliderAdapter {
    public GameObject: GameObject;

    constructor(gameObject: GameObject) {
        this.GameObject = gameObject;
    }
}

export class RendererAdapter {
    public GameObject: GameObject;

    constructor(gameObject: GameObject) {
        this.GameObject = gameObject;
    }
}

export class AnimationAdapter {
    public GameObject: GameObject;

    constructor(gameObject: GameObject) {
        this.GameObject = gameObject;
    }
}

// GameObject 类的实现
class GameObjectClass implements GameObject {
    public gameObject: Instance;
    public name: string;
    public transform: Transform;

    constructor(robloxObject: Instance) {
        if (!robloxObject) {
            warn("GameObjectClass constructor: robloxObject is nil, creating placeholder");
            // 创建一个临时的 Part 作为占位符
            const placeholder = new Instance("Part");
            placeholder.Name = "PlaceholderGameObject";
            placeholder.Parent = game.Workspace;
            this.gameObject = placeholder;
            this.name = placeholder.Name;
            this.transform = new Transform(placeholder, this);
        } else {
            this.gameObject = robloxObject;
            this.name = robloxObject.Name;
            this.transform = new Transform(robloxObject, this);
        }
    }

    public static create(robloxObject: Instance): GameObject {
        return new GameObjectClass(robloxObject);
    }

    public GetComponent(componentType: string): any {
        if (componentType === "Rigidbody") {
            return new RigidbodyAdapter(this);
        } else if (componentType === "Collider" || componentType === "BoxCollider") {
            return new ColliderAdapter(this);
        } else if (componentType === "Renderer") {
            return new RendererAdapter(this);
        } else if (componentType === "Animation") {
            return new AnimationAdapter(this);
        }
        return undefined;
    }

    public GetComponentInChildren(componentType: string): any {
        return this.GetComponent(componentType);
    }

    public GetComponentsInChildren(componentType: string): any[] {
        const component = this.GetComponent(componentType);
        return component ? [component] : [];
    }

    public AddComponent(componentType: string): Instance {
        const instance = new Instance(componentType as keyof CreatableInstances);
        instance.Parent = this.gameObject;
        return instance;
    }

    public SetActiveRecursively(active: boolean): void {
        const setVisible = (obj: Instance, visible: boolean) => {
            if (obj.IsA("BasePart")) {
                (obj as BasePart).Transparency = visible ? 0 : 1;
            } else if (obj.IsA("GuiObject")) {
                (obj as GuiObject).Visible = visible;
            }
            
            for (const child of obj.GetChildren()) {
                setVisible(child, visible);
            }
        };
        
        setVisible(this.gameObject, active);
    }

    public Find(name: string): Instance | undefined {
        return game.Workspace.FindFirstChild(name, true);
    }

    public GetRobloxObject(): Instance | undefined {
        if (!this.gameObject) {
            warn("GetRobloxObject: gameObject is nil, this may indicate an initialization problem");
            warn(`GetRobloxObject stack trace: ${debug.traceback()}`);
            return undefined;
        }
        return this.gameObject;
    }
}

// 导出所有类
const RobloxUnityAdapter = {
    GameObject: GameObjectClass,
    Transform,
    RigidbodyAdapter,
    ColliderAdapter,
    RendererAdapter,
    AnimationAdapter
};

export default RobloxUnityAdapter;
