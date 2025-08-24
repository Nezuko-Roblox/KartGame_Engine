import type { ComponentCtor } from "@rbxts/matter/lib/component";

import {
	Client,
	DamageResistance,
	Enemy,
	Gibs,
	Health,
	Movement,
	ReceiveForce,
	TrackSync,
	Transform,
} from "shared/ecs/components";

/** 应该自动复制到玩家的组件. */
export const REPLICATED_COMPONENTS = new Set<ComponentCtor>([
	Client,
	Enemy,
	Gibs,
	Health,
	Movement,
	TrackSync,
	Transform,
]);
/** 应该自动复制到分配给的玩家的组件. */
export const REPLICATED_PLAYER_ONLY = new Set<ComponentCtor>([
	DamageResistance,
	Health,
	ReceiveForce,
]);
