using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.Core.Event;
using Utopia.TimeSystem;

public class Plant : MonoBehaviour
{
    [SerializeField] private GameObject[] stageModels; // 各个阶段的模型数组
    public Seed seed;                                  //作物对应的种子
    private float maxGrowTime;                         //需要成熟的时间
    private float currentGrowTime;                     //当前种植时间
    private PlantStatus currentStatus;                 //当前种植阶段
    private GameObject currentModel = null;            //当前显示的模型
    private TimeManager timeManager;                   //时间管理器

    public enum PlantStatus
    {
        intermediateStage, //初级阶段
        advancedStage, //高级阶段
        matureStage //成熟阶段
    }

    private void Awake()
    {
        TryGetComponent(out timeManager);
        maxGrowTime = seed.GrowDay;
    }

    private void OnEnable()
    {
        timeManager.OnDayChanged += AfterADay;
    }

    private void OnDisable()
    {
        timeManager.OnDayChanged -= AfterADay;
    }

    private void Start()
    {
        //初始化种植时间
        currentGrowTime = 0;
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

    public void AfterADay(int day)
    {
        currentGrowTime++;
        ChangePlantStatus();
    }
}
