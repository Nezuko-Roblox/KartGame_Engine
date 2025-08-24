import { component } from "@rbxts/matter";

/** 敌人AI状态组件. */
export const EnemyAI = component<{
	/** 当前巡逻点索引. */
	currentPatrolIndex?: number;
	/** 当前AI状态. */
	currentState: "attack" | "chase" | "dead" | "idle" | "patrol";
	/** 上次状态改变时间. */
	lastStateChange: number;
	/** 下次思考时间. */
	nextThinkTime: number;
	/** 巡逻点列表. */
	patrolPoints?: Array<Vector3>;
	/** 目标实体ID. */
	targetId?: number;
}>("EnemyAI");
export type EnemyAI = ReturnType<typeof EnemyAI>;
