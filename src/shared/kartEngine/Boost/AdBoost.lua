-- AdBoost struct的Lua等效实现
local AdBoost = {}
AdBoost.__index = AdBoost

function AdBoost.new()
    local self = setmetatable({}, AdBoost)
    self:Initialize()
    return self
end

function AdBoost:Initialize()
    self.validTrigger = false
    self.validTime = 0.0
    self.useLeftTime = 0.0
end

return AdBoost