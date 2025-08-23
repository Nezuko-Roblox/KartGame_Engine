import { component } from "@rbxts/matter";

/** 应该重生组件. */
export const ShouldRespawn = component<{
	destroyed: boolean;
	destroyTime?: number;
	respawnExclusionRadius?: number;
	respawnFrame?: CFrame;
	respawnTime?: number;
}>("ShouldRespawn", { destroyed: false, respawnTime: 30 });
export type ShouldRespawn = ReturnType<typeof ShouldRespawn>;
