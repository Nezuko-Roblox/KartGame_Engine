# 《鬼灭之刃》章节主线 + Rogue 轻量化混合设计（极简版）
需求变更：设计尽量简单——“每次升级事件弹出三选一供玩家选择”。  
舍弃之前复杂：首发 Draft / 权重偏移 / 保护机制 / 多层 RunTier（5层） / 事件/分支。  
保留：线性章节、关卡波次、‘升级事件’触发点、三选一=（新增角色 或 强化已有角色）。

---

## 1. 目标与原则
| 目标 | 说明 |
|------|------|
| 极简实现 | 一周内可完成雏形；无复杂概率引擎。 |
| 核心反馈清晰 | “升级 → 3 选 1 → 变强”单一循环。 |
| 低策划成本 | 配置尽量少：角色池 / 触发波次 / Tier加成表。 |
| 可自然扩展 | 后期再添加事件卡、reroll、稀有度权重。 |
| 安全可控 | 临时增幅不外溢经济。 |

---

## 2. 基本流程（单关卡内）
1. 进入关卡：直接用玩家自己在“上阵界面”已选定的初始队伍（不做开局 Draft）。  
2. 战斗推进（按波次或击杀） → 触发“升级事件”。  
3. 弹出“三选一”界面：三个候选卡（可能是：某个已上阵角色的强化、一个未上阵角色加入、一个通用属性增益）。  
4. 玩家选中其中一项 → 立即生效 → 继续战斗。  
5. 多次重复（Normal 关 2~3 次，Elite 3 次，Boss 2 次）。  
6. 关卡结算：发放常规掉落；本关临时强化全部清空。

---

## 3. 触发时机（默认配置）
| 关卡类型 | 默认波数 | 升级触发点（波号结束后） | 触发次数 |
|----------|----------|---------------------------|----------|
| Normal (3-4 波) | 3 或 4 | 波1、波2（若有波3则不在末波再触发） | 2 |
| Elite (4 波) | 4 | 波1、波2、波3 | 3 |
| Boss (单波多阶段) | 1（阶段） | Boss HP 降到 70%、40% | 2 |

（简化：不采用经验阈值；后期可追加。）

---

## 4. 三选一候选类型（简化）
| 类型 | 代号 | 说明 | 使用条件 |
|------|------|------|----------|
| 角色强化（Duplicate Tier Up） | TUP | 对当前任意已上阵角色之一+1 Tier | 如果该角色未达 Tier 上限 |
| 新角色加入 | NEW | 将未上阵角色加入空槽（带基础战力） | 有空槽且存在未加入角色 |
| 通用属性增益 | BUFF | 给“全队/单指定职业”加百分比属性或一次性回复 | 任意时刻（兜底） |

注：为“极简”首发仅需保证：  
1) 优先尝试生成：未满 Tier 的已上阵角色强化 + 一个可加入的新角色 + 一个通用 Buff。  
2) 若条件不满足（例如槽位已满且所有角色都满 Tier），则使用不同的 Buff 方案填满 3 个。

---

## 5. 槽位与上阵
| 项 | 值 |
|----|----|
| 上阵槽位最大数 | 3 |
| 初始上阵 | 玩家预先在关卡外界面选好的 1~3 个角色 |
| NEW 选项 | 只在空槽存在时出现（默认最多 1 个 NEW 出现在一个三选集合中） |

---

## 6. Tier（强化层）设计（极简 3 层）
| Tier | 获取方式 | 角色面板展示 | 属性加成 (示例) | 额外效果 |
|------|----------|--------------|-----------------|----------|
| 0 | 初始 | 无标识 | 基础 Meta 属性 | - |
| 1 | 首次强化该角色 | +I 标记 | ATK +12%, HP +10%, 技能系数 +8% | - |
| 2 | 再次强化 | +II 标记 | ATK +26%, HP +22%, 技能系数 +18% | 技能冷却 -1（下限1） |
| 3 (上限) | 再次强化 | +III（高亮） | ATK +40%, HP +34%, 技能系数 +28% | 战斗中首次受致命伤保留1HP（触发后失效） |

说明：Tier 超过上限时不再生成该角色的 TUP 候选。

属性融合： FinalStat = MetaStat * (1 + tierAtkPct / HP 类似)。  
技能倍率增幅直接乘： Damage = BaseDamage * (1 + skillPctByTier)。

---

