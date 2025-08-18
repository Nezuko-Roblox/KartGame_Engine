-- Roblox到Unity的适配器，提供Unity原生API的等效实现
local UnityMath = require(script.Parent.UnityMath)
local UnityVector3 = UnityMath.Vector3
local Quaternion = UnityMath.Quaternion
-- 保留对Roblox原生Vector3的引用
local RobloxVector3 = Vector3
-- 在此脚本中，Vector3指向Unity的Vector3
local Vector3 = UnityVector3

local RobloxUnityAdapter = {}

-- 前向声明所有适配器类
local Transform
local RigidbodyAdapter
local ColliderAdapter
local RendererAdapter
local AnimationAdapter

-- Rigidbody适配器
RigidbodyAdapter = {}
RigidbodyAdapter.__index = RigidbodyAdapter

function RigidbodyAdapter.new(gameObject)
    local self = setmetatable({}, RigidbodyAdapter)
    self.GameObject = gameObject
    
    -- 模拟Unity的Kinematic Rigidbody：设置所有BasePart为Anchored
    self:setKinematicMode(true)
    
    -- 缓存身体组件引用（主要用于兼容性）
    self._bodyAngularVelocity = self.GameObject.gameObject:FindFirstChild("BodyAngularVelocity")
    -- 不再使用BodyVelocity和BodyPosition - 完全通过Transform控制位置
    self._bodyVelocity = nil
    self._bodyPosition = nil
    
    -- 设置子物体位置同步系统（模拟Unity刚体行为）
    self:setupChildSync()
    
    return self
end

-- 设置Kinematic模式（模拟Unity的isKinematic = true）
function RigidbodyAdapter:setKinematicMode(isKinematic)
    local function setAnchored(parent, anchored)
        for _, child in pairs(parent:GetChildren()) do
            if child:IsA("BasePart") then
                child.Anchored = anchored
            elseif child:IsA("Model") or child:IsA("Folder") then
                setAnchored(child, anchored)
            end
        end
    end
    
    if self.GameObject.gameObject:IsA("BasePart") then
        self.GameObject.gameObject.Anchored = isKinematic
    else
        setAnchored(self.GameObject.gameObject, isKinematic)
    end
end

-- Unity刚体子物体自动跟随的模拟
function RigidbodyAdapter:setupChildSync()
    local RunService = game:GetService("RunService")
    local gameObject = self.GameObject.gameObject
    
    -- 对于Model，不需要子物体同步，Model本身会处理
    if gameObject:IsA("Model") then
        return
    end
    
    local children = {}
    local childOffsets = {}
    
    -- 收集所有子物体并计算初始相对位置
    for _, child in pairs(gameObject:GetChildren()) do
        if child:IsA("BasePart") and child ~= gameObject then
            table.insert(children, child)
            local relativePos = gameObject.CFrame:Inverse() * child.CFrame
            childOffsets[child] = relativePos.Position
        end
    end
    
    if #children == 0 then return end
    
    local lastCarCFrame = gameObject.CFrame
    
    -- 使用Heartbeat进行高频率同步
    local connection = RunService.Heartbeat:Connect(function()
        local currentCarCFrame = gameObject.CFrame
        
        if currentCarCFrame ~= lastCarCFrame then
            for _, child in pairs(children) do
                if child.Parent and childOffsets[child] then
                    local newWorldPos = currentCarCFrame:PointToWorldSpace(childOffsets[child])
                    -- 只更新位置，保持当前旋转（避免与Transform.localRotation冲突）
                    local currentRotation = child.CFrame - child.CFrame.Position
                    child.CFrame = CFrame.new(newWorldPos) * currentRotation
                end
            end
            lastCarCFrame = currentCarCFrame
        end
    end)
    
    -- 当对象被销毁时断开连接
    gameObject.AncestryChanged:Connect(function()
        if not gameObject.Parent then
            connection:Disconnect()
        end
    end)
end

