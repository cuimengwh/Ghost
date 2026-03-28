using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryBag_SO", menuName = "Inventory/InventoryBag_SO", order = 0)]  
public class InventoryBag_SO : ScriptableObject
{
    //创建一个个背包来管理数据，一个背包的数据文件只包含两种数据类型，物品的ID和数量
    public List<InventoryItem> itemList;

    public InventoryItem GetInventoryItem(int ID)
    {
        return itemList.Find(i => i.itemID == ID);
    }
}
