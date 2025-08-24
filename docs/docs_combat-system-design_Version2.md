# 战斗与技能系统统一设计文档 (ECS / Matter 架构，含带注释的全部组件)

本文件整合“技能系统”与“普通攻击（多目标范围 / Cleave）系统”，并在所有组件定义中为每个属性补充详细中文注释，便于团队协作、维护与后续扩展。

---

## 目录
1. 设计目标与核心原则  
2. 总体结构与数据流  
3. 组件角色总览  
4. 技能系统  
   4.1 技能类型分类  
   4.2 SkillCastRequest 模型  
   4.3 技能配置示例  
   4.4 技能释放流水线  
   4.5 投射物技能  
   4.6 范围即时技能  
   4.7 持续效果  
   4.8 冷却与资源  
   4.9 伤害结算流水线  
   4.10 复合/高级扩展  
5. 普通攻击系统  
   5.1 普攻与技能关系  
   5.2 时间轴  
   5.3 多目标命中判定  
   5.4 攻速/暴击/吸血/连击  
   5.5 On-Hit 效果  
6. 统一伤害与状态组件  
7. 系统清单与职责矩阵  
8. 目录结构建议  
9. 代码示例（所有组件属性含注释）  
   9.1 技能相关组件与系统  
   9.2 普通攻击相关组件与系统  
   9.3 持续效果 / 状态 / 公共组件  
   9.4 系统实现示例  
10. 性能与优化策略  
11. 调试与监控建议  
12. 常见扩展路线  
13. 实施与迭代策略  
14. 总结  

---

## 1. 设计目标与核心原则

| 目标 | 说明 |
|------|------|
| 数据驱动 | 配置主导行为，逻辑模块通用化。 |
| 高扩展 | 新技能/普攻形态通过新增配置 + 少量系统适配。 |
| 低耦合 | 释放、移动、命中、结算、持续效果解耦。 |
| 一体化 | 技能与普攻共享伤害 / 状态管线。 |
| 可观测 | 关键节点可插入 CombatLog / 事件。 |
| 安全与验证 | 服务端校验请求合法性（距离/冷却/资源）。 |

---

## 2. 总体结构与数据流

输入(玩家/AI) → 请求组件(SkillCastRequest/AttackRequest) → 解析分发 → 中间效果组件(Projectile / Hitbox / Area) → 产出伤害/状态(TakeDamage / DotEffect / BuffEffect) → 结算(Health 更新) → 后处理 (OnHitEffectQueue / CombatLog) → 清理临时组件。

---

## 3. 组件角色总览

| 组件 | 作用 |
|------|------|
| SkillCastRequest | 技能释放意图输入 |
| AttackRequest | 普攻释放意图输入 |
| SkillProjectile | 技能投射物状态 |
| AttackSwing | 普攻时间轴（前摇/命中/后摇） |
| AttackHitbox | 普攻命中窗口（扇形/圆形） |
| ApplyDamageSphere | 技能一次性范围伤害输入 |
| TakeDamage | 待结算伤害事件 |
| Health | 实体生命值 |
| DotEffect | 持续伤害状态 |
| BuffEffect | 通用属性增益状态 |
| SlowEffect | 减速状态 |
| Cooldown | 技能冷却 |
| Resource | 资源（法力/能量） |
| AttackStats | 攻击者统计（攻速/攻击力/暴击等） |
| ComboState | 普攻连击状态 |
| OnHitEffectQueue | 命中后待应用效果队列 |
| CombatLog (扩展) | 战斗日志记录（可选实现） |

---

## 4. 技能系统（概要）

### 4.1 技能类型
Area / Target / Projectile / Composite / OverTime。

### 4.2 SkillCastRequest
统一输入结构，便于后端校验 & 扩展字段。

### 4.3 技能配置（示例见 9.1）
包含伤害、形态、范围、冷却、是否附加 DOT 等。

### 4.4 流水线
请求 → 分发 → 生成效果组件 → 运行系统推进行为 → 产出伤害/状态 → 结算。

### 4.5 投射物
生命周期：生成→飞行→命中→爆炸/直伤→回收。

### 4.6 范围即时技能
ApplyDamageSphere → 一帧处理 → 生成 TakeDamage → 移除。

### 4.7 持续效果
DotEffect / BuffEffect / SlowEffect 由专用系统定时或聚合处理。

### 4.8 冷却与资源
Cooldown + Resource 组件控制释放条件。

### 4.9 伤害结算
TakeDamage → damageResolveSystem → Health 更新 → 后续处理。

### 4.10 高级扩展
多段/链式/穿透/延迟可通过组合配置 + 新组件实现。

---

## 5. 普通攻击系统（概要）

### 5.1 与技能关系
可独立也可统一到技能框架；本方案分离以减轻 castSkillSystem 复杂度。

### 5.2 时间轴
AttackSwing：startTime / impactTime / endTime；命中帧生成 AttackHitbox。

