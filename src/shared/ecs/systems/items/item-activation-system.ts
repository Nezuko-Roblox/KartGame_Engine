import type { World, AnyEntity } from "@rbxts/matter";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { ItemHolder, KartReference } from "shared/ecs/components/items";
import { BoostItem } from "shared/ecs/items/implementations/boost-item";
import { BananaItem } from "shared/ecs/items/implementations/banana-item";
import { ShieldItem } from "shared/ecs/items/implementations/shield-item";
import { MissileItem } from "shared/ecs/items/implementations/missile-item";
import { RunService } from "@rbxts/services";

// 存储待处理的使用道具请求
const pendingUseRequests: Array<{ kartIndex: number; timestamp: number }> = [];

/**
 * 添加使用道具请求（由远程事件调用）
 */
function addUseItemRequest(kartIndex: number): void {
	print(`[ItemActivationSystem] 添加使用道具请求到队列，卡丁车: ${kartIndex}`);
	pendingUseRequests.push({
		kartIndex,
		timestamp: tick(),
	});
	print(`[ItemActivationSystem] 当前队列长度: ${pendingUseRequests.size()}`);
}

/**
 * 道具激活系统 - 处理道具使用（仅服务端）
 */
function itemActivationSystem(world: World): void {
	// 只在服务端运行
	if (!RunService.IsServer()) return;

	// 处理所有待处理的使用道具请求
	while (pendingUseRequests.size() > 0) {
		const request = pendingUseRequests.shift();
		if (!request) break;

		print(`[ItemActivationSystem] 处理使用道具请求，卡丁车: ${request.kartIndex}`);

		// 获取对应的ECS实体
		const bridge = KartECSBridge.getInstance();
		const entity = bridge.getEntityByKartIndex(request.kartIndex);

		if (!entity || !world.contains(entity)) {
			warn(`[ItemActivationSystem] Entity not found for kart ${request.kartIndex}`);
			continue;
		}

		const holder = world.get(entity, ItemHolder);
		const kartRef = world.get(entity, KartReference);

		if (!holder || !kartRef) {
			warn(`[ItemActivationSystem] 缺少必要组件，卡丁车: ${request.kartIndex}`);
			continue;
		}

		// 从栈顶获取道具（数组末尾为栈顶）
		const item = holder.items[holder.items.size() - 1];
		if (!item) {
			print(`[ItemActivationSystem] 道具栈为空，卡丁车 ${request.kartIndex}，栈大小: ${holder.items.size()}`);
			
			// 显示栈的详细内容
			print(`[ItemActivationSystem] 栈内容: [${holder.items.join(", ")}]`);
			continue;
		}

		print(`[ItemActivationSystem] 准备使用道具: ${item}，栈大小: ${holder.items.size()}`);

		// 检查冷却时间
		const now = tick();
		if (holder.lastUsedTime && now - holder.lastUsedTime < 0.5) {
			print(`[ItemActivationSystem] 道具冷却中，卡丁车: ${request.kartIndex}`);
			continue;
		}

		// 检查是否被冻结
		if (holder.frozen) {
			print(`[ItemActivationSystem] 道具被冻结，卡丁车: ${request.kartIndex}`);
			continue;
		}

		// 激活道具
		print(`[ItemActivationSystem] 开始激活道具: ${item}`);
		activateItem(world, entity, item, kartRef);

		// 从栈中弹出道具（消耗）- 移除数组末尾元素
		const newItems = [...holder.items];
		newItems.pop();

		// 更新持有者状态
		world.insert(
			entity,
			holder.patch({
				items: newItems,
				lastUsedTime: now,
			}),
		);

		print(`[ItemActivationSystem] 道具使用完成，剩余道具: ${newItems.size()}`);
		
		// 显示剩余道具栈内容
		print(`[ItemActivationSystem] 剩余道具栈: [${newItems.join(", ")}]`);
	}
}

/**
 * 激活具体道具
 */
function activateItem(world: World, entity: AnyEntity, itemType: string, kartRef: KartReference): void {
	print(`[ItemActivationSystem] 激活道具类型: ${itemType}，卡丁车: ${kartRef.kartIndex}`);
	
	switch (itemType) {
		case "Booster":
			print(`[ItemActivationSystem] 激活加速道具`);
			BoostItem.activate(world, entity, kartRef);
			break;

		case "Banana":
			print(`[ItemActivationSystem] 激活香蕉道具`);
			BananaItem.activate(world, entity, kartRef);
			break;

		case "Shield":
		case "Guard":
			print(`[ItemActivationSystem] 激活护盾道具`);
			ShieldItem.activate(world, entity, kartRef);
			break;

		case "WaterMissile":
			print(`[ItemActivationSystem] 激活水弹道具`);
			MissileItem.activate(world, entity, kartRef);
			break;

		case "UFO":
		case "WaterFly":
		case "WaterBomb":
		case "Flip":
		case "Devil":
			// 基本的占位实现
			print(`[ItemActivationSystem] Kart ${kartRef.kartIndex} used ${itemType}! (Not fully implemented)`);
			break;

		default:
			warn(`[ItemActivationSystem] Unknown item type: ${itemType}`);
	}
	
	print(`[ItemActivationSystem] 道具激活完成: ${itemType}`);
}

export = {
	priority: 10,
	system: itemActivationSystem,
	addUseItemRequest,
};