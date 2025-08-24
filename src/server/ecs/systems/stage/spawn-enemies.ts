import type { World } from "@rbxts/matter";
import { useMemo } from "@rbxts/matter-hooks";

import { StageWave } from "shared/ecs/components";
import { configDataProvider } from "shared/modules/config-data-provider";

function spawnEnemies(world: World) {
	for (const [_id, { waveId }] of world.query(StageWave)) {
		// Spawn the wave for the stage
		const config = useMemo(
			() => configDataProvider.GetConfig("WaveConfigTable", waveId),
			[waveId],
		);

		const { spawns } = config;
		for (const spawn of spawns) {
			// Spawn the enemy at the specified position
			print(`Spawning enemy at position: ${spawn}`);
		}
	}
}

export = {
	system: spawnEnemies,
};
