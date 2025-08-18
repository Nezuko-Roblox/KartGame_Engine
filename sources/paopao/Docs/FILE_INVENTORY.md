# 跑跑卡丁车项目文件清单

## 文件分布统计

本文档提供项目中所有文件的详细清单，按功能模块分类整理。

### 按模块统计

| 模块 | 文件数量 | 占比 | 主要功能 |
|------|----------|------|----------|
| **Misc/** | 130 | 21.5% | 杂项和工具文件 |
| **GUI/** | 120 | 19.9% | 用户界面系统 |
| **Kart/** | 54 | 8.9% | 卡丁车核心系统 |
| **Core/** | 35 | 5.8% | 核心游戏框架 |
| **Network/** | 29 | 4.8% | 网络通信系统 |
| **Items/** | 26 | 4.3% | 道具系统 |
| **Quest/** | 25 | 4.1% | 任务系统 |
| **Platform/** | 16 | 2.6% | 平台集成 |
| **Camera/** | 13 | 2.2% | 摄像机控制 |
| **Input/** | 12 | 2.0% | 输入处理 |
| **Types/** | 11 | 1.8% | 类型定义 |
| **Exceptions/** | 10 | 1.7% | 异常处理 |
| **Game/** | 10 | 1.7% | 游戏逻辑 |
| **Ranking/** | 10 | 1.7% | 排行榜系统 |
| **Utils/** | 9 | 1.5% | 工具类 |
| **States/** | 8 | 1.3% | 状态管理 |
| **Alerts/** | 7 | 1.2% | 警告系统 |
| **Fia/** | 7 | 1.2% | Fia服务 |
| **Messages/** | 7 | 1.2% | 消息系统 |
| **Track/** | 7 | 1.2% | 赛道系统 |
| **Data/** | 6 | 1.0% | 数据结构 |
| **Effects/** | 6 | 1.0% | 视觉效果 |
| **Character/** | 6 | 1.0% | 角色系统 |
| **Speed/** | 6 | 1.0% | 速度控制 |
| **Store/** | 5 | 0.8% | 商店系统 |
| **Assets/** | 4 | 0.7% | 资源管理 |
| **Controllers/** | 4 | 0.7% | 通用控制器 |
| **Registry/** | 4 | 0.7% | 注册表 |
| **Audio/** | 3 | 0.5% | 音频系统 |
| **Serialization/** | 3 | 0.5% | 序列化 |
| **Boost/** | 2 | 0.3% | 加速系统 |
| **Graphics/** | 2 | 0.3% | 图形处理 |
| **Interfaces/** | 2 | 0.3% | 接口定义 |
| **Threading/** | 2 | 0.3% | 多线程 |
| **Collections/** | 1 | 0.2% | 集合类 |
| **External/** | 1 | 0.2% | 外部服务 |
| **Properties/** | 1 | 0.2% | 程序集属性 |

**总计: 604个C#文件**

## 重点模块文件说明

### 1. GUI模块 (120个文件)
用户界面系统，包含游戏所有UI组件：

**核心组件:**
- GUIBase.cs - GUI基础框架
- GUIController.cs - GUI控制器
- GUIManager.cs - GUI管理器

**HUD组件:**
- GUISpeed.cs - 速度显示
- GUITachometer.cs - 转速表
- GUIMinimap.cs - 小地图
- GUIBoosterGauge.cs - 加速条

**界面面板:**
- GUIMain.cs - 主菜单
- GUIGarage.cs - 车库界面
- GUIResult.cs - 结果界面
- GUILoading.cs - 加载界面

### 2. Kart模块 (54个文件)
卡丁车核心系统，游戏最重要的模块：

**物理系统 (Physics/):**
- DriftControl.cs - 漂移控制
- Suspension.cs - 悬挂系统
- PhysicSpec.cs - 物理规格
- Skidmarks.cs - 轮胎痕迹

**AI系统 (AI/):**
- AIController.cs - AI控制器
- AISpeedEnhancer.cs - AI速度增强

**控制器 (Controllers/):**
- KartBasicController.cs - 基础控制
- KartManager.cs - 卡丁车管理器

**记录系统 (Records/):**
- GhostController.cs - 幽灵控制
- GhostRecordManager.cs - 记录管理

### 3. Network模块 (29个文件)
网络通信系统，支持多人游戏：

**数据包 (Packets/):**
- GameControlPacket.cs - 游戏控制数据包
- PlayerPacket.cs - 玩家数据包
- ItemPacket.cs - 道具数据包

**会话管理 (Session/):**
- Session.cs - 游戏会话
- SessionManager.cs - 会话管理器

**WiFi连接 (Wifi/):**
- WifiStage.cs - WiFi阶段
- WifiController.cs - WiFi控制器

### 4. Items模块 (26个文件)
道具系统，实现各种游戏道具：

**道具对象 (GameObjects/):**
- GoItemBanana.cs - 香蕉道具
- GoItemWaterBomb.cs - 水炸弹
- GoItemMissile.cs - 导弹道具

**道具效果:**
- ItemEffectController.cs - 效果控制器
- ItemBox.cs - 道具箱

### 5. Core模块 (35个文件)
核心游戏框架：

**游戏阶段 (Stages/):**
- GameStage.cs - 游戏阶段
- MainMenuStage.cs - 主菜单阶段
- LoadingStage.cs - 加载阶段

**初始化 (Initialization/):**
- Initialization.cs - 游戏初始化
- InitScene.cs - 初始场景

## 开发重点文件

### 必须理解的核心文件
1. **Core/GameStage.cs** - 游戏主循环
2. **Kart/Controllers/KartBasicController.cs** - 卡丁车控制核心
3. **Kart/Physics/DriftControl.cs** - 漂移系统实现
4. **GUI/Core/GUIBase.cs** - UI框架基础
5. **Network/Controllers/NetController.cs** - 网络同步核心

### 特色系统实现
1. **漂移系统**: Kart/Physics/目录下的漂移相关文件
2. **AI系统**: Kart/AI/目录下的智能对手实现
3. **道具系统**: Items/目录下的完整道具机制
4. **多人同步**: Network/目录下的网络架构

---
*最后更新: 2025年6月10日*
