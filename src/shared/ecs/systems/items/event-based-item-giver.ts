import type { World, AnyEntity } from "@rbxts/matter";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { ItemHolder, KartReference } from "shared/ecs/components/items";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { RunService } from "@rbxts/services";

/**
 * 基于事件的道具给予系统
 * 监听事件总线，处理道具给予请求
 * 只在服务端运行，确保道具状态统一管理
 */
function eventBasedItemGiverSystem(world: World): void {
	// 只在服务端运行
	if (!RunService.IsServer()) return;
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
		
		// 获取对应的ECS实体（服务端）
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

		// 尝试将道具推入栈（数组末尾为栈顶）
		const newItems = [...holder.items];
		
		if (newItems.size() < holder.maxSlots) {
			// 栈未满，直接推入
			newItems.push(data.itemType);
			
			world.insert(entity, holder.patch({
				items: newItems,
			}));
		} else {
			// 栈满，需要替换最旧的道具（移除数组开头，添加到末尾）
			// 移除最旧的道具（数组开头）并添加新道具到栈顶（数组末尾）
			newItems.shift(); // 移除数组第一个元素（栈底/最旧道具）
			newItems.push(data.itemType); // 添加到数组末尾（栈顶）

			world.insert(entity, holder.patch({
				items: newItems,
			}));
		}
	}
}

export = {
	priority: 90, // 在test-item-giver(80)之后执行，处理事件
	system: eventBasedItemGiverSystem,
};