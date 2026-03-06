//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Threading.Tasks; // 引入任务库用于异步操作
//using UnityEngine;
//using Newtonsoft.Json; // 引入强大的 Json 库
//using Utopia.Data.NPC;

//namespace Utopia.Data
//{
//    /// <summary>
//    /// 数据管理器 (优化版)
//    /// 1. 使用 Newtonsoft.Json 支持 Dictionary 序列化
//    /// 2. 使用异步 IO 避免保存时游戏卡顿
//    /// 3. 优化加密算法减少 GC
//    /// </summary>
//    public class DataManager : MonoBehaviour, IDataService
//    {
//        [Header("数据配置")]
//        [SerializeField] private string saveFileName = "UtopiaSave.json"; // 改为json后缀更直观
//        [SerializeField] private bool enableEncryption = true;
//        [SerializeField] private string encryptionKey = "UtopiaEncryptionKey2024";

//        // Newtonsoft Json 设置
//        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
//        {
//            Formatting = Formatting.Indented, // 格式化输出，方便调试（发布时可改为 None 减小体积）
//            ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 忽略循环引用
//            TypeNameHandling = TypeNameHandling.Auto // 自动处理子类/多态
//        };

//        // 数据存储
//        private Dictionary<string, ScriptableObject> scriptableObjects = new Dictionary<string, ScriptableObject>();
//        private Dictionary<Type, object> dataContainers = new Dictionary<Type, object>();

//        // 缓存数据
//        private Dictionary<string, NPCData> npcDataCache = new Dictionary<string, NPCData>();
//        private Dictionary<string, Quest> questDataCache = new Dictionary<string, Quest>();

//        // 当前游戏数据
//        private GameData currentGameData;
//        public GameData CurrentGameData => currentGameData;

//        // 状态标记
//        private bool _isSaving = false; // 防止保存操作重叠

//        // 资源路径
//        private const string NPC_DATA_PATH = "Data/NPCs";

//        public event Action<GameData> OnGameDataLoaded;
//        public event Action<GameData> OnGameDataSaved;
//        public event Action OnGameDataDeleted;

//        private void Awake()
//        {
//            ServiceLocator.Register<IDataService>(this);
//            InitializeDataManager();
//        }

//        private void OnDestroy()
//        {
//            ServiceLocator.Unregister<IDataService>(this);
//            // 游戏退出/销毁时，强制使用同步保存，防止异步线程被系统杀掉导致坏档
//            SaveGameData(forceSync: true);
//        }

//        private void InitializeDataManager()
//        {
//            LoadAllScriptableObjects();
//            // 先尝试加载，如果没有存档内部会自动创建新数据
//            LoadGameData();
//        }

//        private void LoadAllScriptableObjects()
//        {
//            // (保持原有的加载逻辑不变)
//            NPCData[] npcAssets = Resources.LoadAll<NPCData>(NPC_DATA_PATH);
//            foreach (NPCData npc in npcAssets)
//            {
//                if (!string.IsNullOrEmpty(npc.npcId) && !npcDataCache.ContainsKey(npc.npcId))
//                {
//                    npcDataCache.Add(npc.npcId, npc);
//                    scriptableObjects.Add($"NPCData_{npc.npcId}", npc);
//                }
//            }
//        }

//        #region 核心保存与加载逻辑 (重构部分)

//        /// <summary>
//        /// 保存游戏数据
//        /// </summary>
//        /// <param name="forceSync">是否强制同步保存（用于程序退出时）</param>
//        public async void SaveGameData(bool forceSync = false)
//        {
//            if (currentGameData == null) return;

//            // 如果正在保存且不是强制保存，则跳过本次请求
//            if (_isSaving && !forceSync) return;

//            _isSaving = true;
//            currentGameData.LastSaveTime = DateTime.Now;

//            try
//            {
//                // 1. 序列化 (必须在主线程完成，因为访问Unity对象不线程安全)
//                string json = JsonConvert.SerializeObject(currentGameData, _jsonSettings);

//                // 准备写入参数
//                string filePath = GetSaveFilePath();
//                string key = encryptionKey;
//                bool encrypt = enableEncryption;

//                // 定义写入操作
//                Action writeAction = () =>
//                {
//                    byte[] bytes = Encoding.UTF8.GetBytes(json);

//                    if (encrypt)
//                    {
//                        bytes = XOREncryptDecrypt(bytes, key);
//                    }

//                    // 使用原子写入防止写入中断导致文件损坏（可选优化：先写临时文件再Move）
//                    File.WriteAllBytes(filePath, bytes);
//                };

//                if (forceSync)
//                {
//                    // 同步执行
//                    writeAction.Invoke();
//                    Debug.Log($"[DataManager] Game saved (Sync): {filePath}");
//                }
//                else
//                {
//                    // 2. 异步执行 IO 和 加密 (避免卡顿)
//                    await Task.Run(writeAction);
//                    Debug.Log($"[DataManager] Game saved (Async): {filePath}");
//                }

