using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Items/Crop")]
public class Crop : ScriptableObject
{
    [Header("农产品属性")]
    public int id;
    public int buyPrice;             // 购买价格
    public int sellPrice;            // 出售价格
}
