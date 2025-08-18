# Store 文件夹完整功能文档

## 概述

Store 文件夹包含了卡丁车游戏的完整内购(IAP)和商店管理系统，采用了单例模式、模板方法模式、策略模式和观察者模式的复合架构设计。该系统通过平台抽象、加密存储、异步操作和完善的错误处理，为游戏提供了安全、稳定、跨平台的商业化功能。

## 系统架构

### 核心设计原则
- **单例模式**: 全局统一的商店实例管理
- **模板方法模式**: 标准化的购买流程框架
- **策略模式**: 真实商店与模拟商店的灵活切换
- **观察者模式**: 基于回调的异步操作通知
- **安全优先**: AES加密和设备绑定的数据保护

### 系统组件分层
- **抽象层**: 统一的商店接口和状态管理
- **实现层**: 真实商店和模拟商店的具体实现
- **安全层**: 加密存储和数据完整性验证
- **集成层**: 平台原生接口和UI组件集成

## 核心架构系统

### 1. 主商店系统 - FiaStore.cs

#### 单例商店管理器
**功能概述**: 核心商店实现，提供完整的内购功能和状态管理

**单例模式实现**:
```csharp
public class FiaStore : IStartable
{
    private static FiaStore store_;
    
    // 单例访问点
    public static FiaStore Inst
    {
        get
        {
            if (FiaStore.store_ == null)
            {
                FiaStore.store_ = new FiaStore();
            }
            return FiaStore.store_;
        }
    }
    
    // 核心状态属性
    public StartableState State { get; set; }
    public Exception Error { get; set; }
    public List<Product> ProductList { get; private set; }
    public List<string> PurchasedProductList { get; private set; }
    
    private FiaStore()
    {
        this.State = StartableState.WAITING;
        this.ProductList = new List<Product>();
        this.PurchasedProductList = new List<string>();
        
        // 启动时加载已购买产品列表
        this.LoadPurchasedProductList();
    }
}
```

**商店状态管理**:
```csharp
public enum StartableState
{
    COMPLETED = 0,  // 操作成功完成
    WAITING = 1,    // 等待执行
    RUNNING = 2,    // 正在执行
    FAILED = 3      // 执行失败
}

// 状态转换方法
public void Start()
{
    this.State = StartableState.RUNNING;
    this.Error = null;
}

public void Complete()
{
    this.State = StartableState.COMPLETED;
    this.Error = null;
}

public void Fail(Exception error)
{
    this.State = StartableState.FAILED;
    this.Error = error;
}
```

#### 产品信息管理系统
**功能概述**: 管理商店产品信息的获取、缓存和查询

**产品请求架构**:
```csharp
public virtual IEnumerator RequestProductInfo()
{
    this.Start();
    
    try
    {
        // 构建产品ID列表
        List<string> productIds = this.BuildProductIdList();
        
        if (productIds.Count == 0)
        {
            Debug.LogWarning("No product IDs to request");
            this.Complete();
            yield break;
        }
        
        // 发起平台商店请求
        FiaStore._RequestProductInfo(productIds.ToArray());
        
        // 轮询等待响应
        while (this.State == StartableState.RUNNING)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        // 检查请求结果
        if (this.State == StartableState.FAILED)
        {
            Debug.LogError($"Product info request failed: {this.Error?.Message}");
            throw this.Error;
        }
        
        Debug.Log($"Successfully loaded {this.ProductList.Count} products");
    }
    catch (Exception e)
    {
        this.Fail(e);
        throw;
    }
}

private List<string> BuildProductIdList()
{
    List<string> ids = new List<string>();
    
    // 添加基础产品ID
    for (int i = 0; i < 10; i++)
    {
        ids.Add($"track.{i}");      // 单个赛道
        ids.Add($"kart.{i}");       // 单个卡丁车
        ids.Add($"bundle.{i}");     // 组合包
    }
    
    // 添加特殊组合
    ids.Add("bundle.all_tracks");
    ids.Add("bundle.all_karts");
    ids.Add("bundle.premium_pack");
    
    return ids;
}
```

