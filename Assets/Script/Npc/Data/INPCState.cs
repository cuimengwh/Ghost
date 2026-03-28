using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Utopia.Npc;
using Utopia.NPC;

namespace Utopia.Npc
{
    /// <summary>
    /// NPC状态接口，定义了所有居民状态（如幽灵、恐慌、低迷、孤独、哀惧、正常等）必须实现的方法。
    /// 符合设计文档中居民状态系统的要求，支持状态切换和每帧更新。
    /// </summary>
    public interface INPCState
    {
        /// <summary>
        /// 进入状态时调用，用于初始化状态所需的数据和组件引用。
        /// </summary>
        /// <param name="npc">持有该状态的NPC控制器</param>
        void Enter(NPCControl npc);

        /// <summary>
        /// 退出状态时调用，用于清理状态资源或还原NPC属性。
        /// </summary>
        void Exit();

        /// <summary>
        /// 处理玩家与该NPC交互时的逻辑，根据当前状态可能产生不同对话、选项或效果。
        /// </summary>
        void Interacting();

        /// <summary>
        /// 每帧更新状态，处理状态特有的行为逻辑，如时间判定、数值变化、行为切换等。
        /// </summary>
        /// <param name="time">当前游戏时间（可能为归一化值0~1，或具体小时），用于判断昼夜、日程等</param>
        void Update(float time);
    }
}