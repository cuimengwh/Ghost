using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;
using Utopia.Core.Event;

namespace Utopia.Core.Services
{
    /// <summary>
    /// 对象池服务接口，定义对象池的创建、销毁、获取和回收操作。
    /// </summary>
    public interface IObjectPoolService
    {
        #region 创建与销毁
        /// <summary>
        /// 创建对象池。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="prefab">要池化的预制体。</param>
        /// <param name="defaultCapacity">默认容量。</param>
        /// <param name="maxSize">最大容量（超出则销毁对象）。</param>
        void CreatePool(string poolKey, GameObject prefab, int defaultCapacity = 10, int maxSize = 100);

        /// <summary>
        /// 销毁指定的对象池。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        void DisposePool(string poolKey);

        /// <summary>
        /// 销毁所有对象池。
        /// </summary>
        void DisposeAllPools();
        #endregion

        #region 获取对象
        /// <summary>
        /// 从池子获取对象（默认位置/旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <returns>池化的游戏对象。</returns>
        GameObject GetFromPool(string poolKey);

        /// <summary>
        /// 从池子获取对象（指定位置和旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="position">对象位置。</param>
        /// <param name="rotation">对象旋转。</param>
        /// <returns>池化的游戏对象。</returns>
        GameObject GetFromPool(string poolKey, Vector3 position, Quaternion rotation);

        /// <summary>
        /// 从池子获取对象（指定父节点）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="parent">父节点Transform。</param>
        /// <param name="worldPositionStays">是否保持世界坐标。</param>
        /// <returns>池化的游戏对象。</returns>
        GameObject GetFromPool(string poolKey, Transform parent, bool worldPositionStays = false);

        /// <summary>
        /// 从池子获取对象并返回指定组件（泛型重载）。
        /// </summary>
        /// <typeparam name="T">要获取的组件类型。</typeparam>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <returns>对象上的指定组件，如果对象不存在或没有该组件则返回null。</returns>
        T GetFromPool<T>(string poolKey) where T : Component;

        /// <summary>
        /// 批量从池子获取对象（默认位置/旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="count">要获取的对象数量。</param>
        /// <returns>获取到的对象列表（数量可能少于count，例如池子不存在时返回空列表）。</returns>
        List<GameObject> GetMultipleFromPool(string poolKey, int count);

        /// <summary>
        /// 批量从池子获取对象（指定统一的位置/旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="count">要获取的对象数量。</param>
        /// <param name="position">所有对象的统一位置。</param>
        /// <param name="rotation">所有对象的统一旋转。</param>
        /// <returns>获取到的对象列表。</returns>
        List<GameObject> GetMultipleFromPool(string poolKey, int count, Vector3 position, Quaternion rotation);

        /// <summary>
        /// 批量从池子获取对象并返回指定组件（泛型重载）。
        /// </summary>
        /// <typeparam name="T">要获取的组件类型。</typeparam>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="count">要获取的对象数量。</param>
        /// <returns>组件列表（仅包含成功获取到组件的对象）。</returns>
        List<T> GetMultipleFromPool<T>(string poolKey, int count) where T : Component;
        #endregion

        #region 回收对象
        /// <summary>
        /// 将对象回收至指定池子。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="gameObject">要回收的对象。</param>
        void ReturnToPool(string poolKey, GameObject gameObject);

        /// <summary>
        /// 自动识别池子并回收对象（依赖PoolableObject组件）。
        /// </summary>
        /// <param name="gameObject">要回收的对象。</param>
        void ReturnToPool(GameObject gameObject);

        /// <summary>
        /// 批量回收对象到指定池子。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="gameObjects">要回收的对象列表。</param>
        void ReturnMultipleToPool(string poolKey, List<GameObject> gameObjects);

        /// <summary>
        /// 批量自动识别池子回收对象（依赖每个对象的 PoolableObject 组件）。
        /// </summary>
        /// <param name="gameObjects">要回收的对象列表。</param>
        void ReturnMultipleToPool(List<GameObject> gameObjects);

        /// <summary>
        /// 批量回收对象到指定池子（支持统一延迟）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="gameObjects">要回收的对象列表。</param>
        /// <param name="delay">延迟时间（秒）。</param>
        void ReturnMultipleToPool(string poolKey, List<GameObject> gameObjects, float delay = 0f);

