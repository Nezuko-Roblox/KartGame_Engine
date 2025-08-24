import type { CommandContextWithWorld } from "@rbxts/cmdr";

import { ENEMY_CONFIGS } from "shared/ecs/constants/enemy-config";
import { spawnEnemy } from "shared/ecs/utils/spawn-enemy-utils";

/**
 * 服务器端 spawn-enemy 命令实现 (与 shared 命令定义同名).
 *
 * @param context - 命令上下文(包含 world 注入).
 * @param enemyType - 敌人类型 key.
 * @param x - 世界 X 坐标.
 * @param y - 世界 Y 坐标.
 * @param z - 世界 Z 坐标.
 * @param level - 敌人等级 (默认 1).
 * @returns 结果字符串.
 */
export = function (
	context: CommandContextWithWorld,
	enemyType: string,
	x: number,
	y: number,
	z: number,
	level = 1,
) {
	print(`Spawn request received: ${enemyType} (Level ${level}) at (${x}, ${y}, ${z})`);
	const { world } = context;
	if (!world) {
		return "World not available (permission denied or not initialized)";
	}

	if (!ENEMY_CONFIGS[enemyType]) {
		return `Unknown enemy type: ${enemyType}`;
	}

	const position = new Vector3(x, y, z);
	spawnEnemy(world, position, enemyType, level);
	return `Spawn request queued: ${enemyType} (Level ${level}) at (${x}, ${y}, ${z})`;
};
