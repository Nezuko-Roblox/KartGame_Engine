-- LagCompensatedSync.lua
-- 延迟补偿同步系统 - 专门处理高延迟网络环境
-- 使用预测、插值和自适应算法

local RunService = game:GetService("RunService")

local LagCompensatedSync = {}
LagCompensatedSync.__index = LagCompensatedSync

function LagCompensatedSync.new(remotePlayer)
    local self = setmetatable({}, LagCompensatedSync)
    
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    
    -- 状态历史（用于预测）
    self.stateHistory = {}
    self.maxHistory = 10
    
    -- 当前渲染状态
    self.renderPosition = nil
    self.renderRotation = nil
    
    -- 预测参数
    self.velocity = Vector3.new(0, 0, 0)
    self.angularVelocity = 0
    self.lastUpdateTime = 0
    self.averageLatency = 0.1  -- 初始假设100ms延迟
    
    -- 自适应插值速度
    self.interpSpeed = 5
    self.minInterpSpeed = 3
    self.maxInterpSpeed = 20
    
    -- 平滑参数
    self.smoothFactor = 0.9  -- 更高的平滑因子
    
    self:setupGhostCar()
    
    print("[LagCompensatedSync] 初始化延迟补偿同步")
    
    return self
end

function LagCompensatedSync:setupGhostCar()
    if not self.kartModel then return end
    
    for _, part in pairs(self.kartModel:GetDescendants()) do
        if part:IsA("BasePart") then
            part.CanCollide = false
            part.CanTouch = false
            part.CanQuery = false
            if part.Transparency < 0.5 then
                part.Transparency = 0.3
            end
        end
    end
end

function LagCompensatedSync:AddNetworkState(state)
    local position = state.position
    if type(position) == "table" then
        position = Vector3.new(position.X or 0, position.Y or 0, position.Z or 0)
    end
    
    local rotation = state.rotation or Vector3.new(0, 0, 0)
    if type(rotation) == "table" then
        rotation = Vector3.new(rotation.X or 0, rotation.Y or 0, rotation.Z or 0)
    end
    
    local currentTime = tick()
    
    -- 计算延迟
    if self.lastUpdateTime > 0 then
        local timeDelta = currentTime - self.lastUpdateTime
        -- 更新平均延迟（指数移动平均）
        self.averageLatency = self.averageLatency * 0.9 + timeDelta * 0.1
        
        -- 自适应调整插值速度
        if timeDelta > 0.15 then  -- 高延迟
            self.interpSpeed = math.max(self.minInterpSpeed, self.interpSpeed - 0.5)
        else  -- 低延迟
            self.interpSpeed = math.min(self.maxInterpSpeed, self.interpSpeed + 0.5)
        end
    end
    
    -- 添加到历史
    local newState = {
        position = position,
        rotation = rotation,
        timestamp = currentTime
    }
    
    table.insert(self.stateHistory, newState)
    
    -- 限制历史大小
    while #self.stateHistory > self.maxHistory do
        table.remove(self.stateHistory, 1)
    end
    
    -- 计算速度（用于预测）
    if #self.stateHistory >= 2 then
        local prev = self.stateHistory[#self.stateHistory - 1]
        local curr = self.stateHistory[#self.stateHistory]
        local dt = curr.timestamp - prev.timestamp
        
        if dt > 0 then
            -- 计算线速度
            local newVelocity = (curr.position - prev.position) / dt
            -- 平滑速度变化
            self.velocity = self.velocity * 0.7 + newVelocity * 0.3
            
            -- 计算角速度
            local angleDiff = curr.rotation.Y - prev.rotation.Y
            if angleDiff > 180 then angleDiff = angleDiff - 360 end
            if angleDiff < -180 then angleDiff = angleDiff + 360 end
            local newAngularVel = angleDiff / dt
            self.angularVelocity = self.angularVelocity * 0.7 + newAngularVel * 0.3
        end
    end
    
    -- 初始化渲染位置
    if not self.renderPosition then
        self.renderPosition = position
        self.renderRotation = rotation
        if self.kartModel and self.kartModel.PrimaryPart then
            self.kartModel:SetPrimaryPartCFrame(
                CFrame.new(position) * CFrame.Angles(
                    math.rad(rotation.X),
                    math.rad(rotation.Y),
                    math.rad(rotation.Z)
                )
            )
        end
    end
    
    self.lastUpdateTime = currentTime
end

function LagCompensatedSync:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    if #self.stateHistory == 0 then
        return
    end
    
    local currentTime = tick()
    local latestState = self.stateHistory[#self.stateHistory]
    local timeSinceUpdate = currentTime - latestState.timestamp
    
    -- 计算目标位置（带预测）
    local targetPosition = latestState.position
    local targetRotationY = latestState.rotation.Y
    
    -- 根据延迟进行预测
    if timeSinceUpdate < 0.3 then  -- 最多预测300ms
        -- 使用速度进行位置预测
        local prediction = self.velocity * timeSinceUpdate * 0.8  -- 80%预测强度
        targetPosition = targetPosition + prediction
        
        -- 角度预测
        targetRotationY = targetRotationY + self.angularVelocity * timeSinceUpdate * 0.8
    end
    
    -- 平滑插值到目标位置
    if self.renderPosition then
        -- 计算距离
        local distance = (targetPosition - self.renderPosition).Magnitude
        
        -- 动态插值因子
        local lerpFactor = math.min(1, deltaTime * self.interpSpeed)
        
        -- 如果距离太大，增加插值速度
        if distance > 10 then
            lerpFactor = math.min(1, lerpFactor * 2)
        elseif distance > 50 then
            -- 直接传送
            self.renderPosition = targetPosition
            self.renderRotation = latestState.rotation
        else
            -- 平滑插值
            self.renderPosition = self.renderPosition:Lerp(targetPosition, lerpFactor)
            
            -- 角度插值
            local currentY = self.renderRotation.Y
            local diff = targetRotationY - currentY
            if diff > 180 then diff = diff - 360 end
            if diff < -180 then diff = diff + 360 end
            
            self.renderRotation = Vector3.new(
                latestState.rotation.X,
                currentY + diff * lerpFactor,
                latestState.rotation.Z
            )
        end
        
        -- 应用到模型
        self.kartModel:SetPrimaryPartCFrame(
            CFrame.new(self.renderPosition) * CFrame.Angles(
                math.rad(self.renderRotation.X),
                math.rad(self.renderRotation.Y),
                math.rad(self.renderRotation.Z)
            )
        )
    end
end

function LagCompensatedSync:Destroy()
    -- 清理
end

return LagCompensatedSync