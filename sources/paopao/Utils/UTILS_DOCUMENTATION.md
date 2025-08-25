# Utils 模块详细功能文档

## 概述

Utils 模块包含了卡丁车游戏项目中的核心工具类和辅助函数，提供数组操作、数学计算、Unity 扩展、图形处理、游戏对象管理等功能。该模块为整个游戏系统提供基础设施支持。

## 模块结构

```
Utils/
├── ArrayEx.cs           - 扩展数组操作类
├── FiaUtil.cs          - 游戏核心工具函数集
├── MathHelper.cs       - 数学计算辅助类
├── MonoBehaviourEx.cs  - MonoBehaviour 扩展基类
├── NativeHelper.cs     - 原生平台交互助手
├── RectHelper.cs       - 矩形操作工具类
├── ShaderHelper.cs     - 着色器工具类
├── StuckHelper.cs      - 卡丁车卡住检测结构
└── Vector3Helper.cs    - 三维向量操作工具类
```

## 核心类详细分析

### 1. ArrayEx.cs - 扩展数组操作类

**功能概述：**
提供泛型数组的扩展功能，支持动态起始位置和长度管理。

**关键代码：**
```csharp
public class ArrayEx<T>
{
    public int begin_;          // 数组起始索引
    public int length_;         // 有效长度
    public T[] data_;          // 底层数据数组
    
    // 构造函数 - 创建指定大小的扩展数组
    public ArrayEx(int size)
    {
        this.begin_ = 0;
        this.length_ = size;
        this.data_ = new T[size];
    }
    
    // 索引器 - 支持偏移访问
    public T this[int i]
    {
        get { return this.data_[this.begin_ + i]; }
        set { this.data_[this.begin_ + i] = value; }
    }
    
    // 数组拷贝功能
    public void Copy(T[] data)
    {
        this.begin_ = 0;
        this.length_ = data.Length;
        if (this.data_ == null)
            this.data_ = new T[this.length_];
        Array.Copy(data, this.data_, this.length_);
    }
}
```

**应用场景：**
- 游戏中的动态数据缓冲区管理
- 循环队列实现
- 内存池优化

### 2. FiaUtil.cs - 游戏核心工具函数集

**功能概述：**
游戏系统的核心工具类，提供资源管理、文件操作、游戏对象生成、路径处理等综合功能。

**关键功能模块：**

#### 2.1 资源和文件管理
```csharp
// 跨平台文档路径获取
public static string docPath
{
    get
    {
        foreach (RuntimePlatform platform in desktopPlatforms)
        {
            if (Application.platform == platform)
                return Path.Combine(Path.GetDirectoryName(Application.dataPath), "Documents");
        }
        return Application.persistentDataPath;
    }
}

// 安全创建 StreamWriter
public static StreamWriter CreateStreamWriter(string path)
{
    string directoryName = Path.GetDirectoryName(path);
    if (!Directory.Exists(directoryName))
        Directory.CreateDirectory(directoryName);
    return new StreamWriter(path);
}
```

