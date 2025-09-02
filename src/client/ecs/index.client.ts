// 设置命令行界面
import { CmdrClient } from "@rbxts/cmdr";
import type { AnyEntity, World } from "@rbxts/matter";

import type { ClientState } from "shared/ecs/constants/client-state";
import { start } from "shared/ecs/start";
import { setupTags } from "shared/ecs/utils/setup-tags";
import { remotes } from "shared/remotes";
import { Players } from "@rbxts/services";

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
const world = start([script.systems, ReplicatedStorage.TS.ecs.systems], state)(receiveReplication, setupTags);

// 保存 world 引用供全局使用
const globalWorld = _G as unknown as { __clientWorld?: World };
globalWorld.__clientWorld = world;

// 设置客户端卡丁车创建通知
const globalG = _G as unknown as {
	ECS_OnKartCreated?: (kartIndex: number, kartInstance: unknown) => void;
};

// 导入必要的组件和工具
import { KartReference, ItemHolder } from "shared/ecs/components/items";

// 客户端卡丁车映射
const clientKartToEntity = new Map<number, AnyEntity>();

globalG.ECS_OnKartCreated = (kartIndex: number, kartInstance: unknown) => {
	print(`[ECS_OnKartCreated] 卡丁车创建回调被调用，索引: ${kartIndex}`);

	// 通知服务端
	const player = Players.LocalPlayer;
	if (player) {
		print(`[ECS_OnKartCreated] 通知服务端卡丁车创建，玩家ID: ${player.UserId}`);
		remotes.kart.notifyKartCreated.fire(kartIndex, player.UserId);
	}

	// 在客户端也创建实体（用于道具系统）
	const globalWorldRef = _G as unknown as { __clientWorld?: World };
	const clientWorld = globalWorldRef.__clientWorld;
	if (clientWorld) {
		// 判断是否为玩家卡丁车（现在卡丁车索引是玩家UserId，通常是较大的数字）
		const isPlayer = player ? kartIndex === player.UserId : false;

		// 客户端只创建KartReference，ItemHolder由服务端管理并复制过来
		const entity = clientWorld.spawn(
			KartReference({
				kartIndex,
				kartInstance,
				isPlayer,
			}),
		) as AnyEntity;

		clientKartToEntity.set(kartIndex, entity);
		print(`[ECS_OnKartCreated] 客户端实体创建完成，实体ID: ${entity}`);

		// 设置全局访问函数供道具系统使用
		const globalG2 = _G as unknown as {
			getClientKartEntity?: (kartIndex: number) => AnyEntity | undefined;
		};
		globalG2.getClientKartEntity = (kartIndex: number) => clientKartToEntity.get(kartIndex);
	}
};

// 监听服务端的加速指令
remotes.kart.applyBoost.connect((kartIndex: number, duration: number, boostType: number) => {
	print(`[Client] 收到加速指令，卡丁车: ${kartIndex}, 持续时间: ${duration}ms, 类型: ${boostType}`);
	
	const entity = clientKartToEntity.get(kartIndex);
	if (!entity) {
		warn(`[Client] 找不到卡丁车实体，索引: ${kartIndex}`);
		return;
	}
	
	const clientWorld = globalWorld.__clientWorld;
	if (!clientWorld || !clientWorld.contains(entity)) {
		warn(`[Client] 卡丁车实体不存在于世界中，索引: ${kartIndex}`);
		return;
	}
	
	const kartRef = clientWorld.get(entity, KartReference);
	if (!kartRef) {
		warn(`[Client] 找不到卡丁车引用组件，索引: ${kartIndex}`);
		return;
	}
	
	// 直接调用GoPlayKart的setBoost方法
	const goPlayKart = kartRef.kartInstance as unknown as {
		setBoost?: (self: unknown, duration: number, boostType: number) => void;
	};
	
	if (!goPlayKart || !goPlayKart.setBoost) {
		warn(`[Client] GoPlayKart实例或setBoost方法未找到，卡丁车: ${kartIndex}`);
		return;
	}
	
	try {
		goPlayKart.setBoost(goPlayKart, duration, boostType);
		print(`[Client] 成功应用加速效果，卡丁车: ${kartIndex}`);
	} catch (e) {
		warn(`[Client] 应用加速失败: ${e}`);
	}
});
