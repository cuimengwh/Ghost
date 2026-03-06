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
    public int resultingCropId;      // 所得产物ID
    public GameObject plantPrefab;   // 种下的预制体
    public GameObject growPrefab;    // 生长中预制体
    public GameObject harvestPrefab; // 可收获预制体
    public int yieldAmount;          // 产量
    public bool isGhost;             // 是否为幽灵作物
}
