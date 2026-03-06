using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包 ScriptableObject
/// 用于存储和管理背包中的物品数据
/// </summary>
[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventory")]
public class Inventory : ScriptableObject
{
    /// <summary>
    /// 背包物品列表
    /// </summary>
    public List<InventoryItem> items = new List<InventoryItem>();

    /// <summary>
    /// 获取第一个空的物品格下标
    /// </summary>
    /// <returns>空格下标，如果没有空格返回 -1</returns>
    public int GetEmptySlotIndex()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item == null) return i;
        }

        Debug.Log("背包已满");
        return -1;
    }

    /// <summary>
    /// 添加物品
    /// 如果物品可堆叠，则尝试叠加；否则寻找空格存放
    /// </summary>
    /// <param name="item">要添加的物品</param>
    /// <returns>成功返回 true，失败返回 false</returns>
    public bool AddItem(ItemData item)
    {
        // 尝试叠加
        foreach (InventoryItem i in items)
        {
            if (i.item == item && item.isStackable && i.amount < item.maxStack)
            {
                i.amount++; // 数量+1
                InventoryManager.RefreshItem(); // 刷新 UI
                Debug.Log("物品叠加成功");
                return true;
            }
        }

        // 寻找空格
        int index = GetEmptySlotIndex();
        if (index != -1)
        {
            items[index] = new InventoryItem();
            items[index].item = item;
            items[index].amount = 1;
            InventoryManager.RefreshItem(); // 刷新 UI
            Debug.Log("物品添加成功");
            return true;
        }
        else
        {
            // 背包已满
            return false;
        }
    }

    /// <summary>
    /// 使用物品
    /// 数量减 1，当数量为 0 时清空格子
    /// </summary>
    /// <param name="index">物品所在格子下标</param>
    /// <returns>使用成功返回 true，失败返回 false</returns>
    public virtual bool UseItem(int index)
    {
        Debug.Log("使用格子 " + index + " 的物品");
        if (items[index].item != null)
        {
            Debug.Log("物品名称: " + items[index].item.itemName);
            items[index].amount--; // 数量-1
            Debug.Log("物品数量 -1");

            if (items[index].amount <= 0)
            {
                Debug.Log("物品数量为 0，清空格子");
                items[index].item = null;
                items[index].amount = 0;
            }

            Debug.Log("使用完成");
            InventoryManager.RefreshItem(); // 刷新 UI
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 丢弃物品
    /// 清空对应格子
    /// </summary>
    /// <param name="index">物品所在格子下标</param>
    /// <returns>成功返回 true，失败返回 false</returns>
    public bool DropItem(int index)
    {
        Debug.Log("丢弃格子 " + index + " 的物品");
        if (items[index].item != null)
        {
            Debug.Log("物品名称: " + items[index].item.itemName);

            items[index].item = null;
            items[index].amount = 0;

            Debug.Log("丢弃完成");
            InventoryManager.RefreshItem(); // 刷新 UI
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 交换两个格子的物品
    /// </summary>
    /// <param name="indexA">第一个格子下标</param>
    /// <param name="indexB">第二个格子下标</param>
    public void SwapItem(int indexA, int indexB)
    {
        InventoryItem temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        InventoryManager.RefreshItem(); // 刷新 UI
    }
}

/// <summary>
/// 背包格子数据类
/// 在 Inspector 中可见
/// </summary>
[System.Serializable]
public class InventoryItem
{
    /// <summary>
    /// 物品对象
    /// </summary>
    public ItemData item;

    /// <summary>
    /// 物品数量
    /// </summary>
    public int amount;
}
