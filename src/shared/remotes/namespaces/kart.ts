import type { Client, Server } from "@rbxts/remo";
import { namespace, remote } from "@rbxts/remo";

export const kartRemote = namespace({
	// 客户端通知服务端卡丁车已创建
	notifyKartCreated: remote<Server, [kartIndex: number, playerId: number]>(),
	// 服务端确认卡丁车注册
	confirmKartRegistered: remote<Client, [kartIndex: number, entityId: number]>(),
	// 客户端同步卡丁车位置到服务端
	syncKartPosition: remote<Server, [kartIndex: number, position: Vector3]>(),
	// 服务端通知客户端应用加速效果
	applyBoost: remote<Client, [kartIndex: number, duration: number, boostType: number]>(),
});