**产品信息解析**:
```csharp
public static void RequestProductInfoSuccess(string productInfo)
{
    try
    {
        FiaStore store = FiaStore.Inst;
        store.ProductList.Clear();
        
        // 解析JSON格式的产品信息
        string[] productEntries = productInfo.Split('\n');
        
        foreach (string entry in productEntries)
        {
            if (string.IsNullOrEmpty(entry.Trim())) continue;
            
            Product product = ParseProductEntry(entry);
            if (product != null)
            {
                store.ProductList.Add(product);
            }
        }
        
        store.Complete();
        Debug.Log($"Parsed {store.ProductList.Count} products successfully");
    }
    catch (Exception e)
    {
        FiaStore.Inst.Fail(new XMLParsingException($"Failed to parse product info: {e.Message}"));
    }
}

private static Product ParseProductEntry(string entry)
{
    // 产品信息格式: "ID|Title|Description|Price|PriceString|Type|IconIndex"
    string[] parts = entry.Split('|');
    
    if (parts.Length < 7)
    {
        Debug.LogWarning($"Invalid product entry format: {entry}");
        return null;
    }
    
    return new Product
    {
        ID = parts[0],
        LocalizedTitle = parts[1],
        LocalizedDescription = parts[2],
        Price = float.TryParse(parts[3], out float price) ? price : 0f,
        PriceString = parts[4],
        Type = Enum.TryParse<ProductType>(parts[5], out ProductType type) ? type : ProductType.TRACK,
        IconIndex = int.TryParse(parts[6], out int iconIndex) ? iconIndex : 0
    };
}
```

#### 产品查询系统
**功能概述**: 提供高效的产品查询和筛选功能

**查询API设计**:
```csharp
// 按ID查找产品
public Product FindProductWithID(string productID)
{
    return this.ProductList.FirstOrDefault(p => 
        string.Equals(p.ID, productID, StringComparison.OrdinalIgnoreCase));
}

// 按类型查找产品
public List<Product> FindProductsWithType(ProductType type)
{
    return this.ProductList.Where(p => p.Type == type).ToList();
}

// 查找未购买的指定类型产品
public List<Product> FindUnpurchasedProductsWithType(ProductType type)
{
    return this.ProductList.Where(p => 
        p.Type == type && !this.PurchasedProductList.Contains(p.ID)).ToList();
}

// 检查产品是否已购买
public bool IsProductPurchased(string productID)
{
    return this.PurchasedProductList.Contains(productID);
}

// 获取已购买的指定类型产品
public List<Product> GetPurchasedProductsWithType(ProductType type)
{
    return this.ProductList.Where(p => 
        p.Type == type && this.PurchasedProductList.Contains(p.ID)).ToList();
}

// 高级查询 - 支持多条件筛选
public List<Product> FindProducts(Func<Product, bool> predicate)
{
    return this.ProductList.Where(predicate).ToList();
}

// 获取产品统计信息
public ProductStatistics GetProductStatistics()
{
    var stats = new ProductStatistics();
    
    foreach (Product product in this.ProductList)
    {
        stats.TotalProducts++;
        
        if (this.IsProductPurchased(product.ID))
        {
            stats.PurchasedProducts++;
            stats.TotalSpent += product.Price;
        }
        
        switch (product.Type)
        {
            case ProductType.TRACK:
                stats.TrackCount++;
                break;
            case ProductType.KART:
                stats.KartCount++;
                break;
            case ProductType.BUNDLE:
                stats.BundleCount++;
                break;
        }
    }
    
    stats.PurchaseRate = stats.TotalProducts > 0 ? 
        (float)stats.PurchasedProducts / stats.TotalProducts : 0f;
    
    return stats;
}
```

#### 购买流程系统
**功能概述**: 核心购买逻辑和状态管理

