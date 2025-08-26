import type { World } from "@rbxts/matter";
import { ItemHolder, KartReference } from "shared/ecs/components/items";

let hasGivenItem = false;

/**
 * 测试系统 - 自动给玩家添加道具
 */
function testItemGiverSystem(world: World): void {
	// 只执行一次
	if (hasGivenItem) return;
	
	// 查找玩家实体
	for (const [entity, kartRef, holder] of world.query(KartReference, ItemHolder)) {
		if (kartRef.isPlayer) {
			// 给玩家添加一个加速道具
			const newHolder = holder.patch({
				items: ["Boost", undefined, undefined],
			});
			
			world.insert(entity, newHolder);
			
			print(`[TestItemGiver] Gave Boost item to player (entity ${entity})`);
			hasGivenItem = true;
			break;
		}
	}
}

export = testItemGiverSystem;