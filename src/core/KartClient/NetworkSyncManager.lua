-- NetworkSyncManager.lua
-- 实现客户端预测和服务器调和的网络同步管理器
-- 基于Dead Reckoning算法和插值技术

local RunService = game:GetService("RunService")

local NetworkSyncManager = {}
NetworkSyncManager.__index = NetworkSyncManager

-- 配置参数
local SYNC_CONFIG = {
    -- 插值缓冲区设置
    INTERPOLATION_BUFFER_SIZE = 0.1,  -- 100ms的缓冲区
    MAX_EXTRAPOLATION_TIME = 0.2,     -- 最大外推时间200ms
    
    -- 误差修正
    POSITION_THRESHOLD = 0.5,         -- 位置误差阈值
    SNAP_THRESHOLD = 20,               -- 直接传送阈值
    
    -- 平滑参数
    POSITION_SMOOTHING = 0.15,        -- 位置平滑系数
    ROTATION_SMOOTHING = 0.2,         -- 旋转平滑系数
}

function NetworkSyncManager.new(remotePlayer)
    local self = setmetatable({}, NetworkSyncManager)
    
    -- 关联的远程玩家
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    self.rigidbodyWalker = remotePlayer.rigidbodyWalker
    
    -- 状态缓冲区（用于插值）
    self.stateBuffer = {}
    self.maxBufferSize = 20
    
    -- 当前插值状态
    self.currentState = {
        position = Vector3.new(0, 0, 0),
        rotation = Vector3.new(0, 0, 0),
        velocity = Vector3.new(0, 0, 0),
        timestamp = 0
    }
    
    -- 预测状态
    self.predictedState = {
        position = Vector3.new(0, 0, 0),
        rotation = Vector3.new(0, 0, 0),
        velocity = Vector3.new(0, 0, 0)
    }
    
    -- Dead Reckoning参数
    self.lastReceivedState = nil
    self.lastReceivedTime = 0
    
    return self
end

-- 添加新的网络状态到缓冲区
function NetworkSyncManager:AddNetworkState(state)
    -- 转换数据类型
    local position = state.position
    if type(position) == "table" then
        position = Vector3.new(position.X or 0, position.Y or 0, position.Z or 0)
    end
    
    local velocity = state.velocity or Vector3.new(0, 0, 0)
    if type(velocity) == "table" then
        velocity = Vector3.new(velocity.X or 0, velocity.Y or 0, velocity.Z or 0)
    end
    
    local rotation = state.rotation or Vector3.new(0, 0, 0)
    if type(rotation) == "table" then
        rotation = Vector3.new(rotation.X or 0, rotation.Y or 0, rotation.Z or 0)
    end
    
    -- 创建状态快照
    local snapshot = {
        position = position,
        velocity = velocity,
        rotation = rotation,
        timestamp = state.timestamp or tick(),
        serverTime = tick()
    }
    
    -- 添加到缓冲区
    table.insert(self.stateBuffer, snapshot)
    
    -- 限制缓冲区大小
    if #self.stateBuffer > self.maxBufferSize then
        table.remove(self.stateBuffer, 1)
    end
    
    -- 更新最后接收的状态
    self.lastReceivedState = snapshot
    self.lastReceivedTime = tick()
end

-- 获取插值时间（考虑缓冲区延迟）
function NetworkSyncManager:GetInterpolationTime()
    local currentTime = tick()
    local bufferTime = currentTime - SYNC_CONFIG.INTERPOLATION_BUFFER_SIZE
    return bufferTime
end

-- 插值计算
function NetworkSyncManager:InterpolateStates(state1, state2, t)
    local position = state1.position:Lerp(state2.position, t)
    local rotation = Vector3.new(
        self:LerpAngle(state1.rotation.X, state2.rotation.X, t),
        self:LerpAngle(state1.rotation.Y, state2.rotation.Y, t),
        self:LerpAngle(state1.rotation.Z, state2.rotation.Z, t)
    )
    
    -- 速度使用线性插值
    local velocity = state1.velocity:Lerp(state2.velocity, t)
    
    return {
        position = position,
        rotation = rotation,
        velocity = velocity
    }
end

-- 角度插值（处理360度循环）
function NetworkSyncManager:LerpAngle(a, b, t)
    local diff = b - a
    if diff > 180 then
        diff = diff - 360
    elseif diff < -180 then
        diff = diff + 360
    end
    return a + diff * t
