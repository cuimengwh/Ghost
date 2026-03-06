using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Utopia.Core.Services;
using Utopia.Core;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Utopia.Data
{
    /// <summary>
    /// 数据服务接口 — 定义数据层的契约
    /// 用于抽象数据保存和加载操作，便于依赖注入和单元测试
    /// </summary>
    public interface IDataService
    {
        // 异步保存泛型数据到指定键
        Task SaveDataAsync<T>(string key, T data) where T : class;
        // 异步从指定键加载泛型数据  
        Task<T> LoadDataAsync<T>(string key) where T : class;
        // 删除指定键的数据
        void DeleteData(string key);
        // 检查指定键是否存在数据
        bool HasData(string key);
    }

    /// <summary>
    /// 数据管理器实现。
    /// 负责磁盘IO、序列化、加密。
    /// </summary>
    public class DataManager : MonoBehaviour, IDataService
    {
        [Header("配置")]
        [SerializeField]
        private string _fileName = "save_data.json";              // 保存文件名
        [SerializeField]
        private bool _useEncryption = true;                       // 是否启用加密
        [SerializeField]
        private string _encryptionKey = "SoulFarm_Secret_2025";   // 加密密钥

        private string _savePath; // 完整的保存路径

        // 内存缓存字典，避免频繁IO操作
        private Dictionary<string, object> _runtimeCache = new Dictionary<string, object>();

        // JSON序列化配置设置
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            // 格式化输出，带缩进便于阅读
            Formatting = Formatting.Indented,
            // 忽略循环引用（防止无限递归）
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            // 包含类型信息，支持多态序列化（子类类型信息会保存在JSON中）
            TypeNameHandling = TypeNameHandling.Auto
        };

        private void Awake()
        {
            if (ServiceLocatorProvider.Global != null)
            {
                ServiceLocatorProvider.Global.Locator.Register<IDataService>(this);
                Debug.Log("[DataManager] 服务已注册。");
            }

            // 构建保存路径：Application.persistentDataPath是Unity的持久化数据路径
            _savePath = Path.Combine(Application.persistentDataPath, "Save");
            // 如果目录不存在则创建
            if (!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath);

            // 使该GameObject在场景切换时不销毁
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            // 防止游戏退出时Global已被销毁导致的空引用报错
            if (ServiceLocatorProvider.Global != null)
            {
                // 从服务定位器中注销服务
                ServiceLocatorProvider.Global.Locator.Unregister<IDataService>();
            }
        }

        /// <summary>
        /// 异步保存数据文件
        /// </summary>
        /// <typeparam name="T">数据类型，必须是类</typeparam>
        /// <param name="key">数据标识键，用作文件名</param>
        /// <param name="data">要保存的数据对象</param>
        /// <returns>异步任务</returns>
        public async Task SaveDataAsync<T>(string key, T data) where T : class
        {
            // 1、更新内存缓存
            _runtimeCache[key] = data;

            // 2、构建完整的文件路径（添加.json扩展名）
            string fullPath = Path.Combine(_savePath, key + ".json");

            try
            {
                // 3、使用Task.Run在后台线程执行IO操作，避免阻塞主线程
                await Task.Run(() =>
                {
                    // 3.1、将数据对象序列化为JSON字符串
                    string json = JsonConvert.SerializeObject(data, _jsonSettings);

                    // 3.2、将JSON字符串转换为UTF-8编码的字节数组
                    byte[] bytes = Encoding.UTF8.GetBytes(json);

                    // 3.3、如果启用了加密，对字节数组进行加密
                    if (_useEncryption)
                    {
                        bytes = XOREncrypt(bytes, _encryptionKey);
                    }

                    // 4、使用"写入临时文件再重命名"的模式，确保数据写入的原子性和安全性
                    string tempPath = fullPath + ".tmp"; // 临时文件路径

                    // 4.1、将加密后的字节数组写入临时文件
                    File.WriteAllBytes(tempPath, bytes);

                    // 如果目标文件已存在，先删除（File.Move要求目标不存在）
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }

                    // 4.2、将临时文件重命名为目标文件（原子操作）
                    File.Move(tempPath, fullPath);
                });

                Debug.Log($"[DataManager] 保存成功 :{key}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 保存失败 {key}: {e.Message}");
                // 重新抛出异常，允许上层处理
                throw new Exception($"保存数据失败: {key}", e);
            }
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <typeparam name="T">期望的数据类型</typeparam>
        /// <param name="key">数据标识键</param>
        /// <returns>加载的数据对象，不存在时返回null</returns>
        public async Task<T> LoadDataAsync<T>(string key) where T : class
        {
            // 1、优先从内存缓存中提取数据
            if (_runtimeCache.TryGetValue(key, out object data))
            {
                // 检查类型是否匹配
                if (data is T tData)
                {
                    return tData;
                }
            }

            // 2、构建完整文件路径
            string fullPath = Path.Combine(_savePath, key + ".json");

            // 3、检查文件是否存在
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[DataManager] 存档不存在: {key}");
                return null;
            }

            try
            {
                // 4、在后台线程执行IO和反序列化操作
                return await Task.Run(() =>
                {
                    // 4.1、读取文件的字节数据
                    byte[] bytes = File.ReadAllBytes(fullPath);

                    // 4.2、如果启用了加密，进行解密
                    if (_useEncryption)
                    {
                        bytes = XOREncrypt(bytes, _encryptionKey);
                    }

                    // 4.3、将字节数组转换为JSON字符串
                    string json = Encoding.UTF8.GetString(bytes);

                    // 4.4、将JSON字符串反序列化为对象
                    T data = JsonConvert.DeserializeObject<T>(json, _jsonSettings);

                    // 4.5、使用锁确保线程安全，更新内存缓存
                    lock (_runtimeCache)
                    {
                        _runtimeCache[key] = data;
                    }

                    return data;
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataManager] 加载失败 {key}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 删除指定键的数据
        /// </summary>
        /// <param name="key">数据标识键</param>
        public void DeleteData(string key)
        {
            // 1、从内存缓存中移除
            if (_runtimeCache.ContainsKey(key))
                _runtimeCache.Remove(key);

            // 2、构建文件路径并删除物理文件
            string fullPath = Path.Combine(_savePath, key + ".json");
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        /// <summary>
        /// 检查指定键是否存在数据
        /// </summary>
        /// <param name="key">数据标识键</param>
        /// <returns>是否存在数据</returns>
        public bool HasData(string key)
        {
            // 1、先检查内存缓存
            if (_runtimeCache.ContainsKey(key))
                return true;

            // 2、再检查物理文件
            return File.Exists(Path.Combine(_savePath, key + ".json"));
        }

        /// <summary>
        /// XOR异或加密/解密算法
        /// 注意：这是简单的加密，不适合高安全需求
        /// </summary>
        /// <param name="data">要加密/解密的数据</param>
        /// <param name="key">加密密钥</param>
        /// <returns>处理后的字节数组</returns>
        private byte[] XOREncrypt(byte[] data, string key)
        {
            byte[] result = new byte[data.Length];
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            // 遍历每个字节，与密钥字节进行异或操作
            for (int i = 0; i < data.Length; i++)
            {
                // 使用密钥字节循环（i % keyBytes.Length）
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return result;
        }
    }
}