using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备栏（快捷栏）管理器
/// 
/// 功能说明：
/// 1. 提供游戏内的快捷物品栏，直接引用背包前N个格子
/// 2. 支持1-9数字键快速切换和选择物品
/// 3. 单例模式实现，全局唯一访问点
/// 4. 与背包数据实时同步，无独立存储
/// 
/// 设计特点：
/// - 数据引用模式：直接引用背包数据，不单独存储
/// - 快捷键支持：数字键1-9对应快捷栏1-9
/// - 索引映射：快捷栏索引=背包前N个格子索引
/// 
/// 使用要求：
/// 1. 需要关联到有效的Inventory背包
/// 2. 背包必须有足够的格子数量（≥hotbarSize）
/// </summary>
public class EquipBar : MonoBehaviour
{
    // 单例实例，全局访问点
    public static EquipBar instance;

    [Header("背包数据")]
    public Inventory myBag;         // 绑定的背包数据引用（与背包前N个格子共享）
    public int hotbarSize = 9;      // 快捷栏格子数量（默认9，对应键盘1-9数字键）

    [Header("当前选择")]
    public int currentIndex = 0;    // 当前选中的快捷栏索引（0-8）

    /// <summary>
    /// Awake初始化方法
    /// 实现单例模式，确保全局只有一个快捷栏管理器
    /// </summary>
    void Awake()
    {
        // 单例模式实现
        if (instance == null)
        {
            instance = this;  // 首次创建，设置单例实例
        }
        else
        {
            Destroy(gameObject);  // 销毁重复实例，确保单例唯一性
        }
    }

    /// <summary>
    /// 每帧更新方法
    /// 监听键盘数字键1-9，实现快捷键切换功能
    /// </summary>
    void Update()
    {
        // 循环监听1-9数字键
        for (int i = 0; i < hotbarSize; i++)
        {
            // 检测对应数字键是否被按下（i+1转换索引为1-9数字键）
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                SelectSlot(i);  // 选中对应槽位
            }
        }
    }

    /// <summary>
    /// 选择指定快捷栏槽位
    /// </summary>
    /// <param name="index">快捷栏索引（0-8）</param>
    public void SelectSlot(int index)
    {
        // 参数有效性检查
        if (index < 0 || index >= hotbarSize) return;

        // 更新当前选中索引
        currentIndex = index;

        // 从背包获取对应槽位的物品数据
        var item = myBag.items[index].item;

        // 调试输出当前选择信息
        if (item != null)
        {
            Debug.Log($"当前选择：槽位 {index + 1} -> {item.itemName}");
        }
        else
        {
            Debug.Log($"当前选择：槽位 {index + 1} -> 空");
        }

        // TODO: 后续功能扩展点
        // 1. 更新UI高亮：在快捷栏UI上高亮显示当前选中的槽位
        // 2. 手持物品：调用HandManager.SetItem(item)显示玩家手中的物品模型
    }

    /// <summary>
    /// 获取当前选中的物品数据
    /// 用于物品使用、装备等操作
    /// </summary>
    /// <returns>当前选中槽位的物品数据，可能为null</returns>
    public ItemData GetCurrentItem()
    {
        // 直接从背包数据中获取当前索引对应的物品
        return myBag.items[currentIndex].item;
    }
}