### 5.3 命中判定
扇形/圆形/盒形；空间预筛选 + 形状精判。

### 5.4 属性与连击
AttackStats 决定攻速/暴击；ComboState 引导下一击形态。

### 5.5 On-Hit
吸血、附魔 DOT、减速等在 OnHitEffectQueue 中延迟处理。

---

## 6. 统一伤害与状态组件

技能与普攻最终都产出 TakeDamage；持续类由 DotEffect 等驱动；BuffEffect / SlowEffect 影响属性聚合或移动系统。

---

## 7. 系统清单与职责矩阵

| 系统 | 职责 |
|------|------|
| castSkillSystem | 技能请求→效果组件 |
| projectileSystem | 投射物移动/命中 |
| applyDamageSystem | 范围伤害→TakeDamage |
| damageResolveSystem | 伤害结算 |
| dotSystem | DOT Tick |
| buffSystem | Buff/Slow 生命周期与属性聚合（示例未完全实现） |
| cooldownSystem | 冷却写入/校验（可与 cast 合并） |
| cleanupSystem | 通用清理 |
| startAttackSystem | 普攻请求→AttackSwing |
| attackWindowSystem | 生成命中窗口 |
| collectTargetsSystem | 命中判定→伤害 |
| comboSystem | 连击推进/过期 |
| onHitEffectSystem | 命中后效果（吸血等） |
| attackCleanupSystem | 命中窗口清理 |

---

## 8. 目录结构建议

```
src/
  shared/
    config/
      skills.ts
      basicAttacks.ts
    components/
      skillCastRequest.ts
      skillProjectile.ts
      damage/
        applyDamageSphere.ts
        takeDamage.ts
        health.ts
      effects/
        dotEffect.ts
        buffEffect.ts
        slowEffect.ts
      meta/
        cooldown.ts
        resource.ts
      attack/
        attackRequest.ts
        attackStats.ts
        attackSwing.ts
        attackHitbox.ts
        comboState.ts
        onHitEffectQueue.ts
  server/
    systems/
      skill/
        castSkillSystem.ts
        projectileSystem.ts
        applyDamageSystem.ts
        damageResolveSystem.ts
        dotSystem.ts
        buffSystem.ts
        cooldownSystem.ts
        cleanupSystem.ts
      attack/
        startAttackSystem.ts
        attackWindowSystem.ts
        collectTargetsSystem.ts
        comboSystem.ts
        onHitEffectSystem.ts
        attackCleanupSystem.ts
```

---

## 9. 代码示例（所有组件属性含注释）

### 9.1 技能配置与组件

```typescript name=src/shared/config/skills.ts
/**
 * 技能静态配置表（仅示例字段，可扩展）
 */
export const SkillConfigs = {
  Fireball: {
    type: "Projectile",                 // 技能类型：投射物
    speed: 32,                          // 投射物初速度
    damage: 60,                         // 基础伤害
    radiusOnHit: 8,                     // 命中爆炸半径
    damageType: "Fire",                 // 伤害类型
    cooldown: 3,                        // 冷却（秒）
    explodeOnHit: true,                 // 命中是否爆炸
    canApplyDot: true,                  // 是否附加 DOT
    dot: {                              // DOT 配置
      damagePerTick: 5,                 // 单次跳动伤害
      interval: 0.5,                    // 跳动间隔
      ticks: 6,                         // 跳动次数
    },
  },
  Explosion: {
    type: "Area",
    damage: 80,
    radius: 12,
    damageType: "Fire",
    cooldown: 5,
  },
  Heal: {
    type: "Target",
    heal: 40,                           // 治疗量
    cooldown: 6,
  },
};
export type SkillName = keyof typeof SkillConfigs;
```

```typescript name=src/shared/components/skillCastRequest.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 技能释放请求（输入意图）
 */
export const SkillCastRequest = component<{
  /** 技能名称（应存在 SkillConfigs 中） */
  skillName: string;
  /** 释放者实体 ID */
  caster: AnyEntity;
  /** 释放位置（对范围/投射物起点有效） */
  position?: Vector3;
  /** 朝向/方向向量（对投射物或方向性技能有效） */
  direction?: Vector3;
  /** 单体指向目标实体（Target 型技能） */
  target?: AnyEntity;
  /** 额外扩展参数（临时传递） */
  extra?: Record<string, unknown>;
}>("SkillCastRequest");
```

