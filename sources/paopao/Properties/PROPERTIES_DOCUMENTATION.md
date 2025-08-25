# Properties 模块详细功能文档

## 概述

Properties 模块是卡丁车游戏项目中的程序集属性定义模块，包含了.NET程序集的元数据信息。该模块主要负责定义程序集的版本信息、编译器指令和其他程序集级别的属性，为整个项目提供标准的程序集标识和版本管理。

## 模块结构

```
Properties/
└── AssemblyInfo.cs - 程序集属性定义文件
```

## 系统架构图

```
Assembly Properties System (程序集属性系统)
└─ 程序集元数据层
   └─ AssemblyInfo.cs
      ├─ 版本信息 (AssemblyVersion)
      ├─ 文件版本 (AssemblyFileVersion)
      ├─ 产品信息 (AssemblyProduct)
      ├─ 公司信息 (AssemblyCompany)
      ├─ 版权信息 (AssemblyCopyright)
      ├─ 描述信息 (AssemblyDescription)
      └─ 编译器指令 (CompilerGenerated等)

编译时处理:
源代码 → 编译器 → AssemblyInfo属性 → 最终程序集
```

## 核心文件详细分析

### AssemblyInfo.cs - 程序集属性定义文件

**功能概述：**
AssemblyInfo.cs是.NET项目的标准程序集信息文件，定义了程序集的版本、元数据和编译器指令。在该游戏项目中，它提供了最基础的版本控制信息。

**当前实现：**
```csharp
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyVersion("0.0.0.0")]
```

**实现分析：**
1. **命名空间引用**: 引入必要的反射和编译器服务命名空间
2. **版本定义**: 设置程序集版本为"0.0.0.0"
3. **最小配置**: 仅包含最基本的版本信息

#### 完整的AssemblyInfo.cs扩展示例

**标准游戏项目的AssemblyInfo.cs：**
```csharp
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 程序集的常规信息
[assembly: AssemblyTitle("Kart Game Assembly")]
[assembly: AssemblyDescription("Kart Racing Game Core Assembly")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Game Studio")]
[assembly: AssemblyProduct("Kart Game")]
[assembly: AssemblyCopyright("Copyright © Game Studio 2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 程序集版本信息
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-beta")]

// COM互操作设置
[assembly: ComVisible(false)]

// 程序集的GUID (如果此项目向COM公开)
[assembly: Guid("12345678-1234-1234-1234-123456789012")]

// 调试和优化设置
#if DEBUG
[assembly: AssemblyMetadata("BuildType", "Debug")]
#else
[assembly: AssemblyMetadata("BuildType", "Release")]
#endif

// 安全和权限设置
[assembly: System.Security.AllowPartiallyTrustedCallers]

// Unity特定属性
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
[assembly: AssemblyMetadata("Platform", "Unity")]
[assembly: AssemblyMetadata("UnityVersion", "2022.3.0f1")]
#endif

// 游戏特定元数据
[assembly: AssemblyMetadata("GameName", "Kart Racer")]
[assembly: AssemblyMetadata("GameGenre", "Racing")]
[assembly: AssemblyMetadata("TargetRating", "E")]

// 构建信息
[assembly: AssemblyMetadata("BuildDate", "2024-01-15")]
[assembly: AssemblyMetadata("BuildMachine", "BUILD-SERVER-01")]

// 内部可见性 (用于测试程序集)
[assembly: InternalsVisibleTo("Assembly-CSharp-Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Moq测试框架
```

## 属性详细说明

### 1. 基础程序集属性

#### 1.1 标题和描述
```csharp
[assembly: AssemblyTitle("Kart Game Assembly")]
[assembly: AssemblyDescription("Kart Racing Game Core Assembly")]
```
- **AssemblyTitle**: 程序集的友好名称
- **AssemblyDescription**: 程序集功能的详细描述

