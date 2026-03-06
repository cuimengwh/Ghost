using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utopia.Npc
{
    [CreateAssetMenu(menuName = "Utpia/NPC/Profile")]
    public class NPCProfile : ScriptableObject
    {
        [Header("基础信息")]
        public string npcId;
        public string npcShowedName;
        [TextArea] public string description;

        [Header("外观与资源")]
        public Sprite npcIcon;
        public GameObject prefab;

        [Header("性格与设定")]
        public bool isGhostInitialized;

        //[Header("基础设定")]

        //[Header("性格设定")]

        [Header("日程与行为")]
        public List<NPCScheduleItem> scheduleItems;
        public float moveSpeed = 3.5f;
    }
}
