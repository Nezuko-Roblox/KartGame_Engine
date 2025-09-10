// UnityEngine.Ray结构体的Roblox实现
// 模拟Unity的Ray结构

import { UnityVector3 } from "../UnityMath";

export class Ray {
    public origin: UnityVector3; // Vector3 type
    public direction: UnityVector3; // Vector3 type

    constructor(origin: UnityVector3, direction: UnityVector3) {
        this.origin = origin;
        // Unity的Ray构造函数会对direction进行归一化处理
        // 对应Unity源码：m_Direction = direction.normalized
        this.direction = direction.getNormalized();
    }

    // 获取射线上指定距离的点
    public GetPoint(distance: number): UnityVector3 {
        return this.origin.add(this.direction.mul(distance));
    }
}
