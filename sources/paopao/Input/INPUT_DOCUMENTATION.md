# Input 文件夹完整功能文档

## 概述

Input 文件夹包含了卡丁车游戏的完整跨平台输入系统，实现了统一的输入抽象层和多种输入方式的支持。该系统通过策略模式、状态机模式和观察者模式，为游戏提供了键盘、鼠标、触摸、加速度计等多种输入方式的统一管理，确保了跨平台的一致性体验和高性能的实时响应。

## 系统架构

### 核心设计原则
- **分层抽象**: 硬件输入到游戏逻辑的多层抽象
- **跨平台统一**: 不同平台输入方式的统一接口
- **状态机管理**: 精确的输入状态跟踪和转换
- **事件驱动**: 观察者模式的解耦事件处理
- **性能优先**: 高效的更新循环和内存管理

### 输入系统分层
- **硬件层**: 原始输入捕获（Unity输入API）
- **平台层**: 设备特定处理和规范化
- **抽象层**: 统一的输入状态管理
- **应用层**: 游戏特定的输入解释
- **界面层**: UI元素交互处理

## 核心输入管理系统

### 1. 输入状态管理 - InputStatus.cs

#### 精确状态跟踪
**功能概述**: 核心输入状态类，提供时间相关的输入状态跟踪和边缘检测

**状态跟踪架构**:
```csharp
public class InputStatus
{
    private bool pressed_;        // 当前按下状态
    private bool prevPressed_;    // 前一帧状态
    private int pressedAt_;       // 按下时间戳
    
    // 状态查询接口
    public bool Pressed()
    {
        return this.pressed_;
    }
    
    // 边缘检测 - 按钮刚被按下
    public bool Pushed()
    {
        return this.pressed_ && !this.prevPressed_;
    }
    
    // 边缘检测 - 按钮刚被释放
    public bool Released()
    {
        return !this.pressed_ && this.prevPressed_;
    }
    
    // 状态更新
    public void UpdateState(bool currentState)
    {
        this.prevPressed_ = this.pressed_;
        this.pressed_ = currentState;
        
        if (this.Pushed())
        {
            this.pressedAt_ = MonoBehaiourExConst.GetTick();
        }
    }
    
    // 获取按下持续时间
    public float GetPressDuration()
    {
        if (!this.pressed_) return 0f;
        
        int currentTick = MonoBehaiourExConst.GetTick();
        return (currentTick - this.pressedAt_) / 60f; // 转换为秒
    }
    
    // 长按检测
    public bool IsLongPress(float threshold = 1f)
    {
        return this.pressed_ && this.GetPressDuration() >= threshold;
    }
}
```

**高级状态分析**:
```csharp
public class AdvancedInputStatus : InputStatus
{
    private Queue<int> pressHistory_;
    private const int MAX_HISTORY = 10;
    
    public AdvancedInputStatus()
    {
        this.pressHistory_ = new Queue<int>();
    }
    
    public override void UpdateState(bool currentState)
    {
        base.UpdateState(currentState);
        
        if (this.Pushed())
        {
            this.AddPressToHistory();
        }
    }
    
    private void AddPressToHistory()
    {
        int currentTick = MonoBehaiourExConst.GetTick();
        this.pressHistory_.Enqueue(currentTick);
        
        if (this.pressHistory_.Count > MAX_HISTORY)
        {
            this.pressHistory_.Dequeue();
        }
    }
    
    // 检测快速连击
    public bool IsRapidPress(float timeWindow = 0.5f)
    {
        if (this.pressHistory_.Count < 2) return false;
        
        int windowTicks = (int)(timeWindow * 60f);
        int currentTick = MonoBehaiourExConst.GetTick();
        int validPresses = 0;
        
        foreach (int pressTick in this.pressHistory_)
        {
            if (currentTick - pressTick <= windowTicks)
            {
                validPresses++;
            }
        }
        
        return validPresses >= 3; // 3次或以上为快速连击
    }
}
```

### 2. 键盘状态机 - KeyState & KeyStateTransfer

#### 有限状态机实现
**功能概述**: 实现精确的按键状态转换逻辑

