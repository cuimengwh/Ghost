using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// 游戏数据编辑器窗口，用于编辑游戏中的物品、装备和掉落表等数据
/// 继承自EditorWindow，作为Unity编辑器的一个自定义窗口
/// </summary>
public class GameDataEditor : EditorWindow
{
    // 滚动视图的位置
    private Vector2 scrollPosition;
    // 当前选中的工具栏索引
    private int selectedToolbar = 0;
    // 工具栏选项标签
    private readonly string[] toolbarOptions = { "物品", "装备", "掉落表" };

    // 数据列表
    private List<ItemData> items;
    // private List<Equipment> equipment;
    // private List<DropTable> dropTables;

    // 列表和详细信息区域的滚动位置
    private Vector2 itemListScroll;
    private Vector2 detailScroll;
    // 当前选中的物品索引
    private int selectedItemIndex = -1;

    /// <summary>
    /// 显示编辑器窗口的菜单项方法
    /// </summary>
    [MenuItem("策划工具/游戏数据编辑器")]
    public static void ShowWindow()
    {
        // 获取或创建游戏数据编辑器窗口
        GetWindow<GameDataEditor>("游戏数据编辑器");
    }

    /// <summary>
    /// 当窗口启用时调用的Unity事件
    /// </summary>
    private void OnEnable()
    {
        // 加载游戏数据
        LoadData();
    }

    /// <summary>
    /// 加载所有游戏数据
    /// </summary>
    private void LoadData()
    {
        // 从指定路径加载所有物品数据
        items = LoadAll<ItemData>("Assets/Rubbsh");
        // equipment = LoadAll<Equipment>("Equipment");
        // dropTables = LoadAll<DropTable>("DropTables");
    }

    /// <summary>
    /// 从Resources文件夹加载指定类型的所有资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns>资源列表</returns>
    private List<T> LoadAll<T>(string path) where T : UnityEngine.Object
    {
        // 使用Resources.LoadAll加载资源并转换为列表
        return Resources.LoadAll<T>(path).ToList();
    }

    /// <summary>
    /// 绘制编辑器窗口的GUI内容
    /// </summary>
    private void OnGUI()
    {
        // 绘制工具栏
        DrawToolbar();
        EditorGUILayout.Space();
        // 绘制主内容区域
        DrawMainContent();
    }

    /// <summary>
    /// 绘制顶部工具栏
    /// </summary>
    private void DrawToolbar()
    {
        // 使用GUILayout.Toolbar创建工具栏
        selectedToolbar = GUILayout.Toolbar(selectedToolbar, toolbarOptions, GUILayout.Height(30));
    }

