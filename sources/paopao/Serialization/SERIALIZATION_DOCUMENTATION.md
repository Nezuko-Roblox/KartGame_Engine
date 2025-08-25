# Serialization 模块详细功能文档

## 概述

Serialization 模块是卡丁车游戏项目中的数据序列化系统，负责处理游戏数据的持久化存储、网络传输和安全保护。该模块提供了加密功能、通用序列化接口和Unity特定数据类型的序列化支持，确保游戏数据的安全性和完整性。

## 模块结构

```
Serialization/
├── Encryption.cs      - 数据加密和解密工具
├── Serializable.cs    - 序列化接口定义
└── Serializer.cs      - Unity数据类型序列化器
```

## 系统架构图

```
游戏数据序列化体系
├─ 数据安全层
│  └─ Encryption (AES加密 + HMAC验证)
├─ 序列化接口层
│  └─ Serializable (统一序列化接口)
└─ 类型支持层
   └─ Serializer (Unity类型序列化)

数据流向:
原始数据 → Serializer → 二进制数据 → Encryption → 加密数据
加密数据 → Encryption → 二进制数据 → Serializer → 原始数据
```

## 核心类详细分析

### 1. Encryption.cs - 数据加密和解密工具

**功能概述：**
提供基于AES（Rijndael）算法的数据加密和HMAC-MD5消息认证的安全数据处理功能，确保游戏数据的机密性和完整性。

**安全架构：**
- **加密算法**: AES (Rijndael) 对称加密
- **密钥管理**: 自动生成随机密钥和初始化向量
- **完整性保护**: HMAC-MD5 消息认证码
- **数据结构**: [HMAC][IV][加密数据][密钥] 格式

#### 1.1 加密流程实现

**核心加密方法：**
```csharp
public static byte[] Encrypt(string original)
{
    // 1. 创建AES加密器
    RijndaelManaged rijndael = new RijndaelManaged();
    
    // 2. 执行AES加密
    byte[] encryptedData = EncryptString(original, rijndael.Key, rijndael.IV);
    
    // 3. 构建数据包：[IV][加密数据][密钥]
    byte[] dataPacket = new byte[rijndael.IV.Length + encryptedData.Length + rijndael.Key.Length];
    Buffer.BlockCopy(rijndael.IV, 0, dataPacket, 0, rijndael.IV.Length);                    // IV: 16字节
    Buffer.BlockCopy(encryptedData, 0, dataPacket, 16, encryptedData.Length);              // 加密数据
    Buffer.BlockCopy(rijndael.Key, 0, dataPacket, 16 + encryptedData.Length, rijndael.Key.Length); // 密钥: 32字节
    
    // 4. 计算HMAC-MD5消息认证码
    byte[] hmac = new HMACMD5(rijndael.Key).ComputeHash(dataPacket);
    
    // 5. 构建最终数据包：[HMAC][数据包]
    byte[] finalPacket = new byte[hmac.Length + dataPacket.Length];
    Buffer.BlockCopy(hmac, 0, finalPacket, 0, hmac.Length);                                // HMAC: 16字节
    Buffer.BlockCopy(dataPacket, 0, finalPacket, hmac.Length, dataPacket.Length);
    
    return finalPacket;
}
```

**数据包结构分析：**
```
最终加密数据包格式：
┌─────────┬─────────┬─────────────┬─────────┐
│  HMAC   │   IV    │ 加密数据     │  密钥   │
│ 16字节  │ 16字节  │   变长      │ 32字节  │
└─────────┴─────────┴─────────────┴─────────┘
 0        16        32           32+数据长度
```

