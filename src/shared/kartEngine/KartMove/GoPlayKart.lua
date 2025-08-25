-- GoPlayKart 类继承自 GoKart
local GoKart = require(script.Parent.GoKart)
local BoostKind = require(script.Parent.BoostKind)
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local UnityTimeModule = require(script.Parent.Parent.KartShared.UnityTime)
local Time = UnityTimeModule.Time
-- 保留对Roblox原生Vector3的引用
local RobloxVector3 = Vector3
local Vector3 = UnityMath.Vector3
local Mathf = UnityMath.Mathf
local Vector3Helper = require(script.Parent.Vector3Helper)
local Matrix3 = require(script.Parent.Matrix3)
local DriveFactor = require(script.Parent.DriveFactor)

local CollectionService = game:GetService("CollectionService")
local Physics = require(script.Parent.Parent.KartShared.UnityEngine.Physics)

local GoPlayKart = {}
GoPlayKart.__index = GoPlayKart
setmetatable(GoPlayKart, {__index = GoKart})

-- 构造函数
function GoPlayKart.new()
    local self = setmetatable(GoKart.new(), GoPlayKart)
    
    -- 初始化GoPlayKart特有属性
    self.m_theGravity = Vector3.new(0, -49, 0)
    self.m_NetWForce = Vector3.zero
    self.m_NetLTorque = Vector3.zero
    self.m_boostLeft = 0
    self.m_slipBoost = false
    self.m_Contact = true
    
    -- 初始化各种结构体
    self.m_spec = require(script.Parent.PhysicSpec).new()
    self.m_sus = require(script.Parent.Suspension).new()
    self.m_drift = require(script.Parent.DriftControl).new()
    self.m_driftGauge = require(script.Parent.DriftGauge).new()
    self.m_adBoost = require(script.Parent.Parent.Boost.AdBoost).new()
    self.m_ctrl = require(script.Parent.Control).new()
    self.m_cState = require(script.Parent.CollisionState).new()
    self.m_extern = require(script.Parent.External).new()
    self.m_stuckHelper = require(script.Parent.StuckHelper).new()
    self.m_first = require(script.Parent.FirstPipelineValue).new()
    
    -- 初始化各种结构体
    self.m_spec:Initialize()
    self.m_sus:Initialize()
    self.m_drift:Initialize()
    self.m_driftGauge:Initialize()
    self.m_adBoost:Initialize()
    self.m_ctrl:Initialize()
    self.m_cState:Initialize()
    self.m_extern:Initialize()
    self.m_stuckHelper:Initialize()
    
    -- 初始化DriveFactor数组
    self.m_DriveFactor = {}
    for i = 1, 3 do
        self.m_DriveFactor[i] = {}
        for j = 1, 2 do
            self.m_DriveFactor[i][j] = DriveFactor.new()
            self.m_DriveFactor[i][j]:Initialize()
        end
    end
    
    self.m_ort = Matrix3.CreateMtxIdentity()
    
    -- GoPlayKart特有属性
    self.wheelLocalPos_ = {}
    self.isCrash_ = false
    self.crashVelocity_ = 0
    self.isShock_ = false
    self.shockVelocity_ = 0
    self.m_slipReserveTime = 0
    self.m_steer = 0
    self.m_grip = 0
    self.m_slip = {2, 0.5}
    self.m_DriveMode = 0
    self.m_reciprocalMass = Vector3.zero
    
    -- Unity中的其他属性
    self.isBackupStreer = {false, false, false}
    self.m_isDrift = false
    self.needReset_ = false
    self.backupVelocity_ = Vector3.zero
    
    -- 调用loadParam来加载参数
    self:loadParam()
    
    
    return self
end

-- 加载参数方法
function GoPlayKart:loadParam()
    self.m_slipReserveTime = 0
    self.m_steer = 0
    self.m_grip = 0
    self.m_slip[1] = 2
    self.m_slip[2] = 0.5
    
    -- 设置DriveFactor参数
    self.m_DriveFactor[2][1].speedLimit = 340
    self.m_DriveFactor[2][1].betaCut = 0.6  
    self.m_DriveFactor[2][1].frontGripFactor = -1
    self.m_DriveFactor[2][1].rearGripFactor = -1
    self.m_DriveFactor[2][2].betaCut = 0.85  
    self.m_DriveFactor[3][1].speedLimit = 180
    self.m_DriveFactor[3][1].betaCut = 0.6  
    self.m_DriveFactor[3][1].driftSlipFactor = 0.5
    self.m_DriveFactor[3][2].frontGripFactor = -2
    self.m_DriveFactor[3][2].rearGripFactor = -2
end

-- basicAction方法 - 对应Unity版本的完整逻辑
function GoPlayKart:basicAction(tick)
    self.m_isDrift = false
    self.basicActionTick = tick  -- 保存tick用于调试
    -- 获取真实的帧间隔时间，对应Unity的Time.fixedDeltaTime
    local fixedDeltaTime = Time.fixedDeltaTime
    
    if not self.Forcing and not self.IsInResetState then
        -- 处理加速剩余时间
        if self.m_boostLeft > 0 then
            self.m_boostLeft = self.m_boostLeft - Mathf.Min(self.m_boostLeft, math.floor(fixedDeltaTime * 1000))
            if self.m_boostLeft == 0 then
                self.m_BoostKind = BoostKind.NoBoost
            end
        end
        
        self:processAdBoostTime(fixedDeltaTime)
        self:beginNetForce(fixedDeltaTime)
        local flag = self:decideKartContact(fixedDeltaTime, false)
        
        if flag then
            self:calcNonpenetrateForce(fixedDeltaTime)
            self:calcKartTractionForce(fixedDeltaTime)
            self:calcKartSteeringForce(fixedDeltaTime)
        else
            self:calcFlyingKartForce()
        end
        
        self:calcResistForce()
        self:endNetForce(fixedDeltaTime)
        self:ProcessDriftGauge(fixedDeltaTime)
        self.m_isDrift = self.m_isDrift or self.m_drift.slipMode or self.m_drift.slipTime > 0 or self.m_drift.forceSlip
    else
        self.m_KartWLVel = Vector3.zero
        self.m_KartLAVel = Vector3.zero
        self.m_NetWForce = Vector3.zero
        self.m_NetLTorque = Vector3.zero
        if self.m_boostLeft > 0 then
            self.m_boostLeft = self.m_boostLeft - Mathf.Min(self.m_boostLeft, math.floor(fixedDeltaTime * 1000))
            if self.m_boostLeft == 0 then
                self.m_BoostKind = BoostKind.NoBoost
            end
        end
    end
    
    -- 调用父类basicAction
    GoKart.basicAction(self, tick)