        /// <summary>
        /// 批量自动识别池子回收对象（支持统一延迟）。
        /// </summary>
        /// <param name="gameObjects">要回收的对象列表。</param>
        /// <param name="delay">延迟时间（秒）。</param>
        void ReturnMultipleToPool(List<GameObject> gameObjects, float delay = 0f);
        #endregion
    }

    /// <summary>
    /// 对象池服务实现类，继承自MonoBehaviour并实现IObjectPoolService接口。
    /// 负责管理多个对象池，提供对象的创建、获取、回收功能。
    /// </summary>
    public class ObjectPoolServiceimpl : MonoBehaviour, IObjectPoolService
    {
        // 存储所有对象池的字典：Key = PoolKey, Value = 对象池实例
        private Dictionary<string, IObjectPool<GameObject>> _pools = new Dictionary<string, IObjectPool<GameObject>>();
        // 存储预制体引用的字典：Key = PoolKey, Value = 预制体
        private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        // 存储每个池的父节点：Key = PoolKey, Value = 父节点Transform
        private Dictionary<string, Transform> _poolRoots = new Dictionary<string, Transform>();
        // 所有池根节点的总父节点（即此服务脚本所在的GameObject的Transform）
        private Transform _serviceRoot;

        #region 生命周期
        /// <summary>
        /// Awake中初始化根节点，并将服务注册到全局服务定位器。
        /// </summary>
        private void Awake()
        {
            _serviceRoot = this.transform;

            if (ServiceLocatorProvider.Global.Locator != null)
            {
                ServiceLocatorProvider.Global.Locator.Register<IObjectPoolService>(this);
                ServiceLocatorProvider.Global.Locator.Register<ObjectPoolServiceimpl>(this);
            }
        }

        /// <summary>
        /// 销毁时释放所有对象池。
        /// </summary>
        private void OnDestroy()
        {
            DisposeAllPools();
        }
        #endregion

        #region 创建与销毁
        /// <summary>
        /// 创建对象池。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="prefab">要池化的预制体。</param>
        /// <param name="defaultCapacity">默认容量。</param>
        /// <param name="maxSize">最大容量（超出则销毁对象）。</param>
        public void CreatePool(string poolKey, GameObject prefab, int defaultCapacity = 10, int maxSize = 100)
        {
            if (_pools.ContainsKey(poolKey))
            {
                Debug.LogWarning($"名为 '{poolKey}' 的池子已经创建.");
                return;
            }

            _prefabs[poolKey] = prefab;

            // 创建该池的根节点，用于存放池内对象，便于层级管理
            GameObject poolRootObj = new GameObject($"{poolKey}_PoolRoot");
            poolRootObj.transform.SetParent(_serviceRoot);
            _poolRoots[poolKey] = poolRootObj.transform;

            // 使用Unity的ObjectPool<T0>创建池实例
            var pool = new ObjectPool<GameObject>(
                createFunc: () => CreateNewObject(poolKey, prefab, _poolRoots[poolKey]), // 创建新对象
                actionOnGet: OnGetObject,                                               // 获取时回调
                actionOnRelease: OnReleaseObject,                                       // 回收时回调
                actionOnDestroy: OnDestroyObject,                                       // 销毁时回调
                collectionCheck: true,                                                  // 是否检查重复回收
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
                );

            _pools.Add(poolKey, pool);
        }

        /// <summary>
        /// 销毁指定的对象池。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        public void DisposePool(string poolKey)
        {
            if (_pools.TryGetValue(poolKey, out var pool))
            {
                pool.Clear(); // 清空池，内部会调用actionOnDestroy销毁所有对象
                _pools.Remove(poolKey);
                _prefabs.Remove(poolKey);

                if (_poolRoots.TryGetValue(poolKey, out var root))
                {
                    if (root != null) Destroy(root.gameObject);
                    _poolRoots.Remove(poolKey);
                }
            }
        }

        /// <summary>
        /// 销毁所有对象池。
        /// </summary>
        public void DisposeAllPools()
        {
            foreach (var key in new List<string>(_poolRoots.Keys))
            {
                DisposePool(key);
            }

            _pools.Clear();
            _poolRoots.Clear();
            _prefabs.Clear();
        }
        #endregion

