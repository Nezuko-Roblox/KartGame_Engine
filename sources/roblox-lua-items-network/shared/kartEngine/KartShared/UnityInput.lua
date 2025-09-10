-- Unity Input.GetAxis的Roblox等效实现
-- 模拟Unity的平滑输入机制

local RunService = game:GetService("RunService")
local UserInputService = game:GetService("UserInputService")

local UnityInput = {}

-- 轴状态存储
local axisStates = {}

-- 默认配置（精确模拟Unity的Input Manager设置）
local axisConfig = {
    Horizontal = {
        positiveKeys = {Enum.KeyCode.D, Enum.KeyCode.Right},
        negativeKeys = {Enum.KeyCode.A, Enum.KeyCode.Left},
        gravity = 3.0,      -- Unity默认值：松开键时的减速度
        sensitivity = 3.0,  -- Unity默认值：按下键时的加速度  
        snap = true,        -- Unity默认值：反向输入时立即切换
        deadZone = 0.001    -- Unity默认值：死区
    },
    Vertical = {
        positiveKeys = {Enum.KeyCode.W, Enum.KeyCode.Up},
        negativeKeys = {Enum.KeyCode.S, Enum.KeyCode.Down},
        gravity = 3.0,      -- Unity默认值
        sensitivity = 3.0,  -- Unity默认值
        snap = true,        -- Unity默认值
        deadZone = 0.001    -- Unity默认值
    }
}

-- 初始化轴状态
for axisName, config in pairs(axisConfig) do
    axisStates[axisName] = {
        value = 0.0,
        targetValue = 0.0,
        config = config
    }
end

-- 检查某个轴的按键状态
local function getAxisInput(axisName)
    local config = axisConfig[axisName]
    if not config then return 0 end
    
    local positivePressed = false
    local negativePressed = false
    
    -- 检查正向按键
    for _, key in ipairs(config.positiveKeys) do
        if UserInputService:IsKeyDown(key) then
            positivePressed = true
            break
        end
    end
    
    -- 检查负向按键
    for _, key in ipairs(config.negativeKeys) do
        if UserInputService:IsKeyDown(key) then
            negativePressed = true
            break
        end
    end
    
    -- 计算目标值
    if positivePressed and negativePressed then
        return 0  -- 两个方向同时按下，抵消
    elseif positivePressed then
        return 1
    elseif negativePressed then
        return -1
    else
        return 0
    end
end

-- 更新轴状态
local function updateAxis(axisName, deltaTime)
    local state = axisStates[axisName]
    if not state then return end
    
    local config = state.config
    local currentValue = state.value
    local targetValue = getAxisInput(axisName)
    
    -- 如果启用了snap且方向发生反转，立即切换
    if config.snap and ((currentValue > 0 and targetValue < 0) or (currentValue < 0 and targetValue > 0)) then
        state.value = 0
        currentValue = 0
    end
    
    -- 计算新值
    local newValue = currentValue
    
    if targetValue ~= 0 then
        -- 向目标值移动（加速）
        local direction = targetValue > currentValue and 1 or -1
        newValue = currentValue + direction * config.sensitivity * deltaTime
        
        -- 限制在目标值范围内
        if direction > 0 then
            newValue = math.min(newValue, targetValue)
        else
            newValue = math.max(newValue, targetValue)
        end
    else
        -- 向0移动（减速）
        if math.abs(currentValue) > config.deadZone then
            local direction = currentValue > 0 and -1 or 1
            newValue = currentValue + direction * config.gravity * deltaTime
            
            -- 防止过冲
            if (currentValue > 0 and newValue < 0) or (currentValue < 0 and newValue > 0) then
                newValue = 0
            end
        else
            newValue = 0
        end
    end
    
    -- 应用死区
    if math.abs(newValue) < config.deadZone then
        newValue = 0
    end
    
    -- 限制在[-1, 1]范围内
    newValue = math.max(-1, math.min(1, newValue))
    
    state.value = newValue
end

-- 获取轴值（主要接口）
function UnityInput.GetAxis(axisName)
    local state = axisStates[axisName]
    if not state then return 0 end
    return state.value
end

-- 获取原始轴值（不经过平滑）
function UnityInput.GetAxisRaw(axisName)
    return getAxisInput(axisName)
end

-- 检查按键是否被按下
function UnityInput.GetKey(keyCode)
    return UserInputService:IsKeyDown(keyCode)
end

-- 检查按键是否刚被按下
function UnityInput.GetKeyDown(keyCode)
    -- 这需要额外的状态跟踪，暂时简化实现
    return UserInputService:IsKeyDown(keyCode)
end

-- 设置轴配置
function UnityInput.SetAxisConfig(axisName, config)
    if axisConfig[axisName] then
        for key, value in pairs(config) do
            axisConfig[axisName][key] = value
        end
        axisStates[axisName].config = axisConfig[axisName]
    end
end

-- 初始化输入系统
local function initialize()
    -- 只在客户端连接到RenderStepped来更新输入状态
    if RunService:IsClient() then
        RunService.RenderStepped:Connect(function(deltaTime)
            for axisName, _ in pairs(axisStates) do
                updateAxis(axisName, deltaTime)
            end
        end)
    end
end

-- 启动输入系统
initialize()

-- 创建Unity风格的静态访问接口
local Input = setmetatable({}, {
    __index = function(_, key)
        if key == "GetAxis" then
            return UnityInput.GetAxis
        elseif key == "GetAxisRaw" then
            return UnityInput.GetAxisRaw
        elseif key == "GetKey" then
            return UnityInput.GetKey
        elseif key == "GetKeyDown" then
            return UnityInput.GetKeyDown
        else
            return UnityInput[key]
        end
    end
})

-- 导出
return {
    Input = Input,
    UnityInput = UnityInput
}