#### 2.2 游戏对象生成和管理
```csharp
// 生成卡丁车对象
public static GameObject GenerateKart(byte kartIdx)
{
    string mainAsset = KartAssetDefinitionManager.Instance.GetMainAsset((int)kartIdx);
    GameObject gameObjectMainAsset = ResourceLoader.Instance.GetGameObjectMainAsset(mainAsset, true);
    
    // 应用材质
    MeshRenderer[] renderers = gameObjectMainAsset.GetComponentsInChildren<MeshRenderer>();
    Material[] material = MaterialManager.Instance.GetMaterial(MaterialManager.MaterialPresetType.KART_TEXTURE);
    foreach (MeshRenderer renderer in renderers)
    {
        if (renderer.sharedMaterials.Length > 0 && renderer.sharedMaterials[0] == null)
            renderer.sharedMaterials = material;
    }
    return gameObjectMainAsset;
}

// 生成角色对象（包含动画）
public static GameObject GenerateCharacter(byte characterIdx)
{
    List<string> assets = new List<string>();
    CharacterAssetDefinitionManager.Instance.GetAssets((int)characterIdx, ref assets);
    
    // 分离动画和模型资源
    string animAsset = string.Empty;
    string modelAsset = string.Empty;
    foreach (string asset in assets)
    {
        if (asset.Contains("_ani"))
            animAsset = asset;
        else if (!asset.Contains("_noani"))
            modelAsset = asset;
    }
    
    // 实例化并配置骨骼绑定
    GameObject character = (GameObject)Object.Instantiate(ResourceLoader.Instance.GetMainAsset(animAsset));
    Transform[] bones = character.GetComponentsInChildren<Transform>();
    
    // 生成身体和面部骨骼
    Transform[] bodyBones, faceBones;
    GenerateBone(bones, modelAsset, "body_bonenames", out bodyBones);
    GenerateBone(bones, modelAsset, "face_bonenames", out faceBones);
    
    // 配置蒙皮网格渲染器
    string[] parts = { "body", "face" };
    Transform[][] boneArrays = { bodyBones, faceBones };
    
    for (int i = 0; i < parts.Length; i++)
    {
        GameObject part = ResourceLoader.Instance.GetGameObjectAsset(modelAsset, parts[i], true);
        if (part != null)
        {
            SkinnedMeshRenderer skinnedRenderer = part.GetComponent<SkinnedMeshRenderer>();
            if (skinnedRenderer != null)
            {
                skinnedRenderer.bones = boneArrays[i];
                skinnedRenderer.updateWhenOffscreen = true;
            }
            AttachChild(ref character, ref part);
        }
    }
    
    return character;
}
```

#### 2.3 数据校验和加密
```csharp
// 8位校验和计算
public static byte CalcChecksum8(byte[] data, uint dataLength)
{
    uint checksum = Checksum8Update(data, dataLength);
    checksum = (checksum >> 8) + (checksum & 255U);
    checksum += checksum >> 8;
    return (byte)(~(byte)checksum);
}

// 校验和更新算法
public static uint Checksum8Update(byte[] input, uint length)
{
    uint sum = 0U;
    uint offset = 0U;
    
    // 处理大块数据
    while (length >> 17 > 0U)
    {
        length -= 131072U;
        for (uint i = 65536U; i > 0U; i -= 1U)
        {
            sum += (uint)ReadUShort(input, offset);
            offset += 2U;
        }
        sum = (sum >> 16) + (sum & 65535U);
        sum = (uint)((ushort)(sum + (sum >> 16)));
    }
    
    // 处理剩余数据
    for (uint i = length >> 1; i > 0U; i -= 1U)
    {
        sum += (uint)ReadUShort(input, offset);
        offset += 2U;
    }
    
    // 处理奇数字节
    if ((length & 1U) != 0U)
        sum += (uint)input[offset];
    
    sum = (sum >> 16) + (sum & 65535U);
    sum += sum >> 16;
    return (uint)((ushort)sum);
}
```

#### 2.4 用户名处理和验证
```csharp
// 用户名生成和过滤
public static string GenerateUserName(string name)
{
    string result = string.Empty;
    string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:,(*!?')/;- ";
    
    for (int i = 0; i < name.Length; i++)
    {
        if (validChars.IndexOf(name[i]) < 0)
            result += '?';  // 替换无效字符
        else
            result += name[i];
    }
    
    // 长度限制
    if (result.Length > 10)
    {
        result = result.Substring(0, 9);
        result += "=";
    }
    return result;
}
```

### 3. MathHelper.cs - 数学计算辅助类

**功能概述：**
提供游戏中需要的高级数学运算，包括四元数操作、矩阵变换、射线检测、贝塞尔曲线等。

**关键功能：**

