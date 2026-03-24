using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utopia.Core;
using Utopia.Core.Event;
using Utopia.Core.Services;
using Utopia.Data;

namespace Utopia.TimeSystem
{
    /// <summary>
    /// 时间管理器的服务接口
    /// 其他模块通过此接口访问时间管理功能，而非直接依赖 TimeManager 类
    /// </summary>
    public interface ITimeManager
    {
        // 只读属性
        CustomDateTime CurrentTime { get; }
        int Year { get; }
        int Month { get; }
        int Day { get; }
        float TimeOfDay { get; }
        Season CurrentSeason { get; }
        SunState CurrentSunState { get; }
        bool IsPaused { get; }
        float TimeScale { get; }

        // 事件
        event Action<CustomDateTime> OnTick;
        event Action<int> OnDayChanged;
        event Action<int> OnMonthChanged;
        event Action<int> OnYearChanged;
        event Action<Season> OnSeasonChanged;
        event Action<SunState> OnSunStateChanged;

        // 控制方法
        void Pause();
        void Resume();
        void SetTimeScale(float scale);
        void RegisterEvent(TimeEvent timeEvent);
        void UnregisterEvent(TimeEvent timeEvent);
    }
    /// <summary>
    /// TimeManager - 负责游戏内时间的管理和事件系统
    /// 
    /// 功能说明：
    /// 1. 管理游戏内的日期、时间、季节等状态
    /// 2. 支持时间缩放和暂停功能
    /// 3. 提供事件系统，允许其他系统订阅时间变化事件
    /// 4. 支持自定义日期时间结构，便于游戏内逻辑使用
    /// 5. 实现 <see cref="ISaveableSystem"/> 接口，支持存档/读档
    /// 2. 配置 <see cref="TimeSettings"/> ScriptableObject 以定义时间参数
    /// </summary>
    public class TimeManager : MonoBehaviour, ISaveableSystem, ITimeManager
    {
        public static TimeManager instance;

        #region 依赖 配置

        /// <summary>
        /// 数据存储服务，通过依赖注入获取。
        /// 用于存档/读档游戏时间数据。
        /// </summary>
        [ServiceInject]
        private IDataService _dataService;

        /// <summary>
        /// 事件管理器，用于发布全局时间事件。
        /// 可以发送如 DayPassedEvent 等全局事件，供其他模块监听。
        /// </summary>
        [ServiceInject]
        private IEventManager _eventManager;

        /// <summary>
        /// 时间配置数据（ScriptableObject），在Inspector中赋值。
        /// 包含一年月数、一月天数、一天秒数、时段划分、季节划分等配置。
        /// </summary>
        [SerializeField] private TimeSettings timeSettings;

        /// <summary>
        /// 存档数据在 <see cref="IDataService"/> 中的唯一键名。
        /// 用于存储和加载时间数据。
        /// </summary>
        private const string SAVE_KEY = "GameTimeData";

        #endregion

        #region 运行时状态

        /// <summary>
        /// 当前游戏日期时间（包含年月日及时刻）。
        /// 时刻以 0~1 浮点数表示一天中的进度。
        /// </summary>
        private CustomDateTime _currentTime;

        /// <summary>
        /// 当前一天中的进度（0.0 ~ 1.0），用于Inspector调试显示。
        /// </summary>
        [SerializeField]
        private float currentTimeOfDay; // 0-1 表示一天中的时间段

        /// <summary>
        /// 时间缩放倍数（1.0 = 现实时间流速）。
        /// 实际时间增量 = 现实时间增量 * _timeScale * timeSettings.timeMultiplier。
        /// </summary>
        private float _timeScale = 1f;

        /// <summary>
        /// 时间是否暂停。暂停时不会推进游戏时间。
        /// </summary>
        private bool _isPaused = false;

        /// <summary>
        /// 系统是否已完成初始化。
        /// 初始化前不会处理时间推进。
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// 当前季节（根据月份计算得出）。
        /// </summary>
        private Season _currentSeason;

        /// <summary>
        /// 当前时段（如早晨、中午、傍晚、夜晚）。
        /// </summary>
        private SunState _currentSunState;

        /// <summary>
        /// 已注册的定时事件列表。
        /// 这些事件会在时间达到触发条件时执行回调。
        /// </summary>
        private List<TimeEvent> _scheduledEvents = new List<TimeEvent>();

        #endregion

        #region 公开事件

        /// <summary>
        /// 每帧触发一次，传递当前时间（高频事件，适合UI刷新）。
        /// </summary>
        public event Action<CustomDateTime> OnTick;

