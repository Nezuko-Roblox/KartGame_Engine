# 敌人生成系统

这是一个基于 Matter.js ECS 架构的敌人生成系统，提供了灵活的敌人生成和管理功能。

## 功能特性

- **敌人组件系统**：基于ECS架构的敌人实体管理
- **AI状态管理**：支持idle、patrol、chase、attack、dead等状态
- **动态生成**：支持延迟生成、批量生成、区域生成
- **等级系统**：敌人属性随等级自动调整
- **巡逻系统**：支持预设巡逻路径
- **死亡处理**：自动清理死亡敌人并播放效果

## 文件结构

```
src/shared/ecs/
├── components/enemy/
│   ├── enemy.ts                 # 敌人基础组件
│   ├── enemy-ai.ts             # AI状态组件  
│   ├── enemy-spawn-request.ts  # 生成请求组件
│   └── index.ts                # 组件导出
├── constants/
│   └── enemy-config.ts         # 敌人配置
└── utils/
    └── spawn-enemy-utils.ts    # 生成辅助函数

src/server/ecs/
├── systems/
│   └── spawn-enemies.ts        # 敌人生成系统
└── examples/
    └── enemy-spawn-examples.ts # 使用示例
```

## 组件说明

### Enemy 组件
存储敌人的基础属性：
- `type`: 敌人类型
- `level`: 敌人等级  
- `maxHealth`: 最大生命值
- `attackPower`: 攻击力
- `moveSpeed`: 移动速度
- `attackRange`: 攻击范围
- `detectionRange`: 检测范围

### EnemyAI 组件
管理敌人AI状态：
- `currentState`: 当前AI状态 (idle/patrol/chase/attack/dead)
- `targetId`: 目标实体ID
- `lastStateChange`: 上次状态改变时间
- `nextThinkTime`: 下次思考时间
- `patrolPoints`: 巡逻点列表
- `currentPatrolIndex`: 当前巡逻点索引

### EnemySpawnRequest 组件
生成请求信息：
- `position`: 生成位置
- `enemyType`: 敌人类型
- `level`: 敌人等级（可选）
- `patrolPoints`: 巡逻点列表（可选）
- `spawnDelay`: 生成延迟时间（可选）
- `requestTime`: 请求生成的时间

## 敌人配置

在 `enemy-config.ts` 中预定义了几种敌人类型：

```typescript
// 配置示例
goblin: {
    attackPower: 15,
    attackRange: 3, 
    detectionRange: 15,
    maxHealth: 100,
    modelName: "GoblinModel",
    moveSpeed: 8,
    type: "goblin",
}
```

支持的敌人类型：
- `goblin`: 小怪，血量低但速度快
- `orc`: 中等怪物，平衡属性
- `skeleton`: 亡灵，攻击力高
- `bossDragon`: Boss，血量和攻击力都很高

## 使用方法

### 1. 基础生成

```typescript
import { spawnEnemy } from "shared/ecs/utils/spawn-enemy-utils";

// 生成一个2级哥布林
spawnEnemy(world, new Vector3(10, 0, 10), "goblin", 2);
```

### 2. 带巡逻的敌人

```typescript
const patrolPoints = [
    new Vector3(0, 0, 0),
    new Vector3(10, 0, 0), 
    new Vector3(10, 0, 10),
    new Vector3(0, 0, 10),
];

spawnEnemy(
    world,
    new Vector3(5, 0, 5),
    "orc",
    3, // 等级
    patrolPoints,
    2 // 2秒后生成
);
```

### 3. 区域批量生成

```typescript  
import { spawnEnemiesInArea } from "shared/ecs/utils/spawn-enemy-utils";

// 在指定区域生成5个随机敌人
spawnEnemiesInArea(
    world,
    new Vector3(50, 0, 50), // 中心位置
    20, // 半径
    ["goblin", "skeleton", "orc"], // 可能的敌人类型
    5, // 数量
    1, // 最低等级
    3 // 最高等级  
);
```

### 4. 生成Boss

```typescript
import { spawnBoss } from "shared/ecs/utils/spawn-enemy-utils";

spawnBoss(world, new Vector3(100, 0, 100), "bossDragon", 5);
```

### 5. 波次生成示例

```typescript
import { spawnWave } from "server/ecs/examples/enemy-spawn-examples";

// 生成第3波敌人
spawnWave(world, 3);
```

## 系统集成

要使用敌人生成系统，需要将 `spawn-enemies.ts` 系统添加到服务器的系统列表中：

```typescript
// 在服务器系统配置中
import spawnEnemiesSystem from "server/ecs/systems/spawn-enemies";

const systems = [
    // ...其他系统
    spawnEnemiesSystem,
];
```

## 模型要求

系统需要在 `ReplicatedStorage` 中创建一个名为 `EnemyModels` 的文件夹，包含敌人模型：

```
ReplicatedStorage/
└── EnemyModels/
    ├── GoblinModel
    ├── OrcModel  
    ├── SkeletonModel
    └── DragonModel
```

每个模型应该：
- 设置合适的 `PrimaryPart` 或至少包含一个 `BasePart`
- 包含必要的动画和效果
- 适当的碰撞设置

## 扩展功能

### 添加新敌人类型

1. 在 `enemy-config.ts` 中添加配置
2. 在 `ReplicatedStorage/EnemyModels` 中添加对应模型
3. 可选：为新敌人类型添加特殊AI行为

### 自定义AI行为

系统提供了基础的AI状态管理，可以扩展：
- 创建额外的AI系统处理specific状态
- 添加新的AI状态类型
- 实现复杂的行为树

### 性能优化

- 系统自动处理死亡敌人的清理
- 支持延迟生成避免性能峰值
- 可以添加敌人池化机制进一步优化

## 注意事项

1. 确保在服务器端运行生成系统
2. 模型文件夹结构必须正确
3. 等级系统会自动调整敌人属性（每级+15%）
4. AI系统需要配合移动和攻击系统使用
5. 建议为不同关卡设置不同的敌人配置

这个系统为您的游戏提供了完整的敌人生成和管理基础，可以根据具体需求进行定制和扩展。
