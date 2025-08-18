# 跑跑卡丁车核心引擎 (Roblox版)

## 项目概述

这是一个从Unity迁移到Roblox平台的跑跑卡丁车游戏引擎，包含完整的物理模拟、网络同步、AI系统等核心功能。项目采用模块化架构设计，支持多人在线对战。

## 项目结构

```
kart-game-engine/
├── docs/                              # 技术文档
│   ├── KartPhysicsEngine.md          # 物理引擎设计文档
│   ├── Unity2RobloxMigrationPlan.md  # Unity到Roblox迁移计划
│   ├── Unity网络同步机制详细分析.md    # Unity网络同步分析
│   ├── Unity赛车网络同步技术分析.md    # 赛车网络同步技术
│   ├── Roblox卡丁车网络同步完整实现方案.md # Roblox网络同步方案
│   ├── item-system-documentation.md  # 道具系统文档
│   ├── 完整赛车参数技术文档.md        # 赛车参数配置
│   └── 迁移.md                       # 迁移指南
│
├── sources/                           # Unity源代码（C#）
│   ├── kart-game/                    # 卡丁车游戏源码
│   └── paopao/                       # 跑跑卡丁车原始代码
│       ├── Alerts/                   # 警报系统
│       ├── Assets/                   # 资源管理
│       ├── Audio/                    # 音频系统
│       ├── Boost/                    # 加速系统
│       ├── Camera/                   # 摄像机系统
│       ├── Character/                # 角色系统
│       ├── Controllers/              # 控制器
│       ├── Core/                     # 核心系统
│       ├── Data/                     # 数据结构
│       ├── Effects/                  # 特效系统
│       ├── GUI/                      # 图形界面
│       ├── Game/                     # 游戏逻辑
│       ├── Input/                    # 输入系统
│       ├── Items/                    # 道具系统
│       ├── Kart/                     # 卡丁车核心
│       ├── Network/                  # 网络系统
│       ├── Platform/                 # 平台相关
│       ├── Quest/                    # 任务系统
│       ├── Ranking/                  # 排行榜系统
│       ├── Speed/                    # 速度控制
│       ├── States/                   # 状态管理
│       ├── Store/                    # 商店系统
│       ├── Track/                    # 赛道系统
│       └── Utils/                    # 工具类
│
├── src/                               # Roblox实现（Lua）
│   ├── core/                         # 核心引擎模块
│   │   ├── Boost/                   # 加速系统
│   │   │   └── AdBoost.lua         # 广告加速
│   │   ├── GameStage/               # 游戏阶段管理
│   │   │   └── GoPlayKartBuilder.lua # 卡丁车构建器
│   │   ├── KartClient/              # 客户端系统
│   │   │   ├── BasicPositionSync.lua      # 基础位置同步
│   │   │   ├── ClientNetworkManager.lua   # 客户端网络管理
│   │   │   ├── ClientSkidmarkManager.lua  # 轮胎痕迹管理
│   │   │   ├── DirectPositionSync.lua     # 直接位置同步
│   │   │   ├── KartInit.lua              # 卡丁车初始化
│   │   │   ├── LagCompensatedSync.lua    # 延迟补偿同步
│   │   │   ├── NetworkSyncManager.lua    # 网络同步管理器
│   │   │   ├── OptimizedNetworkSync.lua  # 优化网络同步
│   │   │   ├── PredictiveNetworkSync.lua # 预测性网络同步
│   │   │   └── TweenSync.lua            # 补间同步
│   │   ├── KartMove/                # 移动系统
│   │   │   ├── BoostKind.lua       # 加速类型
│   │   │   ├── CollisionState.lua  # 碰撞状态
│   │   │   ├── Control.lua         # 控制输入
│   │   │   ├── DriftControl.lua    # 漂移控制
│   │   │   ├── DriftGauge.lua      # 漂移量表
│   │   │   ├── DriveFactor.lua     # 驾驶因子
│   │   │   ├── External.lua        # 外部接口
│   │   │   ├── FirstPipelineValue.lua # 管线值
│   │   │   ├── GoKart.lua          # 卡丁车基类
│   │   │   ├── GoKartBuilder.lua   # 卡丁车构建器
│   │   │   ├── GoPlayKart.lua      # 可玩卡丁车
│   │   │   ├── KartBasicController.lua # 基础控制器
│   │   │   ├── KartManager.lua     # 卡丁车管理器
│   │   │   ├── MathHelper.lua      # 数学辅助
│   │   │   ├── Matrix3.lua         # 3x3矩阵
│   │   │   ├── PhysicSpec.lua      # 物理参数
│   │   │   ├── RigidbodyFPSWalker.lua # 刚体FPS控制
│   │   │   ├── StuckHelper.lua     # 卡住检测
│   │   │   ├── Suspension.lua      # 悬挂系统
│   │   │   └── Vector3Helper.lua   # 向量辅助
│   │   ├── KartServer/              # 服务器系统
│   │   │   ├── CharacterProtection.lua # 角色保护
│   │   │   └── NetworkManager.lua      # 网络管理器
│   │   └── KartShared/              # 共享模块
│   │       ├── MonoBehaviour.lua    # Unity行为模拟
│   │       ├── RobloxUnityAdapter.lua # Unity适配器
│   │       ├── UnityCameraFollow.lua  # 摄像机跟随
│   │       ├── UnityEngine/         # Unity引擎模拟
│   │       │   ├── LayerMask.lua   # 层级掩码
│   │       │   ├── Physics.lua     # 物理系统
│   │       │   ├── Ray.lua         # 射线
│   │       │   └── RaycastHit.lua  # 射线检测
│   │       ├── UnityInput.lua      # 输入系统
│   │       ├── UnityMath.lua       # 数学库
│   │       ├── UnityTime.lua       # 时间系统
│   │       └── UnityTrigger.lua    # 触发器
│   ├── dev/                         # 开发入口
│   │   ├── ServerScriptService/     # 服务器脚本
│   │   │   └── KartServerInit.server.lua # 服务器初始化
│   │   └── StarterPlayerScripts/    # 客户端脚本
│   │       └── KartClientInit.client.lua # 客户端初始化
│   └── tests/                       # 测试代码
│
└── default.project.json             # Rojo项目配置
```

