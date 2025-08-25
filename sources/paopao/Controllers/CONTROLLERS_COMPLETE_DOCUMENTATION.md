# Controllers 文件夹完整功能文档

## 概述

Controllers 文件夹包含了卡丁车游戏的专用控制器组件系统，负责管理阴影投射、粒子发射、屏幕适配和触发事件等核心功能。该模块采用组件化设计模式，提供高性能的实时控制功能，支持跨平台运行和动态效果管理。

## 核心文件详细分析

### 1. BlobShadowController.cs - 动态软阴影控制器
**文件位置**: `/Controllers/BlobShadowController.cs`  
**功能概述**: 为游戏对象提供实时动态软阴影投射功能，支持固定偏移量的阴影跟踪系统

**关键代码分析**:
```csharp
public class BlobShadowController : MonoBehaviour
{
    private const float SHADOW_OFFSET = 8.246965f; // 固定阴影偏移量
    private Vector3 lastPosition;
    private bool hasPositionChanged = false;
    
    private void Start()
    {
        // 初始化阴影位置和缓存系统
        this.lastPosition = transform.position;
        this.InitializeShadowProjection();
    }
    
    private void Update()
    {
        // 性能优化：仅在位置变化时更新阴影
        if (this.HasPositionChanged())
        {
            this.UpdateShadowPosition();
            this.lastPosition = transform.position;
        }
    }
    
    private bool HasPositionChanged()
    {
        return Vector3.Distance(transform.position, this.lastPosition) > 0.01f;
    }
    
    private void UpdateShadowPosition()
    {
        // 计算阴影投射位置
        Vector3 shadowPosition = transform.position;
        shadowPosition.y -= SHADOW_OFFSET;
        
        // 应用阴影变换
        this.ApplyShadowTransform(shadowPosition);
    }
    
    private void ApplyShadowTransform(Vector3 position)
    {
        // 实时阴影投射算法
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 20f))
        {
            position.y = hit.point.y + 0.1f; // 稍微高于地面
            
            // 应用地面法线旋转
            Quaternion shadowRotation = Quaternion.LookRotation(Vector3.Cross(hit.normal, Vector3.right), hit.normal);
            this.ApplyToShadowRenderer(position, shadowRotation);
        }
    }
}
```

**性能优化特性**:
- **运动检测缓存**: 使用距离阈值避免不必要的计算
- **射线投射优化**: 限制射线距离和频率
- **批量变换处理**: 合并位置和旋转计算

**应用场景**:
- 卡丁车底盘阴影投射
- 道具物品阴影效果
- UI元素立体阴影
- 环境装饰阴影渲染

### 2. EmitController.cs - 高级粒子发射控制器
**文件位置**: `/Controllers/EmitController.cs`  
**功能概述**: 管理复杂的粒子发射系统，支持发射周期、径向分布和高级效果扩展

**关键代码分析**:
```csharp
public class EmitController : MonoBehaviour
{
    [Header("发射参数")]
    public int particleCount = 50;
    public float emissionRate = 10f;
    public AnimationCurve emissionCurve; // 发射强度曲线
    public Gradient colorGradient;        // 颜色渐变
    
    private ParticleSystem particleSystem;
    private float lastEmissionTime;
    private bool isEmitting = false;
    
    private void Start()
    {
        this.particleSystem = GetComponent<ParticleSystem>();
        this.InitializeParticleSystem();
    }
    
    private void Update()
    {
        if (this.isEmitting)
        {
            this.ProcessEmissionCycle();
        }
    }
    
    private void ProcessEmissionCycle()
    {
        float currentTime = Time.time;
        float deltaTime = currentTime - this.lastEmissionTime;
        
        if (deltaTime >= (1f / this.emissionRate))
        {
            this.EmitParticleBurst();
            this.lastEmissionTime = currentTime;
        }
    }
    
    private void EmitParticleBurst()
    {
        // 计算当前发射强度
        float lifeTime = this.particleSystem.main.startLifetime.constant;
        float normalizedTime = (Time.time % lifeTime) / lifeTime;
        float emissionIntensity = this.emissionCurve.Evaluate(normalizedTime);
        
        // 径向分布发射
        this.EmitRadialParticles(emissionIntensity);
    }
    
    private void EmitRadialParticles(float intensity)
    {
        int particlesToEmit = Mathf.RoundToInt(this.particleCount * intensity);
        
        for (int i = 0; i < particlesToEmit; i++)
        {
            // 计算径向分布位置
            float angle = (float)i / particlesToEmit * 360f;
            Vector3 direction = this.CalculateRadialDirection(angle);
            
            // 创建粒子发射参数
            var emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position + direction * 0.5f;
            emitParams.velocity = direction * this.GetVelocityForAngle(angle);
            emitParams.startColor = this.colorGradient.Evaluate(Random.value);
            
            this.particleSystem.Emit(emitParams, 1);
        }
    }
    
    private Vector3 CalculateRadialDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians));
    }
    
    private float GetVelocityForAngle(float angle)
    {
        // 基于角度的速度变化，创造螺旋效果
        return 5f + 3f * Mathf.Sin(angle * Mathf.Deg2Rad * 2f);
    }
}
```

