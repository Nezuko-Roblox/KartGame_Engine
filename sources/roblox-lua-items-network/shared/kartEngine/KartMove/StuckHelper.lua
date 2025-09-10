-- StuckHelper struct的Lua等效实现
local StuckHelper = {}
StuckHelper.__index = StuckHelper

function StuckHelper.new()
    local self = setmetatable({}, StuckHelper)
    self:Initialize()
    return self
end

function StuckHelper:Initialize()
    self.wallStuckTime = 0.0
    self.obstStuckTime = 0.0
    self.inStuck = false
    self.gndStuckTime = 0.0  -- 这个字段在Initialize中没有设置，但在struct中存在
end

return StuckHelper