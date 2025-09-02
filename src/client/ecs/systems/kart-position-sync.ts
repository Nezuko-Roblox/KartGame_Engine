import type { World } from "@rbxts/matter";
import { KartReference } from "shared/ecs/components/items";
import { remotes } from "shared/remotes";
import { RunService } from "@rbxts/services";

let lastSyncTime = 0;
const SYNC_INTERVAL = 0.1; // 每0.1秒同步一次位置

/**
 * 客户端卡丁车位置同步系统
 * 定期将卡丁车位置发送到服务端用于碰撞检测
 */
function kartPositionSyncSystem(world: World): void {
	// 只在客户端运行
	if (!RunService.IsClient()) return;
	
	const currentTime = tick();
	if (currentTime - lastSyncTime < SYNC_INTERVAL) return;
	
	lastSyncTime = currentTime;
	
	// 同步所有卡丁车的位置
	for (const [, kartRef] of world.query(KartReference)) {
		// 获取卡丁车位置
		let kartPosition: Vector3 | undefined;
		
		const kartInstance = kartRef.kartInstance as unknown as {
			m_kart?: {
				gameObject?: BasePart | Model;
			};
		};
		
		if (kartInstance?.m_kart?.gameObject) {
			const kartObject = kartInstance.m_kart.gameObject;
			if (kartObject) {
				if (kartObject.IsA("Model")) {
					const model = kartObject as Model;
					if (model.PrimaryPart) {
						kartPosition = model.PrimaryPart.Position;
					} else {
						const firstPart = model.FindFirstChildOfClass("Part") || model.FindFirstChildOfClass("MeshPart");
						if (firstPart) {
							kartPosition = firstPart.Position;
						}
					}
				} else if (kartObject.IsA("BasePart")) {
					kartPosition = (kartObject as BasePart).Position;
				}
			}
		}
		
		if (kartPosition) {
			// 发送位置到服务端
			remotes.kart.syncKartPosition.fire(kartRef.kartIndex, kartPosition);
		}
	}
}

export = {
	system: kartPositionSyncSystem,
	priority: 15,
};