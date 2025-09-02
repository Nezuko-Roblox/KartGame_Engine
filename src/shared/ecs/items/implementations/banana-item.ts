import type { World, AnyEntity } from "@rbxts/matter";
import type { KartReference } from "shared/ecs/components/items/kart-reference";

/**
 * 香蕉道具实现类
 */
export class BananaItem {
	/**
	 * 激活香蕉道具
	 */
	static activate(world: World, owner: AnyEntity, kartRef: KartReference): void {
		print(`[BananaItem] Kart ${kartRef.kartIndex} used Banana!`);
		
		// TODO: 实现香蕉道具逻辑
		// 这里可以添加放置香蕉皮的逻辑
	}
}