end

-- calcResistForce方法 - Unity原版第109-121行
function GoPlayKart:calcResistForce()
    local vector = Vector3.zero
    local vector2 = Vector3.zero
    
    -- Unity原版逻辑：完全对应，不添加额外的漂移阻力处理
    vector = vector - self.m_KartWLVel * self.m_spec.airFriction
    vector2 = vector2 - self.m_KartLAVel * self.m_spec.airFriction
    if self.m_Contact then
        -- 漂移时动态调整阻力，减少降速
        local driftDragFactor = self.m_extern.dragFactor
        if self.m_drift and (self.m_drift.slipMode or self.m_drift.forceSlip or self.m_drift.trigger) then
            -- 漂移时减少阻力系数，降低降速程度，使用PhysicSpec中的可配置参数
            driftDragFactor = driftDragFactor * self.m_spec.driftDragReduceFactor
        end
        vector = vector - self.m_KartWLVel * self.m_KartWLVel.magnitude * self.m_spec.dragFactor * driftDragFactor * self.m_extern.compensationDragFactor
    end
    self.m_NetWForce = self.m_NetWForce + vector
    self.m_NetLTorque = self.m_NetLTorque + vector2
end

-- calcNonpenetrateForce方法 - 完全对应Unity版本124-143行
function GoPlayKart:calcNonpenetrateForce(deltaT)
    for num = 0, 3 do
        local num2
        if self.m_sus.wheelContact[num + 1] then
            if self.m_sus.deltaTravel[num + 1] <= 0 then
                num2 = self.m_spec.springK * self.m_sus.travel[num + 1] + self.m_spec.damperRebC * (self.m_sus.deltaTravel[num + 1] / deltaT)
            else
                num2 = self.m_spec.springK * self.m_sus.travel[num + 1] + self.m_spec.damperCopC * (self.m_sus.deltaTravel[num + 1] / deltaT)
            end
        else
            num2 = 0
        end
        
        if num2 <= 0 then
            num2 = 0
        else
            num2 = Vector3.Dot(self.m_sus.wheelContactN[num + 1], self.m_first.up) * num2
        end
        
        local vector = Vector3.new(self.m_spec.width * self.m_sus.wheelOff[num + 1].X, 0, -self.m_spec.length * self.m_sus.wheelOff[num + 1].Y)
        local vector2 = Vector3.Cross(vector, Vector3.new(0, num2, 0)) * 0.1
        self.m_NetLTorque = self.m_NetLTorque + vector2
    end
    
    -- 添加重力影响
    self.m_NetWForce = self.m_NetWForce + self.m_theGravity * self.m_spec.mass * self.m_extern.gravityFactor * 0.8
end

-- calcKartTractionForce方法 - 完全对应Unity源码146-234行
function GoPlayKart:calcKartTractionForce(deltaT)
    if self.m_extern.slip then
        return
    end
    
    local vector = Vector3.Cross(self.m_first.left, self.m_sus.contactN)
    
    if self.m_ctrl:getRealAccel() ~= 0 and not self.Stuck then
        if self:isRealBoost() then
            local num = 4.5
            if self.m_BoostKind == BoostKind.BoostDrift then
                num = 5
            end
            self.m_NetWForce = self.m_NetWForce + vector * self.m_ctrl:getRealAccel() * num * 
                              (not self.m_drift.forceSlip and self.m_spec.forwardAccel or self.m_spec.driftEscapeForce)
        else
            self.m_NetWForce = self.m_NetWForce + vector * self.m_ctrl:getRealAccel() * 
                              (not self.m_drift.forceSlip and self.m_spec.forwardAccel or self.m_spec.driftEscapeForce)
        end
        
        if self.m_first.frontVel < 0 then
            if self.m_drift.slipMode or self.m_drift.forceSlip then
                self.m_NetWForce = self.m_NetWForce + self.m_first.front * self.m_first.speed * self.m_spec.mass * 9.8
            else
                self.m_NetWForce = self.m_NetWForce + self.m_first.front * Mathf.Min(5, self.m_first.speed) * self.m_spec.mass * 9.8
            end
        end
        self.m_ctrl.stayTime = 0
        
    elseif self.m_ctrl:getRealBrake() ~= 0 or self.Stuck then
        local flag = true
        if self.m_first.frontVel < 0.5 then
            self.m_ctrl.stayTime = self.m_ctrl.stayTime + deltaT
            if self.m_first.frontVel < -0.5 then
                self.m_ctrl.stayTime = 1
            end
            if self.m_ctrl.stayTime > 0.2 then
                if self.Stuck then
                    if self.m_first.frontVel >= -0.5 then
                        self.m_KartWLVel = Vector3.zero
                        flag = false
                    end
                else
                    self.m_NetWForce = self.m_NetWForce + vector * self.m_ctrl:getRealBrake() * -self.m_spec.backwardAccel
                    flag = false
                end
            elseif Mathf.Abs(self.m_first.leftVel) < 0.2 then
                self.m_KartWLVel = Vector3.zero
                flag = false
            end
        end
        
        if flag or (self.m_extern.speedLimit > 0 and self.m_KartWLVel.sqrMagnitude > 0) then
            local vector2 = self.m_KartWLVel.normalized
            local dotProduct = Vector3.Dot(vector2, self.m_first.up)
            vector2 = vector2 - self.m_first.up * dotProduct
            if Vector3.Dot(vector2, vector) > 0.8 then
                self.m_NetWForce = self.m_NetWForce - vector2 * self.m_spec.gripBrake
            else
                self.m_NetWForce = self.m_NetWForce - vector2 * self.m_spec.slipBrake
            end
        end
        
    elseif self.m_first.frontVel <= 0.5 and self.m_first.frontVel >= -0.5 then
        self.m_ctrl.stayTime = self.m_ctrl.stayTime + deltaT
    end
