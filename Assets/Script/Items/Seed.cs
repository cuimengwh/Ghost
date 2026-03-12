using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.TimeSystem;

[CreateAssetMenu(fileName = "New Seed", menuName = "Items/Seed")]
[System.Serializable]
public class Seed : ScriptableObject
{
    [Header("种子属性")]
    public int id;                   // 种子ID
    public Season plantSeason;       // 最佳种植季节（双倍产量）
    public Season[] groweasons;      // 可生长季节
    public float growDay;            // 生长时间（小时）
    public int resultingCropId;      // 所得产物ID
    public int yieldAmount;          // 产量
    public bool isGhost;             // 是否为幽灵作物
    public Plant plant;              // 对应的作物模型
    public int buyPrice;             // 购买价格
    public int sellPrice;            // 出售价格
}