function RigidbodyAdapter:__index(key)
    if key == "velocity" then
        -- 在Kinematic模式下，velocity由外部系统维护，这里只返回存储的值
        return self._currentVelocity or Vector3.zero
    elseif key == "mass" then
        -- 获取Model的总质量
        if self.GameObject.gameObject:IsA("BasePart") then
            return self.GameObject.gameObject.Mass
        else
            local totalMass = 0
            for _, part in pairs(self.GameObject.gameObject:GetDescendants()) do
                if part:IsA("BasePart") then
                    totalMass = totalMass + part.Mass
                end
            end
            return totalMass
        end
    elseif key == "position" then
        -- 获取Model的中心位置
        if self.GameObject.gameObject:IsA("BasePart") then
            local pos = self.GameObject.gameObject.Position
            return Vector3.new(pos.X, pos.Y, pos.Z)
        elseif self.GameObject.PrimaryPart then
            local pos = self.GameObject.PrimaryPart.Position
            return Vector3.new(pos.X, pos.Y, pos.Z)
        else
            local firstPart = self.GameObject.gameObject:FindFirstChildOfClass("BasePart")
            if firstPart then
                local pos = firstPart.Position
                return Vector3.new(pos.X, pos.Y, pos.Z)
            else
                return Vector3.zero
            end
        end
    elseif key == "freezeRotation" then
        -- 检查是否存在BodyAngularVelocity来判断是否冻结旋转
        return self._bodyAngularVelocity ~= nil
    elseif key == "useGravity" then
        -- 在Kinematic模式下，重力由外部物理系统处理
        return self._useGravity ~= false
    elseif key == "isKinematic" then
        -- 返回存储的isKinematic状态，如果未设置则检查锚定状态
        if self._isKinematic ~= nil then
            return self._isKinematic
        end
        -- 回退：检查是否锚定来判断是否为运动学模式
        if self.GameObject.gameObject:IsA("BasePart") then
            return self.GameObject.gameObject.Anchored
        else
            -- 对于Model，检查PrimaryPart或第一个BasePart是否锚定
            local targetPart = self.GameObject.PrimaryPart or self.GameObject.gameObject:FindFirstChildOfClass("BasePart")
            if targetPart then
                return targetPart.Anchored
            end
            return false
        end
    else
        return rawget(RigidbodyAdapter, key)
    end
end

