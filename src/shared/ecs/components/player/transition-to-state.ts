import { component } from "@rbxts/matter";

import { PlayerGameState } from "shared/ecs/constants/player-state";

const Workspace = game.GetService("Workspace");

/** 状态过渡组件. */
export const TransitionToState = component<{
	created: number;
	nonSkippable?: boolean;
	state: PlayerGameState;
	time: number;
}>("TransitionToState", {
	created: Workspace.GetServerTimeNow(),
	state: PlayerGameState.Playing,
	time: 0,
});
export type TransitionToState = ReturnType<typeof TransitionToState>;
