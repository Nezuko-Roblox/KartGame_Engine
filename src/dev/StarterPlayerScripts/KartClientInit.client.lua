-- KartClientInit.client.lua
-- 客户端启动脚本 - 简单加载客户端模块

local ReplicatedStorage = game:GetService("ReplicatedStorage")

-- 等待 KartEngine 加载
local KartEngine = ReplicatedStorage:WaitForChild("KartEngine")

-- 初始化客户端系统
print("[KartClientInit] 启动客户端系统...")

-- 加载客户端初始化脚本（自动执行）
require(KartEngine.KartClient.KartInit)

print("[KartClientInit] 客户端系统启动完成")