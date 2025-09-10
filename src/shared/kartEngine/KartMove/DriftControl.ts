/**
 * DriftControl struct的TypeScript实现
 * 与Lua版本完全一致，用于处理漂移控制状态
 */

export class DriftControl {
    public slipMode: boolean = false;
    public slipTime: number = 0.0;
    public forceSlip: boolean = false;
    public trigger: boolean = false;
    public triggerTime: number = 0.0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.slipMode = false;
        this.slipTime = 0.0;
        this.forceSlip = false;
        this.trigger = false;
        this.triggerTime = 0.0;
    }

    // 静态工厂方法
    static create(): DriftControl {
        return new DriftControl();
    }
}

export default DriftControl;