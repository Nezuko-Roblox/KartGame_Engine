-- KartInit.client.lua
-- 客户端赛车初始化脚本（本地使用RigidbodyFPSWalker + 网络同步远程玩家）
local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local Workspace = game:GetService("Workspace")
local RunService = game:GetService("RunService")

-- 等待ReplicatedStorage中的共享模块加载
local KartEngine = ReplicatedStorage:WaitForChild("KartEngine")
local RigidbodyFPSWalker = require(KartEngine.KartMove.RigidbodyFPSWalker)
local UnityCameraFollow = require(KartEngine.KartShared.UnityCameraFollow)

-- 尝试加载网络管理器
local ClientNetworkManagerClass = nil
local clientNetworkManager = nil
local networkEnabled = false

-- 检查是否存在网络模块
local success, err = pcall(function()
    ClientNetworkManagerClass = require(KartEngine.KartClient.ClientNetworkManager)
    networkEnabled = true
end)
print("版本： 0.1")
if not success then
    warn("[KartInit] 网络模块未找到，使用单机模式:", err)
end

local player = Players.LocalPlayer

-- 等待角色和赛车模型
local function waitForKartModel()
    local kartModel = Workspace:WaitForChild("Kart")  -- 改为查找名为Kart的Model（注意大写）
    if kartModel then
        -- 确保是Model类型
        if not kartModel:IsA("Model") then
            warn("KartController: kart不是Model，是", kartModel.ClassName)
            return nil
        end
        
        
        -- 检查并设置PrimaryPart
        if kartModel.PrimaryPart then
        else
            -- 等待名为"PrimaryPart"的Part加载
            local primaryPart = kartModel:WaitForChild("PrimaryPart", 10)
            if primaryPart and primaryPart:IsA("BasePart") then
                kartModel.PrimaryPart = primaryPart
            else
                warn("KartController: 等待PrimaryPart超时或Part不是BasePart")
            end
        end
        
        return kartModel
    end
    warn("KartController: 找不到赛车模型!")
    return nil
end

-- 保护和禁用角色控制的函数
local function protectCharacter(character)
    if not character then
        return
    end
    
    -- 获取角色的Humanoid
    local humanoid = character:WaitForChild("Humanoid")
    
    -- 设置角色不死亡
    humanoid.MaxHealth = math.huge  -- 无限生命值
    humanoid.Health = math.huge     -- 当前生命值设为无限
    
    -- 禁用角色的默认移动
    humanoid.WalkSpeed = 0
    humanoid.JumpPower = 0
    humanoid.JumpHeight = 0
    humanoid.AutoRotate = false
    
    -- 防止角色受到伤害
    humanoid.HealthChanged:Connect(function()
        if humanoid.Health < humanoid.MaxHealth then
            humanoid.Health = humanoid.MaxHealth -- 立即恢复满血
        end
    end)
    
    -- 防止角色死亡
    humanoid.Died:Connect(function()
        -- 如果意外死亡，立即恢复
        humanoid.Health = humanoid.MaxHealth
        humanoid:ChangeState(Enum.HumanoidStateType.Running)
    end)
    
end

