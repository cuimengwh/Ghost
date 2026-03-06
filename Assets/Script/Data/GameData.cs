using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utopia.Data
{
    public class GameData
    {
        // 元数据 
        public string saveSlotId; // 槽位ID
        public string saveName;   // 存档名称
        public DateTime lastSaveTime;  // 显示世界时间
        public long totalPlayTimeSeconds; // 总游玩时长

        // 全局游戏状态
        public string currentSceneName; // 当前场景
        public Vector3 position; // 玩家坐标

        public GameData() { }
    }
}