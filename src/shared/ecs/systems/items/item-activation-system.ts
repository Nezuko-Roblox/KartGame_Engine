import type { World, AnyEntity } from "@rbxts/matter";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { ItemHolder, KartReference } from "shared/ecs/components/items";
import { BoostItem } from "shared/ecs/items/implementations/boost-item";
import { Stack } from "shared/util/stack";

/**
 * 道具激活系统 - 处理道具使用
 */
function itemActivationSystem(world: World): void {
	const bridge = KartECSBridge.getInstance();
	// 确保桥接器使用正确的 world
	bridge.initialize(world);
	
	const events = ItemEventBus.consume();

	// 处理使用道具事件
	for (const event of events) {
		if (event.type !== "USE_ITEM") continue;
		
		print(`[ItemActivationSystem] Processing USE_ITEM for kart ${event.kartIndex}, world: ${tostring(world)}`);

		const entity = bridge.getEntityByKartIndex(event.kartIndex);
		if (!entity || !world.contains(entity)) {
			warn(`[ItemActivationSystem] Entity not found for kart ${event.kartIndex}`);
			continue;
		}
		
		print(`[ItemActivationSystem] Found entity ${entity} for kart ${event.kartIndex}`);

		const holder = world.get(entity, ItemHolder);
		const kartRef = world.get(entity, KartReference);

		if (!holder || !kartRef) {
			warn(`[ItemActivationSystem] Missing components for kart ${event.kartIndex}:`);
			warn(`  - ItemHolder: ${holder ? "present" : "missing"}`);
			warn(`  - KartReference: ${kartRef ? "present" : "missing"}`);
			continue;
		}

		// 从栈顶获取道具（使用peek查看，不立即弹出）
		const item = holder.itemStack.peek();
		if (!item) {
			print(`[ItemActivationSystem] No items in stack for kart ${event.kartIndex}`);
			continue;
		}

		// 检查冷却时间
		const now = tick();
		if (holder.lastUsedTime && now - holder.lastUsedTime < 0.5) {
			print(`[ItemActivationSystem] Item on cooldown for kart ${event.kartIndex}`);
			continue;
		}

		// 检查是否被冻结
		if (holder.frozen) {
			print(`[ItemActivationSystem] Kart ${event.kartIndex} is frozen`);
			continue;
		}

		// 激活道具
		print(`[ItemActivationSystem] Activating item: ${item} for kart ${event.kartIndex}`);
		activateItem(world, entity, item, kartRef);

		// 从栈中弹出道具（消耗）
		const newStack = holder.itemStack.clone();
		newStack.pop();

		// 更新持有者状态
		world.insert(
			entity,
			holder.patch({
				itemStack: newStack,
				lastUsedTime: now,
			}),
		);

		print(`[ItemActivationSystem] Kart ${event.kartIndex} used item: ${item}`);
	}
}

/**
 * 激活具体道具
 */
function activateItem(world: World, entity: AnyEntity, itemType: string, kartRef: KartReference): void {
	switch (itemType) {
		case "Boost":
			BoostItem.activate(world, entity, kartRef);
			break;

		case "Banana":
			// TODO: 实现香蕉道具
			print("[ItemActivationSystem] Banana item not implemented yet");
			break;

		case "Missile":
			// TODO: 实现导弹道具
			print("[ItemActivationSystem] Missile item not implemented yet");
			break;

		case "Shield":
			// TODO: 实现护盾道具
			print("[ItemActivationSystem] Shield item not implemented yet");
			break;

		default:
			warn(`[ItemActivationSystem] Unknown item type: ${itemType}`);
	}
}

export = {
	priority: 90,
	system: itemActivationSystem,
};