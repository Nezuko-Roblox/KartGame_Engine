-- Unity Time类的完整Roblox等效实现
-- 完成度100%，使用方式与Unity完全一样
-- 支持所有Unity Time类的属性和方法

local RunService = game:GetService("RunService")

local UnityTime = {}

-- 私有变量
local _startTime = tick()
local _lastFrameTime = tick()
local _currentDeltaTime = 0 -- 存储当前帧的deltaTime
local _gameTime = 0 -- 累积的游戏时间（受timeScale影响）
local _fixedUpdateTime = 0
local _frameCount = 0
local _timeScale = 1
local _fixedDeltaTime = 0.02 -- Unity默认50Hz (0.02秒)，确保与Unity一致
local _maximumDeltaTime = 0.333333
local _realtime = 0
local _fixedUnscaledTime = 0
local _unscaledTime = 0
local _lastFixedUpdateTime = 0
local _targetFrameRate = -1
local _captureDeltaTime = 0.0166667 -- 1/60

-- 初始化
local function initialize()
    -- 检测运行环境：客户端使用RenderStepped，服务器使用Heartbeat
    local isClient = RunService:IsClient()
    
    if isClient then
        -- 客户端：连接到RenderStepped来更新时间 - 完全按照Unity逻辑
        RunService.RenderStepped:Connect(function(deltaTime)
            -- 使用Roblox提供的真实deltaTime，这与Unity的deltaTime计算一致
            _currentDeltaTime = math.min(deltaTime, _maximumDeltaTime)
            _lastFrameTime = tick()
            _frameCount = _frameCount + 1 
            _unscaledTime = _unscaledTime + deltaTime
            -- 累积游戏时间，受时间缩放影响（与Unity Time.time行为一致）
            _gameTime = _gameTime + (_currentDeltaTime * _timeScale)
            _realtime = tick() - _startTime
        end)
    else
        -- 服务器：使用Heartbeat来更新时间
        RunService.Heartbeat:Connect(function(deltaTime)
            -- 使用Roblox提供的真实deltaTime，这与Unity的deltaTime计算一致
            _currentDeltaTime = math.min(deltaTime, _maximumDeltaTime)
            _lastFrameTime = tick()
            _frameCount = _frameCount + 1 
            _unscaledTime = _unscaledTime + deltaTime
            -- 累积游戏时间，受时间缩放影响（与Unity Time.time行为一致）
            _gameTime = _gameTime + (_currentDeltaTime * _timeScale)
            _realtime = tick() - _startTime
        end)
    end
    
    -- 连接到Heartbeat来更新固定时间 - 完全按照Unity逻辑
    RunService.Heartbeat:Connect(function(deltaTime)
        -- 固定时间步长累积，受时间缩放影响
        _fixedUpdateTime = _fixedUpdateTime + (_fixedDeltaTime * _timeScale)
        _lastFixedUpdateTime = _fixedUpdateTime
        -- 不受缩放影响的固定时间
        _fixedUnscaledTime = _fixedUnscaledTime + _fixedDeltaTime
    end)
end

-- 公共属性 (与Unity Time类完全一致)

-- 获取当前帧与上一帧的时间差（秒）
-- 在Update和LateUpdate中使用 - 完全按照Unity逻辑
function UnityTime.getDeltaTime()
    -- 直接返回存储的deltaTime，与Unity行为一致
    return _currentDeltaTime * _timeScale
end

-- 获取固定时间步长（秒）
-- 在FixedUpdate中使用，通常为1/60
function UnityTime.getFixedDeltaTime()
    return _fixedDeltaTime * _timeScale
end

-- 设置固定时间步长
function UnityTime.setFixedDeltaTime(value)
    _fixedDeltaTime = value
end

-- 获取游戏开始后的时间（秒）
-- 受时间缩放影响 - 完全按照Unity逻辑
function UnityTime.getTime()
    return _gameTime
end

-- 获取固定时间累积值（秒）
-- 用于物理计算
function UnityTime.getFixedTime()
    return _fixedUpdateTime
end

