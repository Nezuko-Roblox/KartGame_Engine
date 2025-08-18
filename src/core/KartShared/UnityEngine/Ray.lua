-- UnityEngine.Ray结构体的Roblox实现
-- 模拟Unity的Ray结构

local Ray = {}
Ray.__index = Ray

function Ray.new(origin, direction)
    local self = setmetatable({}, Ray)
    self.origin = origin
    -- Unity的Ray构造函数会对direction进行归一化处理
    -- 对应Unity源码：m_Direction = direction.normalized
    self.direction = direction.normalized
    return self
end

-- 获取射线上指定距离的点
function Ray:GetPoint(distance)
    return self.origin + self.direction * distance
end

return Ray