```typescript name=src/shared/components/skillProjectile.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 技能投射物状态
 */
export const SkillProjectile = component<{
  /** 当前世界位置 */
  position: Vector3;
  /** 前进方向（单位向量） */
  direction: Vector3;
  /** 每秒飞行速度（单位：stud/s 或游戏坐标单位） */
  speed: number;
  /** 投射物所属施法者 */
  caster: AnyEntity;
  /** 基础伤害（命中或爆炸使用） */
  damage: number;
  /** 伤害类型（Fire/Physical/...） */
  damageType: string;
  /** 命中时爆炸半径（存在则可生成范围伤害） */
  radiusOnHit?: number;
  /** 是否在命中时引发爆炸效果 */
  explodeOnHit?: boolean;
  /** 是否附加 DOT 效果 */
  canApplyDot?: boolean;
  /** DOT 具体配置（可选） */
  dot?: { damagePerTick: number; interval: number; ticks: number };
  /** 投射物最大存活时间（秒） */
  lifeTime: number;
  /** 当前存活时间（秒） */
  age: number;
  /** 是否已命中（命中后停止更新） */
  hit?: boolean;
}>("SkillProjectile");
```

```typescript name=src/shared/components/damage/applyDamageSphere.ts
import { component } from "@rbxts/matter";

/**
 * 一次性范围伤害输入组件
 */
export const ApplyDamageSphere = component<{
  /** 范围中心位置 */
  position: Vector3;
  /** 伤害半径 */
  radius: number;
  /** 基础伤害值 */
  damage: number;
  /** 伤害类型 */
  damageType: string;
  /** 来源实体（可用于统计/归属） */
  source?: number;
}>("ApplyDamageSphere");
```

```typescript name=src/shared/components/damage/takeDamage.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 待结算单体伤害事件
 */
export const TakeDamage = component<{
  /** 伤害数值（已计算基础伤害，不含最终减免） */
  amount: number;
  /** 伤害类型（用于抗性和效果判定） */
  damageType: string;
  /** 伤害来源（施加者实体） */
  inflictor?: AnyEntity;
}>("TakeDamage");
```

```typescript name=src/shared/components/damage/health.ts
import { component } from "@rbxts/matter";

/**
 * 生命值组件
 */
export const Health = component<{
  /** 当前生命值 */
  current: number;
  /** 最大生命值 */
  max: number;
}>("Health");
```

### 9.2 持续效果 / 状态 / Buff 组件

```typescript name=src/shared/components/effects/dotEffect.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 持续伤害 (Damage Over Time) 组件
 */
export const DotEffect = component<{
  /** 单次跳动伤害 */
  damagePerTick: number;
  /** 每次跳动间隔（秒） */
  interval: number;
  /** 剩余跳动次数 */
  ticksRemaining: number;
  /** 下次触发时间戳（os.clock()） */
  nextTime: number;
  /** 来源实体（输出归属） */
  source?: AnyEntity;
  /** DOT 的伤害类型 */
  damageType: string;
}>("DotEffect");
```

```typescript name=src/shared/components/effects/buffEffect.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 通用 Buff（属性增益）组件
 * 示例：增加攻击力 / 防御 / 移动速度等
 */
export const BuffEffect = component<{
  /** Buff 名称（用于识别与刷新） */
  name: string;
  /** 目标实体（受影响对象） */
  target: AnyEntity;
  /** 生效的属性键（如 AttackPower / MoveSpeed） */
  stat: string;
  /** 属性增减值（可为加成） */
  delta: number;
  /** 持续时长（秒） */
  duration: number;
  /** 已经过时间（秒） */
  elapsed: number;
  /** 可选：叠加层数（扩展） */
  stacks?: number;
  /** 是否可与自身重复叠加 */
  stackable?: boolean;
}>("BuffEffect");
```

```typescript name=src/shared/components/effects/slowEffect.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 减速 Debuff 组件
 */
export const SlowEffect = component<{
  /** 目标实体 */
  target: AnyEntity;
  /** 移动速度倍率（0.7 表示降低到 70%） */
  factor: number;
  /** 持续时间（秒） */
  duration: number;
  /** 已经过时间（秒） */
  elapsed: number;
  /** 来源实体（用于来源统计/清除） */
  source?: AnyEntity;
}>("SlowEffect");
```

### 9.3 冷却 / 资源 组件

```typescript name=src/shared/components/meta/cooldown.ts
import { component } from "@rbxts/matter";

/**
 * 技能冷却组件
 */
export const Cooldown = component<{
  /** 技能名称 */
  skillName: string;
  /** 冷却结束时间戳（os.clock()） */
  endTime: number;
  /** 拥有者实体 ID（施法者） */
  caster: number;
}>("Cooldown");
```

```typescript name=src/shared/components/meta/resource.ts
import { component } from "@rbxts/matter";

/**
 * 资源（法力/能量等）组件
 */
export const Resource = component<{
  /** 当前法力值 */
  mana: number;
  /** 能量值（可选，用于特殊职业） */
  energy?: number;
}>("Resource");
```

### 9.4 普通攻击相关组件

```typescript name=src/shared/components/attack/attackRequest.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 普通攻击请求（输入意图）
 */
export const AttackRequest = component<{
  /** 攻击者实体 ID */
  attacker: AnyEntity;
  /** 攻击配置名称（缺省用默认形态） */
  attackName?: string;
  /** 自定义方向（无则读取 Transform） */
  direction?: Vector3;
  /** 自定义起点（无则读取当前角色位置） */
  position?: Vector3;
}>("AttackRequest");
```