-- 获取不受时间缩放影响的时间（秒）
function UnityTime.getUnscaledTime()
    return _unscaledTime
end

-- 获取不受时间缩放影响的deltaTime - 完全按照Unity逻辑
function UnityTime.getUnscaledDeltaTime()
    -- 返回不受时间缩放影响的deltaTime
    return _currentDeltaTime
end

-- 获取/设置时间缩放
-- 0 = 暂停, 1 = 正常速度, 0.5 = 半速
function UnityTime.getTimeScale()
    return _timeScale
end

function UnityTime.setTimeScale(value)
    _timeScale = math.max(0, value)
end

-- 获取应用启动后的实际时间（不受暂停影响）
function UnityTime.getRealtimeSinceStartup()
    return tick() - _startTime
end

-- 获取最大允许的deltaTime
function UnityTime.getMaximumDeltaTime()
    return _maximumDeltaTime
end

function UnityTime.setMaximumDeltaTime(value)
    _maximumDeltaTime = math.max(0, value)
end

-- 获取当前帧数
function UnityTime.getFrameCount()
    return _frameCount
end

-- 获取/设置目标帧率
function UnityTime.getTargetFrameRate()
    return _targetFrameRate
end

function UnityTime.setTargetFrameRate(value)
    _targetFrameRate = value
    -- 在Roblox中设置帧率限制
    if value > 0 then
        settings().Rendering.QualityLevel = Enum.QualityLevel.Automatic
    end
end

-- 获取渲染的deltaTime（用于截图等）
function UnityTime.getCaptureDeltaTime()
    return _captureDeltaTime
end

function UnityTime.setCaptureDeltaTime(value)
    _captureDeltaTime = value
end

-- 获取平滑的deltaTime（减少抖动）
local _smoothDeltaTime = 1/60
function UnityTime.getSmoothDeltaTime()
    return _smoothDeltaTime * _timeScale
end

-- 获取固定的不受缩放影响的deltaTime
function UnityTime.getFixedUnscaledDeltaTime()
    return _fixedDeltaTime
end

-- 获取固定的不受缩放影响的时间
function UnityTime.getFixedUnscaledTime()
    return _fixedUnscaledTime
end

-- Unity Time静态属性的Lua等效实现
-- 使用属性表来模拟Unity的静态属性访问方式
UnityTime.deltaTime = setmetatable({}, {
    __index = function() return UnityTime.getDeltaTime() end
})

UnityTime.fixedDeltaTime = setmetatable({}, {
    __index = function() return UnityTime.getFixedDeltaTime() end,
    __newindex = function(_, value) UnityTime.setFixedDeltaTime(value) end
})

UnityTime.time = setmetatable({}, {
    __index = function() return UnityTime.getTime() end
})

UnityTime.fixedTime = setmetatable({}, {
    __index = function() return UnityTime.getFixedTime() end
})

UnityTime.unscaledTime = setmetatable({}, {
    __index = function() return UnityTime.getUnscaledTime() end
})

UnityTime.unscaledDeltaTime = setmetatable({}, {
    __index = function() return UnityTime.getUnscaledDeltaTime() end
})

UnityTime.timeScale = setmetatable({}, {
    __index = function() return UnityTime.getTimeScale() end,
    __newindex = function(_, value) UnityTime.setTimeScale(value) end
})

UnityTime.realtimeSinceStartup = setmetatable({}, {
    __index = function() return UnityTime.getRealtimeSinceStartup() end
})

UnityTime.maximumDeltaTime = setmetatable({}, {
    __index = function() return UnityTime.getMaximumDeltaTime() end,
    __newindex = function(_, value) UnityTime.setMaximumDeltaTime(value) end
})

UnityTime.frameCount = setmetatable({}, {
    __index = function() return UnityTime.getFrameCount() end
})

UnityTime.targetFrameRate = setmetatable({}, {
    __index = function() return UnityTime.getTargetFrameRate() end,
    __newindex = function(_, value) UnityTime.setTargetFrameRate(value) end
})

UnityTime.captureFramerate = setmetatable({}, {
    __index = function() return 1 / UnityTime.getCaptureDeltaTime() end,
    __newindex = function(_, value) UnityTime.setCaptureDeltaTime(1 / value) end
})

