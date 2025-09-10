-- MathHelper class的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3
local Quaternion = UnityMath.Quaternion
local Mathf = UnityMath.Mathf

local MathHelper = {}

-- 静态构造函数的等效实现
MathHelper.next = {1, 2, 0}  -- C#中的数组索引从0开始，这里调整为Lua的1开始

function MathHelper.CreateQuaternion(w, v)
    return Quaternion.new(v.X, v.Y, v.Z, w)
end

function MathHelper.QuaMulScala(q, v)
    q.X = q.X * v
    q.Y = q.Y * v
    q.Z = q.Z * v
    q.w = q.w * v
end

function MathHelper.QuaNormalize(q)
    local magnitude = Mathf.Sqrt(q.w * q.w + q.X * q.X + q.Y * q.Y + q.Z * q.Z)
    MathHelper.QuaMulScala(q, 1.0 / magnitude)
end

function MathHelper.QuaAdd(q1, q2)
    q1.w = q1.w + q2.w
    q1.X = q1.X + q2.X
    q1.Y = q1.Y + q2.Y
    q1.Z = q1.Z + q2.Z
end

function MathHelper.IsBetweenII(value, min, max)
    return value >= min and value <= max
end

function MathHelper.IsBetweenIE(value, min, max)
    return value >= min and value < max
end

return MathHelper