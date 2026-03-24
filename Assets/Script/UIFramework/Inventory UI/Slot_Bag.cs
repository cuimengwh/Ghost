using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot_Bag : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("组件获取")]
    [SerializeField] private Image slotImage;
    [SerializeField] private TextMeshProUGUI amountText;
    public Image slotHightlight;
    [SerializeField] private Button button;

    [Header("格子类型")]
    public SlotType slotType;
    public int slotIndex;

    //判断格子是否被选中的bool
    public bool isSelected;

    private Tween selectTween;

    private void Start()
    {
        //游戏开始时，将物品被选中关闭
        isSelected = false;

        if (itemDetails == null)
        {
            UpdateEmptySlot();
        }
    }

    //物品信息
    public ItemDetails itemDetails;
    public int itemAmount;

    public InventoryLocation Location
    {
        get
        {
            return slotType switch
            {
                SlotType.Bag => InventoryLocation.Player,
                SlotType.Box => InventoryLocation.Box,
                _ => InventoryLocation.Player,
            };
        }
    }

    public InventoryUI inventoryUI => FindAnyObjectByType<InventoryUI>();

    /// <summary>
    /// 将Shot更新为空
    /// </summary>
    public void UpdateEmptySlot()
    {
        if (isSelected)
        {
            isSelected = false;

            inventoryUI.UpdateSlotHeight(-1);
            EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
        }

        itemDetails = null;
        //将图片和文字都设置为空，将按钮设置为不能被点按
        slotImage.enabled = false;
        amountText.text = string.Empty;
        button.interactable = false;
    }

    /// <summary>
    /// 更新UI和格子信息
    /// </summary>
    /// <param name="item">itemDetails</param>
    /// <param name="amount">持有数量</param>
    public void UpdateSlot(ItemDetails item, int amount)
    {
        if (item == null)
            UpdateEmptySlot();

        itemDetails = item;
        slotImage.sprite = item.itemIcon;
        itemAmount = amount;
        amountText.text = amount.ToString();
        slotImage.enabled = true;
        button.interactable = true;
    }

    /// <summary>
    /// 控制物品被选中时高亮显示
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        SlotSelected();
    }

    public void SlotSelected()
    {
        if (itemDetails == null || (selectTween != null && selectTween.IsPlaying()))
            return;

        isSelected = !isSelected;

        //通过InventoryUI上面的函数来控制物品框高亮显示，同时其它物品框的高亮关闭
        inventoryUI.UpdateSlotHeight(slotIndex);

        if (slotType == SlotType.Bag)
        {
            //通知被选中物品的信息，更新物品举起的动画
            EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemAmount != 0)
        {
            //显示拖拽的图片，并且调整为原来的尺寸
            inventoryUI.dragItem.enabled = true;
            inventoryUI.dragItem.sprite = slotImage.sprite;

            //拖拽时也保持高亮显示
            isSelected = true;
            inventoryUI.UpdateSlotHeight(slotIndex);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        inventoryUI.dragItem.transform.position = Input.mousePosition + new Vector3(-50f, -5f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        inventoryUI.dragItem.enabled = false;
        Debug.Log(eventData.pointerCurrentRaycast.gameObject);

        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            //图标放在其它UI上面就return
            if (eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot_Bag>() == null)
                return;

            #region 背包内交换
            var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot_Bag>();
            int targetIndex = targetSlot.slotIndex;

            //判断交换的双方是不是都是Player的背包里面得到格子
            if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Bag) //Bag内部交换
            {
                InventoryManager.Instance.SwapItem(slotIndex, targetIndex);
            }
            else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag) //从商店买东西 
            {
                EventHandler.CallShowTradeUIEvent(itemDetails, false);
            }
            else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop) //从背包卖东西 
            {
                EventHandler.CallShowTradeUIEvent(itemDetails, true);
            }
            else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType) //跨背包数据交换
            {
                InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                Debug.Log("交换成功");
            }
            else if (slotType == SlotType.Box && slotType == targetSlot.slotType)
            {
                InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
            }

            //交换玩关闭所有高亮
            inventoryUI.UpdateSlotHeight(-1);
            #endregion
        }
        else
        {
            #region 扔在地上
            //判断物体可以被扔到地下吗
            if (itemDetails.canDropped)
            {
                //鼠标对应的世界坐标
                var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
                itemAmount -= 1;
                EventHandler.CallInstantiateItemScene(itemDetails.itemID, pos);
            }
            #endregion
        }
    }
}
