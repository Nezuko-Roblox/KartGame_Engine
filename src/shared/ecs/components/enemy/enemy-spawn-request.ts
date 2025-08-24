import { component } from "@rbxts/matter";

/** 敌人生成请求组件. */
export const EnemySpawnRequest = component<{
	/** 敌人类型. */
	enemyType: string;
	/** 敌人等级. */
	level?: number;
	/** 巡逻点列表. */
	patrolPoints?: Array<Vector3>;
	/** 生成位置. */
	position: Vector3;
	/** 请求生成的时间. */
	requestTime: number;
	/** 生成延迟时间（秒）. */
	spawnDelay?: number;
}>("EnemySpawnRequest");
export type EnemySpawnRequest = ReturnType<typeof EnemySpawnRequest>;
