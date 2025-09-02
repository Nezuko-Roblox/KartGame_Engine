import type { World } from "@rbxts/matter";
import { ItemBox, ItemHolder, KartReference } from "shared/ecs/components/items";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { RunService } from "@rbxts/services";

// 道具类型（基于源代码GameItem枚举）
const ItemTypes = {
	BOOSTER: "Booster",
	BANANA: "Banana",
	UFO: "UFO",
	WATER_FLY: "WaterFly",
	WATER_BOMB: "WaterBomb",
	FLIP: "Flip",
	DEVIL: "Devil",
	WATER_MISSILE: "WaterMissile",
	GUARD: "Guard",
	SHIELD: "Shield",
} as const;

// 可获得的道具列表及权重
const ITEM_WEIGHTS = [
	{ item: ItemTypes.BOOSTER, weight: 30 },
	{ item: ItemTypes.BANANA, weight: 20 },
	{ item: ItemTypes.SHIELD, weight: 15 },
	{ item: ItemTypes.WATER_MISSILE, weight: 10 },
	{ item: ItemTypes.WATER_BOMB, weight: 10 },
	{ item: ItemTypes.UFO, weight: 5 },
	{ item: ItemTypes.WATER_FLY, weight: 5 },
	{ item: ItemTypes.FLIP, weight: 3 },
	{ item: ItemTypes.DEVIL, weight: 2 },
];

/**
 * 根据权重随机选择道具
 */
function getRandomItem(): string {
	const totalWeight = ITEM_WEIGHTS.reduce((sum, item) => sum + item.weight, 0);
	let random = math.random() * totalWeight;
	
	for (const itemData of ITEM_WEIGHTS) {
		random -= itemData.weight;
		if (random <= 0) {
			return itemData.item;
		}
	}
	
	return ItemTypes.BOOSTER;
}

/**
 * 检查碰撞
 */
function checkCollision(kartPos: Vector3, boxPos: Vector3, radius: number): boolean {
	const distance = (kartPos.sub(boxPos)).Magnitude;
	return distance < radius;
}

/**
 * 服务端道具盒子系统 - 权威处理碰撞检测和状态管理
 */
function itemBoxServerSystem(world: World): void {
	// 只在服务端运行
	if (!RunService.IsServer()) return;
	 
	const currentTime = tick();
	
	// 处理重生逻辑
	for (const [boxEntity, itemBox] of world.query(ItemBox)) {
		// 检查重生
		if (!itemBox.available && itemBox.lastCollectedTime) {
			if (currentTime - itemBox.lastCollectedTime >= itemBox.respawnDelay) {
				// 重生道具盒子（状态会自动通过复制系统同步到客户端）
				world.insert(boxEntity, itemBox.patch({
					available: true,
					lastCollectedTime: undefined,
				}));
				
				print(`[ItemBoxServer] Box ${boxEntity} respawned`);
			}
		}
	}
	
	// 处理碰撞检测（只对可用的盒子）
	// 计算可用盒子和卡丁车实体数量
	let availableBoxCount = 0;
	let kartEntityCount = 0;
	
	for (const [, itemBox] of world.query(ItemBox)) {
		if (itemBox.available) availableBoxCount++;
	}
	
	for (const [,] of world.query(KartReference, ItemHolder)) {
		kartEntityCount++;
	}
	
	// 每10秒输出一次状态信息
	if (currentTime % 10 < 0.1) {
		print(`[ItemBoxServer] 状态检查 - 可用箱子: ${availableBoxCount}, 卡丁车实体: ${kartEntityCount}`);
	}
	
	for (const [boxEntity, itemBox] of world.query(ItemBox)) {
		if (!itemBox.available) continue;
		// 检查与所有卡丁车的碰撞
		for (const [kartEntity, kartRef] of world.query(KartReference, ItemHolder)) {
			// 使用组件中存储的位置（由客户端同步）
			const kartPosition = kartRef.position;
			
			if (!kartPosition) {
				// 位置还未同步，跳过这个卡丁车
				continue;
			}
			
			// 检查碰撞
			if (checkCollision(kartPosition, itemBox.position, itemBox.collisionRadius)) {
				// 获取卡丁车索引
				const bridge = KartECSBridge.getInstance();
				const kartIndex = bridge.getKartIndexByEntity(kartEntity);
				
				if (kartIndex !== undefined) {
					// 随机选择道具
					const randomItem = getRandomItem();
					
					// 发送拾取事件
					ItemEventBus.push({
						type: "PICKUP",
						kartIndex: kartIndex,
						data: { itemType: randomItem },
						timestamp: tick(),
					});
					
					// 标记盒子为不可用（这会自动通过复制系统同步到客户端）
					world.insert(boxEntity, itemBox.patch({
						available: false,
						lastCollectedTime: currentTime,
					}));
					
					print(`[ItemBoxServer] Player ${kartIndex} picked up ${randomItem} from box ${boxEntity}`);
				}
			}
		}
	}
}

export = {
	system: itemBoxServerSystem,
	priority: 20,
};