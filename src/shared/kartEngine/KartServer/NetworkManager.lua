-- NetworkManager.server.lua
-- 简化的服务器端网络管理器 - 只负责同步远程赛车状态
-- 客户端直接使用RigidbodyFPSWalker进行本地物理计算

local RunService = game:GetService("RunService")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local Players = game:GetService("Players")

-- 网络配置常量
local NETWORK_CONFIG = {
    SYNC_RATE = 20,              -- 状态同步频率(Hz) - 20Hz
    MAX_PLAYERS = 8,             -- 最大玩家数量
}

local NetworkManager = {}
NetworkManager.__index = NetworkManager

-- 创建网络通信文件夹和事件
local function createNetworkInfrastructure()
    local existingFolder = ReplicatedStorage:FindFirstChild("NetworkRemotes")
    if existingFolder then
        return existingFolder
    end
    
    local remotes = Instance.new("Folder")
    remotes.Name = "NetworkRemotes"
    remotes.Parent = ReplicatedStorage
    
    -- 创建远程事件
    local events = {
        -- 玩家输入同步
        PlayerInput = Instance.new("RemoteEvent"),      -- 客户端发送自己的输入
        AllPlayersInput = Instance.new("RemoteEvent"),  -- 服务器广播所有玩家的输入
        
        -- 位置同步
        PlayerPosition = Instance.new("RemoteEvent"),   -- 客户端发送自己的位置
        AllPlayersPosition = Instance.new("RemoteEvent"), -- 服务器广播所有玩家的位置
        
        -- 玩家管理
        PlayerJoined = Instance.new("RemoteEvent"),     -- 玩家加入通知
        PlayerLeft = Instance.new("RemoteEvent"),       -- 玩家离开通知
    }
    
    for name, event in pairs(events) do
        event.Name = name
        event.Parent = remotes
    end
    
    return remotes
end

function NetworkManager.new()
    local self = setmetatable({}, NetworkManager)
    
    -- 初始化网络基础设施
    self.remotes = createNetworkInfrastructure()
    
    -- 玩家输入数据
    self.playerInputs = {}         -- 存储所有玩家的最新输入
    self.playerPositions = {}      -- 存储所有玩家的位置
    self.connectedPlayers = {}     -- 连接的玩家列表
    
    -- 同步相关
    self.syncAccumulator = 0
    self.positionSyncAccumulator = 0
    
    -- 初始化连接
    self:setupConnections()
    
    return self
end

function NetworkManager:setupConnections()
    -- 玩家加入处理
    Players.PlayerAdded:Connect(function(player)
        self:onPlayerJoin(player)
    end)
    
    -- 玩家离开处理
    Players.PlayerRemoving:Connect(function(player)
        self:onPlayerLeave(player)
    end)
    
    -- 接收客户端输入
    self.remotes.PlayerInput.OnServerEvent:Connect(function(player, inputData)
        self:updatePlayerInput(player, inputData)
    end)
    
    -- 接收客户端位置
    self.remotes.PlayerPosition.OnServerEvent:Connect(function(player, positionData)
        self:updatePlayerPosition(player, positionData)
    end)
    
    -- 主同步循环
    RunService.Heartbeat:Connect(function(deltaTime)
        self:networkSync(deltaTime)
        self:positionSync(deltaTime)
    end)
end

function NetworkManager:onPlayerJoin(player)
    
    -- 添加到连接列表
    self.connectedPlayers[player] = {
        userId = player.UserId,
        name = player.Name,
        joinTime = tick()
    }
    
    -- 初始化玩家输入
    self.playerInputs[player.UserId] = {
        playerId = player.UserId,
        playerName = player.Name,
        horizontal = 0,  -- 水平输入 (-1 到 1)
        vertical = 0,    -- 垂直输入 (-1 到 1)
        drift = false,   -- 是否按住漂移键
        boost = false,   -- 是否按住加速键
        timestamp = tick()
    }
    
    -- 初始化玩家位置
    self.playerPositions[player.UserId] = {
        playerId = player.UserId,
        position = Vector3.new(0, 0, 0),
        rotation = Vector3.new(0, 0, 0),
        velocity = Vector3.new(0, 0, 0),
        timestamp = tick()
    }
    
    -- 向新玩家发送所有已存在玩家的信息
    for existingUserId, existingInput in pairs(self.playerInputs) do
        if existingUserId ~= player.UserId then
            -- 向新玩家发送每个已存在玩家的信息
            self.remotes.PlayerJoined:FireClient(player, {
                playerId = existingInput.playerId,
                playerName = existingInput.playerName
            })
        end
    end
    
    -- 向所有其他客户端通知新玩家加入
    for _, otherPlayer in pairs(Players:GetPlayers()) do
        if otherPlayer ~= player then
            self.remotes.PlayerJoined:FireClient(otherPlayer, {
                playerId = player.UserId,
                playerName = player.Name
            })
        end
    end
