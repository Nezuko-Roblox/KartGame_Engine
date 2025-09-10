-- FirstPipelineValue struct的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3

local FirstPipelineValue = {}
FirstPipelineValue.__index = FirstPipelineValue

function FirstPipelineValue.new()
    local self = setmetatable({}, FirstPipelineValue)
    
    self.left = Vector3.new()
    self.up = Vector3.new()
    self.front = Vector3.new()
    self.frontVel = 0.0
    self.leftVel = 0.0
    self.upVel = 0.0
    self.speed = 0.0
    
    return self
end

return FirstPipelineValue