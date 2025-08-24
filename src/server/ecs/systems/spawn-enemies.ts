import type { AnyEntity, World } from "@rbxts/matter";

import { Enemy, EnemyAI, EnemySpawnRequest, Health, Transform } from "shared/ecs/components";
import { ENEMY_CONFIGS } from "shared/ecs/constants/enemy-config";

/**
 * 敌人生成系统 - 处理敌人生成请求并创建敌人实体（仅逻辑，不包含渲染）.
 *
 * @param world - Matter.js ECS 世界实例.
 */
function spawnEnemies(world: World) {
	const currentTime = os.clock();

	// 处理敌人生成请求
	for (const [id, spawnRequest] of world.queryChanged(EnemySpawnRequest)) {
		if (!spawnRequest.new) {
			continue;
		}

		const request = spawnRequest.new;
		const spawnDelay = request.spawnDelay ?? 0;

		// 检查是否到达生成时间
		if (currentTime < request.requestTime + spawnDelay) {
			continue;
		}

		// 获取敌人配置
		const enemyConfig = ENEMY_CONFIGS[request.enemyType];
		if (!enemyConfig) {
			warn(`Unknown enemy type: ${request.enemyType}`);
			world.remove(id as AnyEntity, EnemySpawnRequest);
			continue;
		}

		// 计算等级修正
		const level = request.level ?? 1;
		/** 每级增加15%属性. */
		const levelMultiplier = 1 + (level - 1) * 0.15;

		// 创建敌人实体（仅逻辑组件，不包含Renderable）
		world.spawn(
			Enemy({
				attackPower: math.floor(enemyConfig.attackPower * levelMultiplier),
				attackRange: enemyConfig.attackRange,
				detectionRange: enemyConfig.detectionRange,
				level,
				maxHealth: math.floor(enemyConfig.maxHealth * levelMultiplier),
				moveSpeed: enemyConfig.moveSpeed,
				type: enemyConfig.type,
			}),
			Health({
				health: math.floor(enemyConfig.maxHealth * levelMultiplier),
			}),
			Transform({
				cf: new CFrame(request.position),
			}),
			EnemyAI({
				currentPatrolIndex: 0,
				currentState: "idle",
				lastStateChange: currentTime,
				/** 随机延迟开始思考. */
				nextThinkTime: currentTime + math.random(1, 3),
				patrolPoints: request.patrolPoints,
			}),
		);

		print(`Spawned enemy: ${enemyConfig.type} (Level ${level}) at ${request.position}`);

		// 移除生成请求
		world.remove(id as AnyEntity, EnemySpawnRequest);
	}

	// 处理死亡的敌人
	for (const [id, enemy, health] of world.query(Enemy, Health)) {
		if (health.health <= 0) {
			// 更新AI状态为死亡
			const ai = world.get(id, EnemyAI);
			if (ai) {
				world.insert(
					id as AnyEntity,
					ai.patch({
						currentState: "dead",
						lastStateChange: currentTime,
						targetId: undefined,
					}),
				);
			}

			print(`Enemy ${enemy.type} (Level ${enemy.level}) died`);

			// 可以选择立即移除实体或者保留一段时间让客户端处理死亡动画
			// world.despawn(id as AnyEntity);
		}
	}
}

/** 导出敌人生成系统. */
export = {
	system: spawnEnemies,
};