**AES加密实现：**
```csharp
private static byte[] EncryptString(string plainText, byte[] Key, byte[] IV)
{
    // 输入验证
    if (plainText == null || plainText.Length <= 0)
        throw new ArgumentNullException("plainText");
    if (Key == null || Key.Length <= 0)
        throw new ArgumentNullException("Key");
    if (IV == null || IV.Length <= 0)
        throw new ArgumentNullException("IV");
    
    MemoryStream memoryStream = null;
    RijndaelManaged rijndael = null;
    
    try
    {
        // 配置AES加密器
        rijndael = new RijndaelManaged();
        rijndael.Key = Key;  // 设置密钥
        rijndael.IV = IV;    // 设置初始化向量
        
        // 创建加密转换器
        ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);
        
        // 执行加密
        memoryStream = new MemoryStream();
        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        {
            using (StreamWriter writer = new StreamWriter(cryptoStream))
            {
                writer.Write(plainText);  // 写入明文数据
            }
        }
    }
    finally
    {
        // 清理资源
        if (rijndael != null)
        {
            rijndael.Clear();  // 清除密钥材料
        }
    }
    
    return memoryStream.ToArray();
}
```

#### 1.2 解密流程实现

**核心解密方法：**
```csharp
public static string Decrypt(byte[] ciek)
{
    // 1. 解析数据包各部分
    byte[] hmacReceived = new byte[16];      // 接收到的HMAC
    byte[] iv = new byte[16];                // 初始化向量
    byte[] encryptedData = new byte[ciek.Length - 64]; // 加密数据
    byte[] key = new byte[32];               // 密钥
    byte[] dataForVerify = new byte[ciek.Length - 16]; // 用于验证的数据
    
    // 2. 从数据包中提取各部分
    Buffer.BlockCopy(ciek, 0, hmacReceived, 0, 16);              // 提取HMAC
    Buffer.BlockCopy(ciek, 16, iv, 0, 16);                       // 提取IV
    Buffer.BlockCopy(ciek, 32, encryptedData, 0, ciek.Length - 64); // 提取加密数据
    Buffer.BlockCopy(ciek, ciek.Length - 32, key, 0, 32);        // 提取密钥
    Buffer.BlockCopy(ciek, 16, dataForVerify, 0, ciek.Length - 16); // 提取验证数据
    
    // 3. 验证数据完整性
    byte[] hmacCalculated = new HMACMD5(key).ComputeHash(dataForVerify);
    for (int i = 0; i < 16; i++)
    {
        if (hmacReceived[i] != hmacCalculated[i])
        {
            throw new ArgumentException("Checksum does not match");
        }
    }
    
    // 4. 执行AES解密
    RijndaelManaged rijndael = new RijndaelManaged();
    rijndael.IV = iv;
    rijndael.Key = key;
    return DecryptString(encryptedData, rijndael.Key, rijndael.IV);
}
```

**AES解密实现：**
```csharp
private static string DecryptString(byte[] cipherText, byte[] Key, byte[] IV)
{
    // 输入验证
    if (cipherText == null || cipherText.Length <= 0)
        throw new ArgumentNullException("cipherText");
    if (Key == null || Key.Length <= 0)
        throw new ArgumentNullException("Key");
    if (IV == null || IV.Length <= 0)
        throw new ArgumentNullException("IV");
    
    RijndaelManaged rijndael = null;
    string plaintext = null;
    
    try
    {
        // 配置AES解密器
        rijndael = new RijndaelManaged();
        rijndael.Key = Key;
        rijndael.IV = IV;
        
        // 创建解密转换器
        ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
        
        // 执行解密
        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    plaintext = srDecrypt.ReadToEnd();
                }
            }
        }
    }
    finally
    {
        // 清理资源
        if (rijndael != null)
        {
            rijndael.Clear();
        }
    }
    
    return plaintext;
}
```

#### 1.3 安全特性分析

**加密强度：**
1. **AES加密**: 使用工业标准的AES算法
2. **随机密钥**: 每次加密生成新的密钥和IV
3. **完整性保护**: HMAC-MD5确保数据未被篡改
4. **资源清理**: 加密完成后立即清理密钥材料

**安全考虑：**
- **密钥管理**: 密钥包含在数据包中，适用于数据混淆而非高安全场景
- **完整性验证**: 防止数据在传输或存储过程中被篡改
- **错误处理**: 验证失败时抛出明确异常

