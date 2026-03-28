using System;

namespace Utopia.Npc
{
    [Serializable]
    public class NPCRuntimeData
    {
        public string npcID;              // NPC唯一标识符
        public bool isGhost = true;       // 初始全镇皆为灵魂
        public bool isResurrected = false;// 是否已复活
        public bool isDead = false;       // 彻底消散

        public float panicValue = 0f;     // 恐慌值 (满100消散)
        public float lonelinessValue = 0f;// 孤独值
        public float soulEnergy = 100f;   // 灵魂能量 (需要吃灵魂根果，低于0低迷)

        public int friendship = 1;        // 好感度 1-10
    }
}
