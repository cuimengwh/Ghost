using System;
using Unity.Collections;

namespace Utopia.Core.Services
{
    /// <summary>
    /// 分层服务定位器，支持服务查找的层级继承机制。
    /// 允许创建父子层级的服务容器，子容器可以访问父容器的服务。
    /// 实现 IDisposable 接口以确保资源正确释放。
    /// </summary>
    public class HierarchicalServiceLocator : IDisposable
    {
        /// <summary>
        /// 当前层级（本地）的服务容器，存储本层注册的所有服务实例。
        /// 使用 ServiceLocator 类作为底层容器实现。
        /// </summary>
        private readonly ServiceLocator _container = new ServiceLocator();

        /// <summary>
        /// 上级（父级）服务定位器的引用，用于服务查找的向上回溯。
        /// 当在当前层级找不到服务时，会尝试从父级查找。
        /// 如果为 null，表示这是顶层（根）定位器。
        /// </summary>
        private readonly HierarchicalServiceLocator _parent;

        /// <summary>
        /// 构造函数，创建分层服务定位器。
        /// </summary>
        /// <param name="parent">父级服务定位器，如果为null则创建顶层定位器</param>
        /// <remarks>
        /// 分层设计模式：允许创建服务查找的层次结构。
        /// 典型应用场景：
        /// 1. 全局服务注册在根定位器
        /// 2. 场景特定服务注册在场景定位器
        /// 3. 物体特定服务注册在物体定位器
        /// 每个子定位器可以访问父定位器的服务，形成服务查找链。
        /// </remarks>
        public HierarchicalServiceLocator(HierarchicalServiceLocator parent = null)
        {
            _parent = parent;
        }

        #region 服务注册方法

        /// <summary>
        /// 注册指定类型的服务实例到当前层级容器。
        /// </summary>
        /// <typeparam name="T">要注册的服务类型（通常为接口或抽象类）</typeparam>
        /// <param name="service">服务实例</param>
        /// <remarks>
        /// 注意：此方法将服务注册到当前层级，不会影响父级或子级容器。
        /// 如果同一类型已注册，通常会覆盖原有注册（取决于 ServiceLocator 的实现）。
        /// </remarks>
        public void Register<T>(T service) where T : class => _container.Register(service);

        /// <summary>
        /// 使用运行时类型信息注册服务实例。
        /// </summary>
        /// <param name="type">要注册的服务类型（Type对象）</param>
        /// <param name="service">服务实例</param>
        /// <remarks>
        /// 此方法允许在运行时动态确定注册类型，比泛型版本更灵活但类型安全性稍差。
        /// </remarks>
        public void Register(Type type, object service) => _container.Register(type, service);

        /// <summary>
        /// 注册服务工厂方法，延迟创建服务实例。
        /// </summary>
        /// <typeparam name="T">要注册的服务类型</typeparam>
        /// <param name="factory">工厂方法，在需要时调用以创建服务实例</param>
        /// <remarks>
        /// 适用场景：
        /// 1. 服务创建成本高，需要延迟初始化
        /// 2. 服务实例需要每次请求时重新创建（瞬态服务）
        /// 3. 服务创建需要依赖其他服务
        /// </remarks>
        public void RegisterFactory<T>(Func<T> factory) where T : class => _container.RegisterFactory(factory);

        /// <summary>
        /// 从当前层级容器中注销指定类型的服务。
        /// </summary>
        /// <typeparam name="T">要注销的服务类型</typeparam>
        /// <remarks>
        /// 只影响当前层级容器，不影响父级容器中的注册。
        /// 注意：注销后，后续通过父级容器可能仍然能访问到该服务（如果父级有注册）。
        /// </remarks>
        public void Unregister<T>() where T : class => _container.Unregister<T>();

        #endregion

        #region 服务解析方法

