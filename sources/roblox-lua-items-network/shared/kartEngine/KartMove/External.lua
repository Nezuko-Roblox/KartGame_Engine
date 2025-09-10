-- External struct的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3

local External = {}
External.__index = External

function External.new()
    local self = setmetatable({}, External)
    self:Initialize()
    return self
end

function External:Initialize()
    self.slip = false
    self.dragFactor = 1.0
    self.compensationDragFactor = 1.0
    self.wheelFactor = 1.0
    self.annexForce = Vector3.zero
    self.force = Vector3.zero
    self.torque = Vector3.zero
    self.upDownTime = 0.0
    self.upDownLastTime = 0.0
    self.gravityFactor = 1.0
    self.speedLimit = 0.0
    
    -- 这些字段在Initialize中没有设置，但在struct中存在
    self.upDownInterval = 0.0
    self.upDownForce = Vector3.zero
    self.upDownForceIndex = 0
    self.liftVel = Vector3.zero
end

return External