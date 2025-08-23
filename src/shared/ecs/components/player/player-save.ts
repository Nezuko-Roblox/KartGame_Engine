import { component } from "@rbxts/matter";

import type { CurrentPlayerSave } from "shared/ecs/constants/player-save";

/** 玩家存档组件. */
export const PlayerSave = component<{ playerId: number; save: CurrentPlayerSave }>("PlayerSave");
export type PlayerSave = ReturnType<typeof PlayerSave>;
