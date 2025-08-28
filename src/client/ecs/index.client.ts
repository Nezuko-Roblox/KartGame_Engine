// 设置命令行界面
import { CmdrClient } from "@rbxts/cmdr";
import type { AnyEntity } from "@rbxts/matter";

import type { ClientState } from "shared/ecs/constants/client-state";
import { start } from "shared/ecs/start";
import { setupTags } from "shared/ecs/utils/setup-tags";
import { initializeItemBoxes } from "shared/ecs/plugins/initialize-item-boxes";

import { ConfirmLoaded } from "./network";
import { receiveReplication } from "./receiveReplication";

CmdrClient.SetActivationKeys([Enum.KeyCode.F2]);
CmdrClient.SetEnabled(true);

const ReplicatedStorage = game.GetService("ReplicatedStorage");
const StarterGui = game.GetService("StarterGui");

// 禁用默认的核心 GUI
StarterGui.SetCoreGuiEnabled(Enum.CoreGuiType.Health, false);
StarterGui.SetCoreGuiEnabled(Enum.CoreGuiType.Backpack, false);

/** 异步预加载客户端资源. */
(async () => {
	print("Preloading start");
	ConfirmLoaded.fire();
	print("Preloading ended");
})();

// 初始化客户端状态
const state: ClientState = {
	debugEnabled: false,
	entityIdMap: new Map<string, AnyEntity>(),
};
declare const script: { systems: Folder };

// 启动客户端 ECS 框架，配置复制接收和标签设置
start([script.systems, ReplicatedStorage.TS.ecs.systems], state)(receiveReplication, setupTags, initializeItemBoxes);
