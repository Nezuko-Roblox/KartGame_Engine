import { component } from "@rbxts/matter";

/** 标记实体为敌人的组件. */
export const Enemy = component<{
	/** 攻击力. */
	attackPower: number;
	/** 攻击范围. */
	attackRange: number;
	/** 检测范围. */
	detectionRange: number;
	/** 敌人等级. */
	level: number;
	/** 最大生命值. */
	maxHealth: number;
	/** 移动速度. */
	moveSpeed: number;
	/** 敌人类型. */
	type: string;
}>("Enemy");
export type Enemy = ReturnType<typeof Enemy>;