//                // 回到主线程触发事件
//                OnGameDataSaved?.Invoke(currentGameData);
//            }
//            catch (Exception e)
//            {
//                Debug.LogError($"[DataManager] Save failed: {e.Message}");
//            }
//            finally
//            {
//                _isSaving = false;
//            }
//        }

//        // 为了兼容接口，提供无参版本
//        public void SaveGameData() => SaveGameData(false);

//        /// <summary>
//        /// 加载游戏数据
//        /// </summary>
//        public void LoadGameData()
//        {
//            string filePath = GetSaveFilePath();

//            if (!File.Exists(filePath))
//            {
//                Debug.Log("[DataManager] No save found, creating new.");
//                CreateNewGameData();
//                OnGameDataLoaded?.Invoke(currentGameData);
//                return;
//            }

//            try
//            {
//                // 读取字节流
//                byte[] fileBytes = File.ReadAllBytes(filePath);

//                // 解密
//                if (enableEncryption)
//                {
//                    fileBytes = XOREncryptDecrypt(fileBytes, encryptionKey);
//                }

//                // 转换为字符串
//                string json = Encoding.UTF8.GetString(fileBytes);

//                // 反序列化 (Newtonsoft 支持 Dictionary)
//                currentGameData = JsonConvert.DeserializeObject<GameData>(json, _jsonSettings);

//                if (currentGameData == null) throw new Exception("Data resulted in null");

//                // 数据完整性检查
//                EnsureAllNPCStates();

//                Debug.Log($"[DataManager] Data loaded: {filePath}");
//                OnGameDataLoaded?.Invoke(currentGameData);
//            }
//            catch (Exception e)
//            {
//                Debug.LogError($"[DataManager] Load failed (Corrupted?): {e.Message}");

//                // 备份损坏的存档，防止玩家数据彻底丢失
//                try
//                {
//                    File.Copy(filePath, filePath + ".corrupted", true);
//                    Debug.LogWarning("Corrupted save file backed up.");
//                }
//                catch { }

//                // 创建新游戏兜底
//                CreateNewGameData();
//                OnGameDataLoaded?.Invoke(currentGameData);
//            }
//        }

//        /// <summary>
//        /// 优化的字节级加密 (减少 String GC)
//        /// </summary>
//        private byte[] XOREncryptDecrypt(byte[] data, string key)
//        {
//            byte[] result = new byte[data.Length];
//            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
//            int keyLen = keyBytes.Length;

//            for (int i = 0; i < data.Length; i++)
//            {
//                result[i] = (byte)(data[i] ^ keyBytes[i % keyLen]);
//            }
//            return result;
//        }

//        #endregion

//        private void CreateNewGameData()
//        {
//            currentGameData = new GameData();
//            // 确保字典被实例化，防止空引用
//            currentGameData.NPCStates = new Dictionary<string, NPCStateData>();
//            currentGameData.FriendshipData = new Dictionary<string, FriendshipData>();
//            currentGameData.EmotionData = new Dictionary<string, NPCEmotionRuntimeData>();
//            currentGameData.ResurrectionRequirements = new Dictionary<string, ResurrectionRequirement>();

//            currentGameData.Initialize(); // 如果 GameData 有自定义初始化逻辑

//            EnsureAllNPCStates();

//            Debug.Log("New game data created.");
//        }

//        private void EnsureAllNPCStates()
//        {
//            if (currentGameData.NPCStates == null) currentGameData.NPCStates = new Dictionary<string, NPCStateData>();

//            foreach (var npcData in npcDataCache.Values)
//            {
//                if (!currentGameData.NPCStates.ContainsKey(npcData.npcId))
//                {
//                    currentGameData.NPCStates[npcData.npcId] = new NPCStateData
//                    {
//                        npcId = npcData.npcId,
//                        isSpirit = npcData.isSpirit,
//                        isResurrected = false,
//                        position = npcData.initialPosition,
//                        currentScene = "MainScene"
//                    };
//                }
//            }
//        }

//        public void DeleteSaveData()
//        {
//            try
//            {
//                string filePath = GetSaveFilePath();
//                if (File.Exists(filePath))
//                {
//                    File.Delete(filePath);
//                    Debug.Log("Save data deleted.");
//                }
//                CreateNewGameData();
//                OnGameDataDeleted?.Invoke();
//            }
//            catch (Exception e)
//            {
//                Debug.LogError($"Delete failed: {e.Message}");
//            }
//        }

//        private string GetSaveFilePath()
//        {
//            return Path.Combine(Application.persistentDataPath, saveFileName);
//        }

//        // --- 下面是接口的具体实现 (基本保持不变，增加了判空保护) ---

