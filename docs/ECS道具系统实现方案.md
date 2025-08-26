# 赛车游戏ECS道具系统详细实现方案

## 一、道具类型定义（基于Unity源码）

根据Unity源码分析，游戏包含以下道具类型：

### 1. 核心道具枚举
```typescript
export enum ItemType {
    NONE = -1,
    BOOSTER = 0,      // 加速器
    BANANA = 1,       // 香蕉皮（陷阱）
    UFO = 2,          // UFO（特殊攻击）
    WATER_FLY = 3,    // 水炸弹（飞行）
    WATER_BOMB = 4,   // 水炸弹（爆炸）
    FLIP = 5,         // 翻转（控制干扰）
    DEVIL = 6,        // 恶魔（debuff）
    WATER_MISSILE = 7,// 水导弹（追踪）
    GUARD = 8,        // 防护罩
    SHIELD = 9,       // 盾牌
}
```

## 二、组件架构设计

### 2.1 基础组件

#### Transform组件
```typescript
// src/shared/ecs/components/items/transform.ts
export interface ItemTransform {
    position: Vector3;
    rotation: CFrame;
    velocity?: Vector3;
    scale?: Vector3;
}
```

#### ItemHolder组件（道具持有者）
```typescript
// src/shared/ecs/components/items/item-holder.ts
export interface ItemHolder {
    items: Array<{
        type: ItemType;
        count: number;
        slot: number;
    }>;
    maxSlots: number;
    currentSlot: number;
    frozen: boolean;
    lastUsedTime: number;
}
```

#### ItemBox组件（道具箱）
```typescript
// src/shared/ecs/components/items/item-box.ts
export interface ItemBox {
    type: "Random" | "Specific" | "Weighted";
    specificItem?: ItemType;
    weightedItems?: Array<{item: ItemType; weight: number}>;
    active: boolean;
    respawnTime: number;
    lastPickupTime: number;
    rotationSpeed: number;
}
```

### 2.2 道具实体组件

#### ItemActive组件（活跃道具）
```typescript
// src/shared/ecs/components/items/item-active.ts
export interface ItemActive {
    itemType: ItemType;
    ownerId: EntityId;
    activationTime: number;
    duration: number;
    state: "idle" | "active" | "expired";
}
```

#### ItemProjectile组件（投射物）
```typescript
// src/shared/ecs/components/items/item-projectile.ts
export interface ItemProjectile {
    speed: number;
    acceleration: number;
    maxSpeed: number;
    target?: EntityId;
    trackingStrength: number;
    damage: number;
    lifetime: number;
    spawnTime: number;
    trajectory: "Straight" | "Arc" | "Homing";
    defendable: boolean;
}
```

#### ItemTrap组件（陷阱）
```typescript
// src/shared/ecs/components/items/item-trap.ts
export interface ItemTrap {
    triggerRadius: number;
    triggerDelay: number;
    slipDuration: number;
    slipIntensity: number;
    armed: boolean;
    ignoreOwner: boolean;
    ignoreDuration: number;
    lifetime: number;
    maxTriggers: number;
    triggeredCount: number;
}
```

#### ItemBuff组件（增益效果）
```typescript
// src/shared/ecs/components/items/item-buff.ts
export interface ItemBuff {
    buffType: "Speed" | "Shield" | "Invincible";
    modifiers: Map<string, {
        type: "Additive" | "Multiplicative";
        value: number;
        duration: number;
    }>;
    stackable: boolean;
    maxStacks: number;
    currentStacks: number;
    activationTime: number;
    expirationTime: number;
}
```

## 三、系统实现

