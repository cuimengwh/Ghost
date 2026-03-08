using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    [SerializeField] private float maxGrowTime; //需要成熟的时间
    private float currentGrowTime; //当前种植时间
    private PlantStatus currentStatus; //当前种植阶段
    [SerializeField] private GameObject[] stageModels; // 各个阶段的模型数组
    private GameObject currentModel = null; //当前显示的模型

    public enum PlantStatus
    {
        intermediateStage, //初级阶段
        advancedStage, //高级阶段
        matureStage //成熟阶段
    }

    private void Start()
    {
        //初始化种植时间
        currentGrowTime = 0;
    }

    private void Update()
    {
        ChangePlantStatus(); // 每帧检测作物状态

        //每帧更新种植时间
        if (currentGrowTime <  maxGrowTime)
        {
            currentGrowTime += Time.deltaTime;
        }
    }
    /// <summary>
    /// 更改作物状态
    /// </summary>
    private void ChangePlantStatus()
    {
        //第一阶段没有模型
        if (currentGrowTime / maxGrowTime <= 0.25f) { }
        else if (currentGrowTime / maxGrowTime <= 0.5f)
        {
            currentStatus = PlantStatus.intermediateStage;
            UpdateModel((int)currentStatus);
        }
        else if (currentGrowTime / maxGrowTime < 1.0f)
        {
            currentStatus = PlantStatus.advancedStage;
            UpdateModel((int)currentStatus);
        }
        else
        {
            currentStatus = PlantStatus.matureStage;
            UpdateModel((int)currentStatus);
        }
    }
    /// <summary>
    /// 更新作物模型
    /// </summary>
    private void UpdateModel(int stageIndex)
    {
        //隐藏当前状态模型
        currentModel.gameObject.SetActive(false);

        // 显示当前阶段的模型
        if (stageModels != null && stageIndex < stageModels.Length)
        {
            stageModels[stageIndex].SetActive(true);
            currentModel = stageModels[stageIndex];
        }
    }
}
