using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

#region 总数据

/// <summary>
/// 公共物品数据类
/// </summary>
[System.Serializable]
public class ItemData : ScriptableObject
{
    [Header("基础物品属性")]
    public string id;              // 物品ID
    public string itemName;        // 物品名称
    [TextArea]
    [Tooltip("物品描述")]
    public string itemDescription; // 物品描述
    public int maxStack = 99;      // 最大堆叠
    public bool isStackable = true;// 是否可堆叠
    public bool canSell = true;    // 是否可出售
    public int value;              // 价值
    public int sellPrice;          // 售出价格
    public GameObject prefab;      // Prefab
    public Sprite itemIcon;        // 物品图标
}

#endregion
