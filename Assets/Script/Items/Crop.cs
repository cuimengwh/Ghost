using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop", menuName = "Items/Crop")]
public class Crop : Items
{
    [Header("农产品属性")]
    [SerializeField] private int buyPrice;             // 购买价格
    [SerializeField] private int sellPrice;            // 出售价格
    [SerializeField] private int seedId;              // 对应的种子id

    public int BuyPrice { get => buyPrice; set => buyPrice = value; }
    public int SellPrice { get => sellPrice; set => sellPrice = value; }
}