        /// <summary>
        /// 日期变更时触发，传递新的天数（当月第几天）。
        /// </summary>
        public event Action<int> OnDayChanged;

        /// <summary>
        /// 月份变更时触发，传递新的月份（1-based）。
        /// </summary>
        public event Action<int> OnMonthChanged;

        /// <summary>
        /// 年份变更时触发，传递新的年份。
        /// </summary>
        public event Action<int> OnYearChanged;

        /// <summary>
        /// 季节变更时触发，传递新的季节。
        /// </summary>
        public event Action<Season> OnSeasonChanged;

        /// <summary>
        /// 时段（早/中/晚）变更时触发，传递新的时段。
        /// </summary>
        public event Action<SunState> OnSunStateChanged;

        #endregion

        #region 公开属性

        /// <summary>
        /// 当前完整时间对象。
        /// </summary>
        public CustomDateTime CurrentTime => _currentTime;

        /// <summary>
        /// 当前年份。
        /// </summary>
        public int Year => _currentTime.year;

        /// <summary>
        /// 当前月份（1~monthsPerYear）。
        /// </summary>
        public int Month => _currentTime.month;

        /// <summary>
        /// 当前日期（1~daysPerMonth）。
        /// </summary>
        public int Day => _currentTime.day;

        /// <summary>
        /// 当前一天中的进度（0.0~1.0）。
        /// </summary>
        public float TimeOfDay => _currentTime.time;

        /// <summary>
        /// 当前季节。
        /// </summary>
        public Season CurrentSeason => _currentSeason;

        /// <summary>
        /// 当前时段。
        /// </summary>
        public SunState CurrentSunState => _currentSunState;

        /// <summary>
        /// 时间是否暂停。
        /// </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        /// 当前时间缩放倍数。
        /// </summary>
        public float TimeScale => _timeScale;

        /// <summary>
        /// 实现 <see cref="ISaveableSystem.SaveKey"/> 接口。
        /// 返回用于存档的唯一键名。
        /// </summary>
        public string SaveKey => SAVE_KEY;

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化单例，将自身注册到全局服务定位器。
        /// 同时设置为 DontDestroyOnLoad，确保跨场景不销毁。
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            if (ServiceLocatorProvider.Global.Locator != null)
            {
                ServiceLocatorProvider.Global.Locator.Register(this);
            }
            else
            {
                Debug.LogError("TimeManager 初始化失败: ServiceLocatorProvider.Global.Locator 为空");
            }

            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 启动时初始化时间系统。
        /// </summary>
        private void Start()
        {
            InitializeTime();
        }

        /// <summary>
        /// 每帧更新，负责时间推进（如果未暂停）。
        /// </summary>
        private void Update()
        {
            if (!_isInitialized || _isPaused)
                return;

            AdvanceTime(Time.deltaTime);
            // 当前代码未包含实际推进逻辑，需根据需求补充
        }

        #endregion

        #region 核心逻辑

        /// <summary>
        /// 初始化时间系统，设置起始时间并刷新派生状态。
        /// 起始时间从 <see cref="timeSettings"/> 中读取。
        /// </summary>
        private void InitializeTime()
        {
            _currentTime = timeSettings.startDateTime;
            RefreshDerivedStates(true);
            _isInitialized = true;
        }

        /// <summary>
        /// 根据流逝的现实时间推进游戏内时间。
        /// </summary>
        /// <param name="deltaTime">上一帧的现实时间（秒）</param>
        private void AdvanceTime(float deltaTime)
        {
            // 计算逻辑：
            // 现实 1秒 * 时间倍率 (60) = 游戏内 60秒
            // 游戏内 60秒 / 一天总秒数 (86400) = 这一帧增加的"天数比例"
            float timeIncrement = (deltaTime * _timeScale * timeSettings.timeMultiplier) / timeSettings.secondsPerFullDay;

            _currentTime.time += timeIncrement;

            if (_currentTime.time >= 1f)
            {
                _currentTime.time -= 1f;
                AdvanceDay();
            }

            CheckSunState();
            OnTick?.Invoke(_currentTime);
            CheckScheduledEvents();
        }

        /// <summary>
        /// 推进到新的一天，处理跨月和事件触发。
        /// </summary>
        private void AdvanceDay()
        {
            _currentTime.day++;

            // 检查跨月
            if (_currentTime.day > timeSettings.daysPerMonth)
            {
                _currentTime.day = 1;
                AdvanceMonth();
            }

            OnDayChanged?.Invoke(_currentTime.day);
            // 可以在这里发布全局总线事件
            //_eventManager?.Publish(new DayPassedEvent(_currentTime)); 

            Debug.Log($"[TimeManager] 新的一天: {_currentTime}");
        }

