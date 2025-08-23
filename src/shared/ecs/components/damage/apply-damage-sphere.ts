import { component } from "@rbxts/matter";

import type { DamageType } from "shared/ecs/constants/data";

/** 施加伤害球体组件. */
export const ApplyDamageSphere = component<{
	affectsPlayers: boolean;
	damage: number;
	damageType: DamageType;
	position: Vector3;
	radius: number;
}>("ApplyDamageSphere");
export type ApplyDamageSphere = ReturnType<typeof ApplyDamageSphere>;
