import { component } from "@rbxts/matter";

/** 施加力球体组件. */
export const ApplyForceSphere = component<{
	affectsPlayers: boolean;
	force: number;
	position: Vector3;
	radius: number;
}>("ApplyForceSphere");
export type ApplyForceSphere = ReturnType<typeof ApplyForceSphere>;
