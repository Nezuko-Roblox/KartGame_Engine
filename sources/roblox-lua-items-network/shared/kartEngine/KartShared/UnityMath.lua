-- Unity数学库的Lua等效实现
local UnityMath = {}

-- 保存Roblox原生Vector3引用
local RobloxVector3 = Vector3

-- Vector2类
local Vector2 = {}

-- 使用元表实现Unity风格的属性访问
function Vector2.__index(self, key)
    if key == "magnitude" or key == "Magnitude" then
        return math.sqrt(self.X * self.X + self.Y * self.Y)
    elseif key == "sqrMagnitude" or key == "SqrMagnitude" then
        return self.X * self.X + self.Y * self.Y
    elseif key == "normalized" or key == "Normalized" then
        local mag = math.sqrt(self.X * self.X + self.Y * self.Y)
        if mag > 0 then
            return Vector2.new(self.X / mag, self.Y / mag)
        end
        return Vector2.new(0, 0)
    elseif key == "x" then
        return self.X
    elseif key == "y" then
        return self.Y
    else
        return Vector2[key]
    end
end

function Vector2.new(x, y)
    return setmetatable({
        X = x or 0,
        Y = y or 0
    }, Vector2)
end

function Vector2:__add(other)
    return Vector2.new(self.X + other.X, self.Y + other.Y)
end

function Vector2:__sub(other)
    return Vector2.new(self.X - other.X, self.Y - other.Y)
end

function Vector2:__mul(scalar)
    return Vector2.new(self.X * scalar, self.Y * scalar)
end

Vector2.zero = Vector2.new(0, 0)
Vector2.one = Vector2.new(1, 1)

-- Vector3类
local Vector3 = {}

-- 使用元表实现Unity风格的属性访问
function Vector3.__index(self, key)
    if key == "magnitude" or key == "Magnitude" then
        return math.sqrt(self.X * self.X + self.Y * self.Y + self.Z * self.Z)
    elseif key == "sqrMagnitude" or key == "SqrMagnitude" then
        return self.X * self.X + self.Y * self.Y + self.Z * self.Z
    elseif key == "normalized" or key == "Normalized" then
        local mag = math.sqrt(self.X * self.X + self.Y * self.Y + self.Z * self.Z)
        if mag > 0 then
            return Vector3.new(self.X / mag, self.Y / mag, self.Z / mag)
        end
        return Vector3.new(0, 0, 0)
    elseif key == "x" then
        return self.X
    elseif key == "y" then
        return self.Y
    elseif key == "z" then
        return self.Z
    else
        return Vector3[key]
    end
end

function Vector3.new(x, y, z)
    return setmetatable({
        X = x or 0,
        Y = y or 0,
        Z = z or 0
    }, Vector3)
end

function Vector3:__add(other)
    return Vector3.new(self.X + other.X, self.Y + other.Y, self.Z + other.Z)
end

function Vector3:__sub(other)
    return Vector3.new(self.X - other.X, self.Y - other.Y, self.Z - other.Z)
end

function Vector3:__mul(scalar)
    if type(scalar) == "number" then
        return Vector3.new(self.X * scalar, self.Y * scalar, self.Z * scalar)
    else
        return Vector3.new(self.X * scalar.X, self.Y * scalar.Y, self.Z * scalar.Z)
    end
end

function Vector3:__div(scalar)
    return Vector3.new(self.X / scalar, self.Y / scalar, self.Z / scalar)
end

function Vector3:__unm()
    return Vector3.new(-self.X, -self.Y, -self.Z)
end

function Vector3.Dot(a, b)
    return a.X * b.X + a.Y * b.Y + a.Z * b.Z
end

function Vector3.Cross(a, b)
    return Vector3.new(
        a.Y * b.Z - a.Z * b.Y,
        a.Z * b.X - a.X * b.Z,
        a.X * b.Y - a.Y * b.X
    )
end

function Vector3.Scale(a, b)
    return Vector3.new(a.X * b.X, a.Y * b.Y, a.Z * b.Z)
end

Vector3.zero = Vector3.new(0, 0, 0)
Vector3.one = Vector3.new(1, 1, 1)
Vector3.up = Vector3.new(0, 1, 0)
Vector3.down = Vector3.new(0, -1, 0)
Vector3.forward = Vector3.new(0, 0, 1)
Vector3.back = Vector3.new(0, 0, -1)
-- Vector3.left = Vector3.new(-1, 0, 0)
-- Vector3.right = Vector3.new(1, 0, 0)

-- Mathf类
local Mathf = {}

