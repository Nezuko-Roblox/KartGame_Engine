import { Controller, OnStart } from "@flamework/core";
import { remotes } from "shared/remotes";
// 导入道具激活系统的函数
import itemActivationSystemModule = require("shared/ecs/systems/items/item-activation-system");

/**
 * 服务端道具控制器 - 处理客户端的道具相关请求
 */
@Controller()
export class ItemController implements OnStart {
	onStart(): void {
		print("[ItemController] ItemController 正在启动...");
		this.setupRemoteHandlers();
		print("[ItemController] ItemController 启动完成");
	}

	private setupRemoteHandlers(): void {
		print("[ItemController] 开始设置远程事件监听器...");
		
		// 检查远程事件是否存在
		if (!remotes.items.requestUseItem) {
			warn("[ItemController] requestUseItem 远程事件不存在！");
			return;
		}
		
		// 处理客户端的使用道具请求
		remotes.items.requestUseItem.connect((player, kartIndex) => {
			print(`[ItemController] 收到玩家 ${player.Name} 的使用道具请求，卡丁车索引: ${kartIndex}`);
			
			// 验证请求的合法性（可选）
			// 例如：检查玩家是否拥有该卡丁车，是否在游戏中等
			
			// 将请求添加到道具激活系统的队列中
			itemActivationSystemModule.addUseItemRequest(kartIndex);
		});

		print("[ItemController] 远程事件监听器设置完成");
	}
}