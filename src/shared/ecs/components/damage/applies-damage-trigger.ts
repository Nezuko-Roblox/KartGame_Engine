import { component } from "@rbxts/matter";

import { DamageType } from "shared/ecs/constants/data";

/** 施加伤害触发器组件. */
export const AppliesDamageTrigger = component<{
	damage: number;
	damageCooldown: number;
	damageType: DamageType;
}>("AppliesDamageTrigger", { damage: 1, damageCooldown: 1, damageType: DamageType.Slash });
export type AppliesDamageTrigger = ReturnType<typeof AppliesDamageTrigger>;
