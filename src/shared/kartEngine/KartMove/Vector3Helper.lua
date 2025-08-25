-- Vector3Helper class的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3

local Vector3Helper = {}

function Vector3Helper.SetVector3(v, x, y, z)
    if y == nil and z == nil then
        -- SetVector3(ref Vector3 v, float t) - 重载版本
        local t = x
        Vector3Helper.SetVector3(v, t, t, t)
    else
        -- SetVector3(ref Vector3 v, float x, float y, float z)
        v.X = x
        v.Y = y
        v.Z = z
    end
end

return Vector3Helper