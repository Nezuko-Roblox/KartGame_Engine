import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { remotes } from "shared/remotes";

// 导出道具实现
export * from "./implementations/boost-item";
export * from "./implementations/banana-item";
export * from "./implementations/shield-item";
export * from "./implementations/missile-item";

const UserInputService = game.GetService("UserInputService");
const RunService = game.GetService("RunService");

/**
 * 初始化道具系统 - 设置全局函数和输入处理
 */
export function initItemSystem(): void {
	// 设置全局函数供Lua调用
	ItemEventBus.setupGlobalFunctions();

	// 设置输入监听（仅客户端）
	if (RunService.IsClient()) {
		setupInputHandling();
	}
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
			print("[ItemSystem] 空格键被按下，发送使用道具请求到服务端");

			// 检查远程事件是否存在
			if (!remotes.items.requestUseItem) {
				warn("[ItemSystem] requestUseItem 远程事件不存在！");
				return;
			}

			try {
				// 使用玩家的 UserId 作为卡丁车索引，确保每个玩家有唯一的索引
				const Players = game.GetService("Players");
				const LocalPlayer = Players.LocalPlayer;
				const kartIndex = LocalPlayer.UserId; // 使用 UserId 作为唯一标识
				
				// 发送远程事件到服务端
				remotes.items.requestUseItem.fire(kartIndex);
				print(`[ItemSystem] 使用道具请求已发送，卡丁车索引: ${kartIndex}`);
			} catch (error) {
				warn(`[ItemSystem] 发送远程事件失败: ${error}`);
			}
		}

		// Q键切换道具槽位（可选）
		if (input.KeyCode === Enum.KeyCode.Q) {
			// TODO: 实现切换槽位逻辑
		}
	});


}

