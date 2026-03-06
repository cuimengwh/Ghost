using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 土地交互组件,挂在玩家脚下的空物体上，负责检测玩家与土地的交互
/// </summary>
public class LandInteraction : MonoBehaviour
{
    PlayerController playerController; // 玩家控制器引用
    Land selectedLand = null; // 当前选中的土地

    // 初始化
    void Start()
    {
        // 从父对象获取PlayerController组件
        playerController = transform.parent.GetComponent<PlayerController>();
        /* 调试代码：检查是否成功获取PlayerController
        if (playerController == null) {Debug.LogError("PlayerController component not found on parent.");}
        else {Debug.Log("PlayerController found: " + playerController);}
        */
    }

    // 每帧更新
    void Update()
    {
        RaycastHit hit; // 射线命中信息

        // 向下发射射线检测土地（射线长度2个单位）
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            SelectLand(hit.collider.GetComponent<Land>()); // 尝试选择土地
            // 检测E键按下交互
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnInteractableHit(hit); // 执行交互操作
            }
        }
        else
        {
            SelectLand(null); // 没有命中土地，取消选择
        }
    }

    // 处理与可交互物体的交互
    public void OnInteractableHit(RaycastHit hit)
    {
        Collider sth = hit.collider; // 获取碰撞体
        // 检查是否为土地
        if (sth.CompareTag("Land"))
        {
            Land land = sth.GetComponent<Land>(); // 获取土地组件
            land.Interact(); // 调用土地交互方法
        }
    }

    // 选择土地的方法
    void SelectLand(Land land)
    {
        // 点空地：只清旧选
        if (land == null)
        {
            if (selectedLand != null)
            {
                selectedLand.Select(false);
                selectedLand = null;
            }
            return;
        }

        // 点同一块：直接返回
        if (land == selectedLand) return;

        // 切到另一块：清旧选新
        if (selectedLand != null)
            selectedLand.Select(false);

        selectedLand = land;
        land.Select(true);
    }
}
