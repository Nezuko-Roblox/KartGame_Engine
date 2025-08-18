# Exceptions 文件夹完整功能文档

## 概述

Exceptions 文件夹包含了卡丁车游戏的完整异常处理系统，实现了分层次的错误管理架构。该系统覆盖了网络通信、身份验证、数据处理和游戏逻辑等各个领域的异常情况，通过工厂模式和标准化的异常继承体系，为游戏提供了健壮的错误处理和调试支持。

## 系统架构

### 核心设计原则
- **分层异常处理**: 从具体到通用的异常分类体系
- **工厂模式**: 集中化的异常创建和类型管理
- **标准化构造**: 一致的异常构造函数模式
- **异常链传递**: 保留原始异常上下文的包装策略
- **调试友好**: 详细的错误信息和位置跟踪

### 异常分类体系
- **网络通信异常**: 请求和服务器通信错误
- **身份验证异常**: Fia和Facebook认证失败
- **数据处理异常**: JSON和XML解析错误
- **应用特定异常**: 游戏记录和操作取消异常

## 文件详细分析

### 1. ExceptionFactory.cs - 异常工厂管理器
**文件位置**: `/Exceptions/ExceptionFactory.cs`
**功能概述**: 集中化的异常类型管理和动态创建工厂

**工厂注册系统**:
```csharp
public class ExceptionFactory
{
    // 静态类型注册表
    private static readonly Type[] exceptionTypes_ = new Type[]
    {
        typeof(RequestException),
        typeof(ServerException),
        typeof(FiaAuthException),
        typeof(JSONFormattingException),
        typeof(XMLParsingException),
        typeof(XMLParseException),
        typeof(InvalidRecordException),
        typeof(CanceledException),
        typeof(UnknownException)
    };
    
    // 延迟加载的类型字典
    private static Dictionary<string, Type> nameToType_;
}
```

**延迟初始化字典**:
```csharp
private static void InitializeTypeDictionary()
{
    if (nameToType_ == null)
    {
        nameToType_ = new Dictionary<string, Type>();
        
        foreach (Type exceptionType in exceptionTypes_)
        {
            nameToType_[exceptionType.Name] = exceptionType;
        }
    }
}

public static bool IsFiaException(Type type)
{
    InitializeTypeDictionary();
    return nameToType_.ContainsValue(type);
}

public static Type FiaExceptionFromTypeName(string typeName)
{
    InitializeTypeDictionary();
    
    if (nameToType_.TryGetValue(typeName, out Type exceptionType))
    {
        return exceptionType;
    }
    
    return typeof(UnknownException); // 默认异常类型
}
```

**动态异常创建系统**:
```csharp
public static Exception CreateException(string typeName, string message = null, Exception innerException = null)
{
    Type exceptionType = FiaExceptionFromTypeName(typeName);
    
    try
    {
        // 尝试使用带消息和内部异常的构造函数
        if (message != null && innerException != null)
        {
            return (Exception)Activator.CreateInstance(exceptionType, message, innerException);
        }
        
        // 尝试使用带消息的构造函数
        if (message != null)
        {
            return (Exception)Activator.CreateInstance(exceptionType, message);
        }
        
        // 使用默认构造函数
        return (Exception)Activator.CreateInstance(exceptionType);
    }
    catch (Exception e)
    {
        // 创建失败时返回通用异常
        return new UnknownException($"Failed to create exception of type {typeName}: {e.Message}");
    }
}

public static T CreateException<T>(string message = null, Exception innerException = null) where T : Exception
{
    return (T)CreateException(typeof(T).Name, message, innerException);
}
```

### 2. 网络通信异常类

#### RequestException.cs - 请求异常
**文件位置**: `/Exceptions/RequestException.cs`
**功能概述**: 处理HTTP和网络请求失败

**标准异常模式**:
```csharp
public class RequestException : Exception
{
    // 默认构造函数
    public RequestException() : base() { }
    
    // 消息构造函数
    public RequestException(string message) : base(message) { }
    
    // 包装构造函数
    public RequestException(Exception innerException) 
        : base("RequestException", innerException) { }
}
```

**使用示例**:
```csharp
public class NetworkManager
{
    public async Task<string> GetDataAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new RequestException($"HTTP request failed with status {response.StatusCode}: {response.ReasonPhrase}");
            }
            
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            throw new RequestException(e);
        }
        catch (TaskCanceledException e)
        {
            throw new CanceledException(e);
        }
    }
}
```

#### ServerException.cs - 服务器异常
**功能概述**: 处理服务器端错误和响应失败

