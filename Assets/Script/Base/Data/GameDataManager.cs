using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utopia.Core;
using Utopia.Core.Services; // 引用服务定位器
using Utopia.Data;

namespace Utopia.GameLogic
{
    /// <summary>
    /// 游戏数据管理器
    /// 负责全局游戏数据的存储、存档/读档、场景切换以及与各个可存档系统的协调。
    /// 通过服务定位器注册自身，供其他模块获取。
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        [Header("设置")]
        [Tooltip("默认加载的场景名称，当没有存档或首次启动时加载该场景")]
        [SerializeField]
        private string _defaultSceneName = "FarmScene";

        [Tooltip("玩家对象的Transform，用于在切换场景或存档时获取/恢复玩家位置")]
        [SerializeField]
        private Transform _playerTransform;

        /// <summary>
        /// 当前使用的存档槽ID，默认为"AutoSave"（自动存档）
        /// </summary>
        public string CurrentSlotId { get; private set; } = "AutoSave";

        /// <summary>
        /// 当前全局游戏数据对象，包含所有需要跨场景持久化的状态
        /// 注意：属性名拼写为 CurentGlobalData（应改为 CurrentGlobalData），为保持兼容暂保留
        /// </summary>
        public GameData CurentGlobalData { get; private set; }

        /// <summary>
        /// 数据服务接口，用于执行实际的存档/读档操作
        /// 通过依赖注入或服务定位器获取具体实现
        /// </summary>
        public IDataService _dataService;

        /// <summary>
        /// 所有实现了 ISaveableSystem 接口的可存档系统列表
        /// 这些系统会在存档时提供数据，读档时接收数据
        /// </summary>
        private List<ISaveableSystem> _saveableSystems = new List<ISaveableSystem>();

        private void Awake()
        {
            // 将该管理器实例注册到全局服务定位器，其他模块可以通过服务定位器获取GameDataManager实例
            ServiceLocatorProvider.Global.Locator.Register<GameDataManager>(this);
        }

        // TODO: 以下方法需要根据项目需求实现
        // - 初始化加载（LoadInitialData）
        // - 创建新存档（NewGame）
        // - 保存当前进度（SaveGame）
        // - 加载指定存档（LoadGame）
        // - 切换场景（ChangeScene）
        // - 注册可存档系统（RegisterSaveableSystem）
        // - 取消注册可存档系统（UnregisterSaveableSystem）
    }
}