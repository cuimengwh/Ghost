using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Utopia.TimeSystem
{
    /// <summary>
    /// 时间系统配置数据
    /// 支持28天为一月，一年4个月的自定义日历系统
    /// </summary>
    [CreateAssetMenu(fileName = "TimeSettings", menuName = "Time System/Time Settings")]
    public class TimeSettings : ScriptableObject
    {
        #region 时间流逝设置
        [Header("时间流逝设置")]
        [Tooltip("时间流逝倍率（现实1秒 = 游戏时间秒数；现实1f = 游戏内1分）")]
        public float timeMultiplier = 60f;

        [Tooltip("游戏内完整一天的秒数")]
        public float secondsPerFullDay = 86400f; // 86400秒对应游戏一天 (24小时)
        #endregion

        #region 自定义日历设置  28/4/1 : 112天
        [Header("日历规则")]
        [Tooltip("每月包含的天数 (固定历法)")]
        [Min(1)]
        public int daysPerMonth = 28;

        [Tooltip("每年包含的月数")]
        [Min(1)]
        public int monthsPerYear = 4;

        /// <summary>
        /// 每年包含的总天数（只读）
        /// 计算公式：monthsPerYear × daysPerMonth
        /// 默认值：4 × 28 = 112 天/年
        /// </summary>
        public int daysPerYear => monthsPerYear * daysPerMonth;
        #endregion

        #region 起始设置
        [Header("起始时间")]
        [Tooltip("游戏开始时的时间点")]
        public CustomDateTime startDateTime = new CustomDateTime(1, 1, 1, 0.25f); // 默认起始时间：1年1月1日6:00 (0.25表示6:00)
        #endregion

        #region 辅助计算方法

        /// <summary>
        /// 根据月份获取对应的季节
        /// </summary>
        /// <param name="month">月份（1~monthsPerYear）</param>
        /// <returns>季节枚举</returns>
        /// <remarks>
        /// 注意：原代码错误地将 month 限制在 [1, daysPerMonth] 范围内，已修正为 [1, monthsPerYear]。
        /// 季节与月份的对应关系为：1月→春，2月→夏，3月→秋，4月→冬。
        /// </remarks>
        public Season GetSeason(int month)
        {
            // 确保月份在有效范围内（1 ~ monthsPerYear）
            int clampedMonth = Mathf.Clamp(month, 1, monthsPerYear);
            // 将月份（1-based）映射为季节索引（0-based）
            return (Season)(clampedMonth - 1);
        }

        /// <summary>
        /// 根据当天的时间进度（0-1）获取时段状态
        /// </summary>
        /// <param name="time01">0~1的小数，0=0:00，1=次日0:00</param>
        /// <returns>太阳状态（早晨/中午/黄昏/午夜）</returns>
        /// <remarks>
        /// 时段划分规则（与SunState枚举的Tooltip保持一致）：
        /// - 6:00 ~ 10:00 → Morn
        /// - 10:00 ~ 16:00 → Noon
        /// - 16:00 ~ 20:00 → Dusk
        /// - 20:00 ~ 次日6:00 → Midnight
        /// </remarks>
        public SunState GetSunState(float time01)
        {
            // 将时间进度（0~1）转换为小时（0~24）
            float hour = time01 * 24f;

            if (hour >= 6f && hour < 10f) return SunState.Morn;      // 6-10 早晨
            if (hour >= 10f && hour < 16f) return SunState.Noon;     // 10-16 白天/中午
            if (hour >= 16f && hour < 20f) return SunState.Dusk;     // 16-20 黄昏
            return SunState.Midnight;                                // 20-6  午夜
        }

        #endregion

        #region 编辑器校验 (安全网)

        /// <summary>
        /// 当在Inspector中修改数值时自动调用，用于修正非法数值
        /// </summary>
        private void OnValidate()
        {
            // 确保日历参数为正数
            if (daysPerMonth < 1) daysPerMonth = 28;
            if (monthsPerYear < 1) monthsPerYear = 4;
            // 时间倍率不能太小，避免游戏时间几乎静止
            if (timeMultiplier < 0.1f) timeMultiplier = 0.1f;

            // 强制修正起始时间，使其符合日历规则
            startDateTime.month = Mathf.Clamp(startDateTime.month, 1, monthsPerYear);
            startDateTime.day = Mathf.Clamp(startDateTime.day, 1, daysPerMonth);
            // 注意：startDateTime.time01 应在0~1之间，但未强制限制（可由CustomDateTime内部处理）
        }

        #endregion
    }

    #region 辅助类和枚举
    /// <summary>
    /// 太阳状态枚举（一天中的时间段）
    /// 具体时段划分请参考 <see cref="TimeSettings.GetSunState"/> 方法的实现。
    /// </summary>
    [Serializable]
    public enum SunState
    {
        [Tooltip("早晨 (6:00-10:00)")]
        Morn,    // 早晨

        [Tooltip("中午 (10:00-16:00)")]
        Noon,    // 中午（注意：实际划分到16点，与Tooltip一致）

        [Tooltip("傍晚 (16:00-20:00)")]
        Dusk,    // 傍晚

        [Tooltip("午夜 (20:00-6:00)")]
        Midnight // 午夜
    }

    /// <summary>
    /// 季节枚举
    /// 对应一年四个月：1月=Spring, 2月=Summer, 3月=Autumn, 4月=Winter
    /// </summary>
    public enum Season
    {
        [Tooltip("春季")]
        Spring, // 春

        [Tooltip("夏季")]
        Summer, // 夏

        [Tooltip("秋季")]
        Autumn, // 秋

        [Tooltip("冬季")]
        Winter, // 冬

        [Tooltip("任意")]
        Any     // 全季节（用于需要忽略季节的场合）
    }
    #endregion
}