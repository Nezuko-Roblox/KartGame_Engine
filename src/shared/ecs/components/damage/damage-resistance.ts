import { component } from "@rbxts/matter";

/** 伤害抵抗组件. */
export const DamageResistance = component<{
	bluntResistance?: number;
	crushResistance?: number;
	electricResistance?: number;
	explosiveResistance?: number;
	fireResistance?: number;
	poisonResistance?: number;
	radiationResistance?: number;
	slashResistance?: number;
}>("DamageResistance");
export type DamageResistance = ReturnType<typeof DamageResistance>;