end

function NetworkManager:onPlayerLeave(player)
    
    local userId = player.UserId
    
    -- 清理数据
    self.connectedPlayers[player] = nil
    self.playerInputs[userId] = nil
    self.playerPositions[userId] = nil
    
    -- 通知其他客户端
    self.remotes.PlayerLeft:FireAllClients(userId)
end

function NetworkManager:updatePlayerInput(player, inputData)
    -- 验证数据
    if not inputData then
        return
    end
    
    -- 更新玩家输入
    local userId = player.UserId
    self.playerInputs[userId] = {
        playerId = userId,
        playerName = player.Name,
        horizontal = inputData.horizontal or 0,
        vertical = inputData.vertical or 0,
        drift = inputData.drift or false,
        boost = inputData.boost or false,
        timestamp = tick()
    }
end

function NetworkManager:networkSync(deltaTime)
    self.syncAccumulator = self.syncAccumulator + deltaTime
    
    local syncInterval = 1 / NETWORK_CONFIG.SYNC_RATE
    if self.syncAccumulator < syncInterval then
        return
    end
    
    -- 防止累积积压，直接重置
    self.syncAccumulator = 0
    
    -- 收集所有玩家输入
    local allInputs = {}
    
    for userId, input in pairs(self.playerInputs) do
        table.insert(allInputs, input)
    end
    
    -- 广播给所有客户端（包括发送者自己，用于权威服务器模式）
    if #allInputs > 0 then
        self.remotes.AllPlayersInput:FireAllClients(allInputs)
    end
end

-- 更新玩家位置
function NetworkManager:updatePlayerPosition(player, positionData)
    if not positionData then
        return
    end
    
    local userId = player.UserId
    local position = positionData.position or Vector3.new(0, 0, 0)
    
    -- 调试输出：服务端接收到的位置（注释掉，太频繁）
    -- print(string.format("[Server-Recv] 玩家:%s 位置:(%.2f, %.2f, %.2f)", 
    --     player.Name, position.X, position.Y, position.Z))
    
    self.playerPositions[userId] = {
        playerId = userId,
        position = position,
        rotation = positionData.rotation or Vector3.new(0, 0, 0),
        velocity = positionData.velocity or Vector3.new(0, 0, 0),
        timestamp = tick()
    }
end

-- 位置同步（固定频率，优化数据）
function NetworkManager:positionSync(deltaTime)
    self.positionSyncAccumulator = self.positionSyncAccumulator + deltaTime
    
    -- 固定20Hz同步频率
    local syncInterval = 0.05
    
    if self.positionSyncAccumulator < syncInterval then
        return
    end
    
    -- 防止累积积压，直接重置
    self.positionSyncAccumulator = 0
    
    -- 收集所有玩家位置
    local allPositions = {}
    for userId, positionData in pairs(self.playerPositions) do
        -- 简化数据格式，减少网络开销
        local simplifiedData = {
            playerId = userId,
            position = positionData.position,
            rotation = Vector3.new(0, positionData.rotation.Y or 0, 0)  -- 只发送Y轴旋转
        }
        table.insert(allPositions, simplifiedData)
    end
    
    -- 广播给所有客户端
    if #allPositions > 0 then
        self.remotes.AllPlayersPosition:FireAllClients(allPositions)
    end
end

-- 创建并启动网络管理器
local networkManager = NetworkManager.new()

return networkManager