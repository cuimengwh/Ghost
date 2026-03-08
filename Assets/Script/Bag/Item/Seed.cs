using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.TimeSystem;

[CreateAssetMenu(fileName = "New Seed", menuName = "Inventory/New Seed")]
[System.Serializable]
public class SeedData : ItemData
{
    [Header("种子属性")]
    public Season plantSeason;       // 可种植季节
    public float growDay;            // 生长时间（小时）
    public int PlantId;              // 植物ID
    public int resultingCropId;      // 所得产物ID
    public int yieldAmount;          // 产量
    public bool isGhost;             // 是否为幽灵作物
}
