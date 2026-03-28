using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace Utopia.Npc
{
    /// <summary>
    /// NPC移动控制组件
    /// 基于Unity NavMeshAgent实现NPC的导航移动。
    /// 负责设置目的地、速度、可见性等，供各个状态（如GhostState、LowMoodState）调用。
    /// 可见性控制用于实现幽灵昼夜切换（设计文档：灵魂夜晚活动）。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCMovement : MonoBehaviour
    {
        private NavMeshAgent _agent;  // Unity导航代理组件
        private bool _isVisible = true; // 当前可见性状态，用于避免重复设置

        private void Awake()
        {
            // 获取NavMeshAgent组件
            _agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// 设置NPC的移动目标位置。
        /// 仅当NavMeshAgent存在且启用时才生效。
        /// </summary>
        /// <param name="position">目标世界坐标</param>
        public void SetDestination(Vector3 position)
        {
            if (_agent != null && _agent.enabled)
            {
                _agent.SetDestination(position);
            }
        }

        /// <summary>
        /// 设置NPC的移动速度。
        /// 用于状态切换时调整速度，例如低迷状态下的迟缓移动。
        /// </summary>
        /// <param name="speed">速度值（单位：米/秒）</param>
        public void SetSpeed(float speed)
        {
            if (_agent != null)
            {
                _agent.speed = speed;
            }
        }

        /// <summary>
        /// 设置NPC的可见性。
        /// 用于实现幽灵昼夜切换（设计文档：灵魂夜晚出现，白天消失）。
        /// 当visible为false时，通常会禁用渲染器和碰撞器，此处通过启用/禁用NavMeshAgent来避免寻路占用。
        /// </summary>
        /// <param name="visible">是否可见</param>
        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return; // 无变化则跳过
            _isVisible = visible;

            // 控制NPC可视状态
            // 这里可以根据需要启用/禁用渲染组件、碰撞组件等
            // 目前仅控制NavMeshAgent的启用状态，防止不可见时仍参与寻路计算

            if (_agent != null)
            {
                _agent.enabled = visible; // 仅在可见时启用NavMeshAgent
            }
            // TODO: 启用/禁用Renderer、Collider等
        }

        /// <summary>
        /// 检查NPC是否已到达当前目的地。
        /// 用于行为决策，如到达后执行停留或切换行为。
        /// </summary>
        /// <returns>是否已到达（考虑停止距离）</returns>
        public bool HasReachedDestination()
        {
            if (_agent == null || !_agent.enabled) return false;
            return !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
        }

        /// <summary>
        /// 获取NPC当前的移动速度（实际速度矢量的大小）。
        /// 可用于动画参数或其他状态判定。
        /// </summary>
        /// <returns>当前速度大小（米/秒）</returns>
        public float GetCurrentSpeed()
        {
            return _agent != null ? _agent.velocity.magnitude : 0f;
        }
    }
}