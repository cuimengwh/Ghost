using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName = "New Tool", menuName = "Inventory/New Tool")]
[Serializable]
public class Tool : ItemData
{
    [Header("工具属性")]
    public ToolType toolType;           // 工具类型
    public ToolMaterial toolMaterial;   // 工具材质
    public int energyCost;              // 精力花费
    public int durability;              // 耐久度
    public float useSpeed;              // 使用速度
    public float range;                 // 作用/攻击范围
    public int damage;                  // 伤害值
}
/// <summary>
/// 工具类型
/// </summary>
public enum ToolType
{
    Hoe,           // 锄头
    WateringCan,   // 喷壶
    Axe,           // 斧子
    Pickaxe,       // 镐子
    Scythe         // 镰刀
}
/// <summary>
/// 工具材质
/// </summary>
public enum ToolMaterial
{
    Wood,         // 木质
    Stone,        // 石质
    Iron,         // 铁质
    Gold,         // 金质
    Diamond       // 钻石质
}