## 7. 通用 Buff（示例池）
| Buff ID | 类型 | 效果（即时或持续） | 备注 |
|---------|------|--------------------|------|
| B_ATK10 | 持续 | 全队攻击 +10%（可叠加，最多 3 层） | 简单线性 |
| B_HPHEAL20 | 即时 | 立刻为全队回复 20% 最大 HP | 不叠加（多选多次即可） |
| B_DEF12 | 持续 | 全队防御 +12%（最多 2 层） | |
| B_SKILL15 | 持续 | 技能伤害 +15%（最多 2 层） | |
| B_CRIT5 | 持续 | 暴击率 +5%（最多 3 层） | 若存在暴击体系 |
| B_ENERGY1 | 即时 | 所有上阵获得 +1 初始能量（当场生效一次） | 仅在战斗中 |

首发建议准备 6~8 个即可。  
候选生成优先级：  
1) 如果少于 3 个候选位，用不同 Buff 填满；  
2) 不做复杂互斥，只需防止同一次出现完全相同的 Buff。

---

## 8. 候选生成（算法极简）

伪代码：
```
generateChoices(runState):
  choices = []

  // 1. 角色强化候选
  upgradable = [r for r in runState.team if r.tier < TIER_MAX]
  if upgradable not empty:
      pick1 = randomOne(upgradable) -> TUP(pick1)
      choices.append(TUP)

  // 2. 新角色
  if team.size < SLOTS_MAX:
      candidateNew = [owned_not_in_team]
      if candidateNew not empty:
         choices.append( NEW( randomOne(candidateNew) ) )

  // 3. 填满剩余位置
  while len(choices) < 3:
      choices.append( randomDistinctBuff(exclude=already_in_choices) )

  // 排序（可选：固定顺序）
  return choices
```
无权重、无保护、无 reroll。

---

## 9. 交互/UI（最小）

| 步骤 | UI 元素 | 要点 |
|------|---------|------|
| 升级弹窗 | 三张卡 | 卡类型角标：强化 / 新角色 / Buff |
| Hover/点击 | 属性预览 | 强化卡：显示“当前 Tier→下一级加成” |
| 选择后 | 简短特效 | 强化：角色头像冒光；新角色：槽位出现；Buff：浮动文本 |
| HUD | 简洁列表 | 每个上阵角色显示 Tier (I / II / III) |
| 结算面板 | 概览 | 统计：最终队伍 + 获得的 Buff 列表（可选） |

无需复杂动画，优先保证战斗节奏不被阻断（可设置自动暂停再继续）。

---

## 10. 服务端数据结构（Run 内）
```
run_state = {
  "run_id": "r10001",
  "player_id": "u123",
  "stage_id": "CH2_N3",
  "wave_index": 1,
  "team": {
    "char_tanjiro": { "tier": 1 },
    "char_zenitsu": { "tier": 0 }
  },
  "buffs": {
    "B_ATK10": 1,
    "B_HPHEAL20": 0
  },
  "slots_max": 3,
  "choice_log": [
     { "trigger":"wave1_end", "candidates":["TUP:char_tanjiro","NEW:char_inosuke","BUFF:B_ATK10"], "picked":"TUP:char_tanjiro" }
  ]
}
```

---

## 11. 接口（最小）

| 接口 | 描述 | 关键参数 |
|------|------|----------|
| POST /run/start | 初始化 run_state | stage_id |
| POST /run/trigger | 触发升级 → 返回三选一 | run_id |
| POST /run/pick | 提交选择 | run_id, choice_id |
| POST /run/wave_end | 更新 wave_index & 判断是否触发 | run_id, wave_index |
| POST /run/settle | 结算 | run_id, result |

注意：  
- 服务器校验 choice_id 是否在最近一次生成的 choices 列表内。  
- 所有临时加成都不写入永久角色数据。

---

## 12. 日志（埋点）

| 事件 | 字段 | 示例 |
|------|------|------|
| run_start | run_id, stage_id, initial_team | team=["tanjiro"] |
| run_trigger | run_id, trigger_type(wave/phase), wave_index, candidates | ["TUP:tanjiro","NEW:inosuke","BUFF:B_ATK10"] |
| run_pick | run_id, picked, pre_tier, post_tier | TUP:tanjiro 0→1 |
| run_buff_apply | run_id, buff_id, stack_after | B_ATK10,1 |
| run_settle | run_id, result, final_team_tiers, buffs | success |
| run_abort | run_id, reason | player_exit |

---

## 13. 数值 & 平衡（初值建议）

