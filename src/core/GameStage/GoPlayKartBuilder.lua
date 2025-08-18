-- GoPlayKartBuilder class的Lua等效实现，继承自GoKartBuilder
local GoKartBuilder = require(script.Parent.Parent.KartMove.GoKartBuilder)
local GoPlayKart = require(script.Parent.Parent.KartMove.GoPlayKart)

local GoPlayKartBuilder = {}
GoPlayKartBuilder.__index = GoPlayKartBuilder
setmetatable(GoPlayKartBuilder, {__index = GoKartBuilder})

function GoPlayKartBuilder.new()
    local self = setmetatable(GoKartBuilder.new(), GoPlayKartBuilder)
    return self
end

-- 重写父类的Build()方法
function GoPlayKartBuilder:Build()
    return GoPlayKart.new()
end

return GoPlayKartBuilder