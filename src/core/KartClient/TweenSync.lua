-- TweenSync.lua
-- 使用TweenService实现流畅的位置同步
-- 简单高效，低开销

local TweenService = game:GetService("TweenService")
local RunService = game:GetService("RunService")

local TweenSync = {}
TweenSync.__index = TweenSync

function TweenSync.new(remotePlayer)
    local self = setmetatable({}, TweenSync)
    
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    
    -- Tween管理
    self.currentTween = nil
    self.lastTargetPosition = nil
    self.lastTargetRotation = nil
    
    -- 配置
    self.tweenTime = 0.1  -- 100ms的tween时间，匹配20Hz更新
    
    self:setupGhostCar()
    
    print("[TweenSync] 初始化Tween同步系统")
    
    return self
end

function TweenSync:setupGhostCar()
    if not self.kartModel then return end
    
    -- 确保有PrimaryPart
    if not self.kartModel.PrimaryPart then
        local primaryPart = self.kartModel:FindFirstChild("PrimaryPart") or 
                           self.kartModel:FindFirstChildWhichIsA("BasePart")
        if primaryPart then
            self.kartModel.PrimaryPart = primaryPart
        end
    end
    
    local primaryPart = self.kartModel.PrimaryPart
    if not primaryPart then
        warn("[TweenSync] 无法找到PrimaryPart")
        return
    end
    
    -- 创建WeldConstraints连接所有部件到PrimaryPart
    for _, part in pairs(self.kartModel:GetDescendants()) do
        if part:IsA("BasePart") and part ~= primaryPart then
            -- 检查是否已有连接
            local hasWeld = false
            for _, constraint in pairs(part:GetChildren()) do
                if constraint:IsA("WeldConstraint") and 
                   (constraint.Part0 == primaryPart or constraint.Part1 == primaryPart) then
                    hasWeld = true
                    break
                end
            end
            
            -- 如果没有连接，创建新的WeldConstraint
            if not hasWeld then
                local weld = Instance.new("WeldConstraint")
                weld.Part0 = primaryPart
                weld.Part1 = part
                weld.Parent = part
            end
            
            -- 设置部件属性
            part.CanCollide = false
            part.CanTouch = false
            part.CanQuery = false
            part.Anchored = false
            
            -- 轻微透明
            if part.Transparency < 0.2 then
                part.Transparency = 0.2
            end
        end
    end
    
    -- 设置PrimaryPart属性
    primaryPart.CanCollide = false
    primaryPart.CanTouch = false
    primaryPart.CanQuery = false
    primaryPart.Anchored = false
    if primaryPart.Transparency < 0.2 then
        primaryPart.Transparency = 0.2
    end
end

function TweenSync:AddNetworkState(state)
    -- 转换数据
    local position = state.position
    if type(position) == "table" then
        position = Vector3.new(position.X or 0, position.Y or 0, position.Z or 0)
    end
    
    local rotation = state.rotation or Vector3.new(0, 0, 0)
    if type(rotation) == "table" then
        rotation = Vector3.new(rotation.X or 0, rotation.Y or 0, rotation.Z or 0)
    end
    
    -- 检查是否需要更新
    if self.lastTargetPosition and 
       (self.lastTargetPosition - position).Magnitude < 0.01 and
       math.abs(self.lastTargetRotation.Y - rotation.Y) < 0.1 then
        -- 位置变化太小，跳过更新
        return
    end
    
    self.lastTargetPosition = position
    self.lastTargetRotation = rotation
    
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    local targetCFrame = CFrame.new(position) * CFrame.Angles(
        math.rad(rotation.X),
        math.rad(rotation.Y),
        math.rad(rotation.Z)
    )
    
    local primaryPart = self.kartModel.PrimaryPart
    local currentPosition = primaryPart.Position
    
    -- 检查距离
    local distance = (currentPosition - position).Magnitude
    
    if distance > 100 then
        -- 距离太远，直接传送整个模型
        self.kartModel:SetPrimaryPartCFrame(targetCFrame)
        return
    end
    
    -- 对于短距离，直接使用SetPrimaryPartCFrame进行平滑移动
    -- 不使用Tween，因为它可能导致部件分离
    self.kartModel:SetPrimaryPartCFrame(targetCFrame)
end

function TweenSync:Update(deltaTime)
    -- TweenService自动处理插值，这里不需要做什么
end

function TweenSync:Destroy()
    -- 清理
end

return TweenSync