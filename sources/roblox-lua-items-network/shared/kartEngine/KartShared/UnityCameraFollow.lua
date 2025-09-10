-- Unity风格的相机跟随系统
-- 完全按照Unity的相机跟随逻辑实现，不简化任何逻辑
local UnityCameraFollow = {}
UnityCameraFollow.__index = UnityCameraFollow

local RunService = game:GetService("RunService")

-- 保存Roblox原生Vector3引用，避免命名冲突
local RobloxVector3 = Vector3

-- Unity数学库
local UnityMath = require(script.Parent.UnityMath)
local Vector3 = UnityMath.Vector3
local Quaternion = UnityMath.Quaternion

function UnityCameraFollow.new(camera, target, rigidbodyWalker)
    local self = setmetatable({}, UnityCameraFollow)
    
    -- 相机设置
    self.camera = camera or workspace.CurrentCamera
    self.target = target
    self.rigidbodyWalker = rigidbodyWalker
    
    -- Unity风格的相机参数
    self.height = 2.0
    self.distance = 10.0
    self.heightDamping = 8.0  -- 增加高度跟随速度
    self.rotationDamping = 15.0  -- 增加旋转跟随速度
    self.followSpeed = 8.0  -- 增加位置跟随速度
    self.lookAhead = 2.0
    
    -- 内部状态
    self.currentHeight = 0
    self.wantedHeight = 0
    
    -- 连接
    self.connection = nil
    
    return self
end

-- 开始相机跟随
function UnityCameraFollow:Start()
    if self.connection then
        self.connection:Disconnect()
    end
    
    self.connection = RunService.Heartbeat:Connect(function(deltaTime)
        self:UpdateCamera(deltaTime)
    end)
end

-- 停止相机跟随
function UnityCameraFollow:Stop()
    if self.connection then
        self.connection:Disconnect()
        self.connection = nil
    end
end

-- Unity风格的相机更新逻辑
function UnityCameraFollow:UpdateCamera(deltaTime)
    if not self.target then
        return
    end
    
    -- 获取目标位置和方向
    local targetPosition = self:GetTargetPosition()
    local targetForward = self:GetTargetForward()
    local targetVelocity = self:GetTargetVelocity()
    
    -- 检查获取的数据是否有效
    if not targetPosition or not targetForward then
        return
    end
    
    -- 计算相机方向 - 优先使用车头朝向，响应更快
    local cameraDirection = targetForward
    
    -- 计算期望高度
    self.wantedHeight = targetPosition.Y + self.height
    self.currentHeight = self:Lerp(self.currentHeight, self.wantedHeight, self.heightDamping * deltaTime)
    
    -- 计算相机位置（在目标后方）
    local backwardDirection = cameraDirection * -1
    local desiredPosition = targetPosition + backwardDirection * self.distance
    desiredPosition = Vector3.new(desiredPosition.X, self.currentHeight, desiredPosition.Z)
    
    -- 应用平滑跟随
    local currentPos = self:RobloxToUnityVector3(self.camera.CFrame.Position)
    local smoothedPosition = self:LerpVector3(currentPos, desiredPosition, self.followSpeed * deltaTime)
    
    -- 转换为Roblox坐标系并设置相机
    local robloxCameraPos = self:UnityToRobloxVector3(smoothedPosition)
    local robloxTargetPos = self:UnityToRobloxVector3(targetPosition)
    
    -- 确保转换成功（应该是Roblox Vector3 userdata类型）
    if not robloxCameraPos or not robloxTargetPos then
        warn("相机位置转换失败!")
        return
    end
    
    -- 使用简单的CFrame.lookAt方式，如果失败则使用备用方案
    local success, cameraCFrame = pcall(function()
        return CFrame.lookAt(robloxCameraPos, robloxTargetPos)
    end)
    
    if success then
        self.camera.CFrame = cameraCFrame
    else
        -- 备用方案：手动构造CFrame
        warn("CFrame.lookAt失败，使用备用方案")
        local lookDirection = (robloxTargetPos - robloxCameraPos).Unit
        self.camera.CFrame = CFrame.new(robloxCameraPos, robloxCameraPos + lookDirection)
    end
end

-- 获取目标位置
function UnityCameraFollow:GetTargetPosition()
    if not self.target then
        return nil
    end
    
    if self.target.ClassName == "Model" then
        local primaryPart = self.target.PrimaryPart or self.target:FindFirstChildOfClass("BasePart")
        if primaryPart then
            return self:RobloxToUnityVector3(primaryPart.Position)
        else
            return Vector3.zero
        end
    else
        return self:RobloxToUnityVector3(self.target.Position)
    end
end

-- 获取目标前进方向
function UnityCameraFollow:GetTargetForward()
    if not self.target then
        return nil
    end
    
    if self.target.ClassName == "Model" then
        local primaryPart = self.target.PrimaryPart or self.target:FindFirstChildOfClass("BasePart")
        if primaryPart then
            -- 使用 -LookVector 作为前进方向，与Unity逻辑保持一致
            local lookVec = primaryPart.CFrame.LookVector
            return self:RobloxToUnityVector3(RobloxVector3.new(-lookVec.X, -lookVec.Y, -lookVec.Z))
        else
            return Vector3.new(0, 0, -1)
        end
    else
        -- 使用 -LookVector 作为前进方向，与Unity逻辑保持一致
        local lookVec = self.target.CFrame.LookVector
        return self:RobloxToUnityVector3(RobloxVector3.new(-lookVec.X, -lookVec.Y, -lookVec.Z))
    end
end

-- 获取目标速度
function UnityCameraFollow:GetTargetVelocity()
    if self.rigidbodyWalker and self.rigidbodyWalker.goPlayKart_ then
        local kartVelocity = self.rigidbodyWalker.goPlayKart_.m_KartRealVelocity
        if kartVelocity then
            return kartVelocity
        end
    end
    return Vector3.zero
end

-- 坐标系转换函数
function UnityCameraFollow:RobloxToUnityVector3(robloxVec3)
    if not robloxVec3 then
        return Vector3.zero
    end
    return Vector3.new(robloxVec3.X, robloxVec3.Y, robloxVec3.Z)
end

function UnityCameraFollow:UnityToRobloxVector3(unityVec3)
    if not unityVec3 then
        return RobloxVector3.new(0, 0, 0)
    end
    
    -- 确保访问的是正确的属性
    local x = unityVec3.X or 0
    local y = unityVec3.Y or 0
    local z = unityVec3.Z or 0
    
    return RobloxVector3.new(x, y, z)
end

-- 设置相机参数
function UnityCameraFollow:SetDistance(distance)
    self.distance = distance
end

function UnityCameraFollow:SetHeight(height)
    self.height = height
end

function UnityCameraFollow:SetDamping(heightDamping, rotationDamping)
    self.heightDamping = heightDamping
    self.rotationDamping = rotationDamping
end

function UnityCameraFollow:SetTarget(target)
    self.target = target
end

-- 辅助数学函数
function UnityCameraFollow:Lerp(a, b, t)
    local clampedT = math.min(math.max(t, 0), 1)
    return a + (b - a) * clampedT
end

function UnityCameraFollow:LerpVector3(a, b, t)
    if not a or not b then
        return a or b or Vector3.zero
    end
    
    local clampedT = math.min(math.max(t, 0), 1)
    return Vector3.new(
        a.X + (b.X - a.X) * clampedT,
        a.Y + (b.Y - a.Y) * clampedT,
        a.Z + (b.Z - a.Z) * clampedT
    )
end

-- 清理资源
function UnityCameraFollow:Destroy()
    self:Stop()
end

return UnityCameraFollow