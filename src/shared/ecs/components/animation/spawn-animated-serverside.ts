import { component } from "@rbxts/matter";

/**
 * 为实体附加动画器，同步到所有客户端
 */
export const SpawnAnimatedServerside = component<{ animation?: string; animationSpeed?: number }>(
	"SpawnAnimatedServerside",
	{
		animationSpeed: 1,
	},
);
export type SpawnAnimatedServerside = ReturnType<typeof SpawnAnimatedServerside>;
