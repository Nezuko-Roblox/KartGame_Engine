import { component } from "@rbxts/matter";

/** 追踪和自动释放短时间存在的部件的方法. */
export const Gibs = component<{
	existTime: number;
	fadeTime: number;
	parts: Array<BasePart>;
	spawnTime: number;
}>("Gibs");
export type Gibs = ReturnType<typeof Gibs>;
