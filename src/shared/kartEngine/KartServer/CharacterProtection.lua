-- CharacterProtection.server.lua
-- 服务器端脚本：保护玩家角色不死亡

local Players = game:GetService("Players")
local RunService = game:GetService("RunService")

-- 保护角色不死亡的函数
local function protectCharacter(character)
    local humanoid = character:WaitForChild("Humanoid", 10)
    if not humanoid then
        return
    end
    
    -- 设置无限生命值
    humanoid.MaxHealth = math.huge
    humanoid.Health = math.huge
    
    -- 监听生命值变化
    local healthConnection = humanoid.HealthChanged:Connect(function(health)
        if health < humanoid.MaxHealth then
            humanoid.Health = humanoid.MaxHealth
        end
    end)
    
    -- 监听死亡事件
    local diedConnection = humanoid.Died:Connect(function()
        -- 立即复活
        humanoid.Health = humanoid.MaxHealth
        humanoid:ChangeState(Enum.HumanoidStateType.Running)
    end)
    
    -- 防止掉出地图
    local fallCheckConnection
    fallCheckConnection = RunService.Heartbeat:Connect(function()
        if character.PrimaryPart then
            local position = character.PrimaryPart.Position
            -- 如果角色掉到Y=-500以下，传送回出生点
            if position.Y < -500 then
                local spawnLocation = workspace:FindFirstChild("SpawnLocation")
                if spawnLocation then
                    character:SetPrimaryPartCFrame(spawnLocation.CFrame + Vector3.new(0, 5, 0))
                else
                    -- 如果没有出生点，传送到原点上方
                    character:SetPrimaryPartCFrame(CFrame.new(0, 50, 0))
                end
                humanoid.Health = humanoid.MaxHealth
            end
        end
    end)
    
    -- 清理连接
    humanoid.AncestryChanged:Connect(function()
        if not humanoid.Parent then
            if healthConnection then
                healthConnection:Disconnect()
            end
            if diedConnection then
                diedConnection:Disconnect()
            end
            if fallCheckConnection then
                fallCheckConnection:Disconnect()
            end
        end
    end)
end

-- 当玩家加入时
Players.PlayerAdded:Connect(function(player)
    -- 当角色生成时
    player.CharacterAdded:Connect(function(character)
        protectCharacter(character)
        print("CharacterProtection: 保护", player.Name, "的角色")
    end)
    
    -- 注释掉CharacterAutoLoads，因为这个属性不存在
    -- player.CharacterAutoLoads = false
    
    -- 如果角色已经存在
    if player.Character then
        protectCharacter(player.Character)
    end
end)

-- 对于已经在游戏中的玩家
for _, player in pairs(Players:GetPlayers()) do
    -- player.CharacterAutoLoads = false  -- 注释掉，属性不存在
    if player.Character then
        protectCharacter(player.Character)
    end
    
    player.CharacterAdded:Connect(function(character)
        protectCharacter(character)
    end)
end

print("CharacterProtection: 服务器端角色保护已启动")

-- 返回空表以满足 require
return {}