**应用场景：**
```csharp
// 游戏存档加密
public void SaveGameData(GameSaveData saveData)
{
    string jsonData = JsonUtility.ToJson(saveData);
    byte[] encryptedData = Encryption.Encrypt(jsonData);
    File.WriteAllBytes(saveFilePath, encryptedData);
}

// 游戏存档解密
public GameSaveData LoadGameData()
{
    byte[] encryptedData = File.ReadAllBytes(saveFilePath);
    string jsonData = Encryption.Decrypt(encryptedData);
    return JsonUtility.FromJson<GameSaveData>(jsonData);
}

// 网络数据保护
public void SendSecureMessage(string message)
{
    byte[] encryptedMessage = Encryption.Encrypt(message);
    networkManager.SendData(encryptedMessage);
}
```

### 2. Serializable.cs - 序列化接口定义

**功能概述：**
定义游戏对象序列化的标准接口，提供统一的二进制读写规范。

**完整接口定义：**
```csharp
public interface Serializable
{
    void WriteTo(BinaryWriter writer);   // 序列化：将对象写入二进制流
    void ReadFrom(BinaryReader reader);  // 反序列化：从二进制流读取对象
}
```

**接口设计原则：**
1. **双向对称**: WriteTo和ReadFrom必须严格对应
2. **类型安全**: 使用强类型的BinaryWriter/BinaryReader
3. **顺序一致**: 写入和读取的顺序必须完全一致
4. **版本兼容**: 支持数据格式的向后兼容

**实现示例：**

#### 2.1 基础数据类型实现

**简单游戏对象：**
```csharp
public class GameSettings : Serializable
{
    public float musicVolume;
    public float sfxVolume;
    public int graphicsQuality;
    public bool fullScreen;
    public string playerName;
    
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(musicVolume);      // float: 4字节
        writer.Write(sfxVolume);        // float: 4字节
        writer.Write(graphicsQuality);  // int: 4字节
        writer.Write(fullScreen);       // bool: 1字节
        writer.Write(playerName ?? ""); // string: 长度前缀
    }
    
    public void ReadFrom(BinaryReader reader)
    {
        musicVolume = reader.ReadSingle();
        sfxVolume = reader.ReadSingle();
        graphicsQuality = reader.ReadInt32();
        fullScreen = reader.ReadBoolean();
        playerName = reader.ReadString();
    }
}
```

#### 2.2 复杂数据结构实现

**卡丁车数据：**
```csharp
public class KartData : Serializable
{
    public int kartId;
    public Vector3 position;
    public Quaternion rotation;
    public float speed;
    public List<int> equippedItems;
    public Dictionary<string, float> stats;
    
    public void WriteTo(BinaryWriter writer)
    {
        // 基础数据
        writer.Write(kartId);
        
        // Unity类型（使用Serializer）
        Serializer.Write(writer, position);
        Serializer.Write(writer, rotation);
        
        writer.Write(speed);
        
        // 集合类型
        writer.Write(equippedItems.Count);
        foreach (int item in equippedItems)
        {
            writer.Write(item);
        }
        
        // 字典类型
        writer.Write(stats.Count);
        foreach (var kvp in stats)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value);
        }
    }
    
    public void ReadFrom(BinaryReader reader)
    {
        // 基础数据
        kartId = reader.ReadInt32();
        
        // Unity类型
        position = Serializer.ReadVector3(reader);
        rotation = Serializer.ReadQuaternion(reader);
        
        speed = reader.ReadSingle();
        
        // 集合类型
        int itemCount = reader.ReadInt32();
        equippedItems = new List<int>(itemCount);
        for (int i = 0; i < itemCount; i++)
        {
            equippedItems.Add(reader.ReadInt32());
        }
        
        // 字典类型
        int statsCount = reader.ReadInt32();
        stats = new Dictionary<string, float>(statsCount);
        for (int i = 0; i < statsCount; i++)
        {
            string key = reader.ReadString();
            float value = reader.ReadSingle();
            stats[key] = value;
        }
    }
}
```

#### 2.3 版本控制实现

