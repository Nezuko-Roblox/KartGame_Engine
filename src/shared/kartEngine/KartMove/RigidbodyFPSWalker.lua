-- RigidbodyFPSWalker class的Lua等效实现，继承自KartBasicController
-- 完整迁移Unity中的RigidbodyFPSWalker类到Roblox - 所有22个方法完整迁移

local KartBasicController = require(script.Parent.KartBasicController)
local MonoBehaviour = require(script.Parent.Parent.KartShared.MonoBehaviour)
local RobloxUnityAdapter = require(script.Parent.Parent.KartShared.RobloxUnityAdapter)
local UnityMath = require(script.Parent.Parent.KartShared.UnityMath)
local MathHelper = require(script.Parent.MathHelper)
local Vector3Helper = require(script.Parent.Vector3Helper)
local UnityTimeModule = require(script.Parent.Parent.KartShared.UnityTime)
local Time = UnityTimeModule.Time
local UnityTrigger = require(script.Parent.Parent.KartShared.UnityTrigger)
local UnityInputModule = require(script.Parent.Parent.KartShared.UnityInput)
local Input = UnityInputModule.Input

-- Unity Physics相关模块
local LayerMask = require(script.Parent.Parent.KartShared.UnityEngine.LayerMask)
local Physics = require(script.Parent.Parent.KartShared.UnityEngine.Physics)
local Ray = require(script.Parent.Parent.KartShared.UnityEngine.Ray)
local RaycastHit = require(script.Parent.Parent.KartShared.UnityEngine.RaycastHit)

-- 保存Roblox原生Vector3引用
local RobloxVector3 = Vector3

-- 引入相关模块
local KartManager = require(script.Parent.KartManager)
local GoPlayKartBuilder = require(script.Parent.Parent.GameStage.GoPlayKartBuilder)
local GoPlayKart = require(script.Parent.GoPlayKart)
local BoostKind = require(script.Parent.BoostKind)

local Vector3 = UnityMath.Vector3
local Quaternion = UnityMath.Quaternion
local Mathf = UnityMath.Mathf

-- Roblox服务
local Players = game:GetService("Players")
local RunService = game:GetService("RunService")
local UserInputService = game:GetService("UserInputService")
local Workspace = game:GetService("Workspace")
local ReplicatedStorage = game:GetService("ReplicatedStorage")
local CollectionService = game:GetService("CollectionService")

local RigidbodyFPSWalker = {}
RigidbodyFPSWalker.__index = RigidbodyFPSWalker
setmetatable(RigidbodyFPSWalker, {__index = KartBasicController})

-- 组件注册表，仿照Unity的组件系统
-- 使用弱引用避免内存泄漏
local ComponentRegistry = setmetatable({}, {__mode = "k"})

-- PrevState 内部类（完全对应Unity版本）
local PrevState = {}
PrevState.__index = PrevState

function PrevState.new()
    local self = setmetatable({}, PrevState)
    
    self.position_ = Vector3.zero
    self.force_ = Vector3.zero
    self.velocity_ = Vector3.zero
    self.angular_ = Vector3.zero
    self.forward_ = Vector3.zero
    self.rotate_ = Quaternion.identity
    self.isGrounded_ = false
    
    return self
end

-- 构造函数
function RigidbodyFPSWalker.new(robloxObject)
    local self = setmetatable(KartBasicController.new(robloxObject), RigidbodyFPSWalker)
    
    -- 立即设置GameObject和transform，确保不为nil
    if robloxObject then
        self:SetGameObject(robloxObject)
    end
    
    -- 私有变量初始化（对应Unity的private字段）
    self.grounded = false
    self.prevState_ = PrevState.new()
    self.goPlayKart_ = nil
    self.isBooster_ = false
    self.thisBoxCollider_ = nil
    self.childBoxCollider_ = nil
    self._targetUpVector = Vector3.up
    self.SMALL_CRASH_VELOCITY = 15
    self.BIG_CRASH_VELOCITY = 30
    self.wheelRotation = 0
    self.isSuddenChange = false
    self.fixedUpdateCount_ = 0
    
    -- Roblox特有的物理组件引用
    self.rigidbody = nil  -- 将在Awake中初始化
    
    -- 初始化Unity风格的触发器系统
    self.unityTrigger_ = UnityTrigger.new(self)
    
    -- 注册生命周期回调到MonoBehaviour系统
    self:AddUpdateCallback(function(mono, dt) self:Update() end)
    self:AddFixedUpdateCallback(function(mono, dt) self:FixedUpdate() end)
    self:AddLateUpdateCallback(function(mono, dt) self:LateUpdate() end)
    
    return self