```typescript name=src/shared/components/attack/attackStats.ts
import { component } from "@rbxts/matter";

/**
 * 攻击者战斗统计（可被 Buff 效果修改）
 */
export const AttackStats = component<{
  /** 攻击强度（用于伤害公式 scaling） */
  attackPower: number;
  /** 攻击速度倍率（1 为基准，>1 更快） */
  attackSpeed: number;
  /** 暴击概率（0~1） */
  critChance: number;
  /** 暴击伤害倍数（默认 2） */
  critMultiplier: number;
  /** 吸血百分比（0~1） */
  lifeSteal?: number;
  /** 额外攻击范围加成（附加到 radius） */
  bonusRange?: number;
}>("AttackStats");
```

```typescript name=src/shared/components/attack/attackSwing.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 一次普通攻击的时间轴状态
 */
export const AttackSwing = component<{
  /** 攻击者实体 */
  attacker: AnyEntity;
  /** 使用的攻击配置名称 */
  attackName: string;
  /** 攻击开始时间 */
  startTime: number;
  /** 命中帧时间（生成 Hitbox 时刻） */
  impactTime: number;
  /** 完成时间（进入后摇结束） */
  endTime: number;
  /** 命中形状类型（Sector/Circle/Box） */
  shape: "Sector" | "Circle" | "Box";
  /** 攻击半径（或范围尺度） */
  radius: number;
  /** 扇形角度（仅 Sector 有效） */
  angle?: number;
  /** 最大命中目标数（溢出则截断） */
  maxTargets: number;
  /** 攻击朝向（用于扇形/盒体） */
  direction: Vector3;
  /** 攻击起点位置 */
  position: Vector3;
  /** 是否已生成命中窗口（Hitbox） */
  hitGenerated: boolean;
  /** 是否已完全结束 */
  finished: boolean;
}>("AttackSwing");
```

```typescript name=src/shared/components/attack/attackHitbox.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 普攻命中窗口（用于一次命中批处理）
 */
export const AttackHitbox = component<{
  /** 发起攻击者实体 */
  attacker: AnyEntity;
  /** 过期时间（now > expireTime 即失效） */
  expireTime: number;
  /** 形状类型 */
  shape: "Sector" | "Circle" | "Box";
  /** Hitbox 中心位置 */
  position: Vector3;
  /** 朝向（扇形/盒形使用） */
  direction: Vector3;
  /** 半径或主尺度 */
  radius: number;
  /** 扇形角度（Sector 时有效） */
  angle?: number;
  /** 最多命中数量 */
  maxTargets: number;
  /** 对应的攻击配置名称 */
  attackName: string;
}>("AttackHitbox");
```

```typescript name=src/shared/components/attack/comboState.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 连击状态
 */
export const ComboState = component<{
  /** 攻击者实体 */
  attacker: AnyEntity;
  /** 当前连击段索引（从 1 开始） */
  index: number;
  /** 上次攻击结束时间（用于窗口判定） */
  lastAttackEnd: number;
}>("ComboState");
```

```typescript name=src/shared/components/attack/onHitEffectQueue.ts
import { AnyEntity, component } from "@rbxts/matter";

/**
 * 命中后效果队列（延迟处理吸血 / 附魔 / Debuff）
 */
export const OnHitEffectQueue = component<{
  /** 发起者实体 */
  attacker: AnyEntity;
  /** 被命中目标实体 */
  target: AnyEntity;
  /** 效果列表（类型 + 数据） */
  effects: Array<{
    /** 效果类型（如 LifeSteal / ApplyDot / ApplySlow） */
    type: string;
    /** 额外数据（幅度/持续等） */
    data?: Record<string, unknown>;
  }>;
  /** 来源攻击名称（用于统计/回放） */
  sourceAttack?: string;
  /** 实际造成的伤害值（用于吸血/触发计算） */
  damageDealt?: number;
}>("OnHitEffectQueue");
```

### 9.5 普通攻击配置