**支持版本升级的序列化：**
```csharp
public class VersionedGameData : Serializable
{
    private const int CURRENT_VERSION = 2;
    
    public int version = CURRENT_VERSION;
    public string playerName;
    public int level;
    public float experience;
    public List<string> achievements; // 版本2新增
    
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(version);
        writer.Write(playerName ?? "");
        writer.Write(level);
        writer.Write(experience);
        
        // 版本2的新功能
        if (version >= 2)
        {
            writer.Write(achievements?.Count ?? 0);
            if (achievements != null)
            {
                foreach (string achievement in achievements)
                {
                    writer.Write(achievement);
                }
            }
        }
    }
    
    public void ReadFrom(BinaryReader reader)
    {
        version = reader.ReadInt32();
        playerName = reader.ReadString();
        level = reader.ReadInt32();
        experience = reader.ReadSingle();
        
        // 向后兼容处理
        if (version >= 2)
        {
            int achievementCount = reader.ReadInt32();
            achievements = new List<string>(achievementCount);
            for (int i = 0; i < achievementCount; i++)
            {
                achievements.Add(reader.ReadString());
            }
        }
        else
        {
            achievements = new List<string>(); // 默认空列表
        }
        
        // 升级到当前版本
        version = CURRENT_VERSION;
    }
}
```

#### 2.4 序列化工具类

**批量序列化管理：**
```csharp
public static class SerializationHelper
{
    public static byte[] SerializeObject(Serializable obj)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                obj.WriteTo(writer);
            }
            return stream.ToArray();
        }
    }
    
    public static T DeserializeObject<T>(byte[] data) where T : Serializable, new()
    {
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                T obj = new T();
                obj.ReadFrom(reader);
                return obj;
            }
        }
    }
    
    public static void SaveToFile(string filePath, Serializable obj, bool encrypt = false)
    {
        byte[] data = SerializeObject(obj);
        
        if (encrypt)
        {
            string base64Data = Convert.ToBase64String(data);
            data = Encryption.Encrypt(base64Data);
        }
        
        File.WriteAllBytes(filePath, data);
    }
    
    public static T LoadFromFile<T>(string filePath, bool encrypted = false) where T : Serializable, new()
    {
        byte[] data = File.ReadAllBytes(filePath);
        
        if (encrypted)
        {
            string base64Data = Encryption.Decrypt(data);
            data = Convert.FromBase64String(base64Data);
        }
        
        return DeserializeObject<T>(data);
    }
}
```

### 3. Serializer.cs - Unity数据类型序列化器

**功能概述：**
提供Unity引擎特定数据类型（Vector3、Quaternion等）的标准化序列化支持。

**完整实现：**
```csharp
public class Serializer
{
    // Vector3序列化
    public static Vector3 ReadVector3(BinaryReader reader)
    {
        return new Vector3(
            reader.ReadSingle(),  // x坐标
            reader.ReadSingle(),  // y坐标
            reader.ReadSingle()   // z坐标
        );
    }
    
    public static void Write(BinaryWriter writer, Vector3 v)
    {
        writer.Write(v.x);  // 写入x坐标 (4字节)
        writer.Write(v.y);  // 写入y坐标 (4字节)
        writer.Write(v.z);  // 写入z坐标 (4字节)
    }
    
    // Quaternion序列化
    public static Quaternion ReadQuaternion(BinaryReader reader)
    {
        return new Quaternion(
            reader.ReadSingle(),  // x分量
            reader.ReadSingle(),  // y分量
            reader.ReadSingle(),  // z分量
            reader.ReadSingle()   // w分量
        );
    }
    
    public static void Write(BinaryWriter writer, Quaternion q)
    {
        writer.Write(q.x);  // 写入x分量 (4字节)
        writer.Write(q.y);  // 写入y分量 (4字节)
        writer.Write(q.z);  // 写入z分量 (4字节)
        writer.Write(q.w);  // 写入w分量 (4字节)
    }
}
```

**扩展支持的数据类型：**

#### 3.1 更多Unity类型支持

