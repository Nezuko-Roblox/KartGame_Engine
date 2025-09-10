/**
 * GoKart class的TypeScript实现
 * 与Lua版本完全一致
 */

import { UnityVector3 } from "../KartShared/UnityMath";
import { BoostKind } from "./BoostKind";
import { GameObject } from "../KartShared/RobloxUnityAdapter";

export class GoKart {
    // 私有属性
    private stuck_: boolean = false;
    private valid_: boolean = true;
    private forcing_: boolean = false;
    private isInResetState_: boolean = false;
    private status_: number = 0;
    
    // 公共成员
    public m_kart: unknown = undefined; // Roblox对象
    public controller_: unknown = undefined;
    public m_KartWLVel: UnityVector3 = UnityVector3.zero;
    public m_KartLAVel: UnityVector3 = UnityVector3.zero;
    public m_KartRealVelocity: UnityVector3 = UnityVector3.zero;

    constructor() {
        // 属性初始化在声明时完成
    }

    // 属性访问器
    public GetStuck(): boolean {
        return this.stuck_;
    }

    public SetStuck(value: boolean): void {
        this.stuck_ = value;
    }

    public GetValid(): boolean {
        return this.valid_;
    }

    public SetValid(value: boolean): void {
        if (this.valid_ !== value) {
            this.valid_ = value;
        }
    }

    public GetForcing(): boolean {
        return this.forcing_;
    }

    public SetForcing(value: boolean): void {
        this.forcing_ = value;
    }

    public GetIsInResetState(): boolean {
        return this.isInResetState_;
    }

    public SetIsInResetState(value: boolean): void {
        this.isInResetState_ = value;
    }

    // 虚函数 - 子类可以重写
    public basicAction(tick: number): void {
        // 基类的空实现
    }

    public setReKart(controller: unknown, wheels: unknown): void {
        this.controller_ = controller;
        // 严格按照Lua代码：if controller and controller.gameObject then
        if (controller && typeOf(controller) === "table") {
            const ctrl = controller as Record<string, unknown>;
            if (ctrl.gameObject) {
                this.m_kart = ctrl.gameObject as GameObject;
            } else {
                warn("GoKart: 无法获取Roblox对象");
            }
        } else {
            warn("GoKart: 无法获取Roblox对象");
        }
    }

    public Warp(pos: any, rot: unknown, flush?: boolean, resetVel?: boolean): void {
        if (this.m_kart && (this.m_kart as Record<string, unknown>).transform) {
            const kart = this.m_kart as Record<string, Record<string, unknown>>;
            const transform = kart.transform as unknown as {
                setLocalPosition: (pos: Vector3) => void;
                setLocalRotation: (rot: unknown) => void;
            };
            if (transform) {
                transform.setLocalPosition(pos);
                transform.setLocalRotation(rot);
            }
        }
        
        if (flush) {
            // 刷新逻辑
        }
        
        if (resetVel) {
            this.m_KartWLVel = UnityVector3.zero;
            this.m_KartLAVel = UnityVector3.zero;
        }
    }

    public isRealBoost(): boolean {
        return false;
    }

    public isRealBoostWithKind(kind: BoostKind): boolean {
        return false;
    }

    public isItemBoost(): boolean {
        return false;
    }

    public isItemBoostWithKind(kind: BoostKind): boolean {
        return false;
    }

    public isZoneBoost(): boolean {
        return false;
    }

    public isZoneBoostWithKind(kind: BoostKind): boolean {
        return false;
    }

    public isBoost(kind: BoostKind): boolean {
        return false;
    }

    public ResetForRestarting(): void {
        this.m_KartWLVel = new UnityVector3(0, 0, 0);
        this.m_KartLAVel = new UnityVector3(0, 0, 0);
        this.m_KartRealVelocity = new UnityVector3(0, 0, 0);
        this.stuck_ = false;
        this.valid_ = true;
        this.forcing_ = false;
        this.status_ = 0;
    }
}

export default GoKart;