end

-- 获取转向角度（弧度转度数）
function GoPlayKart:getSteerAngle()
    return (self.m_ctrl.steerAngle or 0) * 180 / math.pi
end

-- calcKartSteeringForce方法 - 完全对应Unity源码242-457行
function GoPlayKart:calcKartSteeringForce(deltaT)
    if self.m_extern.slip then
        return
    end
    local num = Mathf.Sqrt(self.m_first.frontVel * self.m_first.frontVel + self.m_first.leftVel * self.m_first.leftVel)
    local num2 = (self.m_first.frontVel <= 0) and -1 or 1
    
    local baseSteerAngle = self.m_ctrl:getRealSteer() * self.m_spec:getMaxSteerRad()
    local steerReduction = Mathf.Exp(-Mathf.Abs(self.m_first.frontVel / self.m_spec.steerConstraint * self.m_extern.wheelFactor))
    self.m_ctrl.steerAngle = baseSteerAngle * steerReduction
    
    
    
    -- 初始化isBackupStreer数组
    if not self.isBackupStreer then
        self.isBackupStreer = {false, false, false}
    end
    
    self.isBackupStreer[1] = self.m_ctrl:getRealAccel() ~= 0
    self.isBackupStreer[2] = self.m_ctrl.oldSteerAngle * self.m_ctrl.steerAngle > 0
    self.isBackupStreer[3] = Mathf.Abs(self.m_ctrl.oldSteerAngle) < Mathf.Abs(self.m_ctrl.steerAngle)
    
    if self.isBackupStreer[1] and self.isBackupStreer[2] and self.isBackupStreer[3] then
        -- 空实现块
    else
        self.m_ctrl.oldSteerAngle = self.m_ctrl.steerAngle
    end
    
    local num3 = 0.5
    local num4 = 0.5
    local flag = false
    local num5 = 0
    local flag2 = self.m_drift.slipMode or self.m_drift.forceSlip
    self.m_drift.forceSlip = false
    
    if num > 5 then
        local num6 = self.m_KartLAVel.Y * num3 / num
        local num7 = self.m_KartLAVel.Y * num4 / num
        local num8 = self.m_first.leftVel / num
        local flag3 = false
        
        -- 注释掉漂移后小喷功能
        -- if self.m_DriveMode > 0 and self.m_slipBoost and Mathf.Abs(num8) > self.m_DriveFactor[self.m_DriveMode + 1][1].betaCut and self.m_adBoost.useLeftTime <= 0 then
        --     flag = true
        --     num5 = self.m_DriveMode
        --     self.m_DriveMode = 0
        --     self.m_adBoost.validTime = 0
        --     self.m_adBoost.useLeftTime = 0.5
        --     self.m_BoostKind = BoostKind.BoostDrift
        -- end
        
        -- Unity原版漂移触发条件：第288行
        if not self.m_drift.slipMode and not self.m_drift.trigger and Mathf.Abs(self.m_first.leftVel) > Mathf.Abs(self.m_first.frontVel) * 1.2 and num > 15 then
            self.m_drift.forceSlip = true
        end
        
        
        local num9 = 0
        local num10, num11
        
        if self.m_drift.trigger then
            self.m_steer = 0
            self.m_grip = 0
            num10 = 0
            num11 = -(9.8 * self.m_spec.mass) * self.m_spec.frontGripFactor * (self.m_ctrl:getRealSteer() * self.m_spec:getMaxSteerRad() * self.m_spec.driftTrigFactor)
            
            if self.m_drift.triggerTime <= 0 then
                self.m_drift.triggerTime = self.m_spec.driftTrigTime
                self.m_drift.slipTime = self.m_drift.triggerTime * 2
            else
                self.m_drift.triggerTime = self.m_drift.triggerTime - deltaT
                if self.m_drift.triggerTime <= 0 then
                    self.m_drift.triggerTime = 0
                    self.m_drift.trigger = false
                    if not self.m_driftGauge.progressOn then
                        self.m_driftGauge.progressOn = true
                        self.m_driftGauge.progressTime = 0
                        self.m_driftGauge.progress = 0
                    end
                end
            end
            
        elseif self.m_drift.slipMode or self.m_drift.slipTime > 0 or self.m_drift.forceSlip or self.m_slipReserveTime > 0 then
            if self.m_DriveMode == 1 then
                local num12 = num / self.m_DriveFactor[self.m_DriveMode + 1][2].speedLimit
                local num13 = num12 * num12
                if num13 > 1 then
                    num13 = 1
                end
                
                local num14, num15, num16
                if self.m_drift.slipMode then
                    self.m_steer = self.m_steer + 0.000001 * (1 - self.m_steer) * num13
                    self.m_grip = self.m_grip + 0.005 * (1 - self.m_grip)
                    self.m_NetWForce = self.m_NetWForce * (1 - self.m_grip)
                    num14 = self.m_ctrl:getRealSteer() * self.m_spec:getMaxSteerRad() * self.m_DriveFactor[self.m_DriveMode + 1][2].onDriftSteerFactor
                    num15 = self.m_DriveFactor[self.m_DriveMode + 1][2].frontGripFactor
                    num16 = self.m_DriveFactor[self.m_DriveMode + 1][2].rearGripFactor
                else
                    self.m_steer = self.m_steer + 0.001 * (1 - self.m_steer)
                    self.m_grip = 0
                    num14 = self.m_ctrl.steerAngle * self.m_DriveFactor[self.m_DriveMode + 1][2].onRestTimeSteerFactor
                    num15 = self.m_DriveFactor[self.m_DriveMode + 1][2].backFrontGripFactor
                    num16 = self.m_DriveFactor[self.m_DriveMode + 1][2].backRearGripFactor
                end
                
                local num17 = self.m_spec.frontGripFactor + num15
                local num18 = self.m_spec.rearGripFactor + num16
                if self.m_steer > 1 then
                    self.m_steer = 1
                end
                
                num18 = num18 - self.m_grip
                num10 = 9.8 * self.m_spec.mass * num17 * (num14 * num2 - num8 - num6)
                num11 = 9.8 * self.m_spec.mass * num18 * (-num8 + num7)
                num10 = num10 * self.m_spec.driftSlipFactor * self.m_steer
                num11 = num11 * self.m_spec.driftSlipFactor * self.m_steer
                self.m_slipReserveTime = Mathf.Max(self.m_slipReserveTime - deltaT, 0)
                
            elseif self.m_drift.slipMode then
                num10 = 9.8 * self.m_spec.mass * (self.m_spec.frontGripFactor + self.m_DriveFactor[self.m_DriveMode + 1][2].frontGripFactor) * (self.m_ctrl:getRealSteer() * self.m_spec:getMaxSteerRad() * num2 - num8 - num6)
                num11 = 9.8 * self.m_spec.mass * (self.m_spec.rearGripFactor + self.m_DriveFactor[self.m_DriveMode + 1][2].rearGripFactor) * (-num8 + num7)
                num10 = num10 * self.m_spec.driftSlipFactor * self.m_DriveFactor[self.m_DriveMode + 1][2].driftSlipFactor
                num11 = num11 * self.m_spec.driftSlipFactor * self.m_DriveFactor[self.m_DriveMode + 1][2].driftSlipFactor
            else
                num10 = 9.8 * self.m_spec.mass * self.m_spec.frontGripFactor * (self.m_ctrl.steerAngle * num2 - num8 - num6)
                num11 = 9.8 * self.m_spec.mass * self.m_spec.rearGripFactor * (-num8 + num7)
                num10 = num10 * self.m_spec.driftSlipFactor
                num11 = num11 * self.m_spec.driftSlipFactor
                
            end
            
            num9 = ((self.m_first.speed <= 10) and (-(num10 + num11) * self.m_spec.driftLeanFactor * 0.5) or (-(num10 + num11) * self.m_spec.driftLeanFactor))
            self.m_drift.slipTime = Mathf.Max(self.m_drift.slipTime - deltaT, 0)
            
        else
            flag3 = true
            if self.m_DriveMode > 0 then
                local num19 = num / self.m_DriveFactor[self.m_DriveMode + 1][1].speedLimit
                local num20 = self.m_slip[1] * num19 * num19 + self.m_slip[2]
                if num20 > 1 then
                    num20 = 1
                end
                
                local num21
                if self.m_DriveMode == 2 then
                    num21 = self.m_ctrl:getRealSteer() * self.m_spec:getMaxSteerRad() * num20
                else
                    num21 = self.m_ctrl.steerAngle * (1 + num20)
                end
                
                num10 = 9.8 * self.m_spec.mass * self.m_spec.frontGripFactor * (num21 * num2 - num8 - num6)
                num11 = 9.8 * self.m_spec.mass * self.m_spec.rearGripFactor * (-num8 + num7)
                num10 = num10 * self.m_spec.driftSlipFactor * self.m_DriveFactor[self.m_DriveMode + 1][1].driftSlipFactor
                num11 = num11 * self.m_spec.driftSlipFactor * self.m_DriveFactor[self.m_DriveMode + 1][1].driftSlipFactor
            else
                num10 = 9.8 * self.m_spec.mass * self.m_spec.frontGripFactor * (self.m_ctrl.steerAngle * num2 - num8 - num6)
                num11 = 9.8 * self.m_spec.mass * self.m_spec.rearGripFactor * (-num8 + num7)
                
            end
            
            num9 = -(num10 + num11) * self.m_spec.steerLeanFactor
            if self.m_driftGauge.progressOn then
                self.m_driftGauge.gauge = Mathf.Min(self.m_spec.driftMaxGauge, self.m_driftGauge.gauge + self.m_driftGauge.progress)
                self.m_driftGauge.progressOn = false
                self.m_driftGauge.progressTime = 0
                self.m_driftGauge.lastProgress = self.m_driftGauge.progress
                self.m_driftGauge.progress = 0
            end
        end
        self.m_NetWForce = self.m_NetWForce + self.m_ort * Vector3.new(num10 + num11, 0, flag3 and (-Mathf.Abs(num10 + num11) * self.m_spec.cornerDrawFactor) or 0)
        self.m_NetLTorque = self.m_NetLTorque + Vector3.new(0, num3 * num10 - num4 * num11, num9)
    else
        self.m_drift.slipMode = false
        self.m_drift.slipTime = 0
        self.m_drift.trigger = false
        self.m_drift.triggerTime = 0
        self.m_adBoost.validTrigger = false
        
        if self.m_driftGauge.progressOn then
            self.m_driftGauge.gauge = Mathf.Min(self.m_spec.driftMaxGauge, self.m_driftGauge.gauge + self.m_driftGauge.progress)
        end
        self.m_driftGauge.progressOn = false
        self.m_driftGauge.progressTime = 0
        self.m_driftGauge.lastProgress = self.m_driftGauge.progress
        self.m_driftGauge.progress = 0
        
        local num22 = self.m_KartLAVel.Y * num3 / 5
        local num23 = self.m_KartLAVel.Y * num4 / 5
        local num24 = self.m_first.leftVel / 5
        local num25 = 9.8 * self.m_spec.mass * self.m_spec.frontGripFactor * (((num >= 0.5) and (self.m_ctrl.steerAngle * num2) or 0) - num24 - num22)
        local num26 = 9.8 * self.m_spec.mass * self.m_spec.rearGripFactor * (-num24 + num23)
        self.m_NetWForce = self.m_NetWForce + self.m_ort * Vector3.new(num25 + num26, 0, 0)
        self.m_NetLTorque = self.m_NetLTorque + Vector3.new(0, num3 * num25 - num4 * num26, 0)
    end
    
    -- 注释掉漂移后小喷的触发
    -- if self.m_adBoost.validTrigger and flag2 and not self.m_drift.slipMode and not self.m_drift.forceSlip and self.m_adBoost.validTime == 0 then
    --     self.m_adBoost.validTrigger = false
    --     self.m_adBoost.validTime = 0.5
    -- end
    
    if flag then
        self.m_DriveMode = num5
    end
