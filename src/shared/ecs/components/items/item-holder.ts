import { component } from "@rbxts/matter";

/**
 * 道具持有者组件 - 管理卡丁车的道具栈
 */
export const ItemHolder = component<{
	/** 道具数组 - 使用数组模拟栈（后进先出），末尾为栈顶 */
	items: string[];
	/** 最大槽位数 */
	maxSlots: number;
	/** 是否被冻结无法使用 */
	frozen: boolean;
	/** 上次使用道具的时间 */
	lastUsedTime?: number;
}>("ItemHolder");

export type ItemHolder = ReturnType<typeof ItemHolder>;