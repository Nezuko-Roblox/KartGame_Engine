import { component } from "@rbxts/matter";

/** 修改实体的坐标框架. */
export const Transform = component<{ cf: CFrame; doNotReconcile?: boolean }>("Transform");
export type Transform = ReturnType<typeof Transform>;