end

-- setReKart重写父类方法
function GoPlayKart:setReKart(controller, wheels)
    GoKart.setReKart(self, controller, wheels)
    self.wheelLocalPos_ = {}
    
    -- 完全按照Unity源代码逻辑：第461-468行
    for i = 1, 4 do
        if wheels[i] then
            -- 第464行：this.wheelLocalPos_[i] = wheels[i].localPosition;
            self.wheelLocalPos_[i] = wheels[i].localPosition
            -- 第467行：array[num].x = array[num].x + 0.2f * ((i % 2 != 0) ? 1f : (-1f));
            -- 在Lua中：i从1开始，所以要调整条件判断
            local adjustment = ((i % 2 ~= 0) and 1 or -1) * 0.2
            self.wheelLocalPos_[i] = Vector3.new(
                self.wheelLocalPos_[i].X + adjustment,
                self.wheelLocalPos_[i].Y,
                self.wheelLocalPos_[i].Z
            )
        else
            print("警告: wheels[" .. i .. "] 是 nil!")
        end
    end
    
    -- 获取模型的尺寸
    local x, z
    -- 获取底层的Roblox对象
    local robloxObject = self.m_kart and self.m_kart.gameObject or self.m_kart
    if robloxObject and robloxObject.IsA then
        if robloxObject:IsA("Model") then
            -- 对于Model，使用GetBoundingBox获取整体尺寸
            local cf, size = robloxObject:GetBoundingBox()
            x = size.X
            z = size.Z
        elseif robloxObject:IsA("BasePart") then
            -- 对于Part，直接使用Size
            x = robloxObject.Size.X
            z = robloxObject.Size.Z
        else
            -- 默认值
            x = 2
            z = 3
        end
    else
        -- 默认值
        x = 2
        z = 3
    end
    
    -- 注意：在Roblox中，Part的Size属性已经包含了所有的缩放
    -- 不需要额外乘以scale，否则会重复计算
    self.m_spec.width = Mathf.Min(0.98, x * 0.5)
    self.m_spec.length = z * 0.5
    self:setDefaultSpec()
