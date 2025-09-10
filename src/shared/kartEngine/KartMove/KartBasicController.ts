/**
 * KartBasicController class的TypeScript实现，继承自MonoBehaviour
 */

import { MonoBehaviour } from "../KartShared/MonoBehaviour";
import { MathHelper } from "./MathHelper";
import { UnityVector3 } from "../KartShared/UnityMath";
import RobloxUnityAdapter from "../KartShared/RobloxUnityAdapter";

export enum PlayMode {
    NORMAL = 0,
    ANIMATION_PLAYING = 1
}

export class KartBasicController extends MonoBehaviour {
    // 常量
    public static readonly MAX_BOOSTER = 2;
    
    // 公共成员变量
    public kartBody: unknown;
    
    // 保护成员变量
    protected boosterWave_: Instance | undefined;
    protected booster_: (Instance | undefined)[] = [];
    protected character_: Instance | undefined;
    protected characterFace_: Instance | undefined;
    protected kartBody_: Instance | undefined;
    protected waterBombBubble_: Instance | undefined;
    protected waterFlyBubble_: Instance | undefined;
    protected shield_: Instance | undefined;
    protected guard_: Instance | undefined;
    protected flipEffect_: Instance | undefined;
    protected devilStartEffect_: Instance | undefined;
    protected devilPlayEffect_: Instance | undefined;
    protected isNoAnimationCharacter_: boolean = false;
    protected wheels_: (unknown | undefined)[] = [];
    protected kartIndex_: number = 0;
    protected flipRenderer_: Instance | undefined;
    protected playMode_: PlayMode = PlayMode.NORMAL;
    
    constructor(robloxObject?: Instance) {
        super("KartBasicController");
        
        // 初始化wheels_数组为1-based索引，完全按照Lua逻辑
        for (let i = 1; i <= 4; i++) {
            this.wheels_[i] = undefined;
        }
        
        // 如果提供了robloxObject，立即设置GameObject
        if (robloxObject) {
            print(`[KartBasicController] 构造函数 - robloxObject: ${robloxObject.Name}`);
            this.SetGameObject(robloxObject);
            print(`[KartBasicController] 构造函数 - gameObject设置后: ${this.gameObject ? "已设置" : "nil"}`);
        } else {
            warn("[KartBasicController] robloxObject is nil!");
        }
        
        // 初始化轮子数组，从下标1开始（对应Unity原版）
        this.wheels_ = [];
        this.wheels_[0] = undefined; // 下标0不使用
        for (let i = 1; i <= 4; i++) {
            this.wheels_[i] = undefined;
        }
    }
    
    public Awake(): void {
        // 基类的Awake实现为空
    }
    
    public Start(): void {
        // 基类的Start实现为空
    }
    
    public Update(): void {
        // 基类的Update实现为空
    }
    
    public FixedUpdate(): void {
        // 翻转特效处理 - 适配Roblox
        if (this.flipEffect_ !== undefined && this.flipEffect_.Parent !== undefined) {
            if (this.kartBody_ && (this.kartBody_ as Model).PrimaryPart) {
                const primaryPart = (this.kartBody_ as Model).PrimaryPart!;
                (this.flipEffect_ as unknown as { CFrame: CFrame }).CFrame = primaryPart.CFrame;
            }
        }
        
        // 恶魔开始特效处理 - 适配Roblox
        if (this.devilStartEffect_ !== undefined && this.devilStartEffect_.Parent !== undefined) {
            if (this.kartBody_ && (this.kartBody_ as Model).PrimaryPart) {
                const primaryPart = (this.kartBody_ as Model).PrimaryPart!;
                const localPosition = primaryPart.Position;
                const vector = new Vector3(localPosition.X, localPosition.Y + 3, localPosition.Z);
                (this.devilStartEffect_ as any).Position = vector;
            }
        }
        
        // 恶魔游戏特效处理 - 适配Roblox
        if (this.devilPlayEffect_ !== undefined && this.devilPlayEffect_.Parent !== undefined) {
            if (this.kartBody_ && (this.kartBody_ as Model).PrimaryPart) {
                const primaryPart = (this.kartBody_ as Model).PrimaryPart!;
                const localPosition2 = primaryPart.Position;
                const vector2 = new Vector3(localPosition2.X, localPosition2.Y + 3, localPosition2.Z);
                (this.devilPlayEffect_ as any).Position = vector2;
            }
        }
    }
    
    public Initialize(kartBodyIdx: number, characterIdx: number, isNoAniCharacter: boolean): void {
        const gameObject = this.kartBody;
        const mainAsset = "";
        this.isNoAnimationCharacter_ = isNoAniCharacter;
        
        // 确保gameObject存在且是Instance类型再赋值
        if (this.gameObject && this.gameObject.GetRobloxObject) {
            // 从GameObject中获取底层的Roblox Instance
            this.kartBody_ = this.gameObject.GetRobloxObject() as Instance;
        } else if (this.kartBody && typeOf(this.kartBody) === "Instance") {
            // 如果gameObject不存在，使用传入的kartBody作为fallback，但需要确保它是Instance
            this.kartBody_ = this.kartBody as Instance;
        } else {
            // 如果都不存在或不是Instance，则保持undefined
            this.kartBody_ = undefined;
        }
        
        this.booster_ = [];
        for (let j = 0; j < 2; j++) {
            this.booster_[j] = undefined;
        }
        
        this.ObjectSetting();
    }
    