**状态定义和转换**:
```csharp
public enum KeyState
{
    NONE,     // 未按下状态
    PUSH,     // 刚按下（单帧）
    PRESS,    // 持续按下
    RELEASE   // 刚释放（单帧）
}

public static class KeyStateTransfer
{
    // 状态转换表
    private static readonly KeyState[,] transitionTable_ = new KeyState[4, 2]
    {
        //           按下      释放
        /*NONE*/   { KeyState.PUSH,    KeyState.NONE    },
        /*PUSH*/   { KeyState.PRESS,   KeyState.RELEASE },
        /*PRESS*/  { KeyState.PRESS,   KeyState.RELEASE },
        /*RELEASE*/{ KeyState.PUSH,    KeyState.NONE    }
    };
    
    public static KeyState GetNextState(KeyState currentState, bool isPressed)
    {
        int stateIndex = (int)currentState;
        int inputIndex = isPressed ? 0 : 1;
        
        return transitionTable_[stateIndex, inputIndex];
    }
    
    // 状态验证
    public static bool IsValidTransition(KeyState from, KeyState to, bool isPressed)
    {
        KeyState expectedState = GetNextState(from, isPressed);
        return expectedState == to;
    }
}
```

**状态机应用示例**:
```csharp
public class StatefulKeyInput
{
    private KeyState currentState_;
    private KeyCode keyCode_;
    
    public StatefulKeyInput(KeyCode key)
    {
        this.keyCode_ = key;
        this.currentState_ = KeyState.NONE;
    }
    
    public void Update()
    {
        bool isPressed = Input.GetKey(this.keyCode_);
        KeyState newState = KeyStateTransfer.GetNextState(this.currentState_, isPressed);
        
        if (newState != this.currentState_)
        {
            this.OnStateChanged(this.currentState_, newState);
            this.currentState_ = newState;
        }
    }
    
    private void OnStateChanged(KeyState oldState, KeyState newState)
    {
        switch (newState)
        {
            case KeyState.PUSH:
                Debug.Log($"Key {this.keyCode_} pushed");
                break;
                
            case KeyState.RELEASE:
                Debug.Log($"Key {this.keyCode_} released");
                break;
        }
    }
    
    // 状态查询方法
    public bool WasPushed() => this.currentState_ == KeyState.PUSH;
    public bool IsPressed() => this.currentState_ == KeyState.PRESS || this.currentState_ == KeyState.PUSH;
    public bool WasReleased() => this.currentState_ == KeyState.RELEASE;
}
```

### 3. 跨平台输入管理器 - InputManager.cs

#### 统一输入处理
**功能概述**: 管理多种输入设备的统一输入管理器

**多输入源整合**:
```csharp
public class InputManager : MonoBehaviour
{
    // 基础输入结构
    public struct basicInput
    {
        public KeyCode key;          // 键盘按键
        public InputStatus status;   // 状态跟踪
        public bool isAccel;         // 是否支持加速度计
        public Vector3 accelAxis;    // 加速度计轴向
        public float threshold;      // 触发阈值
    }
    
    [Header("输入配置")]
    public basicInput[] inputMappings_;
    public bool enableAccelerometer = true;
    public bool enableMultiTouch = true;
    public float deadZone = 0.1f;
    
    private Dictionary<InputType, InputStatus> inputStates_;
    private Vector3 lastAcceleration_;
    
    private void Start()
    {
        this.InitializeInputSystem();
    }
    
    private void InitializeInputSystem()
    {
        this.inputStates_ = new Dictionary<InputType, InputStatus>();
        
        // 初始化所有输入类型
        foreach (InputType inputType in Enum.GetValues(typeof(InputType)))
        {
            this.inputStates_[inputType] = new InputStatus();
        }
        
        // 启用加速度计
        if (this.enableAccelerometer)
        {
            Input.gyro.enabled = true;
        }
    }
}
```

**加速度计集成**:
```csharp
private void UpdateAccelerometerInput()
{
    if (!this.enableAccelerometer) return;
    
    Vector3 acceleration = Input.acceleration;
    
    // 应用死区过滤
    if (acceleration.magnitude < this.deadZone)
    {
        acceleration = Vector3.zero;
    }
    
    // 更新加速度计相关的输入状态
    foreach (basicInput input in this.inputMappings_)
    {
        if (input.isAccel)
        {
            float accelValue = Vector3.Dot(acceleration, input.accelAxis);
            bool isTriggered = Mathf.Abs(accelValue) > input.threshold;
            
            input.status.UpdateState(isTriggered);
        }
    }
    
    this.lastAcceleration_ = acceleration;
}

public Vector3 GetAcceleration()
{
    return this.lastAcceleration_;
}

public float GetSteeringInput()
{
    // 基于加速度计的转向输入
    float steeringValue = this.lastAcceleration_.x;
    
    // 应用死区和灵敏度调节
    if (Mathf.Abs(steeringValue) < this.deadZone)
    {
        steeringValue = 0f;
    }
    else
    {
        steeringValue = Mathf.Sign(steeringValue) * 
                       (Mathf.Abs(steeringValue) - this.deadZone) / 
                       (1f - this.deadZone);
    }
    
    return Mathf.Clamp(steeringValue, -1f, 1f);
}
```