**Vector2和Vector4：**
```csharp
public static class SerializerExtended
{
    // Vector2支持
    public static Vector2 ReadVector2(BinaryReader reader)
    {
        return new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
    
    public static void Write(BinaryWriter writer, Vector2 v)
    {
        writer.Write(v.x);
        writer.Write(v.y);
    }
    
    // Vector4支持
    public static Vector4 ReadVector4(BinaryReader reader)
    {
        return new Vector4(
            reader.ReadSingle(),
            reader.ReadSingle(), 
            reader.ReadSingle(),
            reader.ReadSingle()
        );
    }
    
    public static void Write(BinaryWriter writer, Vector4 v)
    {
        writer.Write(v.x);
        writer.Write(v.y);
        writer.Write(v.z);
        writer.Write(v.w);
    }
    
    // Color支持
    public static Color ReadColor(BinaryReader reader)
    {
        return new Color(
            reader.ReadSingle(),  // r
            reader.ReadSingle(),  // g
            reader.ReadSingle(),  // b
            reader.ReadSingle()   // a
        );
    }
    
    public static void Write(BinaryWriter writer, Color c)
    {
        writer.Write(c.r);
        writer.Write(c.g);
        writer.Write(c.b);
        writer.Write(c.a);
    }
    
    // Matrix4x4支持
    public static Matrix4x4 ReadMatrix4x4(BinaryReader reader)
    {
        Matrix4x4 matrix = new Matrix4x4();
        for (int i = 0; i < 16; i++)
        {
            matrix[i] = reader.ReadSingle();
        }
        return matrix;
    }
    
    public static void Write(BinaryWriter writer, Matrix4x4 matrix)
    {
        for (int i = 0; i < 16; i++)
        {
            writer.Write(matrix[i]);
        }
    }
}
```

#### 3.2 数组和集合序列化

**通用数组序列化：**
```csharp
public static class CollectionSerializer
{
    // Vector3数组
    public static Vector3[] ReadVector3Array(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        Vector3[] array = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = Serializer.ReadVector3(reader);
        }
        return array;
    }
    
    public static void Write(BinaryWriter writer, Vector3[] array)
    {
        writer.Write(array.Length);
        foreach (Vector3 v in array)
        {
            Serializer.Write(writer, v);
        }
    }
    
    // Quaternion列表
    public static List<Quaternion> ReadQuaternionList(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        List<Quaternion> list = new List<Quaternion>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(Serializer.ReadQuaternion(reader));
        }
        return list;
    }
    
    public static void Write(BinaryWriter writer, List<Quaternion> list)
    {
        writer.Write(list.Count);
        foreach (Quaternion q in list)
        {
            Serializer.Write(writer, q);
        }
    }
}
```

## 综合应用示例

### 1. 游戏存档系统

