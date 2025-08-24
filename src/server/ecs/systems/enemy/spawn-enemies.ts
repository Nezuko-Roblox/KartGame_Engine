import type { AnyEntity, World } from "@rbxts/matter";
import { useMemo } from "@rbxts/matter-hooks";

import { Enemy, EnemySpawnRequest, Movement, Transform } from "shared/ecs/components";
import { configDataProvider } from "shared/modules/config-data-provider";

/**
 * 敌人生成系统 - 处理敌人生成请求并创建敌人实体（仅逻辑，不包含渲染）.
 *
 * @param world - Matter.js ECS 世界实例.
 */
function spawnEnemies(world: World) {
	const currentTime = tick();

	// 处理敌人生成请求
	for (const [id, { enemyId, position, requestTime, spawnDelay = 0 }] of world.query(
		EnemySpawnRequest,
	)) {
		// 检查是否到达生成时间
		if (currentTime < requestTime + spawnDelay) {
			continue;
		}

		const enemyConfig = useMemo(
			() => configDataProvider.GetConfig("EnemyConfigTable", enemyId),
			[enemyId],
		);

		const enemyTypeConfig = useMemo(
			() => configDataProvider.GetConfig("EnemyTypeConfigTable", enemyConfig.enemyType),
			[enemyConfig.enemyType],
		);

		// 创建敌人实体（仅逻辑组件，不包含Renderable）
		world.spawn(
			Enemy({ enemyId }),
			Transform({ cf: new CFrame(position) }),
			Movement({ direction: new Vector3(), speed: enemyTypeConfig.stats.moveSpeed }),
		);

		print(
			`Spawned enemy: ${enemyConfig.enemyType} (Level ${enemyConfig.level}) at ${position}`,
		);

		world.despawn(id as AnyEntity);
	}
}

/** 导出敌人生成系统. */
export = {
	system: spawnEnemies,
};
