-- KartServerInit.server.lua
-- 服务器端启动脚本 - 简单加载服务器模块

local ReplicatedStorage = game:GetService("ReplicatedStorage")

-- 等待 KartEngine 加载
local KartEngine = ReplicatedStorage:WaitForChild("KartEngine")

-- 初始化服务器端系统
print("[KartServerInit] 启动服务器端系统...")

-- 加载角色保护（自动执行）
require(KartEngine.KartServer.CharacterProtection)

-- 加载网络管理器（自动创建实例并启动）
require(KartEngine.KartServer.NetworkManager)

print("[KartServerInit] 服务器端系统启动完成")