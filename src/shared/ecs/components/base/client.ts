import { component } from "@rbxts/matter";

/** 客户端组件用于识别玩家实体. */
export const Client = component<{
	loaded?: boolean;
	player: Player;
}>("Client");
export type Client = ReturnType<typeof Client>;