### 3.1 道具拾取系统
```typescript
// src/shared/ecs/systems/items/item-pickup-system.ts
import { World, System } from "@rbxts/matter";

export const ItemPickupSystem: System = {
    priority: 100,
    
    update(world: World, state: SharedState, deltaTime: number) {
        // 处理道具箱碰撞检测
        for (const [id, itemBox, transform, collider] of world.query(ItemBox, Transform, Collider)) {
            if (!itemBox.active) {
                // 检查重生
                if (state.time - itemBox.lastPickupTime > itemBox.respawnTime) {
                    world.insert(id, itemBox.patch({ active: true }));
                }
                continue;
            }
            
            // 旋转动画
            const rotation = transform.rotation * CFrame.Angles(0, itemBox.rotationSpeed * deltaTime, 0);
            world.insert(id, transform.patch({ rotation }));
            
            // 碰撞检测
            for (const [kartId, kartTransform, itemHolder] of world.query(KartTransform, ItemHolder)) {
                const distance = (kartTransform.position - transform.position).Magnitude;
                
                if (distance < collider.radius) {
                    // 检查是否有空槽位
                    const emptySlot = this.findEmptySlot(itemHolder);
                    if (emptySlot !== -1) {
                        // 生成道具
                        const itemType = this.generateItem(itemBox, kartId, state);
                        
                        // 添加到持有者
                        const newItems = [...itemHolder.items];
                        newItems[emptySlot] = {
                            type: itemType,
                            count: 1,
                            slot: emptySlot
                        };
                        
                        world.insert(kartId, itemHolder.patch({ items: newItems }));
                        
                        // 禁用道具箱
                        world.insert(id, itemBox.patch({
                            active: false,
                            lastPickupTime: state.time
                        }));
                        
                        // 播放音效
                        this.playPickupSound(world, transform.position);
                        
                        // 网络同步
                        if (state.isServer) {
                            this.syncPickup(kartId, itemType);
                        }
                    }
                }
            }
        }
    },
    
    findEmptySlot(holder: ItemHolder): number {
        for (let i = 0; i < holder.maxSlots; i++) {
            if (!holder.items[i]) {
                return i;
            }
        }
        return -1;
    },
    
    generateItem(box: ItemBox, kartId: EntityId, state: SharedState): ItemType {
        // 根据排名和游戏状态生成道具
        const kartInfo = world.get(kartId, KartInfo);
        const rank = kartInfo?.currentRank || 1;
        
        // 基于排名的权重系统
        const weights = this.getItemWeights(rank, state.raceProgress);
        return this.weightedRandom(weights);
    }
};
```

### 3.2 道具使用系统
```typescript
// src/shared/ecs/systems/items/item-activation-system.ts
export const ItemActivationSystem: System = {
    priority: 90,
    
    update(world: World, state: SharedState, deltaTime: number) {
        // 处理道具使用输入
        for (const [kartId, itemHolder, input] of world.query(ItemHolder, PlayerInput)) {
            if (input.useItem && !itemHolder.frozen) {
                const currentItem = itemHolder.items[itemHolder.currentSlot];
                
                if (currentItem && state.time - itemHolder.lastUsedTime > 0.5) {
                    this.activateItem(world, kartId, currentItem, state);
                    
                    // 更新持有者状态
                    world.insert(kartId, itemHolder.patch({
                        lastUsedTime: state.time
                    }));
                }
            }
        }
        
        // AI道具使用逻辑
        for (const [kartId, itemHolder, aiController] of world.query(ItemHolder, AIController)) {
            if (itemHolder.items.length > 0 && aiController.autoUseItems) {
                this.aiItemDecision(world, kartId, itemHolder, aiController, state);
            }
        }
    },
    
    activateItem(world: World, kartId: EntityId, item: ItemData, state: SharedState) {
        const config = ItemConfigs[item.type];
        const kartTransform = world.get(kartId, Transform);
        
        switch (config.category) {
            case "Projectile":
                this.spawnProjectile(world, kartId, item.type, kartTransform, state);
                break;
                
            case "Trap":
                this.placeTrap(world, kartId, item.type, kartTransform, state);
                break;
                
            case "Buff":
                this.applyBuff(world, kartId, item.type, state);
                break;
                
            case "Instant":
                this.applyInstant(world, kartId, item.type, state);
                break;
        }
        
        // 消耗道具
        this.consumeItem(world, kartId, item.slot);
    }
};
```

### 3.3 投射物系统
```typescript
// src/shared/ecs/systems/items/item-projectile-system.ts
export const ItemProjectileSystem: System = {
    priority: 85,
    
    update(world: World, state: SharedState, deltaTime: number) {
        for (const [id, transform, projectile, velocity] of world.query(Transform, ItemProjectile, Velocity)) {
            // 生命周期检查
            const age = state.time - projectile.spawnTime;
            if (age > projectile.lifetime) {
                world.despawn(id);
                continue;
            }
            
            // 目标追踪
            if (projectile.target && projectile.trackingStrength > 0) {
                const targetTransform = world.get(projectile.target, Transform);
                if (targetTransform) {
                    const toTarget = targetTransform.position.sub(transform.position);
                    const targetDir = toTarget.Unit;
                    const currentDir = velocity.Unit;
                    
                    // 插值转向
                    const newDir = currentDir.Lerp(targetDir, projectile.trackingStrength * deltaTime);
                    const newVelocity = newDir.mul(projectile.speed);
                    
                    world.insert(id, velocity.patch({ value: newVelocity }));
                }
            }
            
            // 碰撞检测
            const hit = this.checkCollision(world, id, transform, projectile);
            if (hit) {
                this.handleImpact(world, id, projectile, hit, state);
                world.despawn(id);
                continue;
            }
            
            // 更新位置
            const newPosition = transform.position.add(velocity.value.mul(deltaTime));
            world.insert(id, transform.patch({ position: newPosition }));
        }
    }
};
```