**完整的存档管理器：**
```csharp
public class SaveGameManager
{
    private const string SAVE_FILE_NAME = "gamesave.dat";
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    
    public void SaveGame(GameSaveData saveData)
    {
        try
        {
            // 序列化游戏数据
            byte[] serializedData = SerializationHelper.SerializeObject(saveData);
            
            // 加密保护
            string base64Data = Convert.ToBase64String(serializedData);
            byte[] encryptedData = Encryption.Encrypt(base64Data);
            
            // 写入文件
            File.WriteAllBytes(SaveFilePath, encryptedData);
            
            Debug.Log("Game saved successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save game: {ex.Message}");
        }
    }
    
    public GameSaveData LoadGame()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                return CreateDefaultSaveData();
            }
            
            // 读取加密数据
            byte[] encryptedData = File.ReadAllBytes(SaveFilePath);
            
            // 解密
            string base64Data = Encryption.Decrypt(encryptedData);
            byte[] serializedData = Convert.FromBase64String(base64Data);
            
            // 反序列化
            GameSaveData saveData = SerializationHelper.DeserializeObject<GameSaveData>(serializedData);
            
            Debug.Log("Game loaded successfully");
            return saveData;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load game: {ex.Message}");
            return CreateDefaultSaveData();
        }
    }
    
    private GameSaveData CreateDefaultSaveData()
    {
        return new GameSaveData
        {
            playerName = "Player",
            level = 1,
            experience = 0,
            currentKart = 0,
            unlockedKarts = new List<int> { 0 },
            gameSettings = new GameSettings
            {
                musicVolume = 0.8f,
                sfxVolume = 0.9f,
                graphicsQuality = 1,
                fullScreen = true
            }
        };
    }
}

[System.Serializable]
public class GameSaveData : Serializable
{
    public string playerName;
    public int level;
    public float experience;
    public int currentKart;
    public List<int> unlockedKarts;
    public GameSettings gameSettings;
    public Vector3 lastPosition;
    public DateTime lastPlayTime;
    
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(playerName ?? "");
        writer.Write(level);
        writer.Write(experience);
        writer.Write(currentKart);
        
        writer.Write(unlockedKarts.Count);
        foreach (int kartId in unlockedKarts)
        {
            writer.Write(kartId);
        }
        
        gameSettings.WriteTo(writer);
        Serializer.Write(writer, lastPosition);
        writer.Write(lastPlayTime.ToBinary());
    }
    
    public void ReadFrom(BinaryReader reader)
    {
        playerName = reader.ReadString();
        level = reader.ReadInt32();
        experience = reader.ReadSingle();
        currentKart = reader.ReadInt32();
        
        int kartCount = reader.ReadInt32();
        unlockedKarts = new List<int>(kartCount);
        for (int i = 0; i < kartCount; i++)
        {
            unlockedKarts.Add(reader.ReadInt32());
        }
        
        gameSettings = new GameSettings();
        gameSettings.ReadFrom(reader);
        lastPosition = Serializer.ReadVector3(reader);
        lastPlayTime = DateTime.FromBinary(reader.ReadInt64());
    }
}
```

### 2. 网络数据传输

**安全的网络消息系统：**
```csharp
public class NetworkMessage : Serializable
{
    public enum MessageType
    {
        PlayerUpdate,
        GameEvent,
        ChatMessage
    }
    
    public MessageType type;
    public int senderId;
    public float timestamp;
    public byte[] payload;
    
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write((int)type);
        writer.Write(senderId);
        writer.Write(timestamp);
        writer.Write(payload?.Length ?? 0);
        if (payload != null)
        {
            writer.Write(payload);
        }
    }
    
    public void ReadFrom(BinaryReader reader)
    {
        type = (MessageType)reader.ReadInt32();
        senderId = reader.ReadInt32();
        timestamp = reader.ReadSingle();
        int payloadLength = reader.ReadInt32();
        if (payloadLength > 0)
        {
            payload = reader.ReadBytes(payloadLength);
        }
    }
}

public class SecureNetworkManager
{
    public void SendSecureMessage(NetworkMessage message)
    {
        try
        {
            // 序列化消息
            byte[] messageData = SerializationHelper.SerializeObject(message);
            
            // 加密传输
            byte[] encryptedData = Encryption.Encrypt(Convert.ToBase64String(messageData));
            
            // 发送到网络
            networkLayer.SendData(encryptedData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to send secure message: {ex.Message}");
        }
    }
    
    public NetworkMessage ReceiveSecureMessage(byte[] encryptedData)
    {
        try
        {
            // 解密数据
            string base64Data = Encryption.Decrypt(encryptedData);
            byte[] messageData = Convert.FromBase64String(base64Data);
            
            // 反序列化消息
            return SerializationHelper.DeserializeObject<NetworkMessage>(messageData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to receive secure message: {ex.Message}");
            return null;
        }
    }
}
```

### 3. 配置文件管理

