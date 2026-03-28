using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utopia.Core.Event;
using Utopia.TimeSystem;
public enum PlantStatus
{
    intermediateStage, //初级阶段
    advancedStage, //高级阶段
    matureStage //成熟阶段
}

public class Plant : MonoBehaviour
{
    [SerializeField] private GameObject[] stageModels; // 各个阶段的模型数组
    private Seed seed;                                  //作物对应的种子
    [SerializeField] public int seedID;               // 种子的id
    private float maxGrowTime;                         //需要成熟的时间
    [SerializeField] private float currentGrowTime;                     //当前种植时间
    [SerializeField] private PlantStatus currentStatus;                 //当前种植阶段
    [SerializeField] private GameObject currentModel;            //当前显示的模型
    [SerializeField] private SeedDataList_SO SeedDataList_SO; //种子列表

    public PlantStatus CurrentStatus { get => currentStatus; set => currentStatus = value; }
    public Seed Seed { get => seed; set => seed = value; }

    private void Awake()
    {
        Seed = SeedDataList_SO.Find(seedID);
        maxGrowTime = Seed.GrowDay;
    }

    private void OnEnable()
    {
        TimeManager.instance.OnDayChanged += AfterADay;
    }

    private void OnDisable()
    {
        TimeManager.instance.OnDayChanged -= AfterADay;
    }

    private void Start()
    {
        //初始化种植时间
        currentGrowTime = 0;
        currentModel = stageModels[0];
        currentModel.SetActive(false);
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
            CurrentStatus = PlantStatus.intermediateStage;
            UpdateModel((int)CurrentStatus);
        }
        else if (currentGrowTime / maxGrowTime < 1.0f)
        {
            CurrentStatus = PlantStatus.advancedStage;
            UpdateModel((int)CurrentStatus);
        }
        else
        {
            CurrentStatus = PlantStatus.matureStage;
            UpdateModel((int)CurrentStatus);
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
