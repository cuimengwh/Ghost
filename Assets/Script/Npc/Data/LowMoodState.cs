using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utopia.Npc;

namespace Utopia.NPC
{
    /// <summary>
    /// 低迷状态（Low Mood State）
    /// 对应设计文档中居民的“低迷”状态。
    /// 触发条件：长期未获得灵魂作物，导致灵魂能量低下。
    /// 表现：交互异常（吞钱、乱码、无法增加好感），移动卡顿（迟缓-正常交替）。
    /// 解除：提供灵魂作物恢复灵魂能量后自动切换回正常状态或其他状态。
    /// </summary>
    public class LowMoodState : INPCState
    {
        private NPCControl _npc;               // 所属NPC控制器
        private float _moveSpeed;               // 正常状态下的移动速度（从配置文件中读取）
        private float _stuckMoveSpeed;           // 迟缓状态下的移动速度（按比例降低）
        private float _minStuckDuration = 0.2f;  // 每次迟缓的最小持续时间（秒）
        private float _maxStuckDuration = 0.5f;  // 每次迟缓的最大持续时间（秒）
        private float _minInterval = 5f;         // 两次迟缓之间的最小间隔（秒）
        private float _maxInterval = 10f;        // 两次迟缓之间的最大间隔（秒）

        private Coroutine _stuckCoroutine;       // 控制迟缓-正常交替的协程
        private bool _isStuck = false;            // 当前是否处于迟缓状态（用于可能的动画或判定）

        private float _stuckSpeedRatio = 0.3f;    // 迟缓时速度占正常速度的比例（即降低到30%）

        /// <summary>
        /// 进入低迷状态时调用。
        /// 初始化速度、启动迟缓协程，并将NPC初始速度设为0以模拟“发呆”效果。
        /// </summary>
        /// <param name="npc">进入该状态的NPC控制器</param>
        public void Enter(NPCControl npc)
        {
            if (npc == null)
            {
                Debug.LogError("LowMoodState: NPC reference is null.");
                return;
            }

            _npc = npc;

            // 设计文档描述低迷时“交互异常”，这里将移动速度暂时设为0，体现行动迟滞
            if (_npc?.Movement != null)
            {
                _npc.Movement.SetSpeed(0); // 进入低迷状态时，先将移动速度设为0
            }

            // 从NPC配置文件中获取正常移动速度，并计算迟缓速度
            _moveSpeed = _npc.profile.moveSpeed;
            _stuckMoveSpeed = _moveSpeed * _stuckSpeedRatio; // 迟缓状态的移动速度为正常速度的一定比例

            // 启动协程，让NPC在正常移动和迟缓移动之间随机交替
            if (_stuckCoroutine == null && _npc != null)
            {
                _stuckCoroutine = _npc.StartCoroutine(StuckMovementCoroutine());
            }
        }

        /// <summary>
        /// 退出低迷状态时调用。
        /// 停止迟缓协程，恢复NPC的正常移动速度。
        /// </summary>
        public void Exit()
        {
            if (_stuckCoroutine != null && _npc != null)
            {
                _npc.StopCoroutine(_stuckCoroutine);
                _stuckCoroutine = null;
            }
            _isStuck = false;

            // 恢复NPC的正常速度
            if (_npc.Movement != null)
            {
                _npc.Movement.SetSpeed(_moveSpeed);
            }
        }

        /// <summary>
        /// 处理玩家与低迷状态NPC的交互。
        /// 根据设计文档，低迷状态交互会产生异常效果：
        /// - 吞钱（扣除金钱但不增加好感）
        /// - 对话随机显示乱码
        /// - 任务不可交互
        /// - 送礼不加好感度
        /// 具体逻辑需在此方法中实现。
        /// </summary>
        public void Interacting()
        {
            // 低迷状态交互设定:吞钱，随机的讲话乱码，任务不可交互，随礼不加好感度，等等
            // TODO: 实现具体交互异常逻辑
        }

        /// <summary>
        /// 每帧更新低迷状态。
        /// 主要检查灵魂能量是否恢复到阈值以上，若恢复则通知NPC重新评估状态（可能切换回正常状态）。
        /// </summary>
        /// <param name="time">当前游戏时间（归一化或小时）</param>
        public void Update(float time)
        {
            // 依据具体灵魂能力设定，进行状态的转换
            // 设计文档：提供灵魂作物可解除低迷。这里用灵魂能量>1作为恢复判断条件
            if (_npc.runtimeData.soulEnergy > 1f)
            {
                // 通知NPC重新评估状态（会调用内部逻辑，可能切换到正常状态或其他）
                _npc.DetermineState();
            }
        }

        /// <summary>
        /// 协程：模拟低迷状态下的“迟缓-正常”交替移动。
        /// 在随机间隔后进入迟缓状态（速度降低），持续短暂时间后恢复，循环进行。
        /// 这种行为体现了低迷状态NPC行动呆滞、卡顿的特点。
        /// </summary>
        private IEnumerator StuckMovementCoroutine()
        {
            if (_npc == null || _npc.Movement == null)
            {
                Debug.LogWarning("LowMoodState: NPC or NPCMovement reference is null. StuckMovementCoroutine will not run.");
                yield break;
            }

            while (true)
            {
                // 随机等待一段时间后进入迟缓状态（模拟正常移动时段）
                float waitInterval = Mathf.Clamp(Random.Range(_minInterval, _maxInterval), 1f, 20f);
                yield return new WaitForSeconds(waitInterval);

                // 进入迟缓状态：速度变慢
                _isStuck = true;
                _npc?.Movement?.SetSpeed(_stuckMoveSpeed);

                // 随机等待一段时间后恢复正常状态
                yield return new WaitForSeconds(Random.Range(_minStuckDuration, _maxStuckDuration));

                // 恢复正常移动速度
                _isStuck = false;
                _npc?.Movement?.SetSpeed(_moveSpeed);
            }

        }
    }
}