function Mathf.Clamp(value, min, max)
    return math.max(min, math.min(max, value))
end

function Mathf.Min(...)
    return math.min(...)
end

function Mathf.Max(...)
    return math.max(...)
end

function Mathf.Abs(value)
    return math.abs(value)
end

function Mathf.Sqrt(value)
    return math.sqrt(value)
end

function Mathf.Exp(value)
    return math.exp(value)
end

function Mathf.Approximately(a, b, epsilon)
    epsilon = epsilon or 0.0001
    return math.abs(a - b) < epsilon
end

function Mathf.Repeat(t, length)
    local result = t - math.floor(t / length) * length
    -- Unity的Clamp步骤：确保结果在[0, length)范围内
    return Mathf.Clamp(result, 0.0, length)
end

-- Quaternion类
local Quaternion = {}

-- 使用元表实现Unity风格的属性访问
function Quaternion.__index(self, key)
    if key == "eulerAngles" then
        -- 将四元数转换为欧拉角
        local x, y, z, w = self.X, self.Y, self.Z, self.w
        
        -- Roll (x-axis rotation)
        local sinr_cosp = 2 * (w * x + y * z)
        local cosr_cosp = 1 - 2 * (x * x + y * y)
        local roll = math.atan2(sinr_cosp, cosr_cosp)
        
        -- Pitch (y-axis rotation)
        local sinp = 2 * (w * y - z * x)
        local pitch
        if math.abs(sinp) >= 1 then
            pitch = math.pi / 2 * (sinp > 0 and 1 or -1) -- use 90 degrees if out of range
        else
            pitch = math.asin(sinp)
        end
        
        -- Yaw (z-axis rotation)
        local siny_cosp = 2 * (w * z + x * y)
        local cosy_cosp = 1 - 2 * (y * y + z * z)
        local yaw = math.atan2(siny_cosp, cosy_cosp)
        
        -- 转换为度数并返回Vector3
        return Vector3.new(math.deg(roll), math.deg(pitch), math.deg(yaw))
    else
        return Quaternion[key]
    end
end

function Quaternion.new(x, y, z, w)
    return setmetatable({
        X = x or 0,
        Y = y or 0,
        Z = z or 0,
        w = w or 1
    }, Quaternion)
end

function Quaternion:__mul(other)
    if type(other) == "number" then
        return Quaternion.new(self.X * other, self.Y * other, self.Z * other, self.w * other)
    else
        return Quaternion.new(
            self.w * other.X + self.X * other.w + self.Y * other.Z - self.Z * other.Y,
            self.w * other.Y + self.Y * other.w + self.Z * other.X - self.X * other.Z,
            self.w * other.Z + self.Z * other.w + self.X * other.Y - self.Y * other.X,
            self.w * other.w - self.X * other.X - self.Y * other.Y - self.Z * other.Z
        )
    end
end

function Quaternion.Euler(x, y, z)
    
    
    -- 注释掉原来的实现
    local cx, cy, cz = math.cos(math.rad(x/2)), math.cos(math.rad(y/2)), math.cos(math.rad(z/2))
    local sx, sy, sz = math.sin(math.rad(x/2)), math.sin(math.rad(y/2)), math.sin(math.rad(z/2))
    return Quaternion.new(
        sx * cy * cz + cx * sy * sz,  
        cx * sy * cz - sx * cy * sz, 
        cx * cy * sz + sx * sy * cz, 
        cx * cy * cz - sx * sy * sz   
    )
end

function Quaternion.LookRotation(forward, up)
    up = up or Vector3.up
    
    -- 标准化向量
    local f = forward.normalized
    local u = up.normalized
    
    -- 构建正交基 (右手坐标系)
    local r = Vector3.Cross(u, f).normalized  -- right = up × forward
    local newUp = Vector3.Cross(f, r)         -- newUp = forward × right
    
    -- 构建旋转矩阵 (列矩阵, Unity标准)
    -- 列0: right, 列1: newUp, 列2: forward
    local m00, m10, m20 = r.X, r.Y, r.Z          -- right列
    local m01, m11, m21 = newUp.X, newUp.Y, newUp.Z  -- up列  
    local m02, m12, m22 = f.X, f.Y, f.Z          -- forward列
    
    -- Unity标准的旋转矩阵到四元数转换
    local trace = m00 + m11 + m22
    local quaternion = Quaternion.new()
    
    if trace > 0 then
        local s = math.sqrt(trace + 1) * 2  -- s = 4 * qw
        quaternion.w = 0.25 * s
        quaternion.X = (m21 - m12) / s
        quaternion.Y = (m02 - m20) / s  
        quaternion.Z = (m10 - m01) / s
    elseif m00 > m11 and m00 > m22 then
        local s = math.sqrt(1 + m00 - m11 - m22) * 2  -- s = 4 * qx
        quaternion.w = (m21 - m12) / s
        quaternion.X = 0.25 * s
        quaternion.Y = (m01 + m10) / s
        quaternion.Z = (m02 + m20) / s
    elseif m11 > m22 then
        local s = math.sqrt(1 + m11 - m00 - m22) * 2  -- s = 4 * qy  
        quaternion.w = (m02 - m20) / s
        quaternion.X = (m01 + m10) / s
        quaternion.Y = 0.25 * s
        quaternion.Z = (m12 + m21) / s
    else
        local s = math.sqrt(1 + m22 - m00 - m11) * 2  -- s = 4 * qz
        quaternion.w = (m10 - m01) / s
        quaternion.X = (m02 + m20) / s
        quaternion.Y = (m12 + m21) / s
        quaternion.Z = 0.25 * s
    end
    
    return quaternion
