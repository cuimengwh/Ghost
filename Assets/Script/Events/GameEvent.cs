using UnityEngine;

namespace Utopia.Core.Event
{
    /// <summary>
    /// 所有事件基类，用于定义事件的存在，包含基础必须属性
    /// 其余事件继承自此
    /// </summary>
    public abstract class GameEventBase
    {
        // 时间戳，用于调试和事件排序
        public float Timestamp { get; }
        // 事件发送者
        public object Sender { get; protected set; }
        
        // 无参构造函数,初始化时间戳
        protected GameEventBase() 
        { 
            Timestamp = Time.time; 
        }

        // 含参(object)构造函数，初始化时间戳和事件发送者
        protected GameEventBase(object _serder) : this()
        {
            Sender = _serder;
        }
    }
}