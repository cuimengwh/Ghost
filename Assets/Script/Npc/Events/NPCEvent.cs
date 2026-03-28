using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Utopia.Core.Event
{
    public class NPCEvent : GameEventBase
    {
        public string npcId { get; protected set; }
        protected NPCEvent(string _npcId) : base() 
        {
            npcId = _npcId;
        }
    }
}