#### 3.1 四元数操作
```csharp
// 四元数标量乘法
public static void QuaMulScala(ref Quaternion q, float v)
{
    q.x *= v; q.y *= v; q.z *= v; q.w *= v;
}

// 四元数归一化
public static void QuaNormalize(ref Quaternion q)
{
    QuaMulScala(ref q, 1f / Mathf.Sqrt(q.w * q.w + q.x * q.x + q.y * q.y + q.z * q.z));
}

// 从方向向量创建四元数
public static Quaternion ToQuaternion(Vector3 left, Vector3 dir, Vector3 up)
{
    float[] quat = new float[4];
    float[,] matrix = new float[3, 3];
    
    // 构建旋转矩阵
    for (int i = 0; i < 3; i++)
    {
        matrix[i, 0] = left[i];
        matrix[i, 1] = up[i];
        matrix[i, 2] = dir[i];
    }
    
    float trace = matrix[0, 0] + matrix[1, 1] + matrix[2, 2];
    
    if (trace > 0f)
    {
        float s = Mathf.Sqrt(trace + 1f);
        quat[0] = 0.5f * s;
        s = 0.5f / s;
        quat[1] = (matrix[2, 1] - matrix[1, 2]) * s;
        quat[2] = (matrix[0, 2] - matrix[2, 0]) * s;
        quat[3] = (matrix[1, 0] - matrix[0, 1]) * s;
    }
    else
    {
        // 使用最大对角元素的算法
        int maxIndex = 0;
        if (matrix[1, 1] > matrix[0, 0]) maxIndex = 1;
        if (matrix[2, 2] > matrix[maxIndex, maxIndex]) maxIndex = 2;
        
        int next1 = next[maxIndex];
        int next2 = next[next1];
        
        float s = Mathf.Sqrt(matrix[maxIndex, maxIndex] - matrix[next1, next1] - matrix[next2, next2] + 1f);
        quat[maxIndex + 1] = 0.5f * s;
        s = 0.5f / s;
        quat[0] = (matrix[next2, next1] - matrix[next1, next2]) * s;
        quat[next1 + 1] = (matrix[next1, maxIndex] + matrix[maxIndex, next1]) * s;
        quat[next2 + 1] = (matrix[next2, maxIndex] + matrix[maxIndex, next2]) * s;
    }
    
    return new Quaternion(quat[1], quat[2], quat[3], quat[0]);
}
```

#### 3.2 射线-面相交检测
```csharp
// 射线与三角面相交检测（Möller-Trumbore算法）
public static bool RayFaceIntersect(Vector3[] vertices, Vector3 rayStart, Vector3 rayDir)
{
    Vector3 edge1 = vertices[1] - vertices[0];
    Vector3 edge2 = vertices[2] - vertices[0];
    Vector3 h = Vector3.Cross(rayDir, edge2);
    float a = Vector3.Dot(edge1, h);
    
    // 射线与三角面平行
    if (a > -0.0001f && a < 0.0001) return false;
    
    float f = 1f / a;
    Vector3 s = rayStart - vertices[0];
    float u = f * Vector3.Dot(s, h);
    
    if (u < 0f || u > 1f) return false;
    
    Vector3 q = Vector3.Cross(s, edge1);
    float v = f * Vector3.Dot(rayDir, q);
    
    if (v < 0f || u + v > 1f) return false;
    
    float t = f * Vector3.Dot(edge2, q);
    return t >= 0f && t <= 1f;
}
```

#### 3.3 贝塞尔曲线计算
```csharp
// 三次贝塞尔曲线插值
public static Vector3 BEZ3(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
{
    return a * (1f - t) * (1f - t) * (1f - t) + 
           b * 3f * (1f - t) * (1f - t) * t + 
           c * 3f * (1f - t) * t * t + 
           d * t * t * t;
}
```

### 4. MonoBehaviourEx.cs - MonoBehaviour 扩展基类

**功能概述：**
扩展Unity的MonoBehaviour，提供统一的消息系统和生命周期管理。