**购买流程实现**:
```csharp
public virtual IEnumerator Purchase(string productID)
{
    // 验证购买前提条件
    if (!this.ValidatePurchaseConditions(productID))
    {
        yield break;
    }
    
    this.Start();
    
    try
    {
        Debug.Log($"Starting purchase for product: {productID}");
        
        // 发起平台购买请求
        FiaStore._Purchase(productID);
        
        // 轮询等待购买完成
        float timeout = 60f; // 60秒超时
        float elapsed = 0f;
        
        while (this.State == StartableState.RUNNING && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }
        
        // 检查超时
        if (elapsed >= timeout)
        {
            this.Fail(new TimeoutException("Purchase request timed out"));
            yield break;
        }
        
        // 检查购买结果
        if (this.State == StartableState.FAILED)
        {
            Debug.LogError($"Purchase failed for {productID}: {this.Error?.Message}");
            throw this.Error;
        }
        
        Debug.Log($"Purchase completed successfully for {productID}");
    }
    catch (Exception e)
    {
        this.Fail(e);
        throw;
    }
}

private bool ValidatePurchaseConditions(string productID)
{
    // 检查产品是否存在
    Product product = this.FindProductWithID(productID);
    if (product == null)
    {
        this.Fail(new ArgumentException($"Product not found: {productID}"));
        return false;
    }
    
    // 检查是否已购买
    if (this.IsProductPurchased(productID))
    {
        Debug.LogWarning($"Product already purchased: {productID}");
        this.Complete();
        return false;
    }
    
    // 检查网络连接
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
        this.Fail(new ITunesStoreConnectionException("No internet connection"));
        return false;
    }
    
    return true;
}
```

**购买成功处理**:
```csharp
public static void PurchaseSuccess(string productID)
{
    try
    {
        FiaStore store = FiaStore.Inst;
        
        // 添加到已购买列表
        if (!store.PurchasedProductList.Contains(productID))
        {
            store.PurchasedProductList.Add(productID);
        }
        
        // 保存到本地存储
        bool saveSuccess = store.SavePurchasedProductList();
        if (!saveSuccess)
        {
            Debug.LogWarning("Failed to save purchase data locally");
        }
        
        // 应用购买内容
        store.ApplyPurchaseContent(productID);
        
        store.Complete();
        Debug.Log($"Purchase processed successfully: {productID}");
        
        // 发送购买完成事件
        EventManager.Instance.TriggerEvent("PurchaseCompleted", productID);
    }
    catch (Exception e)
    {
        FiaStore.Inst.Fail(e);
    }
}

private void ApplyPurchaseContent(string productID)
{
    Product product = this.FindProductWithID(productID);
    if (product == null) return;
    
    // 根据产品类型解锁内容
    switch (product.Type)
    {
        case ProductType.TRACK:
            this.UnlockTrack(productID);
            break;
            
        case ProductType.KART:
            this.UnlockKart(productID);
            break;
            
        case ProductType.BUNDLE:
            this.UnlockBundle(product);
            break;
            
        default:
            Debug.LogWarning($"Unknown product type for {productID}");
            break;
    }
}

private void UnlockBundle(Product bundle)
{
    // 解析组合包内容
    if (bundle.Unlocks != null)
    {
        foreach (string unlockID in bundle.Unlocks)
        {
            if (unlockID.StartsWith("track."))
            {
                this.UnlockTrack(unlockID);
            }
            else if (unlockID.StartsWith("kart."))
            {
                this.UnlockKart(unlockID);
            }
        }
    }
}
```

#### 购买恢复系统
**功能概述**: 恢复用户之前的购买记录

