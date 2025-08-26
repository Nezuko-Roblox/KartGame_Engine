import { component } from "@rbxts/matter";

/**
 * 加速Buff组件 - 管理加速道具效果
 */
export const BoostBuff = component<{
	/** 目标卡丁车索引 */
	kartIndex: number;
	/** 持续时间（毫秒） */
	duration: number;
	/** 开始时间 */
	startTime: number;
	/** 是否已应用 */
	applied: boolean;
}>("BoostBuff");

export type BoostBuff = ReturnType<typeof BoostBuff>;