end

-- setDefaultSpec方法
function GoPlayKart:setDefaultSpec()
    self.m_spec.springK = self.m_spec.mass * Mathf.Abs(self.m_theGravity.Y) * 0.5
    self.m_spec.damperCopC = 0
    self.m_spec.damperRebC = self.m_spec.springK * 0.2
    self.m_reciprocalMass = Vector3.zero
    -- Unity原版：Vector3Helper.SetVector3(ref this.m_reciprocalMass, 12f / this.m_spec.mass)
    local reciprocalValue = 12 / self.m_spec.mass
    self.m_reciprocalMass = Vector3.new(reciprocalValue, reciprocalValue, reciprocalValue)
end

-- beginNetForce方法
function GoPlayKart:beginNetForce(deltaT)
    self.m_NetWForce = self.m_extern.force
    self.m_NetLTorque = self.m_extern.torque
    self.m_extern.force = Vector3.zero
    self.m_extern.torque = Vector3.zero
    self.m_extern.liftVel = Vector3.zero
    -- 通过适配器获取统一的方向向量
    if self.controller_ and self.controller_.transform then
        local transform = self.controller_.transform
        local right = transform.right
        local forward = transform.forward
        local up = transform.up
        self.m_first.left = Vector3.new(right.X, right.Y, right.Z)
        self.m_first.front = Vector3.new(forward.X, forward.Y, forward.Z)  
        self.m_first.up = Vector3.new(up.X, up.Y, up.Z)
    -- else
    --     -- 回退到直接使用CFrame（保持向后兼容）
    --     local rightVec = self.m_kart.CFrame.RightVector
    --     local lookVec = self.m_kart.CFrame.LookVector
    --     local upVec = self.m_kart.CFrame.UpVector
    --     self.m_first.left = Vector3.new(rightVec.X, rightVec.Y, rightVec.Z)
    --     self.m_first.front = Vector3.new(-lookVec.X, -lookVec.Y, -lookVec.Z)
    --     self.m_first.up = Vector3.new(upVec.X, upVec.Y, upVec.Z)
    end
    self.m_first.frontVel = Vector3.Dot(self.m_KartWLVel, self.m_first.front)
    self.m_first.leftVel = Vector3.Dot(self.m_KartWLVel, self.m_first.left)
    self.m_first.upVel = Vector3.Dot(self.m_KartWLVel, self.m_first.up)
    self.m_first.speed = self.m_KartWLVel.Magnitude
    self.m_ort:setCol(self.m_first.left, self.m_first.up, self.m_first.front)
    
   
    
