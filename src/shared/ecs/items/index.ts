import { start } from "shared/ecs/start";
import { KartECSBridge } from "shared/ecs/bridge/kart-ecs-bridge";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { ItemHolder } from "shared/ecs/components/items";

const UserInputService = game.GetService("UserInputService");
const RunService = game.GetService("RunService");

/**
 * 初始化道具系统
 */
export function initItemSystem(): void {
	print("[ItemSystem] Starting initialization...");
	
	// 调试：检查全局变量
	debugGlobalVariables();

	// 设置全局函数供Lua调用
	ItemEventBus.setupGlobalFunctions();
	
	// 注意：KartECSBridge 将在第一个系统运行时初始化
	// 这样可以确保使用正确的 World 实例

	// 设置输入监听（仅客户端）
	if (RunService.IsClient()) {
		setupInputHandling();
	}

	// 注意：测试代码已移除
	// 道具应该通过游戏逻辑添加，而不是在初始化时硬编码

	print("[ItemSystem] Initialization complete");
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

/**
 * 调试全局变量
 */
function debugGlobalVariables(): void {
	print("[ItemSystem] Debugging global variables...");
	
	const globalG = _G as unknown as Record<string, unknown>;
	
	print("[ItemSystem] Available global variables:");
	for (const [key, value] of pairs(globalG)) {
		if (typeOf(value) === "table" && typeOf(key) === "string" && (key as string).find("Kart")) {
			print(`  - ${key}: ${typeOf(value)}`);
		}
	}
	
	// 检查 KartManager
	if (globalG.KartManager) {
		print(`[ItemSystem] Found KartManager: ${typeOf(globalG.KartManager)}`);
		const kartManager = globalG.KartManager as unknown as { Instance?: unknown };
		if (kartManager.Instance) {
			print(`[ItemSystem] Found KartManager.Instance: ${typeOf(kartManager.Instance)}`);
		}
	} else {
		print("[ItemSystem] KartManager not found in global variables");
	}
	
	// 检查 BoostKind
	if (globalG.BoostKind) {
		print(`[ItemSystem] Found BoostKind: ${typeOf(globalG.BoostKind)}`);
		const boostKind = globalG.BoostKind as unknown as { BoostNormal?: number };
		if (boostKind.BoostNormal !== undefined) {
			print(`[ItemSystem] BoostKind.BoostNormal = ${boostKind.BoostNormal}`);
		}
	} else {
		print("[ItemSystem] BoostKind not found in global variables");
	}
}