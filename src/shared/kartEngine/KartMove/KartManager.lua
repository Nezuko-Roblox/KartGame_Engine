-- KartManager class的Lua等效实现

local KartManager = {}
KartManager.__index = KartManager

-- 常量
KartManager.KART_SCALE_FACTOR = 0.15
KartManager.MAX_KART = 6
KartManager.KART_GRAVITY = -49.0
KartManager.PLAYER_KART_IDX = 1
KartManager.FIXED_UPDATE_COUNTER = 0

-- 单例模式
local instance_ = nil

function KartManager.new()
    local self = setmetatable({}, KartManager)
    
    self.goKart_ = {}
    for i = 1, 6 do
        self.goKart_[i] = nil
    end
    
    self.goPlayKart_ = nil
    self.driveStartTime_ = 0.0
    self.driveEndTime_ = 0.0
    self.goKartCount_ = 0
    self.isPaused_ = false
    
    return self
end

function KartManager.GetInstance()
    if instance_ == nil then
        instance_ = KartManager.new()
    end
    return instance_
end

-- 静态属性访问
KartManager.Instance = setmetatable({}, {
    __index = function(_, key)
         return KartManager.GetInstance()[key]
    end
})

function KartManager:SetKart(idx, builder, controller, wheelPos)
    if self.goKart_[idx] ~= nil then
        return nil
    end
    
    self.goKart_[idx] = builder:Build()
    self.goKart_[idx]:setReKart(controller, wheelPos)
    
    if (idx) == KartManager.PLAYER_KART_IDX then
        self.goPlayKart_ = self.goKart_[idx]
    end
    
    self.goKartCount_ = self.goKartCount_ + 1
    
    return self.goKart_[idx]
end

return KartManager