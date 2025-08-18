-- DriveFactor struct的Lua等效实现
local DriveFactor = {}
DriveFactor.__index = DriveFactor

function DriveFactor.new()
    local self = setmetatable({}, DriveFactor)
    self:Initialize()
    return self
end

function DriveFactor:Initialize()
    self.frontGripFactor = 0.0
    self.rearGripFactor = 0.0
    self.driftSlipFactor = 1.0
    self.backFrontGripFactor = 0.0
    self.backRearGripFactor = 0.0
    self.backDriftSlipFactor = 1.0
    self.betaCut = 1.0
    self.onDriftSteerFactor = 0.7  -- 降低漂移中的转向速度
    self.onRestTimeSteerFactor = 0.8  -- 降低漂移恢复时的转向速度
    self.onTriggerSteerFactor = 1.0
    self.speedLimit = 120.0
end

return DriveFactor