## 四、具体道具实现

### 4.1 香蕉皮（Banana）
```typescript
// src/shared/ecs/items/implementations/banana.ts
export const BananaItem: ItemImplementation = {
    type: ItemType.BANANA,
    category: "Trap",
    
    config: {
        modelId: "BananaModel",
        spawnOffset: new Vector3(0, 0, -3),
        triggerRadius: 2,
        triggerDelay: 0.5,
        slipDuration: 2,
        slipIntensity: 0.8,
        lifetime: 30,
        ignoreOwnerDuration: 2
    },
    
    onActivate(world: World, ownerId: EntityId, state: SharedState): EntityId {
        const ownerTransform = world.get(ownerId, Transform);
        
        // 在车后方生成香蕉
        const spawnPosition = ownerTransform.position.sub(
            ownerTransform.rotation.LookVector.mul(3)
        );
        
        // 射线检测地面
        const groundPosition = this.findGroundPosition(spawnPosition);
        
        // 创建香蕉实体
        const bananaId = world.spawn(
            Transform({ position: groundPosition }),
            ItemTrap({
                triggerRadius: this.config.triggerRadius,
                slipDuration: this.config.slipDuration,
                slipIntensity: this.config.slipIntensity,
                armed: false,
                ignoreOwner: true,
                ignoreDuration: this.config.ignoreOwnerDuration,
                lifetime: this.config.lifetime
            }),
            Model({ assetId: this.config.modelId }),
            Collider({ shape: "Sphere", radius: this.config.triggerRadius })
        );
        
        // 延迟激活
        task.wait(this.config.triggerDelay);
        const trap = world.get(bananaId, ItemTrap);
        if (trap) {
            world.insert(bananaId, trap.patch({ armed: true }));
        }
        
        return bananaId;
    },
    
    onTrigger(world: World, trapId: EntityId, targetId: EntityId, state: SharedState) {
        const trap = world.get(trapId, ItemTrap);
        const targetKart = world.get(targetId, KartController);
        
        if (trap && targetKart && trap.armed) {
            // 应用打滑效果
            world.insert(targetId, SlipEffect({
                duration: trap.slipDuration,
                intensity: trap.slipIntensity,
                startTime: state.time
            }));
            
            // 播放效果
            this.playSlipEffect(world, targetId);
            
            // 销毁香蕉
            world.despawn(trapId);
        }
    }
};
```