### 4. 触摸控制系统 - TouchController.cs

#### 高级触摸处理
**功能概述**: 触摸设备的专用输入控制器，支持多点触摸和手势识别

**多点触摸管理**:
```csharp
public class TouchController : MonoBehaviour
{
    private const int MAX_TOUCHES = 10;
    private InputStatus[] touchStates_;
    private TouchRegion[] touchRegions_;
    
    [Header("触摸设置")]
    public bool analogSteering = true;
    public bool autoAcceleration = false;
    public float shakeSensitivity = 2f;
    public float shakeDetectionWindow = 0.5f;
    
    // 手势检测
    private Vector3[] shakeHistory_;
    private int shakeHistoryIndex_;
    private float lastShakeTime_;
    
    private void Start()
    {
        this.InitializeTouchSystem();
    }
    
    private void InitializeTouchSystem()
    {
        this.touchStates_ = new InputStatus[MAX_TOUCHES];
        for (int i = 0; i < MAX_TOUCHES; i++)
        {
            this.touchStates_[i] = new InputStatus();
        }
        
        // 初始化震动检测历史
        this.shakeHistory_ = new Vector3[30]; // 0.5秒历史（60FPS）
        this.shakeHistoryIndex_ = 0;
    }
    
    private void Update()
    {
        this.UpdateTouchInput();
        this.UpdateShakeDetection();
        this.ProcessTouchRegions();
    }
}
```

**触摸区域处理**:
```csharp
private void ProcessTouchRegions()
{
    for (int i = 0; i < Input.touchCount && i < MAX_TOUCHES; i++)
    {
        Touch touch = Input.GetTouch(i);
        
        if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
        {
            TouchRegion hitRegion = this.GetTouchRegionAt(touch.position);
            
            if (hitRegion != null)
            {
                this.ProcessTouchRegionInput(hitRegion, touch);
            }
        }
        
        // 更新触摸状态
        bool isTouching = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
        this.touchStates_[i].UpdateState(isTouching);
    }
}

private TouchRegion GetTouchRegionAt(Vector2 screenPosition)
{
    // 转换屏幕坐标到GUI坐标
    Vector2 guiPosition = this.ScreenToGUIPosition(screenPosition);
    
    foreach (TouchRegion region in this.touchRegions_)
    {
        if (region.IsActive && region.ContainsPoint(guiPosition))
        {
            return region;
        }
    }
    
    return null;
}

private void ProcessTouchRegionInput(TouchRegion region, Touch touch)
{
    switch (region.regionType)
    {
        case TouchRegionType.Steering:
            this.ProcessSteeringTouch(region, touch);
            break;
            
        case TouchRegionType.Acceleration:
            this.ProcessAccelerationTouch(region, touch);
            break;
            
        case TouchRegionType.Brake:
            this.ProcessBrakeTouch(region, touch);
            break;
            
        case TouchRegionType.Item:
            this.ProcessItemTouch(region, touch);
            break;
    }
}
```

**模拟转向实现**:
```csharp
private void ProcessSteeringTouch(TouchRegion region, Touch touch)
{
    if (this.analogSteering)
    {
        // 模拟转向 - 基于触摸位置
        Vector2 regionCenter = region.GetCenterPosition();
        Vector2 touchOffset = touch.position - regionCenter;
        float regionWidth = region.GetWidth();
        
        float steeringValue = (touchOffset.x / regionWidth) * 2f; // 归一化到[-1, 1]
        steeringValue = Mathf.Clamp(steeringValue, -1f, 1f);
        
        // 应用死区
        if (Mathf.Abs(steeringValue) < this.deadZone)
        {
            steeringValue = 0f;
        }
        
        // 更新转向输入
        this.SetSteeringInput(steeringValue);
    }
    else
    {
        // 数字转向 - 基于触摸区域
        Vector2 regionCenter = region.GetCenterPosition();
        
        if (touch.position.x < regionCenter.x)
        {
            this.SetInput(InputType.LEFT, true);
        }
        else
        {
            this.SetInput(InputType.RIGHT, true);
        }
    }
}
```

