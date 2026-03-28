using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Utopia.Npc
{
    public class NPCStateMachine
    {
        private NPCControl _npc;
        private INPCState _currentState;

        public NPCStateMachine(NPCControl npc) { _npc = npc; }

        public void ChangeState<T>() where T : INPCState, new()
        {
            _currentState?.Exit();
            _currentState = new T();
            _currentState.Enter(_npc);
            // 根据NPC的当前状态、环境因素和时间等条件，决定NPC应该进入哪个状态
            // 例如：
            // if (npc.runtimeData.isSleeping) { npc.ChangeState(new SleepState()); }
            // else if (npc.runtimeData.isWorking) { npc.ChangeState(new WorkState()); }
            // else { npc.ChangeState(new IdleState()); }
        }
        public void Update(float dt) => _currentState?.Update(dt);
        public void Interaction() => _currentState?.Interacting();
        public INPCState GetCurrentState => _currentState;
    }
}
