import type { World, AnyEntity } from "@rbxts/matter";
import { KartReference, ItemHolder } from "shared/ecs/components/items";
import { Client } from "shared/ecs/components/base/client";
import { remotes } from "shared/remotes";
import { ItemEventBus } from "shared/ecs/bridge/event-bus";
import { RunService, Players } from "@rbxts/services";


/**
 * 卡丁车ECS桥接器 - 连接ECS系统和原有卡丁车系统
 */
export class KartECSBridge {
	private static instance: KartECSBridge;
	private kartToEntity: Map<number, AnyEntity> = new Map();
	private entityToKart: Map<AnyEntity, number> = new Map();
	private world!: World;

	static getInstance(): KartECSBridge {
		if (!this.instance) {
			this.instance = new KartECSBridge();
		}
		return this.instance;
	}

	/**
	 * 初始化桥接器（仅服务端）
	 */
	initialize(world: World): void {
		if (!RunService.IsServer()) {
			warn("[KartECSBridge] KartECSBridge should only be initialized on server");
			return;
		}
		
		if (this.world && this.world !== world) {
			warn("[KartECSBridge] Warning: Re-initializing with different world instance");
		}
		this.world = world;
		this.setupKartNotificationListener();
		
		// 延迟扫描现有卡丁车，给KartManager时间初始化
		task.wait(1);
		this.scanExistingKarts();
		

	}
	
	/**
	 * 确保已初始化（延迟初始化）
	 */
	private ensureInitialized(world: World): void {
		if (!this.world) {
			this.initialize(world);
		}
	}

	/**
	 * 设置卡丁车通知监听器（仅服务端）
	 */
	private setupKartNotificationListener(): void {
		if (!RunService.IsServer()) {
			return; // 客户端不需要初始化 KartECSBridge
		}
		
		// 服务端监听客户端的卡丁车创建通知
		remotes.kart.notifyKartCreated.connect((player, kartIndex, playerId) => {
			print(`[KartECSBridge] 收到客户端卡丁车创建通知，玩家: ${player.Name}, 索引: ${kartIndex}, ID: ${playerId}`);
			
			// 在服务端注册卡丁车（这里我们需要模拟 kartInstance，因为实际的卡丁车在客户端）
			const mockKartInstance = {
				playerId: playerId,
				playerName: player.Name,
				createdOnClient: true,
			};
			
			const entity = this.registerKart(kartIndex, mockKartInstance, player);
			print(`[KartECSBridge] 服务端实体创建完成，实体ID: ${entity}`);
			
			// 发送确认给客户端
			remotes.kart.confirmKartRegistered.fire(player, kartIndex, entity as number);
		});
		
		// 监听位置同步
		remotes.kart.syncKartPosition.connect((player, kartIndex, position) => {
			const entity = this.getEntityByKartIndex(kartIndex);
			if (entity && this.world.contains(entity)) {
				const kartRef = this.world.get(entity, KartReference);
				if (kartRef) {
					// 更新位置
					this.world.insert(entity, kartRef.patch({ position }));
				}
			}
		});
		
		// 监听道具拾取通知（来自客户端的漂移等）
		remotes.items.notifyItemPickup.connect((player, kartIndex, itemType) => {
			print(`[KartECSBridge] 收到道具拾取通知，玩家: ${player.Name}, 卡丁车索引: ${kartIndex}, 道具类型: ${itemType}`);
			// 推送到服务端事件总线
			ItemEventBus.push({
				type: "PICKUP",
				kartIndex: kartIndex,
				data: { itemType: itemType },
				timestamp: tick(),
			});
		});
	}

	/**
	 * 扫描已存在的卡丁车
	 */
	private scanExistingKarts(): void {
		// 服务端不需要扫描 KartManager，因为卡丁车通过远程事件同步
		// 这个方法保留用于向后兼容，但在当前架构下不执行任何操作
	}

	/**
	 * 注册卡丁车到ECS
	 */
	registerKart(kartIndex: number, kartInstance: unknown, player?: Player): AnyEntity {
		if (!this.world) {
			warn(`[KartECSBridge] Cannot register kart ${kartIndex}: world not initialized`);
			return -1 as AnyEntity;
		}
		
		// 检查是否已注册
		if (this.kartToEntity.has(kartIndex)) {
			return this.kartToEntity.get(kartIndex)!;
		}

		// 判断是否为玩家卡丁车（现在使用player参数或者kartIndex范围判断）
		// 玩家卡丁车索引现在是玩家的UserId，通常是较大的数字
		const isPlayer = player !== undefined || kartIndex > 6;

		// 基础组件
		const kartRef = KartReference({
			kartIndex,
			kartInstance,
			isPlayer,
		});
		
		const itemHolder = ItemHolder({
			items: [], // 空的道具数组，使用数组模拟栈
			maxSlots: 3,
			frozen: false,
		});

		// 创建ECS实体
		let entity: AnyEntity;
		
		// 如果是服务端且提供了 player 参数，添加 Client 组件
		if (RunService.IsServer() && player) {
			const clientComponent = Client({ player });
			entity = this.world.spawn(kartRef, itemHolder, clientComponent) as AnyEntity;
			print(`[KartECSBridge] 为玩家 ${player.Name} 的卡丁车添加 Client 组件`);
		} else {
			entity = this.world.spawn(kartRef, itemHolder) as AnyEntity;
		}

		// 双向映射
		this.kartToEntity.set(kartIndex, entity);
		this.entityToKart.set(entity, kartIndex);

		// 在Lua侧存储实体引用
		(kartInstance as unknown as { ecsEntity?: AnyEntity }).ecsEntity = entity;

		return entity;
	}

	/**
	 * 通过卡丁车索引获取ECS实体
	 */
	getEntityByKartIndex(kartIndex: number): AnyEntity | undefined {
		return this.kartToEntity.get(kartIndex);
	}

	/**
	 * 通过ECS实体获取卡丁车索引
	 */
	getKartIndexByEntity(entity: AnyEntity): number | undefined {
		return this.entityToKart.get(entity);
	}

	/**
	 * 获取世界实例
	 */
	getWorld(): World {
		return this.world;
	}
}