-- PredictiveNetworkSync.lua
-- 高级预测网络同步系统 - 专门处理高延迟和不稳定网络
-- 使用卡尔曼滤波和改进的Dead Reckoning

local RunService = game:GetService("RunService")

local PredictiveNetworkSync = {}
PredictiveNetworkSync.__index = PredictiveNetworkSync

-- 配置参数
local PREDICTION_CONFIG = {
    -- 卡尔曼滤波参数
    PROCESS_NOISE = 0.1,             -- 过程噪声
    MEASUREMENT_NOISE = 0.5,         -- 测量噪声
    INITIAL_COVARIANCE = 1.0,        -- 初始协方差
    
    -- 预测参数
    MAX_PREDICTION_FRAMES = 10,      -- 最大预测帧数
    PHYSICS_TIMESTEP = 1/60,         -- 物理时间步长
    
    -- 路径预测
    PATH_PREDICTION_POINTS = 5,      -- 路径预测点数
    PATH_SMOOTHING = 0.3,            -- 路径平滑系数
    
    -- 碰撞预测
    COLLISION_LOOKAHEAD = 0.5,       -- 碰撞预测时间
    AVOIDANCE_FORCE = 10,            -- 避碰力度
}

-- 卡尔曼滤波器类
local KalmanFilter = {}
KalmanFilter.__index = KalmanFilter

function KalmanFilter.new()
    local self = setmetatable({}, KalmanFilter)
    
    -- 状态向量 [position, velocity, acceleration]
    self.state = {
        pos = Vector3.new(0, 0, 0),
        vel = Vector3.new(0, 0, 0),
        acc = Vector3.new(0, 0, 0)
    }
    
    -- 协方差矩阵（简化为标量）
    self.covariance = PREDICTION_CONFIG.INITIAL_COVARIANCE
    
    -- 噪声参数
    self.processNoise = PREDICTION_CONFIG.PROCESS_NOISE
    self.measurementNoise = PREDICTION_CONFIG.MEASUREMENT_NOISE
    
    return self
end

function KalmanFilter:predict(deltaTime)
    -- 状态预测
    local predictedPos = self.state.pos + self.state.vel * deltaTime + 
                        self.state.acc * (deltaTime * deltaTime * 0.5)
    local predictedVel = self.state.vel + self.state.acc * deltaTime
    
    -- 更新预测状态
    self.state.pos = predictedPos
    self.state.vel = predictedVel
    
    -- 更新协方差
    self.covariance = self.covariance + self.processNoise
    
    return self.state
end

function KalmanFilter:update(measurement)
    -- 计算卡尔曼增益
    local kalmanGain = self.covariance / (self.covariance + self.measurementNoise)
    
    -- 更新状态估计
    local innovation = measurement.pos - self.state.pos
    self.state.pos = self.state.pos + innovation * kalmanGain
    
    if measurement.vel then
        local velInnovation = measurement.vel - self.state.vel
        self.state.vel = self.state.vel + velInnovation * kalmanGain
    end
    
    -- 更新协方差
    self.covariance = (1 - kalmanGain) * self.covariance
    
    return self.state
end

function PredictiveNetworkSync.new(remotePlayer)
    local self = setmetatable({}, PredictiveNetworkSync)
    
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    self.rigidbodyWalker = remotePlayer.rigidbodyWalker
    
    -- 卡尔曼滤波器（每个轴一个）
    self.kalmanFilters = {
        x = KalmanFilter.new(),
        y = KalmanFilter.new(),
        z = KalmanFilter.new()
    }
    
    -- 状态历史（用于路径预测）
    self.stateHistory = {}
    self.maxHistorySize = 20
    
    -- 预测路径
    self.predictedPath = {}
    self.pathVisualization = nil
    
    -- 输入预测
    self.inputHistory = {}
    self.predictedInputs = {}
    
    -- 物理状态
    self.physicsState = {
        position = Vector3.new(0, 0, 0),
        velocity = Vector3.new(0, 0, 0),
        acceleration = Vector3.new(0, 0, 0),
        angularVelocity = Vector3.new(0, 0, 0),
        rotation = Vector3.new(0, 0, 0)
    }
    
    -- 碰撞预测
    self.predictedCollisions = {}
    
    -- 性能监控
    self.predictionAccuracy = 1.0
    self.predictionErrors = {}
    
    -- 初始化可视化（调试用）
    self:setupVisualization()
    
    return self
end

