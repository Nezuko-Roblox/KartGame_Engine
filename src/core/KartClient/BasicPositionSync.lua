-- BasicPositionSync.lua
-- 极简的位置同步方案，用于测试基础同步功能

local RunService = game:GetService("RunService")

local BasicPositionSync = {}
BasicPositionSync.__index = BasicPositionSync

function BasicPositionSync.new(remotePlayer)
    local self = setmetatable({}, BasicPositionSync)
    
    self.remotePlayer = remotePlayer
    self.kartModel = remotePlayer.kartModel
    
    -- 目标位置和当前位置
    self.targetPosition = Vector3.new(0, 0, 0)
    self.targetRotation = 0
    self.currentPosition = Vector3.new(0, 0, 0)
    self.currentRotation = 0
    
    -- 初始化位置
    if self.kartModel and self.kartModel.PrimaryPart then
        self.currentPosition = self.kartModel.PrimaryPart.Position
        
        -- 设置为幽灵车
        for _, part in pairs(self.kartModel:GetDescendants()) do
            if part:IsA("BasePart") then
                part.CanCollide = false
                part.CanTouch = false
                part.CanQuery = false
            end
        end
    end
    
    print("[BasicPositionSync] 初始化完成，玩家:", remotePlayer.name or "Unknown")
    
    return self
end

function BasicPositionSync:AddNetworkState(state)
    -- 简单地更新目标位置
    if state.position then
        if type(state.position) == "table" then
            self.targetPosition = Vector3.new(
                state.position.X or 0,
                state.position.Y or 0,
                state.position.Z or 0
            )
        else
            self.targetPosition = state.position
        end
    end
    
    if state.rotation then
        if type(state.rotation) == "table" then
            self.targetRotation = state.rotation.Y or 0
        else
            self.targetRotation = state.rotation.Y
        end
    end
    
    print("[BasicPositionSync] 收到新位置:", self.targetPosition)
end

function BasicPositionSync:Update(deltaTime)
    if not self.kartModel or not self.kartModel.PrimaryPart then
        return
    end
    
    -- 简单的线性插值
    local lerpSpeed = 0.1
    self.currentPosition = self.currentPosition:Lerp(self.targetPosition, lerpSpeed)
    self.currentRotation = self.currentRotation + (self.targetRotation - self.currentRotation) * lerpSpeed
    
    -- 直接设置位置
    self.kartModel:SetPrimaryPartCFrame(
        CFrame.new(self.currentPosition) * 
        CFrame.Angles(0, math.rad(self.currentRotation), 0)
    )
end

function BasicPositionSync:Destroy()
    -- 清理
end

return BasicPositionSync