# Lua到TypeScript转换技术细节参考

## 语法对照表

### 基础语法转换

#### 变量声明
```lua
-- Lua
local variable = value
local num = 42
local str = "hello"
local bool = true
```

```typescript
// TypeScript
const variable = value;
const num: number = 42;
const str: string = "hello";
const bool: boolean = true;
```

#### 函数定义
```lua
-- Lua
function ClassName:methodName(param1, param2)
    return result
end

local function localFunction(param)
    return param * 2
end
```

```typescript
// TypeScript
public methodName(param1: ParamType, param2: ParamType): ReturnType {
    return result;
}

private localFunction(param: number): number {
    return param * 2;
}
```

#### 类定义和继承
```lua
-- Lua
local BaseClass = require(script.Parent.BaseClass)
local DerivedClass = {}
DerivedClass.__index = DerivedClass
setmetatable(DerivedClass, {__index = BaseClass})

function DerivedClass.new()
    local self = setmetatable(BaseClass.new(), DerivedClass)
    self.property = value
    return self
end

function DerivedClass:method()
    -- 调用父类方法
    BaseClass.method(self)
    -- 子类逻辑
end
```

```typescript
// TypeScript
import { BaseClass } from "../BaseClass";

export class DerivedClass extends BaseClass {
    private property: PropertyType;
    
    constructor() {
        super();
        this.property = value;
    }
    
    public method(): void {
        // 调用父类方法
        super.method();
        // 子类逻辑
    }
}
```

### 模块系统转换

#### 模块导入
```lua
-- Lua
local Module = require(script.Parent.ModuleName)
local Utils = require(script.Parent.Parent.Utils)
local External = require(game.ReplicatedStorage.External)
```

```typescript
// TypeScript
import { Module } from "../ModuleName";
import { Utils } from "../../Utils";
import { External } from "@rbxts/external-package";
```

#### 模块导出
```lua
-- Lua
local ModuleName = {}
ModuleName.__index = ModuleName

function ModuleName.new()
    -- constructor
end

function ModuleName:method()
    -- method
end

return ModuleName
```

```typescript
// TypeScript
export class ModuleName {
    constructor() {
        // constructor
    }
    
    public method(): void {
        // method
    }
}
```

### 数据结构转换

#### 数组和表
```lua
-- Lua
local array = {}
local dict = {}
dict.key = value
dict["key2"] = value2

for i = 1, 10 do
    array[i] = i * 2
end

for key, value in pairs(dict) do
    print(key, value)
end
```

```typescript
// TypeScript
const array: number[] = [];
const dict: { [key: string]: ValueType } = {};
dict.key = value;
dict["key2"] = value2;

for (let i = 1; i <= 10; i++) {
    array[i - 1] = i * 2; // 注意索引差异
}

for (const [key, value] of Object.entries(dict)) {
    print(key, value);
}
```

#### 枚举
```lua
-- Lua
local BoostKind = {
    None = 0,
    StartBoost = 1,
    DriftBoost = 2,
    AdBoost = 3
}
```

```typescript
// TypeScript
export enum BoostKind {
    None = 0,
    StartBoost = 1,
    DriftBoost = 2,
    AdBoost = 3
}
```

### 特殊模式转换

#### 单例模式
```lua
-- Lua
local ClassName = {}
ClassName.__index = ClassName

local instance_ = nil

function ClassName.GetInstance()
    if instance_ == nil then
        instance_ = ClassName.new()
    end
    return instance_
end

function ClassName.new()
    local self = setmetatable({}, ClassName)
    return self
end
```

```typescript
// TypeScript
export class ClassName {
    private static instance: ClassName | undefined;
    
    private constructor() {
        // private constructor for singleton
    }
    
    public static GetInstance(): ClassName {
        if (!this.instance) {
            this.instance = new ClassName();
        }
        return this.instance;
    }
}
```