## 核心功能

### 🎮 物理引擎
- **真实物理模拟**: 完整的卡丁车物理系统，包括悬挂、抓地力、漂移等
- **漂移系统**: 精确的漂移控制，支持多级漂移加速
- **碰撞检测**: 高效的碰撞检测和响应系统

### 🌐 网络同步
- **多种同步策略**: 
  - 基础位置同步
  - 直接位置同步
  - 延迟补偿同步
  - 预测性同步
  - 优化网络同步
- **客户端预测**: 减少延迟感的客户端预测机制
- **服务器权威**: 服务器端权威验证，防止作弊

### 🤖 AI系统
- **智能AI**: 具有不同难度等级的AI对手
- **路径寻找**: 智能赛道路径规划
- **动态难度**: 根据玩家水平调整AI难度

### 🎨 游戏特性
- **道具系统**: 丰富的游戏道具（香蕉、水炸弹、导弹等）
- **加速系统**: 多种加速机制（漂移加速、道具加速、广告加速）
- **排行榜系统**: 完整的积分和排行榜功能
- **任务系统**: 多样化的游戏任务和成就

### 🛠 开发特性
- **Unity到Roblox适配层**: 无缝迁移Unity代码到Roblox
- **模块化架构**: 清晰的模块划分，易于维护和扩展
- **完整文档**: 详细的技术文档和迁移指南

## 快速开始

### 环境要求
- Roblox Studio (最新版本)
- Rojo 7.0+ (用于项目同步)

### 安装步骤

1. **克隆项目**
```bash
git clone https://github.com/yourusername/kart-game-engine.git
cd kart-game-engine
```

2. **安装Rojo**
```bash
cargo install rojo
```

3. **启动Rojo服务**
```bash
rojo serve
```

4. **在Roblox Studio中连接**
- 打开Roblox Studio
- 安装Rojo插件
- 连接到本地服务器（默认端口: 34882）

### 项目配置

项目使用 `default.project.json` 进行配置：
- **ReplicatedStorage/KartEngine**: 核心引擎模块
- **ServerScriptService**: 服务器端脚本
- **StarterPlayerScripts**: 客户端脚本

## 开发指南

### 代码结构
- `sources/`: Unity原始C#代码，作为参考实现
- `src/core/`: Roblox核心引擎模块（Lua）
- `src/dev/`: 开发环境入口脚本
- `docs/`: 详细的技术文档

### 主要模块说明

#### KartMove (移动系统)
负责卡丁车的所有移动相关逻辑，包括：
- 基础移动控制
- 物理参数配置
- 漂移系统
- 碰撞处理

#### KartClient (客户端系统)
处理客户端特定功能：
- 网络同步策略
- 视觉效果（轮胎痕迹等）
- 输入处理

#### KartServer (服务器系统)
服务器端逻辑：
- 权威性验证
- 网络消息分发
- 游戏状态管理

#### KartShared (共享模块)
客户端和服务器共用的代码：
- Unity API适配
- 数学库
- 通用工具

## 技术文档

详细的技术文档请参考 `docs/` 目录：
- [物理引擎设计](docs/KartPhysicsEngine.md)
- [Unity到Roblox迁移计划](docs/Unity2RobloxMigrationPlan.md)
- [网络同步方案](docs/Roblox卡丁车网络同步完整实现方案.md)
- [道具系统文档](docs/item-system-documentation.md)
- [完整赛车参数](docs/完整赛车参数技术文档.md)

## 贡献指南

欢迎贡献代码！请遵循以下步骤：

1. Fork 项目
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情

## 联系方式

项目维护者 - [@yourusername](https://github.com/yourusername)

项目链接: [https://github.com/yourusername/kart-game-engine](https://github.com/yourusername/kart-game-engine)