**关键代码：**
```csharp
public class MonoBehaviourEx : MonoBehaviour
{
    public int id_ = -1;  // 对象唯一标识
    
    // 注册到消息中心
    public virtual void RegistMonoBehaviour(int id)
    {
        this.id_ = id;
        MonoBehaviourExCenter.Instance.RegistMonoBehaviour(this.id_, this);
    }
    
    // 发送点对点消息
    public void SendMessage(int targetId, MonoBehaviourMessage msg)
    {
        if (this.id_ == -1) return;
        MonoBehaviourExCenter.Instance.SendMessage(this.id_, targetId, msg);
    }
    
    // 广播消息
    public void BroadcastMessage(MonoBehaviourMessage msg)
    {
        if (this.id_ == -1) return;
        MonoBehaviourExCenter.Instance.BroadcastMessage(this.id_, msg);
    }
    
    // 虚拟方法 - 子类重写实现具体逻辑
    public virtual void ReceiveMessage(int senderId, MonoBehaviourMessage msg) { }
    public virtual void OnLoadStage() { }
    public virtual void OnUnloadStage() { }
}
```

### 5. NativeHelper.cs - 原生平台交互助手

**功能概述：**
处理Android平台的原生交互，包括应用生命周期、本地化、输入处理等。

**关键功能：**

#### 5.1 平台生命周期管理
```csharp
public class NativeHelper : MonoBehaviour
{
    protected static NativeHelper _instance;
    private static string locale = "ko";
    private static string buildType_ = "SKT";
    
    // 应用启动处理
    public void OnStart(string msg)
    {
        BackgroundNotifier.DidBecomeActive();
    }
    
    // 应用后台处理
    public void OnStop(string msg)
    {
        BackgroundNotifier.DidEnterBackground();
    }
    
    // 设置当前语言环境
    public void currentLocale(string msg)
    {
        NativeHelper.locale = msg;
    }
}
```

#### 5.2 Android 构建类型检测
```csharp
public static string buildType
{
    get
    {
        string result;
        using (AndroidJavaClass nativeClass = new AndroidJavaClass("com.nexon.kartriderrush.android.core.natives"))
        {
            string buildTypeResult = nativeClass.CallStatic<string>("getBuildType", new object[0]);
            result = (buildTypeResult == null) ? buildType_ : buildTypeResult;
        }
        return result;
    }
}
```

#### 5.3 返回键处理系统
```csharp
public void OnKeyDown(string msg)
{
    if (KartManager.Instance?.parameter_ == null) return;
    
    GameObject activePopup = null;
    string[] popupNames;
    
    // 根据不同游戏阶段查找活动弹窗
    switch (KartManager.Instance.parameter_.Stage)
    {
        case StageType.MAIN:
            popupNames = new string[] { "accomplish_popup", "gui_loading_popup", "gui_patch_summary_popup", "gui_tutorial_multi" };
            break;
        case StageType.WAITROOM_HOST:
        case StageType.WAITROOM_CLIENT:
            popupNames = new string[] { "accomplish_popup", "gui_waiting_players_popup", "gui_mode" };
            break;
        case StageType.GAME:
            popupNames = new string[] { "pause", "quit_popup", "controls" };
            break;
        default:
            popupNames = new string[] { "accomplish_popup", "gui_loading_popup", "gui_fb_login_popup", "gui_ranking_reset_popup", "quest_popup", "purchase_confirm_popup", "restore_purchase_popup", "package_popup", "gui_mode" };
            break;
    }
    
    // 查找并处理活动弹窗
    foreach (string popupName in popupNames)
    {
        string path = GetPopupPath(popupName, KartManager.Instance.parameter_.Stage);
        activePopup = GameObject.Find(path);
        if (activePopup?.active == true) break;
    }
    
    if (activePopup?.active == true)
    {
        activePopup.SendMessage("BackButtonAction", msg);
    }
}
```

### 6. RectHelper.cs - 矩形操作工具类

**功能概述：**
提供矩形的创建、验证和尺寸计算功能。

**关键代码：**
```csharp
public class RectHelper
{
    public static Rect zero = new Rect(0f, 0f, 0f, 0f);
    
    // 获取矩形尺寸
    public static Vector2 GetSize(Rect rc)
    {
        return new Vector2(rc.width, rc.height);
    }
    
    // 验证矩形有效性
    public static bool IsValidRect(Rect rc)
    {
        return rc.width != 0f && rc.height != 0f;
    }
    
    // 从字符串数组创建矩形
    public static Rect CreateRect(string[] tokens, int startIdx)
    {
        return new Rect
        {
            xMin = (float)int.Parse(tokens[startIdx]),
            yMin = (float)int.Parse(tokens[startIdx + 1]),
            xMax = (float)int.Parse(tokens[startIdx + 2]),
            yMax = (float)int.Parse(tokens[startIdx + 3])
        };
    }
}
```

