using System.Collections.Generic;
using UnityEngine;
using Utopia.Core.Services;

namespace Utopia.Npc
{
    public interface INPCManager
    {

    }

    public class NPCManager : MonoBehaviour, INPCManager
    {
        FindObjectsSortMode sortMode = FindObjectsSortMode.InstanceID;

        // 缓存所有的 NPC 位置，key 是 locationID，value 是对应的 Transform
        public Dictionary<string, Transform> _locaationMap = new Dictionary<string, Transform>();

        private void Awake()
        {
            if (ServiceLocatorProvider.Global.Locator != null)
            {
                ServiceLocatorProvider.Global.Locator.Register<INPCManager>(this);
            }
            else
            {
                Debug.LogError("TimeManager 初始化失败: ServiceLocatorProvider.Global.Locator 为空");
            }

            var maskers = FindObjectsByType<NPCLocationMasker>(sortMode);
            foreach (var masker in maskers)
            {
                if (!_locaationMap.ContainsKey(masker.locationID))
                {
                    _locaationMap.Add(masker.locationID, masker.transform);
                }
                else
                {
                    Debug.LogWarning($"发现重复的 locationID: {masker.locationID}，请确保每个 NPCLocationMasker 的 locationID 唯一");
                }
            }
        }

        public Transform GetLocation(string locationID)
        {
            if (_locaationMap.TryGetValue(locationID, out var transform))
                return transform;

            return null;
        }
    }
}