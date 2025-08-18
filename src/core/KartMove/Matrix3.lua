-- Matrix3 struct的Lua等效实现
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3

local Matrix3 = {}
Matrix3.__index = Matrix3

function Matrix3.new()
    local self = setmetatable({}, Matrix3)
    self.m = {}
    for i = 1, 3 do
        self.m[i] = {}
        for j = 1, 3 do
            self.m[i][j] = 0.0
        end
    end
    return self
end

function Matrix3.CreateMtxIdentity()
    return Matrix3.CreateMtx(1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0)
end

function Matrix3.CreateMtx(_11, _12, _13, _21, _22, _23, _31, _32, _33)
    local matrix = Matrix3.new()
    matrix.m[1][1] = _11
    matrix.m[1][2] = _12
    matrix.m[1][3] = _13
    matrix.m[2][1] = _21
    matrix.m[2][2] = _22
    matrix.m[2][3] = _23
    matrix.m[3][1] = _31
    matrix.m[3][2] = _32
    matrix.m[3][3] = _33
    return matrix
end

-- setCol(int col, Vector3 v) - 设置单个列
function Matrix3:setColSingle(col, v)
    col = col + 1  -- 转换为Lua的1-based索引
    for i = 1, 3 do
        local vIndex
        if i == 1 then
            vIndex = v.X
        elseif i == 2 then
            vIndex = v.Y
        else -- i == 3
            vIndex = v.Z
        end
        self.m[i][col] = vIndex
    end
end

-- setCol - 支持两种调用方式
-- 1. setCol(int col, Vector3 v) - 设置单个列
-- 2. setCol(Vector3 v1, Vector3 v2, Vector3 v3) - 设置三个列
function Matrix3:setCol(v1, v2, v3)
    if type(v1) == "number" and v3 == nil then
        -- 第一种情况：setCol(int col, Vector3 v)
        local col = v1
        local v = v2
        self:setColSingle(col, v)
    elseif v2 ~= nil and v3 ~= nil then
        -- 第二种情况：setCol(Vector3 v1, Vector3 v2, Vector3 v3)
        self:setColSingle(0, v1)
        self:setColSingle(1, v2)
        self:setColSingle(2, v3)
    else
        error("Matrix3:setCol invalid arguments")
    end
end

function Matrix3:__mul(other)
    if getmetatable(other) == Vector3 then
        -- Matrix3 * Vector3
        return Vector3.new(
            self.m[1][1] * other.X + self.m[1][2] * other.Y + self.m[1][3] * other.Z,
            self.m[2][1] * other.X + self.m[2][2] * other.Y + self.m[2][3] * other.Z,
            self.m[3][1] * other.X + self.m[3][2] * other.Y + self.m[3][3] * other.Z
        )
    else
        -- Matrix3 * Matrix3
        return Matrix3.CreateMtx(
            self.m[1][1] * other.m[1][1] + self.m[1][2] * other.m[2][1] + self.m[1][3] * other.m[3][1],
            self.m[1][1] * other.m[1][2] + self.m[1][2] * other.m[2][2] + self.m[1][3] * other.m[3][2],
            self.m[1][1] * other.m[1][3] + self.m[1][2] * other.m[2][3] + self.m[1][3] * other.m[3][3],
            self.m[2][1] * other.m[1][1] + self.m[2][2] * other.m[2][1] + self.m[2][3] * other.m[3][1],
            self.m[2][1] * other.m[1][2] + self.m[2][2] * other.m[2][2] + self.m[2][3] * other.m[3][2],
            self.m[2][1] * other.m[1][3] + self.m[2][2] * other.m[2][3] + self.m[2][3] * other.m[3][3],
            self.m[3][1] * other.m[1][1] + self.m[3][2] * other.m[2][1] + self.m[3][3] * other.m[3][1],
            self.m[3][1] * other.m[1][2] + self.m[3][2] * other.m[2][2] + self.m[3][3] * other.m[3][2],
            self.m[3][1] * other.m[1][3] + self.m[3][2] * other.m[2][3] + self.m[3][3] * other.m[3][3]
        )
    end
end

return Matrix3