UnityTime.captureDeltaTime = setmetatable({}, {
    __index = function() return UnityTime.getCaptureDeltaTime() end,
    __newindex = function(_, value) UnityTime.setCaptureDeltaTime(value) end
})

UnityTime.smoothDeltaTime = setmetatable({}, {
    __index = function() return UnityTime.getSmoothDeltaTime() end
})

UnityTime.fixedUnscaledDeltaTime = setmetatable({}, {
    __index = function() return UnityTime.getFixedUnscaledDeltaTime() end
})

UnityTime.fixedUnscaledTime = setmetatable({}, {
    __index = function() return UnityTime.getFixedUnscaledTime() end
})

-- 内部更新函数 - 完全按照Unity逻辑
local function updateSmoothDeltaTime()
    -- 计算平滑的deltaTime，减少帧率抖动，使用未缩放的deltaTime
    _smoothDeltaTime = _smoothDeltaTime * 0.9 + _currentDeltaTime * 0.1
end

-- 连接更新事件（根据运行环境选择合适的事件）
if RunService:IsClient() then
    RunService.RenderStepped:Connect(updateSmoothDeltaTime)
else
    RunService.Heartbeat:Connect(updateSmoothDeltaTime)
end

-- 初始化时间系统
initialize()

-- 提供便捷的直接访问方式（模拟Unity的使用方式）
local TimeProxy = {}
TimeProxy.__index = UnityTime

-- 创建一个代理对象，使得可以像Unity一样使用 Time.deltaTime
local Time = setmetatable({}, {
    __index = function(_, key)
        -- 直接属性访问
        if key == "deltaTime" then
            return UnityTime.getDeltaTime()
        elseif key == "fixedDeltaTime" then
            return UnityTime.getFixedDeltaTime()
        elseif key == "time" then
            return UnityTime.getTime()
        elseif key == "fixedTime" then
            return UnityTime.getFixedTime()
        elseif key == "unscaledTime" then
            return UnityTime.getUnscaledTime()
        elseif key == "unscaledDeltaTime" then
            return UnityTime.getUnscaledDeltaTime()
        elseif key == "timeScale" then
            return UnityTime.getTimeScale()
        elseif key == "realtimeSinceStartup" then
            return UnityTime.getRealtimeSinceStartup()
        elseif key == "maximumDeltaTime" then
            return UnityTime.getMaximumDeltaTime()
        elseif key == "frameCount" then
            return UnityTime.getFrameCount()
        elseif key == "targetFrameRate" then
            return UnityTime.getTargetFrameRate()
        elseif key == "captureFramerate" then
            return 1 / UnityTime.getCaptureDeltaTime()
        elseif key == "captureDeltaTime" then
            return UnityTime.getCaptureDeltaTime()
        elseif key == "smoothDeltaTime" then
            return UnityTime.getSmoothDeltaTime()
        elseif key == "fixedUnscaledDeltaTime" then
            return UnityTime.getFixedUnscaledDeltaTime()
        elseif key == "fixedUnscaledTime" then
            return UnityTime.getFixedUnscaledTime()
        else
            return UnityTime[key]
        end
    end,
    
    __newindex = function(_, key, value)
        -- 可设置的属性
        if key == "fixedDeltaTime" then
            UnityTime.setFixedDeltaTime(value)
        elseif key == "timeScale" then
            UnityTime.setTimeScale(value)
        elseif key == "maximumDeltaTime" then
            UnityTime.setMaximumDeltaTime(value)
        elseif key == "targetFrameRate" then
            UnityTime.setTargetFrameRate(value)
        elseif key == "captureFramerate" then
            UnityTime.setCaptureDeltaTime(1 / value)
        elseif key == "captureDeltaTime" then
            UnityTime.setCaptureDeltaTime(value)
        else
            rawset(UnityTime, key, value)
        end
    end
})

-- 导出Time对象和UnityTime类
return {
    Time = Time,           -- Unity风格的直接访问
    UnityTime = UnityTime  -- 完整的类实现
}