using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.Inventory
{
    [RequireComponent(typeof(Slot_Bag))]
    public class ActionBarButton : MonoBehaviour
    {
        public KeyCode key;

        private Slot_Bag slotBag;

        private bool canUse;

        private void Awake()
        {
            slotBag = GetComponent<Slot_Bag>();
            canUse = true;
        }

        private void OnEnable()
        {
            EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        }

        private void OnDisable()
        {
            EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        }

        private void OnUpdateGameStateEvent(GameState gameState)
        {
            //在一些情况如交易下禁用快捷选中背包格子
            canUse = gameState == GameState.GamePlay;
            Debug.Log(gameState.ToString());
        }

        private void Update()
        {
            if(Input.GetKeyDown(key) && canUse)
            {
                slotBag.SlotSelected();
            }
        }
    }
}
