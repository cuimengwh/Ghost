//using System.Collections;
//using System.Collections.Generic;
//using System.Dynamic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//namespace MFarm.Inventory
//{
//    public class InventoryUI : MonoBehaviour
//    {
//        public ItemToolTip itemToolTip;

//        [Header("拖拽图片")]
//        public Image dragItem;

//        [Header("玩家背包UI")]
//        [SerializeField] private GameObject bagUI;
//        private bool bagOpened;

//        [Header("通用背包")]
//        [SerializeField] private GameObject baseBag;
//        public GameObject shopSlotPrefab;
//        public GameObject boxSlotPrefab;
//        public Button ESCBaseBag;

//        [Header("交易UI")]
//        public TradeUI tradeUI;
//        public TextMeshProUGUI playerMoneyText;

//        [SerializeField] private Slot_Bag[] playerSlots;
//        [SerializeField] private List<Slot_Bag> baseBagSlots;

//        private void OnEnable()
//        {
//            EventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
//            EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
//            EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
//            EventHandler.BaseBagCloseEvent += OnBaseBagCloseEvent;
//            EventHandler.ShowTradeUI += OnShowTradeUI;
//        }

//        private void OnDisable()
//        {
//            EventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
//            EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
//            EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
//            EventHandler.BaseBagCloseEvent -= OnBaseBagCloseEvent;
//            EventHandler.ShowTradeUI -= OnShowTradeUI;
//        }

//        private void Start()
//        {
//            //给每个格子编一个序号
//            for (int i = 0; i < playerSlots.Length; i++)
//            {
//                playerSlots[i].slotIndex = i;
//            }

//            //检测背包是否打开
//            bagOpened = bagUI.activeInHierarchy; 

//            //更新钱的UI
//            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
//        }

//        private void Update()
//        {
//            if(Input.GetKeyDown(KeyCode.B))
//            {
//                OpenBagUI();
//            }
//        }

//        private void OnShowTradeUI(ItemDetails item, bool isSell)
//        {
//            tradeUI.gameObject.SetActive(true);
//            tradeUI.SetupTradeUI(item, isSell);

//        }

//        /// <summary>
//        /// 打开通用包裹UI事件
//        /// </summary>
//        /// <param name="slotType"></param>
//        /// <param name="bagData"></param>
//        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bagData)
//        {
//            //通用箱子prefab
//            GameObject prefab = slotType switch
//            {
//                SlotType.Shop => shopSlotPrefab,
//                SlotType.Box => boxSlotPrefab,
//                _ => null,
//            };

//            //生成背包UI
//            baseBag.SetActive(true);

//            baseBagSlots = new List<Slot_Bag>();

//            for (int i = 0;i < bagData.itemList.Count; i++)
//            {
//                var slot = Instantiate(prefab, baseBag.transform.GetChild(0)).GetComponent<Slot_Bag>();
//                slot.slotIndex = i;
//                baseBagSlots.Add(slot);
//            }
//            LayoutRebuilder.ForceRebuildLayoutImmediate(baseBag.GetComponent<RectTransform>());

//            if(slotType == SlotType.Shop)
//            {
//                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-0.5f, 0.5f);
//                bagUI.SetActive(true);
//                bagOpened = true;
//            }

//            //更新UI显示
//            OnUpdateInventoryUI(InventoryLocation.Box, bagData.itemList); 
//        }

//        /// <summary>
//        /// 关闭通用包裹UI事件
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="sO"></param>
//        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBag_SO sO)
//        {
//            baseBag.SetActive(false);
//            itemToolTip.gameObject.SetActive(false);
//            UpdateSlotHeight(-1);

//            foreach (var slot in baseBagSlots)
//            {
//                Destroy(slot.gameObject);
//            }
//            baseBagSlots.Clear();

//            if (slotType == SlotType.Shop)
//            {
//                bagUI.GetComponent<RectTransform>().pivot = new Vector2(-1, 0.5f);
//                bagUI.SetActive(false);
//                bagOpened = false;
//            }
//        }

//        private void OnBeforeSceneUnloadEvent()
//        {
//            UpdateSlotHeight(-1);
//        }

//        /// <summary>
//        /// 更新背包的UI
//        /// </summary>
//        /// <param name="location"></param>
//        /// <param name="list"></param>
//        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
//        {
//            switch (location)
//            {
//                case InventoryLocation.Player:
//                    for (int i = 0;i < playerSlots.Length;i++)
//                    {
//                        if (list[i].itemAmount > 0)
//                        {
//                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
//                            playerSlots[i].UpdateSlot(item, list[i].itemAmount);
//                        }
//                        else
//                        {
//                            playerSlots[i].UpdateEmptySlot();
//                        }
//                    }
//                    break;  
//                case InventoryLocation.Box: 
//                    for (int i = 0; i < baseBagSlots.Count; i++)
//                    {
//                        if (list[i].itemAmount > 0)
//                        {
//                            var item = InventoryManager.Instance.GetItemDetails(list[i].itemID);
//                            baseBagSlots[i].UpdateSlot(item, list[i].itemAmount);
//                        }
//                        else
//                        {
//                            baseBagSlots[i].UpdateEmptySlot();
//                        }
//                    }
//                    break;
//            }

//            //更新钱的UI
//            playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
//        }

//        /// <summary>
//        /// 打开关闭背包UI
//        /// </summary>
//        public void OpenBagUI()
//        {
//            //通过bagOpened = !bagOpened;来实现切换背包的打开状态
//            bagOpened = !bagOpened;

//            bagUI.SetActive(bagOpened);
//        }

//        /// <summary>
//        /// 更新Slot高亮显示
//        /// </summary>
//        /// <param name="index"></param>
//        public void UpdateSlotHeight(int index)
//        {
//            foreach(var slot in playerSlots)
//            {
//                if (slot.slotIndex == index && slot.isSelected)
//                    slot.slotHightlight.gameObject.SetActive(true);
//                else
//                    slot.slotHightlight.gameObject.SetActive(false);
//            }
//        }
//    }
//}