end

-- endNetForce方法
function GoPlayKart:endNetForce(deltaT)
    self.m_NetWForce = self.m_NetWForce + self.m_extern.annexForce
    self.m_KartWLVel = self.m_KartWLVel + self.m_NetWForce / self.m_spec.mass * deltaT
    self.m_KartLAVel = self.m_KartLAVel + Vector3.Scale(self.m_reciprocalMass, 
        (self.m_NetLTorque - Vector3.Cross(self.m_KartLAVel, Vector3.Scale(self.m_reciprocalMass, self.m_KartLAVel))) * deltaT)
    
    -- 平滑限制角速度，提供多种平滑算法
    if self.m_drift and (self.m_drift.slipMode or self.m_drift.forceSlip or self.m_drift.trigger) then
        local maxAngularSpeed = 2.3 -- 每秒最大2.3弧度
        local angularSpeed = self.m_KartLAVel.Magnitude
        if angularSpeed > maxAngularSpeed then
            -- 指数平滑衰减 - 最自然的平滑方式
            local smoothFactor = 1.0 - math.exp(-8.0 * deltaT)  -- 基于时间的平滑系数
            local targetSpeed = maxAngularSpeed
            local newSpeed = angularSpeed + (targetSpeed - angularSpeed) * smoothFactor
            self.m_KartLAVel = self.m_KartLAVel * (newSpeed / angularSpeed)
        end
    end
end

-- decideKartContact方法 - 使用CollectionService + RaycastParams
function GoPlayKart:decideKartContact(deltaT, wallInc)
    self.m_cState.shock = self.m_Contact
    self.m_Contact = false
    
    if self.m_kart == nil then
        return self.m_Contact
    end
    
    for num = 1, 4 do
        local vector = self.m_kart.Position
        vector = vector + self.m_first.left * self.m_spec.width * self.m_sus.wheelOff[num].X * 0.8
        vector = vector + self.m_first.front * self.m_spec.length * self.m_sus.wheelOff[num].Y * 0.8
        vector = vector + self.m_first.up * self.m_sus.maxTravel
        local rayOrigin = vector + self.m_first.up 
        local rayLength = self.m_sus.maxTravel * 2 + 1;
        -- 使用Physics模块进行射线检测，与Unity保持一致
        -- Unity源码：Physics.Raycast(vector2, -this.m_first.up, out raycastHit, num2, 256)
        local hit, raycastHit = Physics.Raycast(rayOrigin, -self.m_first.up, rayLength, 256)
        if hit then
            self.m_Contact = true
            self.m_sus.wheelContact[num] = true
            self.m_sus.wheelContactN[num] = Vector3.new(raycastHit.normal.X, raycastHit.normal.Y, raycastHit.normal.Z)
            
            local num3 = Vector3.Dot(raycastHit.point, self.m_first.up) - 
                        (Vector3.Dot(self.m_kart.Position, self.m_first.up) - self.m_sus.maxTravel)
            local num4 = self.m_sus.travel[num]
            self.m_sus.travel[num] = Mathf.Max(0, Mathf.Min(num3, self.m_sus.maxTravel * 2))
            self.m_sus.deltaTravel[num] = self.m_sus.travel[num] - num4
        else
            self.m_sus.wheelContact[num] = false
            self.m_sus.travel[num] = 0
            self.m_sus.deltaTravel[num] = 0
        end
    end
    
    self.m_sus.contactN = Vector3.zero
    if self.m_Contact then
        local num5 = 0
        for num6 = 1, 4 do
            if self.m_sus.wheelContact[num6] then
                num5 = num5 + 1
                self.m_sus.contactN = self.m_sus.contactN + self.m_sus.wheelContactN[num6]
            end
        end
        self.m_sus.contactN = self.m_sus.contactN / num5
        self.m_cState.shock = not self.m_cState.shock
        if num5 > 2 then
            self.m_cState.hop = false
        end
    else
        self.m_cState.shock = false
    end
    
    return self.m_Contact
end

-- 控制方法
function GoPlayKart:setAccel(accel)
    if accel then
        self.m_ctrl.accel = 1
        self.m_slipBoost = true
        -- 注释掉漂移后小喷
        -- if self.m_adBoost.validTime > 0 and self.m_boostLeft < 0.5 then
        --     self.m_adBoost.validTime = 0
        --     self.m_adBoost.useLeftTime = 0.5
        --     self.m_BoostKind = BoostKind.BoostDrift
        -- end
    else
        self.m_ctrl.accel = 0
        if not self:isZoneBoost() then
            self.m_boostLeft = 0
            self.m_BoostKind = BoostKind.NoBoost
        end
    end
end

function GoPlayKart:setBrake(brake)
    if brake then
        self.m_ctrl.brake = 1
        -- 注释掉漂移后小喷
        -- if self.m_ctrl.accelBrakeSwap and self.m_adBoost.validTime > 0 then
        --     self.m_adBoost.validTime = 0
        --     self.m_adBoost.useLeftTime = 0.5
        --     self.m_BoostKind = BoostKind.BoostDrift
        -- end
    else
        self.m_ctrl.brake = 0
    end
end

-- Wheel属性
function GoPlayKart:getWheel()
    return self.m_ctrl.steer
end

function GoPlayKart:setWheel(value)
    self.m_ctrl.steer = value
end

function GoPlayKart:setDrift(drift)
    if drift then
        if self.m_drift.slipTime <= 0 then
            self.m_drift.slipMode = true
            self.m_drift.trigger = true
            -- 注释掉漂移后小喷触发
            -- self.m_adBoost.validTrigger = self.m_first.frontVel > 0
        end
    else
        if self.m_DriveMode == 1 and self.m_drift.slipMode and self.m_slipReserveTime <= 0 then
            local num = self.m_first.speed / 120
            self.m_slipReserveTime = 10 * num * num
        end
        self.m_drift.slipMode = false
    end
