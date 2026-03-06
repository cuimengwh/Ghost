using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Utopia.Core.Event;
using Utopia.TimeSystem;

namespace Utopia.TimeTool
{
    /// <summary>
    /// 时间触发事件，包含时间触发相关信息
    /// </summary>
    public class TimeTriggeredEvent : GameEventBase
    {
        public string EventName { get; private set; } // 哪个事件被触发了
        public CustomDateTime TriggerTime { get; private set; } // 触发的具体时间

        public TimeTriggeredEvent(string eventName, CustomDateTime triggerTime)
        {
            EventName = eventName;
            TriggerTime = triggerTime;
        }
    }
}