**恢复流程实现**:
```csharp
public IEnumerator RestorePurchases()
{
    this.Start();
    
    try
    {
        Debug.Log("Starting purchase restoration...");
        
        // 发起平台恢复请求
        FiaStore._RestorePurchases();
        
        // 等待恢复完成
        while (this.State == StartableState.RUNNING)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        if (this.State == StartableState.FAILED)
        {
            Debug.LogError($"Purchase restoration failed: {this.Error?.Message}");
            throw this.Error;
        }
        
        Debug.Log($"Purchase restoration completed. Restored {this.PurchasedProductList.Count} purchases.");
    }
    catch (Exception e)
    {
        this.Fail(e);
        throw;
    }
}

public static void RestorePurchasesSuccess(string purchasedProducts)
{
    try
    {
        FiaStore store = FiaStore.Inst;
        
        // 解析恢复的购买记录（逗号分隔）
        store.PurchasedProductList.Clear();
        
        if (!string.IsNullOrEmpty(purchasedProducts))
        {
            string[] products = purchasedProducts.Split(',');
            
            foreach (string productID in products)
            {
                string trimmedID = productID.Trim();
                if (!string.IsNullOrEmpty(trimmedID))
                {
                    store.PurchasedProductList.Add(trimmedID);
                    
                    // 应用恢复的购买内容
                    store.ApplyPurchaseContent(trimmedID);
                }
            }
        }
        
        // 保存恢复的数据
        store.SavePurchasedProductList();
        
        store.Complete();
        Debug.Log($"Restored {store.PurchasedProductList.Count} purchases");
        
        // 发送恢复完成事件
        EventManager.Instance.TriggerEvent("PurchasesRestored", store.PurchasedProductList.Count);
    }
    catch (Exception e)
    {
        FiaStore.Inst.Fail(e);
    }
}
```

### 2. 安全存储系统

#### 加密购买记录
**功能概述**: 使用AES加密和设备绑定保护购买数据

**加密存储实现**:
```csharp
public bool SavePurchasedProductList()
{
    try
    {
        string purchasedProductsPath = Path.Combine(FiaUtil.docPath, "purchasedProducts");
        
        // 构建要加密的数据（设备绑定）
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        string productData = string.Join(",", this.PurchasedProductList.ToArray());
        string dataToEncrypt = deviceID + "," + productData;
        
        // AES加密
        byte[] encryptedData = Encryption.Encrypt(dataToEncrypt);
        
        // 写入加密文件
        using (FileStream fileStream = new FileStream(purchasedProductsPath, FileMode.Create))
        {
            fileStream.Write(encryptedData, 0, encryptedData.Length);
        }
        
        Debug.Log($"Saved {this.PurchasedProductList.Count} purchased products to encrypted storage");
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to save purchased products: {e.Message}");
        return false;
    }
}

public bool LoadPurchasedProductList()
{
    try
    {
        string purchasedProductsPath = Path.Combine(FiaUtil.docPath, "purchasedProducts");
        
        if (!File.Exists(purchasedProductsPath))
        {
            Debug.Log("No existing purchase data found");
            return true;
        }
        
        // 读取加密文件
        byte[] encryptedData;
        using (FileStream fileStream = new FileStream(purchasedProductsPath, FileMode.Open))
        {
            encryptedData = new byte[fileStream.Length];
            fileStream.Read(encryptedData, 0, encryptedData.Length);
        }
        
        // 解密数据
        string decryptedData = Encryption.Decrypt(encryptedData);
        string[] parts = decryptedData.Split(',');
        
        // 验证设备绑定
        if (parts.Length < 1 || parts[0] != SystemInfo.deviceUniqueIdentifier)
        {
            Debug.LogWarning("Purchase data is bound to different device, clearing data");
            File.Delete(purchasedProductsPath);
            return false;
        }
        
        // 恢复购买列表
        this.PurchasedProductList.Clear();
        for (int i = 1; i < parts.Length; i++)
        {
            if (!string.IsNullOrEmpty(parts[i]))
            {
                this.PurchasedProductList.Add(parts[i]);
            }
        }
        
        Debug.Log($"Loaded {this.PurchasedProductList.Count} purchased products from encrypted storage");
        return true;
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to load purchased products: {e.Message}");
        
        // 删除损坏的文件
        try
        {
            string purchasedProductsPath = Path.Combine(FiaUtil.docPath, "purchasedProducts");
            if (File.Exists(purchasedProductsPath))
            {
                File.Delete(purchasedProductsPath);
                Debug.Log("Deleted corrupted purchase data file");
            }
        }
        catch (Exception deleteEx)
        {
            Debug.LogError($"Failed to delete corrupted file: {deleteEx.Message}");
        }
        
        return false;
    }
}
```

#### 数据完整性验证
**功能概述**: 多层验证确保数据完整性

