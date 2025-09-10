// Unity风格的触发器类 - 正确处理OnTriggerEnter/Stay/Exit生命周期
// 解决Roblox中触发器自动触发的问题
// 严格按照原Lua代码逻辑实现

import { RunService, CollectionService } from "@rbxts/services";

interface ITriggerOwner {
    gameObject: { GetRobloxObject(): Instance | undefined } | undefined;
    OnTriggerEnter(collider: BasePart): void;
    OnTriggerStay(collider: BasePart): void;
    OnTriggerExit?(collider: BasePart): void;
}

export class UnityTrigger {
    private owner: ITriggerOwner | undefined;
    private currentTriggers: BasePart[] = [];
    private lastFrameTriggers: BasePart[] = [];
    private isEnabled: boolean = true;
    private updateCount: number = 0;

    constructor(owner?: ITriggerOwner) {
        this.owner = owner;
    }

    // 启用/禁用触发器
    public SetEnabled(enabled: boolean): void {
        this.isEnabled = enabled;
    }

    // 检测对象是否在触发范围内 - 使用真正的碰撞体重叠检测 - 严格按照原Lua逻辑
    public IsInTriggerRange(targetObject: BasePart): boolean {
        if (!this.owner || !this.owner.gameObject || !targetObject) {
            return false;
        }

        // 获取卡丁车的真实Roblox对象
        const kartRobloxObject = this.owner.gameObject.GetRobloxObject();
        if (!kartRobloxObject || !kartRobloxObject.IsA("BasePart")) {
            // 如果卡丁车是Model，尝试获取PrimaryPart
            if (kartRobloxObject && kartRobloxObject.IsA("Model")) {
                const model = kartRobloxObject as Model;
                const primaryPart = model.PrimaryPart || model.FindFirstChildOfClass("Part");
                if (primaryPart) {
                    return this.CheckOverlap(primaryPart, targetObject);
                }
            }
            return false;
        }

        // 获取卡丁车的位置和大小（模拟BoxCollider）
        const kartPart = kartRobloxObject as BasePart;
        return this.CheckOverlap(kartPart, targetObject);
    }

    // 辅助方法：检查两个BasePart是否重叠
    private CheckOverlap(kartPart: BasePart, targetObject: BasePart): boolean {
        const kartPos = kartPart.Position;
        const kartSize = kartPart.Size;

        // 创建一个稍大的检测区域（模拟Unity的触发器）
        const triggerSize = kartSize.mul(1.5); // 增加触发器范围

        // 使用简单但准确的边界盒重叠检测
        const kartMin = kartPos.sub(triggerSize.div(2));
        const kartMax = kartPos.add(triggerSize.div(2));

        const targetMin = targetObject.Position.sub(targetObject.Size.div(2));
        const targetMax = targetObject.Position.add(targetObject.Size.div(2));

        // 检查两个AABB是否重叠
        const overlapX = kartMax.X >= targetMin.X && kartMin.X <= targetMax.X;
        const overlapY = kartMax.Y >= targetMin.Y && kartMin.Y <= targetMax.Y;
        const overlapZ = kartMax.Z >= targetMin.Z && kartMin.Z <= targetMax.Z;

        const result = overlapX && overlapY && overlapZ;
        
        return result;
    }

    // 更新触发器状态（在FixedUpdate中调用） - 严格按照原Lua逻辑
    public UpdateTriggers(): void {
        if (!this.isEnabled) {
            return;
        }

        // 清空当前帧触发列表
        this.currentTriggers = [];

        // 检测所有可能的触发对象（不需要特定标签）
        const allObjects: BasePart[] = [];

        // 添加Terrain
        allObjects.push(game.Workspace.Terrain as any);

        // 获取workspace中所有BasePart（临时移除过滤，查看所有对象）
        for (const obj of game.Workspace.GetDescendants()) {
            if (obj.IsA("BasePart") && obj.CanTouch && obj.CanCollide) {
                // 排除赛车本身
                if (this.owner && this.owner.gameObject) {
                    const kartObject = this.owner.gameObject.GetRobloxObject();
                    if (kartObject && obj !== kartObject && !obj.IsDescendantOf(kartObject)) {
                        allObjects.push(obj);
                    }
                } else {
                    allObjects.push(obj);
                }
            }
        }

        let foundTriggers = 0;
        for (const obj of allObjects) {
            if (this.IsInTriggerRange(obj)) {
                this.currentTriggers.push(obj);
                foundTriggers++;
            }
        }

        // 每隔60帧输出一次调试信息
        this.updateCount++;
        
        // 处理触发事件
        this.ProcessTriggerEvents();

        // 更新上一帧状态 - 严格按照原Lua逻辑
        this.lastFrameTriggers = [];
        for (const obj of this.currentTriggers) {
            this.lastFrameTriggers.push(obj);
        }
    }

    // 处理触发器事件（Enter/Stay/Exit） - 严格按照原Lua逻辑
    private ProcessTriggerEvents(): void {
        // OnTriggerEnter - 当前帧有但上一帧没有的对象
        for (const currentObj of this.currentTriggers) {
            if (!this.WasTriggeredLastFrame(currentObj)) {
                this.CallOnTriggerEnter(currentObj);
            }
        }

        // OnTriggerStay - 当前帧有且上一帧也有的对象 (但每帧最多只调用一次)
        let hasStayEvent = false;
        for (const currentObj of this.currentTriggers) {
            if (this.WasTriggeredLastFrame(currentObj) && !hasStayEvent) {
                this.CallOnTriggerStay(currentObj);
                hasStayEvent = true; // 确保每帧最多只调用一次Stay
            }
        }

        // OnTriggerExit - 上一帧有但当前帧没有的对象
        for (const lastObj of this.lastFrameTriggers) {
            if (!this.IsTriggeredThisFrame(lastObj)) {
                this.CallOnTriggerExit(lastObj);
            }
        }
    }

    // 检查对象在上一帧是否被触发 - 严格按照原Lua逻辑
    private WasTriggeredLastFrame(obj: BasePart): boolean {
        for (const lastObj of this.lastFrameTriggers) {
            if (lastObj === obj) {
                return true;
            }
        }
        return false;
    }

    // 检查对象在当前帧是否被触发 - 严格按照原Lua逻辑
    private IsTriggeredThisFrame(obj: BasePart): boolean {
        for (const currentObj of this.currentTriggers) {
            if (currentObj === obj) {
                return true;
            }
        }
        return false;
    }

    // 调用OnTriggerEnter - 严格按照原Lua逻辑
    private CallOnTriggerEnter(collider: BasePart): void {
        if (this.owner) {
            this.owner.OnTriggerEnter(collider);
        }
    }

    // 调用OnTriggerStay - 严格按照原Lua逻辑
    private CallOnTriggerStay(collider: BasePart): void {
        if (this.owner) {
            this.owner.OnTriggerStay(collider);
        }
    }

    // 调用OnTriggerExit - 严格按照原Lua逻辑
    private CallOnTriggerExit(collider: BasePart): void {
        if (this.owner && this.owner.OnTriggerExit) {
            this.owner.OnTriggerExit(collider);
        }
    }

    // 获取当前触发的对象数量 - 严格按照原Lua逻辑
    public GetTriggerCount(): number {
        return this.currentTriggers.size();
    }

    // 获取当前触发的对象列表 - 严格按照原Lua逻辑
    public GetTriggeredObjects(): readonly BasePart[] {
        return this.currentTriggers;
    }

    // 清理 - 严格按照原Lua逻辑
    public Destroy(): void {
        this.currentTriggers = [];
        this.lastFrameTriggers = [];
        this.owner = undefined;
    }
}