//        public NPCData GetNPCData(string npcId) => npcDataCache.ContainsKey(npcId) ? npcDataCache[npcId] : null;

//        public List<NPCData> GetAllNPCData() => new List<NPCData>(npcDataCache.Values);

//        public ItemData GetItemData(string itemId) => CreateDefaultItemData(itemId); // 保持原样

//        private ItemData CreateDefaultItemData(string itemId)
//        {
//            ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
//            itemData.itemId = itemId;
//            itemData.itemName = $"Item_{itemId}";
//            itemData.baseValue = 10;
//            return itemData;
//        }

//        public Quest GetQuestData(string questId) => questDataCache.ContainsKey(questId) ? questDataCache[questId] : null;

//        public NPCStateData GetNPCState(string npcId)
//        {
//            return (currentGameData?.NPCStates != null && currentGameData.NPCStates.ContainsKey(npcId))
//                ? currentGameData.NPCStates[npcId] : null;
//        }

//        public void UpdateNPCState(string npcId, NPCStateData stateData)
//        {
//            if (currentGameData?.NPCStates != null) currentGameData.NPCStates[npcId] = stateData;
//        }

//        public FriendshipData GetFriendshipData(string npcId)
//        {
//            return (currentGameData?.FriendshipData != null && currentGameData.FriendshipData.ContainsKey(npcId))
//                ? currentGameData.FriendshipData[npcId] : null;
//        }

//        public void UpdateFriendshipData(string npcId, FriendshipData friendshipData)
//        {
//            if (currentGameData?.FriendshipData != null) currentGameData.FriendshipData[npcId] = friendshipData;
//        }

//        public NPCEmotionRuntimeData GetEmotionData(string npcId)
//        {
//            return (currentGameData?.EmotionData != null && currentGameData.EmotionData.ContainsKey(npcId))
//                ? currentGameData.EmotionData[npcId] : null;
//        }

//        public void UpdateEmotionData(string npcId, NPCEmotionRuntimeData emotionData)
//        {
//            if (currentGameData?.EmotionData != null) currentGameData.EmotionData[npcId] = emotionData;
//        }

//        public ResurrectionRequirement GetResurrectionRequirement(string npcId)
//        {
//            return (currentGameData?.ResurrectionRequirements != null && currentGameData.ResurrectionRequirements.ContainsKey(npcId))
//                ? currentGameData.ResurrectionRequirements[npcId] : null;
//        }

//        public void UpdateResurrectionRequirement(string npcId, ResurrectionRequirement requirement)
//        {
//            if (currentGameData?.ResurrectionRequirements != null) currentGameData.ResurrectionRequirements[npcId] = requirement;
//        }

//        public void RegisterDataContainer<T>(T container) where T : class
//        {
//            dataContainers[typeof(T)] = container;
//        }

//        public T GetDataContainer<T>() where T : class
//        {
//            return dataContainers.ContainsKey(typeof(T)) ? dataContainers[typeof(T)] as T : null;
//        }

//        public bool HasSaveData() => File.Exists(GetSaveFilePath());

//        public DateTime GetGameTime() => currentGameData?.GameTime ?? DateTime.Now;

//        public void UpdateGameTime(DateTime newTime)
//        {
//            if (currentGameData != null) currentGameData.GameTime = newTime;
//        }

//        public void ApplySettings(GameSettings settings)
//        {
//            if (currentGameData != null && settings != null)
//            {
//                currentGameData.GameSettings = settings;
//                SaveGameData(false); // 异步保存设置
//            }
//        }

//        // 移动设备: 暂停时保存 (使用异步，但需注意如果系统杀后台过快可能无法完成)
//        private void OnApplicationPause(bool pauseStatus)
//        {
//            if (pauseStatus)
//            {
//                SaveGameData(false);
//            }
//        }
//    }

//    // --- ServiceLocator 增强版 (增加安全性) ---

//    public static class ServiceLocator
//    {
//        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

//        public static void Register<T>(T service) where T : class
//        {
//            var type = typeof(T);
//            if (services.ContainsKey(type))
//            {
//                Debug.LogWarning($"[ServiceLocator] Service {type.Name} already registered. Overwriting.");
//                services[type] = service;
//            }
//            else
//            {
//                services.Add(type, service);
//            }
//        }

//        public static void Unregister<T>(T service) where T : class
//        {
//            var type = typeof(T);
//            if (services.ContainsKey(type) && services[type] == service)
//            {
//                services.Remove(type);
//            }
//        }

//        public static T Get<T>() where T : class
//        {
//            var type = typeof(T);
//            if (services.ContainsKey(type))
//            {
//                return services[type] as T;
//            }
//            return null;
//        }

//        public static bool IsRegistered<T>() where T : class
//        {
//            return services.ContainsKey(typeof(T));
//        }
//    }
//}