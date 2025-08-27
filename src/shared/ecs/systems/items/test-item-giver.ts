import type { World } from "@rbxts/matter";
import { ItemHolder, KartReference } from "shared/ecs/components/items";
import { Stack } from "shared/util/stack";

// 记录上次检查的集气值，防止重复触发
const lastGaugeValues = new Map<number, number>();

/**
 * 漂移集气道具系统 - 集气满后自动给玩家加速道具
 */
function driftGaugeItemSystem(world: World): void {
	// 查找玩家实体
	for (const [entity, kartRef, holder] of world.query(KartReference, ItemHolder)) {
		if (!kartRef.isPlayer) continue;
		
		// 获取 GoPlayKart 实例
		const goPlayKart = kartRef.kartInstance as unknown as {
			m_driftGauge?: {
				gauge: number;
			};
			m_spec?: {
				driftMaxGauge: number;
			};
			ResetDriftGauge?: (self: unknown) => void;
		};
		
		if (!goPlayKart?.m_driftGauge || !goPlayKart?.m_spec) continue;
		
		const currentGauge = goPlayKart.m_driftGauge.gauge;
		const maxGauge = goPlayKart.m_spec.driftMaxGauge;
		const lastGauge = lastGaugeValues.get(kartRef.kartIndex) || 0;
		
		// 检查是否刚达到满集气（避免重复触发）
		if (currentGauge >= maxGauge && lastGauge < maxGauge) {
			// 调试：打印当前道具栈状态
			print(`[DriftGauge] 道具栈大小: ${holder.itemStack.size()}/${holder.maxSlots}`);
			
			// 显示当前道具栈内容
			const stackItems = holder.itemStack.toArray();
			const itemsDisplay: string[] = [];
			for (let i = 0; i < holder.maxSlots; i++) {
				const item = stackItems[i];
				itemsDisplay.push(item !== undefined ? item : "空");
			}
			print(`[DriftGauge] 当前道具栈（底到顶）: [${itemsDisplay.join(", ")}]`);
			
			// 尝试将新道具推入栈
			const newStack = holder.itemStack.clone();
			const pushSuccess = newStack.push("Boost");
			
			if (pushSuccess) {
				// 成功推入道具
				const newHolder = holder.patch({
					itemStack: newStack,
				});
				
				world.insert(entity, newHolder);
				
				// 重置集气条
				if (goPlayKart.ResetDriftGauge) {
					goPlayKart.ResetDriftGauge(goPlayKart);
				}
				goPlayKart.m_driftGauge.gauge = 0;
				
				print(`[DriftGauge] 集气满! 获得加速道具（推入栈顶）`);
				
				// 显示更新后的栈
				const updatedItems = newStack.toArray();
				const updatedDisplay: string[] = [];
				for (let i = 0; i < holder.maxSlots; i++) {
					const item = updatedItems[i];
					updatedDisplay.push(item !== undefined ? item : "空");
				}
				print(`[DriftGauge] 更新后道具栈: [${updatedDisplay.join(", ")}]`);
			} else {
				// 栈满，弹出底部道具后推入新道具
				print("[DriftGauge] 道具栈满，弹出栈底道具");
				
				// 创建新栈，移除最旧的道具（栈底）
				const newStack = new Stack<string>(holder.maxSlots);
				const items = holder.itemStack.toArray();
				
				// 跳过第一个道具（最旧的），从第二个开始重新入栈
				for (let i = 1; i < items.size(); i++) {
					const item = items[i];
					if (item !== undefined) {
						newStack.push(item);
					}
				}
				// 推入新道具
				newStack.push("Boost");
				
				const newHolder = holder.patch({
					itemStack: newStack,
				});
				
				world.insert(entity, newHolder);
				
				// 重置集气条
				if (goPlayKart.ResetDriftGauge) {
					goPlayKart.ResetDriftGauge(goPlayKart);
				}
				goPlayKart.m_driftGauge.gauge = 0;
				
				print(`[DriftGauge] 集气满! 替换最旧道具为加速道具`);
				
				// 显示更新后的栈
				const replacedItems = newStack.toArray();
				const replacedDisplay: string[] = [];
				for (const item of replacedItems) {
					replacedDisplay.push(item !== undefined ? item : "空");
				}
				print(`[DriftGauge] 更新后道具栈: [${replacedDisplay.join(", ")}]`);
			}
		}
		
		// 更新记录的集气值
		lastGaugeValues.set(kartRef.kartIndex, currentGauge);
	}
}

export = driftGaugeItemSystem;