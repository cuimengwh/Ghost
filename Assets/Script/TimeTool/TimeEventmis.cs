//using System;
//using Newtonsoft.Json.Converters;
//using NUnit.Framework.Constraints;
//using UnityEngine;
//using UnityEngine.Events;

//namespace Utopia.TimeSystem
//{
//    [Serializable]
//    public class TimeEvent
//    {
//        public string eventName;
//        public bool isRepeatable;
//        public TimeEventTrigger triggerCondition;
//        public UnityEvent OnTimeEvent;

//        [NonSerialized] private bool _hasTriggeredOneShot = false;

//        public bool CheckAndTrigger(CustomDateTime currentTime)
//        {
//            if (!isRepeatable && _hasTriggeredOneShot) return true;

//            if (triggerCondition.IsConditionMet(currentTime))
//            {
//                Trigger();
                
//                if (!isRepeatable)
//                {
//                    _hasTriggeredOneShot = true;
//                    return true;
//                }
//            }

//            return false;
//        }

//        private void Trigger()
//        {
//            Debug.Log($"[TimeEvent] 触发: {eventName}");
//            OnTimeEvent?.Invoke();
//        }

//        public void ResetState()
//        {
//            _hasTriggeredOneShot = false;
//            triggerCondition?.ResetState();
//        }

//        public static TimeEvent CreatOneShot(string name, CustomDateTime targerTime, UnityAction action)
//        {
//            var evt = new TimeEvent
//            {
//                eventName = name,
//                isRepeatable = false,
//                triggerCondition = new TimeEventTrigger
//                {
//                    triggerType = TimeEventTrigger.TriggerType.SpecificDateTime,
//                    targetParams = targerTime,
//                }
//            };
//            evt.OnTimeEvent.AddListener(action);
//            return evt;
//        }
//    }

//    public class TimeEventTrigger
//    {
//        public enum TriggerType
//        {
//            SpecificDateTime,
//            Daily,
//            Monthly,
//            YearLy
//        }

//        public TriggerType triggerType = TriggerType.SpecificDateTime;

//        [Tooltip("触发参数容器。\n" +
//                "- Specific: 使用 年/月/日/时\n" +
//                "- Daily: 只使用 时\n" +
//                "- Monthly: 使用 日/时\n" +
//                "- Yearly: 使用 月/日/时")]
//        public CustomDateTime targetParams;

//        [Tooltip("时间容差 (0.01 ≈ 14分钟)。\n只要当前时间超过目标时间，且在容差范围内，就算触发。")]
//        [Range(0.001f, 0.1f)]
//        public float timeTolerance = 0.01f;

//        [NonSerialized] private int _lastTriggerDayId = -1;

//        public bool IsConditionMet(CustomDateTime current)
//        {
//            if (triggerType != TriggerType.SpecificDateTime && current.ToTotalDays == _lastTriggerDayId)
//            {
//                return false; 
//            }

//            bool isMet = false;

//            switch (triggerType)
//            {
//                case TriggerType.SpecificDateTime:
//                    isMet = current >= targetParams;
//                    break;

//                case TriggerType.Daily:
//                    isMet = IsTimeReached(current.time, targetParams.time);
//                    break;
//                case TriggerType.Monthly:
//                    isMet = (current.day == targetParams.day && IsTimeReached(current.time, targetParams.time));
//                    break;
//                case TriggerType.YearLy:
//                    isMet = (current.month == targetParams.month) &&
//                            (current.day == targetParams.day) &&
//                            IsTimeReached(current.time, targetParams.time);
//                    break;
//            }

//            if (isMet)
//            {
//                _lastTriggerDayId = current.ToTotalDays;
//            }

//            return isMet;
//        }


//        private bool IsTimeReached(float currentTime, float targetTime)
//        {
//            return currentTime >= targetTime && currentTime < (targetTime + timeTolerance);
//        }

//        public void ResetState()
//        {
//            _lastTriggerDayId = -1;
//        }
//    }
//}