-- 主要的赛车初始化函数
local function initializeKart()
    -- 现在总是使用本地的Kart模型，不再等待服务器分配
    local kartModel = waitForKartModel()
    
    if not kartModel then
        print("[KartInit] 等待赛车模型...")
        local connection
        connection = RunService.Heartbeat:Connect(function()
            kartModel = waitForKartModel()
            if kartModel then
                connection:Disconnect()
                initializeKart() -- 递归调用
            end
        end)
        return
    end
    
    -- 给赛车添加Player标签
    local CollectionService = game:GetService("CollectionService")
    CollectionService:AddTag(kartModel, "Player")
    
    -- 等待玩家角色
    local character = player.Character or player.CharacterAdded:Wait()
    
    -- 保护角色（不掉血、不重生、禁用控制）
    protectCharacter(character)
    
    -- 创建RigidbodyFPSWalker（本地赛车直接使用它进行物理计算）
    local rigidbodyWalker = RigidbodyFPSWalker.new(kartModel)
    
    print("[KartInit] 本地赛车使用RigidbodyFPSWalker进行物理计算")
    
    -- 设置Unity风格的相机跟随
    local camera = Workspace.CurrentCamera
    if camera then
        camera.CameraType = Enum.CameraType.Scriptable
        
        -- 创建Unity风格的相机跟随控制器
        local cameraFollow = UnityCameraFollow.new(camera, kartModel, rigidbodyWalker)
        
        -- 设置相机参数（可根据需要调整）
        cameraFollow:SetDistance(12.0)  -- 跟随距离
        cameraFollow:SetHeight(6.0)    -- 相机高度
        cameraFollow:SetDamping(8.0, 25.0)  -- 高度阻尼, 旋转阻尼 - 大幅提高旋转响应速度
        
        -- 开始相机跟随
        cameraFollow:Start()
        
        -- 将相机控制器保存到rigidbodyWalker中，方便后续访问
        if rigidbodyWalker then
            rigidbodyWalker.cameraFollow = cameraFollow
        end
    end
    
    -- 网络模式下设置远程玩家同步
    if networkEnabled and ClientNetworkManagerClass then
        -- 创建网络管理器实例用于同步远程玩家
        clientNetworkManager = ClientNetworkManagerClass.new()
        clientNetworkManager:setupLocalKart(kartModel, rigidbodyWalker)
        print("[KartInit] 网络同步已启用 - 本地使用RigidbodyFPSWalker，远程玩家状态同步")
    end

    
    -- 显示控制提示
    local gui = Instance.new("ScreenGui")
    gui.Name = "KartControlsGUI"
    gui.Parent = player.PlayerGui
    
    local frame = Instance.new("Frame")
    frame.Size = UDim2.new(0, 300, 0, 170)
    frame.Position = UDim2.new(0, 10, 0, 10)
    frame.BackgroundColor3 = Color3.new(0, 0, 0)
    frame.BackgroundTransparency = 0.3
    frame.BorderSizePixel = 0
    frame.Parent = gui
    
    local corner = Instance.new("UICorner")
    corner.CornerRadius = UDim.new(0, 8)
    corner.Parent = frame
    
    local title = Instance.new("TextLabel")
    title.Size = UDim2.new(1, 0, 0, 30)
    title.Position = UDim2.new(0, 0, 0, 0)
    title.BackgroundTransparency = 1
    title.Text = "🏎️ 赛车控制" .. (networkEnabled and " [网络模式 - 本地物理]" or " [单机模式]")
    title.TextColor3 = Color3.new(1, 1, 1)
    title.TextScaled = true
    title.Font = Enum.Font.SourceSansBold
    title.Parent = frame
    
    local controls = Instance.new("TextLabel")
    controls.Size = UDim2.new(1, -20, 1, -40)
    controls.Position = UDim2.new(0, 10, 0, 35)
    controls.BackgroundTransparency = 1
    controls.Text = "W/S - 前进/后退\nA/D - 左转/右转\nShift - 漂移\nSpace - 加速"
    
    if networkEnabled then
        controls.Text = controls.Text .. "\n\n🌐 本地物理计算 + 远程同步"
    end
    
    controls.TextColor3 = Color3.new(1, 1, 1)
    controls.TextScaled = true
    controls.Font = Enum.Font.SourceSans
    controls.TextXAlignment = Enum.TextXAlignment.Left
    controls.TextYAlignment = Enum.TextYAlignment.Top
    controls.Parent = frame
    
    -- 5秒后隐藏提示
    task.wait(5)
    if gui then
        gui:Destroy()
    end
end

-- 启动初始化
if networkEnabled then
    print("[KartInit] 客户端脚本已加载（网络模式 - 本地物理计算）")
else
    print("[KartInit] 客户端脚本已加载（单机模式）")
end

initializeKart()

-- 返回空表以满足 require
return {}