#### 1.2 公司和产品信息
```csharp
[assembly: AssemblyCompany("Game Studio")]
[assembly: AssemblyProduct("Kart Game")]
[assembly: AssemblyCopyright("Copyright © Game Studio 2024")]
[assembly: AssemblyTrademark("")]
```
- **AssemblyCompany**: 开发公司名称
- **AssemblyProduct**: 产品名称
- **AssemblyCopyright**: 版权信息
- **AssemblyTrademark**: 商标信息

### 2. 版本控制属性

#### 2.1 版本号系统
```csharp
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0-beta")]
```

**版本号格式说明：**
- **Major.Minor.Build.Revision** (1.0.0.0)
- **Major**: 主版本号（重大更新）
- **Minor**: 次版本号（功能更新）
- **Build**: 构建号（bug修复）
- **Revision**: 修订号（热修复）

#### 2.2 版本控制策略
```csharp
// 自动版本生成
[assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// 语义化版本控制
[assembly: AssemblyInformationalVersion("1.2.3-alpha.1+build.456")]
```

### 3. 平台和环境属性

#### 3.1 条件编译属性
```csharp
#if UNITY_EDITOR
[assembly: AssemblyMetadata("Environment", "Editor")]
#elif UNITY_STANDALONE
[assembly: AssemblyMetadata("Environment", "Standalone")]
#elif UNITY_ANDROID
[assembly: AssemblyMetadata("Environment", "Android")]
#elif UNITY_IOS
[assembly: AssemblyMetadata("Environment", "iOS")]
#endif
```

#### 3.2 平台特定配置
```csharp
#if UNITY_ANDROID
[assembly: AssemblyMetadata("AndroidAPILevel", "28")]
[assembly: AssemblyMetadata("AndroidTargetDevice", "Phone")]
#endif

#if UNITY_IOS
[assembly: AssemblyMetadata("iOSDeploymentTarget", "12.0")]
[assembly: AssemblyMetadata("iOSTargetDevice", "iPhone")]
#endif
```

## 高级应用示例

### 1. 动态版本管理系统

**版本信息获取工具：**
```csharp
using System;
using System.Reflection;

public static class VersionHelper
{
    private static Assembly currentAssembly = Assembly.GetExecutingAssembly();
    
    public static string GetAssemblyVersion()
    {
        return currentAssembly.GetName().Version.ToString();
    }
    
    public static string GetFileVersion()
    {
        var fileVersionAttr = currentAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
        return fileVersionAttr?.Version ?? "Unknown";
    }
    
    public static string GetInformationalVersion()
    {
        var infoVersionAttr = currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return infoVersionAttr?.InformationalVersion ?? GetAssemblyVersion();
    }
    
    public static string GetTitle()
    {
        var titleAttr = currentAssembly.GetCustomAttribute<AssemblyTitleAttribute>();
        return titleAttr?.Title ?? "Unknown";
    }
    
    public static string GetDescription()
    {
        var descAttr = currentAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
        return descAttr?.Description ?? "No description";
    }
    
    public static string GetCompany()
    {
        var companyAttr = currentAssembly.GetCustomAttribute<AssemblyCompanyAttribute>();
        return companyAttr?.Company ?? "Unknown";
    }
    
    public static string GetProduct()
    {
        var productAttr = currentAssembly.GetCustomAttribute<AssemblyProductAttribute>();
        return productAttr?.Product ?? "Unknown";
    }
    
    public static string GetCopyright()
    {
        var copyrightAttr = currentAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
        return copyrightAttr?.Copyright ?? "No copyright";
    }
    
    public static string GetBuildConfiguration()
    {
        var metadataAttrs = currentAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        foreach (var attr in metadataAttrs)
        {
            if (attr.Key == "BuildType")
            {
                return attr.Value;
            }
        }
        return "Unknown";
    }
    
    public static DateTime? GetBuildDate()
    {
        var metadataAttrs = currentAssembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        foreach (var attr in metadataAttrs)
        {
            if (attr.Key == "BuildDate")
            {
                if (DateTime.TryParse(attr.Value, out DateTime buildDate))
                {
                    return buildDate;
                }
            }
        }
        return null;
    }
    
    public static string GetFullVersionInfo()
    {
        return $"{GetTitle()} v{GetInformationalVersion()} " +
               $"({GetBuildConfiguration()}) - {GetCompany()}";
    }
}
```

