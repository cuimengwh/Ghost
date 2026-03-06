using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utopia.Core.Services;
using Utopia.Data;

namespace Utopia.TimeSystem
{
    /// <summary>
    /// 时间UI控制器 - 负责显示和更新游戏中的时间、日期、季节等信息 
    /// 通过监听TimeManager的事件来实时更新UI显示
    /// 提供按钮方法用于控制时间流逝（暂停、加速等）
    /// </summary>
    public class TimeUI : MonoBehaviour
    {
        private ITimeManager _timeManager; // 修正命名规范，使用下划线前缀

        #region UI要素

        [Header("UI 元素")]
        [Tooltip("显示时间的文本组件")]
        public TextMeshProUGUI timeText;
        [Tooltip("显示日期的文本组件")]
        public TextMeshProUGUI dateText;
        [Tooltip("显示季节的文本组件")]
        public TextMeshProUGUI seasonText;
        [Tooltip("显示月份的文本组件")]
        public TextMeshProUGUI monthText;
        [Tooltip("时钟指针图像组件")]
        public Image clockHand;

        #endregion

        #region 格式设置

        [Header("格式设置")]
        [Tooltip("时间显示格式（HH:mm 或 HH:mm:ss）")]
        public string timeFormat = "HH:mm";
        [Tooltip("日期显示格式（{0}=年, {1}=月, {2}=日）")]
        public string dateFormat = "第{0}年 第{1}月 第{2}日";
        [Tooltip("季节显示格式")]
        public string seasonFormat = "{0}";
        [Tooltip("月份显示格式")]
        public string monthFormat = "{0}月";

        #endregion

        #region 生命周期 & 事件订阅

        private void Awake()
        {
            // 修正服务获取方式：使用TryResolve而非TryGet
            if (ServiceLocatorProvider.Global.Locator != null &&
                ServiceLocatorProvider.Global.Locator.TryGet<ITimeManager>(out var timeManager))
            {
                _timeManager = timeManager;
            }
            else
            {
                Debug.LogError("TimeUI: 无法获取 ITimeManager 服务，请确保TimeManager已正确注册到服务定位器。");
            }
        }

        private void Start()
        {
            // 初始更新一次UI（确保启动时显示正确）
            if (_timeManager != null)
            {
                UpdateAllUI();
            }
        }

        /// <summary>
        /// 当对象启用时注册事件监听
        /// </summary>
        private void OnEnable()
        {
            // 检查TimeManager实例是否存在
            if (_timeManager == null)
            {
                Debug.LogWarning("TimeUI: TimeManager实例不存在，无法注册事件监听");
                return;
            }

            // 注册正确的事件名称（匹配ITimeManager接口）
            _timeManager.OnTick += UpdateDateTimeUI; // 每帧更新时间
            _timeManager.OnSeasonChanged += UpdateSeasonUI; // 季节变更时更新
            _timeManager.OnMonthChanged += UpdateMonthUI; // 月份变更时更新
            _timeManager.OnDayChanged += UpdateDayUI; // 日期变更时更新
        }

        /// <summary>
        /// 当对象禁用时取消事件监听（防止内存泄漏）
        /// </summary>
        private void OnDisable()
        {
            if (_timeManager == null) return;

            // 取消所有事件订阅
            _timeManager.OnTick -= UpdateDateTimeUI;
            _timeManager.OnSeasonChanged -= UpdateSeasonUI;
            _timeManager.OnMonthChanged -= UpdateMonthUI;
            _timeManager.OnDayChanged -= UpdateDayUI;
        }

        #endregion

        #region UI更新方法

        /// <summary>
        /// 更新所有UI元素（初始化时调用）
        /// </summary>
        private void UpdateAllUI()
        {
            UpdateDateTimeUI(_timeManager.CurrentTime);
            UpdateSeasonUI(_timeManager.CurrentSeason);
            UpdateMonthUI(_timeManager.Month);
            UpdateDayUI(_timeManager.Day);
        }

