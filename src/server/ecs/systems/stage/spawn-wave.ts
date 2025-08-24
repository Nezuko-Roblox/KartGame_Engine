import type { World } from "@rbxts/matter";

import { Stage, StageWave } from "shared/ecs/components";

function spawnWave(world: World) {
	for (const [id] of world.query(Stage)) {
		// Spawn the wave for the stage

		// Check if the wave already exists
		const stageWave = world.get(id, StageWave);
		if (stageWave) {
			continue;
		}

		world.insert(
			id,
			StageWave({
				progress: 0,
				startTime: tick(),
				waveId: "stage_1_wave_1",
			}),
		);
	}
}

export = {
	system: spawnWave,
};