**设备震动检测**:
```csharp
private void UpdateShakeDetection()
{
    Vector3 acceleration = Input.acceleration;
    
    // 记录加速度历史
    this.shakeHistory_[this.shakeHistoryIndex_] = acceleration;
    this.shakeHistoryIndex_ = (this.shakeHistoryIndex_ + 1) % this.shakeHistory_.Length;
    
    // 计算震动强度
    float shakeIntensity = this.CalculateShakeIntensity();
    
    if (shakeIntensity > this.shakeSensitivity)
    {
        float currentTime = Time.time;
        
        // 检查震动间隔，避免过于频繁
        if (currentTime - this.lastShakeTime_ > this.shakeDetectionWindow)
        {
            this.OnDeviceShake(shakeIntensity);
            this.lastShakeTime_ = currentTime;
        }
    }
}

private float CalculateShakeIntensity()
{
    Vector3 averageAccel = Vector3.zero;
    
    // 计算平均加速度
    foreach (Vector3 accel in this.shakeHistory_)
    {
        averageAccel += accel;
    }
    averageAccel /= this.shakeHistory_.Length;
    
    // 计算偏差强度
    float totalDeviation = 0f;
    foreach (Vector3 accel in this.shakeHistory_)
    {
        totalDeviation += (accel - averageAccel).magnitude;
    }
    
    return totalDeviation / this.shakeHistory_.Length;
}

private void OnDeviceShake(float intensity)
{
    Debug.Log($"Device shake detected with intensity: {intensity}");
    
    // 触发震动事件
    ShakeInputMessage message = new ShakeInputMessage();
    message.Initialize(intensity);
    MonoBehaviourMessageFactory.SendMessage(0, message);
}
```

### 5. 鼠标输入管理 - MouseManager.cs

#### 精确鼠标处理
**功能概述**: 鼠标和指针设备的输入管理，支持优先级和多点处理

**优先级事件分发**:
```csharp
public class MouseManager : MonoBehaviour
{
    private List<MouseNotifier> registeredNotifiers_;
    private MouseNotifier currentAuthority_;
    private bool hasInputAuthority_;
    
    // 鼠标状态跟踪
    private Vector2 lastMousePosition_;
    private bool[] mouseButtonStates_;
    private MouseElem[] touchElements_;
    
    private void Start()
    {
        this.InitializeMouseSystem();
    }
    
    private void InitializeMouseSystem()
    {
        this.registeredNotifiers_ = new List<MouseNotifier>();
        this.mouseButtonStates_ = new bool[3]; // 左中右键
        this.touchElements_ = new MouseElem[MAX_TOUCHES];
        
        for (int i = 0; i < MAX_TOUCHES; i++)
        {
            this.touchElements_[i] = new MouseElem();
        }
    }
    
    public void RegisterNotifier(MouseNotifier notifier, int priority = 0)
    {
        if (!this.registeredNotifiers_.Contains(notifier))
        {
            this.registeredNotifiers_.Add(notifier);
            
            // 按优先级排序
            this.registeredNotifiers_.Sort((a, b) => b.GetPriority().CompareTo(a.GetPriority()));
        }
    }
}
```

**多点触摸模拟**:
```csharp
private void UpdateTouchSimulation()
{
    // 桌面环境下模拟多点触摸
    for (int i = 0; i < MAX_TOUCHES && i < Input.touchCount; i++)
    {
        Touch touch = Input.GetTouch(i);
        MouseElem element = this.touchElements_[i];
        
        element.fingerId = touch.fingerId;
        element.position = touch.position;
        element.deltaPosition = touch.deltaPosition;
        element.phase = this.ConvertTouchPhase(touch.phase);
        
        // 分发触摸事件
        this.DistributeTouchEvent(element);
    }
    
    // 处理鼠标作为主要触摸点
    if (Input.touchCount == 0)
    {
        this.ProcessMouseAsTouch();
    }
}

private void ProcessMouseAsTouch()
{
    Vector2 mousePosition = Input.mousePosition;
    bool mousePressed = Input.GetMouseButton(0);
    
    MouseElem mouseElement = this.touchElements_[0];
    mouseElement.position = mousePosition;
    mouseElement.deltaPosition = mousePosition - this.lastMousePosition_;
    
    // 确定鼠标触摸阶段
    if (Input.GetMouseButtonDown(0))
    {
        mouseElement.phase = MousePhase.Began;
    }
    else if (Input.GetMouseButtonUp(0))
    {
        mouseElement.phase = MousePhase.Ended;
    }
    else if (mousePressed)
    {
        mouseElement.phase = MousePhase.Moved;
    }
    else
    {
        mouseElement.phase = MousePhase.None;
    }
    
    if (mouseElement.phase != MousePhase.None)
    {
        this.DistributeTouchEvent(mouseElement);
    }
    
    this.lastMousePosition_ = mousePosition;
}
```

### 6. 输入控制抽象 - Control.cs

#### 统一控制接口
**功能概述**: 游戏控制的统一表示，支持输入修饰和实时计算

