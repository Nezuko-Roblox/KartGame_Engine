import { component } from "@rbxts/matter";

/** 本地客户端组件. */
export const LocalClient = component<{
	player: Player;
}>("LocalClient");
export type LocalClient = ReturnType<typeof LocalClient>;
