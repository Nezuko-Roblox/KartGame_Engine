import type { World } from "@rbxts/matter";

import { Stage, StageWave } from "shared/ecs/components";
import { WaveEnemySpawnRequest } from "shared/ecs/components/stage/wave-enemy-spawn-request";

function spawnWave(world: World) {
	for (const [id] of world.query(Stage).without(StageWave)) {
		// 添加波次信息
		world.insert(
			id,
			StageWave({
				enemies: [],
				progress: 0,
				startTime: tick(),
				waveId: "stage_1_wave_1",
			}),
		);

		if (world.get(id, WaveEnemySpawnRequest) === undefined) {
			// 添加敌人生成请求
			world.insert(id, WaveEnemySpawnRequest());
		}
	}
}

export = {
	system: spawnWave,
};
