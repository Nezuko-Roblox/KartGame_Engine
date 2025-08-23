import { component } from "@rbxts/matter";

/** 可破坏组件. */
export const Breakable = component<{
	health: number;
}>("Breakable", { health: 50 });
export type Breakable = ReturnType<typeof Breakable>;
