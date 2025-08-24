import type { AnyEntity } from "@rbxts/matter";
import { component } from "@rbxts/matter";

export const StageWave = component<{
	enemies: Array<AnyEntity>;
	progress: number;
	startTime: number;
	waveId: string;
}>("StageWave");
export type StageWave = ReturnType<typeof StageWave>;
