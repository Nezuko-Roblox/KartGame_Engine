/**
 * KartManager class的TypeScript实现
 * 与Lua版本完全一致
 */

import { Players } from "@rbxts/services";
import { GoKart } from "./GoKart";
import { GoKartBuilder } from "./GoKartBuilder";

export class KartManager {
    // 常量 - 严格按照Lua代码
    public static readonly KART_SCALE_FACTOR = 0.15;
    public static readonly MAX_KART = 6;
    public static readonly KART_GRAVITY = -49.0;
    public static readonly PLAYER_KART_IDX = 1;  // 保留为默认值，但客户端将使用动态值
    public static readonly FIXED_UPDATE_COUNTER = 0;

    // 获取本地玩家的唯一卡丁车索引 - 严格按照Lua代码
    public static GetPlayerKartIndex(): number {
        const Players = game.GetService("Players");
        const player = Players.LocalPlayer;
        if (player) {
            // 使用玩家UserId作为唯一索引，确保每个客户端都有不同的索引
            return player.UserId;
        } else {
            // 服务端或无玩家时使用默认值
            return KartManager.PLAYER_KART_IDX;
        }
    }

    // 单例模式 - 严格按照Lua代码
    private static instance_: KartManager | undefined = undefined;

    // 实例属性 - 严格按照Lua代码，但使用正确的类型
    private goKart_: (GoKart | undefined)[] = [];
    private goPlayKart_: GoKart | undefined = undefined;
    private driveStartTime_: number = 0.0;
    private driveEndTime_: number = 0.0;
    private goKartCount_: number = 0;
    private isPaused_: boolean = false;

    // 构造函数 - 严格按照Lua代码逻辑
    constructor() {
        this.goKart_ = [];
        // 严格按照Lua代码：for i = 1, 6 do self.goKart_[i] = nil end
        for (let i = 1; i <= 6; i++) {
            this.goKart_[i] = undefined;
        }
        
        this.goPlayKart_ = undefined;
        this.driveStartTime_ = 0.0;
        this.driveEndTime_ = 0.0;
        this.goKartCount_ = 0;
        this.isPaused_ = false;
    }

    // 单例获取 - 严格按照Lua代码
    public static GetInstance(): KartManager {
        if (KartManager.instance_ === undefined) {
            KartManager.instance_ = new KartManager();
        }
        return KartManager.instance_;
    }

    // SetKart方法 - 严格按照Lua代码逻辑，使用正确的类型
    public SetKart(idx: number, builder: GoKartBuilder, controller: unknown, wheelPos: unknown): GoKart | undefined {
        // 严格按照Lua代码：if self.goKart_[idx] ~= nil then return nil end
        if (this.goKart_[idx] !== undefined) {
            return undefined;
        }
        
        // 严格按照Lua代码：self.goKart_[idx] = builder:Build()
        this.goKart_[idx] = builder.Build();
        
        // 严格按照Lua代码：self.goKart_[idx]:setReKart(controller, wheelPos)
        // 现在有了正确的类型，roblox-ts应该能生成冒号调用
        this.goKart_[idx]!.setReKart(controller, wheelPos);
        
        // 严格按照Lua代码：使用动态获取的玩家卡丁车索引
        const playerKartIndex = KartManager.GetPlayerKartIndex();
        if (idx === playerKartIndex) {
            this.goPlayKart_ = this.goKart_[idx];
        }
        
        // 严格按照Lua代码：self.goKartCount_ = self.goKartCount_ + 1
        this.goKartCount_ = this.goKartCount_ + 1;
        
        // 严格按照Lua代码：return self.goKart_[idx]
        return this.goKart_[idx];
    }

    // Getter方法 - 在Lua代码中没有这些，但为了TypeScript兼容性保留
    public getGoKart(idx: number): GoKart | undefined {
        return this.goKart_[idx];
    }

    public getGoPlayKart(): GoKart | undefined {
        return this.goPlayKart_;
    }

    public getDriveStartTime(): number {
        return this.driveStartTime_;
    }

    public setDriveStartTime(value: number): void {
        this.driveStartTime_ = value;
    }

    public getDriveEndTime(): number {
        return this.driveEndTime_;
    }

    public setDriveEndTime(value: number): void {
        this.driveEndTime_ = value;
    }

    public getGoKartCount(): number {
        return this.goKartCount_;
    }

    public getIsPaused(): boolean {
        return this.isPaused_;
    }

    public setIsPaused(value: boolean): void {
        this.isPaused_ = value;
    }

    // 静态工厂方法 - 为了兼容性保留
    static create(): KartManager {
        return new KartManager();
    }
}

// 严格按照Lua代码：暴露到全局变量供TypeScript ECS系统使用
(_G as any).KartManager = KartManager;

export default KartManager;
