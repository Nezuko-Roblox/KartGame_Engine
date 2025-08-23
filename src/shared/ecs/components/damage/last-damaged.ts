import type { AnyEntity } from "@rbxts/matter";
import { component } from "@rbxts/matter";

/** 最后受损组件. */
export const LastDamaged = component<{
	damaged: Array<[AnyEntity, number]>;
}>("LastDamaged", { damaged: [] });
export type LastDamaged = ReturnType<typeof LastDamaged>;
