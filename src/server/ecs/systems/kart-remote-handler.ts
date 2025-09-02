import type { World } from "@rbxts/matter";
import { RunService } from "@rbxts/services";
import { remotes } from "shared/remotes";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";

/**
 * 卡丁车远程事件处理系统 - 处理客户端的卡丁车创建通知
 */
function kartRemoteHandlerSystem(world: World): void {
    // 只在服务端运行
    if (!RunService.IsServer()) return;

    // 这个系统主要是确保远程事件监听器已设置
    // 实际的处理逻辑在 KartECSBridge 中
    const bridge = KartECSBridge.getInstance();
    
    // 确保桥接器已初始化
    if (!bridge.getWorld()) {
        bridge.initialize(world);
    }
}

export = {
    system: kartRemoteHandlerSystem,
    priority: 10, // 最高优先级，确保在其他系统之前运行
};