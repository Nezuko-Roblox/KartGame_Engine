/**
 * DriftGauge struct的TypeScript实现
 * 与Lua版本完全一致，用于处理漂移计量表状态
 */

export class DriftGauge {
    public gauge: number = 0.0;
    public progressOn: boolean = false;
    public progress: number = 0.0;
    public lastProgress: number = 0.0;
    public progressTime: number = 0.0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.gauge = 0.0;
        this.progressOn = false;
        this.progress = 0.0;
        this.lastProgress = 0.0;
        this.progressTime = 0.0;
    }

    // 静态工厂方法
    static create(): DriftGauge {
        return new DriftGauge();
    }
}

export default DriftGauge;