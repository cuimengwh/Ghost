using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在可捡起的物品上，负责与玩家碰撞检测，
/// 并将物品添加到指定的 Inventory 中。
/// </summary>
public class ItemPick : MonoBehaviour
{
    [Header("物品属性")]
    public ItemData thisItem;           // 当前物品的引用，包含物品的所有属性信息

    [Header("背包系统")]
    public Inventory inventory;     // 目标背包系统的引用，物品将被添加到此背包中

    /// <summary>
    /// 当其他碰撞体进入触发器时调用
    /// </summary>
    /// <param name="other">进入触发器的碰撞体</param>
    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞体是否为玩家
        if (other.CompareTag("Player"))
        {
            // 如果是玩家，执行拾取操作
            PickUp();
        }
    }

    /// <summary>
    /// 拾取物品的主要逻辑
    /// 将物品添加到背包，如果添加成功则销毁场景中的物品对象
    /// </summary>
    void PickUp()
    {
        // 尝试将物品添加到背包，并获取添加结果
        bool add = inventory.AddItem(thisItem);

        // 如果物品成功添加到背包，销毁场景中的物品对象
        if (add)
            Destroy(gameObject);
    }
}