**完整性检查实现**:
```csharp
public class PurchaseDataValidator
{
    private static readonly string CHECKSUM_KEY = "KartGame_Purchase_Integrity_2023";
    
    public static bool ValidateDataIntegrity(string data)
    {
        try
        {
            // 分离数据和校验和
            string[] parts = data.Split('|');
            if (parts.Length != 2)
            {
                return false;
            }
            
            string actualData = parts[0];
            string expectedChecksum = parts[1];
            
            // 计算校验和
            string actualChecksum = ComputeChecksum(actualData);
            
            // 验证校验和
            return string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
    
    public static string AddChecksum(string data)
    {
        string checksum = ComputeChecksum(data);
        return $"{data}|{checksum}";
    }
    
    private static string ComputeChecksum(string data)
    {
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            string dataWithKey = data + CHECKSUM_KEY;
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(dataWithKey);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    // 时间戳验证防止回放攻击
    public static bool ValidateTimestamp(DateTime timestamp, TimeSpan maxAge)
    {
        TimeSpan age = DateTime.UtcNow - timestamp;
        return age <= maxAge;
    }
    
    // 产品ID格式验证
    public static bool ValidateProductID(string productID)
    {
        if (string.IsNullOrEmpty(productID))
            return false;
        
        // 产品ID格式：type.identifier
        string[] parts = productID.Split('.');
        if (parts.Length != 2)
            return false;
        
        string type = parts[0];
        string identifier = parts[1];
        
        // 验证类型
        if (!IsValidProductType(type))
            return false;
        
        // 验证标识符（数字或特殊名称）
        return IsValidIdentifier(identifier);
    }
    
    private static bool IsValidProductType(string type)
    {
        return type == "track" || type == "kart" || type == "bundle";
    }
    
    private static bool IsValidIdentifier(string identifier)
    {
        // 数字标识符
        if (int.TryParse(identifier, out int numId))
        {
            return numId >= 0 && numId < 100;
        }
        
        // 特殊标识符
        string[] validSpecialIds = { "all_tracks", "all_karts", "premium_pack" };
        return validSpecialIds.Contains(identifier);
    }
}
```

### 3. 模拟商店系统 - MockFiaStore.cs

#### 开发测试环境
**功能概述**: 为开发和测试提供无需真实交易的模拟商店

