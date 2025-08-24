import type { CommandContextWithWorld } from "@rbxts/cmdr";

import { Stage } from "../components";

/**
 * 服务器端 spawn-enemy 命令实现 (与 shared 命令定义同名).
 *
 * @param context - 命令上下文(包含 world 注入).
 * @param stageId - 关卡 ID.
 * @returns 结果字符串.
 */
export = function (context: CommandContextWithWorld, stageId: string) {
	const { world } = context;
	if (!world) {
		return "World not available (permission denied or not initialized)";
	}

	world.spawn(Stage({ isPaused: false, stageId }));

	return `Spawn request queued: ${stageId}`;
};