function RigidbodyAdapter:__newindex(key, value)
    if key == "velocity" then
        -- 在Kinematic模式下，只存储velocity值，实际移动由Transform控制
        self._currentVelocity = Vector3.new(value.X, value.Y, value.Z)
    elseif key == "mass" then
        -- 设置Model的质量 - 使用CustomPhysicalProperties
        if self.GameObject.gameObject:IsA("BasePart") then
            -- 对于单个BasePart，使用CustomPhysicalProperties设置质量
            local currentProps = self.GameObject.gameObject.CustomPhysicalProperties or PhysicalProperties.new(Enum.Material.Plastic)
            local newProps = PhysicalProperties.new(
                currentProps.Density,
                currentProps.Friction, 
                currentProps.Elasticity,
                currentProps.FrictionWeight,
                currentProps.ElasticityWeight
            )
            -- 通过调整密度来控制质量
            local volume = self.GameObject.gameObject.Size.X * self.GameObject.gameObject.Size.Y * self.GameObject.gameObject.Size.Z
            local density = value / volume
            newProps = PhysicalProperties.new(density, newProps.Friction, newProps.Elasticity, newProps.FrictionWeight, newProps.ElasticityWeight)
            self.GameObject.gameObject.CustomPhysicalProperties = newProps
        else
            -- 对于Model，递归收集所有层级的BasePart
            local parts = {}
            local function collectBaseParts(parent)
                for _, child in pairs(parent:GetChildren()) do
                    if child:IsA("BasePart") then
                        table.insert(parts, child)
                    elseif child:IsA("Model") or child:IsA("Folder") then
                        -- 递归处理嵌套的Model和Folder
                        collectBaseParts(child)
                    end
                end
            end
            collectBaseParts(self.GameObject.gameObject)
            if #parts > 0 then
                -- 计算总体积
                local totalVolume = 0
                local partVolumes = {}
                for i, part in pairs(parts) do
                    local volume = part.Size.X * part.Size.Y * part.Size.Z
                    partVolumes[i] = volume
                    totalVolume = totalVolume + volume
                end
                
                -- 按体积比例分配质量
                for i, part in pairs(parts) do
                    local volume = partVolumes[i]
                    local massForThisPart = value * (volume / totalVolume)  -- 按体积比例分配
                    local density = massForThisPart / volume
                    
                    local currentProps = part.CustomPhysicalProperties or PhysicalProperties.new(Enum.Material.Plastic)
                    local newProps = PhysicalProperties.new(
                        density,
                        currentProps.Friction, 
                        currentProps.Elasticity,
                        currentProps.FrictionWeight,
                        currentProps.ElasticityWeight
                    )
                    part.CustomPhysicalProperties = newProps
                end
            end
        end
    elseif key == "centerOfMass" then
        -- 存储自定义质心位置
        self._customCenterOfMass = RobloxVector3.new(value.X, value.Y, value.Z)
        
        -- 关闭所有BasePart的物理引擎，但保持碰撞检测
        local function disablePhysics(parent)
            for _, child in pairs(parent:GetChildren()) do
                if child:IsA("BasePart") then
                    child.Anchored = true
                elseif child:IsA("Model") or child:IsA("Folder") then
                    disablePhysics(child)
                end
            end
        end
        
        if self.GameObject.gameObject:IsA("BasePart") then
            self.GameObject.gameObject.Anchored = true
        else
            disablePhysics(self.GameObject.gameObject)
        end
    elseif key == "freezeRotation" then
        -- 通过BodyAngularVelocity来实现
        local bav = self.GameObject.gameObject:FindFirstChild("BodyAngularVelocity")
        if value then
            if not bav then
                bav = Instance.new("BodyAngularVelocity")
                bav.MaxTorque = RobloxVector3.new(4000, 4000, 4000)
                bav.AngularVelocity = RobloxVector3.new(0, 0, 0)
                bav.Parent = self.GameObject.gameObject
            end
        else
            if bav then
                bav:Destroy()
            end
        end
    elseif key == "useGravity" then
        -- 在Kinematic模式下，重力由外部物理系统处理，这里只存储设置
        self._useGravity = value
    elseif key == "isKinematic" then
        -- 运动学模式：禁用所有物理响应
        if self.GameObject.gameObject:IsA("BasePart") then
            self.GameObject.gameObject.Anchored = value
        elseif self.GameObject.gameObject:IsA("Model") then
            -- 对于Model，立即设置所有BasePart的锚定状态防止掉落
            for _, part in pairs(self.GameObject.gameObject:GetDescendants()) do
                if part:IsA("BasePart") then
                    part.Anchored = value
                end
            end
        end
        -- 存储isKinematic状态
        self._isKinematic = value
    else
        rawset(self, key, value)
    end
end

-- Collider适配器
ColliderAdapter = {}
ColliderAdapter.__index = ColliderAdapter

function ColliderAdapter.new(gameObject)
    local self = setmetatable({}, ColliderAdapter)
    self.GameObject = gameObject
    return self
end

function ColliderAdapter:__index(key)
    if key == "bounds" then
        local bounds = {}
        if self.GameObject.gameObject:IsA("Model") then
            -- 对于Model，使用GetBoundingBox
            local cf, size = self.GameObject.gameObject:GetBoundingBox()
            bounds.center = Vector3.new(cf.Position.X, cf.Position.Y, cf.Position.Z)
            bounds.size = Vector3.new(size.X, size.Y, size.Z)
        elseif self.GameObject.gameObject:IsA("BasePart") then
            -- 对于Part，使用Position和Size
            bounds.center = Vector3.new(self.GameObject.gameObject.Position.X, self.GameObject.gameObject.Position.Y, self.GameObject.gameObject.Position.Z)
            bounds.size = Vector3.new(self.GameObject.gameObject.Size.X, self.GameObject.gameObject.Size.Y, self.GameObject.gameObject.Size.Z)
        else
            -- 默认值
            bounds.center = Vector3.zero
            bounds.size = Vector3.one
        end
        return bounds
    else
        return rawget(ColliderAdapter, key)
    end
end

-- Renderer适配器
RendererAdapter = {}
RendererAdapter.__index = RendererAdapter

function RendererAdapter.new(gameObject)
    local self = setmetatable({}, RendererAdapter)
    self.GameObject = gameObject
    return self
