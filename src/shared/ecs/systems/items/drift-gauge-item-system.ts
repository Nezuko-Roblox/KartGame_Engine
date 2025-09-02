import type { World } from "@rbxts/matter";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { KartReference } from "shared/ecs/components/items";
import { RunService } from "@rbxts/services";
import { remotes } from "shared/remotes";

// 记录上次检查的集气值，防止重复触发
const lastGaugeValues = new Map<number, number>();

/**
 * 漂移集气道具系统 - 集气满后通过事件总线给玩家加速道具
 * 与 event-based-item-giver.ts 配合工作
 */
function driftGaugeItemSystem(world: World): void {
	// 只在客户端运行，因为集气数据在客户端
	if (!RunService.IsClient()) return;
	// 查找所有卡丁车实体
	for (const [entity, kartRef] of world.query(KartReference)) {
		// 获取 GoKart 实例（可以是玩家或AI）
		const goKart = kartRef.kartInstance as unknown as {
			m_driftGauge?: {
				gauge: number;
			};
			m_spec?: {
				driftMaxGauge: number;
			};
			ResetDriftGauge?: (self: unknown) => void;
		};
		
		if (!goKart?.m_driftGauge || !goKart?.m_spec) continue;
		
		const currentGauge = goKart.m_driftGauge.gauge;
		const maxGauge = goKart.m_spec.driftMaxGauge;
		const lastGauge = lastGaugeValues.get(kartRef.kartIndex) || 0;
		
		// 检查是否刚达到满集气（避免重复触发）
		if (currentGauge >= maxGauge && lastGauge < maxGauge) {
			print(`[DriftGauge] 卡丁车 ${kartRef.kartIndex} 集气满！`);
			
			// 通知服务端给予道具
			remotes.items.notifyItemPickup.fire(kartRef.kartIndex, "Booster");
			
			// 重置集气条
			if (goKart.ResetDriftGauge) {
				goKart.ResetDriftGauge(goKart);
			}
			goKart.m_driftGauge.gauge = 0;
			
			print(`[DriftGauge] 已发送PICKUP事件，道具类型: Booster`);
		}
		
		// 更新记录的集气值
		lastGaugeValues.set(kartRef.kartIndex, currentGauge);
	}
}

export = {
	priority: 80, // 较低的数字 = 较高优先级，先检测集气并发送事件
	system: driftGaugeItemSystem,
};