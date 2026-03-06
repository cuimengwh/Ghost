using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 物品拖拽组件 - 处理可拖拽物品的UI交互
/// 
/// 功能说明：
/// 1. 支持拖拽物品图标在背包内移动
/// 2. 实现三种目标交互：
///    - 与其他物品交换位置
///    - 移动到空槽位
///    - 拖出背包外丢弃物品
/// 3. 拖拽失败时自动归位
/// 
/// 实现接口：
/// - IBeginDragHandler: 开始拖拽
/// - IDragHandler:     拖拽中
/// - IEndDragHandler:  结束拖拽
/// 
/// 使用要求：
/// 1. 挂载在物品图标(Image)GameObject上
/// 2. 父级必须是Slot对象
/// 3. 需要CanvasGroup组件支持射线遮挡控制
/// </summary>
public class ItemDrag : MonoBehaviour,
                        IBeginDragHandler,
                        IDragHandler,
                        IEndDragHandler
{
    // 拖拽相关数据
    public Inventory myBag;        // 当前物品所属的背包数据
    public Transform originalParent; // 拖拽前的父级Transform（槽位）
    public int slotIndex;          // 在背包中的索引位置
    private bool isDragging = false; // 拖拽状态标志，防止重复拖拽

    #region Unity拖拽接口实现

    /// <summary>
    /// 开始拖拽时调用
    /// 初始化拖拽数据，将图标提升到UI层级顶部
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 记录原始位置和背包数据
        originalParent = transform.parent;
        myBag = InventoryManager.instance.myBag;
        slotIndex = originalParent.GetComponent<Slot>().slotIndex;

        // 提升UI层级避免被遮挡
        transform.SetParent(transform.parent.parent);
        transform.position = eventData.position;

        // 禁用射线检测，避免拖拽时检测到自身
        GetComponent<CanvasGroup>().blocksRaycasts = false;
        isDragging = true;
    }

    /// <summary>
    /// 拖拽过程中持续调用
    /// 更新图标位置跟随鼠标/触摸点
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // 持续更新位置跟随指针
        transform.position = eventData.position;
    }

    /// <summary>
    /// 结束拖拽时调用
    /// 处理物品放置逻辑，判断目标位置有效性
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 获取当前指针下的目标物体
        GameObject hitObject = GetHitObject(eventData);

        // 处理拖拽到背包外的情况
        if (hitObject == null)
        {
            HandleDropOutside();
            return;
        }

        // 根据目标物体类型执行不同处理逻辑
        switch (hitObject.name)
        {
            case "Image":  // 拖拽到另一个物品图标上
                HandleSwapWithItem(hitObject);
                break;
            case "slot(Clone)":  // 拖拽到空槽位上
                HandleMoveToEmptySlot(hitObject);
                break;
            default:  // 无效目标，返回原位置
                ReturnToOriginalPosition();
                break;
        }
    }

    #endregion

    #region 拖拽处理逻辑

    /// <summary>
    /// 获取当前指针位置下的GameObject
    /// 使用异常处理确保安全
    /// </summary>
    /// <returns>命中的物体，没有则为null</returns>
    private GameObject GetHitObject(PointerEventData eventData)
    {
        try
        {
            // 获取当前射线检测到的物体
            return eventData.pointerCurrentRaycast.gameObject;
        }
        catch (System.Exception e)
        {
            // 捕获异常并记录日志
            Debug.LogException(e);
            ReturnToOriginalPosition();
            return null;
        }
    }

    /// <summary>
    /// 处理拖拽到背包外的情况（丢弃物品）
    /// </summary>
    private void HandleDropOutside()
    {
        Debug.Log("没有检测到有效物品，丢弃物品");

        // 从背包数据中移除物品
        InventoryManager.instance.myBag.DropItem(slotIndex);

        // [TODO] 后续优化：从对象池回收物品图标
        // Destroy(gameObject);

        // 完成拖拽设置
        EndDragSettings();
    }

    /// <summary>
    /// 处理与另一个物品交换位置
    /// </summary>
    /// <param name="hitItem">命中的物品图标</param>
    private void HandleSwapWithItem(GameObject hitItem)
    {
        // 获取目标物品的槽位索引
        int targetIndex = hitItem.transform.GetComponentInParent<Slot>().slotIndex;

        // 获取UI层级变换所需Transform
        Transform hitParent = hitItem.transform.parent;  // 目标物品的父级（槽位）
        Transform myNewParent = hitParent.parent;        // 目标物品的祖父级（用于重新设置父级）

        // 移动自身到目标位置
        transform.SetParent(myNewParent);
        transform.position = myNewParent.position;

        // 移动目标物品到自身原位置
        hitParent.position = originalParent.position;
        hitParent.SetParent(originalParent);

        // 交换背包中的数据
        Debug.Log($"即将交换 {slotIndex} 和 {targetIndex}");
        myBag.SwapItem(slotIndex, targetIndex);

        // 完成拖拽设置
        EndDragSettings();
    }

    /// <summary>
    /// 处理移动到空槽位
    /// </summary>
    /// <param name="hitSlot">命中的空槽位</param>
    private void HandleMoveToEmptySlot(GameObject hitSlot)
    {
        // 获取目标槽位索引
        int targetIndex = hitSlot.transform.GetComponentInParent<Slot>().slotIndex;

        // 移动自身到目标槽位
        transform.SetParent(hitSlot.transform);
        transform.position = hitSlot.transform.position;

        // 安全处理：如果目标槽位已有子物体（理论上不会，但做防御性处理）
        if (hitSlot.transform.childCount > 0)
        {
            Transform existingChild = hitSlot.transform.GetChild(0);
            existingChild.position = originalParent.position;
            existingChild.SetParent(originalParent);
        }

        // 交换背包中的数据（实际上是移动，但使用交换接口统一处理）
        myBag.SwapItem(slotIndex, targetIndex);

        // 完成拖拽设置
        EndDragSettings();
    }

    #endregion

    #region 通用辅助函数

    /// <summary>
    /// 拖拽结束后的通用设置
    /// 恢复射线检测，重置拖拽状态
    /// </summary>
    private void EndDragSettings()
    {
        // 恢复射线检测
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        // 重置拖拽状态
        isDragging = false;
    }

    /// <summary>
    /// 返回原始位置
    /// 当拖拽到无效区域或出现异常时调用
    /// </summary>
    private void ReturnToOriginalPosition()
    {
        // 确保原始父级存在
        if (originalParent != null)
        {
            // 恢复父级和位置
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
        }
        // 应用结束设置
        EndDragSettings();
    }

    #endregion

    #region 强制结束拖拽方法

    /// <summary>
    /// 强制结束当前物品的拖拽
    /// 在UI关闭、背包切换等情况下调用
    /// </summary>
    private void ForceEndCurrentDrag()
    {
        if (isDragging)
        {
            Debug.Log($"强制结束 {gameObject.name} 的拖拽");
            ReturnToOriginalPosition();
        }
    }

    /// <summary>
    /// 静态方法：强制结束所有正在拖拽的物品
    /// 在背包关闭、场景切换时调用
    /// </summary>
    public static void ForceEndDrag()
    {
        // 获取背包网格容器
        var grid = InventoryManager.instance.slotGrid.transform;

        // 遍历所有槽位，强制结束拖拽中的物品
        for (int i = 0; i < grid.childCount; i++)
        {
            var itemDrag = grid.GetChild(i).GetComponent<ItemDrag>();
            if (itemDrag != null && itemDrag.isDragging)
            {
                itemDrag.ReturnToOriginalPosition();
            }
        }
    }

    #endregion
}