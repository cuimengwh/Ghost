using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CropDataList_SO", menuName = "Crop/CropDataList")]
public class SeedDataList_SO : ScriptableObject
{
    public List<Seed> SeedDataList;

    public Seed Find(int id)
    {
        return SeedDataList.Find(i => i.Id == id);
    }
}
