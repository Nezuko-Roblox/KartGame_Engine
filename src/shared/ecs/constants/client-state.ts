import type { AnyEntity } from "@rbxts/matter";

/** 客户端状态接口定义. */
export interface ClientState {
	/** 是否启用调试模式. */
	debugEnabled: boolean;
	/** 实体ID映射表，用于客户端和服务端实体同步. */
	entityIdMap: Map<string, AnyEntity>;
}
