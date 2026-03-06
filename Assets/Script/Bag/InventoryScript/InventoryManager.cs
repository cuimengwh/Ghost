using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 库存管理器 - 管理背包UI显示和交互
/// 单例模式实现，全局只有一个库存管理器实例
/// </summary>
public class InventoryManager : MonoBehaviour
{
    // 单例实例，通过静态属性全局访问
    public static InventoryManager instance;

    [Header("背包数据")]
    public Inventory myBag;                   // 主背包数据
    public CollectionInventory myCollectionBag; // 收集品背包
    public DishInventory myDishBag;           // 菜肴背包

    [Header("UI组件")]
    public GameObject slotGrid;               // 物品槽容器
    public GameObject emptySlot;              // 物品槽预制体

    [Header("物品信息显示")]
    public TMP_Text itemInfo;                 // 物品描述
    public Image itemIcon;                    // 物品图标
    public TMP_Text itemName;                 // 物品名称

    [Header("背包切换按钮")]
    public Button bagButton;                  // 普通背包按钮
    public Button collectionButton;           // 收集品按钮
    public Button dishButton;                 // 菜肴按钮

    [Header("物品槽列表")]
    public List<GameObject> slots = new List<GameObject>(); // 当前显示的物品槽

    private Inventory currentInventory;       // 当前显示的背包

    /// <summary>
    /// 初始化单例实例
    /// </summary>
    void Awake()
    {
        // 单例模式实现：确保只有一个实例存在
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // 销毁重复实例
        }
    }

    /// <summary>
    /// 激活时刷新背包显示
    /// </summary>
    public void OnEnable()
    {
        RefreshItem(currentInventory); // 刷新当前背包
        instance.itemInfo.text = " ";  // 清空信息显示
    }

    private void Start()
    {
        // 默认显示主背包
        currentInventory = myBag;

        // 绑定背包切换按钮事件
        bagButton.onClick.AddListener(() => SwitchInventory(myBag));
        collectionButton.onClick.AddListener(() => SwitchInventory(myCollectionBag));
        dishButton.onClick.AddListener(() => SwitchInventory(myDishBag));
    }

    /// <summary>
    /// 刷新背包UI显示
    /// </summary>
    /// <param name="targetInventory">要刷新的背包数据，默认使用主背包</param>
    public static void RefreshItem(Inventory targetInventory = null)
    {
        // 使用默认背包（主背包）
        if (targetInventory == null)
            targetInventory = instance.myBag;

        // 清理现有物品槽
        for (int i = 0; i < instance.slotGrid.transform.childCount; i++)
        {
            Destroy(instance.slotGrid.transform.GetChild(i).gameObject);
        }
        instance.slots.Clear();

        // 切换背包时清空物品信息
        if (instance.currentInventory != targetInventory)
        {
            UpdateItemInfo(null);
        }

        // 重新创建物品槽
        for (int i = 0; i < targetInventory.items.Count; i++)
        {
            GameObject newSlot = Instantiate(instance.emptySlot);
            newSlot.transform.SetParent(instance.slotGrid.transform, false);
            newSlot.GetComponent<Slot>().SetUpSlot(targetInventory.items[i], i);
            instance.slots.Add(newSlot);
        }

        // 更新当前背包引用
        instance.currentInventory = targetInventory;
    }

    /// <summary>
    /// 切换到指定背包
    /// </summary>
    public void SwitchInventory(Inventory target)
    {
        RefreshItem(target);
    }

    /// <summary>
    /// 更新右侧物品信息面板
    /// </summary>
    /// <param name="item">要显示信息的物品，null则清空显示</param>
    public static void UpdateItemInfo(ItemData item)
    {
        if (item != null)
        {
            // 显示物品详细信息
            instance.itemInfo.text = item.itemDescription;
            instance.itemIcon.sprite = item.itemIcon;
            instance.itemName.text = item.itemName;
        }
        else
        {
            // 清空信息显示
            instance.itemInfo.text = " ";
            instance.itemIcon.sprite = null;
            instance.itemName.text = " ";
        }
    }
}