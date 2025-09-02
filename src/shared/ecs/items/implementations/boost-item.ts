import type { World, AnyEntity } from "@rbxts/matter";
import { BoostBuff } from "shared/ecs/components/items/boost-buff";
import type { KartReference } from "shared/ecs/components/items/kart-reference";
import { RunService, Players } from "@rbxts/services";
import { remotes } from "shared/remotes";

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
	 * @param _owner 使用道具的实体
	 * @param kartRef 卡丁车引用组件
	 */
	static activate(world: World, _owner: AnyEntity, kartRef: KartReference): void {
		const now = tick();

		// 创建Buff实体
		world.spawn(
			BoostBuff({
				kartIndex: kartRef.kartIndex,
				duration: this.CONFIG.duration,
				startTime: now * 1000, // 转换为毫秒
				applied: false,
			}),
		);

		// 立即调用卡丁车的setBoost方法
		this.applyBoostToKart(kartRef);

		print(`[BoostItem] Kart ${kartRef.kartIndex} activated boost!`);
	}

	/**
	 * 应用加速效果到卡丁车
	 * @param kartRef 卡丁车引用
	 */
	private static applyBoostToKart(kartRef: KartReference): void {
		if (RunService.IsServer()) {
			// 服务端：通过远程事件通知客户端执行加速
			this.applyBoostOnServer(kartRef);
		} else {
			// 客户端：直接调用GoPlayKart的setBoost方法
			this.applyBoostOnClient(kartRef);
		}
	}

	/**
	 * 服务端处理加速效果（通过远程事件）
	 */
	private static applyBoostOnServer(kartRef: KartReference): void {
		// 获取加速参数
		const duration = this.CONFIG.duration;
		const boostType = 1; // BoostNormal

		// 如果是玩家卡丁车，直接发送给对应玩家
		if (kartRef.isPlayer) {
			const kartInstance = kartRef.kartInstance as unknown as { playerId?: number };
			const playerId = kartInstance?.playerId;
			if (playerId !== undefined) {
				const player = Players.GetPlayerByUserId(playerId);
				if (player) {
					// 通过远程事件通知客户端执行加速
					if (remotes.kart && remotes.kart.applyBoost) {
						remotes.kart.applyBoost.fire(player, kartRef.kartIndex, duration, boostType);
						print(`[BoostItem] 服务端通知客户端执行加速，卡丁车: ${kartRef.kartIndex}, 持续时间: ${duration}ms`);
					} else {
						warn("[BoostItem] 远程事件 remotes.kart.applyBoost 未找到");
					}
				} else {
					warn(`[BoostItem] 找不到玩家，ID: ${playerId}`);
				}
			} else {
				warn(`[BoostItem] 玩家ID未定义，卡丁车: ${kartRef.kartIndex}`);
			}
		} else {
			// 对于AI卡丁车，可以在这里添加处理逻辑
			print(`[BoostItem] AI卡丁车加速暂未实现，索引: ${kartRef.kartIndex}`);
		}
	}

	/**
	 * 客户端处理加速效果（直接调用GoPlayKart方法）
	 */
	private static applyBoostOnClient(kartRef: KartReference): void {
		// kartInstance 应该就是 GoPlayKart 实例
		const goPlayKart = kartRef.kartInstance as unknown as {
			setBoost?: (self: unknown, duration: number, boostType: number) => void;
			m_KartWLVel?: unknown;
		};

		if (!goPlayKart || !goPlayKart.setBoost) {
			warn(`[BoostItem] GoPlayKart实例或setBoost方法未找到，卡丁车: ${kartRef.kartIndex}`);
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
			print(`[BoostItem] 使用 BoostKind.BoostNormal = ${boostType}`);
		} else {
			warn("[BoostItem] BoostKind未找到，使用默认值1");
		}

		// 调用setBoost方法，使用静态配置
		const duration = BoostItem.CONFIG.duration;

		// 手动传递 self 参数（第一个参数）
		try {
			// 在 Lua 中，obj:method() 等价于 obj.method(obj, ...)
			// 所以我们需要手动传递 goPlayKart 作为第一个参数
			goPlayKart.setBoost(goPlayKart, duration, boostType);
			print(`[BoostItem] 客户端应用加速效果，卡丁车: ${kartRef.kartIndex}, 持续时间: ${duration}ms, 类型: ${boostType}`);
		} catch (e) {
			warn(`[BoostItem] 应用加速失败: ${e}`);
		}
	}
}