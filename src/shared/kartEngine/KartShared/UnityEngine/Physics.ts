/**
 * UnityEngine.Physics类的TypeScript实现
 * 模拟Unity的Physics静态类功能
 */

import { Workspace, CollectionService } from "@rbxts/services";
import { LayerMask } from "./LayerMask";
import { RaycastHit } from "./RaycastHit";

export class Physics {
    // 模拟Unity的Physics.RaycastAll方法
    static RaycastAll(origin: Vector3, direction: Vector3, maxDistance?: number, layerMask?: number): RaycastHit[] {
        maxDistance = maxDistance || math.huge;
        layerMask = layerMask || 0xFFFFFFFF;

        const raycastParams = new RaycastParams();
        raycastParams.FilterType = Enum.RaycastFilterType.Blacklist;

        // 排除Player标签的物体（玩家赛车）
        const filterList: Instance[] = [];
        const playerObjects = CollectionService.GetTagged("Player");
        for (const obj of playerObjects) {
            filterList.push(obj);
        }
        raycastParams.FilterDescendantsInstances = filterList;

        // 转换为Roblox Vector3
        const robloxOrigin = new Vector3(origin.X, origin.Y, origin.Z);
        // 注意：direction可能已经是完整的向量，需要根据其长度判断
        const dirLength = math.sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);
        let robloxDirection: Vector3;
        if (dirLength > 1.5) {
            // direction已经是完整的向量（从GetCollisionCheckedPosition传入的是toPosition - fromPosition）
            robloxDirection = new Vector3(direction.X, direction.Y, direction.Z);
        } else {
            // direction是单位向量，需要乘以maxDistance
            robloxDirection = new Vector3(direction.X, direction.Y, direction.Z).mul(maxDistance);
        }

        const results: RaycastHit[] = [];
        const hitObjects: Instance[] = [];  // 已经击中的物体

        // 执行多次射线检测以模拟RaycastAll
        for (let i = 0; i < 50; i++) {
            // 合并过滤列表：Player标签物体 + 已击中的物体
            const currentFilterList: Instance[] = [];
            for (const obj of filterList) {
                currentFilterList.push(obj);
            }
            for (const obj of hitObjects) {
                currentFilterList.push(obj);
            }
            raycastParams.FilterDescendantsInstances = currentFilterList;

            const raycastResult = Workspace.Raycast(robloxOrigin, robloxDirection, raycastParams);

            if (raycastResult) {
                const hit = new RaycastHit(raycastResult);

                // 检查layer mask
                if (LayerMask.CheckLayerMask(hit.layer, layerMask)) {
                    results.push(hit);
                }

                hitObjects.push(raycastResult.Instance);
            } else {
                break;
            }
        }

        return results;
    }

    // 重载版本：接受Ray参数
    static RaycastAllFromRay(ray: { origin: Vector3; direction: Vector3 }, maxDistance?: number, layerMask?: number): RaycastHit[] {
        return this.RaycastAll(ray.origin, ray.direction, maxDistance, layerMask);
    }

    // 模拟Unity的Physics.Raycast方法（单次射线检测）
    static Raycast(origin: Vector3, direction: Vector3, maxDistance?: number, layerMask?: number): [boolean, RaycastHit?] {
        const results = this.RaycastAll(origin, direction, maxDistance, layerMask);
        return results.size() > 0 ? [true, results[0]] : [false, undefined];
    }
}

export default Physics;