        /// <summary>
        /// 推进到新的一月，处理跨年和季节变更。
        /// </summary>
        private void AdvanceMonth()
        {
            _currentTime.month++;

            // 检查跨年
            if (_currentTime.month > timeSettings.monthsPerYear)
            {
                _currentTime.month = 1;
                AdvanceYear();
            }

            // 检查季节变化
            CheckSeason();
            OnMonthChanged?.Invoke(_currentTime.month);
        }

        /// <summary>
        /// 推进到新的一年，触发年份变更事件。
        /// </summary>
        private void AdvanceYear()
        {
            _currentTime.year++;
            OnYearChanged?.Invoke(_currentTime.year);
        }

        #endregion

        #region 状态检查 缓存

        /// <summary>
        /// 刷新所有派生状态（季节、时段）。
        /// </summary>
        /// <param name="force">是否强制触发变更事件（即使值未变）</param>
        private void RefreshDerivedStates(bool force = false)
        {
            CheckSeason(force);
            CheckSunState(force);
        }

        /// <summary>
        /// 根据当前月份检查并更新季节，若变化则触发事件。
        /// </summary>
        /// <param name="force">强制触发事件（用于初始化或存档加载）</param>
        private void CheckSeason(bool force = false)
        {
            Season newSeason = timeSettings.GetSeason(_currentTime.month);

            if (force || newSeason != _currentSeason)
            {
                _currentSeason = newSeason;
                OnSeasonChanged?.Invoke(_currentSeason);
                Debug.Log($"[TimeManager] 季节变更: {_currentSeason}");
            }
        }

        /// <summary>
        /// 根据当前时刻检查并更新时段，若变化则触发事件。
        /// </summary>
        /// <param name="force">强制触发事件</param>
        private void CheckSunState(bool force = false)
        {
            SunState newSunState = timeSettings.GetSunState(_currentTime.time);
            if (force || newSunState != _currentSunState)
            {
                _currentSunState = newSunState;
                OnSunStateChanged?.Invoke(_currentSunState);
                Debug.Log($"[TimeManager] 时段变更: {_currentSunState}");
            }
        }

        #endregion

        #region 事件系统集成

        /// <summary>
        /// 注册一个定时事件，事件到达触发时间时将执行回调。
        /// </summary>
        /// <param name="timeEvent">要注册的时间事件对象。</param>
        public void RegisterEvent(TimeEvent timeEvent)
        {
            if (timeEvent == null) return;

            if (!_scheduledEvents.Contains(timeEvent))
            {
                timeEvent.ResetState(); // 注册时重置，确保事件状态正确
                _scheduledEvents.Add(timeEvent);
            }
        }

        /// <summary>
        /// 取消注册一个定时事件。
        /// </summary>
        /// <param name="timeEvent">要取消的事件对象。</param>
        public void UnregisterEvent(TimeEvent timeEvent)
        {
            if (_scheduledEvents.Contains(timeEvent))
            {
                _scheduledEvents.Remove(timeEvent);
            }
        }

        /// <summary>
        /// 检查所有已注册的定时事件，如果满足触发条件则执行回调。
        /// 从后向前遍历以避免集合修改问题。
        /// </summary>
        private void CheckScheduledEvents()
        {
            for (int i = _scheduledEvents.Count - 1; i >= 0; i--)
            {
                var evt = _scheduledEvents[i];

                if (evt.CheckAndTrigger(_currentTime))
                {
                    if (!evt.isRepeatable)
                    {
                        _scheduledEvents.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region 控制API

        /// <summary>
        /// 暂停时间推进。
        /// </summary>
        public void Pause() => _isPaused = true;

        /// <summary>
        /// 恢复时间推进。
        /// </summary>
        public void Resume() => _isPaused = false;

        /// <summary>
        /// 设置时间缩放倍数。
        /// </summary>
        /// <param name="scale">缩放倍数（必须大于等于0）</param>
        public void SetTimeScale(float scale) => _timeScale = Mathf.Max(0f, scale); // 确保时间缩放非负

        #endregion

        #region ISaveableSystem 实现

        /// <summary>
        /// 将当前时间状态保存到存档。
        /// </summary>
        /// <param name="slotId">存档槽位标识。</param>
        /// <returns>异步任务。</returns>
        public Task Save(string slotId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从存档加载时间状态。
        /// </summary>
        /// <param name="slotId">存档槽位标识。</param>
        /// <returns>异步任务。</returns>
        public Task Load(string slotId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}