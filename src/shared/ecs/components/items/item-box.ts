import { component } from "@rbxts/matter";

/**
 * 道具盒子组件
 */
export const ItemBox = component<{
	/** 位置 */
	position: Vector3;
	/** 是否可用 */
	available: boolean;
	/** 上次被拾取的时间 */
	lastCollectedTime?: number;
	/** 重生延迟（秒） */
	respawnDelay: number;
	/** 碰撞半径 */
	collisionRadius: number;
	/** 盒子模型 */
	model?: Part;
}>("ItemBox");

export type ItemBox = ReturnType<typeof ItemBox>;