**游戏配置序列化：**
```csharp
public class ConfigurationManager
{
    private const string CONFIG_FILE = "game_config.dat";
    private string ConfigPath => Path.Combine(Application.streamingAssetsPath, CONFIG_FILE);
    
    public GameConfiguration LoadConfiguration()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                return SerializationHelper.LoadFromFile<GameConfiguration>(ConfigPath, encrypted: true);
            }
            else
            {
                var defaultConfig = GameConfiguration.CreateDefault();
                SaveConfiguration(defaultConfig);
                return defaultConfig;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load configuration: {ex.Message}");
            return GameConfiguration.CreateDefault();
        }
    }
    
    public void SaveConfiguration(GameConfiguration config)
    {
        try
        {
            SerializationHelper.SaveToFile(ConfigPath, config, encrypt: true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save configuration: {ex.Message}");
        }
    }
}

public class GameConfiguration : Serializable
{
    public struct KartSettings
    {
        public float maxSpeed;
        public float acceleration;
        public float handling;
    }
    
    public Dictionary<int, KartSettings> kartConfigs;
    public float[] trackDifficulties;
    public string[] availableLanguages;
    public GameSettings defaultSettings;
    
    public void WriteTo(BinaryWriter writer)
    {
        // 卡丁车配置
        writer.Write(kartConfigs.Count);
        foreach (var kvp in kartConfigs)
        {
            writer.Write(kvp.Key);
            writer.Write(kvp.Value.maxSpeed);
            writer.Write(kvp.Value.acceleration);
            writer.Write(kvp.Value.handling);
        }
        
        // 赛道难度
        writer.Write(trackDifficulties.Length);
        foreach (float difficulty in trackDifficulties)
        {
            writer.Write(difficulty);
        }
        
        // 可用语言
        writer.Write(availableLanguages.Length);
        foreach (string language in availableLanguages)
        {
            writer.Write(language);
        }
        
        // 默认设置
        defaultSettings.WriteTo(writer);
    }
    
    public void ReadFrom(BinaryReader reader)
    {
        // 卡丁车配置
        int kartCount = reader.ReadInt32();
        kartConfigs = new Dictionary<int, KartSettings>(kartCount);
        for (int i = 0; i < kartCount; i++)
        {
            int kartId = reader.ReadInt32();
            KartSettings settings = new KartSettings
            {
                maxSpeed = reader.ReadSingle(),
                acceleration = reader.ReadSingle(),
                handling = reader.ReadSingle()
            };
            kartConfigs[kartId] = settings;
        }
        
        // 赛道难度
        int trackCount = reader.ReadInt32();
        trackDifficulties = new float[trackCount];
        for (int i = 0; i < trackCount; i++)
        {
            trackDifficulties[i] = reader.ReadSingle();
        }
        
        // 可用语言
        int languageCount = reader.ReadInt32();
        availableLanguages = new string[languageCount];
        for (int i = 0; i < languageCount; i++)
        {
            availableLanguages[i] = reader.ReadString();
        }
        
        // 默认设置
        defaultSettings = new GameSettings();
        defaultSettings.ReadFrom(reader);
    }
    
    public static GameConfiguration CreateDefault()
    {
        return new GameConfiguration
        {
            kartConfigs = new Dictionary<int, KartSettings>
            {
                { 0, new KartSettings { maxSpeed = 1.0f, acceleration = 1.0f, handling = 1.0f } },
                { 1, new KartSettings { maxSpeed = 1.2f, acceleration = 0.8f, handling = 0.9f } }
            },
            trackDifficulties = new float[] { 1.0f, 1.2f, 1.5f, 1.8f, 2.0f },
            availableLanguages = new string[] { "en", "ko", "ja", "zh" },
            defaultSettings = new GameSettings
            {
                musicVolume = 0.8f,
                sfxVolume = 0.9f,
                graphicsQuality = 1,
                fullScreen = true,
                playerName = "Player"
            }
        };
    }
}
```

## 性能优化和最佳实践

### 1. 内存管理

**序列化缓冲池：**
```csharp
public static class SerializationPool
{
    private static readonly Stack<MemoryStream> streamPool = new Stack<MemoryStream>();
    private static readonly Stack<BinaryWriter> writerPool = new Stack<BinaryWriter>();
    private static readonly Stack<BinaryReader> readerPool = new Stack<BinaryReader>();
    
    public static MemoryStream GetMemoryStream()
    {
        if (streamPool.Count > 0)
        {
            var stream = streamPool.Pop();
            stream.SetLength(0);
            stream.Position = 0;
            return stream;
        }
        return new MemoryStream();
    }
    
    public static void ReturnMemoryStream(MemoryStream stream)
    {
        if (stream != null && stream.Length < 1024 * 1024) // 限制缓存大小
        {
            streamPool.Push(stream);
        }
    }
    
    public static byte[] SerializeWithPool<T>(T obj) where T : Serializable
    {
        MemoryStream stream = GetMemoryStream();
        try
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                obj.WriteTo(writer);
            }
            return stream.ToArray();
        }
        finally
        {
            ReturnMemoryStream(stream);
        }
    }
}
```

