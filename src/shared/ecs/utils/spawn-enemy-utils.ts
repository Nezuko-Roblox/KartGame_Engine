import type { World } from "@rbxts/matter";

import { EnemySpawnRequest } from "shared/ecs/components";

/**
 * 生成敌人的辅助函数.
 *
 * @param world - Matter.js ECS 世界实例.
 * @param position - 生成位置.
 * @param enemyType - 敌人类型.
 * @param level - 敌人等级（可选，默认为1）.
 * @param patrolPoints - 巡逻点列表（可选）.
 * @param spawnDelay - 生成延迟时间（可选，默认为0）.
 */
export function spawnEnemy(
	world: World,
	position: Vector3,
	enemyType: string,
	level?: number,
	patrolPoints?: Array<Vector3>,
	spawnDelay?: number,
): void {
	world.spawn(
		EnemySpawnRequest({
			enemyType,
			level,
			patrolPoints,
			position,
			requestTime: os.clock(),
			spawnDelay,
		}),
	);
}

/**
 * 在指定区域随机生成多个敌人.
 *
 * @param world - Matter.js ECS 世界实例.
 * @param center - 区域中心位置.
 * @param radius - 生成半径.
 * @param enemyTypes - 敌人类型列表.
 * @param count - 生成数量.
 * @param minLevel - 最小等级.
 * @param maxLevel - 最大等级.
 */
export function spawnEnemiesInArea(
	world: World,
	center: Vector3,
	radius: number,
	enemyTypes: Array<string>,
	count: number,
	minLevel = 1,
	maxLevel = 1,
): void {
	for (let index = 0; index < count; index++) {
		// 在圆形区域内随机选择位置
		const angle = math.random() * math.pi * 2;
		const distance = math.random() * radius;
		const randomPosition = center.add(
			new Vector3(math.cos(angle) * distance, 0, math.sin(angle) * distance),
		);

		// 随机选择敌人类型
		const randomEnemyType = enemyTypes[math.random(0, enemyTypes.size() - 1)] ?? "goblin";

		// 随机生成等级
		const randomLevel = math.random(minLevel, maxLevel);

		// 添加随机延迟避免所有敌人同时生成
		const randomDelay = math.random() * 2;

		spawnEnemy(world, randomPosition, randomEnemyType, randomLevel, undefined, randomDelay);
	}
}

/**
 * 生成Boss敌人.
 *
 * @param world - Matter.js ECS 世界实例.
 * @param position - 生成位置.
 * @param bossType - Boss类型.
 * @param level - Boss等级.
 */
export function spawnBoss(world: World, position: Vector3, bossType: string, level: number): void {
	spawnEnemy(world, position, bossType, level);
	print(`Spawning Boss: ${bossType} (Level ${level}) at ${position}`);
}