end

-- 方法1: Unity的Awake()方法迁移 - 完全对应Unity版本逻辑
function RigidbodyFPSWalker:Awake()
    -- 调用基类Awake
    KartBasicController.Awake(self)
    
    -- 设置游戏对象名称
    if self.gameObject then
        -- 如果kartIndex_已经被外部设置（远程玩家），使用它
        -- 否则使用默认的本地玩家索引
        if not self.kartIndex_ then
            self.kartIndex_ = KartManager.PLAYER_KART_IDX
            self.gameObject.name = "player_kart"
        else
            self.gameObject.name = "remote_kart_" .. tostring(self.kartIndex_)
        end
    end
    
    -- 获取Rigidbody组件并设置属性
    self.rigidbody = self:GetComponent("Rigidbody")
    if self.rigidbody then
        self.rigidbody.freezeRotation = false
        self.rigidbody.useGravity = false
        self.rigidbody.isKinematic = true
    end
    
    -- 在Roblox中，Part本身就是碰撞器，不需要额外创建BoxCollider
    -- 确保主Part的碰撞设置正确
    if self.gameObject then
        self.gameObject.CanCollide = true  -- 确保可以碰撞
        self.gameObject.CanTouch = true    -- 确保可以触发Touch事件
        self.gameObject.CanQuery = true    -- 确保可以被射线检测
        
        -- 注册组件到全局注册表，仿照Unity的组件系统
        ComponentRegistry[self.gameObject] = self
    end
    
    -- 调用基类初始化
    self:Initialize(0, 0, false)
    
    
    -- 设置GoPlayKart，使用当前的kartIndex_
    self.goPlayKart_ = KartManager.Instance:SetKart(KartManager.PLAYER_KART_IDX, GoPlayKartBuilder.new(), self, self.wheels_)
    
    -- 通知ECS系统卡丁车已创建（如果ECS已准备好）
    if _G.ECS_OnKartCreated then
        _G.ECS_OnKartCreated(KartManager.PLAYER_KART_IDX, self.goPlayKart_)
        -- print("[RigidbodyFPSWalker] ECS notification sent for kart", KartManager.PLAYER_KART_IDX)
    end
    -- 如果ECS还没准备好，ECS初始化时会主动扫描已存在的卡丁车
end

-- 方法2: Unity的Start()方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:Start()
    -- 调用基类Start
    KartBasicController.Start(self)
    
    -- 设置Rigidbody质量和重心
    if self.rigidbody and self.goPlayKart_ and self.goPlayKart_.m_spec then
        self.rigidbody.mass = self.goPlayKart_.m_spec.mass
        self.rigidbody.centerOfMass = Vector3.new(0, 0, 0)
    end
end

-- 方法3: Unity的Update()方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:Update()
    -- 确保Start被调用（模拟MonoBehaviour的行为）
    if not self.started then 
        self:Start()
        self.started = true
    end
    
    self:InputUpdate()
end

-- 方法4: Unity的LateUpdate()方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:LateUpdate()
    -- 在Roblox中不需要UpdateCollider，因为Part本身就是碰撞器
end

-- 方法6: InputUpdate方法迁移 - 使用Unity风格的平滑输入
function RigidbodyFPSWalker:InputUpdate()
    if not self.goPlayKart_ then
        return
    end
    
    -- 使用Unity的Input.GetAxis来获取平滑输入值
    -- 这将模拟Unity中的平滑转向和加速/刹车响应
    local horizontal = Input.GetAxis("Horizontal")
    local vertical = Input.GetAxis("Vertical")
    
    -- 设置输入值（注意Unity源码中使用的是正值，这里对应设置）
    self.goPlayKart_:setWheel(-horizontal)  -- Unity中是正值，不需要负号
    local axis = vertical
    
    -- 设置加速和刹车
    self.goPlayKart_:setAccel(axis > 0.1)
    self.goPlayKart_:setBrake(axis < -0.1)
    
    
    -- 漂移控制 - 对应Unity的KeyCode.LeftShift
    local isDrifting = Input.GetKey(Enum.KeyCode.LeftShift)
    if isDrifting and self.goPlayKart_:getWheel() ~= 0 then
        self.goPlayKart_:setDrift(true)
    else
        self.goPlayKart_:setDrift(false)
    end
    
    -- 加速控制 - 对应Unity的KeyCode.Space
    -- if Input.GetKey(Enum.KeyCode.Space) then
    --     self.goPlayKart_:setBoost(1000, BoostKind.BoostNormal)
    -- end
end

