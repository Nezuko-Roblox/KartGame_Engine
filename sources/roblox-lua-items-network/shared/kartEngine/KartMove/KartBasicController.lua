-- KartBasicController class的Lua等效实现，继承自MonoBehaviour
local MonoBehaviour = require(script.Parent.Parent.KartShared.MonoBehaviour)
local MathHelper = require(script.Parent.MathHelper)
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local Vector3 = UnityMath.Vector3

local KartBasicController = {}
KartBasicController.__index = KartBasicController
setmetatable(KartBasicController, {__index = MonoBehaviour})

-- 枚举
local PlayMode = {
    NORMAL = 0,
    ANIMATION_PLAYING = 1
}

function KartBasicController.new(robloxObject)
    local self = setmetatable(MonoBehaviour.new("KartBasicController"), KartBasicController)
    
    -- 如果提供了robloxObject，立即设置GameObject
    if robloxObject then
        self:SetGameObject(robloxObject)
    end
    
    -- 常量
    self.MAX_BOOSTER = 2
    
    -- 公共成员变量
    self.kartBody = nil
    
    -- 保护成员变量
    self.boosterWave_ = nil
    self.booster_ = {}
    self.character_ = nil
    self.characterFace_ = nil
    self.kartBody_ = nil
    self.waterBombBubble_ = nil
    self.waterFlyBubble_ = nil
    self.shield_ = nil
    self.guard_ = nil
    self.flipEffect_ = nil
    self.devilStartEffect_ = nil
    self.devilPlayEffect_ = nil
    self.isNoAnimationCharacter_ = false
    self.wheels_ = {}
    for i = 1, 4 do
        self.wheels_[i] = nil
    end
    self.kartIndex_ = 0
    self.flipRenderer_ = nil
    self.playMode_ = PlayMode.NORMAL
    
    return self
end

-- 重写父类的生命周期方法
function KartBasicController:Awake()
    -- 基类的Awake实现为空
end

function KartBasicController:Start()
    -- 基类的Start实现为空
end

function KartBasicController:Update()
    -- 基类的Update实现为空
end

function KartBasicController:FixedUpdate()
    -- 翻转特效处理 - 适配Roblox
    if self.flipEffect_ ~= nil and self.flipEffect_.Parent ~= nil then
        if self.kartBody_ and self.kartBody_.CFrame then
            self.flipEffect_.CFrame = self.kartBody_.CFrame
        end
    end
    
    -- 恶魔开始特效处理 - 适配Roblox
    if self.devilStartEffect_ ~= nil and self.devilStartEffect_.Parent ~= nil then
        if self.kartBody_ and self.kartBody_.Position then
            local localPosition = self.kartBody_.Position
            local vector = Vector3.new(localPosition.X, localPosition.Y + 3, localPosition.Z)
            self.devilStartEffect_.Position = vector
        end
    end
    
    -- 恶魔游戏特效处理 - 适配Roblox
    if self.devilPlayEffect_ ~= nil and self.devilPlayEffect_.Parent ~= nil then
        if self.kartBody_ and self.kartBody_.Position then
            local localPosition2 = self.kartBody_.Position
            local vector2 = Vector3.new(localPosition2.X, localPosition2.Y + 3, localPosition2.Z)
            self.devilPlayEffect_.Position = vector2
        end
    end
end


function KartBasicController:Initialize(kartBodyIdx, characterIdx, isNoAniCharacter)
    local gameObject = self.kartBody
    local mainAsset = ""
    self.isNoAnimationCharacter_ = isNoAniCharacter
    
    -- 确保gameObject存在再赋值
    if self.gameObject then
        self.kartBody_ = self.gameObject
    else
        -- 如果gameObject不存在，使用传入的kartBody作为fallback
        self.kartBody_ = self.kartBody
    end
    
    self.booster_ = {}
    for j = 1, 2 do
        self.booster_[j] = nil
    end
    
    self:ObjectSetting()
end

function KartBasicController:ObjectSetting()
    if not self.kartBody_ then
        return
    end
    
    -- 引入RobloxUnityAdapter来创建Transform对象
    local RobloxUnityAdapter = require(script.Parent.Parent.KartShared.RobloxUnityAdapter)
    
    -- 简化的轮子查找 - 立即尝试，不阻塞初始化
    local function findWheels()
        local wheelsModel = self.kartBody_.gameObject:FindFirstChild("Wheels")
        if not wheelsModel then
            return 0
        end
        
        local foundCount = 0
        -- 按名称查找轮子
        for i = 1, 4 do
            if not self.wheels_[i] then
                local wheelName = "tire" .. (i - 1)
                local wheel = wheelsModel:FindFirstChild(wheelName)
                if wheel then
                    local transform2 = RobloxUnityAdapter.GameObject.new(wheel).transform
                    self.wheels_[i] = transform2
                    foundCount = foundCount + 1
                end
            else
                foundCount = foundCount + 1
            end
        end
        
        return foundCount
    end
    
    -- 立即尝试一次
    local found = findWheels()
end


function KartBasicController:SetEnableBooster(enable)
    if self.boosterWave_ ~= nil then
        if enable then
            self.boosterWave_.Parent = self.kartBody_
        else
            self.boosterWave_.Parent = nil
        end
    end
    
    for i = 1, 2 do
        if self.booster_[i] ~= nil then
            if enable then
                self.booster_[i].Parent = self.kartBody_
            else
                self.booster_[i].Parent = nil
            end
        end
    end
end


-- 导出PlayMode枚举
KartBasicController.PlayMode = PlayMode

return KartBasicController