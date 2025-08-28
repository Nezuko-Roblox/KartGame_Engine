import type { World } from "@rbxts/matter";
import { ItemBox, ItemHolder, KartReference } from "shared/ecs/components/items";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { RunService, Workspace } from "@rbxts/services";

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
 * 道具盒子系统 - 处理生成、碰撞和重生
 */
function itemBoxSystem(world: World): void {
	// const isServer = RunService.IsServer();
	// if (!isServer) return;
	 
	const currentTime = tick();
	
	// 处理每个道具盒子
	for (const [boxEntity, itemBox] of world.query(ItemBox)) {
		// 检查重生
		if (!itemBox.available && itemBox.lastCollectedTime) {
			if (currentTime - itemBox.lastCollectedTime >= itemBox.respawnDelay) {
				// 重生道具盒子
				world.insert(boxEntity, itemBox.patch({
					available: true,
					lastCollectedTime: undefined,
				}));
				
				// 显示模型
				if (itemBox.model) {
					itemBox.model.Transparency = 0.3;
				}
			}
			continue;
		}
		
		// 只处理可用的盒子
		if (!itemBox.available) continue;
		
		// 检查与所有卡丁车的碰撞
		for (const [kartEntity, kartRef, holder] of world.query(KartReference, ItemHolder)) {
			// 获取卡丁车位置
			let kartPosition: Vector3 | undefined;
			
			// 尝试从 kartInstance 直接获取位置
			const kartInstance = kartRef.kartInstance as unknown as {
				m_kart?: {
					gameObject?: BasePart | Model;
				};
			};
			if (kartInstance?.m_kart?.gameObject) {
				// 从 GoPlayKart 实例获取位置
				const kartObject = kartInstance.m_kart.gameObject;
				if (kartObject) {
					// 如果是 Model，获取其 PrimaryPart 的位置
					if (kartObject.IsA("Model")) {
						const model = kartObject as Model;
						if (model.PrimaryPart) {
							kartPosition = model.PrimaryPart.Position;
						} else {
							// 如果没有 PrimaryPart，尝试获取第一个 Part
							const firstPart = model.FindFirstChildOfClass("Part") || model.FindFirstChildOfClass("MeshPart");
							if (firstPart) {
								kartPosition = firstPart.Position;
							}
						}
					} else if (kartObject.IsA("BasePart")) {
						// 如果是 BasePart，直接获取位置
						kartPosition = (kartObject as BasePart).Position;
					}
				}
			}
			if (!kartPosition) {
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
					
					// 标记盒子为不可用
					world.insert(boxEntity, itemBox.patch({
						available: false,
						lastCollectedTime: currentTime,
					}));
					
					// 隐藏模型
					if (itemBox.model) {
						itemBox.model.Transparency = 1;
					}
					
					print(`[ItemBox] Triggered pickup event for ${randomItem}`);
				}
			}
		}
	}
}

export = {
	system: itemBoxSystem,
	priority: 20,
};