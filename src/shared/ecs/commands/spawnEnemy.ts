import type { CommandDefinition } from "@rbxts/cmdr";

/** 生成敌人命令定义，用于调试时生成敌人实体. */
const command: CommandDefinition = {
	Aliases: ["spawnenemy", "se"],
	Args: [
		{
			Description: "The type of enemy to spawn (goblin, orc, skeleton, bossDragon)",
			Name: "enemyType",
			Type: "string",
		},
		{
			Description: "X position",
			Name: "x",
			Type: "number",
		},
		{
			Description: "Y position",
			Name: "y",
			Type: "number",
		},
		{
			Description: "Z position",
			Name: "z",
			Type: "number",
		},
		{
			Default: 1,
			Description: "Enemy level (optional, default: 1)",
			Name: "level",
			Optional: true,
			Type: "number",
		},
	],
	Description: "Spawns an enemy at the specified position",
	Name: "spawnEnemy",
};

export = command;
