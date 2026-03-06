using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 菜肴数据类，继承自 ItemData
/// </summary>
[CreateAssetMenu(fileName = "New Dish", menuName = "Item/New Dish")]
public class Dish : ItemData
{
    [Header("菜肴属性")]
    public int hungerRestore; // 恢复饥饿值
    public int healthRestore; // 恢复生命值
    public int sanityRestore; // 恢复理智值
    public int dishStars;    // 菜肴星级
    public List<string> ingredients; // 原料列表
    
    // 其他与菜相关的属性和方法可以在这里添加
}