**高级错误处理**:
```csharp
public class AdvancedServerException : ServerException
{
    public int StatusCode { get; }
    public string ServerMessage { get; }
    public DateTime Timestamp { get; }
    
    public AdvancedServerException(int statusCode, string serverMessage) 
        : base($"Server error {statusCode}: {serverMessage}")
    {
        this.StatusCode = statusCode;
        this.ServerMessage = serverMessage;
        this.Timestamp = DateTime.UtcNow;
    }
    
    public override string ToString()
    {
        return $"{base.ToString()}\nStatus Code: {StatusCode}\nServer Message: {ServerMessage}\nTimestamp: {Timestamp:yyyy-MM-dd HH:mm:ss} UTC";
    }
}
```

### 3. 身份验证异常类

#### FiaAuthException.cs - Fia认证异常
**功能概述**: 处理Fia认证系统的失败情况

**认证错误分类**:
```csharp
public class FiaAuthException : Exception
{
    public AuthErrorType ErrorType { get; }
    public string UserId { get; }
    
    public enum AuthErrorType
    {
        InvalidCredentials,
        TokenExpired,
        ServerUnavailable,
        NetworkError,
        UnknownError
    }
    
    public FiaAuthException(AuthErrorType errorType, string userId = null, string message = null) 
        : base(message ?? GetDefaultMessage(errorType))
    {
        this.ErrorType = errorType;
        this.UserId = userId;
    }
    
    private static string GetDefaultMessage(AuthErrorType errorType)
    {
        return errorType switch
        {
            AuthErrorType.InvalidCredentials => "Invalid username or password",
            AuthErrorType.TokenExpired => "Authentication token has expired",
            AuthErrorType.ServerUnavailable => "Authentication server is unavailable",
            AuthErrorType.NetworkError => "Network error during authentication",
            _ => "Unknown authentication error"
        };
    }
}
```

**认证重试机制**:
```csharp
public class FiaAuthService
{
    private const int MAX_RETRY_ATTEMPTS = 3;
    
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        for (int attempt = 1; attempt <= MAX_RETRY_ATTEMPTS; attempt++)
        {
            try
            {
                return await PerformAuthenticationAsync(username, password);
            }
            catch (FiaAuthException e) when (e.ErrorType == FiaAuthException.AuthErrorType.NetworkError && attempt < MAX_RETRY_ATTEMPTS)
            {
                await Task.Delay(1000 * attempt); // 递增延迟
                continue;
            }
            catch (FiaAuthException e) when (e.ErrorType == FiaAuthException.AuthErrorType.ServerUnavailable && attempt < MAX_RETRY_ATTEMPTS)
            {
                await Task.Delay(2000 * attempt); // 更长的服务器错误延迟
                continue;
            }
        }
        
        throw new FiaAuthException(FiaAuthException.AuthErrorType.UnknownError, username, "Authentication failed after maximum retry attempts");
    }
}
```

### 4. 数据处理异常类

#### JSONFormattingException.cs - JSON格式化异常
**功能概述**: 处理JSON解析和格式化错误

**JSON处理包装器**:
```csharp
public static class SafeJsonProcessor
{
    public static T ParseJson<T>(string jsonString)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
        catch (JsonException e)
        {
            throw new JSONFormattingException($"Failed to parse JSON: {e.Message}");
        }
        catch (Exception e)
        {
            throw new JSONFormattingException($"Unexpected error during JSON parsing: {e.Message}");
        }
    }
    
    public static string SerializeJson<T>(T obj, bool formatted = false)
    {
        try
        {
            Formatting formatting = formatted ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(obj, formatting);
        }
        catch (JsonException e)
        {
            throw new JSONFormattingException($"Failed to serialize object to JSON: {e.Message}");
        }
    }
}
```

#### XMLParseException.cs - 高级XML解析异常
**功能概述**: 提供详细的XML解析错误信息，包括行号跟踪

**详细错误报告**:
```csharp
public class XMLParseException : SystemException
{
    private readonly int lineNumber_;
    private readonly string elementName_;
    
    public XMLParseException(string elementName, int lineNumber, string message)
        : base($"XML Parse Error in element '{elementName}' at line {lineNumber}: {message}")
    {
        this.elementName_ = elementName;
        this.lineNumber_ = lineNumber;
    }
    
    public int getLineNr()
    {
        return this.lineNumber_;
    }
    
    public string GetElementName()
    {
        return this.elementName_;
    }
    
    public override string ToString()
    {
        return $"{base.ToString()}\nElement: {elementName_}\nLine: {lineNumber_}";
    }
}
```