### 2. 异步序列化

**异步文件操作：**
```csharp
public class AsyncSerializationManager
{
    public async Task SaveAsync<T>(string filePath, T obj, bool encrypt = false) where T : Serializable
    {
        await Task.Run(() =>
        {
            byte[] data = SerializationPool.SerializeWithPool(obj);
            
            if (encrypt)
            {
                string base64Data = Convert.ToBase64String(data);
                data = Encryption.Encrypt(base64Data);
            }
            
            File.WriteAllBytes(filePath, data);
        });
    }
    
    public async Task<T> LoadAsync<T>(string filePath, bool encrypted = false) where T : Serializable, new()
    {
        return await Task.Run(() =>
        {
            byte[] data = File.ReadAllBytes(filePath);
            
            if (encrypted)
            {
                string base64Data = Encryption.Decrypt(data);
                data = Convert.FromBase64String(base64Data);
            }
            
            return SerializationHelper.DeserializeObject<T>(data);
        });
    }
}
```

## 安全性和错误处理

### 1. 数据验证

**安全的反序列化：**
```csharp
public static class SafeSerializer
{
    private const int MAX_STRING_LENGTH = 10000;
    private const int MAX_ARRAY_LENGTH = 100000;
    
    public static string ReadSafeString(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        if (length < 0 || length > MAX_STRING_LENGTH)
        {
            throw new InvalidDataException($"String length {length} exceeds maximum allowed {MAX_STRING_LENGTH}");
        }
        
        return length == 0 ? string.Empty : reader.ReadString();
    }
    
    public static T[] ReadSafeArray<T>(BinaryReader reader, Func<BinaryReader, T> readElement)
    {
        int count = reader.ReadInt32();
        if (count < 0 || count > MAX_ARRAY_LENGTH)
        {
            throw new InvalidDataException($"Array length {count} exceeds maximum allowed {MAX_ARRAY_LENGTH}");
        }
        
        T[] array = new T[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = readElement(reader);
        }
        return array;
    }
}
```

### 2. 错误恢复

**容错序列化系统：**
```csharp
public class FaultTolerantSerializer
{
    public static bool TrySerialize<T>(T obj, out byte[] result) where T : Serializable
    {
        result = null;
        try
        {
            result = SerializationHelper.SerializeObject(obj);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Serialization failed: {ex.Message}");
            return false;
        }
    }
    
    public static bool TryDeserialize<T>(byte[] data, out T result) where T : Serializable, new()
    {
        result = default(T);
        try
        {
            result = SerializationHelper.DeserializeObject<T>(data);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Deserialization failed: {ex.Message}");
            return false;
        }
    }
}
```

## 总结

Serialization模块为卡丁车游戏提供了完整的数据序列化和安全保护解决方案：

### 核心特性
1. **安全加密**: 基于AES的强加密算法和HMAC完整性验证
2. **统一接口**: 标准化的序列化接口，确保一致性
3. **Unity支持**: 专门针对Unity引擎数据类型优化
4. **版本兼容**: 支持数据格式的向前兼容

### 设计优势
1. **安全性**: 多层安全保护，防止数据泄露和篡改
2. **性能**: 高效的二进制序列化，最小化存储空间
3. **可扩展**: 易于添加新的数据类型支持
4. **容错性**: 完善的错误处理和恢复机制

### 应用价值
1. **数据持久化**: 安全可靠的游戏存档系统
2. **网络传输**: 加密的网络数据保护
3. **配置管理**: 灵活的配置文件处理
4. **开发效率**: 统一的序列化标准，简化开发

该模块是游戏数据管理的核心基础设施，为整个游戏系统的数据安全和可靠性提供了强有力的保障。