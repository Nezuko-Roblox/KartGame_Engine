-- Roblox MonoBehaviour等效框架
-- 用于模拟Unity的MonoBehaviour生命周期
local MonoBehaviour = {}
MonoBehaviour.__index = MonoBehaviour

-- 创建新的MonoBehaviour实例
function MonoBehaviour.new(scriptName)
    local self = setmetatable({}, MonoBehaviour)
    self.scriptName = scriptName or "UnknownScript"
    self.gameObject = nil  -- 对应的Part或Model
    self.transform = nil   -- Transform组件
    self.enabled = true
    self.started = false
    self.destroyed = false
    
    -- 生命周期状态
    self.awakeCallbacks = {}
    self.startCallbacks = {}
    self.updateCallbacks = {}
    self.fixedUpdateCallbacks = {}
    self.lateUpdateCallbacks = {}
    self.destroyCallbacks = {}
    
    -- Roblox连接
    self._heartbeatConn = nil
    self._steppedConn = nil
    
    -- 自动绑定到Roblox事件循环
    self:BindToRoblox()
    
    -- 延迟执行Awake，确保对象完全初始化
    task.defer(function()
        self:Awake()
    end)
    
    return self
end

-- Awake - 在对象创建时立即调用
function MonoBehaviour:Awake()
    for _, callback in pairs(self.awakeCallbacks) do
        callback(self)
    end
end

-- Start - 在第一帧Update前调用
function MonoBehaviour:Start()
    if not self.started then
        self.started = true
        for _, callback in pairs(self.startCallbacks) do
            callback(self)
        end
    end
end

-- Update - 每帧调用
function MonoBehaviour:Update(deltaTime)
    if not self.enabled or self.destroyed then return end
    if not self.started then self:Start() end
    
    for _, callback in pairs(self.updateCallbacks) do
        callback(self, deltaTime)
    end
end

-- FixedUpdate - 固定时间间隔调用
function MonoBehaviour:FixedUpdate(fixedDeltaTime)
    if not self.enabled or self.destroyed then return end
    
    for _, callback in pairs(self.fixedUpdateCallbacks) do
        callback(self, fixedDeltaTime)
    end
end

-- LateUpdate - 在Update后调用
function MonoBehaviour:LateUpdate(deltaTime)
    if not self.enabled or self.destroyed then return end
    
    for _, callback in pairs(self.lateUpdateCallbacks) do
        callback(self, deltaTime)
    end
end

-- OnDestroy - 对象销毁时调用
function MonoBehaviour:OnDestroy()
    self.destroyed = true
    
    -- 断开Roblox连接
    if self._heartbeatConn then
        self._heartbeatConn:Disconnect()
        self._heartbeatConn = nil
    end
    if self._steppedConn then
        self._steppedConn:Disconnect()
        self._steppedConn = nil
    end
    
    for _, callback in pairs(self.destroyCallbacks) do
        callback(self)
    end
end

-- 绑定到 Roblox 事件循环
function MonoBehaviour:BindToRoblox()
    local RunService = game:GetService("RunService")
    
    -- 确保之前的连接已经断开
    if self._heartbeatConn then
        self._heartbeatConn:Disconnect()
    end
    if self._steppedConn then
        self._steppedConn:Disconnect()
    end
    
    self._heartbeatConn = RunService.Heartbeat:Connect(function(dt)
        if self.enabled and not self.destroyed then
            self:Update(dt)
            self:LateUpdate(dt)
        end
    end)
    
    self._steppedConn = RunService.Stepped:Connect(function(_, fixedDt)
        if self.enabled and not self.destroyed then
            self:FixedUpdate(fixedDt)
        end
    end)
end

-- 启用/禁用MonoBehaviour
function MonoBehaviour:SetEnabled(enabled)
    self.enabled = enabled
end

-- 销毁MonoBehaviour
function MonoBehaviour:Destroy()
    if not self.destroyed then
        self:OnDestroy()
    end
end

-- 添加生命周期回调
function MonoBehaviour:AddAwakeCallback(callback)
    table.insert(self.awakeCallbacks, callback)
end

function MonoBehaviour:AddStartCallback(callback)
    table.insert(self.startCallbacks, callback)
end

function MonoBehaviour:AddUpdateCallback(callback)
    table.insert(self.updateCallbacks, callback)
end

function MonoBehaviour:AddFixedUpdateCallback(callback)
    table.insert(self.fixedUpdateCallbacks, callback)
end

function MonoBehaviour:AddLateUpdateCallback(callback)
    table.insert(self.lateUpdateCallbacks, callback)
end

function MonoBehaviour:AddDestroyCallback(callback)
    table.insert(self.destroyCallbacks, callback)
end

-- 设置GameObject
function MonoBehaviour:SetGameObject(robloxObject)
    local RobloxUnityAdapter = require(script.Parent.RobloxUnityAdapter)
    self.gameObject = RobloxUnityAdapter.GameObject.new(robloxObject)
    self.transform = self.gameObject.transform
end

-- 获取组件（模拟Unity的GetComponent）
function MonoBehaviour:GetComponent(componentType)
    if self.gameObject then
        return self.gameObject:GetComponent(componentType)
    end
    return nil
end

-- 获取子对象中的组件
function MonoBehaviour:GetComponentInChildren(componentType)
    if self.gameObject then
        return self.gameObject:GetComponentInChildren(componentType)
    end
    return nil
end

-- 获取子对象中的所有组件
function MonoBehaviour:GetComponentsInChildren(componentType)
    if self.gameObject then
        return self.gameObject:GetComponentsInChildren(componentType)
    end
    return {}
end

-- 添加组件
function MonoBehaviour:AddComponent(componentType)
    if self.gameObject then
        return self.gameObject:AddComponent(componentType)
    end
    return nil
end

return MonoBehaviour