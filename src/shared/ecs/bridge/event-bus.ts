/**
 * 道具事件接口
 */
export interface ItemEvent {
	type: "USE_ITEM" | "COLLISION" | "PICKUP";
	kartIndex: number;
	data?: unknown;
	timestamp: number;
}

/**
 * 道具事件总线 - 处理Lua和TypeScript之间的事件通信
 */
export class ItemEventBus {
	private static events: Array<ItemEvent> = [];

	/**
	 * 推送事件
	 */
	static push(event: ItemEvent): void {
		this.events.push(event);
	}

	/**
	 * 消费所有事件
	 */
	static consume(): Array<ItemEvent> {
		const current = [...this.events];
		this.events = [];
		return current;
	}

	/**
	 * 设置全局函数供Lua调用
	 */
	static setupGlobalFunctions(): void {
		// 使用道具
		(_G as unknown as { ECS_UseItem?: (kartIndex: number) => void }).ECS_UseItem = (kartIndex: number) => {
			this.push({
				type: "USE_ITEM",
				kartIndex,
				timestamp: tick(),
			});
			print(`[EventBus] Received USE_ITEM event from kart ${kartIndex}`);
		};

		// 道具碰撞
		(_G as unknown as { ECS_ItemCollision?: (kartIndex: number, itemId: string) => void }).ECS_ItemCollision = (kartIndex: number, itemId: string) => {
			this.push({
				type: "COLLISION",
				kartIndex,
				data: { itemId },
				timestamp: tick(),
			});
		};

		// 拾取道具
		(_G as unknown as { ECS_PickupItem?: (kartIndex: number, itemType: string) => void }).ECS_PickupItem = (kartIndex: number, itemType: string) => {
			this.push({
				type: "PICKUP",
				kartIndex,
				data: { itemType },
				timestamp: tick(),
			});
		};

		print("[EventBus] Global functions setup complete");
	}
}