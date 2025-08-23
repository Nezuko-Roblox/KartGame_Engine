import type { AnyEntity } from "@rbxts/matter";
import { component } from "@rbxts/matter";

/** 根据它们的工作区模型追踪实体的方法. */
export const BoundEntities = component<{ entities: Array<AnyEntity>; models: Array<Model> }>(
	"BoundEntities",
	{
		entities: [],
		models: [],
	},
);
export type BoundEntities = ReturnType<typeof BoundEntities>;
