import type { World, AnyEntity } from "@rbxts/matter";
import { KartReference, ItemHolder } from "shared/ecs/components/items";
import { Stack } from "shared/util/stack";

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
	 * 初始化桥接器
	 */
	initialize(world: World): void {
		if (this.world && this.world !== world) {
			warn("[KartECSBridge] Warning: Re-initializing with different world instance");
		}
		this.world = world;
		this.setupKartNotificationListener();
		// this.scanExistingKarts();
		// print("[KartECSBridge] Initialized with notification listener");
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
	 * 设置卡丁车通知监听器
	 */
	private setupKartNotificationListener(): void {
		// 设置全局函数，当卡丁车创建时会被调用
		const globalG = _G as unknown as {
			ECS_OnKartCreated?: (kartIndex: number, kartInstance: unknown) => void;
		};
		
		globalG.ECS_OnKartCreated = (kartIndex: number, kartInstance: unknown) => {
			// print(`[KartECSBridge] Received kart creation notification: kartIndex=${kartIndex}`);
			this.registerKart(kartIndex, kartInstance);
		};
		
		// print("[KartECSBridge] Notification listener setup complete");
	}

	/**
	 * 扫描已存在的卡丁车
	 */
	private scanExistingKarts(): void {
		// 检查 KartManager 是否已存在卡丁车
		const globalG = _G as unknown as {
			KartManager?: {
				Instance?: {
					goKart_?: Record<number, unknown>;
					goKartCount_?: number;
				};
			};
		};
		if (!globalG.KartManager?.Instance) {
			return;
		}
		const kartManager = globalG.KartManager.Instance;
		if (!kartManager.goKart_) {
			return;
		}
		// 扫描所有可能的卡丁车索引
		for (const [index, kart] of pairs(kartManager.goKart_)) {
			if (kart && !this.kartToEntity.has(index as number)) {
				print(`[KartECSBridge] Found existing kart at index ${index}`);
				this.registerKart(index as number, kart);
			}
		}
	}

	/**
	 * 注册卡丁车到ECS
	 */
	registerKart(kartIndex: number, kartInstance: unknown): AnyEntity {
		if (!this.world) {
			warn(`[KartECSBridge] Cannot register kart ${kartIndex}: world not initialized`);
			return -1 as AnyEntity;
		}
		
		// 检查是否已注册
		if (this.kartToEntity.has(kartIndex)) {
			print(`[KartECSBridge] Kart ${kartIndex} already registered`);
			return this.kartToEntity.get(kartIndex)!;
		}

		// 玩家卡丁车索引通常是1
		const isPlayer = kartIndex === 1;
		
		print(`[KartECSBridge] Creating entity for kart ${kartIndex} in world ${tostring(this.world)}`);

		// 创建ECS实体
		const entity = this.world.spawn(
			KartReference({
				kartIndex,
				kartInstance,
				isPlayer,
			}),
			ItemHolder({
				itemStack: new Stack<string>(3), // 3个道具槽位的栈
				maxSlots: 3,
				frozen: false,
			}),
		) as AnyEntity;
		
		print(`[KartECSBridge] Entity created: ${entity}, verifying components...`);
		
		// 验证组件是否成功添加
		const hasKartRef = this.world.get(entity, KartReference) !== undefined;
		const hasItemHolder = this.world.get(entity, ItemHolder) !== undefined;
		print(`[KartECSBridge] Component check - KartReference: ${hasKartRef}, ItemHolder: ${hasItemHolder}`);

		// 双向映射
		this.kartToEntity.set(kartIndex, entity);
		this.entityToKart.set(entity, kartIndex);

		// 在Lua侧存储实体引用
		(kartInstance as unknown as { ecsEntity?: AnyEntity }).ecsEntity = entity;

		print(`[KartECSBridge] Registered kart ${kartIndex} with ECS entity ${entity}, isPlayer: ${isPlayer}`);
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