-- UnityEngine.LayerMask类的Roblox实现
-- 基于Unity源码LayerConst.cs的官方定义

local LayerMask = {}

local CollectionService = game:GetService("CollectionService")

-- Unity Layer配置表 - 基于LayerConst.cs的官方定义
LayerMask.LayerConst = {
    TRACK = 256,                -- layer 8 (1 << 8) - 地面/跑道
    GUI = 512,                  -- layer 9 (1 << 9)
    CHARACTER = 1024,           -- layer 10 (1 << 10)
    MINIMAP = 2048,             -- layer 11 (1 << 11)
    RESPAWN = 4096,             -- layer 12 (1 << 12)
    PLAYER = 8192,              -- layer 13 (1 << 13)
    AI = 16384,                 -- layer 14 (1 << 14)
    AI_RESPAWN = 32768,         -- layer 15 (1 << 15)
    AI_SECTION = 65536,         -- layer 16 (1 << 16) - 触发器区域
    BOOSTER_ENHANCER = 4194304, -- layer 22 (1 << 22)
    WALL = 8388608              -- layer 23 (1 << 23) - 墙壁
}

-- 反向映射表（从二进制值到layer编号）
local BitMaskToLayerNumber = {}
for name, bitMask in pairs(LayerMask.LayerConst) do
    BitMaskToLayerNumber[bitMask] = math.log(bitMask) / math.log(2)
end

-- 将layer编号转换为Unity的二进制值
function LayerMask.LayerToBitMask(layerNumber)
    return 2 ^ layerNumber
end

-- 将Unity二进制值转换为layer编号
function LayerMask.BitMaskToLayer(bitMask)
    return BitMaskToLayerNumber[bitMask] or 0
end

-- 获取GameObject的layer（模拟Unity的gameObject.layer属性）
function LayerMask.GetLayer(gameObject)
    if not gameObject then return 0 end
    -- 特殊处理Terrain
    if gameObject == game.Workspace.Terrain then
        return 8  -- TRACK layer - Terrain总是地面
    end
    -- 检查预定义的层标签
    if CollectionService:HasTag(gameObject, "Track") then
        return 8  -- TRACK
    elseif CollectionService:HasTag(gameObject, "Wall") then
        return 23  -- WALL
    elseif CollectionService:HasTag(gameObject, "Player") then
        return 13  -- PLAYER
    elseif CollectionService:HasTag(gameObject, "AI") then
        return 14  -- AI
    end
    return 0  -- 默认layer
end

-- 设置GameObject的layer
function LayerMask.SetLayer(gameObject, layerNumber)
    -- 移除所有现有的layer标签
    for bitMask, existingLayerNumber in pairs(BitMaskToLayerNumber) do
        local tagName = "Layer" .. existingLayerNumber
        if CollectionService:HasTag(gameObject, tagName) then
            CollectionService:RemoveTag(gameObject, tagName)
        end
    end
    
    -- 添加新的layer标签
    local newTagName = "Layer" .. layerNumber
    CollectionService:AddTag(gameObject, newTagName)
end

-- 检查物体是否属于指定layer（对应Unity的 1 << layer 检查）
function LayerMask.CheckLayer(gameObject, expectedBitMask)
    if not gameObject then return false end
    
    -- 特殊处理Terrain
    if gameObject == game.Workspace.Terrain then
        return expectedBitMask == LayerMask.LayerConst.TRACK
    end
    
    local layer = LayerMask.GetLayer(gameObject)
    local currentBitMask = LayerMask.LayerToBitMask(layer)
    
    return currentBitMask == expectedBitMask
end

-- 检查layer mask（用于射线检测等）
function LayerMask.CheckLayerMask(layerNumber, layerMask)
    local bitMask = LayerMask.LayerToBitMask(layerNumber)
    return bit32.band(layerMask, bitMask) ~= 0
end

return LayerMask