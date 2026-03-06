using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.tvOS;
using UnityEngine.UIElements;

namespace Utopia.Core.Event
{
    /// <summary>
    /// 事件管理器行为接口。
    /// 定义了事件系统的核心操作：订阅、取消订阅、发布、查询与清理。
    /// 实现此接口的类应保证线程安全（建议使用锁机制）。
    /// </summary>
    public interface IEventManager
    {
        /// <summary>
        /// 订阅指定类型的事件。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <param name="handler">事件处理回调方法</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        void Subscribe<IEvent>(Action<IEvent> handler) where IEvent : GameEventBase;

        /// <summary>
        /// 取消订阅指定类型的事件。
        /// 如果取消后该事件类型没有任何订阅者，将自动从内部字典中移除。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <param name="handler">之前订阅时使用的相同回调方法</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        void Unsubscribe<IEvent>(Action<IEvent> handler) where IEvent : GameEventBase;

        /// <summary>
        /// 发布指定类型的事件，所有订阅者将收到此事件数据。
        /// 发布过程是线程安全的，并且会捕获单个处理器抛出的异常，避免影响其他处理器。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <param name="eventData">要发布的事件实例</param>
        /// <exception cref="ArgumentNullException">当 eventData 为 null 时抛出</exception>
        void Publish<IEvent>(IEvent eventData) where IEvent : GameEventBase;

        /// <summary>
        /// 检测指定事件类型是否存在至少一个订阅者。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <returns>如果有任何订阅者则返回 true，否则返回 false</returns>
        bool HasSubscribers<IEvent>() where IEvent : GameEventBase;

        /// <summary>
        /// 清除所有事件的所有订阅者。
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 清除指定事件类型的所有订阅者。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        void ClearEvent<IEvent>() where IEvent : GameEventBase;

        /// <summary>
        /// 获取指定事件类型的当前订阅者数量。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <returns>订阅者数量，如果无订阅者则返回 0</returns>
        int GetSubscriberCount<IEvent>() where IEvent : GameEventBase;
    }

    /// <summary>
    /// 全局事件总线。
    /// 纯 C# 实现，独立于 Unity 生命周期，适用于任何需要事件通信的场景。
    /// 无单例模式，建议通过依赖注入或服务定位器访问。
    /// 遵循订阅-发布模式，用于模块间解耦通信。
    /// 所有公开方法均为线程安全（使用对象锁）。
    /// </summary>
    public class EventManager : IEventManager
    {
        #region 内部类：安全的处理器列表

        /// <summary>
        /// 封装特定事件类型的处理器列表，并提供在发布过程中安全遍历的能力。
        /// 使用快照机制（_handlersToInvoke）避免在遍历过程中修改集合导致的异常。
        /// </summary>
        private class EventHandlerList
        {
            /// <summary>
            /// 非运行时事件处理器列表。
            /// 允许在事件发布过程中修改（添加/移除）订阅者，修改不会影响正在进行的调用。
            /// </summary>
            public List<Delegate> Handlers { get; } = new();

            /// <summary>
            /// 运行时事件处理器列表快照。
            /// 仅在 IsInvoking 为 true 时使用，存储 BeginInvoke 时 Handlers 的副本。
            /// </summary>
            private List<Delegate> _handlersToInvoke;

            /// <summary>
            /// 是否正在发布事件（正在调用处理器）。
            /// 控制 GetInvocationList 返回的是原始列表还是快照。
            /// </summary>
            public bool IsInvoking { get; private set; }

            /// <summary>
            /// 添加一个事件处理器。
            /// </summary>
            /// <param name="handler">要添加的委托</param>
            public void Add(Delegate handler) => Handlers.Add(handler);

            /// <summary>
            /// 移除一个事件处理器。
            /// </summary>
            /// <param name="handler">要移除的委托</param>
            /// <returns>成功移除返回 true，否则 false</returns>
            public bool Remove(Delegate handler) => Handlers.Remove(handler);

            /// <summary>
            /// 获取当前应该被调用的处理器列表。
            /// 如果在发布过程中，返回 Handlers 的快照；否则直接返回 Handlers。
            /// </summary>
            /// <returns>用于调用的委托列表</returns>
            public List<Delegate> GetInvocationList()
            {
                if (IsInvoking)
                {
                    // 惰性创建快照列表，并复制当前 Handlers 内容
                    _handlersToInvoke ??= new List<Delegate>();
                    _handlersToInvoke.Clear();
                    _handlersToInvoke.AddRange(Handlers);
                    return _handlersToInvoke;
                }
                return Handlers;
            }

            /// <summary>
            /// 标记开始发布事件。
            /// 应在调用处理器之前调用。
            /// </summary>
            public void BeginInvoke() => IsInvoking = true;

            /// <summary>
            /// 标记结束发布事件。
            /// 清理快照列表，并重置状态。
            /// 应在调用处理器之后调用（包含异常处理）。
            /// </summary>
            public void EndInvoke()
            {
                IsInvoking = false;
                _handlersToInvoke?.Clear(); // 避免 null 引用
            }
        }

        #endregion

        /// <summary>
        /// 存储所有事件类型与其对应处理器列表的字典。
        /// 键：事件类型（Type）
        /// 值：该事件类型的处理器封装对象 <see cref="EventHandlerList"/>
        /// </summary>
        private readonly Dictionary<Type, EventHandlerList> _eventHandlers = new();

        /// <summary>
        /// 线程同步锁对象。
        /// 用于保证所有订阅、取消订阅、发布、查询操作在多线程环境下的原子性和可见性。
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// 日志开关。
        /// 当设置为 true 时，会在控制台输出订阅、取消订阅、发布等关键操作的调试信息。
        /// 默认关闭，可通过其他方式（如配置文件、Inspector）动态修改。
        /// </summary>
        private bool _enableLogging = false;

