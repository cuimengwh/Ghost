using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int itemID;

    private SpriteRenderer sr;
    private BoxCollider2D coll;
    public ItemDetails itemDetails;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        if (itemID != 0)
        {
            Init(itemID);
        }
    }

    public void Init(int ID)
    {
        itemID = ID;

        //Inventory获取当前数据
        itemDetails = InventoryManager.Instance.GetItemDetails(itemID);

        if (itemDetails != null)
        {
            //将物体在地图上显示的图标设置正确
            sr.sprite = itemDetails.itemOnWorldSprite != null ? itemDetails.itemOnWorldSprite : itemDetails.itemIcon;

            //修改碰撞体尺寸，避免锚点不一致导致物体图标偏移碰撞器
            Vector2 newSize = new Vector2(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
            coll.size = newSize;
            coll.offset = new Vector2(0, sr.sprite.bounds.center.y);
        }
    }
}
