import type { World, AnyEntity } from "@rbxts/matter";
import type { KartReference } from "shared/ecs/components/items/kart-reference";

/**
 * 导弹道具实现类
 */
export class MissileItem {
	/**
	 * 激活导弹道具
	 */
	static activate(world: World, owner: AnyEntity, kartRef: KartReference): void {
		print(`[MissileItem] Kart ${kartRef.kartIndex} used Missile!`);
		
		// TODO: 实现导弹道具逻辑
		// 这里可以添加发射导弹的逻辑
	}
}