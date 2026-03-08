using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText; //名字文本
    [SerializeField] private TextMeshProUGUI typeText; //类型文本
    [SerializeField] private TextMeshProUGUI descriptionText; //详情文本
    [SerializeField] private Text valueText; //价格文本
    [SerializeField] private GameObject bottomText; //底部价格栏

    [Header("建造")]
    public GameObject resourcePanel;
    [SerializeField] private Image[] resourceItem;

    public void SetupTooltip(ItemDetails itemDetails, SlotType slotType)
    {
        nameText.text = itemDetails.itemName;
        typeText.text = GetChineseItemType(itemDetails.itemType);
        descriptionText.text = itemDetails.itemDescription;

        //当物品为种子、商品、家具类型的时候才显示价格
        if(itemDetails.itemType == ItemType.Seed || itemDetails.itemType == ItemType.Commodity || itemDetails.itemType == ItemType.Furniture)
        {
            bottomText.SetActive(true);
            var price = itemDetails.itemPrice;

            //当在背包中显示物品的信息的时候，显示实际出售的价格
            if(slotType == SlotType.Bag)
            {
                price = (int)(price * itemDetails.sellPercentage);
            }

            valueText.text = price.ToString();
        }
        else
        {
            bottomText.SetActive(false);
        }

        //调用这个方法可以避免文字从单行变成多行或者多行变成单行时的渲染延迟
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private string GetChineseItemType(ItemType itemType)
    {
        //新语法，比直接用switch简洁很多
        return itemType switch
        {
            ItemType.Seed => "种子",
            ItemType.Commodity => "商品",
            ItemType.Furniture => "家具",
            ItemType.BreakTool => "工具",
            ItemType.HoeTool => "工具",
            ItemType.ReapTool => "工具",
            ItemType.CollectTool => "工具",
            ItemType.WaterTool => "工具",
            ItemType.ChopTool => "工具",

            //下划线_代替其它的所有情况
            _ => "其它"
        };
    }
}