**模拟商店实现**:
```csharp
public class MockFiaStore : FiaStore
{
    private static MockFiaStore mockStore_;
    
    public static new MockFiaStore Inst
    {
        get
        {
            if (MockFiaStore.mockStore_ == null)
            {
                MockFiaStore.mockStore_ = new MockFiaStore();
            }
            return MockFiaStore.mockStore_;
        }
    }
    
    private string locale_ = "en"; // 默认语言
    
    // 模拟产品信息请求
    public override IEnumerator RequestProductInfo()
    {
        base.State = StartableState.RUNNING;
        
        yield return new WaitForSeconds(0.5f); // 模拟网络延迟
        
        try
        {
            string mockDataPath = Path.Combine(FiaUtil.docPath, "mockstore", $"{this.locale_}.txt");
            
            if (!File.Exists(mockDataPath))
            {
                this.CreateDefaultMockData(mockDataPath);
            }
            
            string mockProductData = File.ReadAllText(mockDataPath);
            FiaStore.RequestProductInfoSuccess(mockProductData);
        }
        catch (Exception e)
        {
            FiaStore.RequestProductInfoFailure((int)FiaStoreError.UNKNOWN);
        }
    }
    
    // 模拟购买流程
    public override IEnumerator Purchase(string productID)
    {
        base.State = StartableState.RUNNING;
        
        yield return new WaitForSeconds(1f); // 模拟购买延迟
        
        // 模拟随机失败（5%概率）
        if (UnityEngine.Random.value < 0.05f)
        {
            int randomError = UnityEngine.Random.Range(0, 3);
            FiaStore.PurchaseFailure(randomError);
        }
        else
        {
            FiaStore.PurchaseSuccess(productID);
        }
    }
    
    // 创建默认模拟数据
    private void CreateDefaultMockData(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
        StringBuilder mockData = new StringBuilder();
        
        // 生成模拟赛道产品
        for (int i = 0; i < 10; i++)
        {
            mockData.AppendLine($"track.{i}|Track {i + 1}|A challenging racing track|2.99|$2.99|TRACK|{i}");
        }
        
        // 生成模拟卡丁车产品
        for (int i = 0; i < 8; i++)
        {
            mockData.AppendLine($"kart.{i}|Kart {i + 1}|High-performance racing kart|1.99|$1.99|KART|{i + 10}");
        }
        
        // 生成模拟组合包
        mockData.AppendLine("bundle.all_tracks|All Tracks Bundle|Unlock all racing tracks|19.99|$19.99|BUNDLE|100");
        mockData.AppendLine("bundle.all_karts|All Karts Bundle|Unlock all racing karts|14.99|$14.99|BUNDLE|101");
        mockData.AppendLine("bundle.premium_pack|Premium Pack|Complete game content|29.99|$29.99|BUNDLE|102");
        
        File.WriteAllText(filePath, mockData.ToString());
        Debug.Log($"Created mock store data at {filePath}");
    }
    
    // 设置语言环境
    public void SetLocale(string locale)
    {
        this.locale_ = locale;
    }
    
    // 模拟网络错误
    public void SimulateNetworkError()
    {
        base.State = StartableState.FAILED;
        base.Error = new ITunesStoreConnectionException("Simulated network error");
    }
    
    // 清除模拟数据
    public void ClearMockData()
    {
        string mockStoreDir = Path.Combine(FiaUtil.docPath, "mockstore");
        if (Directory.Exists(mockStoreDir))
        {
            Directory.Delete(mockStoreDir, true);
            Debug.Log("Cleared mock store data");
        }
    }
}
```

### 4. 错误处理系统

#### 统一错误管理
**功能概述**: 完善的错误分类、处理和恢复机制

**错误类型定义**:
```csharp
internal enum FiaStoreError
{
    UNKNOWN = -1,           // 未知错误
    CANCELED = 0,           // 用户取消
    COULD_NOT_BE_REACHED = 1, // 网络不可达
    WRONG_PRODUCT_ID = 2    // 错误的产品ID
}

// 自定义异常类
public class ITunesStoreConnectionException : Exception
{
    public ITunesStoreConnectionException() : base("Could not connect to iTunes Store") { }
    public ITunesStoreConnectionException(string message) : base(message) { }
    public ITunesStoreConnectionException(string message, Exception innerException) 
        : base(message, innerException) { }
}

public class FiaStoreException : Exception
{
    public FiaStoreError ErrorCode { get; }
    
    public FiaStoreException(FiaStoreError errorCode, string message) : base(message)
    {
        this.ErrorCode = errorCode;
    }
    
    public FiaStoreException(FiaStoreError errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        this.ErrorCode = errorCode;
    }
}
```

**错误处理机制**:
```csharp
public static void PurchaseFailure(int errorCode)
{
    FiaStore store = FiaStore.Inst;
    
    Exception error = errorCode switch
    {
        (int)FiaStoreError.CANCELED => new CanceledException("Purchase was canceled by user"),
        (int)FiaStoreError.COULD_NOT_BE_REACHED => new ITunesStoreConnectionException("Cannot reach store servers"),
        (int)FiaStoreError.WRONG_PRODUCT_ID => new ArgumentException("Invalid product ID provided"),
        _ => new UnknownException($"Purchase failed with error code: {errorCode}")
    };
    
    store.Fail(error);
    
    // 记录错误用于分析
    ErrorTracker.Instance.LogError("Store", "PurchaseFailure", errorCode, error.Message);
    
    // 发送错误事件
    EventManager.Instance.TriggerEvent("PurchaseFailed", new PurchaseFailedEventArgs
    {
        ErrorCode = errorCode,
        ErrorMessage = error.Message,
        Timestamp = DateTime.UtcNow
    });
}

public static void RequestProductInfoFailure(int errorCode)
{
    FiaStore store = FiaStore.Inst;
    
    Exception error = (errorCode + 1) switch
    {
        0 => new UnknownException("Unknown error occurred while requesting product info"),
        1 => new CanceledException("Product info request was canceled"),
        2 => new ITunesStoreConnectionException("Cannot connect to store for product info"),
        _ => new UnknownException($"Product info request failed with error code: {errorCode}")
    };
    
    store.Fail(error);
    
    // 尝试使用缓存的产品信息
    if (store.ProductList.Count > 0)
    {
        Debug.LogWarning("Using cached product information due to network error");
        store.Complete();
    }
}
```

