import type { AnyEntity } from "@rbxts/matter";
import { component } from "@rbxts/matter";

/** 追踪被实体触碰的模型及其相应的实体. */
export const Touched = component<{ entities: Array<AnyEntity>; parts: Array<BasePart> }>(
	"Touched",
	{
		entities: [],
		parts: [],
	},
);
export type Touched = ReturnType<typeof Touched>;
