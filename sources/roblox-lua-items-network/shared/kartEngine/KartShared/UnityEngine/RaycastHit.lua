-- UnityEngine.RaycastHit结构体的Roblox实现
-- 模拟Unity的RaycastHit结构

local RaycastHit = {}
RaycastHit.__index = RaycastHit

local UnityMath = require(script.Parent.Parent.UnityMath)
local Vector3 = UnityMath.Vector3
local LayerMask = require(script.Parent.LayerMask)

function RaycastHit.new(robloxRaycastResult)
    local self = setmetatable({}, RaycastHit)
    
    if robloxRaycastResult then
        -- 基本属性
        self.distance = robloxRaycastResult.Distance
        self.point = Vector3.new(robloxRaycastResult.Position.X, robloxRaycastResult.Position.Y, robloxRaycastResult.Position.Z)
        self.normal = Vector3.new(robloxRaycastResult.Normal.X, robloxRaycastResult.Normal.Y, robloxRaycastResult.Normal.Z)
        
        -- GameObject相关属性
        local instance = robloxRaycastResult.Instance
        local layer = LayerMask.GetLayer(instance)
        
        -- 模拟Unity的collider结构
        self.collider = {
            gameObject = {
                layer = layer,
                name = instance.Name,
                transform = instance
            }
        }
        
        -- 兼容性属性
        self.transform = instance
        self.rigidbody = instance
        self.layer = layer  -- 方便直接访问layer
    else
        -- 空的RaycastHit
        self.distance = 0
        self.point = Vector3.zero
        self.normal = Vector3.zero
        self.collider = nil
        self.transform = nil
        self.rigidbody = nil
        self.layer = 0
    end
    
    return self
end

return RaycastHit