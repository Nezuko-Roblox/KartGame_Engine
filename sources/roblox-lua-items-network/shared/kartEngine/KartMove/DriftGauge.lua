-- DriftGauge struct的Lua等效实现
local DriftGauge = {}
DriftGauge.__index = DriftGauge

function DriftGauge.new()
    local self = setmetatable({}, DriftGauge)
    self:Initialize()
    return self
end

function DriftGauge:Initialize()
    self.gauge = 0.0
    self.progressOn = false
    self.progress = 0.0
    self.lastProgress = 0.0
    self.progressTime = 0.0  -- 这个字段在Initialize中没有设置，但在struct中存在
end

return DriftGauge