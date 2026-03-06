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
        farmland,   // 已耕作的土地
        watered     // 已浇水的土地
    }

    // 不同土地状态对应的材质
    public Material dirtMat;        // 土地默认状态的材质
    public Material farmlandMat;    // 已耕作土地的材质
    public Material wateredMat;     // 已浇水土地的材质

    public LandStatus landStatus;   // 当前土地状态
    public GameObject select;       // 土地被选中时显示的提示物体

    new Renderer renderer;          // 土地物体的渲染器组件

    // 初始化函数
    void Start()
    {
        // 获取渲染器组件
        renderer = GetComponent<Renderer>();
        // 初始化土地状态为未开垦
        SwitchLandStatus(LandStatus.dirt);
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
                renderer.material = dirtMat;
                break;
            case LandStatus.farmland:
                renderer.material = farmlandMat;
                break;
            case LandStatus.watered:
                renderer.material = wateredMat;
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
    /// 与土地交互的处理函数
    /// </summary>
    public void Interact()
    {
        // 将土地状态切换为已耕作
        SwitchLandStatus(LandStatus.farmland);
    }
}
