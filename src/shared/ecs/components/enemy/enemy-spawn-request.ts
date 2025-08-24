import { component } from "@rbxts/matter";

/** 敌人生成请求组件. */
export const EnemySpawnRequest = component<{
	/** 敌人ID. */
	enemyId: string;
	/** 生成位置. */
	position: Vector3;
	/** 请求生成的时间. */
	requestTime: number;
	/** 生成延迟时间（秒）. */
	spawnDelay?: number;
}>("EnemySpawnRequest");
export type EnemySpawnRequest = ReturnType<typeof EnemySpawnRequest>;
