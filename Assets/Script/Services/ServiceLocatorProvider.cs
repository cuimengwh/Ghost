using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utopia.Core.Services
{
    /// <summary>
    /// 挂载在场景物体上的服务提供者组件。
    /// 用于在Unity场景中提供分层依赖注入容器，支持全局和局部服务定位器。
    /// </summary>
    [DefaultExecutionOrder(-1000)] // 确保最先初始化，比大多数脚本更早执行
    public class ServiceLocatorProvider : MonoBehaviour
    {
        /// <summary>
        /// 全局唯一的服务提供者实例，用于跨场景访问核心服务。
        /// </summary>
        public static ServiceLocatorProvider Global { get; private set; }

        [Header("配置选项")]
        [Tooltip("是否为全局服务提供者，全局只能存在一个实例")]
        [SerializeField] private bool _isGlobal = false;

        [Tooltip("场景切换时是否保持实例不被销毁（仅对全局实例有效）")]
        [SerializeField] private bool _dontDestroyOnLoad = false;

        [Tooltip("是否自动为当前GameObject注入依赖")]
        [SerializeField] private bool _autoInjectSelf = true;

        [Header("预注册组件")]
        [Tooltip("在Awake时自动注册到服务定位器的组件列表")]
        [SerializeField] private List<Component> _autoRegisterComponents;

        /// <summary>
        /// 分层服务定位器实例，负责实际的依赖注入和服务解析。
        /// </summary>
        private HierarchicalServiceLocator _locator;

        /// <summary>
        /// 公开访问的服务定位器属性。
        /// </summary>
        public HierarchicalServiceLocator Locator => _locator;

        /// <summary>
        /// Unity生命周期方法：在对象初始化时最早调用。
        /// 负责初始化服务定位器并注册预定义组件。
        /// </summary>
        private void Awake()
        {
            SetUpLocator();  // 创建并配置分层服务定位器
            RegisterPredefinedComponents();  // 注册预配置的组件到服务容器

            // 如果启用自动注入，为当前GameObject注入所有依赖
            if (_autoInjectSelf)
            {
                _locator.Inject(this);
            }
        }

        /// <summary>
        /// 设置服务定位器，根据配置创建全局或局部定位器。
        /// </summary>
        private void SetUpLocator()
        {
            // 处理全局服务定位器的情况
            if (_isGlobal)
            {
                // 确保全局实例的唯一性
                if (Global != null && Global != this)
                {
                    // 如果已存在全局实例，销毁当前重复的实例
                    Destroy(gameObject);
                    return;
                }

                // 设置当前实例为全局实例
                Global = this;

                // 根据配置决定是否跨场景持久化
                if (_dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);

                // 创建无父级的新服务定位器（顶级定位器）
                _locator = new HierarchicalServiceLocator(null);
            }
            else
            {
                // 创建局部服务定位器，继承全局定位器的服务（如果存在）
                var parent = Global != null ? Global.Locator : null;
                _locator = new HierarchicalServiceLocator(parent);
            }
        }

        /// <summary>
        /// 注册所有预先配置的组件到服务定位器。
        /// </summary>
        private void RegisterPredefinedComponents()
        {
            // 安全性检查：确保定位器已初始化
            if (_locator == null) return;

            // 遍历所有预配置的组件
            foreach (var component in _autoRegisterComponents)
            {
                // 跳过空引用
                if (component == null) continue;

                // 注册组件及其实现的接口
                RegisterComponentAndInterfaces(component);
            }
        }

        /// <summary>
        /// 注册组件及其实现的非系统/Unity接口到服务定位器。
        /// </summary>
        /// <param name="component">要注册的Unity组件</param>
        private void RegisterComponentAndInterfaces(Component component)
        {
            var type = component.GetType();

            // 1. 以具体类型注册组件
            _locator.Register(type, component);

            // 2. 获取组件实现的所有接口
            var interfaces = type.GetInterfaces();

            // 3. 为每个自定义接口注册服务（排除系统接口和Unity接口）
            foreach (var interfaceType in interfaces)
            {
                // 过滤条件：非空命名空间，且不属于System或UnityEngine命名空间
                if (interfaceType.Namespace != null &&
                    !interfaceType.Namespace.StartsWith("System") &&
                    !interfaceType.Namespace.StartsWith("UnityEngine"))
                {
                    // 以接口类型注册组件，支持接口注入
                    _locator.Register(interfaceType, component);
                }
            }
        }

        /// <summary>
        /// Unity生命周期方法：当对象被销毁时调用。
        /// 清理服务定位器资源，重置全局引用。
        /// </summary>
        private void OnDestroy()
        {
            // 如果这是全局实例，清除全局引用
            if (_isGlobal && Global == this)
                Global = null;

            // 释放服务定位器资源
            _locator?.Dispose();
        }
    }
}