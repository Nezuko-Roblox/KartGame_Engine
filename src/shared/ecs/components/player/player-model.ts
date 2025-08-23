import { component } from "@rbxts/matter";

/** 玩家模型组件. */
export const PlayerModel = component<{
	character: Model;
	humanoid: Humanoid;
}>("PlayerModel");
export type PlayerModel = ReturnType<typeof PlayerModel>;
