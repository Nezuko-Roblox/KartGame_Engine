-- DirectPositionSync.lua
-- 直接位置同步系统 - 基于服务器权威位置
-- 放弃输入同步，只同步实际位置

local RunService = game:GetService("RunService")
local TweenService = game:GetService("TweenService")

local DirectPositionSync = {}
DirectPositionSync.__index = DirectPositionSync

function DirectPositionSync.new(remotePlayer)
    local self = setmetatable({}, DirectPositionSync)
    
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    
    -- 目标状态
    self.targetPosition = nil
    self.targetRotation = nil
    self.lastPosition = nil
    self.lastRotation = nil
    
    -- 插值参数
    self.interpAlpha = 0
    self.interpSpeed = 10  -- 每秒插值速度 - 更快的响应
    
    -- 初始化
    self:setupGhostCar()
    
    print("[DirectPositionSync] 初始化直接位置同步")
    
    return self
end

function DirectPositionSync:setupGhostCar()
    if not self.kartModel then return end
    
    -- 设置为幽灵车
    for _, part in pairs(self.kartModel:GetDescendants()) do
        if part:IsA("BasePart") then
            part.CanCollide = false
            part.CanTouch = false
            part.CanQuery = false
            -- 半透明
            if part.Transparency < 0.5 then
                part.Transparency = 0.3
            end
        end
    end
end

function DirectPositionSync:AddNetworkState(state)
    -- 转换数据
    local position = state.position
    if type(position) == "table" then
        position = Vector3.new(position.X or 0, position.Y or 0, position.Z or 0)
    end
    
    local rotation = state.rotation or Vector3.new(0, 0, 0)
    if type(rotation) == "table" then
        rotation = Vector3.new(rotation.X or 0, rotation.Y or 0, rotation.Z or 0)
    end
    
    -- 保存上一个位置
    if self.targetPosition then
        self.lastPosition = self.targetPosition
        self.lastRotation = self.targetRotation
    else
        -- 第一次更新，直接设置位置
        if self.kartModel and self.kartModel.PrimaryPart then
            self.kartModel:SetPrimaryPartCFrame(
                CFrame.new(position) * CFrame.Angles(
                    math.rad(rotation.X),
                    math.rad(rotation.Y),
                    math.rad(rotation.Z)
                )
            )
        end
        self.lastPosition = position
        self.lastRotation = rotation
    end
    
    -- 设置新目标
    self.targetPosition = position
    self.targetRotation = rotation
    
    -- 重置插值
    self.interpAlpha = 0
end

function DirectPositionSync:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    if not self.targetPosition or not self.lastPosition then
        return
    end
    
    -- 更新插值进度
    self.interpAlpha = math.min(self.interpAlpha + deltaTime * self.interpSpeed, 1)
    
    -- 插值位置
    local currentPosition = self.lastPosition:Lerp(self.targetPosition, self.interpAlpha)
    
    -- 插值旋转（只Y轴）
    local currentRotationY = self:lerpAngle(
        self.lastRotation.Y,
        self.targetRotation.Y,
        self.interpAlpha
    )
    
    -- 应用位置和旋转
    self.kartModel:SetPrimaryPartCFrame(
        CFrame.new(currentPosition) * CFrame.Angles(
            math.rad(self.targetRotation.X),
            math.rad(currentRotationY),
            math.rad(self.targetRotation.Z)
        )
    )
end

function DirectPositionSync:lerpAngle(a, b, t)
    local diff = b - a
    if diff > 180 then
        diff = diff - 360
    elseif diff < -180 then
        diff = diff + 360
    end
    return a + diff * t
end

function DirectPositionSync:Destroy()
    -- 清理
end

return DirectPositionSync