    /// <summary>
    /// 绘制主内容区域
    /// </summary>
    private void DrawMainContent()
    {
        // 使用水平布局
        EditorGUILayout.BeginHorizontal();
        {
            // 左侧列表
            DrawItemsList();

            // 右侧详细信息
            DrawDetailSection();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制物品列表
    /// </summary>
    private void DrawItemsList()
    {
        // 使用垂直布局，固定宽度
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        {
            // 列表标题
            GUILayout.Label("物品列表", EditorStyles.boldLabel);

            // 搜索栏
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("搜索:", GUILayout.Width(40));
            // 搜索输入框
            string searchTerm = GUILayout.TextField("", GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            // 列表滚动视图
            itemListScroll = EditorGUILayout.BeginScrollView(itemListScroll, GUILayout.ExpandHeight(true));
            {
                // 根据当前选中的工具栏选项绘制相应的列表
                switch (selectedToolbar)
                {
                    case 0: DrawListItems(items, searchTerm); break;
                        // case 1: DrawListItems(equipment, searchTerm); break;
                        // case 2: DrawListItems(dropTables, searchTerm); break;
                }
            }
            EditorGUILayout.EndScrollView();

            // 添加新物品按钮
            if (GUILayout.Button("添加新物品"))
            {
                AddNewItem();
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制指定类型的物品列表
    /// </summary>
    /// <typeparam name="T">物品类型</typeparam>
    /// <param name="items">物品列表</param>
    /// <param name="searchTerm">搜索关键词</param>
    private void DrawListItems<T>(List<T> items, string searchTerm) where T : UnityEngine.Object
    {
        // 遍历所有物品
        for (int i = 0; i < items.Count; i++)
        {
            // 获取物品名称
            string itemName = GetItemName(items[i]);

            // 根据搜索词过滤显示
            if (!string.IsNullOrEmpty(searchTerm) &&
                !itemName.ToLower().Contains(searchTerm.ToLower()))
                continue;

            // 绘制单个列表项
            EditorGUILayout.BeginHorizontal();
            {
                // 使用Toggle按钮实现单选效果
                if (GUILayout.Toggle(selectedItemIndex == i, itemName, "Button",
                    GUILayout.ExpandWidth(true)))
                {
                    // 如果选择了不同的项，更新选中索引
                    if (selectedItemIndex != i)
                    {
                        selectedItemIndex = i;
                        GUI.FocusControl(null); // 移除焦点，防止输入框保持焦点状态
                    }
                }

                // 删除按钮
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    DeleteItem(items[i], items);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 绘制详细信息区域
    /// </summary>
    private void DrawDetailSection()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        {
            // 区域标题
            GUILayout.Label("详细信息", EditorStyles.boldLabel);

            // 详细信息滚动视图
            detailScroll = EditorGUILayout.BeginScrollView(detailScroll,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                // 检查是否有选中的物品
                if (selectedItemIndex >= 0)
                {
                    // 根据当前选中的工具栏选项显示相应的详细信息编辑器
                    switch (selectedToolbar)
                    {
                        case 0:
                            if (selectedItemIndex < items.Count)
                                DrawItemDetails(items[selectedItemIndex]);
                            break;
                            // case 1:
                            //     if (selectedItemIndex < equipment.Count)
                            //         DrawEquipmentDetails(equipment[selectedItemIndex]);
                            //     break;
                            // case 2:
                            //     if (selectedItemIndex < dropTables.Count)
                            //         DrawDropTableDetails(dropTables[selectedItemIndex]);
                            //     break;
                    }
                }
                else
                {
                    // 没有选中任何物品时的提示
                    GUILayout.Label("选择一个物品进行编辑");
                }
            }
            EditorGUILayout.EndScrollView();
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 绘制物品详细信息编辑器
    /// </summary>
    /// <param name="item">要编辑的物品数据</param>
    private void DrawItemDetails(ItemData item)
    {
        // 开始检查GUI更改
        EditorGUI.BeginChangeCheck();

        // 使用盒子样式容器
        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            // 物品ID字段
            item.id = EditorGUILayout.TextField("物品ID", item.id);
            // 物品名称字段
            item.itemName = EditorGUILayout.TextField("物品名称", item.itemName);

            // 图标选择字段
            item.itemIcon = (Sprite)EditorGUILayout.ObjectField("图标", item.itemIcon, typeof(Sprite), false);
            // 最大堆叠数滑块
            item.maxStack = EditorGUILayout.IntSlider("最大堆叠数", item.maxStack, 1, 999);
            // 描述文本区域
            item.itemDescription = EditorGUILayout.TextField("描述", item.itemDescription,
                GUILayout.Height(60));
        }
        EditorGUILayout.EndVertical();

        // 检查是否有更改，如果有则标记资源为脏
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(item);
        }
    }

    // 装备和掉落表的详细信息编辑器方法已注释掉
    /*
    private void DrawEquipmentDetails(Equipment equipment)
    {
        // 先绘制基础物品属性
        DrawItemDetails(equipment);

        EditorGUILayout.Space();

        // 然后绘制装备特有属性
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical(GUI.skin.box);
        {
            equipment.slot = (EquipmentSlot)EditorGUILayout.EnumPopup("装备部位", equipment.slot);
            equipment.durability = EditorGUILayout.IntField("耐久度", equipment.durability);
            equipment.defense = EditorGUILayout.IntField("防御力", equipment.defense);
            equipment.attackBonus = EditorGUILayout.IntField("攻击加成", equipment.attackBonus);
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(equipment);
        }
    }
    */

    /// <summary>
    /// 添加新物品
    /// </summary>
    private void AddNewItem()
    {
        // 打开保存文件面板，让用户选择保存位置
        string path = EditorUtility.SaveFilePanelInProject("新建物品", "NewItem", "asset", "选择保存位置");
        if (!string.IsNullOrEmpty(path))
        {
            // 创建新的物品数据实例
            ItemData newItem = CreateInstance<ItemData>();
            // 生成唯一ID（取GUID的前8位）
            newItem.id = System.Guid.NewGuid().ToString().Substring(0, 8);
            // 设置默认名称
            newItem.itemName = "新物品";

            // 创建资源文件
            AssetDatabase.CreateAsset(newItem, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 添加到列表并选中新物品
            items.Add(newItem);
            selectedItemIndex = items.Count - 1;
        }
    }

    /// <summary>
    /// 删除指定物品
    /// </summary>
    /// <typeparam name="T">物品类型</typeparam>
    /// <param name="item">要删除的物品</param>
    /// <param name="list">物品列表</param>
    private void DeleteItem<T>(T item, List<T> list) where T : UnityEngine.Object
    {
        // 显示确认对话框
        if (EditorUtility.DisplayDialog("确认删除",
            $"确定要删除 {GetItemName(item)} 吗？", "是", "否"))
        {
            // 获取资源路径并删除资源
            string path = AssetDatabase.GetAssetPath(item);
            AssetDatabase.DeleteAsset(path);
            // 从列表中移除
            list.Remove(item);

            // 调整选中索引
            if (selectedItemIndex >= list.Count)
                selectedItemIndex = list.Count - 1;

            // 保存并刷新资源数据库
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 获取物品的显示名称
    /// </summary>
    /// <param name="item">物品对象</param>
    /// <returns>物品名称</returns>
    private string GetItemName(UnityEngine.Object item)
    {
        // 根据物品类型返回相应的名称字段
        if (item is ItemData baseItem) return baseItem.itemName;
        // if (item is Equipment equip) return equip.itemName;
        // if (item is DropTable dropTable) return dropTable.tableName;
        return item.name;
    }
}