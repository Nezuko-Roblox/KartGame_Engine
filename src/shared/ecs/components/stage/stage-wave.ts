import { component } from "@rbxts/matter";

export const StageWave = component<{
	progress: number;
	startTime: number;
	waveId: string;
}>("StageWave");
export type StageWave = ReturnType<typeof StageWave>;
