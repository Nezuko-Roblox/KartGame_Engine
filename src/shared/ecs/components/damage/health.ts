import { component } from "@rbxts/matter";

/** 实体的生命值组件. */
export const Health = component<{ health: number }>("Health");
export type Health = ReturnType<typeof Health>;
