using System;
using Newtonsoft.Json.Converters;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Events;

namespace Utopia.TimeSystem
{
    /// <summary>
    /// 时间事件类。
    /// 用于定义在特定时间条件满足时触发的事件。
    /// 支持一次性事件和重复事件，事件触发条件由 <see cref="TimeEventTrigger"/> 定义。
    /// </summary>
    [Serializable]
    public class TimeEvent
    {
        /// <summary>
        /// 事件名称（仅用于识别，不影响逻辑）。
        /// </summary>
        public string eventName;

        /// <summary>
        /// 是否为重复事件。
        /// - true: 每次满足条件都会触发。
        /// - false: 只触发一次，触发后自动失效。
        /// </summary>
        public bool isRepeatable;

        /// <summary>
        /// 触发条件定义。
        /// </summary>
        public TimeEventTrigger triggerCondition;

        /// <summary>
        /// Unity 事件，当条件满足时调用。
        /// 可在 Inspector 中绑定，也可通过代码动态添加监听。
        /// </summary>
        public UnityEvent OnTimeEvent;

        /// <summary>
        /// 内部状态：标记一次性事件是否已经触发过。
        /// 使用 [NonSerialized] 确保不被序列化保存，避免存档导致状态错误。
        /// </summary>
        [NonSerialized] private bool _hasTriggeredOneShot = false;