        #region 获取对象（单对象）
        /// <summary>
        /// 从池子获取对象（默认位置/旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <returns>池化的游戏对象，如果池子不存在则返回null。</returns>
        public GameObject GetFromPool(string poolKey)
        {
            if (_pools.TryGetValue(poolKey, out var pool))
            {
                return pool.Get();
            }
            Debug.LogError($"[ObjectPool] Pool '{poolKey}' 不存在!");
            return null;
        }

        /// <summary>
        /// 从池子获取对象（指定位置和旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="position">对象位置。</param>
        /// <param name="rotation">对象旋转。</param>
        /// <returns>池化的游戏对象，如果池子不存在则返回null。</returns>
        public GameObject GetFromPool(string poolKey, Vector3 position, Quaternion rotation)
        {
            GameObject obj = GetFromPool(poolKey);
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return obj;
        }

        /// <summary>
        /// 从池子获取对象（指定父节点）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="parent">父节点Transform。</param>
        /// <param name="worldPositionStays">是否保持世界坐标。</param>
        /// <returns>池化的游戏对象，如果池子不存在则返回null。</returns>
        public GameObject GetFromPool(string poolKey, Transform parent, bool worldPositionStays = false)
        {
            GameObject obj = GetFromPool(poolKey);
            if (obj != null)
            {
                obj.transform.SetParent(parent, worldPositionStays);
            }
            return obj;
        }

        /// <summary>
        /// 从池子获取对象并返回指定组件。
        /// </summary>
        /// <typeparam name="T">要获取的组件类型。</typeparam>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <returns>对象上的指定组件，如果对象不存在或没有该组件则返回null。</returns>
        public T GetFromPool<T>(string poolKey) where T : Component
        {
            GameObject obj = GetFromPool(poolKey);
            if (obj != null)
            {
                if (obj.TryGetComponent<T>(out var component))
                {
                    return component;
                }
                else
                {
                    Debug.LogError($"[ObjectPool] 从 '{poolKey}' 池中获取的对象没有组件 {typeof(T).Name}!");
                }
            }
            return null;
        }
        #endregion

