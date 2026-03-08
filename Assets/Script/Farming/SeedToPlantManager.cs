using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedToPlantManager : MonoBehaviour
{
    private static Dictionary<int,SeedData> seedDic = new Dictionary<int,SeedData>();
    private static Dictionary<int,Plant> plantDic = new Dictionary<int, Plant>();

    private void Start()
    {
        ReadCSVConfig();
        // 测试：读取ID为10001的道具
        if (seedDic.ContainsKey(1001))
        {
            SeedData potion = seedDic[1001];
            Debug.Log($"道具ID：{potion.itemID}，名称：{potion.itemName}");
        }
    }
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
    /// <summary>
    /// 读取CSV配置表
    /// </summary>
    void ReadCSVConfig()
    {
        try
        {
            // 从Resources加载CSV文件（无需后缀名）
            TextAsset csvFile = Resources.Load<TextAsset>("种子&作物模型&产物配置表");
            if (csvFile == null)
            {
                Debug.LogError("CSV文件不存在！请检查Resources/种子&作物模型&产物配置表.csv是否存在");
                return;
            }

            // 按行分割内容（\r\n是Windows换行符，\n是Mac/Linux）
            string[] lines = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) // 至少要有表头+1行数据
            {
                Debug.LogError("CSV文件内容为空或只有表头！");
                return;
            }

            // 跳过表头（第一行），从第二行开始解析数据
            for (int i = 1; i < lines.Length; i++)
            {
                // 按逗号分割列（简单版，若内容含逗号需用更复杂的解析逻辑）
                string[] cols = lines[i].Split(',');
                //if (cols.Length != 4) // 检查列数是否匹配
                //{
                //    Debug.LogWarning($"第{i + 1}行数据格式错误，跳过：{lines[i]}");
                //    continue;
                //}

                // 转换数据类型并存储
                SeedData data = new SeedData();
                data.itemID = int.Parse(cols[0]);
                data.itemName = cols[1];

                // 存入字典（避免重复ID）
                if (!seedDic.ContainsKey(data.itemID))
                {
                    seedDic.Add(data.itemID, data);
                }
                else
                {
                    Debug.LogWarning($"ID为{data.itemID}的道具重复，跳过");
                }
            }

            Debug.Log($"CSV配置表读取成功，共加载{seedDic.Count}条数据");
        }
        catch (Exception e)
        {
            Debug.LogError($"读取CSV失败：{e.Message}");
        }
    }
}