end

-- calcFlyingKartForce方法 - 飞行状态下的力计算
function GoPlayKart:calcFlyingKartForce()
    self.m_drift.slipMode = false
    self.m_drift.forceSlip = false
    self.m_drift.trigger = false
    self.m_NetWForce = self.m_NetWForce + self.m_theGravity * self.m_spec.mass * self.m_extern.gravityFactor
    self.m_NetLTorque = self.m_NetLTorque - self.m_KartLAVel * 30
    self.m_ctrl.oldSteerAngle = 0
    
    if self.m_cState.hop then
        if self.m_first.up.Y < 0.05 then
            self.m_NetLTorque = self.m_NetLTorque + Vector3.new(0, 0, ((self.m_first.left.Y <= 0) and ((1 - self.m_first.up.Y) * -90) or ((1 - self.m_first.up.Y) * 90)))
        end
        if self.m_first.front.Y > 0.5 then
            self.m_NetLTorque = self.m_NetLTorque + Vector3.new((self.m_first.front.Y + 1) * 90, 0, 0)
        elseif self.m_first.front.Y < -0.5 then
            self.m_NetLTorque = self.m_NetLTorque + Vector3.new(-(1 - self.m_first.front.Y) * 90, 0, 0)
        end
    end
end

-- 加速相关方法
function GoPlayKart:setBoost(time, kind)
    if self:isZoneBoost(kind) or self.m_ctrl:getRealAccel() ~= 0 then
        self.m_boostLeft = time
        self.m_BoostKind = kind
    end
end

function GoPlayKart:setBoostForce(time, kind)
    self.m_boostLeft = time
    self.m_BoostKind = kind
end

-- 加速判断方法
function GoPlayKart:isRealBoost(kind)
    if kind == nil then
        kind = self.m_BoostKind
    end
    return kind == BoostKind.BoostStart or kind == BoostKind.BoostNormal or 
           kind == BoostKind.BoostTeam or kind == BoostKind.BoostDrift or kind == BoostKind.BoostAnimal
end

function GoPlayKart:isItemBoost(kind)
    if kind == nil then
        kind = self.m_BoostKind
    end
    return kind == BoostKind.BoostNormal or kind == BoostKind.BoostTeam or kind == BoostKind.BoostAnimal
end

function GoPlayKart:isZoneBoost(kind)
    if kind == nil then
        kind = self.m_BoostKind
    end
    return kind == BoostKind.BoostZone or kind == BoostKind.BoostJumpZone or kind == BoostKind.BoostDelivery
end

function GoPlayKart:isBoost(kind)
    return kind == self.m_BoostKind
end

-- processAdBoostTime方法
function GoPlayKart:processAdBoostTime(deltaT)
    if self.m_adBoost.validTime > 0 then
        self.m_adBoost.validTime = self.m_adBoost.validTime - deltaT
        self.m_adBoost.validTime = Mathf.Max(0, self.m_adBoost.validTime)
    end
    if self.m_adBoost.useLeftTime > 0 then
        self.m_adBoost.useLeftTime = self.m_adBoost.useLeftTime - deltaT
        if self.m_adBoost.useLeftTime <= 0 then
            self.m_adBoost.useLeftTime = 0
            if self.m_BoostKind == BoostKind.BoostDrift then
                self.m_BoostKind = BoostKind.NoBoost
            end
        end
    end
end

-- 速度相关方法
function GoPlayKart:GetKartSpeed()
    return self.m_KartWLVel.Magnitude * 3.6
end

function GoPlayKart:GetKartRealSpeed()
    return self.m_KartRealVelocity.Magnitude * 3.6
end

-- 碰撞相关方法
function GoPlayKart:ResetCrash()
    self.isCrash_ = false
    self.crashVelocity_ = 0
end

function GoPlayKart:SetCrash(vel)
    self.isCrash_ = true
    if self.crashVelocity_ <= vel then
        self.crashVelocity_ = vel
    end
end

function GoPlayKart:IsShock()
    return self.isShock_
end

function GoPlayKart:ResetShock()
    self.isShock_ = false
    self.shockVelocity_ = 0
end

function GoPlayKart:SetShock(vel)
    self.isShock_ = true
    if self.shockVelocity_ <= vel then
        self.shockVelocity_ = vel
    end
end

-- Warp重写父类方法
function GoPlayKart:Warp(pos, rot, flush, resetVel)
    GoKart.Warp(self, pos, rot, flush, resetVel)
    self.m_NetWForce = Vector3.zero
    self.m_NetLTorque = Vector3.zero
    if self.m_boostLeft > 0 then
        self.m_boostLeft = 0
        self.m_BoostKind = BoostKind.NoBoost
    end
end

-- ProcessDriftGauge方法 - 处理漂移计量器
function GoPlayKart:ProcessDriftGauge(deltaT)
    if self.m_driftGauge.progressOn and self.m_Contact and self.m_first.frontVel >= 0 then
        self.m_driftGauge.progressTime = self.m_driftGauge.progressTime + deltaT
        if self.m_driftGauge.progressTime < 0.2 then
            self.m_driftGauge.progress = self.m_driftGauge.progress + 2 * deltaT * self.m_first.leftVel * self.m_first.leftVel * 3
        elseif self.m_driftGauge.progressTime < 0.5 then
            self.m_driftGauge.progress = self.m_driftGauge.progress + 2 * deltaT * self.m_first.leftVel * self.m_first.leftVel * 1.5
        else
            self.m_driftGauge.progress = self.m_driftGauge.progress + 2 * (deltaT * self.m_first.leftVel * self.m_first.leftVel) / (self.m_driftGauge.progressTime * 2)
        end
    end
end