```typescript name=src/shared/config/basicAttacks.ts
/**
 * 普通攻击配置表（示例）
 */
export const BasicAttackConfigs = {
  DefaultMelee: {
    shape: "Sector" as const,     // 形状：扇形
    radius: 6,                    // 半径
    angle: 100,                   // 扇形角度
    maxTargets: 5,                // 最大命中数
    preDelay: 0.25,               // 前摇时间（可用于动画）
    impactDelay: 0.25,            // 命中帧相对开始时间
    endDelay: 0.6,                // 攻击完整结束时间
    baseDamage: 30,               // 基础伤害
    scaling: { attackPower: 0.8 },// 攻击力伤害系数
    crit: true,                   // 是否可暴击
    critMultiplier: 2.0,          // 暴击倍数
    cleaveFalloff: 0,             // 伤害衰减系数（0 表示无衰减）
    allowMoveDuring: false,       // 是否允许移动
  },
  HeavyMelee: {
    shape: "Sector" as const,
    radius: 7.5,
    angle: 120,
    maxTargets: 8,
    preDelay: 0.5,
    impactDelay: 0.5,
    endDelay: 1.0,
    baseDamage: 60,
    scaling: { attackPower: 1.1 },
    crit: true,
    critMultiplier: 2.1,
    cleaveFalloff: 0.15,
    allowMoveDuring: false,
  },
  RapidDaggers: {
    shape: "Circle" as const,
    radius: 4,
    maxTargets: 3,
    preDelay: 0.12,
    impactDelay: 0.12,
    endDelay: 0.32,
    baseDamage: 18,
    scaling: { attackPower: 0.55 },
    crit: true,
    critMultiplier: 1.8,
    cleaveFalloff: 0,
    allowMoveDuring: true,
    comboWindow: 0.6,             // 连击窗口
  },
};
export type BasicAttackName = keyof typeof BasicAttackConfigs;
```

### 9.6 系统实现（关键示例：保持与上一版一致，仅添加注释）

```typescript name=src/server/systems/skill/castSkillSystem.ts
import { World } from "@rbxts/matter";
import { SkillCastRequest } from "shared/components/skillCastRequest";
import { SkillConfigs } from "shared/config/skills";
import { SkillProjectile } from "shared/components/skillProjectile";
import { ApplyDamageSphere } from "shared/components/damage/applyDamageSphere";

/**
 * 技能释放分发系统：
 * 将 SkillCastRequest 转化为具体效果组件（投射物/范围/治疗等）
 */
export = (world: World) => {
  for (const [id, req] of world.query(SkillCastRequest)) {
    const cfg = SkillConfigs[req.skillName];
    if (!cfg) { world.remove(id, SkillCastRequest); continue; }

    switch (cfg.type) {
      case "Area":
        if (req.position) {
          world.spawn(ApplyDamageSphere({
            position: req.position,
            radius: cfg.radius,
            damage: cfg.damage,
            damageType: cfg.damageType,
          }));
        }
        break;
      case "Projectile":
        if (req.position && req.direction) {
          world.spawn(SkillProjectile({
            position: req.position,
            direction: req.direction.Unit,
            speed: cfg.speed,
            caster: req.caster,
            damage: cfg.damage,
            damageType: cfg.damageType,
            radiusOnHit: cfg.radiusOnHit,
            explodeOnHit: cfg.explodeOnHit,
            canApplyDot: cfg.canApplyDot,
            dot: cfg.dot,
            lifeTime: 5,
            age: 0,
          }));
        }
        break;
      case "Target":
        if (req.target) {
          // 可在此处插入治疗组件（例如 ApplyHeal），此处省略
        }
        break;
    }
    world.remove(id, SkillCastRequest);
  }
};
```

```typescript name=src/server/systems/skill/projectileSystem.ts
import { World } from "@rbxts/matter";
import { SkillProjectile } from "shared/components/skillProjectile";
import { ApplyDamageSphere } from "shared/components/damage/applyDamageSphere";
import { DotEffect } from "shared/components/effects/dotEffect";

const dt = () => 1 / 60;

/**
 * 投射物移动与命中检测系统
 */
export = (world: World) => {
  for (const [id, proj] of world.query(SkillProjectile)) {
    if (proj.hit) continue;

    const newPos = proj.position.add(proj.direction.mul(proj.speed * dt()));
    const hitEntity = raycastForEntity(proj.position, newPos); // TODO: 实现碰撞/实体检测

    const newAge = proj.age + dt();
    if (newAge > proj.lifeTime) {
      world.remove(id, SkillProjectile);
      continue;
    }

    if (hitEntity) {
      if (proj.explodeOnHit && proj.radiusOnHit) {
        world.spawn(ApplyDamageSphere({
          position: newPos,
          radius: proj.radiusOnHit,
          damage: proj.damage,
          damageType: proj.damageType,
          source: id,
        }));
      } else {
        // 单体直接伤害：可插入 TakeDamage（为了演示省略）
      }

      if (proj.canApplyDot && proj.dot) {
        world.insert(hitEntity, DotEffect({
          damagePerTick: proj.dot.damagePerTick,
          interval: proj.dot.interval,
          ticksRemaining: proj.dot.ticks,
          nextTime: os.clock() + proj.dot.interval,
          source: proj.caster,
          damageType: proj.damageType,
        }));
      }
      world.insert(id, SkillProjectile({ ...proj, position: newPos, age: newAge, hit: true }));
    } else {
      world.insert(id, SkillProjectile({ ...proj, position: newPos, age: newAge }));
    }
  }
};

function raycastForEntity(from: Vector3, to: Vector3): number | undefined {
  return undefined;
}
```