### 2. 游戏版本显示系统

**版本信息UI组件：**
```csharp
using UnityEngine;
using UnityEngine.UI;

public class VersionDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public Text versionText;
    public Text buildInfoText;
    public Text copyrightText;
    
    [Header("Display Settings")]
    public bool showInProduction = false;
    public bool showBuildInfo = true;
    public string versionFormat = "v{0}";
    
    private void Start()
    {
        UpdateVersionDisplay();
        
        // 在生产环境中隐藏版本信息
        if (!showInProduction && !Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
        }
    }
    
    private void UpdateVersionDisplay()
    {
        if (versionText != null)
        {
            string version = VersionHelper.GetInformationalVersion();
            versionText.text = string.Format(versionFormat, version);
        }
        
        if (buildInfoText != null && showBuildInfo)
        {
            string buildInfo = $"{VersionHelper.GetBuildConfiguration()}";
            DateTime? buildDate = VersionHelper.GetBuildDate();
            if (buildDate.HasValue)
            {
                buildInfo += $" - {buildDate.Value:yyyy-MM-dd}";
            }
            buildInfoText.text = buildInfo;
        }
        
        if (copyrightText != null)
        {
            copyrightText.text = VersionHelper.GetCopyright();
        }
    }
    
    [ContextMenu("Refresh Version Info")]
    private void RefreshVersionInfo()
    {
        UpdateVersionDisplay();
    }
}
```

### 3. 自动构建版本管理