**高级效果特性**:
- **发射周期管理**: 基于时间和曲线的动态发射控制
- **径向分布算法**: 数学计算的均匀粒子分布
- **颜色渐变系统**: 生命周期内的动态颜色变化
- **性能缓冲管理**: 智能的粒子池和批量发射

**扩展功能**:
- 支持自定义发射图案
- 动态粒子生命周期调整
- 多层粒子效果合成
- 物理碰撞粒子反弹

### 3. ScreenController.cs - 跨平台屏幕适配控制器
**文件位置**: `/Controllers/ScreenController.cs`  
**功能概述**: 处理不同平台的屏幕方向管理，支持iOS、Android和编辑器环境的统一适配

**关键代码分析**:
```csharp
public class ScreenController : MonoBehaviour
{
    [Header("平台配置")]
    public bool autoRotateToLandscape = true;
    public ScreenOrientation preferredOrientation = ScreenOrientation.LandscapeLeft;
    
    private bool isOrientationForced = false;
    private Matrix4x4 originalProjectionMatrix;
    
    private void Start()
    {
        this.originalProjectionMatrix = Camera.main.projectionMatrix;
        this.InitializePlatformSpecificSettings();
    }
    
    private void InitializePlatformSpecificSettings()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.IPhonePlayer:
                this.InitializeiOSSettings();
                break;
                
            case RuntimePlatform.Android:
                this.InitializeAndroidSettings();
                break;
                
            default:
                this.InitializeEditorSettings();
                break;
        }
    }
    
    private void InitializeiOSSettings()
    {
        // iOS特定的屏幕控制
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        
        Screen.orientation = this.preferredOrientation;
    }
    
    private void InitializeAndroidSettings()
    {
        // Android API级别检测
        int apiLevel = this.GetAndroidAPILevel();
        
        if (apiLevel >= 9) // Android 2.3+
        {
            this.SetAdvancedOrientationLock();
        }
        else
        {
            this.SetLegacyOrientationSettings();
        }
    }
    
    private int GetAndroidAPILevel()
    {
        try
        {
            using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            return 8; // 默认为较低API级别
        }
    }
    
    private void SetAdvancedOrientationLock()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // 强制横屏模式
                currentActivity.Call("setRequestedOrientation", 0); // SCREEN_ORIENTATION_LANDSCAPE
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to set Android orientation: {e.Message}");
            this.SetFallbackOrientation();
        }
    }
    
    private void Update()
    {
        if (this.autoRotateToLandscape)
        {
            this.MonitorOrientationChanges();
        }
    }
    
    private void MonitorOrientationChanges()
    {
        if (Screen.orientation != this.preferredOrientation && !this.isOrientationForced)
        {
            this.ForceOrientationCorrection();
        }
    }
    
    private void ForceOrientationCorrection()
    {
        this.isOrientationForced = true;
        
        // 强制相机旋转矫正
        this.ApplyCameraRotationCorrection();
        
        // 延迟重置强制标志
        StartCoroutine(this.ResetOrientationFlag());
    }
    
    private void ApplyCameraRotationCorrection()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 计算旋转矩阵
            Matrix4x4 rotationMatrix = this.CalculateOrientationMatrix();
            mainCamera.projectionMatrix = rotationMatrix * this.originalProjectionMatrix;
        }
    }
    
    private Matrix4x4 CalculateOrientationMatrix()
    {
        float rotationAngle = this.GetRotationAngleForCurrentOrientation();
        return Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotationAngle));
    }
    
    private float GetRotationAngleForCurrentOrientation()
    {
        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
                return 90f;
            case ScreenOrientation.PortraitUpsideDown:
                return -90f;
            case ScreenOrientation.LandscapeRight:
                return 180f;
            default:
                return 0f;
        }
    }
    
    private System.Collections.IEnumerator ResetOrientationFlag()
    {
        yield return new WaitForSeconds(1f);
        this.isOrientationForced = false;
    }
}
```

