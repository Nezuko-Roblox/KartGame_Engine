-- GoKartBuilder abstract class的Lua等效实现
local GoKartBuilder = {}
GoKartBuilder.__index = GoKartBuilder

function GoKartBuilder.new()
    local self = setmetatable({}, GoKartBuilder)
    return self
end

-- 抽象方法 - 子类必须实现
function GoKartBuilder:Build()
    error("GoKartBuilder:Build() must be implemented by subclass")
end

return GoKartBuilder