### 4.2 水导弹（Water Missile）
```typescript
// src/shared/ecs/items/implementations/water-missile.ts
export const WaterMissileItem: ItemImplementation = {
    type: ItemType.WATER_MISSILE,
    category: "Projectile",
    
    config: {
        modelId: "MissileModel",
        projectileSpeed: 80,
        trackingStrength: 0.8,
        damage: 20,
        lifetime: 10,
        explosionRadius: 5,
        defendable: true
    },
    
    onActivate(world: World, ownerId: EntityId, state: SharedState): EntityId {
        const ownerTransform = world.get(ownerId, Transform);
        
        // 查找目标
        const target = this.findBestTarget(world, ownerId, state);
        
        // 创建导弹实体
        const missileId = world.spawn(
            Transform({
                position: ownerTransform.position.add(new Vector3(0, 1, 0)),
                rotation: ownerTransform.rotation
            }),
            ItemProjectile({
                speed: this.config.projectileSpeed,
                target: target,
                trackingStrength: this.config.trackingStrength,
                damage: this.config.damage,
                lifetime: this.config.lifetime,
                defendable: this.config.defendable
            }),
            Velocity({
                value: ownerTransform.rotation.LookVector.mul(this.config.projectileSpeed)
            }),
            Model({ assetId: this.config.modelId }),
            Collider({ shape: "Sphere", radius: 1 })
        );
        
        // 添加特效
        this.createMissileTrail(world, missileId);
        
        return missileId;
    },
    
    onImpact(world: World, projectileId: EntityId, targetId: EntityId, state: SharedState) {
        const projectile = world.get(projectileId, ItemProjectile);
        
        // 检查防御
        const shield = world.get(targetId, ShieldBuff);
        if (shield && shield.active) {
            this.deflectProjectile(world, projectileId);
            return;
        }
        
        // 应用伤害和击退
        const targetKart = world.get(targetId, KartController);
        if (targetKart) {
            world.insert(targetId, DamageEvent({
                amount: projectile.damage,
                source: projectileId,
                type: "Projectile"
            }));
            
            world.insert(targetId, KnockbackEffect({
                force: 30,
                direction: projectile.velocity.Unit,
                duration: 0.5
            }));
        }
        
        // 爆炸效果
        this.createExplosion(world, world.get(projectileId, Transform).position);
    },
    
    findBestTarget(world: World, ownerId: EntityId, state: SharedState): EntityId | undefined {
        const ownerInfo = world.get(ownerId, KartInfo);
        const ownerTransform = world.get(ownerId, Transform);
        
        let bestTarget: EntityId | undefined;
        let bestScore = -Infinity;
        
        for (const [targetId, targetTransform, targetInfo] of world.query(Transform, KartInfo)) {
            if (targetId === ownerId) continue;
            
            const toTarget = targetTransform.position.sub(ownerTransform.position);
            const distance = toTarget.Magnitude;
            const angle = ownerTransform.rotation.LookVector.Dot(toTarget.Unit);
            
            // 优先前方目标
            if (angle > 0.5 && distance < 50) {
                const score = angle * 100 - distance;
                if (score > bestScore) {
                    bestScore = score;
                    bestTarget = targetId;
                }
            }
        }
        
        return bestTarget;
    }
};
```

### 4.3 加速器（Booster）
```typescript
// src/shared/ecs/items/implementations/booster.ts
export const BoosterItem: ItemImplementation = {
    type: ItemType.BOOSTER,
    category: "Buff",
    
    config: {
        speedMultiplier: 1.5,
        accelerationBonus: 20,
        duration: 3,
        visualEffect: "BoosterAura",
        soundEffect: "BoosterSound"
    },
    
    onActivate(world: World, kartId: EntityId, state: SharedState): EntityId {
        // 创建Buff实体
        const buffId = world.spawn(
            ItemBuff({
                buffType: "Speed",
                modifiers: new Map([
                    ["speed", { type: "Multiplicative", value: this.config.speedMultiplier, duration: this.config.duration }],
                    ["acceleration", { type: "Additive", value: this.config.accelerationBonus, duration: this.config.duration }]
                ]),
                activationTime: state.time,
                expirationTime: state.time + this.config.duration
            }),
            BoundTo({ target: kartId })
        );
        
        // 应用到卡丁车
        const kartStats = world.get(kartId, KartStats);
        if (kartStats) {
            world.insert(kartId, kartStats.patch({
                speedMultiplier: kartStats.speedMultiplier * this.config.speedMultiplier,
                accelerationBonus: kartStats.accelerationBonus + this.config.accelerationBonus
            }));
        }
        
        // 视觉效果
        world.insert(kartId, VisualEffect({
            effectId: this.config.visualEffect,
            duration: this.config.duration
        }));
        
        // 音效
        this.playSound(world, kartId, this.config.soundEffect);
        
        // 定时移除Buff
        task.delay(this.config.duration, () => {
            this.removeBuff(world, kartId, buffId);
        });
        
        return buffId;
    },
    
    removeBuff(world: World, kartId: EntityId, buffId: EntityId) {
        const buff = world.get(buffId, ItemBuff);
        const kartStats = world.get(kartId, KartStats);
        
        if (buff && kartStats) {
            // 恢复原始属性
            world.insert(kartId, kartStats.patch({
                speedMultiplier: kartStats.speedMultiplier / buff.modifiers.get("speed").value,
                accelerationBonus: kartStats.accelerationBonus - buff.modifiers.get("acceleration").value
            }));
        }
        
        world.despawn(buffId);
    }
};
```