### 7. ShaderHelper.cs - 着色器工具类

**功能概述：**
提供预定义的着色器代码。

**关键代码：**
```csharp
public static class ShaderHelper
{
    // 无纹理着色器定义
    public static string noTextureShader_ = 
        "Shader \"NoTextueShader\" {\r\n" +
        "\tProperties {\r\n" +
        "\t}\r\n" +
        "\tSubShader {\r\n" +
        "\t\tPass {\r\n" +
        "            BindChannels {\r\n" +
        "               Bind \"Vertex\", vertex\r\n" +
        "               Bind \"Color\", color\r\n" +
        "            }\r\n" +
        "            Lighting Off\r\n" +
        "\t\t}\r\n" +
        "\t}\r\n" +
        "\tFallback off\r\n" +
        "}";
}
```

### 8. StuckHelper.cs - 卡丁车卡住检测结构

**功能概述：**
用于检测卡丁车是否卡住的辅助结构体。

**关键代码：**
```csharp
public struct StuckHelper
{
    public float wallStuckTime;  // 撞墙卡住时间
    public float gndStuckTime;   // 地面卡住时间  
    public float obstStuckTime;  // 障碍物卡住时间
    public bool inStuck;         // 是否处于卡住状态
    
    // 初始化所有状态
    public void Initialize()
    {
        this.wallStuckTime = 0f;
        this.obstStuckTime = 0f;
        this.inStuck = false;
    }
}
```

### 9. Vector3Helper.cs - 三维向量操作工具类

**功能概述：**
提供Vector3的扩展操作功能。

**关键代码：**
```csharp
public class Vector3Helper
{
    // 设置向量三个分量
    public static void SetVector3(ref Vector3 v, float x, float y, float z)
    {
        v.x = x; v.y = y; v.z = z;
    }
    
    // 设置向量为统一值
    public static void SetVector3(ref Vector3 v, float t)
    {
        SetVector3(ref v, t, t, t);
    }
    
    // 从字符串创建向量
    public static Vector3 CreateVector3(string x, string y, string z)
    {
        return new Vector3(float.Parse(x), float.Parse(y), float.Parse(z));
    }
    
    // 向量转字符串（格式化输出）
    public static string ToStringVector3(Vector3 v)
    {
        return string.Concat(new string[]
        {
            " [ ", v.x.ToString(), " , ", v.y.ToString(), " , ", v.z.ToString(), " ] "
        });
    }
    
    // 检查向量是否为零向量
    public static bool IsZero(Vector3 v)
    {
        return v.x == 0f && v.y == 0f && v.z == 0f;
    }
}
```

## 模块间依赖关系

1. **FiaUtil** 是核心工具类，被多个游戏系统调用
2. **MonoBehaviourEx** 为游戏对象提供统一的消息通信基础
3. **MathHelper** 为物理计算和图形渲染提供数学支持
4. **NativeHelper** 处理平台特定功能，主要服务于Android平台
5. **其他Helper类** 提供特定领域的工具函数

## 性能优化要点

1. **ArrayEx** 使用预分配数组减少GC压力
2. **FiaUtil** 中的校验和算法采用分块处理大数据
3. **MathHelper** 的四元数操作使用引用传递避免拷贝
4. **NativeHelper** 使用单例模式减少重复创建

## 安全性考虑

1. 路径操作中的目录存在性检查
2. 用户名过滤和长度限制
3. 数据校验和确保传输完整性
4. 原生调用的异常处理

## 扩展建议

1. 增加更多数学函数（如样条插值、噪声生成等）
2. 扩展文件操作的异步版本
3. 添加更多平台的原生支持
4. 优化大数据量的处理性能

该Utils模块为卡丁车游戏提供了完整的基础工具支持，涵盖了数据处理、图形计算、平台交互等核心功能，是整个游戏系统的重要基础设施。