end

function Quaternion.Slerp(a, b, t)
    local dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.w * b.w
    if dot < 0 then
        b = Quaternion.new(-b.X, -b.Y, -b.Z, -b.w)
        dot = -dot
    end
    
    if dot > 0.9995 then
        return Quaternion.new(
            a.X + (b.X - a.X) * t,
            a.Y + (b.Y - a.Y) * t,
            a.Z + (b.Z - a.Z) * t,
            a.w + (b.w - a.w) * t
        )
    end
    
    local theta0 = math.acos(dot)
    local theta = theta0 * t
    local sinTheta = math.sin(theta)
    local sinTheta0 = math.sin(theta0)
    
    local s0 = math.cos(theta) - dot * sinTheta / sinTheta0
    local s1 = sinTheta / sinTheta0
    
    return Quaternion.new(
        s0 * a.X + s1 * b.X,
        s0 * a.Y + s1 * b.Y,
        s0 * a.Z + s1 * b.Z,
        s0 * a.w + s1 * b.w
    )
end

-- 将Quaternion转换为Roblox CFrame
function Quaternion:toCFrame()
    local x, y, z, w = self.X, self.Y, self.Z, self.w
    
    -- 转换四元数到旋转矩阵
    local xx, yy, zz = x*x, y*y, z*z
    local xy, xz, yz = x*y, x*z, y*z
    local wx, wy, wz = w*x, w*y, w*z
    
    local m00 = 1 - 2*(yy + zz)
    local m01 = 2*(xy - wz)
    local m02 = 2*(xz + wy)
    
    local m10 = 2*(xy + wz)
    local m11 = 1 - 2*(xx + zz)
    local m12 = 2*(yz - wx)
    
    local m20 = 2*(xz - wy)
    local m21 = 2*(yz + wx)
    local m22 = 1 - 2*(xx + yy)
    
    return CFrame.new(0, 0, 0, m00, m01, m02, m10, m11, m12, m20, m21, m22)
end

-- 将Roblox CFrame转换为Quaternion
function Quaternion.fromCFrame(cf)
    local _, _, _, xx, yx, zx, xy, yy, zy, xz, yz, zz = cf:GetComponents()
    -- CFrame到Quaternion的标准转换
    local trace = xx + yy + zz
    if trace > 0 then
        local s = math.sqrt(trace + 1) * 2
        return Quaternion.new(
            (zy - yz) / s,
            (xz - zx) / s, 
            (yx - xy) / s,
            0.25 * s
        )
    elseif xx > yy and xx > zz then
        local s = math.sqrt(1 + xx - yy - zz) * 2
        return Quaternion.new(
            0.25 * s,
            (xy + yx) / s,
            (xz + zx) / s,
            (zy - yz) / s
        )
    elseif yy > zz then
        local s = math.sqrt(1 + yy - xx - zz) * 2
        return Quaternion.new(
            (xy + yx) / s,
            0.25 * s,
            (yz + zy) / s,
            (xz - zx) / s
        )
    else
        local s = math.sqrt(1 + zz - xx - yy) * 2
        return Quaternion.new(
            (xz + zx) / s,
            (yz + zy) / s,
            0.25 * s,
            (yx - xy) / s
        )
    end
end

Quaternion.identity = Quaternion.new(0, 0, 0, 1)

-- 导出
UnityMath.Vector2 = Vector2
UnityMath.Vector3 = Vector3
UnityMath.Quaternion = Quaternion
UnityMath.Mathf = Mathf

return UnityMath