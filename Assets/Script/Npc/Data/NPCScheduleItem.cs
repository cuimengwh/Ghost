using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Utopia.Npc
{
    [Serializable]
    public class NPCScheduleItem
    {
        [Header("时间")]
        [Range(0, 23)] public int hour;
        [Range(0, 59)] public int minute;

        [Header("行为与目标")]

        [Tooltip("NPC 将前往的地点 ID，可以是一个空物体或其他 NPC 的位置")]
        public string locationID;

        [Tooltip("NPC 将前往的目标位置，可以是一个空物体或其他 NPC 的位置")]
        public Transform targetTransform;

        // 辅助：计算 0.0 - 1.0 的时间值，用于和 TimeManager 比较
        public float Time01 => (hour * 60f + minute) / 1440f;
    }
}