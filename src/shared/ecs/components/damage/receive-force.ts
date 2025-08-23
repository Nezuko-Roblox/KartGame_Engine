import type { AnyEntity } from "@rbxts/matter";
import { component } from "@rbxts/matter";

/** 接受力组件. */
export const ReceiveForce = component<{ force: Vector3; inflictor?: AnyEntity }>("ReceiveForce");
export type ReceiveForce = ReturnType<typeof ReceiveForce>;
