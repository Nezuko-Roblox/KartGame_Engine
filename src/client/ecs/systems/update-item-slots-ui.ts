import type { World } from "@rbxts/matter";
import { ItemHolder, KartReference } from "shared/ecs/components/items";
import React from "@rbxts/react";
import ReactRoblox from "@rbxts/react-roblox";
import { Players } from "@rbxts/services";
import { ItemSlotsDisplay } from "client/ui/components/item-slots-display";

const LocalPlayer = Players.LocalPlayer;
let root: ReactRoblox.Root | undefined;
let currentGui: ScreenGui | undefined;

/**
 * 道具槽位UI更新系统 - 实时显示玩家的道具栈
 */
function updateItemSlotsUI(world: World): void {
	if (!LocalPlayer) return;

	// 计数实体
	let entityCount = 0;

	// 查找本地玩家的卡丁车实体
	for (const [entity, kartRef, holder] of world.query(KartReference, ItemHolder)) {
		entityCount++;

		if (!kartRef.isPlayer) continue;

		// 确保UI根存在
		if (!root || !currentGui || !currentGui.Parent) {
			// 清理旧的GUI
			if (currentGui) {
				currentGui.Destroy();
			}

			// 创建新的ScreenGui
			currentGui = new Instance("ScreenGui");
			currentGui.Name = "ItemSlotsUI";
			currentGui.ResetOnSpawn = false;
			currentGui.ZIndexBehavior = Enum.ZIndexBehavior.Sibling;
			currentGui.DisplayOrder = 10; // 确保显示在前面
			
			// 确保 PlayerGui 存在
			const playerGui = LocalPlayer.FindFirstChild("PlayerGui") || LocalPlayer.WaitForChild("PlayerGui");
			currentGui.Parent = playerGui;

			// 创建React根
			root = ReactRoblox.createRoot(currentGui);
		}

		// 从栈中获取道具数组
		const stackItems = holder.itemStack.toArray();
		
		// 创建显示数组
		const displayItems: (string | undefined)[] = [];
		for (let i = 0; i < holder.maxSlots; i++) {
			displayItems[i] = stackItems[i];
		}

		// 更新UI
		root.render(
			React.createElement(ItemSlotsDisplay, {
				items: displayItems,
				currentSlot: 0, // 栈总是从顶部（索引0）使用道具
				maxSlots: holder.maxSlots,
			})
		);

		// 只处理第一个玩家实体
		break;
	}

	// 如果没找到任何实体，创建一个默认UI显示
	if (entityCount === 0 && !currentGui) {
		// 创建新的ScreenGui
		currentGui = new Instance("ScreenGui");
		currentGui.Name = "ItemSlotsUI";
		currentGui.ResetOnSpawn = false;
		currentGui.ZIndexBehavior = Enum.ZIndexBehavior.Sibling;
		currentGui.DisplayOrder = 10;
		
		const playerGui = LocalPlayer.FindFirstChild("PlayerGui") || LocalPlayer.WaitForChild("PlayerGui");
		currentGui.Parent = playerGui;

		// 创建React根
		root = ReactRoblox.createRoot(currentGui);
		
		// 渲染空的道具栏
		root.render(
			React.createElement(ItemSlotsDisplay, {
				items: [],
				currentSlot: 0,
				maxSlots: 3,
			})
		);
	}
}

/**
 * 清理UI系统
 */
function cleanupItemSlotsUI(): void {
	if (root) {
		root.unmount();
		root = undefined;
	}
	if (currentGui) {
		currentGui.Destroy();
		currentGui = undefined;
	}
}

export = {
	system: updateItemSlotsUI,
	priority: 100, // 高优先级，确保UI及时更新
	cleanup: cleanupItemSlotsUI,
};