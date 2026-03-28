using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utopia.Core.Event;
using Utopia.Core.Services;
using Utopia.NPC;
using Utopia.TimeSystem;

namespace Utopia.Npc
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCControl : MonoBehaviour
    {
        [Header("基础配置")]
        public string npcId; // NPC的唯一标识符,方便后续进行总管理器的管理

        public NPCProfile profile; // NPC的配置数据，包含外观、日程、情绪等信息
        public NPCRuntimeData runtimeData; // NPC的运行时数据，包含当前状态、情绪值、目标位置等动态信息

        public NPCMovement Movement { get; private set; } // 负责NPC的移动控制，封装了NavMeshAgent的功能
        public MoodSystem Mood { get; private set; } // 负责NPC的情绪系统，管理情绪值的变化和影响因素
        private NPCStateMachine _stateMachine; // 负责NPC的状态机，管理不同状态（如Idle、Walking、Working等）之间的切换和行为执行

        [ServiceInject]
        private ITimeManager _timeManager;
        [ServiceInject]
        private IEventManager _eventManager;
        private void Awake()
        {
            Movement = this.GetComponent<NPCMovement>();

            // 注入依赖
            if (ServiceLocatorProvider.Global.Locator != null)
            {
                ServiceLocatorProvider.Global.Locator.Inject(this);
            }

            // 初始化数据，待数据读取功能完善后从配置文件中加载
            // profile
            // runtimeData
            // Mood
            // _stateMachine

            // 性格内容修正
            if (profile.optimismTrait == OptimismTrait.Optimist)
            {
                runtimeData.panicValue = Random.Range(0f, 10f); // 乐天派：初始0-10,乐观者的恐慌值较低
            }
            else if (profile.optimismTrait == OptimismTrait.Pessimist)
            {
                runtimeData.panicValue = Random.Range(10f, 20f); // 悲观派：初始10-20,悲观者的恐慌值较高
            }

            // 订阅时间刻度事件（每帧）
            if (_timeManager != null)
                _timeManager.OnTick += OnTimeTick;

            // 订阅全局恐慌最大值事件
            if (_eventManager != null)
                _eventManager.Subscribe<NPCPanicMaxEvent>(OnPanicMax);
        }

        private void Start()
        {
            DetermineState(); // 根据初始数据确定NPC的初始状态
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {
            if (_timeManager != null)
                _timeManager.OnTick -= OnTimeTick;
            _eventManager?.Unsubscribe<NPCPanicMaxEvent>(OnPanicMax);
        }

        public void DetermineState()
        {
            // 处理死亡
            if (runtimeData.isDead) _stateMachine.ChangeState<DeadState>();
            // 处理恐慌值过高导致的灵魂消散（彻底死亡）
            else if (runtimeData.soulEnergy <= 0 && runtimeData.isGhost) _stateMachine.ChangeState<LowMoodState>();
            // 处理进入灵魂状态
            else if (runtimeData.isGhost) _stateMachine.ChangeState<GhostState>();
            // 处理治愈后进入正常状态
            else if (runtimeData.isResurrected) _stateMachine.ChangeState<NormalState>();
        }

        public void OnTimeTick(CustomDateTime datetime)
        {
            Mood.UpDate(Time.deltaTime); // 更新情绪系统
            _stateMachine.Update(datetime.time); // 更新状态机，执行当前状态的行为
        }

        private void OnPanicMax(NPCPanicMaxEvent evt)
        {
            if (evt.npcId == profile.npcId) 
            {
                runtimeData.isDead = true;
                _stateMachine.ChangeState<DeadState>();
                // 发布死亡事件
            }
        }

        private void HandlerTimeUpdate(CustomDateTime customDate)
        {

        }

        private void CheckSchedule(float time)
        {

        }
    }
}
