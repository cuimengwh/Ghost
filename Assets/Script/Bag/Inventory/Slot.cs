using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// 挂在每个背包格子（slot）上。
/// 负责保存并显示该格子对应的物品图标、描述文本，
/// 以及响应点击事件，将物品信息展示到外部面板。
/// </summary>
public class Slot : MonoBehaviour
{
    [Header("UI 组件")]
    public Image slotImage;         // 显示物品图标
    public TMP_Text slotAmount;     // 显示堆叠数量（当前未启用）
    public GameObject itemInSlot;   // 图标容器，用于 SetActive 控制显示/隐藏
    public int slotIndex;          // 当前格子在背包中的索引

    [Header("数据")]
    public ItemData slotItem;           // 当前格子绑定的物品数据

    public void ItemOnClicked()
    {
        // 将当前物品描述传递给 InventoryManager，由它负责刷新详情面板
        InventoryManager.UpdateItemInfo(slotItem);

        //高亮当前格子


        Debug.Log("Clicked " + slotItem.itemName + " in slot " + slotIndex);
    }

    /// <summary>
    /// 外部调用，用于初始化或刷新格子显示
    /// </summary>
    public void SetUpSlot(InventoryItem item,int index)
    {
        slotIndex = index;

        if (item.item == null)
        {
            itemInSlot.SetActive(false);
            
            return;
        }
        slotItem = item.item;

        // 显示图标
        itemInSlot.SetActive(true);
        slotImage.sprite = slotItem.itemIcon;
        if (item.amount <= 1)
            slotAmount.text = "";
        else
            slotAmount.text = item.amount.ToString();

    }
    
}