#### 建造者模式
```lua
-- Lua
local GoKartBuilder = {}
GoKartBuilder.__index = GoKartBuilder

function GoKartBuilder:Build()
    error("GoKartBuilder:Build() must be implemented by subclass")
end

-- 具体建造者
local GoPlayKartBuilder = {}
GoPlayKartBuilder.__index = GoPlayKartBuilder
setmetatable(GoPlayKartBuilder, {__index = GoKartBuilder})

function GoPlayKartBuilder:Build()
    return require(script.Parent.GoPlayKart).new()
end
```

```typescript
// TypeScript
export abstract class GoKartBuilder {
    public abstract Build(): GoKart;
}

// 具体建造者
import { GoPlayKart } from "../GoPlayKart";

export class GoPlayKartBuilder extends GoKartBuilder {
    public Build(): GoKart {
        return new GoPlayKart();
    }
}
```

## 类型系统指南

### 基础类型映射

| Lua类型 | TypeScript类型 | 说明 |
|---------|----------------|------|
| `number` | `number` | 数值类型 |
| `string` | `string` | 字符串类型 |
| `boolean` | `boolean` | 布尔类型 |
| `nil` | `undefined \| null` | 空值 |
| `table` | `object \| interface \| class` | 根据用途决定 |
| `function` | `(...args: Type[]) => ReturnType` | 函数类型 |

### Roblox特定类型

```typescript
// Roblox内置类型
Vector3: Vector3
CFrame: CFrame
Instance: Instance
Model: Model
Part: Part
RemoteEvent: RemoteEvent
RemoteFunction: RemoteFunction

// 自定义Unity兼容类型
UnityVector3: import("../KartShared/UnityMath").Vector3
UnityQuaternion: import("../KartShared/UnityMath").Quaternion
Transform: import("../KartShared/RobloxUnityAdapter").Transform
```

### 接口定义

```typescript
// 配置接口
interface KartConfig {
    maxSpeed: number;
    acceleration: number;
    handling: number;
    boost: BoostKind;
}

// 状态接口
interface KartState {
    position: Vector3;
    velocity: Vector3;
    rotation: CFrame;
    isGrounded: boolean;
}

// 回调函数接口
type UpdateCallback = (deltaTime: number) => void;
type CollisionCallback = (hit: BasePart) => void;
```

## 常见转换陷阱

### 索引差异
```lua
-- Lua (1-based indexing)
for i = 1, #array do
    local item = array[i]
end
```

```typescript
// TypeScript (0-based indexing)
for (let i = 0; i < array.length; i++) {
    const item = array[i];
}

// 或使用现代语法
for (const item of array) {
    // 使用item
}
```

### 字符串操作
```lua
-- Lua
local str = "hello world"
local sub = string.sub(str, 1, 5) -- "hello"
local len = string.len(str)
local find = string.find(str, "world")
```

```typescript
// TypeScript
const str = "hello world";
const sub = str.substring(0, 5); // "hello"
const len = str.length;
const find = str.indexOf("world");
```

### 数学函数
```lua
-- Lua
local result = math.sqrt(16)
local angle = math.atan2(y, x)
local random = math.random()
```

```typescript
// TypeScript
const result = Math.sqrt(16);
const angle = Math.atan2(y, x);
const random = Math.random();
```

### 条件检查
```lua
-- Lua
if value then -- truthy check
    -- value is not nil and not false
end

if value == nil then
    -- nil check
end
```

```typescript
// TypeScript
if (value) { // truthy check
    // value is not undefined, null, false, 0, ""
}

if (value === undefined || value === null) {
    // undefined/null check
}

// 更安全的检查
if (value !== undefined && value !== null) {
    // non-null check
}
```

## 性能优化建议

