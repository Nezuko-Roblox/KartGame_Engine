import { component } from "@rbxts/matter";

/**
 * 为实体附加动画器
 */
export const SpawnAnimated = component<{ animation?: string; animationSpeed?: number }>("SpawnAnimated", {
	animationSpeed: 1,
});
export type SpawnAnimated = ReturnType<typeof SpawnAnimated>;
