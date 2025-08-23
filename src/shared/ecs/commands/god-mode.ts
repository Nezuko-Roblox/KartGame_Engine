import type { CommandDefinition } from "@rbxts/cmdr";

/** 上帝模式命令定义，用于切换玩家无敌状态. */
const command: CommandDefinition = {
	Aliases: ["gdm", "god"],
	Args: [],
	Description: "Turns off damage for a player",
	Name: "godmode",
};

export = command;
