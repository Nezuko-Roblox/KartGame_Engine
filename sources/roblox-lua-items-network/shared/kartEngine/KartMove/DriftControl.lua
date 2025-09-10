-- DriftControl struct的Lua等效实现
local DriftControl = {}
DriftControl.__index = DriftControl

function DriftControl.new()
    local self = setmetatable({}, DriftControl)
    self:Initialize()
    return self
end

function DriftControl:Initialize()
    self.slipMode = false
    self.slipTime = 0.0
    self.forceSlip = false
    self.trigger = false
    self.triggerTime = 0.0
end

return DriftControl