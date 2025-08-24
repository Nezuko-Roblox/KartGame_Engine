import { component } from "@rbxts/matter";

export const Stage = component<{
	isPaused: boolean;
	stageId: string;
}>("Stage");
export type Stage = ReturnType<typeof Stage>;