**控制值计算**:
```csharp
public struct Control
{
    public bool left;
    public bool right;
    public bool accel;
    public bool brake;
    public bool drift;
    public bool item;
    
    // 输入修饰标志
    public bool isDevilMode;    // 恶魔道具影响
    public bool isFlipMode;     // 翻转道具影响
    
    // 模拟输入值
    public float steeringAngle; // 转向角度 [-1, 1]
    public float acceleration;  // 加速度 [0, 1]
    
    // 计算最终转向值
    public float GetSteeringValue()
    {
        float steering = 0f;
        
        // 数字输入处理
        if (this.left && !this.right)
        {
            steering = -1f;
        }
        else if (this.right && !this.left)
        {
            steering = 1f;
        }
        
        // 模拟输入覆盖数字输入
        if (Mathf.Abs(this.steeringAngle) > 0.1f)
        {
            steering = this.steeringAngle;
        }
        
        // 应用道具效果修饰
        if (this.isDevilMode)
        {
            steering = -steering; // 恶魔道具反转控制
        }
        
        if (this.isFlipMode)
        {
            // 翻转道具随机化控制
            steering += Random.Range(-0.3f, 0.3f);
        }
        
        return Mathf.Clamp(steering, -1f, 1f);
    }
    
    // 计算加速度值
    public float GetAccelerationValue()
    {
        float accelValue = 0f;
        
        if (this.accel)
        {
            accelValue = 1f;
        }
        
        // 使用模拟加速度
        if (this.acceleration > 0.1f)
        {
            accelValue = this.acceleration;
        }
        
        // 刹车减少加速度
        if (this.brake)
        {
            accelValue *= 0.5f;
        }
        
        return Mathf.Clamp01(accelValue);
    }
    
    // 输入验证
    public bool IsValid()
    {
        return Mathf.Abs(this.steeringAngle) <= 1f &&
               this.acceleration >= 0f && this.acceleration <= 1f;
    }
}
```

## 性能优化和最佳实践

### 1. 高效更新循环
```csharp
public class OptimizedInputManager : MonoBehaviour
{
    private InputStatus[] inputArray_;
    private bool[] dirtyFlags_;
    private float lastUpdateTime_;
    private const float UPDATE_INTERVAL = 1f / 60f; // 60FPS
    
    private void Update()
    {
        float currentTime = Time.unscaledTime;
        
        // 固定频率更新，避免帧率依赖
        if (currentTime - this.lastUpdateTime_ < UPDATE_INTERVAL)
            return;
        
        this.UpdateInputStatesOptimized();
        this.lastUpdateTime_ = currentTime;
    }
    
    private void UpdateInputStatesOptimized()
    {
        // 批量处理输入更新
        for (int i = 0; i < this.inputArray_.Length; i++)
        {
            if (this.dirtyFlags_[i])
            {
                this.inputArray_[i].UpdateState(this.GetRawInputValue(i));
                this.dirtyFlags_[i] = false;
            }
        }
    }
}
```

### 2. 内存优化策略
```csharp
public static class InputOptimization
{
    // 对象池模式减少GC压力
    private static readonly ObjectPool<InputEvent> eventPool_ = new ObjectPool<InputEvent>();
    
    public static InputEvent GetInputEvent()
    {
        return eventPool_.Get() ?? new InputEvent();
    }
    
    public static void ReturnInputEvent(InputEvent inputEvent)
    {
        inputEvent.Reset();
        eventPool_.Return(inputEvent);
    }
    
    // 值类型缓存
    private static readonly Dictionary<int, Control> controlCache_ = new Dictionary<int, Control>();
    
    public static Control GetCachedControl(int hash)
    {
        controlCache_.TryGetValue(hash, out Control control);
        return control;
    }
}
```

## 总结

Input系统提供了一个完整、高效的跨平台输入框架，具有以下核心优势：

### 技术优势
1. **跨平台统一**: 键盘、鼠标、触摸、加速度计的统一抽象
2. **精确状态管理**: 有限状态机和时间戳的精确输入跟踪
3. **高性能处理**: 优化的更新循环和内存管理策略
4. **可扩展架构**: 易于添加新输入方式和修饰效果
5. **事件驱动**: 解耦的观察者模式事件处理

### 架构特点
- **分层设计**: 硬件抽象到游戏逻辑的清晰分层
- **状态机模式**: 精确的输入状态转换管理
- **策略模式**: 不同平台的输入策略实现
- **观察者模式**: 事件驱动的输入通知系统

该输入系统为卡丁车游戏提供了专业级的输入处理基础设施，支持复杂的跨平台输入需求，同时保持了出色的性能和响应性。