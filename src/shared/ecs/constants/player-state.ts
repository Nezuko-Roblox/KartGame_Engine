/** 玩家游戏状态枚举. */
export enum PlayerGameState {
	/** 已连接. */
	Connected,
	/** 游戏中. */
	Playing,
	/** 死亡. */
	Dead,
	/** 生成中. */
	Spawning,
	/** 已断线. */
	Disconnected,
	/** 准备就绪. */
	Ready,
	/** 复活中. */
	Reviving,
	/** 已复活. */
	Revived,
}

/**
 * 将玩家状态转换为字符串描述.
 *
 * @param state - 玩家状态枚举值或数字.
 * @returns 状态的字符串表示.
 */
export function translatePlayerState(state: number | PlayerGameState): string | undefined {
	const names = [
		"Connected",
		"Playing",
		"Dead",
		"Spawning",
		"Disconnected",
		"Ready",
		"Reviving",
		"Revived",
	];

	return names[state];
}