        #region 获取对象（批量）
        /// <summary>
        /// 批量获取对象（默认位置/旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="count">要获取的对象数量。</param>
        /// <returns>获取到的对象列表（数量可能少于count，例如池子不存在时返回空列表）。</returns>
        public List<GameObject> GetMultipleFromPool(string poolKey, int count)
        {
            List<GameObject> result = new List<GameObject>();
            // 边界检查：数量小于等于0时直接返回空列表
            if (count <= 0 || !_pools.ContainsKey(poolKey))
            {
                if (count > 0) // 仅当数量有效但池子不存在时打印错误
                {
                    Debug.LogError($"[ObjectPool] Pool '{poolKey}' 不存在! 无法批量获取对象。");
                }
                return result;
            }

            // 循环获取指定数量的对象
            for (int i = 0; i < count; i++)
            {
                GameObject obj = GetFromPool(poolKey);
                if (obj != null)
                {
                    result.Add(obj);
                }
                else
                {
                    // 单个对象获取失败时停止循环（避免无效尝试）
                    Debug.LogWarning($"[ObjectPool] 从 '{poolKey}' 池批量获取对象时，第{i + 1}个对象获取失败，停止批量获取。");
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// 批量获取对象（指定统一位置/旋转）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="count">要获取的对象数量。</param>
        /// <param name="position">所有对象的统一位置。</param>
        /// <param name="rotation">所有对象的统一旋转。</param>
        /// <returns>获取到的对象列表。</returns>
        public List<GameObject> GetMultipleFromPool(string poolKey, int count, Vector3 position, Quaternion rotation)
        {
            List<GameObject> result = GetMultipleFromPool(poolKey, count);
            // 给每个对象设置统一的位置和旋转
            foreach (var obj in result)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return result;
        }

        /// <summary>
        /// 批量获取对象并返回指定组件。
        /// </summary>
        /// <typeparam name="T">要获取的组件类型。</typeparam>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="count">要获取的对象数量。</param>
        /// <returns>组件列表（仅包含成功获取到组件的对象）。</returns>
        public List<T> GetMultipleFromPool<T>(string poolKey, int count) where T : Component
        {
            List<T> result = new List<T>();
            List<GameObject> objs = GetMultipleFromPool(poolKey, count);
            // 提取每个对象的指定组件
            foreach (var obj in objs)
            {
                if (obj.TryGetComponent<T>(out var component))
                {
                    result.Add(component);
                }
                else
                {
                    Debug.LogWarning($"[ObjectPool] 从 '{poolKey}' 池获取的对象 {obj.name} 没有组件 {typeof(T).Name}，跳过。");
                }
            }
            return result;
        }
        #endregion

        #region 回收对象（单对象）
        /// <summary>
        /// 将对象回收至指定池子。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="obj">要回收的对象。</param>
        public void ReturnToPool(string poolKey, GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("尝试回收一个空对象!");
                return;
            }

            if (_pools.TryGetValue(poolKey, out var pool))
            {
                if (_poolRoots.TryGetValue(poolKey, out var root))
                {
                    obj.transform.SetParent(root);
                }

                pool.Release(obj);
            }
            else
            {
                Debug.LogError($"[ObjectPool] Pool '{poolKey}' 不存在! 无法回收对象.");
                Destroy(obj); // 池不存在，直接销毁对象
            }
        }

        /// <summary>
        /// 自动识别池子并回收对象（依赖PoolableObject组件）。
        /// </summary>
        /// <param name="obj">要回收的对象。</param>
        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            // 尝试获取对象身上的 PoolableObject 组件来识别它是哪个池子的
            if (obj.TryGetComponent<PoolableObject>(out var poolable))
            {
                ReturnToPool(poolable.PoolKey, obj);
            }
            else
            {
                Debug.LogError($"[ObjectPool] Object '{obj.name}' has no PoolableObject component. Cannot auto-return. Destroying.");
                Destroy(obj);
            }
        }
        #endregion

        #region 回收对象（批量）
        /// <summary>
        /// 批量回收对象到指定池子（核心实现）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="gameObjects">要回收的对象列表。</param>
        public void ReturnMultipleToPool(string poolKey, List<GameObject> gameObjects)
        {
            // 边界防护：空列表/空Key直接返回
            if (string.IsNullOrEmpty(poolKey) || gameObjects == null || gameObjects.Count == 0)
            {
                Debug.LogWarning($"[ObjectPool] 批量回收失败：池子Key为空或对象列表为空！");
                return;
            }

            // 校验池子是否存在
            if (!_pools.ContainsKey(poolKey))
            {
                Debug.LogError($"[ObjectPool] Pool '{poolKey}' 不存在！批量销毁对象替代回收。");
                // 兜底：销毁所有对象
                foreach (var obj in gameObjects)
                {
                    if (obj != null) Destroy(obj);
                }
                return;
            }

            // 循环回收每个对象（复用单对象回收逻辑，保证一致性）
            foreach (var obj in gameObjects)
            {
                // 复用已有单对象回收逻辑，避免重复写边界检查
                ReturnToPool(poolKey, obj);
            }
        }

        /// <summary>
        /// 批量自动识别池子回收对象（无需指定Key，依赖 PoolableObject 组件）。
        /// </summary>
        /// <param name="gameObjects">要回收的对象列表。</param>
        public void ReturnMultipleToPool(List<GameObject> gameObjects)
        {
            if (gameObjects == null || gameObjects.Count == 0)
            {
                Debug.LogWarning($"[ObjectPool] 批量回收失败：对象列表为空！");
                return;
            }

            // 按池子Key分组回收（优化性能：避免重复查找同一个池子）
            Dictionary<string, List<GameObject>> groupByPoolKey = new Dictionary<string, List<GameObject>>();

            // 第一步：分组（空对象/无组件的对象直接销毁）
            foreach (var obj in gameObjects)
            {
                if (obj == null) continue;

                if (obj.TryGetComponent<PoolableObject>(out var poolable))
                {
                    string key = poolable.PoolKey;
                    if (!groupByPoolKey.ContainsKey(key))
                    {
                        groupByPoolKey[key] = new List<GameObject>();
                    }
                    groupByPoolKey[key].Add(obj);
                }
                else
                {
                    Debug.LogWarning($"[ObjectPool] 对象 {obj.name} 无 PoolableObject 组件，直接销毁！");
                    Destroy(obj);
                }
            }

            // 第二步：按分组批量回收（复用上面的批量回收方法）
            foreach (var group in groupByPoolKey)
            {
                ReturnMultipleToPool(group.Key, group.Value);
            }
        }

        /// <summary>
        /// 批量回收对象到指定池子（支持统一延迟）。
        /// </summary>
        /// <param name="poolKey">池子唯一标识符。</param>
        /// <param name="gameObjects">待回收对象列表。</param>
        /// <param name="delay">统一延迟时间（秒）。</param>
        public void ReturnMultipleToPool(string poolKey, List<GameObject> gameObjects, float delay = 0f)
        {
            // 边界防护
            if (string.IsNullOrEmpty(poolKey) || gameObjects == null || gameObjects.Count == 0)
            {
                Debug.LogWarning("[ObjectPool] 批量回收失败：参数无效！");
                return;
            }

            foreach (var obj in gameObjects)
            {
                if (obj == null) continue;

                // 核心：复用 PoolableObject 的延迟回收（自带协程管控）
                if (obj.TryGetComponent<PoolableObject>(out var poolable))
                {
                    // 校验池子Key匹配，避免回收错池子
                    if (poolable.PoolKey == poolKey)
                    {
                        poolable.ReturnToPool(delay); // 调用对象自身的延迟回收
                    }
                    else
                    {
                        Debug.LogWarning($"[ObjectPool] 对象 {obj.name} 不属于 {poolKey} 池，跳过回收！");
                    }
                }
                else
                {
                    // 兜底：无 PoolableObject 组件，立即回收/销毁
                    ReturnToPool(poolKey, obj);
                }
            }
        }

        /// <summary>
        /// 批量回收对象（自动识别池子，支持统一延迟）。
        /// </summary>
        /// <param name="gameObjects">要回收的对象列表。</param>
        /// <param name="delay">延迟时间（秒）。</param>
        public void ReturnMultipleToPool(List<GameObject> gameObjects, float delay = 0f)
        {
            if (gameObjects == null || gameObjects.Count == 0) return;

            foreach (var obj in gameObjects)
            {
                if (obj == null) continue;

                if (obj.TryGetComponent<PoolableObject>(out var poolable))
                {
                    poolable.ReturnToPool(delay); // 直接调用对象自身的延迟回收
                }
                else
                {
                    Debug.LogWarning($"[ObjectPool] 对象 {obj.name} 无 PoolableObject 组件，直接销毁！");
                    Destroy(obj);
                }
            }
        }
        #endregion

        #region 内部 ObjectPool 回调
        /// <summary>
        /// 创建新对象，由对象池内部调用。
        /// </summary>
        /// <param name="poolKey">池子标识。</param>
        /// <param name="prefab">预制体。</param>
        /// <param name="parent">父节点。</param>
        /// <returns>新实例化的GameObject。</returns>
        private GameObject CreateNewObject(string poolKey, GameObject prefab, Transform parent)
        {
            GameObject obj = Instantiate(prefab, parent);
            obj.name = prefab.name;

            // 确保对象上有PoolableObject组件，用于记录所属池和提供回收方法
            var poolable = obj.GetComponent<PoolableObject>();
            if (poolable == null)
            {
                poolable = obj.AddComponent<PoolableObject>();
            }
            poolable.PoolKey = poolKey;

            return obj;
        }

        /// <summary>
        /// 从池中获取对象时的回调，激活对象并触发其OnGetFromPool方法。
        /// </summary>
        /// <param name="obj">获取的对象。</param>
        private void OnGetObject(GameObject obj)
        {
            obj.SetActive(true);
            obj.GetComponent<PoolableObject>()?.OnGetFromPool();
        }

        /// <summary>
        /// 回收对象时的回调，停用对象并触发其OnReturnToPool方法。
        /// </summary>
        /// <param name="obj">回收的对象。</param>
        private void OnReleaseObject(GameObject obj)
        {
            obj.GetComponent<PoolableObject>()?.OnReturnToPool();
            obj.SetActive(false);
        }

        /// <summary>
        /// 对象被池销毁时的回调，触发其OnDestroyInPool方法并销毁对象。
        /// </summary>
        /// <param name="obj">要销毁的对象。</param>
        private void OnDestroyObject(GameObject obj)
        {
            obj.GetComponent<PoolableObject>()?.OnDestroyInPool();
            Destroy(obj);
        }
        #endregion
    }
}