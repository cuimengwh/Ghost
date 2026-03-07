using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Utopia.Npc
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCMovement : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private bool _isVisible = true;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void SetDestination(Vector3 position)
        {
            if (_agent != null && _agent.enabled)
            {
                _agent.SetDestination(position);
            }
        }

        public void SetSpeed(float speed)
        {
            if (_agent != null)
            {
                _agent.speed = speed;
            }
        }

        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return;
            _isVisible = visible;

            // 控制NPC可视状态
            // 这里可以根据需要启用/禁用渲染组件、碰撞组件等

            if (_agent != null)
            {
                _agent.enabled = visible; // 仅在可见时启用NavMeshAgent
            }
        }
    }
}
