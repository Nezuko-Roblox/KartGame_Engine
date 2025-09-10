// UnityEngine.RaycastHit结构体的Roblox实现
// 模拟Unity的RaycastHit结构

import { UnityVector3 } from "../UnityMath";
import { LayerMask } from "./LayerMask";

export interface Collider {
    gameObject: {
        layer: number;
        name: string;
        transform: Instance;
    };
}

export class RaycastHit {
    public distance: number;
    public point: UnityVector3; // Vector3 type
    public normal: UnityVector3; // Vector3 type
    public collider: Collider | undefined;
    public transform: Instance | undefined;
    public rigidbody: Instance | undefined;
    public layer: number;

    constructor(robloxRaycastResult?: RaycastResult) {
        if (robloxRaycastResult) {
            // 基本属性
            this.distance = robloxRaycastResult.Distance;
            this.point = new UnityVector3(
                robloxRaycastResult.Position.X,
                robloxRaycastResult.Position.Y,
                robloxRaycastResult.Position.Z
            );
            this.normal = new UnityVector3(
                robloxRaycastResult.Normal.X,
                robloxRaycastResult.Normal.Y,
                robloxRaycastResult.Normal.Z
            );

            // GameObject相关属性
            const instance = robloxRaycastResult.Instance;
            const layer = LayerMask.GetLayer(instance);

            // 模拟Unity的collider结构
            this.collider = {
                gameObject: {
                    layer: layer,
                    name: instance.Name,
                    transform: instance
                }
            };

            // 兼容性属性
            this.transform = instance;
            this.rigidbody = instance;
            this.layer = layer; // 方便直接访问layer
        } else {
            // 空的RaycastHit
            this.distance = 0;
            this.point = UnityVector3.zero;
            this.normal = UnityVector3.zero;
            this.collider = undefined;
            this.transform = undefined;
            this.rigidbody = undefined;
            this.layer = 0;
        }
    }

    public static create(robloxRaycastResult?: RaycastResult): RaycastHit {
        return new RaycastHit(robloxRaycastResult);
    }
}

export default RaycastHit;