# Lua到TypeScript转换完整指南

## 🎯 转换目标

将 `C:\Users\user\Desktop\Test\KartGame_Engine\sources\roblox-lua-items-network\shared\kartEngine` 中的所有Lua代码转换为TypeScript，保存到 `src/shared/kartEngine`。不用cd到目录，构建命令是pnpm run dev:build;编译命令是pnpm run dev:compile

## 📋 转换清单（共39个文件）

### 阶段1：基础工具类 (KartShared - 11个文件)

```
☐ UnityMath.lua → UnityMath.ts
☐ UnityTime.lua → UnityTime.ts  
☐ RobloxUnityAdapter.lua → RobloxUnityAdapter.ts
☐ MonoBehaviour.lua → MonoBehaviour.ts
☐ UnityInput.lua → UnityInput.ts
☐ UnityCameraFollow.lua → UnityCameraFollow.ts
☐ UnityTrigger.lua → UnityTrigger.ts
☐ UnityEngine/Physics.lua → UnityEngine/Physics.ts
☐ UnityEngine/Ray.lua → UnityEngine/Ray.ts
☐ UnityEngine/RaycastHit.lua → UnityEngine/RaycastHit.ts
☐ UnityEngine/LayerMask.lua → UnityEngine/LayerMask.ts
```

### 阶段2：数据结构和帮助类 (5个文件)

```
☐ BoostKind.lua → BoostKind.ts
☐ Vector3Helper.lua → Vector3Helper.ts
☐ Matrix3.lua → Matrix3.ts
☐ MathHelper.lua → MathHelper.ts
☐ DriveFactor.lua → DriveFactor.ts
```

### 阶段3：核心卡丁车类 (7个文件)·

```
☐ GoKart.lua → GoKart.ts
☐ GoKartBuilder.lua → GoKartBuilder.ts
☐ GoPlayKart.lua → GoPlayKart.ts
☐ KartManager.lua → KartManager.ts
☐ KartBasicController.lua → KartBasicController.ts
☐ RigidbodyFPSWalker.lua → RigidbodyFPSWalker.ts
☐ GoPlayKartBuilder.lua → GoPlayKartBuilder.ts
```

### 阶段4：物理和控制系统 (10个文件)

```
☐ PhysicSpec.lua → PhysicSpec.ts
☐ Suspension.lua → Suspension.ts
☐ DriftControl.lua → DriftControl.ts
☐ DriftGauge.lua → DriftGauge.ts
☐ Control.lua → Control.ts
☐ CollisionState.lua → CollisionState.ts
☐ External.lua → External.ts
☐ FirstPipelineValue.lua → FirstPipelineValue.ts
☐ StuckHelper.lua → StuckHelper.ts
☐ AdBoost.lua → AdBoost.ts
```

### 阶段5：网络和客户端系统 (6个文件)

```
☐ ClientNetworkManager.lua → ClientNetworkManager.ts
☐ NetworkManager.lua → NetworkManager.ts
☐ BasicPositionSync.lua → BasicPositionSync.ts
☐ ClientSkidmarkManager.lua → ClientSkidmarkManager.ts
☐ KartInit.lua → KartInit.ts
☐ CharacterProtection.lua → CharacterProtection.ts
```

## 🔧 标准提示词

### 第一次提示词（分析项目）

```
请分析我的TypeScript项目结构和现有的 src/shared/kartEngine 代码风格，然后开始转换 Lua 代码。

源目录：C:\Users\user\Desktop\Test\KartGame_Engine\sources\roblox-lua-items-network\shared\kartEngine
目标目录：src/shared/kartEngine

请先转换 KartShared 目录下的基础工具类：
1. UnityMath.lua → UnityMath.ts
2. UnityTime.lua → UnityTime.ts

要求：
- 保持所有功能逻辑完全不变
- 使用项目现有的代码风格
- 正确处理模块导入导出
- 添加完整的TypeScript类型注解
```

### 后续提示词（继续转换）

```
已验证前面的转换结果正确。请继续转换下一批文件：

[列出具体要转换的文件]

要求：
- 引用已转换的模块
- 保持类继承关系
- 所有算法逻辑保持一致
- 确保编译无错误
```

## 🔄 语法转换规则

### 类定义

```lua
-- Lua
local BaseClass = require(script.Parent.BaseClass)
local DerivedClass = {}
DerivedClass.__index = DerivedClass
setmetatable(DerivedClass, {__index = BaseClass})

function DerivedClass.new()
    local self = setmetatable(BaseClass.new(), DerivedClass)
    return self
end
```

```typescript
// TypeScript
import { BaseClass } from "../BaseClass";

export class DerivedClass extends BaseClass {
    constructor() {
        super();
    }
}
```

### 单例模式

```lua
-- Lua
local instance_ = nil
function ClassName.GetInstance()
    if instance_ == nil then
        instance_ = ClassName.new()
    end
    return instance_
end
```

```typescript
// TypeScript
export class ClassName {
    private static instance: ClassName | undefined;
  
    public static GetInstance(): ClassName {
        if (!this.instance) {
            this.instance = new ClassName();
        }
        return this.instance;
    }
}
```

### 枚举

```lua
-- Lua
local BoostKind = {
    None = 0,
    StartBoost = 1,
    DriftBoost = 2
}
```

```typescript
// TypeScript
export enum BoostKind {
    None = 0,
    StartBoost = 1,
    DriftBoost = 2
}
```

### 模块导入导出

```lua
-- Lua
local Module = require(script.Parent.ModuleName)
return ModuleName
```

```typescript
// TypeScript
import { Module } from "../ModuleName";
export class ModuleName { }
```

## ✅ 每步验证清单

转换完成后检查：

- [ ] 编译无错误：运行 `npm run build`
- [ ] 导入语句正确
- [ ] 类型注解完整
- [ ] 类继承关系保持
- [ ] 算法逻辑未改变
- [ ] 常量值完全相同

## 🚨 注意事项

1. **索引差异**：Lua使用1-based，TypeScript使用0-based
2. **精度保持**：数学计算必须保持相同精度
3. **循环依赖**：可能需要重构解决
4. **异步操作**：Lua协程转换为Promise
5. **元表机制**：用类继承替代

## 🎯 成功标准

- 所有39个Lua文件成功转换为TypeScript
- 编译通过，无类型错误
- 原有功能完全保持
- 代码风格与项目一致
- 模块依赖关系正确

按照这个指南，分阶段执行，每步验证，即可成功完成转换！
