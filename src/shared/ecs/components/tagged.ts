import { Breakable, DamageResistance, ShatterOnKill, ShouldRespawn } from "shared/ecs/components";

/** 具有与这些组件匹配的 CollectionService 标签的模型将自动添加到世界中. */
export const TAGGED_COMPONENTS = new Set([
	Breakable,
	DamageResistance,
	ShatterOnKill,
	ShouldRespawn,
]);