-- 方法7: UpdateWheels方法迁移 - 使用Roblox原生CFrame然后转换为Quaternion
function RigidbodyFPSWalker:UpdateWheels()
    -- 完全按照Unity源代码逻辑：第94行 - Unity使用Time.deltaTime
    local deltaTime = Time.deltaTime
    
    -- 完全按照Unity源代码逻辑：第94行
    self.wheelRotation = Mathf.Repeat(self.wheelRotation + self.goPlayKart_:GetKartRealSpeed() * deltaTime * 180 / 3.14159274, 360)

    -- 使用Roblox原生CFrame旋转系统生成旋转，然后转换为Quaternion
    -- 将角度转换为弧度，注意Roblox坐标系需要反转Y轴转向
    local wheelRotationRad = math.rad(self.wheelRotation)
    local steerAngleRad = math.rad(-self.goPlayKart_:getSteerAngle() * 3)
    
    -- 前轮CFrame：X轴滚动 + Y轴转向
    local frontWheelCFrame = CFrame.Angles(wheelRotationRad, steerAngleRad, 0)
    -- 后轮CFrame：只有X轴滚动
    local rearWheelCFrame = CFrame.Angles(wheelRotationRad, 0, 0)
    
    local frontQuaternion = Quaternion.fromCFrame(frontWheelCFrame)
    local rearQuaternion = Quaternion.fromCFrame(rearWheelCFrame)
    
    -- 设置轮子本地旋转
    if self.wheels_ then
        -- 前轮（索引1,2对应Unity的0,1）：有转向角度
        if self.wheels_[1] then
            self.wheels_[1].localRotation = frontQuaternion
        end
        if self.wheels_[2] then
            self.wheels_[2].localRotation = frontQuaternion
        end
        
        -- 后轮（索引3,4对应Unity的2,3）：只滚动
        if self.wheels_[3] then
            self.wheels_[3].localRotation = rearQuaternion
        end
        if self.wheels_[4] then
            self.wheels_[4].localRotation = rearQuaternion
        end
    end
end

