/**
 * DriveFactor struct的TypeScript实现
 * 与Lua版本完全一致，用于处理驾驶因素
 */

export class DriveFactor {
    public frontGripFactor: number = 0.0;
    public rearGripFactor: number = 0.0;
    public driftSlipFactor: number = 1.0;
    public backFrontGripFactor: number = 0.0;
    public backRearGripFactor: number = 0.0;
    public backDriftSlipFactor: number = 1.0;
    public betaCut: number = 1.0;
    public onDriftSteerFactor: number = 0.7;  // 降低漂移中的转向速度
    public onRestTimeSteerFactor: number = 0.8;  // 降低漂移恢复时的转向速度
    public onTriggerSteerFactor: number = 1.0;
    public speedLimit: number = 120.0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.frontGripFactor = 0.0;
        this.rearGripFactor = 0.0;
        this.driftSlipFactor = 1.0;
        this.backFrontGripFactor = 0.0;
        this.backRearGripFactor = 0.0;
        this.backDriftSlipFactor = 1.0;
        this.betaCut = 1.0;
        this.onDriftSteerFactor = 0.7;
        this.onRestTimeSteerFactor = 0.8;
        this.onTriggerSteerFactor = 1.0;
        this.speedLimit = 120.0;
    }

    // 静态工厂方法
    static create(): DriveFactor {
        return new DriveFactor();
    }
}

export default DriveFactor;