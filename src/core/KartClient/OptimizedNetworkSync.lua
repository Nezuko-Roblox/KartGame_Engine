-- OptimizedNetworkSync.lua
-- 优化的网络同步系统 - 改进插值、预测和误差修正
-- 支持自适应缓冲、智能预测和平滑过渡

local RunService = game:GetService("RunService")
local TweenService = game:GetService("TweenService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")

-- 保存Roblox原生Vector3
local RobloxVector3 = Vector3
local RobloxCFrame = CFrame

local OptimizedNetworkSync = {}
OptimizedNetworkSync.__index = OptimizedNetworkSync

-- 配置参数
local SYNC_CONFIG = {
    -- 自适应缓冲区设置
    MIN_BUFFER_SIZE = 2,                -- 最小缓冲区大小
    MAX_BUFFER_SIZE = 8,                -- 最大缓冲区大小
    TARGET_BUFFER_TIME = 0.1,           -- 目标缓冲时间(100ms)
    
    -- 插值参数
    MIN_INTERPOLATION_DELAY = 0.05,     -- 最小插值延迟(50ms)
    MAX_INTERPOLATION_DELAY = 0.2,      -- 最大插值延迟(200ms)
    INTERPOLATION_SMOOTHNESS = 0.85,    -- 插值平滑度
    
    -- 预测参数
    MAX_PREDICTION_TIME = 0.15,         -- 最大预测时间(150ms)
    PREDICTION_DAMPENING = 0.7,         -- 预测衰减系数
    VELOCITY_HISTORY_SIZE = 5,          -- 速度历史记录大小
    
    -- 误差修正
    SMOOTH_CORRECTION_THRESHOLD = 2,    -- 平滑修正阈值
    FAST_CORRECTION_THRESHOLD = 10,     -- 快速修正阈值
    TELEPORT_THRESHOLD = 30,            -- 传送阈值
    
    -- 平滑参数
    POSITION_LERP_RATE = 0.12,          -- 位置插值速率
    ROTATION_LERP_RATE = 0.15,          -- 旋转插值速率
    VELOCITY_LERP_RATE = 0.2,           -- 速度插值速率
    
    -- 网络质量参数
    JITTER_WINDOW_SIZE = 10,            -- 抖动检测窗口
    PACKET_LOSS_THRESHOLD = 0.1,        -- 丢包率阈值
}

function OptimizedNetworkSync.new(remotePlayer)
    local self = setmetatable({}, OptimizedNetworkSync)
    
    -- 关联的远程玩家
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    self.rigidbodyWalker = remotePlayer.rigidbodyWalker
    
    -- 状态缓冲区
    self.stateBuffer = {}
    self.bufferSize = SYNC_CONFIG.MIN_BUFFER_SIZE
    
    -- 当前状态
    self.currentState = {
        position = RobloxVector3.new(0, 0, 0),
        rotation = RobloxVector3.new(0, 0, 0),
        velocity = RobloxVector3.new(0, 0, 0),
        acceleration = RobloxVector3.new(0, 0, 0),
        timestamp = 0
    }
    
    -- 显示状态（实际渲染的位置）
    self.displayState = {
        position = RobloxVector3.new(0, 0, 0),
        rotation = RobloxVector3.new(0, 0, 0),
        velocity = RobloxVector3.new(0, 0, 0)
    }
    
    -- 预测数据
    self.velocityHistory = {}
    self.accelerationHistory = {}
    self.predictedVelocity = RobloxVector3.new(0, 0, 0)
    self.predictedAcceleration = RobloxVector3.new(0, 0, 0)
    
    -- 网络质量监测
    self.networkQuality = {
        latency = 0,
        jitter = 0,
        packetLoss = 0,
        timestamps = {},
        latencies = {}
    }
    
    -- 自适应参数
    self.adaptiveDelay = SYNC_CONFIG.MIN_INTERPOLATION_DELAY
    self.adaptiveSmoothness = SYNC_CONFIG.INTERPOLATION_SMOOTHNESS
    
    -- 误差修正
    self.correctionVelocity = RobloxVector3.new(0, 0, 0)
    self.positionError = RobloxVector3.new(0, 0, 0)
    
    -- 初始化物理组件
    self:setupPhysicsComponents()
    
    return self
