import { component } from "@rbxts/matter";

import type { CurrentPlayerSave } from "shared/ecs/constants/player-save";

/** 玩家设置组件. */
export const PlayerSettings = component<CurrentPlayerSave["settings"]>("PlayerSettings", {
	musicEnabled: true,
	musicVolume: 0.5,
	sfxVolume: 0.5,
});
export type PlayerSettings = ReturnType<typeof PlayerSettings>;
