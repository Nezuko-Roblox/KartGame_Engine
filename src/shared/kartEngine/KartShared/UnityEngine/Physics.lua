-- UnityEngine.Physics类的Roblox实现
-- 模拟Unity的Physics静态类功能

local Physics = {}

local LayerMask = require(script.Parent.LayerMask)
local RaycastHit = require(script.Parent.RaycastHit)
local Ray = require(script.Parent.Ray)

-- Roblox服务
local Workspace = game:GetService("Workspace")
local CollectionService = game:GetService("CollectionService")

-- 模拟Unity的Physics.RaycastAll方法
function Physics.RaycastAll(origin, direction, maxDistance, layerMask)
    maxDistance = maxDistance or math.huge
    layerMask = layerMask or 0xFFFFFFFF
    
    local raycastParams = RaycastParams.new()
    raycastParams.FilterType = Enum.RaycastFilterType.Blacklist
    
    -- 排除Player标签的物体（玩家赛车）
    local filterList = {}
    local playerObjects = CollectionService:GetTagged("Player")
    for _, obj in ipairs(playerObjects) do
        table.insert(filterList, obj)
    end
    raycastParams.FilterDescendantsInstances = filterList
    
    -- 转换为Roblox Vector3
    local robloxOrigin = Vector3.new(origin.X, origin.Y, origin.Z)
    -- 注意：direction可能已经是完整的向量，需要根据其长度判断
    local dirLength = math.sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)
    local robloxDirection
    if dirLength > 1.5 then
        -- direction已经是完整的向量（从GetCollisionCheckedPosition传入的是toPosition - fromPosition）
        robloxDirection = Vector3.new(direction.X, direction.Y, direction.Z)
    else
        -- direction是单位向量，需要乘以maxDistance
        robloxDirection = Vector3.new(direction.X, direction.Y, direction.Z) * maxDistance
    end
    
    local results = {}
    local hitObjects = {}  -- 已经击中的物体
    
    -- 执行多次射线检测以模拟RaycastAll
    for i = 1, 50 do
        -- 合并过滤列表：Player标签物体 + 已击中的物体
        local currentFilterList = {}
        for _, obj in ipairs(filterList) do
            table.insert(currentFilterList, obj)
        end
        for _, obj in ipairs(hitObjects) do
            table.insert(currentFilterList, obj)
        end
        raycastParams.FilterDescendantsInstances = currentFilterList
        
        local raycastResult = Workspace:Raycast(robloxOrigin, robloxDirection, raycastParams)
        
        
        if raycastResult then
            local hit = RaycastHit.new(raycastResult)
            
            -- 检查layer mask
            if LayerMask.CheckLayerMask(hit.layer, layerMask) then
                table.insert(results, hit)
            end
            
            table.insert(hitObjects, raycastResult.Instance)
        else
            break
        end
    end
    
    return results
end

-- 重载版本：接受Ray参数
function Physics.RaycastAllFromRay(ray, maxDistance, layerMask)
    return Physics.RaycastAll(ray.origin, ray.direction, maxDistance, layerMask)
end

-- 模拟Unity的Physics.Raycast方法（单次射线检测）
function Physics.Raycast(origin, direction, maxDistance, layerMask)
    local results = Physics.RaycastAll(origin, direction, maxDistance, layerMask)
    return #results > 0, results[1]
end

return Physics