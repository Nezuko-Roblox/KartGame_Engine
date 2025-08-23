import { component } from "@rbxts/matter";

import type { PlayerGameState } from "shared/ecs/constants/player-state";

/** 玩家状态组件. */
export const PlayerState = component<{
	state: PlayerGameState;
}>("PlayerState");
export type PlayerState = ReturnType<typeof PlayerState>;