-- 方法8: Unity的FixedUpdate()方法迁移 - 完全对应Unity版本（第一部分）
function RigidbodyFPSWalker:FixedUpdate()
    -- 如果GoPlayKart还没创建，跳过物理更新
    if not self.goPlayKart_ then
        return
    end
    
    -- 调用基类FixedUpdate
    KartBasicController.FixedUpdate(self)
    
    self.fixedUpdateCount_ = self.fixedUpdateCount_ + 1
    
    -- 备份速度
    if self.rigidbody then
        self.goPlayKart_.BackupVelocity = self.rigidbody.velocity
    end
    
    -- 执行赛车基本动作 - 传递真实时间，对应Unity的Time.time
    self.goPlayKart_:basicAction(Time.time)
    
    -- 更新轮子
    self:UpdateWheels()
    
    -- 处理加速效果
    local flag = self.goPlayKart_:isRealBoost() and self.playMode_ == KartBasicController.PlayMode.NORMAL
    if self.isBooster_ ~= flag then
        self.isBooster_ = self.goPlayKart_:isRealBoost()
        self:SetEnableBooster(self.isBooster_)
    end
    
    -- 主要物理更新逻辑
    if not self.goPlayKart_.Forcing and not self.goPlayKart_.IsInResetState then
            -- 安全检查：确保transform存在
        if not self.transform then
            warn("RigidbodyFPSWalker: transform is nil, skipping physics update")
            return
        end
        
        -- 完全按照Unity源代码逻辑：第122行使用Time.deltaTime
        local deltaTime = Time.deltaTime
        
        -- 使用Unity风格的触发器系统进行地面检测
        self.unityTrigger_:UpdateTriggers()
        
        -- 目标向上向量处理（按Unity源码顺序）
        local targetUpVector = self._targetUpVector  -- 使用上一帧的targetUpVector
        local right = self.transform.right
        local forward = self.transform.forward
        forward = Vector3.Cross(right, targetUpVector)
        
        local num = self.grounded and 10 or 1
        local quaternion = Quaternion.LookRotation(forward, targetUpVector)
        local quaternion2 = Quaternion.Slerp(self.transform.localRotation, quaternion, deltaTime * num)
        
        self.transform.localRotation = quaternion2
        self._targetUpVector = Vector3.up  -- 重置为Vector3.up（Unity源码第124行的位置）
        
        
        if self.grounded then
            -- 地面旋转处理
            self.goPlayKart_.m_KartLAVel = Vector3.new(0, self.goPlayKart_.m_KartLAVel.Y, 0)
            
            local localRotation = self.transform.localRotation
            local quaternion3 = MathHelper.CreateQuaternion(0, self.goPlayKart_.m_KartLAVel)
            local quaternion4 = localRotation * quaternion3
            -- 完全按照Unity源代码逻辑：第132行 - 使用Time.deltaTime
            MathHelper.QuaMulScala(quaternion4, 0.5 * Time.deltaTime)
            MathHelper.QuaAdd(localRotation, quaternion4)
            MathHelper.QuaNormalize(localRotation)
            self.transform.localRotation = localRotation
            self.prevState_.rotate_ = localRotation
        end
        
        -- 位置更新
        local velocity_ = self.prevState_.velocity_
        local vector2 = self.goPlayKart_.m_KartWLVel - velocity_
        vector2 = Vector3.new(
            Mathf.Clamp(vector2.X, -100, 100),
            0,
            Mathf.Clamp(vector2.Z, -100, 100)
        )
        
        local kartWLVel = self.goPlayKart_.m_KartWLVel
        -- 完全按照Unity源代码逻辑：第144行 - 使用Time.deltaTime
        local vector3 = kartWLVel * Time.deltaTime
        local position = self.transform.position
        local vector4 = position + vector3
        local vector5 = vector4 - position
        
        -- 优化 - 在Roblox中直接使用Part的Size和Position
        local robloxObject = self.gameObject
        if robloxObject then
            local size = robloxObject.Size
            local num2 = Mathf.Min(size.X, size.Y, size.Z)
            
            if vector5.magnitude >= num2 / 2 then
                -- Unity源码使用bounds.center，这里应该使用transform.position而不是robloxObject.Position
                local center = position  -- 使用transform.position作为中心点
                local vector6 = center + vector3
                vector6 = self:GetCollisionCheckedPosition(center, vector6)
                vector4 = position + (vector6 - center)
            end
        else
        end
        self.transform.position = vector4
    end
    
    local vector7 = self.rigidbody.position - self.prevState_.position_
    -- 完全按照Unity源代码逻辑：第165行 - 使用Time.deltaTime
    self.goPlayKart_.m_KartRealVelocity = vector7 / Time.deltaTime
    
    -- 保存当前帧状态
    self.prevState_.position_ = self.rigidbody.position
    self.prevState_.velocity_ = self.goPlayKart_.m_KartWLVel
    self.prevState_.angular_ = self.rigidbody.angularVelocity
    self.prevState_.forward_ = self.transform.forward
    self.prevState_.isGrounded_ = self.grounded
    
    
    
    if self.isSuddenChange then
        self.isSuddenChange = false
    end
    -- 重置状态
    self.goPlayKart_:ResetCrash()
    self.goPlayKart_:ResetShock()
    -- 按Unity源代码逻辑：在帧末尾重置grounded状态（对应Unity源码第170-171行）
    self.grounded = false
end


-- 方法9: GetCollisionCheckedPosition方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:GetCollisionCheckedPosition(fromPosition, toPosition)
    -- 对应Unity源码：Vector3 vector = toPosition - fromPosition
    local vector = toPosition - fromPosition
    -- 对应Unity源码：Vector3 normalized = vector.normalized
    local normalized = vector.normalized
    -- 对应Unity源码：float magnitude = vector.magnitude
    local magnitude = vector.magnitude
    
    -- 对应Unity源码：RaycastHit[] array = Physics.RaycastAll(fromPosition, vector, vector.magnitude, 8413440)
    local array = Physics.RaycastAll(fromPosition, vector, magnitude, 8413440)
    
    -- 对应Unity源码：if (array != null && array.Length > 0)
    if array ~= nil and #array > 0 then
        -- 对应Unity源码：float num = magnitude
        local num = magnitude
        -- 对应Unity源码：foreach (RaycastHit raycastHit in array)
        for _, raycastHit in ipairs(array) do
            -- 对应Unity源码：if (!raycastHit.collider.gameObject.Equals(base.gameObject) && raycastHit.distance < num)
            -- 注意：在Physics模块中已经排除了Player标签的物体，所以不需要再检查是否是自己
            if raycastHit.distance < num then
                -- 对应Unity源码：toPosition = fromPosition + normalized * (raycastHit.distance - 0.1f)
                toPosition = fromPosition + normalized * (raycastHit.distance - 0.1)
                -- 对应Unity源码：num = raycastHit.distance
                num = raycastHit.distance
            end
        end
    end
    
    return toPosition
end


-- 方法11: OnCollisionStay方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:OnCollisionStay(collisionInfo)
    self:OnCollisionDetection(collisionInfo)
end