        /// <summary>
        /// 检查当前时间是否满足触发条件，若满足则触发事件。
        /// </summary>
        /// <param name="currentTime">当前游戏时间</param>
        /// <returns>
        /// 如果事件是一次性的并且已经触发，返回 true；
        /// 否则返回条件是否满足并触发的结果（即本次是否触发）。
        /// </returns>
        public bool CheckAndTrigger(CustomDateTime currentTime)
        {
            // 如果是一次性事件且已经触发过，直接返回 true（表示已完成）
            if (!isRepeatable && _hasTriggeredOneShot) return true;

            // 判断条件是否满足
            if (triggerCondition.IsConditionMet(currentTime))
            {
                Trigger(); // 触发事件

                // 如果是一次性事件，标记已触发并返回 true
                if (!isRepeatable)
                {
                    _hasTriggeredOneShot = true;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 执行事件触发逻辑。
        /// 调用 UnityEvent 的所有监听器，并输出调试日志。
        /// </summary>
        private void Trigger()
        {
            Debug.Log($"[TimeEvent] 触发: {eventName}");
            OnTimeEvent?.Invoke();
        }

        /// <summary>
        /// 重置事件的内部状态。
        /// 通常用于重新激活事件（例如读档后重置一次性标记）。
        /// </summary>
        public void ResetState()
        {
            _hasTriggeredOneShot = false;
            triggerCondition?.ResetState();
        }

        /// <summary>
        /// 静态工厂方法：创建一个一次性事件。
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="targerTime">触发目标时间（注意参数名拼写错误，应为 targetTime）</param>
        /// <param name="action">触发时执行的回调</param>
        /// <returns>新创建的一次性事件实例</returns>
        public static TimeEvent CreateOneShot(string name, CustomDateTime targerTime, UnityAction action)
        {
            var evt = new TimeEvent
            {
                eventName = name,
                isRepeatable = false,
                triggerCondition = new TimeEventTrigger
                {
                    triggerType = TimeEventTrigger.TriggerType.SpecificDateTime,
                    targetParams = targerTime,
                }
            };
            evt.OnTimeEvent.AddListener(action);
            return evt;
        }
    }

    /// <summary>
    /// 时间事件触发条件定义类。
    /// 支持多种触发类型：具体时间点、每天、每月、每年。
    /// 包含时间容差，避免因帧更新错过精确时间点。
    /// </summary>
    [Serializable]
    public class TimeEventTrigger
    {
        /// <summary>
        /// 触发类型枚举。
        /// </summary>
        public enum TriggerType
        {
            SpecificDateTime, // 特定日期时间（年、月、日、时）
            Daily,            // 每天固定时间（只比较时）
            Monthly,          // 每月固定日期和时间（日、时）
            YearLy            // 每年固定月、日、时（注意拼写应为 Yearly）
        }

        /// <summary>
        /// 触发类型。
        /// </summary>
        public TriggerType triggerType = TriggerType.SpecificDateTime;

        /// <summary>
        /// 触发参数容器。
        /// 根据 triggerType 的不同，使用的字段也不同：
        /// - SpecificDateTime: 使用完整的年/月/日/时
        /// - Daily: 只使用时
        /// - Monthly: 使用日/时
        /// - Yearly: 使用月/日/时
        /// </summary>
        [Tooltip("触发参数容器。\n" +
                 "- Specific: 使用 年/月/日/时\n" +
                 "- Daily: 只使用 时\n" +
                 "- Monthly: 使用 日/时\n" +
                 "- Yearly: 使用 月/日/时")]
        public CustomDateTime targetParams;

        /// <summary>
        /// 时间容差（单位：天的小数部分，0.01 ≈ 14分钟）。
        /// 对于每日/每月/每年类型的触发，只有当当前时间超过目标时间且在容差范围内时，才认为条件满足。
        /// 用于防止因帧更新错过精确时间点。
        /// </summary>
        [Tooltip("时间容差 (0.01 ≈ 14分钟)。\n只要当前时间超过目标时间，且在容差范围内，就算触发。")]
        [Range(0.001f, 0.1f)]
        public float timeTolerance = 0.01f;

        /// <summary>
        /// 内部状态：记录上一次触发时的“天数ID”（即总天数），用于防止同一周期内重复触发。
        /// 对于 Daily/Monthly/Yearly 类型，每个周期只应触发一次。
        /// </summary>
        [NonSerialized] private int _lastTriggerDayId = -1;

        /// <summary>
        /// 判断给定当前时间是否满足触发条件。
        /// </summary>
        /// <param name="current">当前游戏时间</param>
        /// <returns>条件满足返回 true，否则 false</returns>
        public bool IsConditionMet(CustomDateTime current)
        {
            // 对于非一次性事件，检查是否已经在本周期内触发过
            if (triggerType != TriggerType.SpecificDateTime && current.ToTotalDays == _lastTriggerDayId)
            {
                return false;
            }

            bool isMet = false;

            // 根据触发类型分别判断
            switch (triggerType)
            {
                case TriggerType.SpecificDateTime:
                    // 特定时间点：当前时间 >= 目标时间即触发（不检查容差，因为是精确时刻，一旦超过就触发）
                    isMet = current >= targetParams;
                    break;

                case TriggerType.Daily:
                    // 每天固定时刻：比较当前天的时刻是否达到目标时刻，且在容差范围内
                    isMet = IsTimeReached(current.time, targetParams.time);
                    break;
                case TriggerType.Monthly:
                    // 每月固定日期时刻：日期相同且时刻在容差内
                    isMet = (current.day == targetParams.day && IsTimeReached(current.time, targetParams.time));
                    break;
                case TriggerType.YearLy:
                    // 每年固定月日时刻：月、日相同且时刻在容差内
                    isMet = (current.month == targetParams.month) &&
                            (current.day == targetParams.day) &&
                            IsTimeReached(current.time, targetParams.time);
                    break;
            }

            // 如果条件满足，记录本次触发的天数ID，避免同一周期内再次触发
            if (isMet)
            {
                _lastTriggerDayId = current.ToTotalDays;
            }

            return isMet;
        }

        /// <summary>
        /// 判断当前时间是否已达到或超过目标时刻，并且仍在容差范围内。
        /// </summary>
        /// <param name="currentTime">当前时间（天的比例，0~1）</param>
        /// <param name="targetTime">目标时间（天的比例）</param>
        /// <returns>如果 currentTime 在 [targetTime, targetTime + timeTolerance) 范围内则返回 true</returns>
        private bool IsTimeReached(float currentTime, float targetTime)
        {
            return currentTime >= targetTime && currentTime < (targetTime + timeTolerance);
        }

        /// <summary>
        /// 重置触发器的内部状态，通常用于重新激活或读档时。
        /// </summary>
        public void ResetState()
        {
            _lastTriggerDayId = -1;
        }
    }
}