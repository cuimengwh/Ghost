using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedToPlantManager
{
    private static Dictionary<int,SeedData> seedDic = new Dictionary<int,SeedData>();
    private static Dictionary<int,Plant> plantDic = new Dictionary<int, Plant>();

    /// <summary>
    /// 用种子ID查找对应植物模型
    /// </summary>
    /// <param name="seedID"></param>
    /// <returns></returns>
    public static Plant FindSeedToPlant(int seedID)
    {
        if(seedDic.TryGetValue(seedID, out SeedData _seed) && seedDic != null)
        {
            if(plantDic.TryGetValue(_seed.PlantId,out Plant plant) && plantDic != null)
            {
                return plant;
            }else
            {
                Debug.Log("未找到作物模型");
                return null;
            }
        }else
        {
            Debug.Log("未找到种子");
            return null;
        }
        
    }
}