**构建脚本中的版本更新：**
```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public static class BuildVersionManager
{
    private const string ASSEMBLY_INFO_PATH = "Assets/Scripts/Properties/AssemblyInfo.cs";
    
    [MenuItem("Build/Increment Version/Major")]
    public static void IncrementMajorVersion()
    {
        IncrementVersion(VersionComponent.Major);
    }
    
    [MenuItem("Build/Increment Version/Minor")]
    public static void IncrementMinorVersion()
    {
        IncrementVersion(VersionComponent.Minor);
    }
    
    [MenuItem("Build/Increment Version/Build")]
    public static void IncrementBuildVersion()
    {
        IncrementVersion(VersionComponent.Build);
    }
    
    [MenuItem("Build/Set Version Info")]
    public static void SetVersionInfo()
    {
        VersionInfoWindow.ShowWindow();
    }
    
    public enum VersionComponent
    {
        Major,
        Minor,
        Build,
        Revision
    }
    
    private static void IncrementVersion(VersionComponent component)
    {
        if (!File.Exists(ASSEMBLY_INFO_PATH))
        {
            Debug.LogError($"AssemblyInfo.cs not found at {ASSEMBLY_INFO_PATH}");
            return;
        }
        
        string content = File.ReadAllText(ASSEMBLY_INFO_PATH);
        string pattern = @"\[assembly: AssemblyVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)""\)\]";
        
        Match match = Regex.Match(content, pattern);
        if (match.Success)
        {
            int major = int.Parse(match.Groups[1].Value);
            int minor = int.Parse(match.Groups[2].Value);
            int build = int.Parse(match.Groups[3].Value);
            int revision = int.Parse(match.Groups[4].Value);
            
            switch (component)
            {
                case VersionComponent.Major:
                    major++;
                    minor = 0;
                    build = 0;
                    revision = 0;
                    break;
                case VersionComponent.Minor:
                    minor++;
                    build = 0;
                    revision = 0;
                    break;
                case VersionComponent.Build:
                    build++;
                    revision = 0;
                    break;
                case VersionComponent.Revision:
                    revision++;
                    break;
            }
            
            string newVersion = $"{major}.{minor}.{build}.{revision}";
            string newContent = Regex.Replace(content, pattern, 
                $"[assembly: AssemblyVersion(\"{newVersion}\")]");
            
            // 同时更新 AssemblyFileVersion
            string fileVersionPattern = @"\[assembly: AssemblyFileVersion\(""(\d+)\.(\d+)\.(\d+)\.(\d+)""\)\]";
            newContent = Regex.Replace(newContent, fileVersionPattern,
                $"[assembly: AssemblyFileVersion(\"{newVersion}\")]");
            
            File.WriteAllText(ASSEMBLY_INFO_PATH, newContent);
            AssetDatabase.Refresh();
            
            Debug.Log($"Version updated to {newVersion}");
        }
        else
        {
            Debug.LogError("Could not find AssemblyVersion attribute in AssemblyInfo.cs");
        }
    }
    
    public static void UpdateBuildDate()
    {
        if (!File.Exists(ASSEMBLY_INFO_PATH))
        {
            Debug.LogError($"AssemblyInfo.cs not found at {ASSEMBLY_INFO_PATH}");
            return;
        }
        
        string content = File.ReadAllText(ASSEMBLY_INFO_PATH);
        string buildDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        string pattern = @"\[assembly: AssemblyMetadata\(""BuildDate"", "".*?""\)\]";
        string replacement = $"[assembly: AssemblyMetadata(\"BuildDate\", \"{buildDate}\")]";
        
        if (Regex.IsMatch(content, pattern))
        {
            content = Regex.Replace(content, pattern, replacement);
        }
        else
        {
            // 如果不存在，添加新的属性
            content += $"\n[assembly: AssemblyMetadata(\"BuildDate\", \"{buildDate}\")]";
        }
        
        File.WriteAllText(ASSEMBLY_INFO_PATH, content);
        AssetDatabase.Refresh();
        
        Debug.Log($"Build date updated to {buildDate}");
    }
}

// 版本信息编辑窗口
public class VersionInfoWindow : EditorWindow
{
    private string title = "";
    private string description = "";
    private string company = "";
    private string product = "";
    private string copyright = "";
    private string version = "";
    
    public static void ShowWindow()
    {
        GetWindow<VersionInfoWindow>("Version Info");
    }
    
    private void OnEnable()
    {
        LoadCurrentValues();
    }
    
    private void LoadCurrentValues()
    {
        title = VersionHelper.GetTitle();
        description = VersionHelper.GetDescription();
        company = VersionHelper.GetCompany();
        product = VersionHelper.GetProduct();
        copyright = VersionHelper.GetCopyright();
        version = VersionHelper.GetAssemblyVersion();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Assembly Information", EditorStyles.boldLabel);
        
        title = EditorGUILayout.TextField("Title", title);
        description = EditorGUILayout.TextField("Description", description);
        company = EditorGUILayout.TextField("Company", company);
        product = EditorGUILayout.TextField("Product", product);
        copyright = EditorGUILayout.TextField("Copyright", copyright);
        version = EditorGUILayout.TextField("Version", version);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Update AssemblyInfo"))
        {
            UpdateAssemblyInfo();
        }
        
        if (GUILayout.Button("Reset to Current"))
        {
            LoadCurrentValues();
        }
    }
    
    private void UpdateAssemblyInfo()
    {
        // 实现更新AssemblyInfo.cs文件的逻辑
        Debug.Log("AssemblyInfo updated");
        Close();
    }
}
#endif
```

### 4. 版本兼容性检查

