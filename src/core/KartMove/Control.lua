-- Control struct的Lua等效实现
local Control = {}
Control.__index = Control

function Control.new()
    local self = setmetatable({}, Control)
    self:Initialize()
    return self
end

function Control:getRealAccel()
    if not self.accelBrakeSwap then
        return self.accel
    else
        return self.brake
    end
end

function Control:getRealBrake()
    if not self.accelBrakeSwap then
        return self.brake
    else
        return self.accel
    end
end

function Control:getRealSteer()
    local multiplier = 1.0
    if self.wheelFlip or self.wheelDevil then
        multiplier = -1.0
    end
    return multiplier * self.steer
end

function Control:Initialize()
    self.accel = 0.0
    self.brake = 0.0
    self.accelBrakeSwap = false
    self.steer = 0.0
    self.wheelFlip = false
    self.wheelDevil = false
    self.stayTime = 0.0
    self.steerAngle = 0.0
    self.oldSteerAngle = 0.0
end

return Control