        /// <summary>
        /// 订阅指定类型的事件。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <param name="handler">事件处理回调方法</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public void Subscribe<IEvent>(Action<IEvent> handler) where IEvent : GameEventBase
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                Type eventType = typeof(IEvent);
                if (!_eventHandlers.TryGetValue(eventType, out var handlerList))
                {
                    handlerList = new EventHandlerList();
                    _eventHandlers[eventType] = handlerList;
                }

                if (handlerList.Handlers.Contains(handler))
                {
                    Debug.LogWarning($"重复订阅事件: {eventType.Name}");
                    return;
                }

                handlerList.Add(handler);

                if (_enableLogging)
                {
                    Debug.Log($"订阅事件: {eventType.Name}, 处理器: {handler.Method.Name}");
                }
            }
        }

        /// <summary>
        /// 取消订阅指定类型的事件。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <param name="handler">之前订阅时使用的相同回调方法</param>
        /// <exception cref="ArgumentNullException">当 handler 为 null 时抛出</exception>
        public void Unsubscribe<IEvent>(Action<IEvent> handler) where IEvent : GameEventBase
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            lock (_lock)
            {
                Type eventType = typeof(IEvent);
                if (_eventHandlers.TryGetValue(eventType, out var handlerList))
                {
                    bool removed = handlerList.Remove(handler);

                    if (handlerList.Handlers.Count == 0)
                    {
                        _eventHandlers.Remove(eventType);
                    }

                    if (_enableLogging && removed)
                        Debug.Log($"[EventManager] 取消订阅: {eventType.Name}");
                }
            }
        }

        /// <summary>
        /// 发布指定类型的事件，所有订阅者将收到此事件数据。
        /// 发布过程是线程安全的，并且会捕获单个处理器抛出的异常，避免影响其他处理器。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <param name="eventData">要发布的事件实例</param>
        /// <exception cref="ArgumentNullException">当 eventData 为 null 时抛出</exception>
        public void Publish<IEvent>(IEvent eventData) where IEvent : GameEventBase
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            Type eventType = typeof(IEvent);
            EventHandlerList handlerList;

            lock (_lock)
            {
                if (!_eventHandlers.TryGetValue(eventType, out handlerList))
                {
                    if (_enableLogging)
                        Debug.Log($"[EventManager] 发布事件（无订阅者）: {eventType.Name}");
                    return;
                }
            }

            try
            {
                handlerList.BeginInvoke();
                var handlersToInvoke = handlerList.GetInvocationList();

                if (_enableLogging)
                    Debug.Log($"[EventManager] 发布事件: {eventType.Name}, 订阅者: {handlersToInvoke.Count}");

                foreach (Delegate delegateHandler in handlersToInvoke)
                {
                    try
                    {
                        if (delegateHandler is Action<IEvent> action)
                        {
                            action.Invoke(eventData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventManager] 事件处理器异常: {ex.Message}");
                    }
                }
            }
            finally
            {
                handlerList.EndInvoke();
            }
        }

        /// <summary>
        /// 检测指定事件类型是否存在至少一个订阅者。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <returns>如果有任何订阅者则返回 true，否则返回 false</returns>
        public bool HasSubscribers<IEvent>() where IEvent : GameEventBase
        {
            lock (_lock)
            {
                return _eventHandlers.ContainsKey(typeof(IEvent));
            }
        }

        /// <summary>
        /// 获取指定事件类型的当前订阅者数量。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        /// <returns>订阅者数量，如果无订阅者则返回 0</returns>
        public int GetSubscriberCount<IEvent>() where IEvent : GameEventBase
        {
            lock (_lock)
            {
                return _eventHandlers.TryGetValue(typeof(IEvent), out var handlerList) ? handlerList.Handlers.Count : 0;
            }
        }

        /// <summary>
        /// 清除所有事件的所有订阅者。
        /// </summary>
        public void ClearAll()
        {
            lock (_lock)
            {
                _eventHandlers.Clear();
                if (_enableLogging)
                    Debug.Log("[EventManager] 已清空所有事件订阅");
            }
        }

        /// <summary>
        /// 清除指定事件类型的所有订阅者。
        /// </summary>
        /// <typeparam name="IEvent">事件类型，必须继承自 <see cref="GameEventBase"/></typeparam>
        public void ClearEvent<IEvent>() where IEvent : GameEventBase
        {
            lock (_lock)
            {
                if (_eventHandlers.Remove(typeof(IEvent)) && _enableLogging)
                {
                    Debug.Log($"[EventManager] 已清除事件订阅: {typeof(IEvent).Name}");
                }
            }
        }

        #region 便捷方法（可选，主要用于调试）

        /// <summary>
        /// 获取当前已注册的所有事件类型列表。
        /// 该方法主要用于调试和监控工具。
        /// </summary>
        /// <returns>包含所有事件 Type 对象的列表</returns>
        public List<Type> GetAllEventTypes()
        {
            lock (_lock)
            {
                return new List<Type>(_eventHandlers.Keys);
            }
        }

        /// <summary>
        /// 获取事件订阅统计信息。
        /// 以字典形式返回每种事件类型及其对应的订阅者数量。
        /// 该方法主要用于调试和性能分析。
        /// </summary>
        /// <returns>键为事件类型名称（字符串），值为订阅者数量</returns>
        public Dictionary<string, int> GetEventStatistics()
        {
            lock (_lock)
            {
                var stats = new Dictionary<string, int>();
                foreach (var kvp in _eventHandlers)
                {
                    stats[kvp.Key.Name] = kvp.Value.Handlers.Count;
                }
                return stats;
            }
        }

        #endregion
    }
}