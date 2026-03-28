using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(Vector3 pos)
    {
        this.x = pos.x;
        this.y = pos.y;
        this.z = pos.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int((int)x, (int)y);
    }
}

[System.Serializable]
public class DialogueEntry
{
    public string id;           // 对话ID
    public List<string> text;         // 对话内容

    public DialogueEntry(List<string> data)
    {
        text = new List<string>();

        //data的第一个元素是id，后续元素是text内容
        if (data.Count >= 1) id = data[0];
        for (int i = 1; i < data.Count; i++)
        {
            text.Add(data[i]);
        }
    }
}

[System.Serializable]
public struct InventoryItem
{
    //这个结构是背包的基本信息，使用结构是为了解决一些空引用的问题
    public int itemID; //物品ID

    public int itemAmount; //物品在背包中的数量
}

[System.Serializable]
public class ItemDetails
{
    //这个类是物品的基本信息
    #region  物品基本信息
    public int itemID; //物品id   
    public string itemName; //物品名称
    public ItemType itemType;
    public Sprite itemIcon; //物品的图标
    public Sprite itemOnWorldSprite; //物品在地图上显示的样子
    public string itemDescription;
    public int itemUseRadius; //物品使用的范围
    #endregion

    #region 人物能对物品施加的操作
    public bool canPickedUp; //物品能否被拾取
    public bool canDropped; //物品能否被放下
    public bool canCarried; //物品能否被举着
    #endregion

    #region 出售物品的设置
    public int itemPrice; //物品的价值
    [Range(0, 1)]
    public float sellPercentage; //物品售卖时所打的折扣
    #endregion
}


