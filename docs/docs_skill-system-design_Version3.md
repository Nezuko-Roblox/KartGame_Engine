# 技能系统设计文档 (ECS / Matter 架构示例，无屏障/阻挡特定实现)

本版本不包含任何带“风墙”字样的元素，也移除了具体的屏障/阻挡实现示例，聚焦通用的范围、单体、投射物、持续效果等核心技能形态。后续若需要再加入“可阻挡”类机制，可在扩展章节按提示补充。

---

## 目录
1. 总体目标
2. 设计原则
3. 技能类型分类
4. 核心数据与组件
5. 技能配置（Config）结构
6. 技能释放全流程
7. 系统职责拆分
8. 数据驱动与 Handler 策略
9. 范围即时技能
10. 投射物命中后技能
11. 单体指向技能
12. 复合技能（投射物 + 爆炸 + DOT）
13. 持续效果（DOT / Buff / Debuff）
14. 冷却与资源系统
15. 伤害结算流水线
16. 目录结构建议
17. 代码示例合集
18. 性能与扩展建议
19. 后续扩展方向
20. 总结

---

## 1. 总体目标

- 数据驱动：技能行为以配置驱动，减少硬编码。
- 低耦合：释放系统仅分发，不直接做数值与动画细节。
- 可组合：一个技能可同时产出多个效果（爆炸 + DOT + 减速）。
- 易扩展：新增技能主要是新增配置或 Handler。

---

## 2. 设计原则

| 原则 | 说明 |
|------|------|
| 单一职责 | 每个系统处理单一类别行为（投射物移动 / 范围伤害 / 持续伤害）。 |
| 不可变输入，可变状态 | 输入通过请求组件进入，系统加工产出状态变化。 |
| 临时组件短生命周期 | 即时范围伤害等组件用后即删，降低状态冗余。 |
| 可观测性 | 关键阶段（命中、结算）可发事件或插入日志组件。 |

---

## 3. 技能类型分类

| 类型 | 示例 | 说明 |
|------|------|------|
| Area | 爆炸、范围治疗 | 释放后立即生效（或延迟一次性生效）。 |
| Target | 单体治疗、点对点伤害 | 需要目标实体。 |
| Projectile | 火球、箭矢 | 飞行命中后处理效果。 |
| Composite | 火球命中后爆炸 + DOT | 通过配置组合多个子效果。 |
| OverTime | 光环、持续伤害、持续治疗 | 周期性产生子效果。 |

---

## 4. 核心数据与组件

| 组件 | 作用 |
|------|------|
| SkillCastRequest | 施放入口。 |
| SkillProjectile | 投射物运行数据。 |
| ApplyDamageSphere | 一次性范围伤害输入。 |
| TakeDamage | 待结算单体伤害。 |
| Health | 生命值。 |
| DotEffect | 持续伤害状态。 |
| BuffEffect / SlowEffect (示意) | 统计/移动相关状态变更。 |
| Cooldown | 技能冷却。 |
| Resource | 法力/能量等资源。 |

---

## 5. 技能配置（Config）结构

```typescript
export const SkillConfigs = {
  Fireball: {
    type: "Projectile",
    speed: 32,
    damage: 60,
    radiusOnHit: 8,
    damageType: "Fire",
    cooldown: 3,
    explodeOnHit: true,
    canApplyDot: true,
    dot: { damagePerTick: 5, interval: 0.5, ticks: 6 },
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
    heal: 40,
    cooldown: 6,
  },
};
```

可扩展字段示例：
- delay：延迟生效
- pulses：多段（数组定义每段时间与伤害系数）
- pierceCount：投射物穿透次数
- maxLifeTime：最大存活时长
- status: { apply: ["Burn","Slow"], chance: 0.3 }

---

## 6. 技能释放全流程

1. 输入 → 构造 SkillCastRequest
2. castSkillSystem 查询配置 → 生成对应效果（投射物 / 范围 / 单体）
3. projectileSystem 驱动飞行（如适用）
4. 命中后产出：ApplyDamageSphere 或直接 TakeDamage / DotEffect
5. applyDamageSystem → TakeDamage
6. damageResolveSystem → 修改 Health
7. dotSystem / buffSystem → 周期处理
8. cleanupSystem 移除过期或结束的组件

