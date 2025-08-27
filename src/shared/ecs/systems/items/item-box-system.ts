import type { World } from "@rbxts/matter";
import { ItemBox, ItemHolder, KartReference } from "shared/ecs/components/items";
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

// 道具盒子生成位置（极度分散在地图各处）
const BOX_POSITIONS = [
	// 极东区域
	new Vector3(500, 2, 100),
	new Vector3(480, 2, 150),
	new Vector3(520, 2, 50),
	
	// 极西区域
	new Vector3(200, 2, 100),
	new Vector3(180, 2, 130),
	new Vector3(220, 2, 60),
	
	// 极北区域
	new Vector3(350, 2, 250),
	new Vector3(400, 2, 230),
	new Vector3(300, 2, 270),
	
	// 极南区域
	new Vector3(350, 2, -50),
	new Vector3(380, 2, -30),
	new Vector3(320, 2, -70),
	
	// 东北远角
	new Vector3(450, 2, 200),
	new Vector3(470, 2, 220),
	
	// 西北远角
	new Vector3(250, 2, 200),
	new Vector3(230, 2, 180),
	
	// 东南远角
	new Vector3(450, 2, 0),
	new Vector3(430, 2, -20),
	
	// 西南远角
	new Vector3(250, 2, 0),
	new Vector3(270, 2, -20),
	
	// 中间过渡区
	new Vector3(355, 2, 100),
	new Vector3(400, 2, 120),
	new Vector3(310, 2, 80),
	new Vector3(355, 2, 60),
	
	// 远处散点
	new Vector3(550, 2, 150),
	new Vector3(150, 2, 150),
	new Vector3(350, 2, 300),
	new Vector3(350, 2, -100),
];

let initialized = false;

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
 * 创建道具盒子模型（简单的Part）
 */
function createBoxModel(position: Vector3): Part {
	const part = new Instance("Part");
	part.Name = "ItemBox";
	part.Size = new Vector3(4, 4, 4);
	part.Position = position;
	part.Anchored = true;
	part.CanCollide = false;
	part.BrickColor = new BrickColor("Bright yellow");
	part.Material = Enum.Material.Neon;
	part.Transparency = 0.3;
	part.Parent = Workspace;
	return part;
}

/**
 * 初始化道具盒子
 */
function initializeBoxes(world: World): void {
	if (initialized) return;
	initialized = true;
	
	
	for (const position of BOX_POSITIONS) {
		const model = createBoxModel(position);
		
		world.spawn(
			ItemBox({
				position: position,
				available: true,
				respawnDelay: 5, // 5秒重生
				collisionRadius: 6,
				model: model,
			})
		);
	}
	
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
	const isServer = RunService.IsServer();
	if (!isServer) return;
	
	// 初始化道具盒子
	initializeBoxes(world);
	
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
					gameObject?: BasePart;
				};
			};
			if (kartInstance?.m_kart?.gameObject) {
				// 从 GoPlayKart 实例获取位置
				const kartPart = kartInstance.m_kart.gameObject;
				if (kartPart) {
					kartPosition = kartPart.Position;
				}
			}
			if (!kartPosition) {
				continue;
			}
			
			// 检查碰撞
			if (checkCollision(kartPosition, itemBox.position, itemBox.collisionRadius)) {
				// 检查道具栈是否已满
				if (holder.itemStack.isFull()) {
					continue;
				}
				
				// 随机选择道具
				const randomItem = getRandomItem();
				
				// 添加道具到栈
				const newStack = holder.itemStack.clone();
				if (newStack.push(randomItem)) {
					// 更新持有者
					world.insert(kartEntity, holder.patch({
						itemStack: newStack,
					}));
					
					// 标记盒子为不可用
					world.insert(boxEntity, itemBox.patch({
						available: false,
						lastCollectedTime: currentTime,
					}));
					
					// 隐藏模型
					if (itemBox.model) {
						itemBox.model.Transparency = 1;
					}
					
					print(`[ItemBox] Collected ${randomItem}`);
				}
			}
		}
	}
}

export = {
	system: itemBoxSystem,
	priority: 50,
};