end

function OptimizedNetworkSync:setupPhysicsComponents()
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    local primaryPart = self.kartModel.PrimaryPart
    
    -- 设置为幽灵车（无碰撞）
    for _, part in pairs(self.kartModel:GetDescendants()) do
        if part:IsA("BasePart") then
            part.CanCollide = false
            part.CanTouch = false
            part.CanQuery = false
        end
    end
    
    -- 创建BodyPosition用于位置平滑
    local bodyPosition = Instance.new("BodyPosition")
    bodyPosition.MaxForce = RobloxVector3.new(10000, 10000, 10000)
    bodyPosition.P = 5000
    bodyPosition.D = 500
    bodyPosition.Parent = primaryPart
    self.bodyPosition = bodyPosition
    
    -- 创建BodyVelocity用于速度控制
    local bodyVelocity = Instance.new("BodyVelocity")
    bodyVelocity.MaxForce = RobloxVector3.new(4000, 0, 4000)
    bodyVelocity.Velocity = RobloxVector3.new(0, 0, 0)
    bodyVelocity.Parent = primaryPart
    self.bodyVelocity = bodyVelocity
    
    -- 创建BodyGyro用于旋转平滑
    local bodyGyro = Instance.new("BodyGyro")
    bodyGyro.MaxTorque = RobloxVector3.new(0, 10000, 0)
    bodyGyro.P = 5000
    bodyGyro.D = 500
    bodyGyro.Parent = primaryPart
    self.bodyGyro = bodyGyro
end

