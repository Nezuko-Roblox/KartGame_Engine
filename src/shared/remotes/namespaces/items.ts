import type { Server } from "@rbxts/remo";
import { namespace, remote } from "@rbxts/remo";

export const itemsRemote = namespace({
	// 服务端通知客户端道具箱子状态变化
	syncItemBoxState: remote<Server, [boxId: number, available: boolean]>(),
	// 服务端通知客户端道具箱子重生
	itemBoxRespawned: remote<Server, [boxId: number]>(),
	// 客户端通知服务端获得道具（漂移等）
	notifyItemPickup: remote<Server, [kartIndex: number, itemType: string]>(),
	// 客户端请求使用道具
	requestUseItem: remote<Server, [kartIndex: number]>(),
});