### 避免频繁类型转换
```typescript
// 不好的做法
function updatePosition(pos: Vector3) {
    const unityPos = new UnityVector3(pos.X, pos.Y, pos.Z);
    // 每次调用都创建新对象
}

// 好的做法
class KartController {
    private tempUnityVector = new UnityVector3();
    
    updatePosition(pos: Vector3) {
        this.tempUnityVector.set(pos.X, pos.Y, pos.Z);
        // 重用对象，减少GC压力
    }
}
```

### 缓存频繁访问的引用
```typescript
// 不好的做法
function update() {
    game.GetService("RunService").Heartbeat.Connect(() => {
        // 每次都查找服务
    });
}

// 好的做法
class GameController {
    private runService = game.GetService("RunService");
    
    start() {
        this.runService.Heartbeat.Connect(() => {
            // 使用缓存的引用
        });
    }
}
```

### 类型断言谨慎使用
```typescript
// 危险的做法
const kart = model as Model; // 可能运行时出错

// 安全的做法
const kart = model.IsA("Model") ? model as Model : undefined;
if (kart) {
    // 安全使用
}
```

## 调试和测试

### 添加类型检查
```typescript
function setKartSpeed(kart: GoKart, speed: number): void {
    if (typeof speed !== "number" || speed < 0) {
        throw new Error(`Invalid speed: ${speed}`);
    }
    kart.setSpeed(speed);
}
```

### 单元测试结构
```typescript
// KartManager.test.ts
describe("KartManager", () => {
    let manager: KartManager;
    
    beforeEach(() => {
        manager = KartManager.GetInstance();
    });
    
    it("should create kart correctly", () => {
        const builder = new GoPlayKartBuilder();
        const controller = new KartBasicController();
        const kart = manager.SetKart(0, builder, controller, []);
        
        expect(kart).toBeDefined();
        expect(kart).toBeInstanceOf(GoPlayKart);
    });
});
```

### 错误处理
```typescript
class KartController {
    public initialize(): void {
        try {
            this.setupPhysics();
            this.bindEvents();
        } catch (error) {
            warn(`Failed to initialize KartController: ${error}`);
            throw error;
        }
    }
    
    private setupPhysics(): void {
        // 可能抛出异常的代码
    }
}
```

## 完整示例

### 完整类转换示例

**Lua源码：**
```lua
local GoKart = require(script.Parent.GoKart)
local BoostKind = require(script.Parent.BoostKind)

local GoPlayKart = {}
GoPlayKart.__index = GoPlayKart
setmetatable(GoPlayKart, {__index = GoKart})

function GoPlayKart.new()
    local self = setmetatable(GoKart.new(), GoPlayKart)
    self.maxSpeed = 100
    self.currentBoost = BoostKind.None
    return self
end

function GoPlayKart:accelerate(deltaTime)
    if self.currentBoost == BoostKind.DriftBoost then
        self.speed = self.speed + 20 * deltaTime
    else
        self.speed = self.speed + 10 * deltaTime
    end
    
    if self.speed > self.maxSpeed then
        self.speed = self.maxSpeed
    end
end

function GoPlayKart:setBoost(boostType)
    self.currentBoost = boostType
end

return GoPlayKart
```

**TypeScript转换结果：**
```typescript
import { GoKart } from "./GoKart";
import { BoostKind } from "./BoostKind";

export class GoPlayKart extends GoKart {
    private maxSpeed: number = 100;
    private currentBoost: BoostKind = BoostKind.None;
    
    constructor() {
        super();
    }
    
    public accelerate(deltaTime: number): void {
        if (this.currentBoost === BoostKind.DriftBoost) {
            this.speed += 20 * deltaTime;
        } else {
            this.speed += 10 * deltaTime;
        }
        
        if (this.speed > this.maxSpeed) {
            this.speed = this.maxSpeed;
        }
    }
    
    public setBoost(boostType: BoostKind): void {
        this.currentBoost = boostType;
    }
}
```

这个技术参考文档涵盖了转换过程中的所有重要细节，可以作为转换工作的技术手册使用。
