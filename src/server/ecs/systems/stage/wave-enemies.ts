import type { World } from "@rbxts/matter";
import { useMemo } from "@rbxts/matter-hooks";

import { EnemySpawnRequest, StageWave } from "shared/ecs/components";
import { WaveEnemySpawnRequest } from "shared/ecs/components/stage/wave-enemy-spawn-request";
import { WaveEnemySpawned } from "shared/ecs/components/stage/wave-enemy-spawned";
import { configDataProvider } from "shared/modules/config-data-provider";

function waveEnemies(world: World) {
	for (const [id, { waveId }] of world
		.query(StageWave, WaveEnemySpawnRequest)
		.without(WaveEnemySpawned)) {
		const config = useMemo(
			() => configDataProvider.GetConfig("WaveConfigTable", waveId),
			[waveId],
		);

		const { spawns } = config;
		for (const spawn of spawns) {
			// Spawn the enemy at the specified position
			world.spawn(
				EnemySpawnRequest({
					enemyId: spawn.enemyId,
					position: new Vector3(0, 1, 0),
					requestTime: tick(),
					spawnDelay: spawn.spawnDelay,
				}),
			);
		}

		// 添加敌人生成完成标记
		world.insert(id, WaveEnemySpawned());
	}
}

export = {
	system: waveEnemies,
};
