import type { World } from "@rbxts/matter";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { ItemHolder, KartReference } from "shared/ecs/components/items";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";

/**
 * 基于事件的道具给予系统
 * 监听事件总线，处理道具给予请求
 */
function eventBasedItemGiverSystem(world: World): void {
	// 消费所有待处理的事件
	const events = ItemEventBus.consume();
	
	for (const event of events) {
		// 只处理拾取道具事件
		if (event.type !== "PICKUP") continue;
		
		// 获取事件数据
		const data = event.data as { itemType: string } | undefined;
		if (!data?.itemType) {
			warn(`[EventItemGiver] PICKUP事件缺少itemType数据`);
			continue;
		}
		
		// 获取对应的ECS实体
		const bridge = KartECSBridge.getInstance();
		const entity = bridge.getEntityByKartIndex(event.kartIndex);
		
		if (!entity || !world.contains(entity)) {
			warn(`[EventItemGiver] 找不到卡丁车实体，索引: ${event.kartIndex}`);
			continue;
		}
		
		// 获取组件
		const holder = world.get(entity, ItemHolder);
		const kartRef = world.get(entity, KartReference);
		
		if (!holder || !kartRef) {
			warn(`[EventItemGiver] 卡丁车实体缺少必要组件`);
			continue;
		}
		
		// 尝试将道具推入栈
		const newStack = holder.itemStack.clone();
		const pushSuccess = newStack.push(data.itemType);
		
		if (pushSuccess) {
			// 成功推入道具
			world.insert(entity, holder.patch({
				itemStack: newStack,
			}));
			
			print(`[EventItemGiver] 玩家 ${event.kartIndex} 获得道具: ${data.itemType}`);
			
			// 显示当前栈状态
			const items = newStack.toArray();
			const display: string[] = [];
			for (let i = 0; i < holder.maxSlots; i++) {
				const item = items[i];
				display.push(item !== undefined ? item : "空");
			}
			print(`[EventItemGiver] 道具栈状态: [${display.join(", ")}]`);
		} else {
			// 栈满，需要替换最旧的道具
			print(`[EventItemGiver] 道具栈满，替换最旧道具`);
			
			// 创建新栈
			const newStack = holder.itemStack.clone();
			// 移除栈底（最旧的道具）
			const items = holder.itemStack.toArray();
			newStack.clear();
			
			// 从第二个道具开始重新入栈
			for (let i = 1; i < items.size(); i++) {
				const item = items[i];
				if (item !== undefined) {
					newStack.push(item);
				}
			}
			// 推入新道具
			newStack.push(data.itemType);
			
			world.insert(entity, holder.patch({
				itemStack: newStack,
			}));
			
			print(`[EventItemGiver] 玩家 ${event.kartIndex} 获得道具（替换）: ${data.itemType}`);
		}
	}
}

export = {
	priority: 90, // 在test-item-giver(80)之后执行，处理事件
	system: eventBasedItemGiverSystem,
};