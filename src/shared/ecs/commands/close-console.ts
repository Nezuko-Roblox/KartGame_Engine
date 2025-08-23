import type { CmdrClient, CommandContext, CommandDefinition } from "@rbxts/cmdr";

/** 关闭控制台命令定义. */
const command: CommandDefinition = {
	Aliases: ["exit"],
	Args: [],
	ClientRun: (context: CommandContext) => {
		const clientCmdr = context.Cmdr as CmdrClient;
		clientCmdr.Toggle();
		return "Closing the console";
	},
	Description: "Closes the console",
	Name: "close",
};

export = command;