---

## 7. 系统职责拆分

| 系统 | 职责 |
|------|------|
| skillEventListener | 外部事件→请求组件。 |
| castSkillSystem | 解析请求，产出初始效果组件。 |
| projectileSystem | 投射物移动、寿命、命中判定。 |
| applyDamageSystem | 范围组件→TakeDamage。 |
| damageResolveSystem | TakeDamage → Health 更新。 |
| dotSystem | DotEffect 周期触发。 |
| buffSystem | BuffEffect 生命周期、属性调整。 |
| cooldownSystem | 生成与检查 Cooldown。 |
| cleanupSystem | 清理过期状态。 |

---

## 8. 数据驱动与 Handler 策略

层次：
1. 单纯配置型：无需专用代码逻辑
2. 可选参数组合：例如 explodeOnHit + dot
3. 特殊技能（链式闪电、瞬移）：使用 SkillHandlers 注册函数扩展

```typescript
export const SkillHandlers = {
  Blink: (world, ctx) => {/* 位移逻辑 */},
  ChainLightning: (world, ctx) => {/* 多跳逻辑 */},
};
```

---

## 9. 范围即时技能

- 请求 → 读取配置 → 直接生成 ApplyDamageSphere
- 系统处理后立即删除
- 若有延迟：可先生成 PendingAreaAttack { triggerTime }，到时再转化为 ApplyDamageSphere

---

## 10. 投射物命中后技能

- 投射物包含：位置、方向、速度、伤害、命中行为、寿命
- 命中：
  - explodeOnHit → 生成 ApplyDamageSphere
  - 单体直伤 → 插入 TakeDamage
  - canApplyDot → 给命中目标插入 DotEffect

---

## 11. 单体指向技能

必要数据：skillName + caster + target  
可扩展：距离校验、视线校验、目标合法性（阵营/状态）

---

## 12. 复合技能示例

Fireball：
1. castSkillSystem 生成 SkillProjectile
2. projectileSystem 命中 → explodeOnHit → ApplyDamageSphere
3. applyDamageSystem → 多个目标 TakeDamage
4. damageResolveSystem → 改变生命
5. 如果 canApplyDot → 对每个命中目标附加 DotEffect

---

## 13. 持续效果（DOT / Buff / Debuff）

DotEffect：
```typescript
{ damagePerTick, interval, ticksRemaining, nextTime, source }
```
dotSystem：
- 检查 nextTime ≤ now → 生成 TakeDamage → ticksRemaining--
- ticksRemaining == 0 → 移除

BuffEffect：
```typescript
{ stat: "MoveSpeed", delta: 4, duration, age }
```
在属性聚合/移动系统读取 BuffEffect 汇总。

---

## 14. 冷却与资源系统

- 冷却生成：释放成功后插入 Cooldown
- 检查：castSkillSystem 遍历 caster 的 Cooldown 组件或建立索引
- 资源：
  - Resource { mana }
  - 施放前检查 cost ≤ mana
  - 扣减后写回 Resource

---

## 15. 伤害结算流水线

1. 产生伤害（TakeDamage 插入）
2. 结算系统（damageResolveSystem）：
   - 读取防御/减伤（可扩展：Armor、Resistance）
   - 计算最终伤害
   - 更新 Health
   - 生命归零触发死亡事件（可插入 DeathEvent）

---

## 16. 目录结构建议

```
src/
  shared/
    config/
      skills.ts
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
    skills/
      handlers/
        chainLightning.ts
        blink.ts
      skillHandlers.ts
  server/
    systems/
      skillEventListener.ts
      castSkillSystem.ts
      projectileSystem.ts
      applyDamageSystem.ts
      damageResolveSystem.ts
      dotSystem.ts
      buffSystem.ts
      cooldownSystem.ts
      cleanupSystem.ts
```

---

## 17. 代码示例合集

### 17.1 技能配置

