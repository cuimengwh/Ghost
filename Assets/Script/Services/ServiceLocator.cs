using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utopia.Core.Services
{
    /// <summary>
    /// 线程安全、支持懒加载工厂的非单例服务定位器。
    /// 核心服务容器，提供服务的注册、解析和管理功能。
    /// 特性：
    /// 1. 线程安全：使用读写锁(ReaderWriterLockSlim)优化并发性能
    /// 2. 懒加载：支持通过工厂方法延迟创建服务实例
    /// 3. 非单例：可创建多个独立实例，适用于不同作用域
    /// 4. 生命周期管理：支持IDisposable服务的自动释放
    /// </summary>
    public class ServiceLocator
    {
        /// <summary>
        /// 已实例化的服务存储字典
        /// Key: 服务类型（通常为接口或抽象类）
        /// Value: 已创建的服务实例
        /// </summary>
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// 服务工厂字典，用于延迟创建服务实例
        /// Key: 服务类型
        /// Value: 创建该类型实例的工厂方法
        /// 当首次请求服务时调用工厂方法，创建后移至_services字典
        /// </summary>
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// 读写锁，提供线程安全访问
        /// 使用ReaderWriterLockSlim替代lock关键字，优化读多写少的场景
        /// LockRecursionPolicy.NoRecursion: 禁止递归锁，避免死锁风险
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// 标记容器是否已释放，防止释放后继续使用
        /// </summary>
        private bool _isDisposed = false;

        /// <summary>
        /// 泛型注册方法：注册具体服务实例
        /// </summary>
        /// <typeparam name="T">服务类型（通常为接口）</typeparam>
        /// <param name="service">服务实例</param>
        /// <exception cref="ArgumentNullException">当service为null时抛出</exception>
        /// <remarks>
        /// 此方法将服务实例直接存储在容器中，立即可用。
        /// 如果同一类型已注册，会覆盖原有实例并输出警告。
        /// </remarks>
        public void Register<T>(T service) where T : class
        {
            Register(typeof(T), service);
        }

        /// <summary>
        /// 非泛型注册方法：使用Type对象注册服务
        /// </summary>
        /// <param name="type">服务类型（Type对象）</param>
        /// <param name="service">服务实例</param>
        /// <exception cref="ArgumentNullException">当service为null时抛出</exception>
        /// <remarks>
        /// 提供运行时类型注册能力，比泛型版本更灵活但类型安全性稍差。
        /// 主要用于反射场景或动态类型注册。
        /// </remarks>
        public void Register(Type type, object service)
        {
            // 参数验证
            if (service == null) throw new ArgumentNullException(nameof(service));
            // 安全性检查：如果容器已释放，静默返回
            if (_isDisposed) return;

            // 获取写锁，准备修改字典
            _lock.EnterWriteLock();
            try
            {
                // 检查是否已注册相同类型，输出警告但允许覆盖
                if (_services.ContainsKey(type))
                {
                    Debug.LogWarning($"[ServiceLocator] Service {type.Name} is being overwritten.");
                }
                // 注册或覆盖服务实例
                _services[type] = service;
            }
            finally
            {
                // 确保锁总是被释放，避免死锁
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 注册服务工厂方法，支持懒加载
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="factory">创建服务实例的工厂方法</param>
        /// <exception cref="ArgumentNullException">当factory为null时抛出</exception>
        /// <remarks>
        /// 工厂方法会在首次请求服务时被调用，实现延迟初始化。
        /// 适用场景：
        /// 1. 服务创建成本高，需要按需创建
        /// 2. 服务依赖其他可能尚未注册的服务
        /// 3. 需要控制服务创建时机
        /// </remarks>
        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (_isDisposed) return;

            _lock.EnterWriteLock();
            try
            {
                // 将泛型工厂包装为返回object的委托
                _factories[typeof(T)] = () => factory();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 获取服务实例，如果未创建则调用工厂方法创建
        /// </summary>
        /// <typeparam name="T">请求的服务类型</typeparam>
        /// <returns>服务实例</returns>
        /// <exception cref="InvalidOperationException">当服务未注册时抛出</exception>
        /// <remarks>
        /// 执行流程：
        /// 1. 尝试从已实例化字典中获取（读锁）
        /// 2. 如果未找到，尝试获取工厂方法
        /// 3. 在无锁状态下执行工厂方法（避免死锁）
        /// 4. 使用双重检查锁定模式将新实例存入字典（写锁）
        /// </remarks>
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            object instance;
            Func<object> factory = null;

            // ---------- 第1阶段：快速读取路径（读锁） ----------
            _lock.EnterReadLock();
            try
            {
                // 尝试从已实例化服务中获取
                if (_services.TryGetValue(type, out instance))
                {
                    return (T)instance; // 找到，立即返回
                }

                // 未找到实例，尝试获取工厂方法
                _factories.TryGetValue(type, out factory);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // ---------- 第2阶段：检查工厂 ----------
            // 如果没有实例也没有工厂，抛出异常
            if (factory == null)
            {
                throw new InvalidOperationException($"Service {type.Name} not registered.");
            }

            // ---------- 第3阶段：执行工厂方法（无锁状态） ----------
            // 关键优化：在锁外执行工厂方法，防止：
            // 1. 工厂方法内部再次请求锁导致死锁
            // 2. 长时间运行的工厂阻塞其他线程
            // 3. 递归依赖导致栈溢出
            object newInstance = factory();

            // ---------- 第4阶段：双重检查锁定（写锁） ----------
            _lock.EnterWriteLock();
            try
            {
                // 再次检查是否在等待锁的过程中已被其他线程创建
                if (_services.TryGetValue(type, out instance))
                {
                    // 如果已被创建，丢弃刚才创建的newInstance
                    // 注意：需要确保资源正确释放
                    if (newInstance is IDisposable disposable && !ReferenceEquals(newInstance, instance))
                    {
                        disposable.Dispose();
                    }
                    return (T)instance; // 返回已存在的实例
                }

                // 注册新创建的服务实例
                _services[type] = newInstance;
                // 创建后移除工厂（单例模式）
                // 如果需要瞬态服务，则保留工厂不删除
                _factories.Remove(type);

                return (T)newInstance;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 尝试获取服务（安全版本，不抛出异常）
        /// </summary>
        /// <typeparam name="T">请求的服务类型</typeparam>
        /// <param name="service">输出参数，找到的服务实例</param>
        /// <returns>是否成功获取服务</returns>
        /// <remarks>
        /// 此方法不会抛出异常，适合在不确定服务是否注册时使用。
        /// 如果服务未注册，返回false且service为null。
        /// </remarks>
        public bool TryGet<T>(out T service) where T : class
        {
            try
            {
                // 调用非泛型TryGet方法
                if (TryGet(typeof(T), out var result))
                {
                    service = (T)result;
                    return true;
                }
            }
            catch
            {
                // 忽略并发时的潜在异常，确保方法不会抛出
                // 注意：生产环境中可能需要记录日志
            }
            service = null;
            return false;
        }

        /// <summary>
        /// 核心尝试获取方法（非泛型版本）
        /// </summary>
        /// <param name="type">请求的服务类型</param>
        /// <param name="service">输出参数，找到的服务实例</param>
        /// <returns>是否成功获取服务</returns>
        /// <remarks>
        /// 这是所有获取操作的核心实现，处理懒加载和线程安全。
        /// </remarks>
        public bool TryGet(Type type, out object service)
        {
            service = null;
            // 安全性检查：如果容器已释放，直接返回失败
            if (_isDisposed) return false;

            Func<object> factory = null;
            
            // ---------- 第1阶段：尝试获取已存在实例（读锁） ----------
            _lock.EnterReadLock();
            try
            {
                // 查找已实例化的服务
                if (_services.TryGetValue(type, out service))
                {
                    return true; // 找到，立即返回
                }
                // 未找到，查找是否有对应的工厂方法
                _factories.TryGetValue(type, out factory);      
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // ---------- 第2阶段：检查是否存在工厂 ----------
            // 如果没有工厂方法，返回失败
            if (factory == null) return false;

            // ---------- 第3阶段：执行工厂方法（无锁状态） ----------
            // 在锁外执行工厂方法，避免潜在的死锁和性能问题
            object newInstance = factory();
            
            // ---------- 第4阶段：双重检查锁定（写锁） ----------
            _lock.EnterWriteLock();
            try
            {
                // 再次检查是否在等待锁的过程中已被其他线程创建
                if (_services.TryGetValue(type, out var existing))
                {
                    // 如果已被创建，正确释放刚才创建的实例
                    if (newInstance is IDisposable disposable && !ReferenceEquals(newInstance, existing))
                    {
                        disposable.Dispose();
                    }
                    // 将已存在的服务传出
                    service = existing;
                    return true;
                }
                
                // 注册新创建的服务实例
                _services[type] = newInstance;
                // 移除已使用的工厂（单例模式）
                _factories.Remove(type);
                service = newInstance;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 检查指定类型的服务是否已注册（包括工厂注册）
        /// </summary>
        /// <typeparam name="T">要检查的服务类型</typeparam>
        /// <returns>服务是否已注册</returns>
        /// <remarks>
        /// 注意：此方法仅检查注册状态，不创建服务实例。
        /// 即使只有工厂注册而没有实际实例，也返回true。
        /// </remarks>
        public bool IsRegistered<T>()
        {
            _lock.EnterReadLock();
            try
            {
                Type type = typeof(T);
                // 检查是否已实例化或有工厂方法
                return _services.ContainsKey(type) || _factories.ContainsKey(type);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 泛型注销方法：移除指定类型的服务和工厂
        /// </summary>
        /// <typeparam name="T">要注销的服务类型</typeparam>
        /// <remarks>
        /// 只移除容器中的注册，不会调用服务的Dispose方法。
        /// 如果需要释放资源，请先手动调用服务的Dispose。
        /// </remarks>
        public void Unregister<T>() => Unregister(typeof(T));

        /// <summary>
        /// 非泛型注销方法：移除指定类型的服务和工厂
        /// </summary>
        /// <param name="type">要注销的服务类型</param>
        public void Unregister(Type type)
        {
            if (_isDisposed) return;
            
            _lock.EnterWriteLock();
            try
            {
                _services.Remove(type);
                _factories.Remove(type);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 释放容器资源
        /// </summary>
        /// <remarks>
        /// 执行以下操作：
        /// 1. 释放所有实现IDisposable的服务
        /// 2. 清空服务和工厂字典
        /// 3. 释放读写锁
        /// 4. 标记容器为已释放状态
        /// 
        /// 注意：释放后容器不可再用。
        /// </remarks>
        public void Dispose()
        {
            if (_isDisposed) return;
            
            // 注意：这里应该使用EnterWriteLock而不是ExitWriteLock
            // 修正：获取写锁以安全地清理资源
            _lock.EnterWriteLock();
            try
            {
                // 释放所有实现IDisposable的服务
                foreach (var service in _services.Values)
                {
                    if (service is IDisposable disposable) 
                        disposable.Dispose();
                }
                // 清空字典
                _services.Clear();
                _factories.Clear();
                // 标记为已释放
                _isDisposed = true;
            }
            finally
            {
                _lock.ExitWriteLock();
                // 释放读写锁资源
                _lock.Dispose();
            }
        }

        /// <summary>
        /// 调试用：获取内部服务列表的快照
        /// </summary>
        /// <returns>包含服务类型和描述信息的字典</returns>
        /// <remarks>
        /// 仅供调试使用，生产环境中可能不需要。
        /// 返回的字典是副本，不会影响原始数据。
        /// </remarks>
        public Dictionary<Type, string> GetDebugSnapshot()
        {
            var result = new Dictionary<Type, string>();
            _lock.EnterReadLock();
            try
            {
                // 添加已实例化服务
                foreach (var kvp in _services) 
                    result[kvp.Key] = kvp.Value.ToString();
                // 添加工厂信息
                foreach (var kvp in _factories) 
                    result[kvp.Key] = "(Lazy Factory)";
            }
            finally 
            { 
                _lock.ExitReadLock(); 
            }
            return result;
        }
    }
}