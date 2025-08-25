-- CollisionState struct的Lua等效实现
local CollisionState = {}
CollisionState.__index = CollisionState

function CollisionState.new()
    local self = setmetatable({}, CollisionState)
    self:Initialize()
    return self
end

function CollisionState:Initialize()
    self.kartCollide = false
    self.kartCollideVel = 0.0
    self.kartCollideDominant = false
    self.shock = false
    self.hop = false
    self.unmovingTime = 0.0
    self.shockVel = 0.0  -- 这个字段在Initialize中没有设置，但在struct中存在
end

return CollisionState