        /// <summary>
        /// 更新日期和时间UI显示（适配CustomDateTime结构）
        /// </summary>
        /// <param name="customTime">游戏内自定义时间对象</param>
        private void UpdateDateTimeUI(CustomDateTime customTime)
        {
            if (customTime == null) return;

            // 1. 转换CustomDateTime的0~1时间值为小时/分钟
            var timeParts = ConvertTimeOfDayToHoursMinutes(customTime.time);
            string timeString = $"{timeParts.hours:D2}:{timeParts.minutes:D2}";

            // 更新时间文本
            if (timeText != null)
                timeText.text = timeString;

            // 更新日期文本（使用自定义日历系统）
            if (dateText != null)
            {
                dateText.text = string.Format(dateFormat,
                    customTime.year,
                    customTime.month,
                    customTime.day);
            }

            // 更新时钟指针旋转（360度对应24小时，1小时=15度）
            if (clockHand != null)
            {
                float rotation = (timeParts.hours * 15f) + (timeParts.minutes * 0.25f);
                clockHand.rectTransform.rotation = Quaternion.Euler(0, 0, -rotation);
            }
        }

        /// <summary>
        /// 更新季节UI显示
        /// </summary>
        /// <param name="season">当前季节</param>
        private void UpdateSeasonUI(Season season)
        {
            if (seasonText != null)
            {
                string seasonName = GetSeasonName(season);
                seasonText.text = string.Format(seasonFormat, seasonName);
            }
        }

        /// <summary>
        /// 更新月份UI显示
        /// </summary>
        /// <param name="month">当前月份（1-based）</param>
        private void UpdateMonthUI(int month)
        {
            if (monthText != null)
            {
                monthText.text = string.Format(monthFormat, month);
            }
        }

        /// <summary>
        /// 更新日期UI（可选：单独处理日期变更）
        /// </summary>
        /// <param name="day">当前日期</param>
        private void UpdateDayUI(int day)
        {
            // 如果需要单独更新日期，可以在这里处理
            // 也可以复用UpdateDateTimeUI，因为OnTick会持续更新
        }

        /// <summary>
        /// 将0~1的TimeOfDay值转换为小时和分钟
        /// </summary>
        /// <param name="timeOfDay">0=午夜0点，1=午夜24点</param>
        /// <returns>包含小时和分钟的元组</returns>
        private (int hours, int minutes) ConvertTimeOfDayToHoursMinutes(float timeOfDay)
        {
            // 确保值在0~1范围内
            timeOfDay = Mathf.Clamp01(timeOfDay);

            // 总分钟数 = 一天的总分钟数(1440) * 时间比例
            int totalMinutes = Mathf.FloorToInt(timeOfDay * 24 * 60);
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            // 处理24点转为0点
            if (hours >= 24) hours = 0;

            return (hours, minutes);
        }

        /// <summary>
        /// 获取季节的中文名称
        /// </summary>
        /// <param name="season">季节枚举</param>
        /// <returns>季节的中文名称</returns>
        private string GetSeasonName(Season season)
        {
            switch (season)
            {
                case Season.Spring: return "春季";
                case Season.Summer: return "夏季";
                case Season.Autumn: return "秋季";
                case Season.Winter: return "冬季";
                default: return "未知";
            }
        }

        #endregion

        #region 按钮方法（适配ITimeManager接口）

        /// <summary>
        /// 暂停时间流逝
        /// </summary>
        public void PauseTime()
        {
            if (_timeManager != null)
                _timeManager.Pause();
        }

        /// <summary>
        /// 恢复时间流逝
        /// </summary>
        public void ResumeTime()
        {
            if (_timeManager != null)
                _timeManager.Resume();
        }

        /// <summary>
        /// 切换暂停/恢复状态
        /// </summary>
        public void TogglePause()
        {
            if (_timeManager != null)
            {
                if (_timeManager.IsPaused)
                    _timeManager.Resume();
                else
                    _timeManager.Pause();
            }
        }

        /// <summary>
        /// 加速时间流逝（翻倍）
        /// </summary>
        public void SpeedUpTime()
        {
            if (_timeManager != null)
            {
                float newScale = Mathf.Min(_timeManager.TimeScale * 2f, 16f); // 限制最大倍率
                _timeManager.SetTimeScale(newScale);
            }
        }

        /// <summary>
        /// 减速时间流逝（减半）
        /// </summary>
        public void SlowDownTime()
        {
            if (_timeManager != null)
            {
                float newScale = Mathf.Max(_timeManager.TimeScale / 2f, 0.125f); // 限制最小倍率
                _timeManager.SetTimeScale(newScale);
            }
        }

        /// <summary>
        /// 重置时间流逝速度
        /// </summary>
        public void ResetTimeScale()
        {
            if (_timeManager != null)
                _timeManager.SetTimeScale(1f);
        }

        #endregion
    }
}