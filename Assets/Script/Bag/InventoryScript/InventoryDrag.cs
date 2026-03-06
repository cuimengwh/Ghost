using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

/// <summary>
/// InventoryDrag - 处理背包UI中物品图标的拖拽交互
/// </summary>
public class InventoryDrag : MonoBehaviour, IDragHandler
{
    // 存储当前UI元素的RectTransform组件引用
    RectTransform currentPosition;

    // IDragHandler接口的实现方法，当拖拽发生时每帧调用
    public void OnDrag(PointerEventData eventData)
    {
        // 更新UI元素的位置：当前位置加上拖拽的位移量
        // eventData.delta提供了从上一帧到当前帧的鼠标/触摸位移
        currentPosition.anchoredPosition += eventData.delta;
    }

    // Unity生命周期方法，在脚本初始化时调用（早于Start）
    public void Awake()
    {
        // 获取并存储当前游戏对象上的RectTransform组件引用
        // RectTransform是UI元素的变换组件，包含位置、旋转、缩放等信息
        currentPosition = GetComponent<RectTransform>();
    }
}
