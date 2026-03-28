using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Crop", menuName = "Items/Crop")]
public class Crop : ItemDetails
{
    [Header("农产品属性")]
    [SerializeField] private int seedId;             // 对应的种子id
    [SerializeField] private int star;               //星级
}