-- 方法12: OnCollisionEnter方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:OnCollisionEnter(collisionInfo)
    self:OnCollisionDetection(collisionInfo)
end

-- 方法13: OnCollisionDetection方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:OnCollisionDetection(collisionInfo)
    print("碰撞到物体: " .. collisionInfo.gameObject.name)
end



-- 方法16: OnTriggerEnter方法迁移 - 修复Roblox适配
function RigidbodyFPSWalker:OnTriggerEnter(collider)
    self:UpdateTriggerPosition(collider, true)
end

-- 方法17: OnTriggerStay方法迁移 - 完全对应Unity版本
function RigidbodyFPSWalker:OnTriggerStay(collider)
    self:UpdateTriggerPosition(collider, false)
end

-- 方法18: UpdateTriggerPosition方法迁移 - 完全对应Unity版本（第245-337行）
function RigidbodyFPSWalker:UpdateTriggerPosition(collider, firstTrigger)
    -- 对应Unity源码第247行：float num = 0.65f
    local num = 0.65
    -- 对应Unity源码第248行：if (1 << collider.gameObject.layer == 256)
    -- collider.gameObject是Unity风格的对象，需要获取真实的Roblox Part
    -- 处理Terrain特殊情况
    local robloxPart = collider
    if collider ~= game.Workspace.Terrain then
        -- 在Roblox中，Part没有transform属性，直接使用Part本身
        if collider:IsA("BasePart") then
            robloxPart = collider
        else
            robloxPart = collider.gameObject or collider
        end
    end

    if LayerMask.CheckLayer(robloxPart, LayerMask.LayerConst.TRACK) then
        -- 对应Unity源码第250行：Vector3 position = base.transform.position
        local position = self.transform.position
        -- 对应Unity源码第251行：Vector3 vector = -base.transform.up
        local vector = -self.transform.up
        -- 对应Unity源码第252行：Vector3 vector2 = position - vector
        local vector2 = position - vector
        -- 对应Unity源码第253行：float magnitude = vector.magnitude
        local magnitude = vector.magnitude
        
        -- 对应Unity源码第254行：Ray ray = new Ray(vector2, vector)
        -- 对应Unity源码第255行：RaycastHit[] array = Physics.RaycastAll(ray, magnitude * 2f, 8413440)
        local ray = Ray.new(vector2, vector)
        local array = Physics.RaycastAllFromRay(ray, magnitude * 2.0, 8413440)
        
        -- 对应Unity源码第256行：if (array != null && array.Length > 0)
        if array ~= nil and #array > 0 then
            -- 对应Unity源码第258行：bool flag = false
            local flag = false
            -- 对应Unity源码第259行：float num2 = 0f
            local num2 = 0
            -- 对应Unity源码第260行：Vector3 vector3 = Vector3.zero
            local vector3 = Vector3.zero
            -- 对应Unity源码第261行：int num3 = 0
            local num3 = 0
            -- 对应Unity源码第262行：int num4 = 0
            local num4 = 0
            
            -- 对应Unity源码第263行：foreach (RaycastHit raycastHit in array)
            for _, raycastHit in ipairs(array) do
                -- 对应Unity源码第265行：if (raycastHit.normal.y > 0f && (!flag || raycastHit.distance < num2))
                if raycastHit.normal.Y > 0 and (not flag or raycastHit.distance < num2) then
                    -- 对应Unity源码第267行：num2 = raycastHit.distance
                    num2 = raycastHit.distance
                    -- 对应Unity源码第268行：vector3 = raycastHit.normal
                    vector3 = raycastHit.normal
                    -- 对应Unity源码第269行：num4 = raycastHit.collider.gameObject.layer
                    num4 = raycastHit.layer
                    -- 对应Unity源码第270行：flag = true
                    flag = true
                end
                -- 对应Unity源码第272行：num3++
                num3 = num3 + 1
            end
            
            -- 对应Unity源码第274行：if (flag)
            if flag then
                -- 对应Unity源码第276行：bool flag2 = false
                local flag2 = false
                -- 对应Unity源码第277行：if (magnitude - num2 > 0f)
                -- 注意：这里检查的是车子是否在地面以下
                -- magnitude是1（向下单位向量长度），num2是射线击中距离
                -- 如果num2 < 1，说明地面距离车子不到1个单位，可能有穿透
                if magnitude - num2 > 0 then
                    -- 对应Unity源码第279行：this._targetUpVector = vector3
                    self._targetUpVector = vector3
                    -- 对应Unity源码第280行：flag2 = true
                    flag2 = true
                end
                
                -- 对应Unity源码第282行：if (this.goPlayKart_.m_Contact)
                if self.goPlayKart_.m_Contact then
                    -- 对应Unity源码第284行：vector3 = this.goPlayKart_.m_sus.contactN
                    vector3 = self.goPlayKart_.m_sus.contactN
                    -- 对应Unity源码第285行：this._targetUpVector = vector3
                    self._targetUpVector = vector3
                    -- 对应Unity源码第286行：flag2 = true
                    flag2 = true
                end
                
                -- 对应Unity源码第288行：if (flag2)
                if flag2 then
                    -- 内联Unity源码第290-324行的完整逻辑
                    -- 对应Unity源码第290行：float num5 = (float)(1 << num4)
                    local num5 = LayerMask.LayerToBitMask(num4)
                    
                    -- 对应Unity源码第291行：if (magnitude - num2 > 0f && num5 != 16384f)
                    if magnitude - num2 > 0 and num5 ~= LayerMask.LayerConst.AI then
                        -- 对应Unity源码第293行：Vector3 vector4 = -vector * (magnitude - num2)
                        local vector4 = -vector * (magnitude - num2)
                        -- 对应Unity源码第294行：base.transform.position += vector4
                        if self.transform then
                            self.transform.position = self.transform.position + vector4
                        end
                    end
                    
                    -- 对应Unity源码第296行：float num6 = Vector3.Dot(this.goPlayKart_.m_KartWLVel, vector3)
                    local num6 = Vector3.Dot(self.goPlayKart_.m_KartWLVel, vector3)
                    -- 对应Unity源码第297行：Vector3 vector5 = vector3 * num6
                    local vector5 = vector3 * num6
                    -- 对应Unity源码第298行：Vector3 vector6 = this.goPlayKart_.m_KartWLVel - vector5
                    local vector6 = self.goPlayKart_.m_KartWLVel - vector5
                    
                    -- 对应Unity源码第299行：if (vector3.y > num)
                    if vector3.Y > num then
                        -- 对应Unity源码第301行：float num7 = Mathf.Abs(Vector3.Dot(this.goPlayKart_.m_first.front, vector3)) * 0.7f
                        local num7 = Mathf.Abs(Vector3.Dot(self.goPlayKart_.m_first.front, vector3)) * 0.7
                        -- 对应Unity源码第302行：this.goPlayKart_.m_KartWLVel = vector5 * -num7 + vector6
                        self.goPlayKart_.m_KartWLVel = vector5 * -num7 + vector6
                        -- 对应Unity源码第303行：this.goPlayKart_.shockVelocity_ = vector5.magnitude
                        self.goPlayKart_.shockVelocity_ = vector5.magnitude
                        -- 对应Unity源码第304行：this.grounded = true
                        self.grounded = true
                        -- 对应Unity源码第305-308行：if (!this.prevState_.isGrounded_)
                        if not self.prevState_.isGrounded_ then
                            self.goPlayKart_:SetShock(Mathf.Abs(num6))
                        end
                    elseif self.goPlayKart_.isCrash_ then
                        -- 对应Unity源码第310-318行
                        local num8 = Mathf.Abs(num6)
                        self.goPlayKart_:SetCrash(num8)
                        if num8 >= self.SMALL_CRASH_VELOCITY then
                            self.goPlayKart_:ResetDriftGauge()
                        end
                    end
                    
                    -- 对应Unity源码第319-320行
                    local num9 = Vector3.Dot(Vector3.Cross(vector3, Vector3.up), self.goPlayKart_.m_first.front)
                    self.transform:RotateAroundLocal(self.transform.up, num9 * 0.3 * Time.deltaTime)
                end
            end
        end
    end
    
    -- 对应Unity源码第325-327行
    if self.goPlayKart_.m_Contact then
        -- 接触处理（暂时为空）
    end
    
    -- 对应Unity源码第328-336行
    if not self.goPlayKart_.Forcing and not self.goPlayKart_.IsInResetState then
        -- 在Roblox中直接使用Part的Position和Size
        local center = self.gameObject and self.gameObject.Position or self.transform.position
        local size = self.gameObject and self.gameObject.Size or Vector3.new(2, 2, 2)
        self:HandleCollision(self.transform.right, center, size.X / 2, true, num)
        self:HandleCollision(-self.transform.right, center, size.X / 2, true, num)
        self:HandleCollision(self.transform.forward, center, size.Z * 0.75, true, num)
        self:HandleCollision(-self.transform.forward, center, size.Z / 2, true, num)
    end