### 5. 平台集成接口

#### 原生平台抽象
**功能概述**: 抽象化的平台原生接口调用

**平台接口定义**:
```csharp
// 抽象的平台接口方法（由原生插件实现）
public abstract class PlatformStoreInterface
{
    // 请求产品信息
    protected static extern void _RequestProductInfo(string[] productIDs);
    
    // 发起购买
    protected static extern void _Purchase(string productID);
    
    // 恢复购买
    protected static extern void _RestorePurchases();
    
    // 检查商店可用性
    protected static extern bool _IsStoreAvailable();
    
    // 获取平台特定信息
    protected static extern string _GetPlatformInfo();
}

// iOS平台实现（通过P/Invoke）
#if UNITY_IOS && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern void _RequestProductInfo(string[] productIDs, int count);

[DllImport("__Internal")]
private static extern void _Purchase(string productID);

[DllImport("__Internal")]
private static extern void _RestorePurchases();
#endif

// Android平台实现（通过JNI）
#if UNITY_ANDROID && !UNITY_EDITOR
private static AndroidJavaClass storeClass;
private static AndroidJavaObject storeInstance;

static PlatformStoreInterface()
{
    storeClass = new AndroidJavaClass("com.kartgame.store.StoreManager");
    storeInstance = storeClass.CallStatic<AndroidJavaObject>("getInstance");
}

protected static void _Purchase(string productID)
{
    storeInstance.Call("purchase", productID);
}
#endif
```

### 6. UI集成系统

#### 商店界面集成
**功能概述**: 与游戏UI系统的无缝集成

