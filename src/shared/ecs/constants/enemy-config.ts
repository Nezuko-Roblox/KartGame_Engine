/** 敌人配置接口. */
export interface EnemyConfig {
	/** 攻击力. */
	attackPower: number;
	/** 攻击范围. */
	attackRange: number;
	/** 检测范围. */
	detectionRange: number;
	/** 最大生命值. */
	maxHealth: number;
	/** 移动速度. */
	moveSpeed: number;
	/** 敌人类型. */
	type: string;
}

/** 敌人配置映射. */
export const ENEMY_CONFIGS: Record<string, EnemyConfig> = {
	bossDragon: {
		attackPower: 50,
		attackRange: 8,
		detectionRange: 30,
		maxHealth: 1000,
		moveSpeed: 4,
		type: "boss_dragon",
	},
	goblin: {
		attackPower: 15,
		attackRange: 3,
		detectionRange: 15,
		maxHealth: 100,
		moveSpeed: 8,
		type: "goblin",
	},
	orc: {
		attackPower: 25,
		attackRange: 4,
		detectionRange: 20,
		maxHealth: 200,
		moveSpeed: 6,
		type: "orc",
	},
	skeleton: {
		attackPower: 20,
		attackRange: 5,
		detectionRange: 12,
		maxHealth: 80,
		moveSpeed: 10,
		type: "skeleton",
	},
};
