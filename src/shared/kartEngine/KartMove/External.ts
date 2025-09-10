/**
 * External struct的TypeScript实现
 * 与Lua版本完全一致，用于处理外力和环境因素
 */

import { UnityVector3 } from "../KartShared/UnityMath";

export class External {
    public slip: boolean = false;
    public dragFactor: number = 1.0;
    public compensationDragFactor: number = 1.0;
    public wheelFactor: number = 1.0;
    public annexForce: UnityVector3 = new UnityVector3(0, 0, 0);
    public force: UnityVector3 = new UnityVector3(0, 0, 0);
    public torque: UnityVector3 = new UnityVector3(0, 0, 0);
    public upDownTime: number = 0.0;
    public upDownLastTime: number = 0.0;
    public gravityFactor: number = 1.0;
    public speedLimit: number = 0.0;
    
    // 这些字段在Initialize中没有设置，但在struct中存在
    public upDownInterval: number = 0.0;
    public upDownForce: UnityVector3 = new UnityVector3(0, 0, 0);
    public upDownForceIndex: number = 0;
    public liftVel: UnityVector3 = new UnityVector3(0, 0, 0);

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.slip = false;
        this.dragFactor = 1.0;
        this.compensationDragFactor = 1.0;
        this.wheelFactor = 1.0;
        this.annexForce = new UnityVector3(0, 0, 0);
        this.force = new UnityVector3(0, 0, 0);
        this.torque = new UnityVector3(0, 0, 0);
        this.upDownTime = 0.0;
        this.upDownLastTime = 0.0;
        this.gravityFactor = 1.0;
        this.speedLimit = 0.0;
        this.upDownInterval = 0.0;
        this.upDownForce = new UnityVector3(0, 0, 0);
        this.upDownForceIndex = 0;
        this.liftVel = new UnityVector3(0, 0, 0);
    }

    // 静态工厂方法
    static create(): External {
        return new External();
    }
}

export default External;