import type { World } from "@rbxts/matter";
import { ItemBox } from "shared/ecs/components/items";
import { Workspace } from "@rbxts/services";
import type { ClientState } from "shared/ecs/constants/client-state";

// 道具盒子生成位置
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

/**
 * 创建道具盒子模型
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
 * 插件：初始化道具盒子
 * 这个插件只在启动时运行一次，创建所有道具盒子
 */
export function initializeItemBoxes(world: World, state: ClientState): void {
	print("[ItemBoxPlugin] Initializing item boxes...");
	
	// 清理已存在的道具盒子（防止热重载重复）
	const existingBoxes = Workspace.GetChildren().filter(child => 
		child.IsA("Part") && child.Name === "ItemBox"
	);
	for (const box of existingBoxes) {
		box.Destroy();
	}
	
	// 创建道具盒子
	for (const position of BOX_POSITIONS) {
		const model = createBoxModel(position);
		
		world.spawn(
			ItemBox({
				position: position,
				available: true,
				respawnDelay: 5,
				collisionRadius: 6,
				model: model,
			})
		);
	}
	
	print(`[ItemBoxPlugin] Created ${BOX_POSITIONS.size()} item boxes`);
}