end

function RendererAdapter:__index(key)
    if key == "enabled" then
        return self.GameObject.gameObject.Transparency < 1
    elseif key == "material" then
        return self.GameObject.gameObject.Material
    else
        return rawget(RendererAdapter, key)
    end
end

function RendererAdapter:__newindex(key, value)
    if key == "enabled" then
        self.GameObject.gameObject.Transparency = value and 0 or 1
    elseif key == "material" then
        self.GameObject.gameObject.Material = value
    else
        rawset(self, key, value)
    end
end

-- Animation适配器
AnimationAdapter = {}
AnimationAdapter.__index = AnimationAdapter

function AnimationAdapter.new(gameObject)
    local self = setmetatable({}, AnimationAdapter)
    self.GameObject = gameObject
    self._animationTrack = nil
    return self
end

function AnimationAdapter:Play(animationName)
    local humanoid = self.GameObject.gameObject:FindFirstChild("Humanoid")
    if humanoid then
        local animation = self.GameObject.gameObject:FindFirstChild(animationName)
        if animation then
            self._animationTrack = humanoid:LoadAnimation(animation)
            self._animationTrack:Play()
        end
    end
end

function AnimationAdapter:Stop()
    if self._animationTrack then
        self._animationTrack:Stop()
    end
end

function AnimationAdapter:CrossFade(animationName, fadeTime)
    if self._animationTrack then
        self._animationTrack:AdjustWeight(0, fadeTime)
    end
    self:Play(animationName)
end

-- GameObject类
local GameObject = {}

-- 自定义__index方法来代理底层Roblox对象的属性
function GameObject:__index(key)
    -- 先检查GameObject自己的方法和属性
    local method = rawget(GameObject, key)
    if method then
        return method
    end
    
    -- 检查实例的属性
    local value = rawget(self, key)
    if value then
        return value
    end
    
    -- 最后代理到底层Roblox对象
    local robloxObject = rawget(self, "gameObject")
    if robloxObject then
        -- 特殊处理Model没有的属性
        if key == "Position" then
            if robloxObject:IsA("Model") then
                -- 对于Model，使用PrimaryPart或第一个BasePart的Position
                local targetPart = robloxObject.PrimaryPart or robloxObject:FindFirstChildWhichIsA("BasePart", true)
                if targetPart then
                    return targetPart.Position
                else
                    return RobloxVector3.new(0, 0, 0)
                end
            else
                return robloxObject.Position
            end
        elseif key == "Size" then
            if robloxObject:IsA("Model") then
                -- 对于Model，使用GetBoundingBox获取尺寸
                local success, cf, size = pcall(function()
                    return robloxObject:GetBoundingBox()
                end)
                if success and size then
                    return size
                else
                    -- 如果GetBoundingBox失败，返回默认尺寸
                    return RobloxVector3.new(2, 3, 2)
                end
            else
                return robloxObject.Size
            end
        elseif key == "CFrame" then
            if robloxObject:IsA("Model") then
                local targetPart = robloxObject.PrimaryPart or robloxObject:FindFirstChildWhichIsA("BasePart", true)
                if targetPart then
                    return targetPart.CFrame
                else
                    return CFrame.new()
                end
            else
                return robloxObject.CFrame
            end
        elseif pcall(function() return robloxObject[key] end) then
            return robloxObject[key]
        end
    end
    
    return nil
end

function GameObject.new(robloxObject)
    local self = setmetatable({}, GameObject)
    self.gameObject = robloxObject  -- 底层Roblox对象
    self.name = robloxObject.Name
    self.transform = Transform.new(robloxObject, self)
    return self
end

function GameObject:GetComponent(componentType)
    -- 根据不同的组件类型返回相应的适配器
    if componentType == "Rigidbody" then
        return RigidbodyAdapter.new(self)
    elseif componentType == "Collider" or componentType == "BoxCollider" then
        -- 在Roblox中，Part本身就是碰撞器
        return ColliderAdapter.new(self)
    elseif componentType == "Renderer" then
        return RendererAdapter.new(self)
    elseif componentType == "Animation" then
        return AnimationAdapter.new(self)
    elseif componentType == "RigidbodyFPSWalker" then
        -- 查找存储在GameObject上的RigidbodyFPSWalker组件
        local attr = self.gameObject:GetAttribute("RigidbodyFPSWalker")
        return attr
    else
        return self.gameObject:FindFirstChild(componentType)
    end
