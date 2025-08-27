import type { World, AnyEntity } from "@rbxts/matter";
import { BoostBuff } from "shared/ecs/components/items/boost-buff";
import type { KartReference } from "shared/ecs/components/items/kart-reference";

/**
 * 加速道具实现类
 */
export class BoostItem {
	/** 配置常量 */
	static readonly CONFIG = {
		/** 加速持续时间（毫秒） */
		duration: 2000,
	};

	/**
	 * 激活加速道具
	 * @param world ECS世界
	 * @param owner 使用道具的实体
	 * @param kartRef 卡丁车引用组件
	 */
	static activate(world: World, owner: AnyEntity, kartRef: KartReference): void {
		const now = tick();
		
		// 创建Buff实体
		const buffEntity = world.spawn(
			BoostBuff({
				kartIndex: kartRef.kartIndex,
				duration: this.CONFIG.duration,
				startTime: now * 1000, // 转换为毫秒
				applied: false,
			}),
		) as AnyEntity;

		// 立即调用卡丁车的setBoost方法
		this.applyBoostToKart(kartRef);

		print(`[BoostItem] Kart ${kartRef.kartIndex} activated boost!`);
	}

	/**
	 * 应用加速效果到卡丁车
	 * @param kartRef 卡丁车引用
	 */
	private static applyBoostToKart(kartRef: KartReference): void {
		// kartInstance 应该就是 GoPlayKart 实例
		const goPlayKart = kartRef.kartInstance as unknown as {
			setBoost?: (self: unknown, duration: number, boostType: number) => void;
			m_KartWLVel?: unknown;
		};
		
		if (!goPlayKart || !goPlayKart.setBoost) {
			warn(`[BoostItem] GoPlayKart instance or setBoost method not found for kart ${kartRef.kartIndex}`);
			return;
		}
		
		// 获取 BoostKind 枚举
		const globalG = _G as unknown as {
			BoostKind?: {
				BoostNormal: number;
			};
		};
		
		const BoostKind = globalG.BoostKind;
		let boostType = 1; // 默认值为 BoostNormal = 1
		
		if (BoostKind && BoostKind.BoostNormal !== undefined) {
			boostType = BoostKind.BoostNormal;
			print(`[BoostItem] Using BoostKind.BoostNormal = ${boostType}`);
		} else {
			warn("[BoostItem] BoostKind not found, using default value 1");
		}
		
		// 调用setBoost方法，使用静态配置
		const duration = BoostItem.CONFIG.duration;
		
		// 手动传递 self 参数（第一个参数）
		try {
			// 在 Lua 中，obj:method() 等价于 obj.method(obj, ...)
			// 所以我们需要手动传递 goPlayKart 作为第一个参数
			goPlayKart.setBoost(goPlayKart, duration, boostType);
			print(`[BoostItem] Applied boost to kart ${kartRef.kartIndex}: duration=${duration}, type=${boostType}`);
		} catch (e) {
			warn(`[BoostItem] Failed to apply boost: ${e}`);
		}
	}
}