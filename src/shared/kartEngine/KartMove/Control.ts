/**
 * Control struct的TypeScript实现
 * 与Lua版本完全一致，用于处理卡丁车控制输入
 */

export class Control {
    public accel: number = 0.0;
    public brake: number = 0.0;
    public accelBrakeSwap: boolean = false;
    public steer: number = 0.0;
    public wheelFlip: boolean = false;
    public wheelDevil: boolean = false;
    public stayTime: number = 0.0;
    public steerAngle: number = 0.0;
    public oldSteerAngle: number = 0.0;

    constructor() {
        this.Initialize();
    }

    public getRealAccel(): number {
        if (!this.accelBrakeSwap) {
            return this.accel;
        } else {
            return this.brake;
        }
    }

    public getRealBrake(): number {
        if (!this.accelBrakeSwap) {
            return this.brake;
        } else {
            return this.accel;
        }
    }

    public getRealSteer(): number {
        let multiplier = 1.0;
        if (this.wheelFlip || this.wheelDevil) {
            multiplier = -1.0;
        }
        return multiplier * this.steer;
    }

    public Initialize(): void {
        this.accel = 0.0;
        this.brake = 0.0;
        this.accelBrakeSwap = false;
        this.steer = 0.0;
        this.wheelFlip = false;
        this.wheelDevil = false;
        this.stayTime = 0.0;
        this.steerAngle = 0.0;
        this.oldSteerAngle = 0.0;
    }

    // 静态工厂方法
    static create(): Control {
        return new Control();
    }
}

export default Control;