end

function GameObject:GetComponentInChildren(componentType)
    -- 递归查找子对象中的组件
    local function searchComponent(obj)
        local comp = obj:FindFirstChild(componentType)
        if comp then return comp end
        
        for _, child in pairs(obj:GetChildren()) do
            local result = searchComponent(child)
            if result then return result end
        end
        return nil
    end
    
    return searchComponent(self.gameObject)
end

function GameObject:GetComponentsInChildren(componentType)
    local components = {}
    
    local function searchComponents(obj)
        local comp = obj:FindFirstChild(componentType)
        if comp then table.insert(components, comp) end
        
        for _, child in pairs(obj:GetChildren()) do
            searchComponents(child)
        end
    end
    
    searchComponents(self.gameObject)
    return components
end

function GameObject:AddComponent(componentType)
    -- 在Roblox中添加对应的组件
    local instance = Instance.new(componentType)
    instance.Parent = self.gameObject
    return instance
end

function GameObject:SetActiveRecursively(active)
    -- 递归设置可见性
    local function setVisible(obj, visible)
        if obj:IsA("BasePart") then
            obj.Transparency = visible and 0 or 1
        elseif obj:IsA("GuiObject") then
            obj.Visible = visible
        end
        
        for _, child in pairs(obj:GetChildren()) do
            setVisible(child, visible)
        end
    end
    
    setVisible(self.gameObject, active)
end

function GameObject:Find(name)
    return workspace:FindFirstChild(name, true)
end

-- 获取底层Roblox对象的方法
function GameObject:GetRobloxObject()
    return self.gameObject
end

-- Transform类实现
Transform = {}
Transform.__index = Transform

function Transform.new(robloxObject, gameObject)
    local self = setmetatable({}, Transform)
    self._robloxObject = robloxObject
    self.gameObject = gameObject  -- Unity模式：transform.gameObject 指向拥有此transform的GameObject
    return self
end

-- 位置属性
function Transform:__index(key)
    if key == "localPosition" then
        if self._robloxObject:IsA("BasePart") then
            if self._robloxObject.Parent and self._robloxObject.Parent:IsA("BasePart") then
                -- 有BasePart父对象时，计算相对于父对象的本地位置
                local parentCFrame = self._robloxObject.Parent.CFrame
                local localCFrame = parentCFrame:Inverse() * self._robloxObject.CFrame
                return Vector3.new(localCFrame.Position.X, localCFrame.Position.Y, localCFrame.Position.Z)
            else
                -- 没有BasePart父对象时，返回世界位置
                return Vector3.new(self._robloxObject.Position.X, self._robloxObject.Position.Y, self._robloxObject.Position.Z)
            end
        elseif self._robloxObject:IsA("Model") then
            -- 对于Model，使用PrimaryPart或第一个BasePart的位置
            local targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
            if targetPart then
                return Vector3.new(targetPart.Position.X, targetPart.Position.Y, targetPart.Position.Z)
            end
        end
        return Vector3.zero
    elseif key == "position" then
        if self._robloxObject:IsA("BasePart") then
            return Vector3.new(self._robloxObject.Position.X, self._robloxObject.Position.Y, self._robloxObject.Position.Z)
        elseif self._robloxObject:IsA("Model") then
            -- 对于Model，使用PrimaryPart或第一个BasePart的位置
            local targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
            if targetPart then
                return Vector3.new(targetPart.Position.X, targetPart.Position.Y, targetPart.Position.Z)
            end
        end
        return Vector3.zero
    elseif key == "localRotation" then
        local targetPart = nil
        if self._robloxObject:IsA("BasePart") then
            targetPart = self._robloxObject
        elseif self._robloxObject:IsA("Model") then
            targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
        end
        
        if targetPart then
            local cf = targetPart.CFrame

            -- 获取父物体的旋转（如果有父物体的话）
            local parentRotation = CFrame.new()
            if targetPart.Parent and targetPart.Parent:IsA("BasePart") then
                parentRotation = targetPart.Parent.CFrame.Rotation
            end

            -- 本地旋转CFrame = 父物体旋转的逆 * 当前世界旋转
            local localRotationCFrame = parentRotation:Inverse() * cf.Rotation
            local x, y, z = localRotationCFrame:ToEulerAnglesXYZ()
            return Quaternion.Euler(math.deg(x), math.deg(y), math.deg(z))
        end
        return Quaternion.identity
    elseif key == "forward" then
        local targetPart = nil
        if self._robloxObject:IsA("BasePart") then
            targetPart = self._robloxObject
        elseif self._robloxObject:IsA("Model") then
            targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
        end
        
        if targetPart then
            local cf = targetPart.CFrame
            -- Unity标准坐标系转换：Roblox LookVector(-Z) 转换为 Unity forward(+Z)
            return Vector3.new(-cf.LookVector.X, -cf.LookVector.Y, -cf.LookVector.Z)
        end
        return Vector3.forward
    elseif key == "right" then
        local targetPart = nil
        if self._robloxObject:IsA("BasePart") then
            targetPart = self._robloxObject
        elseif self._robloxObject:IsA("Model") then
            targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
        end
        
        if targetPart then
            local cf = targetPart.CFrame
            return Vector3.new(cf.RightVector.X, cf.RightVector.Y, cf.RightVector.Z)
        end
        return Vector3.right
    elseif key == "up" then
        local targetPart = nil
        if self._robloxObject:IsA("BasePart") then
            targetPart = self._robloxObject
        elseif self._robloxObject:IsA("Model") then
            targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
        end
        
        if targetPart then
            local cf = targetPart.CFrame
            return Vector3.new(cf.UpVector.X, cf.UpVector.Y, cf.UpVector.Z)
        end
        return Vector3.up
    else
        return rawget(Transform, key)
    end
