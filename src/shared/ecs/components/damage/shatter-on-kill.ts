import { component } from "@rbxts/matter";

/** 死亡时破碎组件. */
export const ShatterOnKill = component<{ gibsDuration: number }>("ShatterOnKill", {
	gibsDuration: 5,
});
export type ShatterOnKill = ReturnType<typeof ShatterOnKill>;