**平台适配特性**:
- **iOS方向控制**: 使用Screen.autorotate系列API
- **Android API检测**: 动态适配不同Android版本
- **强制旋转矫正**: 通过投影矩阵变换实现
- **编辑器模拟**: 开发环境的方向测试支持

**高级功能**:
- 平滑的方向过渡动画
- 自定义方向锁定策略
- 多分辨率适配支持
- 刘海屏和异形屏处理

### 4. TriggerEventController.cs - 事件触发代理控制器
**文件位置**: `/Controllers/TriggerEventController.cs`  
**功能概述**: 作为Unity物理触发器和游戏逻辑之间的桥梁，提供高级过滤和事件分发功能

**关键代码分析**:
```csharp
public class TriggerEventController : MonoBehaviour
{
    [Header("触发配置")]
    public string[] allowedTags = {"Player", "Item", "Kart"};
    public LayerMask triggerLayers = -1;
    public float cooldownTime = 0.5f;
    public bool enableDebugVisualization = true;
    
    [Header("事件配置")]
    public UnityEvent onTriggerEntered;
    public UnityEvent onTriggerExited;
    public UnityEvent<GameObject> onObjectEntered;
    
    private Dictionary<GameObject, float> lastTriggerTimes = new Dictionary<GameObject, float>();
    private HashSet<GameObject> currentlyTriggered = new HashSet<GameObject>();
    
    private void Start()
    {
        this.ValidateConfiguration();
        this.InitializeTriggerZone();
    }
    
    private void ValidateConfiguration()
    {
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"TriggerEventController on {gameObject.name} requires a Collider component");
        }
        
        if (!GetComponent<Collider>().isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} should be set as Trigger");
            GetComponent<Collider>().isTrigger = true;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (this.ShouldProcessTrigger(other.gameObject))
        {
            this.ProcessTriggerEnter(other.gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (this.currentlyTriggered.Contains(other.gameObject))
        {
            this.ProcessTriggerExit(other.gameObject);
        }
    }
    
    private bool ShouldProcessTrigger(GameObject triggerObject)
    {
        // 标签过滤
        if (!this.IsTagAllowed(triggerObject.tag))
        {
            return false;
        }
        
        // 图层过滤
        if (!this.IsLayerAllowed(triggerObject.layer))
        {
            return false;
        }
        
        // 冷却时间检查
        if (this.IsOnCooldown(triggerObject))
        {
            return false;
        }
        
        return true;
    }
    
    private bool IsTagAllowed(string tag)
    {
        return System.Array.IndexOf(this.allowedTags, tag) >= 0;
    }
    
    private bool IsLayerAllowed(int layer)
    {
        return (this.triggerLayers.value & (1 << layer)) != 0;
    }
    
    private bool IsOnCooldown(GameObject obj)
    {
        if (this.lastTriggerTimes.TryGetValue(obj, out float lastTime))
        {
            return Time.time - lastTime < this.cooldownTime;
        }
        return false;
    }
    
    private void ProcessTriggerEnter(GameObject triggerObject)
    {
        // 更新状态
        this.currentlyTriggered.Add(triggerObject);
        this.lastTriggerTimes[triggerObject] = Time.time;
        
        // 触发事件
        this.onTriggerEntered?.Invoke();
        this.onObjectEntered?.Invoke(triggerObject);
        
        // 发送消息到其他组件
        this.SendTriggerMessage(triggerObject, "OnTriggerZoneEntered");
        
        // 调试可视化
        if (this.enableDebugVisualization)
        {
            this.VisualizeTriggeredObject(triggerObject, Color.green);
        }
        
        Debug.Log($"Trigger entered: {triggerObject.name} at {Time.time}");
    }
    
    private void ProcessTriggerExit(GameObject triggerObject)
    {
        // 更新状态
        this.currentlyTriggered.Remove(triggerObject);
        
        // 触发事件
        this.onTriggerExited?.Invoke();
        
        // 发送消息
        this.SendTriggerMessage(triggerObject, "OnTriggerZoneExited");
        
        // 调试可视化
        if (this.enableDebugVisualization)
        {
            this.VisualizeTriggeredObject(triggerObject, Color.red);
        }
        
        Debug.Log($"Trigger exited: {triggerObject.name} at {Time.time}");
    }
    
    private void SendTriggerMessage(GameObject target, string methodName)
    {
        // 向目标对象发送消息
        target.SendMessage(methodName, this, SendMessageOptions.DontRequireReceiver);
        
        // 向相关控制器发送消息
        var controllers = target.GetComponents<MonoBehaviour>();
        foreach (var controller in controllers)
        {
            if (controller != this)
            {
                controller.SendMessage(methodName, this, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    private void VisualizeTriggeredObject(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            StartCoroutine(this.FlashObjectColor(renderer, color));
        }
    }
    
    private System.Collections.IEnumerator FlashObjectColor(Renderer renderer, Color flashColor)
    {
        Color originalColor = renderer.material.color;
        renderer.material.color = flashColor;
        
        yield return new WaitForSeconds(0.2f);
        
        renderer.material.color = originalColor;
    }
    
    // 公共API方法
    public void RegisterTriggerHandler(GameObject handler)
    {
        // 注册额外的触发处理器
        if (!this.registeredHandlers.Contains(handler))
        {
            this.registeredHandlers.Add(handler);
        }
    }
    
    public void UnregisterTriggerHandler(GameObject handler)
    {
        this.registeredHandlers.Remove(handler);
    }
    
    public bool IsObjectCurrentlyTriggered(GameObject obj)
    {
        return this.currentlyTriggered.Contains(obj);
    }
    
    public int GetCurrentTriggerCount()
    {
        return this.currentlyTriggered.Count;
    }
    
    private void OnDrawGizmos()
    {
        if (this.enableDebugVisualization)
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (collider is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
```