### 4.4 防护罩（Shield）
```typescript
// src/shared/ecs/items/implementations/shield.ts
export const ShieldItem: ItemImplementation = {
    type: ItemType.SHIELD,
    category: "Buff",
    
    config: {
        duration: 10,
        maxBlocks: 3,
        visualEffect: "ShieldBubble",
        blockEffect: "ShieldBlock"
    },
    
    onActivate(world: World, kartId: EntityId, state: SharedState): EntityId {
        // 创建护盾实体
        const shieldId = world.spawn(
            ShieldBuff({
                active: true,
                blocksRemaining: this.config.maxBlocks,
                activationTime: state.time,
                expirationTime: state.time + this.config.duration
            }),
            BoundTo({ target: kartId })
        );
        
        // 添加护盾组件到卡丁车
        world.insert(kartId, HasShield({
            shieldEntity: shieldId,
            canDeflect: true
        }));
        
        // 视觉效果
        this.createShieldVisual(world, kartId);
        
        return shieldId;
    },
    
    onBlock(world: World, shieldId: EntityId, attackType: string): boolean {
        const shield = world.get(shieldId, ShieldBuff);
        
        if (shield && shield.active) {
            shield.blocksRemaining--;
            
            // 播放格挡效果
            this.playBlockEffect(world, shieldId);
            
            if (shield.blocksRemaining <= 0) {
                // 护盾破碎
                this.breakShield(world, shieldId);
                return false;
            }
            
            world.insert(shieldId, shield);
            return true;
        }
        
        return false;
    }
};
```

## 五、网络同步策略

### 5.1 道具同步组件
```typescript
// src/shared/ecs/components/items/item-network-sync.ts
export interface ItemNetworkSync {
    syncRate: number;
    priority: number;
    reliable: boolean;
    lastSyncTime: number;
    dirtyFields: Set<string>;
    authority: "Server" | "Client" | "Shared";
    ownerId: EntityId;
}
```

### 5.2 网络同步系统
```typescript
// src/server/ecs/systems/items/item-network-system.ts
export const ItemNetworkSystem: System = {
    priority: 50,
    
    update(world: World, state: ServerState, deltaTime: number) {
        // 只在服务器执行
        if (!state.isServer) return;
        
        for (const [id, networkSync, itemActive] of world.query(ItemNetworkSync, ItemActive)) {
            const timeSinceLastSync = state.time - networkSync.lastSyncTime;
            
            if (timeSinceLastSync > 1 / networkSync.syncRate) {
                // 收集需要同步的数据
                const syncData = this.collectSyncData(world, id, networkSync);
                
                // 发送给相关客户端
                this.sendToClients(id, syncData, networkSync);
                
                // 更新同步时间
                world.insert(id, networkSync.patch({
                    lastSyncTime: state.time,
                    dirtyFields: new Set()
                }));
            }
        }
    },
    
    collectSyncData(world: World, entityId: EntityId, sync: ItemNetworkSync): ItemSyncData {
        const data: ItemSyncData = { entityId };
        
        if (sync.dirtyFields.has("Transform")) {
            data.transform = world.get(entityId, Transform);
        }
        
        if (sync.dirtyFields.has("ItemActive")) {
            data.itemActive = world.get(entityId, ItemActive);
        }
        
        return data;
    }
};
```

## 六、配置系统

### 6.1 道具配置定义
```typescript
// src/shared/configs/item-configs.ts
export const ItemConfigs: Map<ItemType, ItemConfig> = new Map([
    [ItemType.BANANA, {
        name: "Banana",
        category: "Trap",
        icon: "rbxassetid://123456",
        model: "rbxassetid://789012",
        rarity: "Common",
        rankWeights: [10, 20, 30, 40, 50, 60], // 按排名的获得权重
        params: {
            triggerRadius: 2,
            slipDuration: 2,
            slipIntensity: 0.8,
            lifetime: 30
        }
    }],
    
    [ItemType.WATER_MISSILE, {
        name: "Water Missile",
        category: "Projectile",
        icon: "rbxassetid://234567",
        model: "rbxassetid://890123",
        rarity: "Rare",
        rankWeights: [60, 50, 40, 30, 20, 10],
        params: {
            speed: 80,
            tracking: 0.8,
            damage: 20,
            lifetime: 10
        }
    }],
    
    // ... 其他道具配置
]);
```

### 6.2 道具权重系统
```typescript
// src/shared/systems/items/item-weight-system.ts
export class ItemWeightSystem {
    getItemForRank(rank: number, totalRacers: number): ItemType {
        const weights = new Map<ItemType, number>();
        
        // 根据排名计算权重
        for (const [type, config] of ItemConfigs) {
            const rankIndex = math.clamp(rank - 1, 0, config.rankWeights.size() - 1);
            const weight = config.rankWeights[rankIndex];
            
            // 根据比赛进度调整权重
            const progressModifier = this.getProgressModifier(type);
            weights.set(type, weight * progressModifier);
        }
        
        return this.weightedRandom(weights);
    }
    
    private weightedRandom(weights: Map<ItemType, number>): ItemType {
        const total = weights.values().reduce((a, b) => a + b, 0);
        let random = math.random() * total;
        
        for (const [type, weight] of weights) {
            random -= weight;
            if (random <= 0) {
                return type;
            }
        }
        
        return ItemType.BANANA; // 默认道具
    }
}
```