```typescript name=src/shared/config/skills.ts
export const SkillConfigs = {
  Fireball: {
    type: "Projectile",
    speed: 32,
    damage: 60,
    radiusOnHit: 8,
    damageType: "Fire",
    cooldown: 3,
    explodeOnHit: true,
    canApplyDot: true,
    dot: { damagePerTick: 5, interval: 0.5, ticks: 6 },
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
    heal: 40,
    cooldown: 6,
  },
};
export type SkillName = keyof typeof SkillConfigs;
```

### 17.2 请求组件

```typescript name=src/shared/components/skillCastRequest.ts
import { AnyEntity, component } from "@rbxts/matter";

export const SkillCastRequest = component<{
  skillName: string;
  caster: AnyEntity;
  position?: Vector3;
  direction?: Vector3;
  target?: AnyEntity;
  extra?: Record<string, unknown>;
}>("SkillCastRequest");
```

### 17.3 投射物组件

```typescript name=src/shared/components/skillProjectile.ts
import { AnyEntity, component } from "@rbxts/matter";

export const SkillProjectile = component<{
  position: Vector3;
  direction: Vector3;
  speed: number;
  caster: AnyEntity;
  damage: number;
  damageType: string;
  radiusOnHit?: number;
  explodeOnHit?: boolean;
  canApplyDot?: boolean;
  dot?: { damagePerTick: number; interval: number; ticks: number };
  lifeTime: number;
  age: number;
  hit?: boolean;
}>("SkillProjectile");
```

### 17.4 范围伤害组件

```typescript name=src/shared/components/damage/applyDamageSphere.ts
import { component } from "@rbxts/matter";

export const ApplyDamageSphere = component<{
  position: Vector3;
  radius: number;
  damage: number;
  damageType: string;
  source?: number;
}>("ApplyDamageSphere");
```

### 17.5 伤害、生命

```typescript name=src/shared/components/damage/takeDamage.ts
import { AnyEntity, component } from "@rbxts/matter";

export const TakeDamage = component<{
  amount: number;
  damageType: string;
  inflictor?: AnyEntity;
}>("TakeDamage");
```

```typescript name=src/shared/components/damage/health.ts
import { component } from "@rbxts/matter";

export const Health = component<{
  current: number;
  max: number;
}>("Health");
```

### 17.6 DOT 组件

```typescript name=src/shared/components/effects/dotEffect.ts
import { AnyEntity, component } from "@rbxts/matter";

export const DotEffect = component<{
  damagePerTick: number;
  interval: number;
  ticksRemaining: number;
  nextTime: number;
  source?: AnyEntity;
  damageType: string;
}>("DotEffect");
```

### 17.7 冷却与资源

```typescript name=src/shared/components/meta/cooldown.ts
import { component } from "@rbxts/matter";
export const Cooldown = component<{
  skillName: string;
  endTime: number;
  caster: number;
}>("Cooldown");
```

```typescript name=src/shared/components/meta/resource.ts
import { component } from "@rbxts/matter";
export const Resource = component<{
  mana: number;
  energy?: number;
}>("Resource");
```

### 17.8 事件监听（示例）

```typescript name=src/server/systems/skillEventListener.ts
import { World } from "@rbxts/matter";
import { SkillCastRequest } from "shared/components/skillCastRequest";

export function pushSkillCast(world: World, args: {
  skillName: string;
  caster: number;
  position?: Vector3;
  direction?: Vector3;
  target?: number;
}) {
  world.spawn(SkillCastRequest(args));
}
```

### 17.9 施放系统

```typescript name=src/server/systems/castSkillSystem.ts
import { World } from "@rbxts/matter";
import { SkillCastRequest } from "shared/components/skillCastRequest";
import { SkillConfigs } from "shared/config/skills";
import { SkillProjectile } from "shared/components/skillProjectile";
import { ApplyDamageSphere } from "shared/components/damage/applyDamageSphere";

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
          // 直接治疗示例：可插入 ApplyHeal 或直接修改 Health（建议统一组件）
        }
        break;
    }
    world.remove(id, SkillCastRequest);
  }
};
```

### 17.10 投射物系统

