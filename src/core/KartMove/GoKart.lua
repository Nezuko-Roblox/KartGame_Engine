-- GoKart class的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3
local BoostKind = require(script.Parent.BoostKind)

local GoKart = {}
GoKart.__index = GoKart

function GoKart.new()
    local self = setmetatable({}, GoKart)
    
    -- 属性初始化
    self.stuck_ = false
    self.valid_ = true
    self.forcing_ = false
    self.isInResetState_ = false
    self.status_ = 0
    
    -- 公共成员
    self.m_kart = nil
    self.controller_ = nil
    self.m_KartWLVel = Vector3.zero
    self.m_KartLAVel = Vector3.zero
    self.m_KartRealVelocity = Vector3.zero
    
    return self
end

-- 属性访问器
function GoKart:GetStuck()
    return self.stuck_
end

function GoKart:SetStuck(value)
    self.stuck_ = value
end

function GoKart:GetValid()
    return self.valid_
end

function GoKart:SetValid(value)
    if self.valid_ ~= value then
        self.valid_ = value
    end
end

function GoKart:GetForcing()
    return self.forcing_
end

function GoKart:SetForcing(value)
    self.forcing_ = value
end

function GoKart:GetIsInResetState()
    return self.isInResetState_
end

function GoKart:SetIsInResetState(value)
    self.isInResetState_ = value
end

-- 虚函数 - 子类可以重写
function GoKart:basicAction(tick)
    -- 基类的空实现
end

function GoKart:setReKart(controller, wheels)
    self.controller_ = controller
    -- 获取真正的Roblox对象
    if controller and controller.gameObject then
        self.m_kart = controller.gameObject
    else
        warn("GoKart: 无法获取Roblox对象")
    end
end

function GoKart:Warp(pos, rot, flush, resetVel)
    if self.m_kart and self.m_kart.transform then
        self.m_kart.transform.localPosition = pos
        self.m_kart.transform.localRotation = rot
    end
    
    if flush then
        -- 刷新逻辑
    end
    
    if resetVel then
        self.m_KartWLVel = Vector3.zero
        self.m_KartLAVel = Vector3.zero
    end
end

function GoKart:isRealBoost()
    return false
end

function GoKart:isRealBoostWithKind(kind)
    return false
end

function GoKart:isItemBoost()
    return false
end

function GoKart:isItemBoostWithKind(kind)
    return false
end

function GoKart:isZoneBoost()
    return false
end

function GoKart:isZoneBoostWithKind(kind)
    return false
end

function GoKart:isBoost(kind)
    return false
end

function GoKart:ResetForRestarting()
    self.m_KartWLVel = Vector3.zero
    self.m_KartLAVel = Vector3.zero
    self.m_KartRealVelocity = Vector3.zero
    self.stuck_ = false
    self.valid_ = true
    self.forcing_ = false
    self.status_ = 0
end

return GoKart