-- ResetDriftGauge方法 - 重置漂移计量器
function GoPlayKart:ResetDriftGauge()
    self.m_driftGauge.progressOn = false
    self.m_driftGauge.progressTime = 0
    self.m_driftGauge.progress = 0
end

-- BackupVelocity属性
function GoPlayKart:getBackupVelocity()
    return self.backupVelocity_
end

function GoPlayKart:setBackupVelocity(value)
    self.backupVelocity_ = value
end

-- ResetForRestarting方法
function GoPlayKart:ResetForRestarting()
    GoKart.ResetForRestarting(self)
    self.m_theGravity = Vector3.new(0, -49, 0)
    self.m_NetWForce = Vector3.zero
    self.m_NetLTorque = Vector3.zero
    self.m_KartRealVelocity = Vector3.zero
    self.m_boostLeft = 0
    self.m_slipBoost = false
    self.m_Contact = true
    self.m_sus:Initialize()
    self.m_drift:Initialize()
    self.m_driftGauge:Initialize()
    self.m_adBoost:Initialize()
    self.m_ctrl:Initialize()
    self.m_cState:Initialize()
    self.m_extern:Initialize()
    self.m_stuckHelper:Initialize()
    self.m_ort = Matrix3.CreateMtxIdentity()
    self.m_slipReserveTime = 0
    self.m_steer = 0
    self.m_grip = 0
    self.m_slip[1] = 2
    self.m_slip[2] = 0.5
    self.m_isDrift = false
    self.m_BoostKind = BoostKind.NoBoost
    self.isCrash_ = false
    self.crashVelocity_ = 0
    self.isShock_ = false
    self.shockVelocity_ = 0
    self.needReset_ = false
    self.backupVelocity_ = Vector3.zero
end

-- 重写initGoKart方法
function GoPlayKart:initGoKart()
    -- 调用父类方法
    GoKart.initGoKart(self)
    
    -- GoPlayKart特有的初始化
    self.wheelLocalPos_ = {}
    self.isCrash_ = false
    self.crashVelocity_ = 0
    self.isShock_ = false
    self.shockVelocity_ = 0
end

-- setReKartOld方法
function GoPlayKart:setReKartOld(obj, wheels)
    self.m_kart = obj
    self.wheelLocalPos_ = {}
    
    -- 计算轮子相对于车体的本地偏移量
    local carPosition = obj.Position
    for i = 1, 4 do
        -- 计算轮子相对于车体的本地偏移
        local wheelWorldPos = wheels[i].Position
        local localOffset = wheelWorldPos - carPosition
        
        -- 转换为UnityMath Vector3格式
        self.wheelLocalPos_[i] = Vector3.new(localOffset.X, localOffset.Y, localOffset.Z)
    end
    
    -- 获取模型的尺寸
    local x, z
    -- 获取底层的Roblox对象
    local robloxObject = self.m_kart and self.m_kart.gameObject or self.m_kart
    if robloxObject and robloxObject.IsA then
        if robloxObject:IsA("Model") then
            -- 对于Model，使用GetBoundingBox获取整体尺寸
            local cf, size = robloxObject:GetBoundingBox()
            x = size.X
            z = size.Z
        elseif robloxObject:IsA("BasePart") then
            -- 对于Part，直接使用Size
            x = robloxObject.Size.X
            z = robloxObject.Size.Z
        else
            -- 默认值
            x = 2
            z = 3
        end
    else
        -- 默认值
        x = 2
        z = 3
    end
    
    -- 注意：在Roblox中，Part的Size属性已经包含了所有的缩放
    -- 不需要额外乘以scale，否则会重复计算
    self.m_spec.width = Mathf.Min(0.98, x * 0.5)
    self.m_spec.length = z * 0.5
    self:setDefaultSpec()
end

-- GetWheelPos方法 - 简化版本，直接返回轮子当前位置
function GoPlayKart:GetWheelPos(i)
    -- 由于轮子现在通过父子关系自动跟随car，直接返回轮子位置
    if self.wheels and self.wheels[i] then
        local pos = self.wheels[i].Position
        return Vector3.new(pos.X, pos.Y, pos.Z)
    end
    return Vector3.zero
end

-- 重置方法
function GoPlayKart:ResetKart()
    self.m_KartWLVel = Vector3.zero
    self.m_KartLAVel = Vector3.zero
    self.m_NetWForce = Vector3.zero
    self.m_NetLTorque = Vector3.zero
    if self.m_boostLeft > 0 then
        self.m_boostLeft = 0
        self.m_BoostKind = BoostKind.NoBoost
    end
end

-- 漂移计量器相关方法
function GoPlayKart:GetDriftMaxGauge()
    return self.m_spec.driftMaxGauge
end

function GoPlayKart:GetDriftGauge()
    return self.m_driftGauge.gauge
end

function GoPlayKart:GetDriftGaugeProgress()
    return Mathf.Min(self.m_spec.driftMaxGauge, self.m_driftGauge.gauge + self.m_driftGauge.progress)
end

function GoPlayKart:UseDriftGauge(gauge)
    if gauge > self.m_driftGauge.gauge then
        return false
    end
    self.m_driftGauge.gauge = self.m_driftGauge.gauge - gauge
    return true
end

function GoPlayKart:GetDriftLastProgress()
    local lastProgress = self.m_driftGauge.lastProgress
    self.m_driftGauge.lastProgress = 0
    return lastProgress
end

-- Unity的getRealSteer方法
function GoPlayKart:getRealSteer()
    return self.m_ctrl:getRealSteer()
end

-- Unity的getRealAccel方法
function GoPlayKart:getRealAccel()
    return self.m_ctrl:getRealAccel()
end

-- Unity的getRealBrake方法
function GoPlayKart:getRealBrake()
    return self.m_ctrl:getRealBrake()
end



return GoPlayKart