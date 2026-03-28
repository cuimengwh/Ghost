using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Utopia.Npc;
using Utopia.NPC;

namespace Utopia.Npc
{
    /// <summary>
    /// 幽灵状态：对应设计文档中居民的“幽灵”状态。
    /// 幽灵仅在夜晚出现，并随时间消耗灵魂能量（soulEnergy），能量耗尽则可能消散或转为其他负面状态。
    /// </summary>
    public class GhostState : INPCState
    {
        private NPCControl _npc;                 // 所属NPC控制器
        private ScheduleMatcher _scheduleMatcher; // 日程匹配器，用于根据时间获取NPC应执行的行为项
        private float _decrease;                  // 灵魂能量衰减速率（单位：每秒），数值待配置

        /// <summary>
        /// 进入幽灵状态时调用，初始化NPC引用和日程匹配器。
        /// </summary>
        public void Enter(NPCControl npc)
        {
            _npc = npc;
            // 从NPC配置文件中加载日程项，构建匹配器
            _scheduleMatcher = new ScheduleMatcher(_npc.profile.scheduleItems);
            // 可在此处设置幽灵移动速度等，但代码中注释掉了：// _npc.SetSpeed()
        }

        /// <summary>
        /// 每帧更新幽灵状态。
        /// 根据现实时间判断是否为夜晚，控制幽灵的可见性；消耗灵魂能量；根据日程执行行为。
        /// </summary>
        public void Update(float realTime)
        {
            // 设计文档中定义了灵魂活动时间：晚上6点至次日早上9点。
            // 这里通过realTime（可能是归一化的时间0~1）进行转换和判断。
            // 转换 ghostTime 用于日程匹配（可能灵魂有自己的时间感知偏移）。
            float ghostTime = (realTime + 0.5f) % 1.0f;

            // 判断是否为夜晚：realTime >= 0.75（相当于18点） 或 realTime < 0.375（相当于9点前）
            // 这是因为realTime归一化后，0=0点，0.25=6点，0.5=12点，0.75=18点。
            // 夜晚时段覆盖 0.75~1 和 0~0.375，即18:00~次日9:00，与设计文档一致。
            bool isNight = realTime >= 0.75f || realTime < 0.375f;

            // 设置幽灵的可见性：夜晚可见（灵魂出现），白天不可见（消失）
            _npc.Movement.SetVisible(isNight);

            if (isNight)
            {
                // 夜晚时，根据日程匹配器获取当前应执行的行为项（如移动到某处、执行动作）
                var item = _scheduleMatcher.GetCurrentItem(ghostTime);
                if (item != null)
                {
                    // 若日程项包含位置限制，可在此处理移动目标
                    // 例如：_npc.Movement.SetDestination(item.position);
                }
            }

            // 持续消耗灵魂能量（设计文档中灵魂需要灵魂作物维持，否则会陷入低迷）
            _npc.runtimeData.soulEnergy -= _decrease * Time.deltaTime;

            // 如果灵魂能量耗尽，应切换状态（例如消散或变为低迷/恐慌状态）
            if (_npc.runtimeData.soulEnergy <= 0)
            {
                // 切换状态逻辑待实现
            }
        }

        /// <summary>
        /// 退出幽灵状态时调用，可进行清理工作。
        /// </summary>
        public void Exit()
        {
            // 暂无清理逻辑
        }

        /// <summary>
        /// 处理与幽灵的交互（如玩家与其对话、送礼等）。
        /// </summary>
        public void Interacting()
        {
            // 交互逻辑待实现
        }
    }
}