```typescript name=src/server/systems/skill/applyDamageSystem.ts
import { World } from "@rbxts/matter";
import { ApplyDamageSphere } from "shared/components/damage/applyDamageSphere";
import { TakeDamage } from "shared/components/damage/takeDamage";

/**
 * 处理范围伤害输入，生成单体伤害事件
 */
export = (world: World) => {
  for (const [id, sphere] of world.query(ApplyDamageSphere)) {
    const targets = findEntitiesInRadius(world, sphere.position, sphere.radius);
    for (const target of targets) {
      world.insert(target, TakeDamage({
        amount: sphere.damage,
        damageType: sphere.damageType,
        inflictor: sphere.source,
      }));
    }
    world.remove(id, ApplyDamageSphere);
  }
};

function findEntitiesInRadius(world: World, position: Vector3, radius: number): number[] {
  return [];
}
```

```typescript name=src/server/systems/skill/damageResolveSystem.ts
import { World } from "@rbxts/matter";
import { TakeDamage } from "shared/components/damage/takeDamage";
import { Health } from "shared/components/damage/health";

/**
 * 伤害结算系统：从 TakeDamage 修改 Health
 */
export = (world: World) => {
  for (const [id, dmg] of world.query(TakeDamage)) {
    const health = world.get(id, Health)[0];
    if (health) {
      const newHP = math.max(0, health.current - dmg.amount);
      world.insert(id, Health({ ...health, current: newHP }));
      // 可扩展：死亡处理 / 战斗日志
    }
    world.remove(id, TakeDamage);
  }
};
```

```typescript name=src/server/systems/skill/dotSystem.ts
import { World } from "@rbxts/matter";
import { DotEffect } from "shared/components/effects/dotEffect";
import { TakeDamage } from "shared/components/damage/takeDamage";

/**
 * DOT 跳动驱动系统
 */
export = (world: World) => {
  const now = os.clock();
  for (const [id, dot] of world.query(DotEffect)) {
    if (now >= dot.nextTime) {
      world.insert(id, TakeDamage({
        amount: dot.damagePerTick,
        damageType: dot.damageType,
        inflictor: dot.source,
      }));
      const remain = dot.ticksRemaining - 1;
      if (remain <= 0) {
        world.remove(id, DotEffect);
      } else {
        world.insert(id, DotEffect({
          ...dot,
            ticksRemaining: remain,
            nextTime: now + dot.interval,
        }));
      }
    }
  }
};
```

```typescript name=src/server/systems/attack/startAttackSystem.ts
import { World } from "@rbxts/matter";
import { AttackRequest } from "shared/components/attack/attackRequest";
import { AttackStats } from "shared/components/attack/attackStats";
import { AttackSwing } from "shared/components/attack/attackSwing";
import { ComboState } from "shared/components/attack/comboState";
import { BasicAttackConfigs } from "shared/config/basicAttacks";

/**
 * 普攻启动系统：将 AttackRequest 转化为 AttackSwing
 */
const startAttackSystem = (world: World) => {
  const now = os.clock();

  for (const [id, req] of world.query(AttackRequest)) {
    const attacker = req.attacker;
    const stats = world.get(attacker, AttackStats)[0];
    const combo = world.get(attacker, ComboState)[0];

    let attackName = req.attackName ?? "DefaultMelee";
    if (combo) {
      const cfg0 = BasicAttackConfigs[attackName];
      if (cfg0 && "comboWindow" in cfg0) {
        const window = (cfg0 as any).comboWindow as number | undefined;
        if (window && now - combo.lastAttackEnd <= window && combo.index === 1) {
          attackName = "HeavyMelee";
        }
      }
    }

    const cfg = BasicAttackConfigs[attackName];
    if (!cfg) { world.remove(id, AttackRequest); continue; }

    const spd = stats?.attackSpeed ?? 1;
    const impactTime = now + cfg.impactDelay / spd;
    const endTime = now + cfg.endDelay / spd;
    const direction = (req.direction && req.direction.Magnitude > 0)
      ? req.direction.Unit
      : new Vector3(0, 0, -1);
    const position = req.position ?? new Vector3(0, 0, 0);

    world.spawn(AttackSwing({
      attacker,
      attackName,
      startTime: now,
      impactTime,
      endTime,
      shape: cfg.shape,
      radius: cfg.radius + (stats?.bonusRange ?? 0),
      angle: cfg.shape === "Sector" ? cfg.angle : undefined,
      maxTargets: cfg.maxTargets,
      direction,
      position,
      hitGenerated: false,
      finished: false,
    }));

    world.remove(id, AttackRequest);
  }
};

export = startAttackSystem;
```

