using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.TimeSystem;

[CreateAssetMenu(fileName = "New Crop", menuName = "Inventory/New Crop")]
[System.Serializable]
public class CropData : ItemData
{
    [Header("作物属性")]
    public int cropQuality;          // 作物品质
    public Season suitableSeason;    // 适应的季节
}

