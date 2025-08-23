import { component } from "@rbxts/matter";

/** 存档节流组件. */
export const SaveThrottle = component<{ time: number }>("SaveThrottle", { time: 10 });
export type SaveThrottle = ReturnType<typeof SaveThrottle>;