```typescript name=src/server/systems/attack/attackWindowSystem.ts
import { World } from "@rbxts/matter";
import { AttackSwing } from "shared/components/attack/attackSwing";
import { AttackHitbox } from "shared/components/attack/attackHitbox";

/**
 * 命中窗口生成系统
 */
const attackWindowSystem = (world: World) => {
  const now = os.clock();

  for (const [id, swing] of world.query(AttackSwing)) {
    if (!swing.hitGenerated && now >= swing.impactTime) {
      world.spawn(AttackHitbox({
        attacker: swing.attacker,
        shape: swing.shape,
        position: swing.position,
        direction: swing.direction,
        radius: swing.radius,
        angle: swing.angle,
        maxTargets: swing.maxTargets,
        attackName: swing.attackName,
        expireTime: now, // 瞬时命中
      }));
      world.insert(id, AttackSwing({ ...swing, hitGenerated: true }));
    }
    if (!swing.finished && now >= swing.endTime) {
      world.insert(id, AttackSwing({ ...swing, finished: true }));
    }
  }
};

export = attackWindowSystem;
```

```typescript name=src/server/systems/attack/collectTargetsSystem.ts
import { World } from "@rbxts/matter";
import { AttackHitbox } from "shared/components/attack/attackHitbox";
import { AttackStats } from "shared/components/attack/attackStats";
import { TakeDamage } from "shared/components/damage/takeDamage";
import { BasicAttackConfigs } from "shared/config/basicAttacks";

/**
 * 普攻命中判定与伤害生成系统
 */
const collectTargetsSystem = (world: World) => {
  const now = os.clock();

  for (const [id, hitbox] of world.query(AttackHitbox)) {
    if (now > hitbox.expireTime) { world.remove(id, AttackHitbox); continue; }

    const cfg = BasicAttackConfigs[hitbox.attackName];
    if (!cfg) { world.remove(id, AttackHitbox); continue; }

    const stats = world.get(hitbox.attacker, AttackStats)[0];
    const candidates = spatialQueryCandidates(world, hitbox.position, hitbox.radius);
    const valid: number[] = [];

    for (const target of candidates) {
      if (isEnemy(hitbox.attacker, target) && shapeContains(hitbox, getEntityPosition(world, target))) {
        valid.push(target);
        if (valid.size() >= hitbox.maxTargets) break;
      }
    }

    for (const target of valid) {
      const dmg = computeDamage(cfg, stats);
      const finalDamage = maybeCrit(dmg, cfg, stats);
      world.insert(target, TakeDamage({
        amount: finalDamage,
        damageType: "Physical",
        inflictor: hitbox.attacker,
      }));
    }

    world.remove(id, AttackHitbox);
  }
};

function computeDamage(cfg: typeof BasicAttackConfigs[string], stats?: ReturnType<typeof AttackStats>) {
  const ap = stats?.attackPower ?? 0;
  const scale = cfg.scaling?.attackPower ?? 0;
  return cfg.baseDamage + ap * scale;
}

function maybeCrit(raw: number, cfg: typeof BasicAttackConfigs[string], stats?: ReturnType<typeof AttackStats>) {
  if (!cfg.crit) return raw;
  const chance = stats?.critChance ?? 0;
  if (math.random() < chance) {
    const multi = stats?.critMultiplier ?? cfg.critMultiplier ?? 2;
    return raw * multi;
  }
  return raw;
}

// ---- 下列函数需结合实际 Transform / 阵营系统实现 ----
function spatialQueryCandidates(world: World, center: Vector3, radius: number): number[] { return []; }
function getEntityPosition(world: World, entity: number): Vector3 { return new Vector3(); }
function isEnemy(attacker: number, target: number): boolean { return attacker !== target; }
function shapeContains(hitbox: ReturnType<typeof AttackHitbox>, point: Vector3): boolean {
  const delta = point.sub(hitbox.position);
  const flat = new Vector3(delta.X, 0, delta.Z);
  if (flat.Magnitude > hitbox.radius) return false;
  if (hitbox.shape === "Sector" && hitbox.angle !== undefined) {
    const dirFlat = new Vector3(hitbox.direction.X, 0, hitbox.direction.Z).Unit;
    const cosAngle = dirFlat.Dot(flat.Unit);
    const limit = math.cos(math.rad(hitbox.angle / 2));
    return cosAngle >= limit;
  }
  return true;
}

export = collectTargetsSystem;
```

```typescript name=src/server/systems/attack/comboSystem.ts
import { World } from "@rbxts/matter";
import { AttackSwing } from "shared/components/attack/attackSwing";
import { ComboState } from "shared/components/attack/comboState";
import { BasicAttackConfigs } from "shared/config/basicAttacks";

/**
 * 连击状态推进与超时重置系统
 */
const comboSystem = (world: World) => {
  const now = os.clock();

  // 处理结束的 Swing
  for (const [id, swing] of world.query(AttackSwing)) {
    if (swing.finished) {
      const cfg = BasicAttackConfigs[swing.attackName];
      const combo = world.get(swing.attacker, ComboState)[0];
      if (cfg && "comboWindow" in cfg) {
        const window = (cfg as any).comboWindow as number | undefined;
        if (window) {
          if (combo) {
            world.insert(swing.attacker, ComboState({
              attacker: swing.attacker,
              index: combo.index + 1,
              lastAttackEnd: now,
            }));
          } else {
            world.insert(swing.attacker, ComboState({
              attacker: swing.attacker,
              index: 1,
              lastAttackEnd: now,
            }));
          }
        }
      }
      world.remove(id, AttackSwing);
    }
  }

  // 超时重置
  for (const [cid, c] of world.query(ComboState)) {
    if (now - c.lastAttackEnd > 1.5 && c.index !== 1) {
      world.insert(c.attacker, ComboState({
        attacker: c.attacker,
        index: 1,
        lastAttackEnd: c.lastAttackEnd,
      }));
    }
  }
};

export = comboSystem;
```

