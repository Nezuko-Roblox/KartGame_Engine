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
    ROTATION_SMOOTHING = 0.08,   -- 旋转平滑系数（降低以获得更平滑的旋转）
    ROTATION_SPEED_MAX = 180,    -- 最大旋转速度（度/秒）
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
    
    -- 旋转速度（用于平滑旋转）
    self.rotationVelocity = 0  -- 当前旋转速度
    
    -- 初始化位置
    if self.kartModel and self.kartModel.PrimaryPart then
        self.displayPosition = self.kartModel.PrimaryPart.Position
        self.shadowPosition = self.displayPosition
        
        -- 设置为幽灵车
        for _, part in pairs(self.kartModel:GetDescendants()) do
            if part:IsA("BasePart") then
                part.CanCollide = true
                part.CanTouch = true
                part.CanQuery = true
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
    
    -- 统一的弹簧-阻尼系统（无阶段切换）
    local toShadow = self.shadowPosition - self.displayPosition
    local distance = toShadow.Magnitude
    
    -- 根据距离决定处理方式
    if distance > SYNC_CONFIG.SNAP_DISTANCE then
        -- 距离太远，直接瞬移
        self.displayPosition = self.shadowPosition
        self.displayRotation = self.shadowRotation
        self.velocity = Vector3.new(0, 0, 0)
    else
        -- 使用统一的弹簧-阻尼系统（避免阶段切换造成的卡顿）
        
        -- 动态调整参数（基于距离）
        local distanceFactor = math.min(distance / 10, 1)  -- 0到1的平滑过渡
        
        -- 弹簧强度随距离增加（远处追得快，近处追得慢）
        local springStrength = 8.0 + distanceFactor * 12.0  -- 8-20的范围
        
        -- 阻尼系数（临界阻尼）
        local criticalDamping = 2 * math.sqrt(springStrength)
        local dampingRatio = 0.9  -- 稍微欠阻尼，更快响应
        local damping = dampingRatio * criticalDamping
        
        -- 计算力
        local springForce = toShadow * springStrength
        local dampingForce = -self.velocity * damping
        
        -- 计算加速度
        local acceleration = springForce + dampingForce
        
        -- 更新速度（带速度限制）
        self.velocity = self.velocity + acceleration * deltaTime
        
        -- 动态速度限制（距离越远，允许的速度越大）
        local maxVelocity = 30 + distanceFactor * 70  -- 30-100的范围
        if self.velocity.Magnitude > maxVelocity then
            self.velocity = self.velocity.Unit * maxVelocity
        end
        
        -- 更新位置
        self.displayPosition = self.displayPosition + self.velocity * deltaTime
        
        -- 添加微小的阻尼，防止永远振荡
        if distance < 0.5 then
            self.velocity = self.velocity * 0.98
        end
    end
    
    -- 3. 处理旋转（简单的弹簧-阻尼系统，无预测）
    local angleDiff = self.shadowRotation - self.displayRotation
    
    -- 处理360度边界（选择最短旋转路径）
    if angleDiff > 180 then
        angleDiff = angleDiff - 360
    elseif angleDiff < -180 then
        angleDiff = angleDiff + 360
    end
    
    -- 使用二阶系统（弹簧-阻尼模型）进行平滑
    local springStrength = 12.0  -- 弹簧强度（响应速度）
    local damping = 0.8          -- 阻尼系数（临界阻尼）
    
    -- 计算加速度
    local springForce = angleDiff * springStrength
    local dampingForce = -self.rotationVelocity * damping * 2 * math.sqrt(springStrength)
    local rotationAcceleration = springForce + dampingForce
    
    -- 更新速度和位置
    self.rotationVelocity = self.rotationVelocity + rotationAcceleration * deltaTime
    
    -- 限制最大旋转速度
    local maxVelocity = 360  -- 度/秒
    if math.abs(self.rotationVelocity) > maxVelocity then
        self.rotationVelocity = self.rotationVelocity / math.abs(self.rotationVelocity) * maxVelocity
    end
    
    -- 更新显示旋转
    self.displayRotation = self.displayRotation + self.rotationVelocity * deltaTime
    
    -- 确保角度在 0-360 范围内
    while self.displayRotation > 360 do
        self.displayRotation = self.displayRotation - 360
    end
    while self.displayRotation < 0 do
        self.displayRotation = self.displayRotation + 360
    end
    
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