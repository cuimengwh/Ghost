using System;  // 引入基础系统命名空间，提供核心功能
using UnityEngine;  // 引入Unity引擎命名空间，提供调试日志功能
using System.Reflection;  // 引入反射命名空间，提供成员信息查询功能

namespace Utopia.Core.Services
{
    /// <summary>
    /// 服务注入标记特性。
    /// 用于标识需要通过依赖注入自动赋值的字段或属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ServiceInjectAttribute : Attribute
    {
        // 这是一个标记特性，不包含额外逻辑
        // 仅用于在反射扫描时识别需要注入的成员
    }

    /// <summary>
    /// 服务定位器的静态扩展方法类。
    /// 为HierarchicalServiceLocator添加依赖注入功能。
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// 执行依赖注入操作。
        /// 扫描目标对象中标记了[ServiceInject]特性的所有字段，
        /// 并从服务定位器中获取对应服务实例进行自动赋值。
        /// </summary>
        /// <param name="locator">服务定位器实例</param>
        /// <param name="target">需要注入服务的对象实例</param>
        /// <remarks>
        /// 典型使用场景：
        /// 1. 在MonoBehaviour的Awake或Start方法中调用
        /// 2. 在普通类的构造函数或初始化方法中调用
        /// 
        /// 示例：
        /// <code>
        /// public class PlayerController : MonoBehaviour
        /// {
        ///     [ServiceInject]
        ///     private IAudioService _audioService;
        ///     
        ///     private void Start()
        ///     {
        ///         ServiceLocatorProvider.Global.Locator.Inject(this);
        ///         _audioService.PlaySound("startup"); // 服务已可用
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static void Inject(this HierarchicalServiceLocator locator, object target)
        {
            // 参数验证：确保目标对象不为空
            if (target == null)
            {
                Debug.LogError("[Injector] 注入失败：目标对象为null");
                return;
            }

            // 获取目标对象的实际运行时类型
            Type targetType = target.GetType();

            // 获取目标类型的所有实例字段（包括公有和私有）
            FieldInfo[] allFields = targetType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            // 遍历所有字段，检查是否需要注入
            foreach (FieldInfo field in allFields)
            {
                // 检查字段是否标记了[ServiceInject]特性
                bool requiresInjection = Attribute.IsDefined(field, typeof(ServiceInjectAttribute));

                if (requiresInjection)
                {
                    // 获取字段声明的服务类型
                    Type serviceType = field.FieldType;

                    // 尝试从服务定位器获取服务实例
                    bool serviceFound = locator.TryGet(serviceType, out object serviceInstance);

                    if (serviceFound && serviceInstance != null)
                    {
                        try
                        {
                            // 将服务实例赋值给目标字段
                            field.SetValue(target, serviceInstance);

                            // 调试时可启用以下日志
                            // Debug.Log($"[Injector] 成功注入 {serviceType.Name} 到 {targetType.Name}.{field.Name}");
                        }
                        catch (ArgumentException ex)
                        {
                            // 类型不匹配：服务实例类型无法赋值给字段
                            Debug.LogError($"[Injector] 类型不匹配：无法将 {serviceInstance.GetType().Name} 赋值给 {targetType.Name}.{field.Name} ({serviceType.Name})。错误：{ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            // 其他意外错误（如字段只读等）
                            Debug.LogError($"[Injector] 注入字段时发生异常：{targetType.Name}.{field.Name}。错误：{ex.Message}");
                        }
                    }
                    else
                    {
                        // 服务未找到，记录错误但不中断程序
                        Debug.LogError($"[Injector] 注入失败：未找到类型 {serviceType.Name} 的服务，无法注入到 {targetType.Name}.{field.Name}");
                    }
                }
            }

            // 注意：当前版本仅支持字段注入
            // 如需支持属性注入，可参考以下代码扩展：
            /*
            PropertyInfo[] allProperties = targetType.GetProperties(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );
            foreach (PropertyInfo property in allProperties)
            {
                if (Attribute.IsDefined(property, typeof(ServiceInjectAttribute)) && property.CanWrite)
                {
                    Type serviceType = property.PropertyType;
                    if (locator.TryGet(serviceType, out var service))
                    {
                        property.SetValue(target, service);
                    }
                }
            }
            */
        }
    }
}