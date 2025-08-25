-- ClientNetworkManager.client.lua
-- 简化的客户端网络管理器
-- 本地赛车直接使用RigidbodyFPSWalker，只同步远程玩家状态

local Players = game:GetService("Players")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local Workspace = game:GetService("Workspace")
local RunService = game:GetService("RunService")

-- 获取 KartEngine
local KartEngine = ReplicatedStorage:WaitForChild("KartEngine")


local player = Players.LocalPlayer

local ClientNetworkManager = {}
ClientNetworkManager.__index = ClientNetworkManager

function ClientNetworkManager.new()
    local self = setmetatable({}, ClientNetworkManager)
    
    -- 等待网络基础设施
    self.remotes = ReplicatedStorage:WaitForChild("NetworkRemotes")
    
    -- 本地数据
    self.localRigidbodyWalker = nil    -- 本地RigidbodyFPSWalker
    self.localKartModel = nil          -- 本地赛车模型
    
    -- 远程玩家数据
    self.remotePlayers = {}            -- 远程玩家的赛车模型、RigidbodyWalker和状态
    self.nextRemoteKartIndex = 2       -- 远程玩家从索引2开始（1是本地玩家）
    
    -- 同步相关（只同步位置，不同步输入）
    self.positionSendRate = 20         -- 位置发送频率(Hz) - 20Hz 高频率
    self.positionSendAccumulator = 0
    
    -- 帧率统计
    self.sendFrameCount = 0            -- 发送帧计数
    self.recvFrameCount = 0            -- 接收帧计数
    self.lastFrameReportTime = tick()  -- 上次报告时间
    
    -- 初始化连接
    self:setupConnections()
    
    return self
end

function ClientNetworkManager:setupConnections()
    -- 等待新的远程事件
    self.remotes:WaitForChild("AllPlayersInput", 5)
    self.remotes:WaitForChild("PlayerInput", 5)
    self.remotes:WaitForChild("AllPlayersPosition", 5)
    self.remotes:WaitForChild("PlayerPosition", 5)
    
    -- 接收所有玩家的输入
    if self.remotes.AllPlayersInput then
        self.remotes.AllPlayersInput.OnClientEvent:Connect(function(allInputs)
            self:processAllPlayersInput(allInputs)
        end)
    else
        warn("[ClientNetworkManager] AllPlayersInput 事件未找到")
    end
    
    -- 接收所有玩家的位置（用于校正）
    if self.remotes.AllPlayersPosition then
        self.remotes.AllPlayersPosition.OnClientEvent:Connect(function(allPositions)
            self:processAllPlayersPosition(allPositions)
        end)
    end
    
    -- 玩家加入事件
    self.remotes.PlayerJoined.OnClientEvent:Connect(function(playerData)
        self:onRemotePlayerJoin(playerData)
    end)
    
    -- 玩家离开事件
    self.remotes.PlayerLeft.OnClientEvent:Connect(function(userId)
        self:onRemotePlayerLeave(userId)
    end)
    
    -- 定期发送本地位置（不再发送输入）
    RunService.Heartbeat:Connect(function(deltaTime)
        self:sendLocalPosition(deltaTime)
        self:updateRemotePositions(deltaTime)  -- 平滑位置更新
    end)
end

function ClientNetworkManager:setupLocalKart(kartModel, rigidbodyWalker)
    self.localKartModel = kartModel
    self.localRigidbodyWalker = rigidbodyWalker
end

-- 不再发送输入，只同步位置
-- function ClientNetworkManager:sendLocalInput(deltaTime)
--     已废弃
-- end

function ClientNetworkManager:collectLocalInput()
    -- 从Unity Input系统获取输入
    local UnityInput = require(KartEngine.KartShared.UnityInput)
    local Input = UnityInput.Input
    
    local input = {
        horizontal = Input.GetAxis("Horizontal"),  -- -1 到 1
        vertical = Input.GetAxis("Vertical"),      -- -1 到 1
        drift = Input.GetKey(Enum.KeyCode.LeftShift),
        boost = Input.GetKey(Enum.KeyCode.Space)
    }
    
    return input
end

-- 发送本地位置到服务器
function ClientNetworkManager:sendLocalPosition(deltaTime)
    if not self.localKartModel then
        return
    end
    
    self.positionSendAccumulator = self.positionSendAccumulator + deltaTime
    
    local sendInterval = 1 / self.positionSendRate
    if self.positionSendAccumulator < sendInterval then
        return
    end
    
    -- 防止累积积压，直接重置而不是累减
    self.positionSendAccumulator = 0
    
    -- 收集位置信息
    local position = self.localKartModel.PrimaryPart and self.localKartModel.PrimaryPart.Position or Vector3.new(0, 0, 0)
    local positionData = {
        position = position,
        rotation = self.localKartModel.PrimaryPart and self.localKartModel.PrimaryPart.Orientation or Vector3.new(0, 0, 0),
        velocity = self.localRigidbodyWalker and self.localRigidbodyWalker.goPlayKart_ and 
                  self.localRigidbodyWalker.goPlayKart_.m_KartWLVel or Vector3.new(0, 0, 0)
    }
    
    -- 更新帧计数
    self.sendFrameCount = self.sendFrameCount + 1
    
    -- 发送到服务器
    if self.remotes.PlayerPosition then
        self.remotes.PlayerPosition:FireServer(positionData)
    end
end

