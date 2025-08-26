import { component } from "@rbxts/matter";

/**
 * 道具持有者组件 - 管理卡丁车的道具槽位
 */
export const ItemHolder = component<{
	/** 道具槽位 */
	items: Array<string | undefined>;
	/** 当前选中的槽位索引 */
	currentSlot: number;
	/** 最大槽位数 */
	maxSlots: number;
	/** 是否被冻结无法使用 */
	frozen: boolean;
	/** 上次使用道具的时间 */
	lastUsedTime?: number;
}>("ItemHolder");

export type ItemHolder = ReturnType<typeof ItemHolder>;