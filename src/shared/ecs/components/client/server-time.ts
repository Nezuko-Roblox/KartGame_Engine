import { component } from "@rbxts/matter";

/** 服务器时间组件. */
export const ServerTime = component<{
	lastSync?: number;
	latency?: number;
	offset?: number;
	serverTime: number;
}>("ServerTime");
export type ServerTime = ReturnType<typeof ServerTime>;
