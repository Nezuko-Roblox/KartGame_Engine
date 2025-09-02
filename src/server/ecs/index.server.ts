import type { CommandContextWithWorld } from "@rbxts/cmdr";
import { Cmdr } from "@rbxts/cmdr";

import { $env } from "rbxts-transform-env";
import type { ClientState } from "shared/ecs/constants/client-state";
import { start } from "shared/ecs/start";
import { setupTags } from "shared/ecs/utils/setup-tags";
import { initializeItemBoxes } from "shared/ecs/plugins/initialize-item-boxes";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";

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
)(setupTags, initializeItemBoxes);

// 初始化卡丁车ECS桥接系统
const kartBridge = KartECSBridge.getInstance();
kartBridge.initialize(world);
print("[Server] KartECSBridge initialized");

// 设置道具使用远程事件监听器
import { remotes } from "shared/remotes";
import itemActivationSystemModule = require("shared/ecs/systems/items/item-activation-system");

print("[Server] 设置道具使用远程事件监听器...");
remotes.items.requestUseItem.connect((player, kartIndex) => {
	print(`[Server] 收到玩家 ${player.Name} 的使用道具请求，卡丁车索引: ${kartIndex}`);
	itemActivationSystemModule.addUseItemRequest(kartIndex);
});
print("[Server] 道具使用远程事件监听器设置完成");

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
