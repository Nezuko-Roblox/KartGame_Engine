import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";

const UserInputService = game.GetService("UserInputService");
const RunService = game.GetService("RunService");

/**
 * 初始化道具系统 - 设置全局函数和输入处理
 */
export function initItemSystem(): void {
	print("[ItemSystem] Starting initialization...");
	
	// 设置全局函数供Lua调用
	ItemEventBus.setupGlobalFunctions();
	
	// 设置输入监听（仅客户端）
	if (RunService.IsClient()) {
		setupInputHandling();
	}

	print("[ItemSystem] Basic initialization complete");
	print("[ItemSystem] Bridge will be initialized in item-activation-system");
}

/**
 * 设置输入处理
 */
function setupInputHandling(): void {
	// 监听键盘输入
	UserInputService.InputBegan.Connect((input, gameProcessed) => {
		// 如果输入被游戏UI处理了，则忽略
		if (gameProcessed) return;

		// Enter键触发使用道具
		if (input.KeyCode === Enum.KeyCode.Space) {
			// 调用全局函数，玩家总是索引1
			const globalG = _G as unknown as {
				ECS_UseItem?: (kartIndex: number) => void;
			};
			
			if (globalG.ECS_UseItem) {
				globalG.ECS_UseItem(1); // 玩家卡丁车索引为1
				print("[ItemSystem] Player pressed Enter to use item");
			}
		}

		// Q键切换道具槽位（可选）
		if (input.KeyCode === Enum.KeyCode.Q) {
			// TODO: 实现切换槽位逻辑
			print("[ItemSystem] Switch item slot (not implemented)");
		}
	});

	print("[ItemSystem] Input handling setup complete");
}