**XML解析增强系统**:
```csharp
public class AdvancedXMLParser
{
    public static XmlDocument ParseWithLineTracking(string xmlContent)
    {
        XmlDocument doc = new XmlDocument();
        
        try
        {
            // 启用行号信息
            XmlReaderSettings settings = new XmlReaderSettings
            {
                IgnoreWhitespace = false,
                IgnoreComments = false
            };
            
            using (StringReader stringReader = new StringReader(xmlContent))
            using (XmlReader reader = XmlReader.Create(stringReader, settings))
            {
                doc.Load(reader);
            }
            
            return doc;
        }
        catch (XmlException e)
        {
            string elementName = ExtractElementNameFromException(e);
            throw new XMLParseException(elementName, e.LineNumber, e.Message);
        }
    }
    
    private static string ExtractElementNameFromException(XmlException e)
    {
        // 从异常消息中提取元素名称
        string message = e.Message;
        Match match = Regex.Match(message, @"'([^']+)'");
        return match.Success ? match.Groups[1].Value : "unknown";
    }
    
    public static T ParseXMLElement<T>(XmlElement element, string elementName, int lineNumber)
    {
        try
        {
            return DeserializeElement<T>(element);
        }
        catch (Exception e)
        {
            throw new XMLParseException(elementName, lineNumber, $"Failed to deserialize element: {e.Message}");
        }
    }
}
```

### 5. 应用特定异常类

#### InvalidRecordException.cs - 无效记录异常
**功能概述**: 处理游戏记录和数据验证失败

**记录验证系统**:
```csharp
public class GameRecordValidator
{
    public static void ValidateRaceRecord(RaceRecord record)
    {
        if (record == null)
        {
            throw new InvalidRecordException("Race record cannot be null");
        }
        
        if (record.RaceTime <= 0)
        {
            throw new InvalidRecordException($"Invalid race time: {record.RaceTime}. Time must be positive.");
        }
        
        if (record.LapCount <= 0)
        {
            throw new InvalidRecordException($"Invalid lap count: {record.LapCount}. Must have at least one lap.");
        }
        
        if (string.IsNullOrEmpty(record.PlayerName))
        {
            throw new InvalidRecordException("Player name cannot be empty");
        }
        
        if (record.TrackId <= 0)
        {
            throw new InvalidRecordException($"Invalid track ID: {record.TrackId}");
        }
        
        // 验证时间合理性
        float averageLapTime = record.RaceTime / record.LapCount;
        if (averageLapTime < MIN_REASONABLE_LAP_TIME || averageLapTime > MAX_REASONABLE_LAP_TIME)
        {
            throw new InvalidRecordException($"Unreasonable lap time detected: {averageLapTime:F2}s. Record may be invalid.");
        }
    }
    
    public static void ValidateLeaderboardEntry(LeaderboardEntry entry)
    {
        ValidateRaceRecord(entry.Record);
        
        if (entry.Rank <= 0)
        {
            throw new InvalidRecordException($"Invalid rank: {entry.Rank}. Rank must be positive.");
        }
        
        if (entry.Score < 0)
        {
            throw new InvalidRecordException($"Invalid score: {entry.Score}. Score cannot be negative.");
        }
    }
}
```

#### CanceledException.cs - 操作取消异常
**功能概述**: 处理用户或系统取消的操作

**取消操作管理**:
```csharp
public class CancellableOperation
{
    private CancellationTokenSource cancellationTokenSource_;
    
    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, int timeoutMs = 30000)
    {
        this.cancellationTokenSource_ = new CancellationTokenSource(timeoutMs);
        
        try
        {
            return await operation(this.cancellationTokenSource_.Token);
        }
        catch (OperationCanceledException e) when (this.cancellationTokenSource_.Token.IsCancellationRequested)
        {
            if (e.CancellationToken.IsCancellationRequested)
            {
                throw new CanceledException("Operation was cancelled by user request");
            }
            else
            {
                throw new CanceledException("Operation timed out");
            }
        }
    }
    
    public void Cancel()
    {
        this.cancellationTokenSource_?.Cancel();
    }
}
```

### 异常处理最佳实践