**版本兼容性管理器：**
```csharp
public static class VersionCompatibility
{
    private const string MIN_SUPPORTED_VERSION = "1.0.0.0";
    private const string CURRENT_VERSION_KEY = "LastKnownVersion";
    
    public static bool CheckCompatibility()
    {
        string currentVersion = VersionHelper.GetAssemblyVersion();
        string lastKnownVersion = PlayerPrefs.GetString(CURRENT_VERSION_KEY, "0.0.0.0");
        
        // 检查是否需要数据迁移
        if (CompareVersions(lastKnownVersion, currentVersion) < 0)
        {
            Debug.Log($"Version upgrade detected: {lastKnownVersion} -> {currentVersion}");
            return PerformVersionUpgrade(lastKnownVersion, currentVersion);
        }
        
        return true;
    }
    
    public static void SaveCurrentVersion()
    {
        string currentVersion = VersionHelper.GetAssemblyVersion();
        PlayerPrefs.SetString(CURRENT_VERSION_KEY, currentVersion);
        PlayerPrefs.Save();
    }
    
    private static int CompareVersions(string version1, string version2)
    {
        Version v1 = new Version(version1);
        Version v2 = new Version(version2);
        return v1.CompareTo(v2);
    }
    
    private static bool PerformVersionUpgrade(string fromVersion, string toVersion)
    {
        try
        {
            Debug.Log($"Performing version upgrade from {fromVersion} to {toVersion}");
            
            // 执行版本升级逻辑
            if (CompareVersions(fromVersion, "1.1.0.0") < 0)
            {
                // 升级到 1.1.0.0 的逻辑
                UpgradeTo110();
            }
            
            if (CompareVersions(fromVersion, "1.2.0.0") < 0)
            {
                // 升级到 1.2.0.0 的逻辑
                UpgradeTo120();
            }
            
            SaveCurrentVersion();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Version upgrade failed: {ex.Message}");
            return false;
        }
    }
    
    private static void UpgradeTo110()
    {
        Debug.Log("Upgrading to version 1.1.0.0");
        // 实现具体的升级逻辑
    }
    
    private static void UpgradeTo120()
    {
        Debug.Log("Upgrading to version 1.2.0.0");
        // 实现具体的升级逻辑
    }
}
```

## 最佳实践建议

### 1. 版本号管理策略

**语义化版本控制：**
```csharp
// 推荐的版本号格式
[assembly: AssemblyVersion("1.2.3.0")]           // API兼容性版本
[assembly: AssemblyFileVersion("1.2.3.456")]     // 构建版本
[assembly: AssemblyInformationalVersion("1.2.3-beta.1+build.456")] // 完整版本信息
```

### 2. 条件编译配置

**多平台配置：**
```csharp
#if UNITY_EDITOR
[assembly: AssemblyMetadata("BuildEnvironment", "Editor")]
#elif DEVELOPMENT_BUILD
[assembly: AssemblyMetadata("BuildEnvironment", "Development")]
#else
[assembly: AssemblyMetadata("BuildEnvironment", "Production")]
#endif
```

### 3. 自动化构建集成

**CI/CD集成示例：**
```csharp
// 通过构建参数设置版本
#if BUILD_NUMBER
[assembly: AssemblyMetadata("BuildNumber", BUILD_NUMBER)]
#endif

#if GIT_COMMIT
[assembly: AssemblyMetadata("GitCommit", GIT_COMMIT)]
#endif
```

## 总结

Properties模块虽然简单，但为整个卡丁车游戏项目提供了重要的元数据管理功能：

### 核心价值
1. **版本控制**: 提供标准的程序集版本管理
2. **元数据存储**: 集中存储项目相关信息
3. **构建管理**: 支持自动化构建和版本更新
4. **兼容性保证**: 确保不同版本间的兼容性

### 扩展建议
1. **完善版本信息**: 添加完整的程序集属性
2. **自动化版本**: 集成构建脚本自动更新版本
3. **版本检查**: 实现运行时版本兼容性检查
4. **元数据利用**: 在游戏中显示版本和构建信息

该模块为项目的版本管理和发布流程提供了基础支撑，是软件工程实践中的重要组成部分。