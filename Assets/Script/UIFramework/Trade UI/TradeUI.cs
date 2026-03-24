using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeUI : MonoBehaviour
{
    [Header("组件获取")]
    public Slot_Bag[] slots;
    public Image SubmitPanel;
    public Image itemIcon;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;
    public Button submitButton;
    public Button cancelButton;
    public TextMeshProUGUI playerMoneyText;

    private ItemDetails item;
    private bool isSellTrade;
    private TradeUIAnimation tradeUIAnimation => FindAnyObjectByType<TradeUIAnimation>();

    private void OnEnable()
    {
        EventHandler.ShowTradeUIEvent += SetupTradeUI;
    }

    private void OnDisable()
    {
        EventHandler.ShowTradeUIEvent -= SetupTradeUI;
    }

    private void Awake()
    {
        cancelButton.onClick.AddListener(CancelTrade);
        submitButton.onClick.AddListener(TradeItem);
    }

    public void InitItemData()
    {
        foreach(var slot in slots)
        {
            var item = InventoryManager.Instance.GetItemDetails(slot.itemDetails.itemID);
            slot.UpdateSlot(item, 10000);
        }
    }

    /// <summary>
    /// 设置TradeUI显示详情
    /// </summary>
    /// <param name="item"></param>
    /// <param name="isSell"></param>
    public void SetupTradeUI(ItemDetails item, bool isSell)
    {
        this.item = item;
        itemIcon.sprite = item.itemIcon;
        itemName.text = item.itemName;
        itemPrice.text = item.itemPrice.ToString();
        isSellTrade = isSell;

        SubmitPanel.gameObject.SetActive(true);
    }

    private void TradeItem()
    {
        //TODO:如果有需要改成买多个
        InventoryManager.Instance.TradeItem(item, 1, isSellTrade);
        CancelTrade();

        playerMoneyText.text = InventoryManager.Instance.playerMoney.ToString();
        tradeUIAnimation.PlayTradeAnimation();
    }

    private void CancelTrade()
    {
        SubmitPanel.gameObject.SetActive(false);
    }
}