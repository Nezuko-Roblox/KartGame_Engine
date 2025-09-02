import type { World } from "@rbxts/matter";
import { KartReference, ItemHolder } from "shared/ecs/components/items";
import { RunService } from "@rbxts/services";

/**
 * 卡丁车桥接扫描系统 - 定期检查服务端ECS中的卡丁车数量
 * 注意：卡丁车在客户端创建，通过远程事件同步到服务端ECS
 */
function kartBridgeScannerSystem(world: World): void {
    // 只在服务端运行
    if (!RunService.IsServer()) return;

    const currentTime = tick();

    // 每5秒扫描一次
    if (currentTime % 5 < 0.1) {
        // 检查当前ECS中的卡丁车数量
        const ecsKartEntities = world.query(KartReference, ItemHolder);
        let ecsKartCount = 0;
        for (const [,] of ecsKartEntities) {
            ecsKartCount++;
        }

        // 服务端不需要检查 KartManager，因为卡丁车是在客户端创建的
        // ECS 实体通过远程事件同步到服务端
    }
}

export = {
    system: kartBridgeScannerSystem,
    priority: 5, // 高优先级，在其他系统之前运行
};