import type { AnyEntity } from "@rbxts/matter";
import { component } from "@rbxts/matter";

import type { DamageType } from "shared/ecs/constants/data";

/** 受伤组件. */
export const TakeDamage = component<{
	amount: number;
	damageType: DamageType;
	inflictor?: AnyEntity;
}>("TakeDamage");
export type TakeDamage = ReturnType<typeof TakeDamage>;