#### 统一异常处理器
```csharp
public static class GlobalExceptionHandler
{
    public static void HandleException(Exception e)
    {
        switch (e)
        {
            case FiaAuthException authEx:
                HandleAuthenticationException(authEx);
                break;
                
            case RequestException reqEx:
                HandleNetworkException(reqEx);
                break;
                
            case JSONFormattingException jsonEx:
                HandleDataFormatException(jsonEx);
                break;
                
            case XMLParseException xmlEx:
                HandleXMLException(xmlEx);
                break;
                
            case InvalidRecordException recordEx:
                HandleRecordException(recordEx);
                break;
                
            case CanceledException cancelEx:
                HandleCancellationException(cancelEx);
                break;
                
            default:
                HandleUnknownException(e);
                break;
        }
    }
    
    private static void HandleAuthenticationException(FiaAuthException e)
    {
        Debug.LogError($"Authentication failed: {e.Message}");
        
        switch (e.ErrorType)
        {
            case FiaAuthException.AuthErrorType.InvalidCredentials:
                ShowLoginDialog("Invalid username or password");
                break;
                
            case FiaAuthException.AuthErrorType.TokenExpired:
                RefreshAuthenticationToken();
                break;
                
            case FiaAuthException.AuthErrorType.NetworkError:
                ShowRetryDialog("Network error. Please check your connection.");
                break;
        }
    }
    
    private static void HandleXMLException(XMLParseException e)
    {
        Debug.LogError($"XML Parse Error: {e.Message}");
        Debug.LogError($"Element: {e.GetElementName()}, Line: {e.getLineNr()}");
        
        // 记录详细的调试信息
        LogDetailedXMLError(e);
    }
}
```

#### 异常日志系统
```csharp
public static class ExceptionLogger
{
    private static readonly string logFilePath_ = Path.Combine(Application.persistentDataPath, "exceptions.log");
    
    public static void LogException(Exception e)
    {
        string logEntry = FormatExceptionLog(e);
        
        try
        {
            File.AppendAllText(logFilePath_, logEntry + Environment.NewLine);
        }
        catch (Exception logEx)
        {
            Debug.LogError($"Failed to log exception: {logEx.Message}");
        }
    }
    
    private static string FormatExceptionLog(Exception e)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] Exception occurred:");
        sb.AppendLine($"Type: {e.GetType().Name}");
        sb.AppendLine($"Message: {e.Message}");
        
        if (e is XMLParseException xmlEx)
        {
            sb.AppendLine($"Element: {xmlEx.GetElementName()}");
            sb.AppendLine($"Line: {xmlEx.getLineNr()}");
        }
        
        if (e is FiaAuthException authEx)
        {
            sb.AppendLine($"Auth Error Type: {authEx.ErrorType}");
            sb.AppendLine($"User ID: {authEx.UserId ?? "N/A"}");
        }
        
        sb.AppendLine($"Stack Trace: {e.StackTrace}");
        
        if (e.InnerException != null)
        {
            sb.AppendLine("--- Inner Exception ---");
            sb.AppendLine(FormatExceptionLog(e.InnerException));
        }
        
        sb.AppendLine(new string('-', 80));
        
        return sb.ToString();
    }
}
```

## 系统集成和使用指南

### 网络层集成
```csharp
public class NetworkClient
{
    public async Task<ApiResponse<T>> CallApiAsync<T>(string endpoint, object requestData = null)
    {
        try
        {
            string jsonRequest = requestData != null ? SafeJsonProcessor.SerializeJson(requestData) : null;
            string jsonResponse = await this.SendRequestAsync(endpoint, jsonRequest);
            
            T responseData = SafeJsonProcessor.ParseJson<T>(jsonResponse);
            return new ApiResponse<T> { Data = responseData, Success = true };
        }
        catch (RequestException e)
        {
            ExceptionLogger.LogException(e);
            return new ApiResponse<T> { Error = e.Message, Success = false };
        }
        catch (JSONFormattingException e)
        {
            ExceptionLogger.LogException(e);
            return new ApiResponse<T> { Error = "Data format error", Success = false };
        }
        catch (Exception e)
        {
            Exception wrappedException = ExceptionFactory.CreateException("UnknownException", "Unexpected API error", e);
            ExceptionLogger.LogException(wrappedException);
            return new ApiResponse<T> { Error = "Unexpected error", Success = false };
        }
    }
}
```

## 总结

Exceptions系统提供了一个完整、健壮的错误处理框架，具有以下核心优势：

### 技术优势
1. **分层异常体系**: 从通用到具体的完整异常分类
2. **工厂模式管理**: 集中化的异常类型注册和创建
3. **标准化构造**: 一致的异常构造函数模式
4. **详细错误信息**: XML解析异常提供行号级别的调试信息
5. **平台感知**: 支持多平台特定的异常处理

### 架构特点
- **集中管理**: 工厂模式统一异常创建
- **可扩展性**: 易于添加新的异常类型
- **调试友好**: 丰富的异常信息和链式传递
- **分层处理**: 不同级别的异常处理策略

该异常处理系统为卡丁车游戏提供了可靠的错误管理基础，支持网络通信、数据处理、身份验证和游戏逻辑等各个方面的异常情况，确保了系统的稳定性和可维护性。