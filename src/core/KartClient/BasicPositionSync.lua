-- BasicPositionSync.lua
-- 使用影子追随算法的位置同步方案

local RunService = game:GetService("RunService")

local BasicPositionSync = {}
BasicPositionSync.__index = BasicPositionSync

-- 配置参数（优化后）
local SYNC_CONFIG = {
    SHADOW_DISTANCE = 5,          -- 影子跟随距离
    FOLLOW_SPEED = 50,            -- 跟随速度（单位/秒）
    SNAP_DISTANCE = 100,          -- 瞬移距离阈值
    SMOOTHING = 0.9,              -- 平滑系数（0-1，越大越平滑）
    MAX_SPEED_MULTIPLIER = 2,    -- 最大速度倍数（更平滑的加速）
    ROTATION_SMOOTHING = 0.15,   -- 旋转平滑系数
}

function BasicPositionSync.new(remotePlayer)
    local self = setmetatable({}, BasicPositionSync)
    
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    
    -- 影子位置（网络接收的实际位置）
    self.shadowPosition = Vector3.new(0, 0, 0)
    self.shadowRotation = 0
    
    -- 显示位置（平滑后的位置）
    self.displayPosition = Vector3.new(0, 0, 0)
    self.displayRotation = 0
    
    -- 速度（用于平滑移动）
    self.velocity = Vector3.new(0, 0, 0)
    
    -- 初始化位置
    if self.kartModel and self.kartModel.PrimaryPart then
        self.displayPosition = self.kartModel.PrimaryPart.Position
        self.shadowPosition = self.displayPosition
        
        -- 设置为幽灵车
        for _, part in pairs(self.kartModel:GetDescendants()) do
            if part:IsA("BasePart") then
                part.CanCollide = false
                part.CanTouch = false
                part.CanQuery = false
            end
        end
    end
    
    return self
end

function BasicPositionSync:AddNetworkState(state)
    -- 更新影子位置（网络接收的实际位置）
    if state.position then
        if type(state.position) == "table" then
            self.shadowPosition = Vector3.new(
                state.position.X or 0,
                state.position.Y or 0,
                state.position.Z or 0
            )
        else
            self.shadowPosition = state.position
        end
    end
    
    -- 更新影子旋转
    if state.rotation then
        if type(state.rotation) == "table" then
            self.shadowRotation = state.rotation.Y or 0
        elseif typeof(state.rotation) == "Vector3" then
            self.shadowRotation = state.rotation.Y
        else
            self.shadowRotation = state.rotation
        end
    end
end

function BasicPositionSync:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    -- 影子追随算法
    -- 1. 计算到影子的距离和方向
    local toShadow = self.shadowPosition - self.displayPosition
    local distance = toShadow.Magnitude
    
    -- 2. 根据距离决定处理方式
    if distance > SYNC_CONFIG.SNAP_DISTANCE then
        -- 距离太远，直接瞬移
        self.displayPosition = self.shadowPosition
        self.displayRotation = self.shadowRotation
        self.velocity = Vector3.new(0, 0, 0)
        
    elseif distance > SYNC_CONFIG.SHADOW_DISTANCE then
        -- 超过影子距离，加速追赶
        local direction = toShadow.Unit
        
        -- 使用更平滑的速度曲线（平方根函数）
        local distanceRatio = math.sqrt(distance / SYNC_CONFIG.SHADOW_DISTANCE)
        local speed = SYNC_CONFIG.FOLLOW_SPEED * math.min(distanceRatio, SYNC_CONFIG.MAX_SPEED_MULTIPLIER)
        
        -- 计算目标速度
        local targetVelocity = direction * speed
        
        -- 平滑速度变化（避免突然加速/减速）
        self.velocity = self.velocity * SYNC_CONFIG.SMOOTHING + targetVelocity * (1 - SYNC_CONFIG.SMOOTHING)
        
        -- 更新位置
        local moveDistance = self.velocity.Magnitude * deltaTime
        if moveDistance > distance then
            -- 避免超过目标
            self.displayPosition = self.shadowPosition
        else
            self.displayPosition = self.displayPosition + self.velocity * deltaTime
        end
        
    else
        -- 在影子距离内，缓慢跟随
        if distance > 0.1 then
            -- 使用改进的弹簧阻尼效果
            local springStrength = 3.0  -- 增强弹簧力以提高响应速度
            local dampingFactor = 0.7   -- 增加阻尼以减少振荡
            
            local springForce = toShadow * springStrength
            local dampingForce = -self.velocity * dampingFactor
            
            -- 更新速度
            local acceleration = springForce + dampingForce
            self.velocity = self.velocity + acceleration * deltaTime
            
            -- 限制最大速度
            if self.velocity.Magnitude > SYNC_CONFIG.FOLLOW_SPEED then
                self.velocity = self.velocity.Unit * SYNC_CONFIG.FOLLOW_SPEED
            end
            
            -- 更新位置
            self.displayPosition = self.displayPosition + self.velocity * deltaTime
        else
            -- 非常接近，逐渐停止
            self.velocity = self.velocity * 0.95  -- 更快的减速
        end
    end
    
    -- 3. 处理旋转（简单插值）
    local angleDiff = self.shadowRotation - self.displayRotation
    -- 处理360度边界
    if angleDiff > 180 then
        angleDiff = angleDiff - 360
    elseif angleDiff < -180 then
        angleDiff = angleDiff + 360
    end
    
    -- 平滑旋转（使用配置的平滑系数）
    self.displayRotation = self.displayRotation + angleDiff * SYNC_CONFIG.ROTATION_SMOOTHING
    
    -- 4. 应用到模型
    self.kartModel:SetPrimaryPartCFrame(
        CFrame.new(self.displayPosition) * 
        CFrame.Angles(0, math.rad(self.displayRotation), 0)
    )
end

function BasicPositionSync:Destroy()
    -- 清理
end

return BasicPositionSync