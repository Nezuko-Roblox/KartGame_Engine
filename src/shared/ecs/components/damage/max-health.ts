import { component } from "@rbxts/matter";

/** 最大生命值组件. */
export const MaxHealth = component<{ amount: number }>("MaxHealth");
export type MaxHealth = ReturnType<typeof MaxHealth>;
