-- Suspension struct的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector2 = UnityMath.Vector2
local Vector3 = UnityMath.Vector3

local Suspension = {}
Suspension.__index = Suspension

function Suspension.new()
    local self = setmetatable({}, Suspension)
    self:Initialize()
    return self
end

function Suspension:InitVector2(v, x, y)
    v.X = x
    v.Y = y
end

function Suspension:Initialize()
    self.wheelOff = {}
    for i = 1, 4 do
        self.wheelOff[i] = Vector2.new()
        local xVal = ((i - 1) % 2 ~= 0) and 1.0 or -1.0
        local yVal = (math.floor((i - 1) / 2) ~= 0) and -1.0 or 1.0
        self:InitVector2(self.wheelOff[i], xVal, yVal)
    end
    
    self.wheelContact = {}
    for i = 1, 4 do
        self.wheelContact[i] = false
    end
    
    self.wheelContactN = {}
    for i = 1, 4 do
        self.wheelContactN[i] = Vector3.new()
    end
    
    self.maxTravel = 1.5  -- 大幅增加悬挂行程，让车体离地更高
    self.travel = {1.5, 1.5, 1.5, 1.5}
    self.deltaTravel = {0.0, 0.0, 0.0, 0.0}
    
    -- 添加遗漏的contactN字段
    self.contactN = Vector3.new()
end

return Suspension