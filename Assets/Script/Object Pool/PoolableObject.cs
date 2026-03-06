using System.Collections;
using UnityEngine;

namespace Utopia.Core.Services
{
    /// <summary>
    /// 可池化对象基类
    /// 挂载在需要被对象池管理的 GameObject 上，提供自动/手动回收、延迟回收等核心能力
    /// 与 IObjectPoolService 配合使用，是对象池系统的核心配套组件
    /// </summary>
    public class PoolableObject : MonoBehaviour
    {
        [Header("池配置")]
        [Tooltip("当前对象所属的对象池唯一标识Key，需与创建池子时的poolKey保持一致")]
        public string PoolKey; // 用于标识对象属于哪个池

        /// <summary>
        /// 储存延迟回收的协程引用，用于中途停止延迟回收（如对象被重新激活时）
        /// </summary>
        private Coroutine _returnCoroutine;

        /// <summary>
        /// 缓存的对象池服务引用，避免重复从服务定位器获取，提升性能
        /// </summary>
        private IObjectPoolService _cachedPoolService;

        /// <summary>
        /// 静态标志位：标记游戏是否正在退出
        /// 防止游戏退出时触发对象池回收逻辑，导致 Unity 报 "Destroying object during exit" 错误
        /// </summary>
        private static bool _isQuitting = false;

        #region 生命周期回调
        /// <summary>
        /// Unity 应用退出时的回调
        /// 标记游戏退出状态，避免退出时执行回收逻辑引发报错
        /// </summary>
        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }
        #endregion

        #region 对象池回调（供外部调用）
        /// <summary>
        /// 对象从池中被取出时的回调（由 ObjectPoolServiceImpl 调用）
        /// 子类可重写此方法，实现对象取出时的状态重置（如重置位置、清空数据、开启特效等）
        /// </summary>
        public virtual void OnGetFromPool() { }

        /// <summary>
        /// 对象被回收至池中时的回调（由 ObjectPoolServiceImpl 调用）
        /// 子类可重写此方法，实现对象回收时的状态清理（如停止特效、重置动画、清空缓存等）
        /// </summary>
        public virtual void OnReturnToPool()
        {
            // 如果有未完成的延迟回收协程，先停止它，避免重复回收
            if (_returnCoroutine != null)
            {
                StopCoroutine(_returnCoroutine);
                _returnCoroutine = null;
            }
        }

        /// <summary>
        /// 对象被池子销毁时的回调（由 ObjectPoolServiceImpl 调用，仅当池子超出最大容量时触发）
        /// 子类可重写此方法，实现对象销毁时的资源释放（如释放非托管资源、取消事件订阅等）
        /// </summary>
        public virtual void OnDestroyInPool() { }
        #endregion

        #region 回收方法（核心API）
        /// <summary>
        /// 将对象回收至所属的对象池（支持延迟回收）
        /// 是外部调用回收的主要入口方法
        /// </summary>
        /// <param name="delay">延迟回收的时间（秒），默认0表示立即回收</param>
        public void ReturnToPool(float delay = 0f)
        {
            // 游戏退出时直接返回，避免执行回收逻辑引发报错
            if (_isQuitting) return;

            // 如果设置了延迟时间，且对象当前是激活状态，则启动延迟回收协程
            if (delay > 0f && gameObject.activeInHierarchy)
            {
                // 如果已经有一个回收协程在运行，先停止它（避免同一对象多次触发延迟回收）
                if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
                _returnCoroutine = StartCoroutine(ReturnWithDelay(delay));
            }
            else
            {
                // 无延迟或对象未激活，直接立即回收
                ReturnToPoolImmediate();
            }
        }

        /// <summary>
        /// 延迟回收的协程逻辑
        /// 等待指定时间后执行立即回收
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        /// <returns>协程迭代器</returns>
        private IEnumerator ReturnWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPoolImmediate();
        }

        /// <summary>
        /// 立即将对象回收至所属的对象池（内部核心回收逻辑）
        /// 封装了回收的边界条件判断和服务调用逻辑
        /// </summary>
        private void ReturnToPoolImmediate()
        {
            // 游戏退出时直接返回，避免执行销毁/回收逻辑
            if (_isQuitting) return;

            // 边界检查：如果 PoolKey 为空，说明对象未归属任何池子，直接销毁
            if (string.IsNullOrEmpty(PoolKey))
            {
                Destroy(gameObject);
                return;
            }

            // 获取对象池服务并执行回收
            var service = GetPoolService();
            if (service != null)
            {
                service.ReturnToPool(PoolKey, gameObject);
            }
            else
            {
                // 兜底逻辑：找不到对象池服务时，直接销毁对象并打印警告
                Debug.LogWarning($"[PoolableObject] 未找到对象池服务，对象 {name} 将被直接销毁。PoolKey: {PoolKey}");
                Destroy(gameObject);
            }
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取对象池服务实例（带缓存，避免重复查找）
        /// 从全局服务定位器中获取 IObjectPoolService 接口实例
        /// </summary>
        /// <returns>对象池服务实例，未找到则返回null</returns>
        private IObjectPoolService GetPoolService()
        {
            // 缓存命中：直接返回已获取的服务实例
            if (_cachedPoolService != null) return _cachedPoolService;

            // 缓存未命中：从全局服务定位器查找服务
            if (ServiceLocatorProvider.Global != null &&
                ServiceLocatorProvider.Global.Locator.TryGet(out _cachedPoolService))
            {
                return _cachedPoolService;
            }

            // 服务未找到：返回null
            return null;
        }
        #endregion
    }
}