```typescript name=src/server/systems/attack/onHitEffectSystem.ts
import { World } from "@rbxts/matter";
import { OnHitEffectQueue } from "shared/components/attack/onHitEffectQueue";
import { Health } from "shared/components/damage/health";

/**
 * 命中后效果处理系统（吸血/附魔等）
 */
const onHitEffectSystem = (world: World) => {
  for (const [id, q] of world.query(OnHitEffectQueue)) {
    for (const eff of q.effects) {
      switch (eff.type) {
        case "LifeSteal": {
          const ratio = (eff.data?.ratio as number) ?? 0;
          if (q.damageDealt && ratio > 0) {
            const health = world.get(q.attacker, Health)[0];
            if (health) {
              const healed = math.min(health.max, health.current + q.damageDealt * ratio);
              world.insert(q.attacker, Health({ ...health, current: healed }));
            }
          }
          break;
        }
        // 其他类型：ApplyDot / ApplySlow / GainShield ...
      }
    }
    world.remove(id, OnHitEffectQueue);
  }
};

export = onHitEffectSystem;
```

```typescript name=src/server/systems/attack/attackCleanupSystem.ts
import { World } from "@rbxts/matter";
import { AttackHitbox } from "shared/components/attack/attackHitbox";

/**
 * 命中窗口清理（防止残留）
 */
const attackCleanupSystem = (world: World) => {
  const now = os.clock();
  for (const [id, hb] of world.query(AttackHitbox)) {
    if (now > hb.expireTime) {
      world.remove(id, AttackHitbox);
    }
  }
};

export = attackCleanupSystem;
```

```typescript name=src/server/systems/skill/cleanupSystem.ts
import { World } from "@rbxts/matter";

/**
 * 通用清理系统（示例：保留扩展位置）
 * 可用于扫描具有 age/duration 字段的组件并统一过期处理
 */
export = (world: World) => {
  // 视需求实现
};
```

---

## 10. 性能与优化策略

| 场景 | 优化方向 |
|------|----------|
| 大量投射物 | 分层更新 / 降采样 / 区域分桶 |
| 范围查询 | 网格/四叉树/空间 HASH |
| 多段 DOT | 按 interval 分组 Batch Tick |
| 高帧写入 | 合并多次 insert（缓冲） |
| 连续普攻 | 针对 Hitbox 复用对象池 |
| 事件日志 | 延迟批量写入 CombatLog |

---

## 11. 调试与监控建议

- Debug 绘制：扇形 / 投射物轨迹  
- CombatLog：记录 {time, source, target, skill/attack, damage, crit}  
- 统计：每帧命中次数 / 系统耗时  
- 守卫：在 castSkillSystem 断言配置字段存在性  

---

## 12. 常见扩展路线

| 需求 | 扩展方式 |
|------|----------|
| 链式技能 | 命中后生成新的 SkillCastRequest（限制深度） |
| 追踪投射物 | SkillProjectile 增加 targetId + turnRate |
| 穿透/反弹 | 添加 pierceCount / bounceCount 字段 |
| 分段蓄力 | ChargeState 组件 + 动态选择配置 |
| 属性系统 | AttributeSnapshot 聚合 AttackStats + BuffEffect |
| 元素反应 | 在 damageResolveSystem 挂钩元素标签判定 |
| 死亡事件 | Health 归零时 spawn DeathEvent 组件 |

---

## 13. 实施与迭代策略

1. 最小功能：SkillCast + Projectile + Damage + Health  
2. 加入 ApplyDamageSphere 支持范围技能  
3. 引入普攻链路 (AttackRequest → AttackSwing → Hitbox)  
4. 加入 DOT / Buff / Slow 等状态  
5. 增加连击 / 吸血 / On-Hit  
6. 优化空间查询与批处理性能  
7. CombatLog / 指标监控  
8. 高级技能（链式 / 穿透 / 复合）  

---

## 14. 总结

通过组件化和系统解耦，本架构使：
- 新技能 / 新普攻形态 → 主要新增配置 + 少量特化系统
- 伤害 / 状态 / 时间轴 → 统一数据路径，便于调试
- 可逐步增强性能（空间索引、批处理）而不破坏已有抽象

接下来可按你的项目实际组件（如 Transform、阵营 Faction）补全空间判定与安全校验。如需进一步生成测试用例、属性聚合公式或网络同步策略，继续提出即可。

---