        /// <summary>
        /// 获取指定类型的服务实例（强制版本）。
        /// 按照"当前层级 → 父级 → 父级的父级..."的顺序查找服务。
        /// </summary>
        /// <typeparam name="T">请求的服务类型</typeparam>
        /// <returns>找到的服务实例</returns>
        /// <exception cref="InvalidOperationException">
        /// 当整个层级链中都找不到指定类型的服务时抛出
        /// </exception>
        /// <remarks>
        /// 查找顺序遵循"就近原则"：优先使用当前层级的注册，
        /// 这允许子容器覆盖父容器的服务实现。
        /// </remarks>
        public T Get<T>() where T : class
        {
            // 尝试在整个层级链中查找服务
            if (TryGet(typeof(T), out var result))
            {
                return (T)result;
            }

            // 服务未找到，抛出异常
            throw new InvalidOperationException($"Service {typeof(T).Name} not found in hierarchy.");
        }

        /// <summary>
        /// 尝试获取指定类型的服务实例（安全版本）。
        /// </summary>
        /// <typeparam name="T">请求的服务类型</typeparam>
        /// <param name="service">输出参数，找到的服务实例；如果未找到则为null</param>
        /// <returns>是否成功找到服务</returns>
        public bool TryGet<T>(out T service) where T : class
        {
            // 使用非泛型版本进行查找
            if (TryGet(typeof(T), out var result))
            {
                service = (T)result;
                return true;
            }

            // 未找到，设置输出参数为null
            service = null;
            return false;
        }

        /// <summary>
        /// 核心查找方法：按照分层顺序查找指定类型的服务。
        /// </summary>
        /// <param name="type">要查找的服务类型</param>
        /// <param name="service">输出参数，找到的服务实例</param>
        /// <returns>是否成功找到服务</returns>
        /// <remarks>
        /// 查找流程：
        /// 1. 在当前层级容器中查找
        /// 2. 如果未找到且存在父级，递归在父级中查找
        /// 3. 如果整个层级链中都未找到，返回false
        /// 
        /// 这是一个深度优先的向上查找过程。
        /// </remarks>
        public bool TryGet(Type type, out object service)
        {
            // 第一步：在当前层级容器中查找
            if (_container.TryGet(type, out service))
            {
                return true; // 找到，立即返回
            }

            // 第二步：如果存在父级，尝试在父级中查找
            if (_parent != null)
            {
                // 递归调用父级的TryGet方法
                // 注意：这里直接返回父级的查找结果，成功或失败都传递
                return _parent.TryGet(type, out service);
            }

            // 第三步：当前层级未找到且没有父级，查找失败
            service = null;
            return false;
        }

        #endregion

        #region 容器管理

        /// <summary>
        /// 创建以当前定位器为父级的子定位器。
        /// </summary>
        /// <returns>新的子级服务定位器</returns>
        /// <remarks>
        /// 典型应用：
        /// 1. 为每个场景创建子定位器，继承全局服务
        /// 2. 为复杂物体创建子定位器，继承场景服务
        /// 3. 实现服务的隔离作用域
        /// </remarks>
        public HierarchicalServiceLocator CreateChild()
        {
            return new HierarchicalServiceLocator(this);
        }

        /// <summary>
        /// 释放当前层级容器占用的资源。
        /// 实现IDisposable接口，遵循资源释放模式。
        /// </summary>
        /// <remarks>
        /// 注意：
        /// 1. 只释放当前层级的容器，不释放父级容器
        /// 2. 子容器应该在父容器之前被释放
        /// 3. 释放后不应再使用此定位器
        /// </remarks>
        public void Dispose()
        {
            _container.Dispose();
        }

        #endregion

        #region 扩展方法和辅助属性（可选，根据实际需求添加）

        /// <summary>
        /// 检查当前层级是否注册了指定类型的服务（不查找父级）。
        /// </summary>
        /// <typeparam name="T">要检查的服务类型</typeparam>
        /// <returns>当前层级是否注册了该服务</returns>
        public bool IsRegisteredLocally<T>() where T : class
        {
            return _container.IsRegistered<T>();
        }

        /// <summary>
        /// 检查整个层级链中是否存在指定类型的服务。
        /// </summary>
        /// <typeparam name="T">要检查的服务类型</typeparam>
        /// <returns>整个层级链中是否存在该服务</returns>
        public bool IsRegisteredInHierarchy<T>() where T : class
        {
            return TryGet<T>(out _);
        }

        /// <summary>
        /// 获取父级定位器（只读）。
        /// 可用于调试或高级场景。
        /// </summary>
        public HierarchicalServiceLocator Parent => _parent;

        #endregion
    }
}