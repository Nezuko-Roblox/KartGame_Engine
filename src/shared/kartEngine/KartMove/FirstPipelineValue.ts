/**
 * FirstPipelineValue struct的TypeScript实现
 * 与Lua版本完全一致，用于处理第一管道值
 */

import { UnityVector3 } from "../KartShared/UnityMath";

export class FirstPipelineValue {
    public left: UnityVector3 = new UnityVector3(0, 0, 0);
    public up: UnityVector3 = new UnityVector3(0, 0, 0);
    public front: UnityVector3 = new UnityVector3(0, 0, 0);
    public frontVel: number = 0.0;
    public leftVel: number = 0.0;
    public upVel: number = 0.0;
    public speed: number = 0.0;

    constructor() {
        this.Initialize();
    }

    public Initialize(): void {
        this.left = new UnityVector3(0, 0, 0);
        this.up = new UnityVector3(0, 0, 0);
        this.front = new UnityVector3(0, 0, 0);
        this.frontVel = 0.0;
        this.leftVel = 0.0;
        this.upVel = 0.0;
        this.speed = 0.0;
    }

    // 静态工厂方法
    static create(): FirstPipelineValue {
        return new FirstPipelineValue();
    }
}

export default FirstPipelineValue;