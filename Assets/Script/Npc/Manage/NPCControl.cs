using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utopia.TimeSystem;

namespace Utopia.Npc
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCControl : MonoBehaviour
    {
        private NPCProfile _profile;

        private NPCRuntimeData _runtimeData;
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;
        private NPCScheduleItem _scheduleItem;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();

            // 读档
            _runtimeData = new NPCRuntimeData()
            {
                npcID = _profile.npcId,
                isGhost = _profile.isGhostInitialized
            };

            _navMeshAgent.speed = _profile.moveSpeed;
        }

        private void Start()
        {
            
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {

        }

        private void HandlerTimeUpdate(CustomDateTime customDate)
        {

        }

        private void CheckSchedule(float time)
        {

        }
    }
}