end

function Transform:__newindex(key, value)
    if key == "localPosition" then
        -- Roblox没有相对位置概念，直接设置世界位置
        -- 父子关系会自动处理跟随移动
        if self._robloxObject:IsA("BasePart") then
            self._robloxObject.Position = RobloxVector3.new(value.X, value.Y, value.Z)
        elseif self._robloxObject:IsA("Model") then
            local targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
            if targetPart then
                local offset = targetPart.Position - self._robloxObject:GetBoundingBox().Position
                self._robloxObject:SetPrimaryPartCFrame(CFrame.new(RobloxVector3.new(value.X, value.Y, value.Z) + offset))
            end
        end
    elseif key == "position" then
        if self._robloxObject:IsA("BasePart") then
            self._robloxObject.Position = RobloxVector3.new(value.X, value.Y, value.Z)
        elseif self._robloxObject:IsA("Model") and self._robloxObject.PrimaryPart then
            -- 对于Model，使用SetPrimaryPartCFrame移动整个模型
            local currentCFrame = self._robloxObject:GetPrimaryPartCFrame()
            self._robloxObject:SetPrimaryPartCFrame(CFrame.new(RobloxVector3.new(value.X, value.Y, value.Z)) * currentCFrame.Rotation)
        end
    elseif key == "localRotation" then
        local targetPart = nil
        local isModel = false
        
        if self._robloxObject:IsA("BasePart") then
            targetPart = self._robloxObject
        elseif self._robloxObject:IsA("Model") then
            targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
            isModel = true
        end
        
        if targetPart then
            -- 按照轮子滚动逻辑：直接使用Quaternion的toCFrame方法
            local cf = targetPart.CFrame
            local pos = cf.Position
            
            -- 获取父物体的旋转（如果有父物体的话）
            local parentRotation = CFrame.new()
            if targetPart.Parent and targetPart.Parent:IsA("BasePart") then
                parentRotation = targetPart.Parent.CFrame.Rotation
            end
            
            -- 使用和Quaternion.Euler相同的转换逻辑
            local rotationCFrame = nil
            if value.toCFrame then
                -- 使用Quaternion的toCFrame方法（与Quaternion.Euler内部逻辑一致）
                local localRotationCFrame = value:toCFrame()
                -- 世界旋转 = 父物体旋转 * 本地旋转
                local worldRotation = parentRotation * localRotationCFrame
                rotationCFrame = CFrame.new(pos) * worldRotation
            elseif value.X and value.Y and value.Z and value.w then
                -- value是Quaternion对象，直接使用Quaternion的转换方法
                local localRotationCFrame = value:toCFrame()
                local worldRotation = parentRotation * localRotationCFrame
                rotationCFrame = CFrame.new(pos) * worldRotation
            else
                -- 其他情况，可能是欧拉角
                local localRotationCFrame = CFrame.Angles(math.rad(value.X), math.rad(value.Y), math.rad(value.Z))
                local worldRotation = parentRotation * localRotationCFrame
                rotationCFrame = CFrame.new(pos) * worldRotation
            end
            
            if rotationCFrame then
                if isModel then
                    self._robloxObject:SetPrimaryPartCFrame(rotationCFrame)
                else
                    self._robloxObject.CFrame = rotationCFrame
                end
            end
        end
    else
        rawset(self, key, value)
    end
