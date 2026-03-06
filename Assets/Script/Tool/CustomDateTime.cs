using System;
using UnityEngine;
using UnityEngine.Events;

namespace Utopia.TimeSystem
{
    /// <summary>
    /// 自定义日期时间结构，适用于 28天/月、4月/年的独特日历系统。
    /// 时间部分以 0~1 的浮点数表示一天内的时间进度（0 对应 00:00，1 对应 24:00）。
    /// 支持比较运算、算术运算（通过时间戳）、以及格式化的字符串输出。
    /// </summary>
    [System.Serializable]
    public struct CustomDateTime : IComparable<CustomDateTime>, IEquatable<CustomDateTime>
    {
        /// <summary>年份。从 1 开始，最小值为 1。</summary>
        [Tooltip("年份")]
        public int year;

        /// <summary>月份，取值范围 1 ~ 4。</summary>
        [Tooltip("月份 (1-4)")]
        [Range(1, 4)]
        public int month;

        /// <summary>日期，取值范围 1 ~ 28。</summary>
        [Tooltip("日期 (1-28)")]
        [Range(1, 28)]
        public int day;

        /// <summary>一天内的时间，0 ~ 1 之间的浮点数，0 表示 00:00，1 表示 24:00。</summary>
        [Tooltip("时间 (0-1表示0-24小时)")]
        [Range(0f, 1f)]
        public float time;

        /// <summary>
        /// 使用指定的年、月、日和时间初始化 CustomDateTime 实例。
        /// 参数会自动钳制到有效范围。
        /// </summary>
        /// <param name="year">年份（自动确保 ≥1）</param>
        /// <param name="month">月份（自动限制在 1~4）</param>
        /// <param name="day">日期（自动限制在 1~28）</param>
        /// <param name="time">时间（0~1，自动限制在范围内）</param>
        public CustomDateTime(int year, int month, int day, float time)
        {
            this.year = Mathf.Max(1, year);
            this.month = Mathf.Clamp(month, 1, 4);
            this.day = Mathf.Clamp(day, 1, 28);
            this.time = Mathf.Clamp01(time);
        }

        #region 核心计算属性

        /// <summary>每年包含的天数（4个月 × 28天）。</summary>
        private const int DAYS_PER_YEAR = 112;
        /// <summary>每月包含的天数。</summary>
        private const int DAYS_PER_MONTH = 28;

        /// <summary>
        /// 获取从元年（1年1月1日）到当前日期经过的整数天数。
        /// 元年1月1日返回 0，1月2日返回 1，依此类推。
        /// </summary>
        public int ToTotalDays => (year - 1) * DAYS_PER_YEAR + (month - 1) * DAYS_PER_MONTH + (day - 1);

        /// <summary>
        /// 获取一个双精度浮点数表示的绝对时间戳，整数部分为 ToTotalDays，小数部分为 time（0~1）。
        /// 该值可用于日期时间的比较和算术运算。
        /// </summary>
        public double Timestamp => ToTotalDays + time;

        #endregion

        #region 运算符重载

        /// <summary>比较当前实例与另一个 CustomDateTime 对象。</summary>
        /// <param name="other">要比较的对象</param>
        /// <returns>负数：当前小于 other；零：相等；正数：当前大于 other</returns>
        public int CompareTo(CustomDateTime other)
        {
            return Timestamp.CompareTo(other.Timestamp);
        }

        /// <summary>指示当前实例是否与另一个 CustomDateTime 对象相等。</summary>
        /// <param name="other">要比较的对象</param>
        /// <returns>如果所有字段（年、月、日、时间）都相等则为 true，否则为 false</returns>
        public bool Equals(CustomDateTime other)
        {
            return year == other.year &&
                   month == other.month &&
                   day == other.day &&
                   Mathf.Approximately(time, other.time);
        }

        /// <summary>指示当前实例是否等于另一个对象。</summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>如果 obj 是 CustomDateTime 且所有字段相等则为 true</returns>
        public override bool Equals(object obj) => obj is CustomDateTime other && Equals(other);

        /// <summary>返回当前实例的哈希码。</summary>
        /// <returns>由年、月、日、时间组合成的哈希值</returns>
        public override int GetHashCode() => HashCode.Combine(year, month, day, time);

        /// <summary>判断两个 CustomDateTime 实例是否表示同一时刻（基于 Timestamp 比较）。</summary>
        public static bool operator ==(CustomDateTime a, CustomDateTime b) => a.Equals(b);
        /// <summary>判断两个 CustomDateTime 实例是否表示不同时刻。</summary>
        public static bool operator !=(CustomDateTime a, CustomDateTime b) => !a.Equals(b);
        /// <summary>判断一个实例是否大于另一个实例（基于 Timestamp 比较）。</summary>
        public static bool operator >(CustomDateTime x, CustomDateTime y) => x.Timestamp > y.Timestamp;
        /// <summary>判断一个实例是否小于另一个实例（基于 Timestamp 比较）。</summary>
        public static bool operator <(CustomDateTime x, CustomDateTime y) => x.Timestamp < y.Timestamp;
        /// <summary>判断一个实例是否大于等于另一个实例（基于 Timestamp 比较）。</summary>
        public static bool operator >=(CustomDateTime a, CustomDateTime b) => a.Timestamp >= b.Timestamp;
        /// <summary>判断一个实例是否小于等于另一个实例（基于 Timestamp 比较）。</summary>
        public static bool operator <=(CustomDateTime a, CustomDateTime b) => a.Timestamp <= b.Timestamp;

        #endregion

        /// <summary>
        /// 返回当前 CustomDateTime 的字符串表示，格式为 "Y{year}-M{month:D2}-D{day:D2} {hours:D2}:{minutes:D2}"。
        /// 例如：Y1-M01-D01 00:00 表示元年1月1日午夜。
        /// </summary>
        /// <returns>格式化的日期时间字符串</returns>
        public override string ToString()
        {
            // 将 0-1 的 float 转换为 00:00 格式
            int totalMinutes = Mathf.FloorToInt(time * 1440f); // 24 * 60 = 1440
            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return $"Y{year}-M{month:D2}-D{day:D2} {hours:D2}:{minutes:D2}";
        }
    }
}