## 七、性能优化

### 7.1 对象池系统
```typescript
// src/shared/systems/items/item-pool-system.ts
export class ItemPoolSystem {
    private pools: Map<ItemType, Array<EntityId>> = new Map();
    private maxPoolSize = 20;
    
    getItem(world: World, type: ItemType): EntityId {
        const pool = this.pools.get(type) || [];
        
        if (pool.size() > 0) {
            const entity = pool.pop()!;
            this.resetEntity(world, entity);
            return entity;
        }
        
        return this.createNewItem(world, type);
    }
    
    returnItem(world: World, entity: EntityId, type: ItemType) {
        const pool = this.pools.get(type) || [];
        
        if (pool.size() < this.maxPoolSize) {
            this.deactivateEntity(world, entity);
            pool.push(entity);
            this.pools.set(type, pool);
        } else {
            world.despawn(entity);
        }
    }
}
```

### 7.2 空间分区优化
```typescript
// src/shared/systems/items/spatial-partitioning.ts
export class SpatialPartitioning {
    private gridSize = 50;
    private grid: Map<string, Set<EntityId>> = new Map();
    
    updateEntity(entity: EntityId, position: Vector3) {
        const oldCell = this.getEntityCell(entity);
        const newCell = this.getCellKey(position);
        
        if (oldCell !== newCell) {
            this.removeFromCell(entity, oldCell);
            this.addToCell(entity, newCell);
        }
    }
    
    getNearbyEntities(position: Vector3, radius: number): Array<EntityId> {
        const nearby: Array<EntityId> = [];
        const cellsToCheck = this.getCellsInRadius(position, radius);
        
        for (const cell of cellsToCheck) {
            const entities = this.grid.get(cell);
            if (entities) {
                nearby.push(...entities);
            }
        }
        
        return nearby;
    }
}
```

## 八、测试用例

### 8.1 道具拾取测试
```typescript
// src/tests/items/item-pickup.test.ts
describe("ItemPickupSystem", () => {
    it("should pick up item when kart collides with item box", () => {
        const world = new World();
        const state = createTestState();
        
        // 创建道具箱
        const itemBox = world.spawn(
            ItemBox({ type: "Random", active: true }),
            Transform({ position: new Vector3(0, 0, 0) }),
            Collider({ radius: 2 })
        );
        
        // 创建卡丁车
        const kart = world.spawn(
            Transform({ position: new Vector3(1, 0, 0) }),
            ItemHolder({ items: [], maxSlots: 3 })
        );
        
        // 执行系统更新
        ItemPickupSystem.update(world, state, 0.016);
        
        // 验证道具被拾取
        const holder = world.get(kart, ItemHolder);
        expect(holder.items.size()).toBe(1);
        
        // 验证道具箱被禁用
        const box = world.get(itemBox, ItemBox);
        expect(box.active).toBe(false);
    });
});
```

## 九、实施计划

### 第一阶段：基础架构（第1-2周）
1. 创建所有组件接口
2. 实现基础系统（拾取、使用）
3. 建立网络同步框架

### 第二阶段：道具实现（第3-4周）
1. 实现所有10种道具
2. 添加视觉效果和音效
3. 完成道具交互逻辑

### 第三阶段：优化和测试（第5-6周）
1. 性能优化（对象池、空间分区）
2. 网络优化（预测、插值）
3. 全面测试和调试

### 第四阶段：平衡调整（第7周）
1. 道具平衡性调整
2. AI使用策略优化
3. 最终测试和发布

## 十、注意事项

1. **坐标系差异**：Roblox使用Y轴向上，Unity使用Y轴向前，需要转换
2. **物理引擎差异**：Roblox的物理更新频率可能不同，需要调整参数
3. **网络延迟**：实现客户端预测和服务器校正机制
4. **性能考虑**：移动设备性能有限，需要LOD和动态调整
5. **作弊防护**：所有关键逻辑在服务器验证

这个方案提供了完整的ECS道具系统实现路线图，可以根据实际需求进行调整和扩展。