end

function Transform:GetChild(index)
    local children = self._robloxObject:GetChildren()
    if children[index + 1] then
        return GameObject.new(children[index + 1])
    end
    return nil
end

function Transform:Find(name)
    local child = self._robloxObject:FindFirstChild(name)
    if child then
        return GameObject.new(child)
    end
    return nil
end

function Transform:RotateAroundLocal(axis, angle)
    local targetPart = nil
    local isModel = false
    
    if self._robloxObject:IsA("BasePart") then
        targetPart = self._robloxObject
    elseif self._robloxObject:IsA("Model") and self._robloxObject.PrimaryPart then
        targetPart = self._robloxObject.PrimaryPart
        isModel = true
    end
    
    if targetPart then
        local cf = targetPart.CFrame
        local rotationCFrame = nil
        
        -- 使用向量分量比较而不是对象比较
        if math.abs(axis.X) < 0.1 and math.abs(axis.Y - 1) < 0.1 and math.abs(axis.Z) < 0.1 then
            -- Y轴旋转 (up)
            rotationCFrame = cf * CFrame.Angles(0, math.rad(angle), 0)
        elseif math.abs(axis.X - 1) < 0.1 and math.abs(axis.Y) < 0.1 and math.abs(axis.Z) < 0.1 then
            -- X轴旋转 (right)
            rotationCFrame = cf * CFrame.Angles(math.rad(angle), 0, 0)
        elseif math.abs(axis.X) < 0.1 and math.abs(axis.Y) < 0.1 and math.abs(axis.Z - 1) < 0.1 then
            -- Z轴旋转 (forward)
            rotationCFrame = cf * CFrame.Angles(0, 0, math.rad(angle))
        end
        
        if rotationCFrame then
            if isModel then
                self._robloxObject:SetPrimaryPartCFrame(rotationCFrame)
            else
                self._robloxObject.CFrame = rotationCFrame
            end
        end
    end
end

function Transform:TransformPoint(localPoint)
    local targetPart = nil
    
    if self._robloxObject:IsA("BasePart") then
        targetPart = self._robloxObject
    elseif self._robloxObject:IsA("Model") then
        targetPart = self._robloxObject.PrimaryPart or self._robloxObject:FindFirstChildWhichIsA("BasePart", true)
    end
    
    if targetPart then
        local cf = targetPart.CFrame
        local worldPoint = cf:PointToWorldSpace(RobloxVector3.new(localPoint.X, localPoint.Y, localPoint.Z))
        return Vector3.new(worldPoint.X, worldPoint.Y, worldPoint.Z)
    end
    
    return localPoint
end

-- 获取子对象数量
function Transform:GetChildCount()
    return #self._robloxObject:GetChildren()
end


-- 导出所有类
RobloxUnityAdapter.GameObject = GameObject
RobloxUnityAdapter.Transform = Transform
RobloxUnityAdapter.RigidbodyAdapter = RigidbodyAdapter
RobloxUnityAdapter.ColliderAdapter = ColliderAdapter
RobloxUnityAdapter.RendererAdapter = RendererAdapter
RobloxUnityAdapter.AnimationAdapter = AnimationAdapter

return RobloxUnityAdapter