import { component } from "@rbxts/matter";

/**
 * 卡丁车引用组件 - 连接ECS实体和原有卡丁车系统
 */
export const KartReference = component<{
	/** KartManager中的索引 (1-6) */
	kartIndex: number;
	/** GoKart实例引用 */
	kartInstance: unknown;
	/** 是否是玩家 */
	isPlayer: boolean;
	/** 当前位置（服务端用于碰撞检测） */
	position?: Vector3;
}>("KartReference");

export type KartReference = ReturnType<typeof KartReference>;