function PredictiveNetworkSync:setupVisualization()
    -- 创建路径可视化容器
    self.pathVisualization = Instance.new("Folder")
    self.pathVisualization.Name = "PredictedPath_" .. tostring(self.remotePlayer.userId)
    self.pathVisualization.Parent = workspace
    
    -- 创建预测点
    for i = 1, PREDICTION_CONFIG.PATH_PREDICTION_POINTS do
        local part = Instance.new("Part")
        part.Name = "PredictionPoint_" .. i
        part.Size = Vector3.new(1, 1, 1)
        part.Shape = Enum.PartType.Ball
        part.Material = Enum.Material.Neon
        part.BrickColor = BrickColor.new("Bright blue")
        part.Transparency = 0.5 + (i * 0.1)
        part.CanCollide = false
        part.Anchored = true
        part.Parent = self.pathVisualization
    end
end

function PredictiveNetworkSync:AddNetworkState(state)
    local currentTime = tick()
    
    -- 转换数据
    local position = self:convertToVector3(state.position)
    local velocity = self:convertToVector3(state.velocity)
    local rotation = self:convertToVector3(state.rotation)
    
    -- 创建测量数据
    local measurement = {
        pos = position,
        vel = velocity,
        time = currentTime
    }
    
    -- 更新卡尔曼滤波器
    self:updateKalmanFilters(measurement)
    
    -- 添加到历史
    table.insert(self.stateHistory, {
        position = position,
        velocity = velocity,
        rotation = rotation,
        timestamp = currentTime,
        input = state.input or {}
    })
    
    -- 限制历史大小
    if #self.stateHistory > self.maxHistorySize then
        table.remove(self.stateHistory, 1)
    end
    
    -- 更新预测精度
    self:updatePredictionAccuracy(position)
    
    -- 生成预测路径
    self:generatePredictedPath()
end

function PredictiveNetworkSync:updateKalmanFilters(measurement)
    -- 分别更新每个轴的卡尔曼滤波器
    self.kalmanFilters.x:update({
        pos = Vector3.new(measurement.pos.X, 0, 0),
        vel = measurement.vel and Vector3.new(measurement.vel.X, 0, 0) or nil
    })
    
    self.kalmanFilters.y:update({
        pos = Vector3.new(0, measurement.pos.Y, 0),
        vel = measurement.vel and Vector3.new(0, measurement.vel.Y, 0) or nil
    })
    
    self.kalmanFilters.z:update({
        pos = Vector3.new(0, 0, measurement.pos.Z),
        vel = measurement.vel and Vector3.new(0, 0, measurement.vel.Z) or nil
    })
end

