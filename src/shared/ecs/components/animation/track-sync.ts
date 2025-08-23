import { component } from "@rbxts/matter";

/** 同步动画轨道. */
export const TrackSync = component<{ serverTime: number; stopped?: boolean; trackTime: number }>(
	"TrackSync",
);
export type TrackSync = ReturnType<typeof TrackSync>;
