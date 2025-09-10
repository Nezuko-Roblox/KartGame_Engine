/**
 * PhysicSpec struct的TypeScript实现
 * 与Lua版本完全一致，用于存储物理规格参数
 */

import { UnityVector3 } from "../KartShared/UnityMath";

export class PhysicSpec {
    public wheelTranslate: UnityVector3[] = [];
    public wheelWidth: number[] = [];
    public itemSlotCapacity: number = 2;
    public useTransformBooster: boolean = false;
    public mass: number = 100.0;
    public airFriction: number = 3.0;
    public dragFactor: number = 0.74;
    public forwardAccel: number = 4000.0;
    public backwardAccel: number = 1500.0;
    public gripBrake: number = 1800.0;
    public slipBrake: number = 1200.0;
    public maxSteerDeg: number = 4.0;
    public steerConstraint: number = 30.0;
    public frontGripFactor: number = 5.0;
    public rearGripFactor: number = 5.0;
    public driftTrigFactor: number = 0.2;
    public driftTrigTime: number = 0.2;
    public driftSlipFactor: number = 0.3;
    public driftEscapeForce: number = 2500.0;
    public cornerDrawFactor: number = 0.2;
    public driftLeanFactor: number = 0.07;
    public steerLeanFactor: number = 0.01;
    public driftMaxGauge: number = 4000.0;
    public normalBoosterTime: number = 3000.0;
    public teamBoosterTime: number = 4500.0;
    public animalBoosterTime: number = 4000.0;
    public driftDragReduceFactor: number = 0.4;
    
    // 动态添加的属性 (在GoPlayKart中设置)
    public width: number = 0;
    public length: number = 0;
    public springK: number = 0;
    public damperCopC: number = 0;
    public damperRebC: number = 0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.wheelTranslate = [
            new UnityVector3(0, 0, 0),
            new UnityVector3(0, 0, 0),
            new UnityVector3(0, 0, 0),
            new UnityVector3(0, 0, 0)
        ];
        this.wheelWidth = [0, 0, 0, 0];
        
        this.itemSlotCapacity = 2;
        this.useTransformBooster = false;
        this.mass = 100.0;
        this.airFriction = 3.0;
        this.dragFactor = 0.74;
        this.forwardAccel = 4000.0;
        this.backwardAccel = 1500.0;
        this.gripBrake = 1800.0;
        this.slipBrake = 1200.0;
        this.maxSteerDeg = 4.0;
        this.steerConstraint = 30.0;
        this.frontGripFactor = 5.0;
        this.rearGripFactor = 5.0;
        this.driftTrigFactor = 0.2;
        this.driftTrigTime = 0.2;
        this.driftSlipFactor = 0.3;
        this.driftEscapeForce = 2500.0;
        this.cornerDrawFactor = 0.2;
        this.driftLeanFactor = 0.07;
        this.steerLeanFactor = 0.01;
        this.driftMaxGauge = 4000.0;
        this.normalBoosterTime = 3000.0;
        this.teamBoosterTime = 4500.0;
        this.animalBoosterTime = 4000.0;
        this.driftDragReduceFactor = 0.4;
    }

    public getMaxSteerRad(): number {
        return math.pi * this.maxSteerDeg / 180.0;
    }

    // 静态工厂方法
    static create(): PhysicSpec {
        return new PhysicSpec();
    }
}

export default PhysicSpec;