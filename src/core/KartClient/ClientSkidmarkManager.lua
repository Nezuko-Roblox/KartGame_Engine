-- 简单的轮胎印记系统
local RunService = game:GetService("RunService")
local Debris = game:GetService("Debris")
local Workspace = game:GetService("Workspace")

local ClientSkidmarkManager = {}
ClientSkidmarkManager.__index = ClientSkidmarkManager

function ClientSkidmarkManager.new(rigidbodyWalker)
    local self = setmetatable({}, ClientSkidmarkManager)
    
    self.rigidbodyWalker = rigidbodyWalker
    self.goPlayKart = nil
    
    -- 痕迹容器
    self.skidmarkFolder = Workspace:FindFirstChild("Skidmarks")
    if not self.skidmarkFolder then
        self.skidmarkFolder = Instance.new("Folder")
        self.skidmarkFolder.Name = "Skidmarks"
        self.skidmarkFolder.Parent = Workspace
    end
    
    -- 开始更新循环
    self.updateConnection = RunService.Heartbeat:Connect(function()
        self:Update()
    end)
    
    return self
end

function ClientSkidmarkManager:Update()
    -- 获取 goPlayKart
    if not self.goPlayKart then
        self.goPlayKart = self.rigidbodyWalker.goPlayKart_
        if not self.goPlayKart then
            return
        end
    end
    
    -- 使用现成的状态变量：漂移或滑行时生成印记
    local isDrifting = self.goPlayKart.m_isDrift  -- 漂移状态
    local isSlipping = self.goPlayKart.m_drift.slipMode  -- 滑行状态
    
    -- 漂移或滑行时生成印记
    if isDrifting or isSlipping then
        self:CreateSkidmarks()
    end
end

function ClientSkidmarkManager:CreateSkidmarks()
    -- 获取后轮位置
    local rearWheelPositions = self:GetRearWheelPositions()
    if not rearWheelPositions then
        return
    end
    
    -- 在每个后轮位置生成印记
    for i, position in pairs(rearWheelPositions) do
        self:CreateSkidmarkAt(position)
    end
end

function ClientSkidmarkManager:GetRearWheelPositions()
    local rigidbody = self.rigidbodyWalker.rigidbody
    if not rigidbody then
        return nil
    end
    
    local actualPart = rigidbody.GameObject.gameObject
    if not actualPart then
        return nil
    end
    
    -- 直接从轮子Part获取位置
    local rearWheelPositions = {}
    
    -- 查找后轮Part（tire2和tire3）
    local tire2 = actualPart:FindFirstChild("tire2")  -- 后左轮
    local tire3 = actualPart:FindFirstChild("tire3")  -- 后右轮
    
    if tire2 then
        table.insert(rearWheelPositions, tire2.Position)
    end
    
    if tire3 then
        table.insert(rearWheelPositions, tire3.Position)
    end
    
    return #rearWheelPositions > 0 and rearWheelPositions or nil
end

function ClientSkidmarkManager:CreateSkidmarkAt(position)
    -- 直接在轮子位置下方创建印记
    local groundPos = Vector3.new(position.X, position.Y - 0.5, position.Z)  -- 向下偏移0.5单位
    
    -- 创建简单的印记Part
    local skidmark = Instance.new("Part")
    skidmark.Name = "Skidmark"
    skidmark.Size = Vector3.new(0.8, 0.01, 0.3)
    skidmark.Position = groundPos
    skidmark.Anchored = true
    skidmark.CanCollide = false
    skidmark.CanTouch = false
    skidmark.CanQuery = false
    skidmark.Material = Enum.Material.Concrete
    skidmark.BrickColor = BrickColor.new("Really black")
    skidmark.Parent = self.skidmarkFolder
    
    -- 5秒后自动删除
    Debris:AddItem(skidmark, 5)
end

function ClientSkidmarkManager:Destroy()
    if self.updateConnection then
        self.updateConnection:Disconnect()
    end
    
    if self.skidmarkFolder then
        self.skidmarkFolder:ClearAllChildren()
    end
end

return ClientSkidmarkManager