| 项目 | 初值 | 说明 |
|------|------|------|
| Normal 关触发次数 | 2 | 不拖节奏 |
| Elite 关触发次数 | 3 | 因难度稍高 |
| Boss 关触发次数 | 2 | 阶段触发 |
| 新角色出现率（有空槽） | 期望 70% 至少出现一次 | 通过逻辑保障（不需概率） |
| 平均最大 Tier | 1.5~2.0 | 控制爆发不超标 |
| Buff 种类 | 6~8 | 避免重复太多 |
| Heal Buff 间隔 | 不限制（玩家主动策略） | 低复杂度 |

调优观察点：  
- 如果胜率过高：下调 Tier 加成百分比或减少 Elite 触发次数。  
- 如果玩家普遍未体验到 Tier>=2：增加触发次数或允许强化候选优先。

---

## 14. 监控指标

| 指标 | 预期 | 说明 |
|------|------|------|
| run_avg_triggers | Normal≈2.0 / Elite≈3.0 | 低于说明触发节点缺失 |
| run_avg_team_size | 2.0~2.5 | 过低：新角色不足；过高：后期多余 |
| tier_distribution (T3率) | <10% | 高了需砍 Tier 强度或机会 |
| buff_pick_rate_top1 | <35% | >50% 表示某 Buff 过强或设计单调 |
| exit_after_trigger1 | <5% | 高说明体验或数值不佳 |
| avg_decision_time | <7s | >10s UI/信息冗余 |

---

## 15. 调整开关（配置）
| 配置键 | 作用 | 默认 |
|--------|------|------|
| enable_new_role_when_full | 滿槽是否仍生成 NEW | false |
| show_buff_prediction | 显示 Buff 数值叠加预估 | true |
| allow_tier_over_cap_convert_to_buff | Tier 满后转 Buff | false |
| triggers.normal | Normal 波次数组 | [1,2] |
| triggers.elite | Elite 波次数组 | [1,2,3] |
| triggers.boss | Boss 阶段 HP 百分比 | [0.7,0.4] |

---

## 16. QA 核心用例
| 用例 | 步骤 | 期望 |
|------|------|------|
| 初次进入 Normal | 打完波1 | 弹出 3 选 1 |
| 槽位已满 | 继续触发 | 不再出现 NEW，出现 TUP 或 Buff |
| 角色 Tier 达上限 | 再触发强化 | 不再出现该角色 TUP |
| Buff 叠加 | 多次选同 Buff | 计数叠加且不超预设上限 |
| 断线重连 | 中途掉线重连 | run_state 复原（当前波、已选加成） |
| 提交非法 choice_id | 篡改客户端 | 服务器拒绝 |

---

## 17. 后续扩展占位（不在本次实现）
| 扩展 | 描述 |
|------|------|
| Reroll 按钮 | 消耗“Rogue 点数”刷新 3 选 |
| 稀有/传奇候选 | 给更强 Buff，掉落更稀有特效 |
| 诅咒选项 | 选负面换更高 Tier 上限或额外触发机会 |
| 事件插入 | 替代一次普通升级为“剧情事件 + 奖励” |
| 跨关延续模式 | 同章节保留 Tier 累积 |
| 槽位解锁卡 | 三选一中出现“解锁第4槽” |

---

## 18. 玩家可见简易提示（文案）
- “波次结束获得升级机会：从三个选项中选择一个。”
- “选已有角色 = 强化（最多 3 次），选新角色 = 扩充阵容，选增益 = 全队加强。”
- “本关卡结束后强化清空，请尽量推进。”

---

## 19. 最小实施任务拆分
| 模块 | 任务 |
|------|------|
| 服务端 | run_state 管理 / 触发点检测 / 候选生成 / pick 校验 / 结算 |
| 客户端 | 升级弹窗 UI / 选择交互 / Tier 徽章 / Buff 图标 |
| 数值 | tier_table.json & buff_table.json 初稿 |
| QA | 触发频次 & Tier 上限 & Buff 叠加 & 断线用例 |
| 数据 | 埋点 & 看板：胜率 / pick rate |

---

## 20. 总结
该极简方案用“固定波次触发 + 三选一 + 3层强化”实现轻量 Roguelike 体验：  
- 开发量小：无复杂概率、无分支地图。  
- 玩家决策清晰：每次只需判断“强化 / 新角色 / 通用增益”。  
- 平衡可控：三个 Tier，属性线性可调。  
为后续的事件、稀有选项、跨关构筑奠定简单稳定骨架。

（完）