```typescript name=src/server/systems/projectileSystem.ts
import { World } from "@rbxts/matter";
import { SkillProjectile } from "shared/components/skillProjectile";
import { ApplyDamageSphere } from "shared/components/damage/applyDamageSphere";
import { DotEffect } from "shared/components/effects/dotEffect";

const dt = () => 1 / 60;

export = (world: World) => {
  for (const [id, proj] of world.query(SkillProjectile)) {
    if (proj.hit) continue;

    const newPos = proj.position.add(proj.direction.mul(proj.speed * dt()));
    const hitEntity = raycastForEntity(proj.position, newPos); // TODO: 实现

    // 超时
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
        // 单体伤害：插入 TakeDamage（略）
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

### 17.11 范围伤害系统

```typescript name=src/server/systems/applyDamageSystem.ts
import { World } from "@rbxts/matter";
import { ApplyDamageSphere } from "shared/components/damage/applyDamageSphere";
import { TakeDamage } from "shared/components/damage/takeDamage";

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

### 17.12 伤害结算

```typescript name=src/server/systems/damageResolveSystem.ts
import { World } from "@rbxts/matter";
import { TakeDamage } from "shared/components/damage/takeDamage";
import { Health } from "shared/components/damage/health";

export = (world: World) => {
  for (const [id, dmg] of world.query(TakeDamage)) {
    const health = world.get(id, Health)[0];
    if (health) {
      const newHP = math.max(0, health.current - dmg.amount);
      world.insert(id, Health({ ...health, current: newHP }));
      // 可插入：死亡检测 / 战斗日志
    }
    world.remove(id, TakeDamage);
  }
};
```

### 17.13 DOT 系统

```typescript name=src/server/systems/dotSystem.ts
import { World } from "@rbxts/matter";
import { DotEffect } from "shared/components/effects/dotEffect";
import { TakeDamage } from "shared/components/damage/takeDamage";

export = (world: World) => {
  const now = os.clock();
  for (const [id, dot] of world.query(DotEffect)) {
    if (now >= dot.nextTime) {
      world.insert(id, TakeDamage({
        amount: dot.damagePerTick,
        damageType: dot.damageType,
        inflictor: dot.source,
      }));
      const remaining = dot.ticksRemaining - 1;
      if (remaining <= 0) {
        world.remove(id, DotEffect);
      } else {
        world.insert(id, DotEffect({
          ...dot,
          ticksRemaining: remaining,
          nextTime: now + dot.interval,
        }));
      }
    }
  }
};
```

### 17.14 清理（示例：若还需专门处理其他过期组件）

```typescript name=src/server/systems/cleanupSystem.ts
import { World } from "@rbxts/matter";
// 此处可遍历有 duration/age 字段的组件统一处理
export = (world: World) => {
  // 留作扩展
};
```

---

## 18. 性能与扩展建议

| 主题 | 建议 |
|------|------|
| 空间查询 | 用网格或八叉树加速范围查询，避免全量遍历。 |
| 投射物数量 | 低优先级投射物可降采样检测频率。 |
| 组件写入 | 合并多次 insert，减少频繁对象重建。 |
| 内存管理 | 瞬时组件（ApplyDamageSphere）及时移除。 |
| 调试 | 建立技能日志（SkillLog）或事件派发。 |

---

## 19. 后续扩展方向

| 方向 | 思路 |
|------|------|
| 复杂弹道 | 曲线、跟踪、加速度字段。 |
| 多段引导 | Channel 组件 + tick 生成子效果。 |
| 属性系统 | Attribute 聚合：基础值 + BuffEffect 修改。 |
| 战斗公式 | (Attack - Defense) * 系数 + 暴击处理。 |
| 事件钩子 | OnCast / OnHit / OnKill / OnExpire 事件组件化。 |
| 组合生成器 | 根据配置 DSL 生成 SkillHandlers。 |
| 网络防作弊 | 服务器重算合法方向/距离，丢弃异常请求。 |

---

## 20. 总结

本设计将技能逻辑拆成“请求→分发→效果→结算”流水线：
- 通过配置与少量 Handler 实现扩展
- 投射物与范围伤害清晰分离
- 持续效果与瞬时效果并存
- 组件简洁、系统职责明确

后续只需：
1. 扩展配置字段
2. 添加必要组件
3. 编写新系统或复用 Handler

即可快速迭代大量技能类型。

若需要我进一步生成某一具体技能的完整测试用例或引入日志/调试模块，请继续提出需求。
