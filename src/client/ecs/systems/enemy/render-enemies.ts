import type { AnyEntity, World } from "@rbxts/matter";

import { Enemy, Renderable, Transform } from "shared/ecs/components";

const Workspace = game.GetService("Workspace");

/**
 * 简单敌人渲染系统: 为每个没有模型的敌人生成一个彩色方块 Model.
 *
 * @param world - ECS 世界.
 */
function renderEnemies(world: World) {
	for (const [id, enemy, transform] of world.query(Enemy, Transform).without(Renderable)) {
		// 创建模型
		const model = new Instance("Model");
		model.Name = `Enemy_${enemy.enemyId}`;

		const primary = new Instance("Part");
		primary.Name = "Primary";
		primary.Size = new Vector3(2, 2, 2);
		primary.CFrame = transform.cf;
		primary.Anchored = true;
		primary.Color = Color3.fromRGB(255, 70, 70);
		primary.Parent = model;
		model.PrimaryPart = primary;
		model.Parent = Workspace;

		world.insert(id as AnyEntity, Renderable({ model }));
	}

	// 同步已有模型位置
	for (const [_id, _enemy, transform, renderable] of world.query(Enemy, Transform, Renderable)) {
		const { model } = renderable;
		const primary = model.PrimaryPart;
		if (primary && primary.CFrame !== transform.cf) {
			primary.CFrame = transform.cf;
		}
	}
}

export = {
	system: renderEnemies,
};
