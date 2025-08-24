import type { CommandContextWithWorld } from "@rbxts/cmdr";
import { Cmdr } from "@rbxts/cmdr";

import { $env } from "rbxts-transform-env";
import type { ClientState } from "shared/ecs/constants/client-state";
import { start } from "shared/ecs/start";
import { setupTags } from "shared/ecs/utils/setup-tags";

// 注册默认命令
Cmdr.RegisterDefaultCommands();
// 获取游戏分析配置
const build = $env.string("BUILD_VERSION") ?? "0.0.1";
print("Build version", build);

const ReplicatedStorage = game.GetService("ReplicatedStorage");
const RunService = game.GetService("RunService");
declare const script: { systems: Folder };
// 启动服务端 ECS 框架
const world = start(
	[script.systems, ReplicatedStorage.TS.ecs.systems],
	{} as ClientState,
)(setupTags);

// 注册命令执行前的权限检查钩子
Cmdr.Registry.RegisterHook("BeforeRun", (context: CommandContextWithWorld) => {
	const studio = RunService.IsStudio();

	if (studio) {
		context.world = world;
	} else {
		return "Admin only";
	}

	return void 0;
});

// 注册命令类型和命令
// Cmdr.RegisterTypesIn(ReplicatedStorage.TS.commands.types);
Cmdr.RegisterCommandsIn(ReplicatedStorage.TS.ecs.commands);