-- 添加网络状态到缓冲区（优化版）
function OptimizedNetworkSync:AddNetworkState(state)
    local currentTime = tick()
    
    -- 转换数据类型
    local position = self:convertToVector3(state.position)
    local velocity = self:convertToVector3(state.velocity)
    local rotation = self:convertToVector3(state.rotation)
    
    -- 计算加速度（如果有前一个状态）
    local acceleration = RobloxVector3.new(0, 0, 0)
    if #self.stateBuffer > 0 then
        local lastState = self.stateBuffer[#self.stateBuffer]
        local timeDelta = currentTime - lastState.localTime
        if timeDelta > 0 then
            acceleration = (velocity - lastState.velocity) / timeDelta
        end
    end
    
    -- 创建状态快照
    local snapshot = {
        position = position,
        velocity = velocity,
        acceleration = acceleration,
        rotation = rotation,
        serverTime = state.timestamp or currentTime,
        localTime = currentTime,
        deltaTime = state.deltaTime or 0.05
    }
    
    -- 更新网络质量指标
    self:updateNetworkQuality(snapshot)
    
    -- 添加到缓冲区
    table.insert(self.stateBuffer, snapshot)
    
    -- 更新速度和加速度历史
    self:updateVelocityHistory(velocity, acceleration)
    
    -- 自适应调整缓冲区大小
    self:adjustBufferSize()
    
    -- 清理旧数据
    self:cleanupOldStates(currentTime)
end

function OptimizedNetworkSync:convertToVector3(value)
    if typeof(value) == "Vector3" then
        return value
    elseif type(value) == "table" then
        return RobloxVector3.new(value.X or 0, value.Y or 0, value.Z or 0)
    else
        return RobloxVector3.new(0, 0, 0)
    end
end

-- 更新网络质量指标
function OptimizedNetworkSync:updateNetworkQuality(snapshot)
    local currentTime = snapshot.localTime
    
    -- 记录时间戳
    table.insert(self.networkQuality.timestamps, currentTime)
    
    -- 计算延迟
    if #self.stateBuffer > 0 then
        local lastState = self.stateBuffer[#self.stateBuffer]
        local timeDiff = currentTime - lastState.localTime
        table.insert(self.networkQuality.latencies, timeDiff)
    end
    
    -- 保持窗口大小
    if #self.networkQuality.timestamps > SYNC_CONFIG.JITTER_WINDOW_SIZE then
        table.remove(self.networkQuality.timestamps, 1)
    end
    if #self.networkQuality.latencies > SYNC_CONFIG.JITTER_WINDOW_SIZE then
        table.remove(self.networkQuality.latencies, 1)
    end
    
    -- 计算平均延迟和抖动
    if #self.networkQuality.latencies > 2 then
        local sum = 0
        local variance = 0
        for _, latency in ipairs(self.networkQuality.latencies) do
            sum = sum + latency
        end
        self.networkQuality.latency = sum / #self.networkQuality.latencies
        
        -- 计算抖动（标准差）
        for _, latency in ipairs(self.networkQuality.latencies) do
            variance = variance + (latency - self.networkQuality.latency) ^ 2
        end
        self.networkQuality.jitter = math.sqrt(variance / #self.networkQuality.latencies)
    end
end

-- 更新速度历史
function OptimizedNetworkSync:updateVelocityHistory(velocity, acceleration)
    table.insert(self.velocityHistory, velocity)
    table.insert(self.accelerationHistory, acceleration)
    
    -- 限制历史大小
    if #self.velocityHistory > SYNC_CONFIG.VELOCITY_HISTORY_SIZE then
        table.remove(self.velocityHistory, 1)
    end
    if #self.accelerationHistory > SYNC_CONFIG.VELOCITY_HISTORY_SIZE then
        table.remove(self.accelerationHistory, 1)
    end
    
    -- 计算平均速度和加速度（用于预测）
    if #self.velocityHistory > 0 then
        local sumVel = RobloxVector3.new(0, 0, 0)
        local sumAcc = RobloxVector3.new(0, 0, 0)
        
        for _, vel in ipairs(self.velocityHistory) do
            sumVel = sumVel + vel
        end
        for _, acc in ipairs(self.accelerationHistory) do
            sumAcc = sumAcc + acc
        end
        
        self.predictedVelocity = sumVel / #self.velocityHistory
        if #self.accelerationHistory > 0 then
            self.predictedAcceleration = sumAcc / #self.accelerationHistory
        end
    end
end

-- 自适应调整缓冲区大小
function OptimizedNetworkSync:adjustBufferSize()
    -- 根据网络抖动调整缓冲区大小
    if self.networkQuality.jitter > 0.1 then
        self.bufferSize = math.min(self.bufferSize + 1, SYNC_CONFIG.MAX_BUFFER_SIZE)
    elseif self.networkQuality.jitter < 0.05 and self.bufferSize > SYNC_CONFIG.MIN_BUFFER_SIZE then
        self.bufferSize = self.bufferSize - 1
    end
    
    -- 根据网络质量调整插值延迟
    local qualityFactor = math.min(self.networkQuality.jitter * 2, 1)
    self.adaptiveDelay = SYNC_CONFIG.MIN_INTERPOLATION_DELAY + 
                         (SYNC_CONFIG.MAX_INTERPOLATION_DELAY - SYNC_CONFIG.MIN_INTERPOLATION_DELAY) * qualityFactor
    
    -- 调整平滑度
    self.adaptiveSmoothness = SYNC_CONFIG.INTERPOLATION_SMOOTHNESS * (1 - qualityFactor * 0.3)
end

-- 清理旧状态
function OptimizedNetworkSync:cleanupOldStates(currentTime)
    local cutoffTime = currentTime - 1.0
    
    while #self.stateBuffer > self.bufferSize do
        table.remove(self.stateBuffer, 1)
    end
    
    while #self.stateBuffer > 0 and self.stateBuffer[1].localTime < cutoffTime do
        table.remove(self.stateBuffer, 1)
    end
end

-- 主更新函数
function OptimizedNetworkSync:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    if #self.stateBuffer < 2 then
        return
    end
    
    local currentTime = tick()
    local renderTime = currentTime - self.adaptiveDelay
    
    -- 获取插值状态
    local interpolatedState = self:getInterpolatedState(renderTime)
    
    -- 应用预测（如果需要）
    if interpolatedState.needsPrediction then
        interpolatedState = self:applyPrediction(interpolatedState, currentTime - renderTime)
    end
    
    -- 计算误差并应用修正
    self:calculateError(interpolatedState)
    interpolatedState = self:applyErrorCorrection(interpolatedState, deltaTime)
    
    -- 平滑过渡到目标状态
    self:smoothTransition(interpolatedState, deltaTime)
    
    -- 应用到模型
    self:applyStateToModel(self.displayState)
end

-- 获取插值状态
function OptimizedNetworkSync:getInterpolatedState(renderTime)
    local state1, state2 = nil, nil
    local needsPrediction = false
    
    -- 查找插值区间
    for i = 1, #self.stateBuffer - 1 do
        if self.stateBuffer[i].localTime <= renderTime and 
           self.stateBuffer[i + 1].localTime >= renderTime then
            state1 = self.stateBuffer[i]
            state2 = self.stateBuffer[i + 1]
            break
        end
    end
    
    if state1 and state2 then
        -- 立方插值（更平滑）
        local timeDiff = state2.localTime - state1.localTime
        if timeDiff > 0 then
            local t = (renderTime - state1.localTime) / timeDiff
            return self:cubicInterpolate(state1, state2, t)
        else
            return state2
        end
    else
        -- 需要预测
        needsPrediction = true
        local latestState = self.stateBuffer[#self.stateBuffer]
        return {
            position = latestState.position,
            velocity = latestState.velocity,
            rotation = latestState.rotation,
            acceleration = latestState.acceleration,
            needsPrediction = true,
            baseTime = latestState.localTime
        }
    end
end

-- 立方插值（使用Catmull-Rom样条）
function OptimizedNetworkSync:cubicInterpolate(state1, state2, t)
    -- 使用Hermite插值以获得更平滑的曲线
    local t2 = t * t
    local t3 = t2 * t
    
    -- Hermite基函数
    local h1 = 2*t3 - 3*t2 + 1
    local h2 = -2*t3 + 3*t2
    local h3 = t3 - 2*t2 + t
    local h4 = t3 - t2
    
    -- 计算切线（使用速度作为导数）
    local tangent1 = state1.velocity * 0.5
    local tangent2 = state2.velocity * 0.5
    
    -- 插值位置
    local position = state1.position * h1 + state2.position * h2 + 
                    tangent1 * h3 + tangent2 * h4
    
    -- 插值速度
    local velocity = state1.velocity:Lerp(state2.velocity, t)
    
    -- 插值旋转（使用球面线性插值）
    local rotation = RobloxVector3.new(
        self:lerpAngle(state1.rotation.X, state2.rotation.X, t),
        self:lerpAngle(state1.rotation.Y, state2.rotation.Y, t),
        self:lerpAngle(state1.rotation.Z, state2.rotation.Z, t)
    )
    
    return {
        position = position,
        velocity = velocity,
        rotation = rotation,
        acceleration = state1.acceleration:Lerp(state2.acceleration, t),
        needsPrediction = false
    }
end

-- 应用预测
function OptimizedNetworkSync:applyPrediction(state, deltaTime)
    -- 限制预测时间
    deltaTime = math.min(deltaTime, SYNC_CONFIG.MAX_PREDICTION_TIME)
    
    -- 使用二阶预测（考虑加速度）
    local predictedPos = state.position + 
                         self.predictedVelocity * deltaTime * SYNC_CONFIG.PREDICTION_DAMPENING +
                         self.predictedAcceleration * (deltaTime * deltaTime * 0.5) * SYNC_CONFIG.PREDICTION_DAMPENING
    
    -- 预测速度
    local predictedVel = self.predictedVelocity + self.predictedAcceleration * deltaTime
    
    return {
        position = predictedPos,
        velocity = predictedVel,
        rotation = state.rotation,
        acceleration = self.predictedAcceleration,
        needsPrediction = false
    }
end

-- 计算误差
function OptimizedNetworkSync:calculateError(targetState)
    if not self.kartModel.PrimaryPart then
        return
    end
    
    local currentPos = self.kartModel.PrimaryPart.Position
    self.positionError = targetState.position - currentPos
end

-- 应用误差修正
function OptimizedNetworkSync:applyErrorCorrection(state, deltaTime)
    local errorMagnitude = self.positionError.Magnitude
    
    if errorMagnitude > SYNC_CONFIG.TELEPORT_THRESHOLD then
        -- 立即传送
        return state
    elseif errorMagnitude > SYNC_CONFIG.FAST_CORRECTION_THRESHOLD then
        -- 快速修正
        local correctionForce = self.positionError * 0.3
        state.position = state.position + correctionForce
        self.correctionVelocity = correctionForce / deltaTime
    elseif errorMagnitude > SYNC_CONFIG.SMOOTH_CORRECTION_THRESHOLD then
        -- 平滑修正
        local correctionForce = self.positionError * 0.1
        self.correctionVelocity = self.correctionVelocity:Lerp(correctionForce / deltaTime, 0.5)
        state.velocity = state.velocity + self.correctionVelocity
    else
        -- 误差很小，逐渐减少修正
        self.correctionVelocity = self.correctionVelocity * 0.9
    end
    
    return state
end

-- 平滑过渡
function OptimizedNetworkSync:smoothTransition(targetState, deltaTime)
    -- 使用自适应插值速率
    local posLerpRate = SYNC_CONFIG.POSITION_LERP_RATE * (2 - self.adaptiveSmoothness)
    local rotLerpRate = SYNC_CONFIG.ROTATION_LERP_RATE * (2 - self.adaptiveSmoothness)
    local velLerpRate = SYNC_CONFIG.VELOCITY_LERP_RATE * (2 - self.adaptiveSmoothness)
    
    -- 平滑插值到目标状态
    self.displayState.position = self.displayState.position:Lerp(targetState.position, posLerpRate)
    self.displayState.velocity = self.displayState.velocity:Lerp(targetState.velocity, velLerpRate)
    
    -- 旋转使用特殊插值
    self.displayState.rotation = RobloxVector3.new(
        self:lerpAngle(self.displayState.rotation.X, targetState.rotation.X, rotLerpRate),
        self:lerpAngle(self.displayState.rotation.Y, targetState.rotation.Y, rotLerpRate),
        self:lerpAngle(self.displayState.rotation.Z, targetState.rotation.Z, rotLerpRate)
    )
end

-- 应用状态到模型
function OptimizedNetworkSync:applyStateToModel(state)
    if not self.kartModel.PrimaryPart then
        return
    end
    
    -- 使用BodyPosition和BodyGyro
    if self.bodyPosition then
        self.bodyPosition.Position = state.position
        
        -- 动态调整力度
        local errorMag = self.positionError.Magnitude
        if errorMag > 5 then
            self.bodyPosition.MaxForce = RobloxVector3.new(15000, 15000, 15000)
            self.bodyPosition.P = 8000
        else
            self.bodyPosition.MaxForce = RobloxVector3.new(10000, 10000, 10000)
            self.bodyPosition.P = 5000
        end
    end
    
    if self.bodyVelocity then
        -- 更新速度
        self.bodyVelocity.Velocity = state.velocity * 0.5  -- 减少速度影响
    end
    
    if self.bodyGyro then
        self.bodyGyro.CFrame = RobloxCFrame.Angles(
            0,
            math.rad(state.rotation.Y),
            0
        )
    end
end

-- 角度插值辅助函数
function OptimizedNetworkSync:lerpAngle(a, b, t)
    local diff = b - a
    if diff > 180 then
        diff = diff - 360
    elseif diff < -180 then
        diff = diff + 360
    end
    return a + diff * t
end

-- 获取网络统计信息
function OptimizedNetworkSync:GetNetworkStats()
    return {
        latency = self.networkQuality.latency,
        jitter = self.networkQuality.jitter,
        bufferSize = self.bufferSize,
        adaptiveDelay = self.adaptiveDelay,
        packetCount = #self.stateBuffer
    }
end

-- 清理
function OptimizedNetworkSync:Destroy()
    if self.bodyPosition then
        self.bodyPosition:Destroy()
    end
    if self.bodyVelocity then
        self.bodyVelocity:Destroy()
    end
    if self.bodyGyro then
        self.bodyGyro:Destroy()
    end
end

return OptimizedNetworkSync