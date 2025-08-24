import { component } from "@rbxts/matter";

/** 实体的基础移动数据 (速度/加速度/最大速度). */
export const Movement = component<{
	direction: Vector3;
	/** 当前速度 (可选). */
	speed: number;
}>("Movement");
export type Movement = ReturnType<typeof Movement>;