end

-- 外推计算（Dead Reckoning）
function NetworkSyncManager:ExtrapolateState(state, deltaTime)
    -- 限制外推时间
    deltaTime = math.min(deltaTime, SYNC_CONFIG.MAX_EXTRAPOLATION_TIME)
    
    -- 基于速度外推位置
    local extrapolatedPos = state.position + state.velocity * deltaTime
    
    return {
        position = extrapolatedPos,
        rotation = state.rotation,
        velocity = state.velocity
    }
end

-- 更新函数（每帧调用）
function NetworkSyncManager:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    -- 如果没有接收到任何状态，跳过
    if #self.stateBuffer == 0 then
        return
    end
    
    local currentTime = tick()
    local interpolationTime = self:GetInterpolationTime()
    
    -- 查找用于插值的两个状态
    local state1, state2 = nil, nil
    
    for i = 1, #self.stateBuffer - 1 do
        if self.stateBuffer[i].serverTime <= interpolationTime and 
           self.stateBuffer[i + 1].serverTime >= interpolationTime then
            state1 = self.stateBuffer[i]
            state2 = self.stateBuffer[i + 1]
            break
        end
    end
    
    local targetState
    
    if state1 and state2 then
        -- 插值模式
        local timeDiff = state2.serverTime - state1.serverTime
        if timeDiff > 0 then
            local t = (interpolationTime - state1.serverTime) / timeDiff
            t = math.max(0, math.min(1, t))
            targetState = self:InterpolateStates(state1, state2, t)
        else
            targetState = state2
        end
    elseif #self.stateBuffer > 0 then
        -- 外推模式（使用最新状态）
        local latestState = self.stateBuffer[#self.stateBuffer]
        local timeSinceLastUpdate = currentTime - latestState.serverTime
        
        if timeSinceLastUpdate < SYNC_CONFIG.MAX_EXTRAPOLATION_TIME then
            targetState = self:ExtrapolateState(latestState, timeSinceLastUpdate)
        else
            -- 时间太长，使用最后已知状态
            targetState = latestState
        end
    end
    
    if targetState then
        self:ApplyState(targetState)
    end
    
    -- 清理旧的缓冲区数据
    self:CleanupBuffer(currentTime)
end

-- 应用状态到模型
function NetworkSyncManager:ApplyState(state)
    if not self.kartModel.PrimaryPart then
        return
    end
    
    local currentPos = self.kartModel.PrimaryPart.Position
    local targetPos = state.position
    
    -- 计算误差
    local distance = (targetPos - currentPos).Magnitude
    
    if distance > SYNC_CONFIG.SNAP_THRESHOLD then
        -- 误差太大，直接传送
        self.kartModel:SetPrimaryPartCFrame(CFrame.new(targetPos) * CFrame.Angles(
            math.rad(state.rotation.X),
            math.rad(state.rotation.Y),
            math.rad(state.rotation.Z)
        ))
    elseif distance > SYNC_CONFIG.POSITION_THRESHOLD then
        -- 平滑修正
        local smoothedPos = currentPos:Lerp(targetPos, SYNC_CONFIG.POSITION_SMOOTHING)
        
        -- 平滑旋转
        local currentCFrame = self.kartModel.PrimaryPart.CFrame
        local currentRotX, currentRotY, currentRotZ = currentCFrame:ToEulerAnglesXYZ()
        
        local smoothedRotY = self:LerpAngle(
            math.deg(currentRotY),
            state.rotation.Y,
            SYNC_CONFIG.ROTATION_SMOOTHING
        )
        
        self.kartModel:SetPrimaryPartCFrame(
            CFrame.new(smoothedPos) * 
            CFrame.Angles(
                math.rad(state.rotation.X),
                math.rad(smoothedRotY),
                math.rad(state.rotation.Z)
            )
        )
    end
    
    -- 更新速度（如果需要）
    if self.rigidbodyWalker and self.rigidbodyWalker.goPlayKart_ then
        -- 可以在这里更新速度预测
    end
end

-- 清理过期的缓冲区数据
function NetworkSyncManager:CleanupBuffer(currentTime)
    local cutoffTime = currentTime - 1.0  -- 保留最近1秒的数据
    
    while #self.stateBuffer > 0 and self.stateBuffer[1].serverTime < cutoffTime do
        table.remove(self.stateBuffer, 1)
    end
end

return NetworkSyncManager