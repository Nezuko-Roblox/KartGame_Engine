import type { CommandDefinition } from "@rbxts/cmdr";

/** 生成敌人命令定义，用于调试时生成敌人实体. */
const command: CommandDefinition = {
	Aliases: [],
	Args: [
		{
			Description: "The ID of the stage to create",
			Name: "stageId",
			Type: "string",
		},
	],
	Description: "Creates a new stage",
	Name: "createStage",
};

export = command;