function PredictiveNetworkSync:generatePredictedPath()
    self.predictedPath = {}
    
    -- 获取当前卡尔曼滤波状态
    local currentState = self:getKalmanState()
    
    -- 预测未来路径点
    for i = 1, PREDICTION_CONFIG.PATH_PREDICTION_POINTS do
        local deltaTime = PREDICTION_CONFIG.PHYSICS_TIMESTEP * i * 2
        
        -- 使用卡尔曼滤波预测
        local predictedState = self:predictFutureState(currentState, deltaTime)
        
        -- 应用路径平滑
        if #self.predictedPath > 0 then
            local lastPoint = self.predictedPath[#self.predictedPath]
            predictedState.position = lastPoint.position:Lerp(
                predictedState.position, 
                1 - PREDICTION_CONFIG.PATH_SMOOTHING
            )
        end
        
        table.insert(self.predictedPath, predictedState)
    end
    
    -- 更新可视化
    self:updatePathVisualization()
end

function PredictiveNetworkSync:getKalmanState()
    -- 组合所有轴的卡尔曼状态
    local xState = self.kalmanFilters.x.state
    local yState = self.kalmanFilters.y.state
    local zState = self.kalmanFilters.z.state
    
    return {
        position = Vector3.new(xState.pos.X, yState.pos.Y, zState.pos.Z),
        velocity = Vector3.new(xState.vel.X, yState.vel.Y, zState.vel.Z),
        acceleration = Vector3.new(xState.acc.X, yState.acc.Y, zState.acc.Z)
    }
end

function PredictiveNetworkSync:predictFutureState(currentState, deltaTime)
    -- 基于物理的预测
    local predictedPos = currentState.position + 
                         currentState.velocity * deltaTime +
                         currentState.acceleration * (deltaTime * deltaTime * 0.5)
    
    -- 考虑输入历史的影响
    if #self.inputHistory > 0 then
        local avgInput = self:getAverageInput()
        -- 根据平均输入调整预测
        local inputInfluence = Vector3.new(
            avgInput.horizontal * 5,
            0,
            avgInput.vertical * 5
        )
        predictedPos = predictedPos + inputInfluence * deltaTime
    end
    
    -- 考虑碰撞避免
    predictedPos = self:applyCollisionAvoidance(predictedPos)
    
    return {
        position = predictedPos,
        velocity = currentState.velocity + currentState.acceleration * deltaTime
    }
end

function PredictiveNetworkSync:getAverageInput()
    if #self.inputHistory == 0 then
        return {horizontal = 0, vertical = 0}
    end
    
    local sumH, sumV = 0, 0
    for _, input in ipairs(self.inputHistory) do
        sumH = sumH + (input.horizontal or 0)
        sumV = sumV + (input.vertical or 0)
    end
    
    return {
        horizontal = sumH / #self.inputHistory,
        vertical = sumV / #self.inputHistory
    }
end

function PredictiveNetworkSync:applyCollisionAvoidance(position)
    -- 简单的碰撞避免逻辑
    local avoidanceVector = Vector3.new(0, 0, 0)
    
    -- 检查与其他玩家的距离
    for _, player in pairs(game.Players:GetPlayers()) do
        if player.Character and player.Character.PrimaryPart then
            local otherPos = player.Character.PrimaryPart.Position
            local distance = (position - otherPos).Magnitude
            
            if distance < 10 and distance > 0.1 then
                -- 计算避让向量
                local direction = (position - otherPos).Unit
                local force = PREDICTION_CONFIG.AVOIDANCE_FORCE / distance
                avoidanceVector = avoidanceVector + direction * force
            end
        end
    end
    
    return position + avoidanceVector
end

function PredictiveNetworkSync:updatePathVisualization()
    if not self.pathVisualization then
        return
    end
    
    local points = self.pathVisualization:GetChildren()
    for i, point in ipairs(points) do
        if self.predictedPath[i] then
            point.Position = self.predictedPath[i].position
            point.Transparency = 0.3 + (i * 0.1)
            
            -- 根据预测精度调整颜色
            if self.predictionAccuracy > 0.8 then
                point.BrickColor = BrickColor.new("Lime green")
            elseif self.predictionAccuracy > 0.5 then
                point.BrickColor = BrickColor.new("New Yeller")
            else
                point.BrickColor = BrickColor.new("Really red")
            end
        else
            point.Transparency = 1
        end
    end
end

function PredictiveNetworkSync:updatePredictionAccuracy(actualPosition)
    if #self.predictedPath > 0 then
        local predictedPos = self.predictedPath[1].position
        local error = (actualPosition - predictedPos).Magnitude
        
        table.insert(self.predictionErrors, error)
        if #self.predictionErrors > 10 then
            table.remove(self.predictionErrors, 1)
        end
        
        -- 计算平均误差
        local avgError = 0
        for _, err in ipairs(self.predictionErrors) do
            avgError = avgError + err
        end
        avgError = avgError / #self.predictionErrors
        
        -- 转换为精度值（0-1）
        self.predictionAccuracy = math.max(0, 1 - (avgError / 10))
    end
end

function PredictiveNetworkSync:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    -- 预测下一帧状态
    for _, filter in pairs(self.kalmanFilters) do
        filter:predict(deltaTime)
    end
    
    -- 获取预测状态
    local predictedState = self:getKalmanState()
    
    -- 应用智能平滑
    local smoothingFactor = 0.1 + (0.3 * (1 - self.predictionAccuracy))
    local currentPos = self.kartModel.PrimaryPart.Position
    local targetPos = currentPos:Lerp(predictedState.position, smoothingFactor)
    
    -- 应用到模型
    self.kartModel:SetPrimaryPartCFrame(
        CFrame.new(targetPos) * 
        CFrame.Angles(0, math.rad(self.physicsState.rotation.Y), 0)
    )
    
    -- 更新物理状态
    self.physicsState.position = targetPos
    self.physicsState.velocity = predictedState.velocity
    self.physicsState.acceleration = predictedState.acceleration
end

function PredictiveNetworkSync:convertToVector3(value)
    if typeof(value) == "Vector3" then
        return value
    elseif type(value) == "table" then
        return Vector3.new(value.X or 0, value.Y or 0, value.Z or 0)
    else
        return Vector3.new(0, 0, 0)
    end
end

function PredictiveNetworkSync:GetPredictionStats()
    local avgError = 0
    if #self.predictionErrors > 0 then
        local sum = 0
        for _, err in ipairs(self.predictionErrors) do
            sum = sum + err
        end
        avgError = sum / #self.predictionErrors
    end
    
    return {
        accuracy = self.predictionAccuracy,
        avgError = avgError,
        pathPoints = #self.predictedPath
    }
end

function PredictiveNetworkSync:Destroy()
    if self.pathVisualization then
        self.pathVisualization:Destroy()
    end
end

return PredictiveNetworkSync