    private ObjectSetting(): void {
        if (!this.kartBody_) {
            return;
        }
        
        // 确保kartBody_确实是一个Roblox Instance并且有FindFirstChild方法
        if (typeOf(this.kartBody_) !== "Instance" || !("FindFirstChild" in this.kartBody_)) {
            warn("警告: kartBody_不是有效的Roblox Instance，跳过ObjectSetting");
            return;
        }
        
        // 引入RobloxUnityAdapter来创建Transform对象
        // 注意：这里需要根据实际的RobloxUnityAdapter实现进行调整
        
        // 简化的轮子查找 - 立即尝试，不阻塞初始化
        const findWheels = (): number => {
            if (!this.kartBody_ || typeOf(this.kartBody_) !== "Instance") {
                print("findWheels: kartBody_为nil或不是Instance");
                return 0;
            }
            
            const wheelsModel = this.kartBody_.FindFirstChild("Wheels") as Model;
            if (!wheelsModel) {
                print("findWheels: 未找到Wheels模型");
                // 列出kartBody_的所有子对象
                print("kartBody_的子对象:");
                for (const child of this.kartBody_.GetChildren()) {
                    print(`  - ${child.Name} (${child.ClassName})`);
                }
                return 0;
            }
            
        		print("findWheels: 找到Wheels模型，开始查找轮子");
		
		// 列出Wheels模型的所有子对象
		print("Wheels模型的子对象:");
		const children = wheelsModel.GetChildren();
		for (let i = 0; i < children.size(); i++) {
			const child = children[i];
			if (child) {
				print(`  - ${child.Name} (${child.ClassName})`);
			}
		}
            let foundCount = 0;
            // 按名称查找轮子 - 按照Lua版本的逻辑
            for (let i = 1; i <= 4; i++) {
                if (!this.wheels_[i]) {
                    // Lua版本使用：tire0, tire1, tire2, tire3 (i-1)
                    const wheelName = `tire${i - 1}`;
                    const wheel = wheelsModel.FindFirstChild(wheelName) as Instance;
                    if (wheel) {
                        print(`findWheels: 找到轮子 ${wheelName}`);
                        // 按照Lua版本：RobloxUnityAdapter.GameObject.new(wheel).transform
                        const gameObject = RobloxUnityAdapter.GameObject.create(wheel);
                        const transform = gameObject.transform;
                        // 确保localPosition属性被正确更新
                        transform.localPosition = transform.getLocalPosition();
                        this.wheels_[i] = transform;
                        foundCount = foundCount + 1;
                    } else {
                        print(`findWheels: 未找到轮子 ${wheelName}`);
                        // 列出Wheels模型的所有子对象
                        if (i === 1) {
                            print("Wheels模型的子对象:");
                            for (const child of wheelsModel.GetChildren()) {
                                print(`  - ${child.Name} (${child.ClassName})`);
                            }
                        }
                    }
                } else {
                    foundCount = foundCount + 1;
                }
            }
            return foundCount;
        };
        
        // 立即尝试查找轮子
        const foundWheels = findWheels();
        if (foundWheels < 4) {
            warn(`警告: 只找到 ${foundWheels}/4 个轮子`);
        }
        
        // 查找其他组件
        if (this.kartBody_ && typeOf(this.kartBody_) === "Instance") {
            // 查找特效组件
            this.flipEffect_ = this.kartBody_.FindFirstChild("FlipEffect") as Instance;
            this.devilStartEffect_ = this.kartBody_.FindFirstChild("DevilStartEffect") as Instance;
            this.devilPlayEffect_ = this.kartBody_.FindFirstChild("DevilPlayEffect") as Instance;
            
            // 查找防护组件
            this.shield_ = this.kartBody_.FindFirstChild("Shield") as Instance;
            this.guard_ = this.kartBody_.FindFirstChild("Guard") as Instance;
            
            // 查找角色相关组件
            this.character_ = this.kartBody_.FindFirstChild("Character") as Instance;
            this.characterFace_ = this.kartBody_.FindFirstChild("CharacterFace") as Instance;
            
            // 查找其他特效
            this.waterBombBubble_ = this.kartBody_.FindFirstChild("WaterBombBubble") as Instance;
            this.waterFlyBubble_ = this.kartBody_.FindFirstChild("WaterFlyBubble") as Instance;
            this.boosterWave_ = this.kartBody_.FindFirstChild("BoosterWave") as Instance;
            
            // 查找渲染器
            this.flipRenderer_ = this.kartBody_.FindFirstChild("FlipRenderer") as Instance;
        }
    }
    
    // 方法用于子类访问受保护的成员
    protected SetEnableBooster(enabled: boolean): void {
        // 启用/禁用推进器的实现
        // 这里需要根据实际需求实现
    }
}

export default KartBasicController;
