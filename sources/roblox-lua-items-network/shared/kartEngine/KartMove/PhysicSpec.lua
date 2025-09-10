-- PhysicSpec struct的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3

local PhysicSpec = {}
PhysicSpec.__index = PhysicSpec

function PhysicSpec.new()
    local self = setmetatable({}, PhysicSpec)
    self:Initialize()
    return self
end

function PhysicSpec:Initialize()
    self.wheelTranslate = {}  -- new Vector3[4]
    self.wheelWidth = {}      -- new float[4]
    
    self.itemSlotCapacity = 2
    self.useTransformBooster = false
    self.mass = 100.0
    self.airFriction = 3.0
    self.dragFactor = 0.74
    self.forwardAccel = 4000.0
    self.backwardAccel = 1500.0
    self.gripBrake = 1800.0
    self.slipBrake = 1200.0
    self.maxSteerDeg = 4.0
    self.steerConstraint = 30.0
    self.frontGripFactor = 5.0  
    self.rearGripFactor = 5.0
    self.driftTrigFactor = 0.2 
    self.driftTrigTime = 0.2
    self.driftSlipFactor = 0.3
    self.driftEscapeForce = 2500.0
    self.cornerDrawFactor = 0.2
    self.driftLeanFactor = 0.07
    self.steerLeanFactor = 0.01
    self.driftMaxGauge = 4000.0
    self.normalBoosterTime = 3000.0
    self.teamBoosterTime = 4500.0
    self.animalBoosterTime = 4000.0
    self.driftDragReduceFactor= 0.4
end

function PhysicSpec:getMaxSteerRad()
    return 3.14159274 * self.maxSteerDeg / 180.0
end

return PhysicSpec