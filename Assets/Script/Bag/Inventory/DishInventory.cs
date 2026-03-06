using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dish Inventory", menuName = "Inventory/New Dish Inventory")]
public class DishInventory : Inventory
{
    public override bool UseItem(int index)
    {
        Debug.Log("使用格子 " + index + " 的菜肴");
        if (items[index].item != null && items[index].item is Dish)
        {
            Dish dish = (Dish)items[index].item;
            // 这里可以添加使用菜肴的逻辑，比如恢复饥饿值、生命值等
            Debug.Log($"你吃了一道 {dish.itemName}，恢复了 {dish.hungerRestore} 点饥饿值，{dish.healthRestore} 点生命值，{dish.sanityRestore} 点理智值。");

            // 使用后减少数量
            items[index].amount--;
            if (items[index].amount <= 0)
            {
                items[index].item = null;

            }
            return true;
        }
        return false;
    }
}
