# Lua 到 Roblox-TS 转换规则完整指南

## 概述

本指南基于 KartGame_Engine 项目的实际转换经验，提供了从 Lua 到 Roblox-TS 的完整转换规则和最佳实践。该指南涵盖了从基础的语法转换到复杂的 OOP 模式转换的各个方面。

## 目录

1. [基础语法转换](#基础语法转换)
2. [面向对象编程转换](#面向对象编程转换)
3. [模块系统转换](#模块系统转换)
4. [Unity API 转换](#unity-api-转换)
5. [事件系统转换](#事件系统转换)
6. [数学库转换](#数学库转换)
7. [网络系统转换](#网络系统转换)
8. [ECS 集成转换](#ecs-集成转换)
9. [性能优化模式](#性能优化模式)
10. [常见陷阱和解决方案](#常见陷阱和解决方案)

---

## 基础语法转换

### 1. 变量声明

**Lua 模式:**
```lua
local playerName = "Player1"
local playerHealth = 100
local isAlive = true
```

**TypeScript 转换:**
```typescript
let playerName: string = "Player1";
let playerHealth: number = 100;
let isAlive: boolean = true;
```

**转换规则:**
- `local` → `let` (可重新赋值) 或 `const` (常量)
- 添加明确的类型注解
- 使用驼峰命名法 (camelCase)

### 2. 函数定义

**Lua 模式:**
```lua
local function calculateDamage(baseDamage, multiplier)
    return baseDamage * multiplier
end

local player = {
    health = 100,
    takeDamage = function(self, damage)
        self.health = self.health - damage
    end
}
```

**TypeScript 转换:**
```typescript
function calculateDamage(baseDamage: number, multiplier: number): number {
    return baseDamage * multiplier;
}

class Player {
    public health: number = 100;
    
    public takeDamage(damage: number): void {
        this.health -= damage;
    }
}
```

**转换规则:**
- 函数添加参数和返回值类型
- 使用 `this` 替代 `self`
- 方法定义在类内部

### 3. 表结构转换

**Lua 模式:**
```lua
local player = {
    name = "Player1",
    health = 100,
    position = {x = 0, y = 0, z = 0},
    inventory = {"sword", "shield", "potion"}
}
```

**TypeScript 转换:**
```typescript
interface Player {
    name: string;
    health: number;
    position: { x: number; y: number; z: number };
    inventory: string[];
}

const player: Player = {
    name: "Player1",
    health: 100,
    position: { x: 0, y: 0, z: 0 },
    inventory: ["sword", "shield", "potion"]
};
```

**转换规则:**
- 定义接口来描述表结构
- 使用明确的类型定义
- 数组使用 `type[]` 语法

---

## 面向对象编程转换

### 1. 基础类转换

**Lua 模式 (使用元表):**
```lua
local GoKart = {}
GoKart.__index = GoKart

function GoKart.new()
    local self = setmetatable({}, GoKart)
    self.stuck_ = false
    self.valid_ = true
    self.m_kart = nil
    return self
end

function GoKart:GetStuck()
    return self.stuck_
end

function GoKart:SetStuck(value)
    self.stuck_ = value
end
```

**TypeScript 转换:**
```typescript
export class GoKart {
    private stuck_: boolean = false;
    private valid_: boolean = true;
    public m_kart: unknown = undefined;
    
    constructor() {
        // 初始化逻辑
    }
    
    public GetStuck(): boolean {
        return this.stuck_;
    }
    
    public SetStuck(value: boolean): void {
        this.stuck_ = value;
    }
}
```

**转换规则:**
- 元表类 → TypeScript `class`
- `__index` 元方法 → 类方法定义
- `new()` 构造函数 → `constructor()`
- 私有属性保持 `_` 后缀约定
- `self` → `this`

### 2. 继承转换

**Lua 模式:**
```lua
local GoPlayKart = {}
GoPlayKart.__index = GoPlayKart
setmetatable(GoPlayKart, {__index = GoKart})

function GoPlayKart.new()
    local self = setmetatable({}, GoPlayKart)
    self.m_theGravity = {x = 0, y = -49, z = 0}
    self:Initialize()
    return self
end

function GoPlayKart:setReKart(controller, wheels)
    GoKart.setReKart(self, controller, wheels)
    -- 子类特定逻辑
end
```

**TypeScript 转换:**
```typescript
import { GoKart } from "./GoKart";
import { UnityVector3 } from "../KartShared/UnityMath";

export class GoPlayKart extends GoKart {
    private m_theGravity: UnityVector3 = new UnityVector3(0, -49, 0);
    
    constructor() {
        super();
        this.Initialize();
    }
    
    public setReKart(controller: unknown, wheels?: unknown[]): void {
        super.setReKart(controller, wheels);
        // 子类特定逻辑
    }
}
```

**转换规则:**
- `setmetatable(child, {__index = parent})` → `class Child extends Parent`
- `Parent.method(self, ...)` → `super.method(...)`
- 在 `constructor()` 中调用 `super()`

### 3. 方法重载

**Lua 模式:**
```lua
function Matrix3:setCol(col, v1, v2)
    if type(col) == "number" then
        -- 单列设置
        self.cols[col] = v1
    else
        -- 三列设置
        self.cols[0] = col
        self.cols[1] = v1
        self.cols[2] = v2
    end
end
```

**TypeScript 转换:**
```typescript
public setCol(v1: number | Vector3, v2?: Vector3, v3?: Vector3): void {
    if (typeIs(v1, "number") && v2 && !v3) {
        // 单列设置: setCol(int col, Vector3 v)
        const col = v1 as number;
        const v = v2 as Vector3;
        this.setColSingle(col, v);
    } else if (v2 && v3 && !typeIs(v1, "number")) {
        // 三列设置: setCol(Vector3 v1, Vector3 v2, Vector3 v3)
        this.setColSingle(0, v1 as Vector3);
        this.setColSingle(1, v2);
        this.setColSingle(2, v3);
    }
}
```

**转换规则:**
- 使用联合类型处理多参数模式
- 添加可选参数 `?`
- 使用类型守卫进行运行时类型检查

---

## 模块系统转换

### 1. require 语句转换

**Lua 模式:**
```lua
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3
local BoostKind = require(script.Parent.BoostKind)
```

**TypeScript 转换:**
```typescript
import { Vector3 } from "../KartShared/UnityMath";
import { BoostKind } from "./BoostKind";
```

**转换规则:**
- `require(path)` → `import { Name } from "relative/path"`
- 相对路径转换：`script.Parent` → `../`
- 命名导入：`local Name = Module.Name` → `import { Name } from "module"`

### 2. 模块导出

**Lua 模式:**
```lua
local Control = {}

function Control.new()
    -- 初始化逻辑
end

return Control
```

**TypeScript 转换:**
```typescript
export class Control {
    constructor() {
        // 初始化逻辑
    }
}

// 或者默认导出
export default Control;
```

**转换规则:**
- `return Module` → `export default Module` 或 `export class Module`
- 使用命名导出暴露特定功能

### 3. 命名空间导出

**Lua 模式:**
```lua
-- 在 index.lua 中
local KartMove = {}

KartMove.Vector3 = require("./Vector3")
KartMove.Matrix3 = require("./Matrix3")
KartMove.Control = require("./Control")

return KartMove
```

**TypeScript 转换:**
```typescript
// 在 index.ts 中
export { Vector3 } from "./Vector3";
export { Matrix3 } from "./Matrix3";
export { Control } from "./Control";

// 或者重新导出整个模块
export * from "./Vector3";
export * from "./Matrix3";
export * from "./Control";
```

---

## Unity API 转换

### 1. MonoBehaviour 生命周期

**Lua 模式:**
```lua
local MonoBehaviour = {}
MonoBehaviour.__index = MonoBehaviour

function MonoBehaviour:Awake()
    -- 初始化逻辑
end

function MonoBehaviour:Update(deltaTime)
    -- 每帧更新逻辑
end

function MonoBehaviour:FixedUpdate(fixedDeltaTime)
    -- 物理更新逻辑
end
```

**TypeScript 转换:**
```typescript
export class MonoBehaviour {
    private awakeCallbacks: ((self: MonoBehaviour) => void)[] = [];
    private updateCallbacks: ((self: MonoBehaviour, deltaTime: number) => void)[] = [];
    private fixedUpdateCallbacks: ((self: MonoBehaviour, fixedDeltaTime: number) => void)[] = [];
    
    constructor() {
        this.BindToRoblox();
        task.defer(() => this.Awake());
    }
    
    public Awake(): void {
        for (const callback of this.awakeCallbacks) {
            callback(this);
        }
    }
    
    public Update(deltaTime: number): void {
        for (const callback of this.updateCallbacks) {
            callback(this, deltaTime);
        }
    }
    
    public FixedUpdate(fixedDeltaTime: number): void {
        for (const callback of this.fixedUpdateCallbacks) {
            callback(this, fixedDeltaTime);
        }
    }
    
    private BindToRoblox(): void {
        const RunService = game.GetService("RunService");
        
        // 绑定到 Roblox 事件循环
        RunService.Heartbeat.Connect((deltaTime) => {
            this.Update(deltaTime);
        });
        
        RunService.Stepped.Connect((time, deltaTime) => {
            this.FixedUpdate(deltaTime);
        });
    }
}
```

**转换规则:**
- Unity 生命周期方法 → Roblox 事件绑定
- 使用回调数组管理多个监听器
- `task.defer()` 确保 `Awake()` 在构造后执行

### 2. Transform 系统

**Lua 模式:**
```lua
local transform = {
    position = {x = 0, y = 0, z = 0},
    rotation = {x = 0, y = 0, z = 0, w = 1},
    
    setPosition = function(self, pos)
        self.position = pos
    end,
    
    getPosition = function(self)
        return self.position
    end
}
```

**TypeScript 转换:**
```typescript
export class Transform {
    private _position: UnityVector3;
    private _rotation: UnityQuaternion;
    
    constructor() {
        this._position = new UnityVector3(0, 0, 0);
        this._rotation = UnityQuaternion.identity;
    }
    
    public get position(): UnityVector3 {
        return this._position;
    }
    
    public set position(value: UnityVector3) {
        this._position = value;
        this.updateRobloxPosition();
    }
    
    public get rotation(): UnityQuaternion {
        return this._rotation;
    }
    
    public set rotation(value: UnityQuaternion) {
        this._rotation = value;
        this.updateRobloxRotation();
    }
    
    private updateRobloxPosition(): void {
        if (this.robloxInstance) {
            this.robloxInstance.Position = new Vector3(
                this._position.X,
                this._position.Y,
                this._position.Z
            );
        }
    }
}
```

**转换规则:**
- 使用 getter/setter 模式
- 自动同步到 Roblox 对象
- 保持 Unity API 兼容性

---

## 事件系统转换

### 1. Unity 事件到 Roblox 事件

**Lua 模式:**
```lua
function MonoBehaviour:BindToRoblox()
    local RunService = game:GetService("RunService")
    self._heartbeatConn = RunService.Heartbeat:Connect(function(dt)
        self:Update(dt)
    end)
end

function MonoBehaviour:OnDestroy()
    if self._heartbeatConn then
        self._heartbeatConn:Disconnect()
    end
end
```

**TypeScript 转换:**
```typescript
export class MonoBehaviour {
    private _heartbeatConn?: RBXScriptConnection;
    
    private BindToRoblox(): void {
        const RunService = game.GetService("RunService");
        this._heartbeatConn = RunService.Heartbeat.Connect((dt) => {
            this.Update(dt);
        });
    }
    
    public OnDestroy(): void {
        if (this._heartbeatConn) {
            this._heartbeatConn.Disconnect();
            this._heartbeatConn = undefined;
        }
    }
}
```

**转换规则:**
- 事件连接存储为类属性
- 使用可选类型 `?` 表示可能为空
- 在销毁时清理连接

### 2. 自定义事件系统

**Lua 模式:**
```lua
local EventManager = {}
local listeners = {}

function EventManager:On(eventName, callback)
    if not listeners[eventName] then
        listeners[eventName] = {}
    end
    table.insert(listeners[eventName], callback)
end

function EventManager:Emit(eventName, ...)
    if listeners[eventName] then
        for _, callback in ipairs(listeners[eventName]) do
            callback(...)
        end
    end
end
```

**TypeScript 转换:**
```typescript
export class EventManager {
    private listeners: Map<string, ((...args: unknown[]) => void)[]> = new Map();
    
    public On(eventName: string, callback: (...args: unknown[]) => void): void {
        if (!this.listeners.has(eventName)) {
            this.listeners.set(eventName, []);
        }
        this.listeners.get(eventName)!.push(callback);
    }
    
    public Emit(eventName: string, ...args: unknown[]): void {
        const callbacks = this.listeners.get(eventName);
        if (callbacks) {
            for (const callback of callbacks) {
                callback(...args);
            }
        }
    }
    
    public Off(eventName: string, callback: (...args: unknown[]) => void): void {
        const callbacks = this.listeners.get(eventName);
        if (callbacks) {
            const index = callbacks.indexOf(callback);
            if (index > -1) {
                callbacks.splice(index, 1);
            }
        }
    }
}
```

**转换规则:**
- 使用 `Map` 存储事件监听器
- 添加类型安全的回调函数
- 提供取消订阅功能

---

## 数学库转换

### 1. Vector3 转换

**Lua 模式:**
```lua
local Vector3 = {}
Vector3.__index = Vector3

function Vector3.new(x, y, z)
    local self = setmetatable({}, Vector3)
    self.X = x or 0
    self.Y = y or 0
    self.Z = z or 0
    return self
end

function Vector3.__add(a, b)
    return Vector3.new(a.X + b.X, a.Y + b.Y, a.Z + b.Z)
end

function Vector3.__mul(a, b)
    if type(b) == "number" then
        return Vector3.new(a.X * b, a.Y * b, a.Z * b)
    else
        return Vector3.new(a.X * b.X, a.Y * b.Y, a.Z * b.Z)
    end
end

function Vector3.__index(self, key)
    if key == "magnitude" then
        return math.sqrt(self.X * self.X + self.Y * self.Y + self.Z * self.Z)
    end
end
```

**TypeScript 转换:**
```typescript
export class Vector3 {
    public X: number;
    public Y: number;
    public Z: number;
    
    constructor(x: number = 0, y: number = 0, z: number = 0) {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
    
    public get magnitude(): number {
        return math.sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
    }
    
    public get normalized(): Vector3 {
        const mag = this.magnitude;
        if (mag > 0) {
            return new Vector3(this.X / mag, this.Y / mag, this.Z / mag);
        }
        return new Vector3(0, 0, 0);
    }
    
    public add(other: Vector3): Vector3 {
        return new Vector3(this.X + other.X, this.Y + other.Y, this.Z + other.Z);
    }
    
    public mul(scalar: number | Vector3): Vector3 {
        if (typeOf(scalar) === "number") {
            const s = scalar as number;
            return new Vector3(this.X * s, this.Y * s, this.Z * s);
        } else {
            const v = scalar as Vector3;
            return new Vector3(this.X * v.X, this.Y * v.Y, this.Z * v.Z);
        }
    }
    
    // 静态方法
    public static Dot(a: Vector3, b: Vector3): number {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    
    public static Cross(a: Vector3, b: Vector3): Vector3 {
        return new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }
    
    // 静态常量
    public static readonly zero = new Vector3(0, 0, 0);
    public static readonly one = new Vector3(1, 1, 1);
    public static readonly up = new Vector3(0, 1, 0);
    public static readonly forward = new Vector3(0, 0, -1);
    public static readonly right = new Vector3(1, 0, 0);
}
```

**转换规则:**
- 元表操作符 → 实例方法
- `__index` 计算属性 → getter
- 添加静态方法和常量
- 保持与 Unity API 兼容

### 2. 四元数转换

**Lua 模式:**
```lua
local Quaternion = {}
Quaternion.__index = Quaternion

function Quaternion.new(x, y, z, w)
    local self = setmetatable({}, Quaternion)
    self.X = x or 0
    self.Y = y or 0
    self.Z = z or 0
    self.W = w or 1
    return self
end

function Quaternion.Euler(x, y, z)
    -- 欧拉角转四元数逻辑
end
```

**TypeScript 转换:**
```typescript
export class Quaternion {
    public X: number;
    public Y: number;
    public Z: number;
    public W: number;
    
    constructor(x: number = 0, y: number = 0, z: number = 0, w: number = 1) {
        this.X = x;
        this.Y = y;
        this.Z = z;
        this.W = w;
    }
    
    public get normalized(): Quaternion {
        const mag = math.sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W);
        if (mag > 0) {
            return new Quaternion(this.X / mag, this.Y / mag, this.Z / mag, this.W / mag);
        }
        return new Quaternion(0, 0, 0, 1);
    }
    
    public static Euler(x: number, y: number, z: number): Quaternion {
        const cx = math.cos(x * 0.5);
        const sx = math.sin(x * 0.5);
        const cy = math.cos(y * 0.5);
        const sy = math.sin(y * 0.5);
        const cz = math.cos(z * 0.5);
        const sz = math.sin(z * 0.5);
        
        return new Quaternion(
            sx * cy * cz + cx * sy * sz,
            cx * sy * cz - sx * cy * sz,
            cx * cy * sz + sx * sy * cz,
            cx * cy * cz - sx * sy * sz
        );
    }
    
    public static readonly identity = new Quaternion(0, 0, 0, 1);
}
```

---

## 网络系统转换

### 1. 远程事件系统

**Lua 模式:**
```lua
local NetworkManager = {}

function NetworkManager:SendToServer(eventName, data)
    local remote = game.ReplicatedStorage:WaitForChild("Remotes"):WaitForChild(eventName)
    remote:FireServer(data)
end

function NetworkManager:ListenToClient(eventName, callback)
    local remote = game.ReplicatedStorage:WaitForChild("Remotes"):WaitForChild(eventName)
    remote.OnServerEvent:Connect(callback)
end
```

**TypeScript 转换:**
```typescript
import { remote, namespace } from "@rbxts/flamework";

// 定义远程类型
interface KartRemotes {
    notifyKartCreated: (kartIndex: number, playerId: number) => void;
    applyBoost: (kartIndex: number, duration: number, boostType: number) => void;
    updatePosition: (kartIndex: number, position: Vector3) => void;
}

export const kartRemote = namespace<KartRemotes>("kartRemote");

export class NetworkManager {
    public static SendToServer<T extends keyof KartRemotes>(
        eventName: T,
        ...args: Parameters<KartRemotes[T]>
    ): void {
        kartRemote[eventName].sendToServer(...args);
    }
    
    public static ListenToClient<T extends keyof KartRemotes>(
        eventName: T,
        callback: (player: Player, ...args: Parameters<KartRemotes[T]>) => void
    ): void {
        kartRemote[eventName].connect(callback);
    }
}
```

**转换规则:**
- 使用 Flamework 的类型安全远程系统
- 定义接口描述远程事件类型
- 使用泛型确保类型安全

### 2. 客户端-服务器同步

**Lua 模式:**
```lua
local ClientNetworkManager = {}
local allKarts = {}

function ClientNetworkManager:UpdateRemotePositions(deltaTime)
    for kartIndex, kartData in pairs(allKarts) do
        if kartIndex ~= KartManager.PLAYER_KART_IDX then
            local kart = KartManager.GetKart(kartIndex)
            if kart then
                kart:SetPosition(kartData.position)
                kart:SetRotation(kartData.rotation)
            end
        end
    end
end
```

**TypeScript 转换:**
```typescript
export class ClientNetworkManager {
    private remoteKarts: Map<number, {
        position: UnityVector3;
        rotation: UnityQuaternion;
        lastUpdate: number;
    }> = new Map();
    
    public updateRemotePositions(deltaTime: number): void {
        for (const [kartIndex, kartData] of this.remoteKarts) {
            if (kartIndex !== KartManager.PLAYER_KART_IDX) {
                const kart = KartManager.GetKart(kartIndex);
                if (kart) {
                    // 插值平滑移动
                    const lerpFactor = math.min(deltaTime * 10, 1);
                    kart.position = kart.position.Lerp(kartData.position, lerpFactor);
                    kart.rotation = UnityQuaternion.Slerp(
                        kart.rotation,
                        kartData.rotation,
                        lerpFactor
                    );
                }
            }
        }
    }
    
    public onPositionUpdate(kartIndex: number, position: UnityVector3, rotation: UnityQuaternion): void {
        this.remoteKarts.set(kartIndex, {
            position,
            rotation,
            lastUpdate: os.time()
        });
    }
}
```

**转换规则:**
- 使用 `Map` 存储远程对象状态
- 添加插值平滑移动
- 使用强类型定义网络数据

---

## ECS 集成转换

### 1. 组件定义

**Lua 模式:**
```lua
local KartReference = {}

function KartReference.new(kartIndex, playerId, isPlayer)
    return {
        kartIndex = kartIndex,
        playerId = playerId,
        isPlayer = isPlayer,
        kartInstance = nil
    }
end
```

**TypeScript 转换:**
```typescript
import { component, Component } from "@rbxts/matter";

export interface KartReferenceComponent {
    kartIndex: number;
    kartInstance: unknown;
    isPlayer: boolean;
    position?: Vector3;
}

export const KartReference = component<KartReferenceComponent>("KartReference");
```

**转换规则:**
- 使用 Matter ECS 的 `component` 函数
- 定义组件接口描述数据结构
- 保持与原有 OOP 系统的兼容性

### 2. 系统转换

**Lua 模式:**
```lua
local KartUpdateSystem = {}

function KartUpdateSystem:Update(world, deltaTime)
    for entityId, kartRef in pairs(world:GetComponents("KartReference")) do
        local kart = KartManager.GetKart(kartRef.kartIndex)
        if kart then
            kart:Update(deltaTime)
            kartRef.position = kart:GetPosition()
        end
    end
end
```

**TypeScript 转换:**
```typescript
import { System, World } from "@rbxts/matter";
import { KartReference } from "./components/KartReference";

export class KartUpdateSystem implements System {
    constructor(private world: World) {}
    
    public update(dt: number): void {
        for (const [entityId, kartRef] of this.world.query(KartReference)) {
            const kart = KartManager.GetKart(kartRef.kartIndex);
            if (kart) {
                kart.Update(dt);
                kartRef.position = kart.position;
            }
        }
    }
}
```

**转换规则:**
- 实现 System 接口
- 使用 `world.query()` 查询组件
- 保持与原有 OOP 系统的桥接

---

## 性能优化模式

### 1. 对象池

**Lua 模式:**
```lua
local ObjectPool = {}
local pool = {}

function ObjectPool:Get()
    if #pool > 0 then
        return table.remove(pool)
    else
        return self:CreateNew()
    end
end

function ObjectPool:Release(obj)
    self:Reset(obj)
    table.insert(pool, obj)
end
```

**TypeScript 转换:**
```typescript
export class ObjectPool<T> {
    private pool: T[] = [];
    private createFn: () => T;
    private resetFn: (obj: T) => void;
    
    constructor(createFn: () => T, resetFn: (obj: T) => void) {
        this.createFn = createFn;
        this.resetFn = resetFn;
    }
    
    public Get(): T {
        if (this.pool.length > 0) {
            return this.pool.pop()!;
        }
        return this.createFn();
    }
    
    public Release(obj: T): void {
        this.resetFn(obj);
        this.pool.push(obj);
    }
    
    public Clear(): void {
        this.pool = [];
    }
}
```

### 2. 连接管理

**Lua 模式:**
```lua
local ConnectionManager = {}
local connections = {}

function ConnectionManager:Connect(signal, callback)
    local conn = signal:Connect(callback)
    table.insert(connections, conn)
    return conn
end

function ConnectionManager:DisconnectAll()
    for _, conn in ipairs(connections) do
        conn:Disconnect()
    end
    connections = {}
end
```

**TypeScript 转换:**
```typescript
export class ConnectionManager {
    private connections: RBXScriptConnection[] = [];
    
    public Connect<T extends unknown[]>(
        signal: RBXScriptSignal<T>,
        callback: (...args: T) => void
    ): RBXScriptConnection {
        const conn = signal.Connect(callback);
        this.connections.push(conn);
        return conn;
    }
    
    public DisconnectAll(): void {
        for (const conn of this.connections) {
            conn.Disconnect();
        }
        this.connections = [];
    }
    
    public Remove(conn: RBXScriptConnection): void {
        const index = this.connections.indexOf(conn);
        if (index > -1) {
            this.connections.splice(index, 1);
            conn.Disconnect();
        }
    }
}
```

---

## 常见陷阱和解决方案

### 1. 类型转换问题

**问题:** Lua 的动态类型与 TypeScript 的静态类型冲突

**解决方案:**
```typescript
// 类型守卫函数
function isVector3(obj: unknown): obj is Vector3 {
    return typeOf(obj) === "table" && 
           "X" in (obj as object) && 
           "Y" in (obj as object) && 
           "Z" in (obj as object);
}

// 类型断言
const vector = unknownValue as Vector3;

// 安全转换函数
function safeVector3(v: unknown): Vector3 {
    if (isVector3(v)) {
        return v;
    }
    return Vector3.zero;
}
```

### 2. 循环依赖

**问题:** Lua 中的循环依赖在 TypeScript 中造成编译错误

**解决方案:**
```typescript
// 使用延迟初始化
class KartManager {
    private static _instance: KartManager;
    private _boostSystem?: BoostSystem;
    
    public static getInstance(): KartManager {
        if (!this._instance) {
            this._instance = new KartManager();
        }
        return this._instance;
    }
    
    public get boostSystem(): BoostSystem {
        if (!this._boostSystem) {
            this._boostSystem = require("./BoostSystem").BoostSystem;
        }
        return this._boostSystem;
    }
}
```

### 3. 内存泄漏

**问题:** 事件连接和对象引用没有正确清理

**解决方案:**
```typescript
export class Disposable {
    private connections: RBXScriptConnection[] = [];
    private disposables: Disposable[] = [];
    
    public AddConnection(conn: RBXScriptConnection): void {
        this.connections.push(conn);
    }
    
    public AddDisposable(disposable: Disposable): void {
        this.disposables.push(disposable);
    }
    
    public Dispose(): void {
        // 清理连接
        for (const conn of this.connections) {
            conn.Disconnect();
        }
        this.connections = [];
        
        // 清理子对象
        for (const disposable of this.disposables) {
            disposable.Dispose();
        }
        this.disposables = [];
    }
}
```

### 4. 数值精度问题

**问题:** Lua 的 number 类型与 TypeScript 的 number 类型精度差异

**解决方案:**
```typescript
export class Mathf {
    public static readonly Epsilon = 1e-6;
    public static readonly Infinity = math.huge;
    
    public static Approximately(a: number, b: number): boolean {
        return math.abs(a - b) < Mathf.Epsilon;
    }
    
    public static Clamp(value: number, min: number, max: number): number {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    
    public static Lerp(a: number, b: number, t: number): number {
        t = Mathf.Clamp01(t);
        return a + (b - a) * t;
    }
}
```

---

## 总结

本转换指南提供了从 Lua 到 Roblox-TS 的完整转换规则和最佳实践。关键要点包括：

1. **保持功能性**: 确保转换后的代码与原 Lua 代码功能完全一致
2. **类型安全**: 充分利用 TypeScript 的类型系统提高代码质量
3. **性能优化**: 使用现代 JavaScript 特性优化性能
4. **可维护性**: 采用模块化设计和清晰的代码结构
5. **兼容性**: 保持与现有系统的兼容性

通过遵循这些规则，可以确保转换过程顺利进行，同时保持代码的高质量和可维护性。