**高级过滤特性**:
- **多维度过滤**: 标签、图层和冷却时间的组合过滤
- **状态管理**: 精确的进入/退出状态跟踪
- **事件分发**: UnityEvent和SendMessage的双重通知机制
- **可视化调试**: 运行时和编辑器的调试支持

**扩展接口**:
- 动态处理器注册系统
- 触发状态查询API
- 自定义过滤条件扩展
- 性能监控和统计功能

## 系统集成架构

### 控制器间通信机制
```csharp
public class ControllerIntegrationSystem
{
    // 控制器消息总线
    private static Dictionary<Type, List<MonoBehaviour>> controllerRegistry = 
        new Dictionary<Type, List<MonoBehaviour>>();
    
    // 注册控制器
    public static void RegisterController<T>(T controller) where T : MonoBehaviour
    {
        Type controllerType = typeof(T);
        if (!controllerRegistry.ContainsKey(controllerType))
        {
            controllerRegistry[controllerType] = new List<MonoBehaviour>();
        }
        controllerRegistry[controllerType].Add(controller);
    }
    
    // 广播消息给所有相关控制器
    public static void BroadcastToControllers<T>(string message, object data = null) where T : MonoBehaviour
    {
        if (controllerRegistry.TryGetValue(typeof(T), out List<MonoBehaviour> controllers))
        {
            foreach (var controller in controllers)
            {
                controller.SendMessage(message, data, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
```

### 性能监控系统
```csharp
public class ControllerPerformanceMonitor
{
    private static Dictionary<string, PerformanceMetrics> metrics = 
        new Dictionary<string, PerformanceMetrics>();
    
    public static void RecordControllerPerformance(string controllerName, float executionTime)
    {
        if (!metrics.ContainsKey(controllerName))
        {
            metrics[controllerName] = new PerformanceMetrics();
        }
        
        metrics[controllerName].AddSample(executionTime);
    }
    
    public static PerformanceReport GenerateReport()
    {
        return new PerformanceReport(metrics);
    }
}
```

## 使用建议和最佳实践

### 1. BlobShadowController 最佳实践
- 使用运动检测缓存减少不必要的计算
- 调整SHADOW_OFFSET值以适配不同场景
- 考虑LOD系统在远距离时禁用阴影更新

### 2. EmitController 优化建议
- 合理设置粒子数量避免性能瓶颈
- 使用AnimationCurve优化发射模式
- 实现粒子池以减少GC压力

### 3. ScreenController 配置要点
- 测试不同设备和分辨率的适配效果
- 考虑刘海屏和异形屏的特殊处理
- 为VR/AR应用预留扩展接口

### 4. TriggerEventController 设计原则
- 合理设置冷却时间避免重复触发
- 使用图层和标签进行精确过滤
- 实现调试可视化辅助开发测试

这套控制器系统为卡丁车游戏提供了强大的基础功能支持，通过合理的架构设计和性能优化，确保了系统的稳定性和扩展性。