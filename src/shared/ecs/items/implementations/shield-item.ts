import type { World, AnyEntity } from "@rbxts/matter";
import type { KartReference } from "shared/ecs/components/items/kart-reference";

/**
 * 护盾道具实现类
 */
export class ShieldItem {
	/**
	 * 激活护盾道具
	 */
	static activate(world: World, owner: AnyEntity, kartRef: KartReference): void {
		print(`[ShieldItem] Kart ${kartRef.kartIndex} used Shield!`);
		
		// TODO: 实现护盾道具逻辑
		// 这里可以添加护盾保护的逻辑
	}
}