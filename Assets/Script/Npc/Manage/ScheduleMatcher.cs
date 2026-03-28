using System.Collections.Generic;
using System.Linq;
using Utopia.Npc;

namespace Utopia.NPC
{
    /// <summary>
    /// 日程匹配器
    /// 根据当前时间（归一化值 0~1）从NPC的日程列表中匹配当前应执行的行为项。
    /// 用于指导NPC在不同时间点执行不同行为，例如幽灵夜晚移动到特定地点、白天休息等。
    /// 对应设计文档中居民AI的日程系统，配合状态机使用（如GhostState中调用）。
    /// </summary>
    public class ScheduleMatcher
    {
        private List<NPCScheduleItem> _schedule; // NPC的日程项列表，每个项包含时间点和行为目标

        /// <summary>
        /// 构造函数，接收NPC的日程配置。
        /// </summary>
        /// <param name="schedule">日程项列表，通常从NPCProfile中加载</param>
        public ScheduleMatcher(List<NPCScheduleItem> schedule)
        {
            _schedule = schedule;
        }

        /// <summary>
        /// 根据当前时间获取对应的日程项。
        /// 算法：选择所有时间点小于等于当前时间的日程项中，时间点最大的那一个。
        /// 如果没有符合条件的（即所有日程项时间都大于当前时间），则返回时间点最大的日程项（即最后一个）。
        /// 这种算法适用于循环日程表，假设日程按时间顺序排列，且一天内行为按时间点顺序切换。
        /// </summary>
        /// <param name="time">当前归一化时间（0~1），例如0.5表示中午12点</param>
        /// <returns>匹配的日程项，若无日程则返回null</returns>
        public NPCScheduleItem GetCurrentItem(float time)
        {
            if (_schedule == null || _schedule.Count == 0)
                return null;

            NPCScheduleItem matchItem = null;

            // 遍历所有日程项，寻找时间点 <= 当前时间且时间点最大的项
            foreach (var item in _schedule)
            {
                if (time >= item.Time01)
                {
                    if (matchItem == null || item.Time01 > matchItem.Time01)
                    {
                        matchItem = item;
                    }
                }
            }

            // 如果没找到（例如当前时间早于所有日程时间），则取时间点最大的项作为默认
            if (matchItem == null)
            {
                matchItem = _schedule.OrderByDescending(item => item.Time01).First();
            }

            return matchItem;
        }
    }
}