-- 处理所有玩家的位置（位置校正）
function ClientNetworkManager:processAllPlayersPosition(allPositions)
    -- 正常处理所有位置更新
    for _, positionData in ipairs(allPositions) do
        if positionData.playerId ~= player.UserId then
            self:updateRemotePosition(positionData)
        end
    end
end

-- 更新远程玩家位置（使用NetworkSyncManager）
function ClientNetworkManager:updateRemotePosition(positionData)
    local remotePlayer = self.remotePlayers[positionData.playerId]
    if not remotePlayer then
        return
    end
    
    -- 使用NetworkSyncManager处理位置同步
    if remotePlayer.syncManager then
        remotePlayer.syncManager:AddNetworkState(positionData)
    end
end

-- 更新所有远程玩家（每帧调用）
function ClientNetworkManager:updateRemotePositions(deltaTime)
    for userId, remotePlayer in pairs(self.remotePlayers) do
        if remotePlayer.syncManager then
            remotePlayer.syncManager:Update(deltaTime)
        end
    end
end

function ClientNetworkManager:onRemotePlayerJoin(playerData)
    if playerData.playerId == player.UserId then
        return -- 忽略自己
    end
    
    -- 创建远程玩家的赛车模型
    self:createRemoteKart(playerData)
end

function ClientNetworkManager:processAllPlayersInput(allInputs)
    -- 不再处理输入，只同步位置
    -- 输入同步已废弃
end

-- 不再处理输入
-- function ClientNetworkManager:applyInputToRemoteKart(inputData)
--     已废弃 - 只同步位置
-- end

function ClientNetworkManager:createRemoteKart(playerData)
    -- 查找原始赛车模型
    local originalKart = Workspace:FindFirstChild("Kart")
    if not originalKart then
        warn("[ClientNetworkManager] 找不到原始Kart模型")
        return
    end
    
    -- 检查是否还有可用的赛车索引
    local KartManager = require(KartEngine.KartMove.KartManager)
    if self.nextRemoteKartIndex > KartManager.MAX_KART then
        warn("[ClientNetworkManager] 达到最大赛车数量限制:", KartManager.MAX_KART)
        return
    end
    
    -- 克隆赛车模型
    local remoteKart = originalKart:Clone()
    remoteKart.Name = playerData.playerName .. "_RemoteKart"
    remoteKart:SetAttribute("PlayerId", playerData.playerId)
    remoteKart:SetAttribute("IsRemote", true)
    
    -- 设置初始位置（默认生成位置），保持原始模板的方向
    local spawnPosition = Vector3.new(355, 1, 84)  -- 默认生成位置
    
    -- 获取原始模板的旋转
    local originalRotation = CFrame.new()
    if originalKart.PrimaryPart then
        originalRotation = originalKart.PrimaryPart.CFrame - originalKart.PrimaryPart.Position
    end
    
    if remoteKart.PrimaryPart then
        remoteKart:SetPrimaryPartCFrame(CFrame.new(spawnPosition) * originalRotation)
    else
        -- 如果没有PrimaryPart，尝试设置
        local primaryPart = remoteKart:FindFirstChild("PrimaryPart")
        if primaryPart and primaryPart:IsA("BasePart") then
            remoteKart.PrimaryPart = primaryPart
            remoteKart:SetPrimaryPartCFrame(CFrame.new(spawnPosition) * originalRotation)
        end
    end
    
    remoteKart.Parent = Workspace
    
    -- 远程赛车不需要RigidbodyFPSWalker，只是显示模型
    -- 不创建物理控制器
    
    -- 保存远程玩家数据
    local remotePlayerData = {
        userId = playerData.playerId,
        name = playerData.playerName,
        kartModel = remoteKart
    }
    
    -- 使用基础位置同步
    local BasicPositionSync = require(KartEngine.KartClient.BasicPositionSync)
    remotePlayerData.syncManager = BasicPositionSync.new(remotePlayerData)
    
    self.remotePlayers[playerData.playerId] = remotePlayerData
end

function ClientNetworkManager:onRemotePlayerLeave(userId)
    if userId == player.UserId then
        return -- 忽略自己
    end
    
    local remotePlayer = self.remotePlayers[userId]
    if remotePlayer then
        
        -- 清理KartManager中的赛车实例
        local KartManager = require(KartEngine.KartMove.KartManager)
        if remotePlayer.kartIndex then
            -- 清理KartManager中对应索引的赛车
            KartManager.Instance.goKart_[remotePlayer.kartIndex] = nil
            KartManager.Instance.goKartCount_ = KartManager.Instance.goKartCount_ - 1
            
            -- 回收索引，让下一个加入的玩家可以使用
            if remotePlayer.kartIndex < self.nextRemoteKartIndex then
                self.nextRemoteKartIndex = remotePlayer.kartIndex
            end
        end
        
        -- 停止RigidbodyWalker的更新
        if remotePlayer.rigidbodyWalker then
            -- 清理生命周期回调
            if remotePlayer.rigidbodyWalker.StopAllCallbacks then
                remotePlayer.rigidbodyWalker:StopAllCallbacks()
            end
        end
        
        -- 销毁赛车模型
        if remotePlayer.kartModel then
            remotePlayer.kartModel:Destroy()
        end
        
        -- 清理数据
        self.remotePlayers[userId] = nil
    end
end

-- 不再处理输入超时
-- function ClientNetworkManager:handleInputTimeout()
--     已废弃 - 只同步位置
-- end

return ClientNetworkManager