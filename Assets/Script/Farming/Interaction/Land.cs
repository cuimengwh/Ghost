using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 土地类，挂在土地物体上，负责管理土地状态和与玩家的交互
/// 土地状态包括：未开垦、已耕作、已浇水等，每种状态对应不同的材质显示
/// </summary>
public class Land : MonoBehaviour
{
    // 土地状态枚举
    public enum LandStatus
    {
        dirt,       // 未开垦的土地
        farmland,   // 已耕作但未浇水的土地
        watered,    // 已浇水的土地
        weeded      // 未除草的土地
    }

    // 不同土地状态对应的材质
    public Material dirtMat;        // 土地默认状态的材质
    public Material farmlandMat;    // 已耕作但未浇水土地的材质
    public Material wateredMat;     // 已浇水土地的材质
    public Material weededMat;      // 未除草的土地

    public LandStatus landStatus;   // 当前土地状态
    public GameObject select;       // 土地被选中时显示的提示物体
    public bool isPlant = false;    // 当前土地是否正在种植作物
    public Plant plant = null;     // 当前土地种植的作物的引用
    public Seed seed = null;        //当前土地正在种植的植物种类

    new Renderer renderer;          // 土地物体的渲染器组件

    // 初始化函数
    void Start()
    {
        // 获取渲染器组件
        renderer = GetComponent<Renderer>();
        // 初始化土地状态为未开垦
        SwitchLandStatus(LandStatus.dirt);
        //初始化土地状态为未选中
        Select(false);
    }

    /// <summary>
    /// 切换土地状态
    /// </summary>
    /// <param name="newStatus">新的土地状态</param>
    public void SwitchLandStatus(LandStatus newStatus)
    {
        landStatus = newStatus;
        // 根据土地状态切换对应的材质
        switch (landStatus)
        {
            case LandStatus.dirt:
                Debug.Log("土地状态变更为：干涸");
                renderer.material = dirtMat;
                break;
            case LandStatus.farmland:
                Debug.Log("土地状态变更为：已开垦");
                renderer.material = farmlandMat;
                break;
            case LandStatus.watered:
                Debug.Log("土地状态变更为：已湿润");
                renderer.material = wateredMat;
                break;
            case LandStatus.weeded:
                Debug.Log("土地状态变更为：杂草");
                renderer.material = weededMat;
                break;

        }
    }

    /// <summary>
    /// 设置土地的选中状态
    /// </summary>
    /// <param name="isSelected">是否被选中</param>
    public void Select(bool isSelected)
    {
        // 显示或隐藏选中提示物体
        select.SetActive(isSelected);
    }

    /// <summary>
    /// 改变土地状态为未开垦
    /// </summary>
    public void ChangStatusToDirt()
    {
        SwitchLandStatus(LandStatus.dirt);
    }
    /// <summary>
    /// 改变土地状态为已开垦
    /// </summary>
    public void ChangStatusToFarmland()
    {
        SwitchLandStatus(LandStatus.farmland);
    }
    /// <summary>
    /// 改变土地状态为已浇水
    /// </summary>
    public void ChangStatusToWatered()
    {
        SwitchLandStatus(LandStatus.watered);
    }
    /// <summary>
    /// 改变土地状态为未除草
    /// </summary>
    public void ChangStatusToWeeded()
    {
        SwitchLandStatus(LandStatus.weeded);
    }
    /// <summary>
    /// 种植作物
    /// </summary>
    public void PlantOnLand(Seed _seed)
    {
        seed = _seed;
        plant = _seed.Plant;
        Instantiate(plant, transform.position,Quaternion.identity);
    }
    /// <summary>
    /// 收获作物
    /// </summary>
    public void HarvestFormLand()
    {
        //InventoryManager.Instance.AddItem(InventoryManager.Instance.GetItemDetails(seed.ResultingCropId), false);

        Destroy(plant);
        plant = null;
        seed = null;
    }
}