**购买按钮组件**:
```csharp
public class GUIStorePurchaseButton : MonoBehaviour
{
    [Header("Product Configuration")]
    public string productID;
    public ProductType productType;
    
    [Header("UI Elements")]
    public Button purchaseButton;
    public Text titleText;
    public Text priceText;
    public Text descriptionText;
    public Image productIcon;
    public GameObject loadingIndicator;
    public GameObject purchasedIndicator;
    
    private FiaStore store_;
    private Product product_;
    
    private void Start()
    {
        // 根据平台选择商店实例
        this.store_ = this.GetStoreInstance();
        
        // 初始化UI
        this.InitializeUI();
        
        // 刷新产品信息
        StartCoroutine(this.RefreshProductInfo());
    }
    
    private FiaStore GetStoreInstance()
    {
        #if UNITY_EDITOR
        return MockFiaStore.Inst; // 编辑器中使用模拟商店
        #elif UNITY_IOS || UNITY_ANDROID
        return FiaStore.Inst; // 真实设备使用真实商店
        #else
        return MockFiaStore.Inst; // 其他平台使用模拟商店
        #endif
    }
    
    private IEnumerator RefreshProductInfo()
    {
        this.loadingIndicator.SetActive(true);
        
        // 等待产品信息加载
        yield return StartCoroutine(this.store_.RequestProductInfo());
        
        this.product_ = this.store_.FindProductWithID(this.productID);
        
        this.UpdateUI();
        this.loadingIndicator.SetActive(false);
    }
    
    private void UpdateUI()
    {
        if (this.product_ != null)
        {
            this.titleText.text = this.product_.LocalizedTitle;
            this.priceText.text = this.product_.PriceString;
            this.descriptionText.text = this.product_.LocalizedDescription;
            
            // 加载产品图标
            this.LoadProductIcon();
        }
        
        // 更新购买状态
        bool isPurchased = this.store_.IsProductPurchased(this.productID);
        this.purchaseButton.interactable = !isPurchased;
        this.purchasedIndicator.SetActive(isPurchased);
        
        // 设置按钮文本
        this.purchaseButton.GetComponentInChildren<Text>().text = 
            isPurchased ? "已购买" : "购买";
    }
    
    public void OnPurchaseClicked()
    {
        if (this.product_ == null) return;
        
        StartCoroutine(this.ProcessPurchase());
    }
    
    private IEnumerator ProcessPurchase()
    {
        this.purchaseButton.interactable = false;
        this.loadingIndicator.SetActive(true);
        
        try
        {
            yield return StartCoroutine(this.store_.Purchase(this.productID));
            
            // 购买成功
            this.UpdateUI();
            this.ShowPurchaseSuccessEffect();
        }
        catch (CanceledException)
        {
            Debug.Log("Purchase was canceled by user");
        }
        catch (ITunesStoreConnectionException)
        {
            this.ShowError("网络连接失败，请检查网络设置");
        }
        catch (Exception e)
        {
            this.ShowError($"购买失败：{e.Message}");
        }
        finally
        {
            this.loadingIndicator.SetActive(false);
            this.purchaseButton.interactable = true;
        }
    }
    
    private void LoadProductIcon()
    {
        if (this.product_.IconIndex >= 0)
        {
            Sprite iconSprite = ResourceManager.Instance.LoadSprite($"product_icon_{this.product_.IconIndex}");
            if (iconSprite != null)
            {
                this.productIcon.sprite = iconSprite;
            }
        }
    }
    
    private void ShowPurchaseSuccessEffect()
    {
        // 播放购买成功动画和音效
        ParticleSystem successEffect = GetComponent<ParticleSystem>();
        if (successEffect != null)
        {
            successEffect.Play();
        }
        
        AudioManager.Instance.PlaySound("purchase_success");
    }
    
    private void ShowError(string message)
    {
        UIManager.Instance.ShowErrorDialog("购买失败", message);
    }
}
```

## 高级功能实现

### 1. 智能缓存系统
```csharp
public class StoreCache
{
    private static readonly TimeSpan CACHE_EXPIRY = TimeSpan.FromHours(6);
    private DateTime lastProductInfoUpdate_;
    private Dictionary<string, Product> productCache_;
    
    public bool IsProductInfoCacheValid()
    {
        return DateTime.UtcNow - this.lastProductInfoUpdate_ < CACHE_EXPIRY;
    }
    
    public void InvalidateCache()
    {
        this.productCache_.Clear();
        this.lastProductInfoUpdate_ = DateTime.MinValue;
    }
}
```

### 2. 分析和追踪
```csharp
public class PurchaseAnalytics
{
    public void TrackPurchaseAttempt(string productID)
    {
        AnalyticsManager.Instance.TrackEvent("Purchase_Attempt", new Dictionary<string, object>
        {
            ["product_id"] = productID,
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["user_session"] = SessionManager.Instance.SessionID
        });
    }
    
    public void TrackPurchaseComplete(string productID, float price)
    {
        AnalyticsManager.Instance.TrackRevenue(productID, price, "USD");
    }
}
```

## 总结

Store系统展现了企业级的内购系统设计，具有以下核心优势：

### 技术优势
1. **安全优先**: AES加密、设备绑定和数据完整性验证
2. **平台抽象**: 统一接口支持多平台商店集成
3. **异步架构**: 基于协程的非阻塞操作模式
4. **错误恢复**: 完善的错误处理和恢复机制
5. **测试支持**: 内置模拟商店支持开发测试

### 架构特点
- **单例模式**: 全局状态管理和资源优化
- **模板方法**: 标准化的操作流程框架
- **策略模式**: 真实与模拟商店的灵活切换
- **观察者模式**: 事件驱动的状态通知机制

该商店系统为卡丁车游戏提供了生产级的商业化基础，支持安全的支付处理、灵活的产品管理和优秀的用户体验，确保了商业模式的成功实施。