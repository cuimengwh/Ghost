using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.TimeSystem;

[CreateAssetMenu(fileName = "New Seed", menuName = "Items/Seed")]
[System.Serializable]
public class Seed
{
    [Header("种子属性")]
    [SerializeField] private Season plantSeason;       // 最佳种植季节（双倍产量）
    [SerializeField] private Season[] groweasons;      // 可生长季节
    [SerializeField] private float growDay;            // 生长时间（小时）
    [SerializeField] private int resultingCropId;      // 所得产物ID
    [SerializeField] private int yieldAmount;          // 产量
    [SerializeField] private bool isGhost;             // 是否为幽灵作物
    [SerializeField] private Plant plant;              // 对应的作物模型

    public float GrowDay { get => growDay; set => growDay = value; }
    public Plant Plant { get => plant;}
    public int ResultingCropId { get => resultingCropId; set => resultingCropId = value; }

    public int YieldAmount { get => yieldAmount; set => yieldAmount = value; }
}