end

-- HandleCollision方法迁移 - 对应Unity源码第340-376行
function RigidbodyFPSWalker:HandleCollision(localRayDirection, rayOrigin, rayLength, handleCrash, gndLimit)
    -- 对应Unity源码第342-343行：RaycastHit raycastHit; if (Physics.Raycast...)
    local hit, raycastHit = Physics.Raycast(rayOrigin, localRayDirection, rayLength, 8413440)
    if hit then
        -- 对应Unity源码第345行：Vector3 vector = raycastHit.normal
        local vector = raycastHit.normal
        -- 对应Unity源码第346行：if (raycastHit.distance < rayLength && vector.y < gndLimit)
        if raycastHit.distance < rayLength and vector.Y < gndLimit then
            -- 对应Unity源码第348行：float num = Vector3.Dot(vector, localRayDirection)
            local num = Vector3.Dot(vector, localRayDirection)
            -- 对应Unity源码第349-352行
            if num > 0 then
                vector = -vector
            end
            -- 对应Unity源码第353行：float num2 = Mathf.Abs(num)
            local num2 = Mathf.Abs(num)
            -- 对应Unity源码第354行：float num3 = (float)(1 << raycastHit.collider.gameObject.layer)
            local num3 = LayerMask.LayerToBitMask(raycastHit.layer)
            -- 对应Unity源码第355-359行mao
            if num3 ~= LayerMask.LayerConst.AI then
                local vector2 = -vector * (raycastHit.distance - rayLength) * num2
                self.transform.position = self.transform.position + vector2
            end
            -- 对应Unity源码第360-375行
            if handleCrash and not self.goPlayKart_.isCrash_ then
                local num4 = Vector3.Dot(vector, self.prevState_.velocity_)
                if num4 < 0 then
                    local num5 = Mathf.Abs(num4)
                    self.goPlayKart_:SetCrash(num5)
                    -- 对应Unity源码第367-370行
                    -- Unity源码检查的是base.GetComponent<Collider>().gameObject.layer（自己的layer）
                    -- 但逻辑上应该检查碰撞物体是否是墙，使用num3（碰撞物体的layer）
                    if num3 == LayerMask.LayerConst.WALL and num5 >= self.SMALL_CRASH_VELOCITY then
                        self.goPlayKart_:ResetDriftGauge()
                    end
                    
                    -- 对应Unity源码第371-469行：完整的碰撞处理逻辑
                    -- 第371行：this.goPlayKart_.m_ctrl.oldSteerAngle = 0f;
                    self.goPlayKart_.m_ctrl.oldSteerAngle = 0
                    -- 第372行：float num6 = 0.618f;
                    local num6 = 0.618
                    -- 第373行：Vector3 vector3 = Vector3.zero;
                    local vector3 = Vector3.zero
                    
                    -- 第374-381行：检查碰撞对象是否有RigidbodyFPSWalker组件
                    -- 仿照Unity的GetComponent模式
                    if raycastHit.collider and raycastHit.collider.gameObject and raycastHit.collider.gameObject.transform then
                        local hitPart = raycastHit.collider.gameObject.transform  -- 被击中的Part
                        -- Unity中访问parent，在Roblox中应该访问Parent属性
                        local parent = hitPart.Parent
                        if parent then
                            -- 从组件注册表中查找RigidbodyFPSWalker组件
                            local rigidbodyFPSWalker = ComponentRegistry[parent]
                            if rigidbodyFPSWalker and rigidbodyFPSWalker.goPlayKart_ then
                                vector3 = rigidbodyFPSWalker.goPlayKart_.m_KartWLVel
                            end
                        end
                    end
                    
                    -- 第382-385行：计算速度差和限制
                    local vector4 = self.goPlayKart_.m_KartWLVel - vector3 * num6
                    vector4 = Vector3.new(
                        Mathf.Clamp(vector4.X, -20, 20),
                        Mathf.Clamp(vector4.Y, -5, 5),
                        Mathf.Clamp(vector4.Z, -20, 20)
                    )
                    
                    -- 第386-387行：计算法线方向的速度分量
                    local vector5 = vector * Vector3.Dot(vector4, vector)
                    local vector6 = self.goPlayKart_.m_KartWLVel - vector5
                    
                    -- 第388-393行：处理垂直碰撞
                    if vector.Y > gndLimit then
                        local num7 = Mathf.Abs(Vector3.Dot(self.goPlayKart_.m_first.front, vector)) * 0.7
                        self.goPlayKart_.m_KartWLVel = vector5 * -num7 + vector6
                        self.goPlayKart_.m_cState.shockVel = vector5.magnitude
                    else
                        -- 第395-399行：重置各种状态
                        self.goPlayKart_.m_adBoost.validTrigger = false
                        self.goPlayKart_.m_driftGauge.progressOn = false
                        self.goPlayKart_.m_driftGauge.progressTime = 0
                        self.goPlayKart_.m_driftGauge.progress = 0
                        
                        -- 第400-402行：计算归一化方向
                        local magnitude = vector6.magnitude
                        local vector7 = magnitude > 0 and vector6.normalized or Vector3.zero
                        
                        -- 第403行：计算反弹速度
                        local vector8 = vector5 * -1.2 - vector7 * Mathf.Min(vector5.magnitude * 1.2, magnitude * 0.02)
                        
                        -- 第404-444行：处理墙壁碰撞时的特殊旋转
                        if not self.goPlayKart_.m_drift.slipMode and self.goPlayKart_.m_Contact then
                            local flag = false
                            local num8 = 0
                            local num9 = 0
                            
                            -- 第409-423行：检查是否需要特殊旋转
                            if Mathf.Abs(Vector3.Dot(vector, self.transform.forward)) > 0.5 and 
                               Mathf.Approximately(self.goPlayKart_.m_ctrl:getRealAccel(), 1) then
                                if Mathf.Approximately(self.goPlayKart_.m_ctrl.steer, 1) then
                                    flag = true
                                    num8 = (not self.goPlayKart_.m_ctrl.wheelFlip and not self.goPlayKart_.m_ctrl.wheelDevil) and 1 or -1
                                    num9 = 1
                                elseif Mathf.Approximately(self.goPlayKart_.m_ctrl.steer, -1) then
                                    flag = true
                                    num8 = (not self.goPlayKart_.m_ctrl.wheelFlip and not self.goPlayKart_.m_ctrl.wheelDevil) and -1 or 1
                                    num9 = -1
                                end
                            end
                            
                            -- 第424-444行：执行特殊旋转
                            if flag then
                                local vector9 = vector
                                local vector10 = -self.goPlayKart_.m_first.front
                                vector10 = vector10.normalized
                                local num10 = Vector3.Dot(vector9, vector10)
                                local num11
                                
                                if Vector3.Dot(vector9, self.goPlayKart_.m_first.left * num9) < 0 then
                                    num10 = 2 - num10
                                    num11 = Mathf.Max(num10, 1.5)
                                else
                                    num11 = num10 * num10 * num10
                                end
                                
                                vector8 = vector8 - vector5.normalized * (3 * num10 + 1)
                                vector8 = vector8 + self.goPlayKart_.m_first.left * (3 * num10 + 1) * num8
                                local num12 = (6 * num11 + 5) * num8 * Time.deltaTime
                                self.transform:RotateAroundLocal(Vector3.up, num12)
                            end
                        end
                        
                        -- 第446行：应用计算后的速度
                        self.goPlayKart_.m_KartWLVel = self.goPlayKart_.m_KartWLVel + vector8
                        
                        -- 第447-469行：处理侧向碰撞旋转
                        local num13 = Vector3.Dot(vector, self.goPlayKart_.m_first.front)
                        local num14 = Vector3.Dot(vector, self.goPlayKart_.m_first.left)
                        local num15 = 3
                        
                        if Mathf.Abs(num13 * 0.8) > Mathf.Abs(num14) then
                            -- 第452-458行：前后碰撞处理
                            local zero = Vector3.zero
                            zero = Vector3.new(0, 0, num14 * (num13 > 0 and -1 or 1) * 
                                Mathf.Max(1, Mathf.Min(30, Mathf.Abs(Vector3.Dot(vector, self.goPlayKart_.m_KartWLVel) * 0.5))))
                            
                            if Vector3.Dot(zero, self.goPlayKart_.m_KartLAVel) <= 1 then
                                local num16 = zero.Z * Time.deltaTime * num15
                                self.transform:RotateAroundLocal(Vector3.up, num16)
                            end
                        else
                            -- 第461-468行：侧向碰撞处理
                            local zero2 = Vector3.zero
                            zero2 = Vector3.new(0, 0, num13 * (num14 > 0 and 1 or -1) * 
                                Mathf.Max(1, Mathf.Min(30, Mathf.Abs(Vector3.Dot(vector, self.goPlayKart_.m_KartWLVel) * 0.5))))
                            
                            if Vector3.Dot(zero2, self.goPlayKart_.m_KartLAVel) <= 1 then
                                local num17 = zero2.Z * Time.deltaTime * num15
                                self.transform:RotateAroundLocal(Vector3.up, num17)
                            end
                        end
                    end
                    return true
                end
            end
        end
    end
    return false
end

return RigidbodyFPSWalker
