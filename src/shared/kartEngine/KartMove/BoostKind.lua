-- BoostKind enum的Lua等效实现
local BoostKind = {
    NoBoost = 0,
    BoostNormal = 1,
    BoostTeam = 2,
    BoostDrift = 3,
    BoostPlay = 4,
    BoostZone = 5,
    BoostJumpZone = 6,
    BoostStart = 7,
    BoostAnimal = 8,
    BoostDelivery = 9
}

-- 暴露到全局变量供TypeScript ECS系统使用
_G.BoostKind = BoostKind

return BoostKind