-- Unity风格的触发器类 - 正确处理OnTriggerEnter/Stay/Exit生命周期
-- 解决Roblox中触发器自动触发的问题

local CollectionService = game:GetService("CollectionService")
local RunService = game:GetService("RunService")

local UnityTrigger = {}
UnityTrigger.__index = UnityTrigger

-- 构造函数
function UnityTrigger.new(owner)
    local self = setmetatable({}, UnityTrigger)
    
    self.owner = owner  -- 拥有此触发器的对象（如RigidbodyFPSWalker）
    self.currentTriggers = {}  -- 当前帧检测到的触发对象
    self.lastFrameTriggers = {}  -- 上一帧的触发对象
    self.isEnabled = true
    
    return self
end

-- 启用/禁用触发器
function UnityTrigger:SetEnabled(enabled)
    self.isEnabled = enabled
end

-- 检测对象是否在触发范围内 - 使用真正的碰撞体重叠检测
function UnityTrigger:IsInTriggerRange(targetObject)
    if not self.owner or not self.owner.gameObject or not targetObject then
        return false
    end
    
    -- 获取卡丁车的位置和大小（模拟BoxCollider）
    local kartPart = self.owner.gameObject
    local kartPos = kartPart.Position
    local kartSize = kartPart.Size
    
    -- 创建一个稍大的检测区域（模拟Unity的触发器）
    local triggerSize = kartSize * 1.2  -- 触发器稍大于实体
    
    -- 使用简单但准确的边界盒重叠检测
    local kartMin = kartPos - triggerSize/2
    local kartMax = kartPos + triggerSize/2
    
    local targetMin = targetObject.Position - targetObject.Size/2
    local targetMax = targetObject.Position + targetObject.Size/2
    
    -- 检查两个AABB是否重叠
    local overlapX = kartMax.X >= targetMin.X and kartMin.X <= targetMax.X
    local overlapY = kartMax.Y >= targetMin.Y and kartMin.Y <= targetMax.Y
    local overlapZ = kartMax.Z >= targetMin.Z and kartMin.Z <= targetMax.Z
    
    local result = overlapX and overlapY and overlapZ
    
    return result
end

-- 更新触发器状态（在FixedUpdate中调用）
function UnityTrigger:UpdateTriggers()
    if not self.isEnabled then
        return
    end
    
    -- 清空当前帧触发列表
    self.currentTriggers = {}
    
    -- 检测所有可能的触发对象（不需要特定标签）
    local allObjects = {}
    
    -- 添加Terrain
    table.insert(allObjects, workspace.Terrain)
    
    -- 获取workspace中所有BasePart
    for _, obj in ipairs(workspace:GetDescendants()) do
        if obj:IsA("BasePart") and obj.CanTouch and obj.CanCollide then
            table.insert(allObjects, obj)
        end
    end
    
    for _, obj in ipairs(allObjects) do
        if self:IsInTriggerRange(obj) then
            table.insert(self.currentTriggers, obj)
        end
    end
    
    -- 处理触发事件
    self:ProcessTriggerEvents()
    
    -- 更新上一帧状态
    self.lastFrameTriggers = {}
    for _, obj in ipairs(self.currentTriggers) do
        table.insert(self.lastFrameTriggers, obj)
    end
end

-- 处理触发器事件（Enter/Stay/Exit）
function UnityTrigger:ProcessTriggerEvents()
    -- OnTriggerEnter - 当前帧有但上一帧没有的对象
    for _, currentObj in ipairs(self.currentTriggers) do
        if not self:WasTriggeredLastFrame(currentObj) then
            self:CallOnTriggerEnter(currentObj)
        end
    end
    
    -- OnTriggerStay - 当前帧有且上一帧也有的对象 (但每帧最多只调用一次)
    local hasStayEvent = false
    for _, currentObj in ipairs(self.currentTriggers) do
        if self:WasTriggeredLastFrame(currentObj) and not hasStayEvent then
            self:CallOnTriggerStay(currentObj)
            hasStayEvent = true -- 确保每帧最多只调用一次Stay
        end
    end
    
    -- OnTriggerExit - 上一帧有但当前帧没有的对象
    for _, lastObj in ipairs(self.lastFrameTriggers) do
        if not self:IsTriggeredThisFrame(lastObj) then
            self:CallOnTriggerExit(lastObj)
        end
    end
end

-- 检查对象在上一帧是否被触发
function UnityTrigger:WasTriggeredLastFrame(obj)
    for _, lastObj in ipairs(self.lastFrameTriggers) do
        if lastObj == obj then
            return true
        end
    end
    return false
end

-- 检查对象在当前帧是否被触发
function UnityTrigger:IsTriggeredThisFrame(obj)
    for _, currentObj in ipairs(self.currentTriggers) do
        if currentObj == obj then
            return true
        end
    end
    return false
end

-- 调用OnTriggerEnter
function UnityTrigger:CallOnTriggerEnter(collider)
    if self.owner and self.owner.OnTriggerEnter then
        self.owner:OnTriggerEnter(collider)
    end
end

-- 调用OnTriggerStay
function UnityTrigger:CallOnTriggerStay(collider)
    if self.owner and self.owner.OnTriggerStay then
        self.owner:OnTriggerStay(collider)
    end
end

-- 调用OnTriggerExit
function UnityTrigger:CallOnTriggerExit(collider)
    if self.owner and self.owner.OnTriggerExit then
        self.owner:OnTriggerExit(collider)
    end
end

-- 获取当前触发的对象数量
function UnityTrigger:GetTriggerCount()
    return #self.currentTriggers
end

-- 获取当前触发的对象列表
function UnityTrigger:GetTriggeredObjects()
    return self.currentTriggers
end

-- 清理
function UnityTrigger:Destroy()
    self.currentTriggers = {}
    self.lastFrameTriggers = {}
    self.owner = nil
end

return UnityTrigger
