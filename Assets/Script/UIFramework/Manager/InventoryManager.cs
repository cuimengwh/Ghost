using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    [Header("物品数据")]
    public ItemDataList_SO itemDataList_SO;
    public ItemDetails currentItemDetails;

    [Header("背包数据")]
    public InventoryBag_SO playerBagTemp;
    public InventoryBag_SO playerBag;
    private InventoryBag_SO currentBoxBag;

    [Header("交易数据")]
    public int playerMoney;

    private Dictionary<string, List<InventoryItem>> boxDataDict = new Dictionary<string, List<InventoryItem>>();
    public int BoxDataAmount => boxDataDict.Count;

    private void OnEnable()
    {
        EventHandler.DropItemEvent += OnDropItemEvent;
        EventHandler.BaseBagOpenEvent += OnBaseBagOpenEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
    }

    private void OnDisable()
    {
        EventHandler.DropItemEvent -= OnDropItemEvent;
        EventHandler.BaseBagOpenEvent -= OnBaseBagOpenEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
    }

    private void OnItemSelectedEvent(ItemDetails details, bool isSelected)
    {
        if(isSelected)
            currentItemDetails = details;
        else
            currentItemDetails = null;
    }

    /// <summary>
    /// 在游戏的一开始，将背包的数据加载进去
    /// </summary>
    private void Start()
    {
        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
    }

    private void OnStartNewGameEvent(int obj)
    {
        playerBag = Instantiate(playerBagTemp);
        playerMoney = Settings.playerStartMoney;
        boxDataDict.Clear();
        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
    }

    private void OnBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        currentBoxBag = bag_SO;
    }

    private void OnDropItemEvent(int ID, Vector3 pos, ItemType itemType)
    {
        RemoveItem(ID, 1);
    }

    /// <summary>
    /// 根据ID返回物品的全部信息
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public ItemDetails GetItemDetails(int ID)
    {
        //根据ID找到物品的全部信息
        return itemDataList_SO.itemDetailsList.Find(i => i.itemID == ID);
    }

    /// <summary>
    /// 拾取物品到背包
    /// </summary>
    /// <param name="item">物品</param>
    /// <param name="toDestroy">是否要销毁这个物品</param>
    public void AddItem(Item item, bool toDestroy)
    {
        

        
    }

    /// <summary>
    /// 检查背包是否有空位
    /// </summary>
    /// <returns></returns>
    private bool CheckBagCapacity()
    {
        for (int i = 0; i < playerBag.itemList.Count; i++)
        {
            //检测背包是否满了，没满才可以添加
            if (playerBag.itemList[i].itemID == 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检查背包是否已经有这个物体了
    /// </summary>
    /// <param name="ID"></param>
    /// <returns>有则返回ID，无则返回-1</returns>
    private int GetItemIndexInBag(int ID)
    {
        for (int i = 0; i < playerBag.itemList.Count; i++)
        {
            if (playerBag.itemList[i].itemID == ID)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 添加物品到背包
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="index"></param>
    /// <param name="amount"></param>
    public void AddItem(int ID, int amount)
    {
        //拾取物品到背包
        //需要考虑两个因素：1、背包是否已经有这个物品了 2、背包是否已经满了

        //1、背包是否已经有这个物品了
        var index = GetItemIndexInBag(ID);

        //2、背包是否已经满了
        if (index == -1 && CheckBagCapacity()) //背包里面没有这个物品，同时背包有空位
        {
            var item = new InventoryItem { itemID = ID, itemAmount = amount };
            for (int i = 0; i < playerBag.itemList.Count; i++)
            {
                if (playerBag.itemList[i].itemID == 0)
                {
                    playerBag.itemList[i] = item;
                    break;
                }
            }
        }
        else if (index != -1) //要去除背包没有这个物品，同时背包没有空位这种情况
        {
            int currentAmount = playerBag.itemList[index].itemAmount + amount;
            var item = new InventoryItem { itemID = ID, itemAmount = currentAmount };
            playerBag.itemList[index] = item;
        }
        else
        {
            Debug.Log("添加物品失败，背包已满或者没有这个物品");
        }

        //更新UI
        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
    }

    /// <summary>
    /// PlayerBag范围内交换物品
    /// </summary>
    /// <param name="fromIndex">起始拖拽的格子序号</param>
    /// <param name="targetIndex">目标格子序号</param>
    public void SwapItem(int fromIndex, int targetIndex)
    {
        InventoryItem currentItem = playerBag.itemList[fromIndex];
        InventoryItem targetItem = playerBag.itemList[targetIndex];

        //两种情况，一种是两个格子都有东西，另外一种是目标格子没有东西
        if (targetItem.itemID != 0)
        {
            playerBag.itemList[fromIndex] = targetItem;
            playerBag.itemList[targetIndex] = currentItem;
        }
        else
        {
            playerBag.itemList[targetIndex] = currentItem;
            playerBag.itemList[fromIndex] = new InventoryItem();
        }

        //交换完成更新背包UI
        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
    }

    /// <summary>
    /// 跨背包交换数据
    /// </summary>
    /// <param name="locationFrom"></param>
    /// <param name="fromIndex"></param>
    /// <param name="locationTarget"></param>
    /// <param name="targetIndex"></param>
    public void SwapItem(InventoryLocation locationFrom, int fromIndex, InventoryLocation locationTarget, int targetIndex)
    {
        var currentList = GetItemList(locationFrom);
        var targetList = GetItemList(locationTarget);

        InventoryItem currentItem = currentList[fromIndex];

        //确保交换到的格子存在
        if (targetIndex < targetList.Count)
        {
            InventoryItem targetItem = targetList[targetIndex];

            if (targetItem.itemID != 0 && currentItem.itemID != targetItem.itemID) //有不相同的两个物品
            {
                currentList[fromIndex] = targetItem;
                targetList[targetIndex] = currentItem;
            }
            else if (currentItem.itemID == targetItem.itemID) //相同两个物品
            {
                targetItem.itemAmount += currentItem.itemAmount;
                targetList[targetIndex] = targetItem; //更新
                currentList[fromIndex] = new InventoryItem();
            }
            else //目标空格子
            {
                targetList[targetIndex] = currentItem;
                currentList[fromIndex] = new InventoryItem();
            }
            EventHandler.CallUpdateInventoryUI(locationFrom, currentList);
            EventHandler.CallUpdateInventoryUI(locationTarget, targetList);
        }
    }

    /// <summary>
    /// 移除指定数量的背包物品
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="removeAmount"></param>
    private void RemoveItem(int ID, int removeAmount)
    {
        var index = GetItemIndexInBag(ID);

        //移除物品分两种情况，一种移除数量大于物品数量，一种移除物品等于物品数量
        if (playerBag.itemList[index].itemAmount > removeAmount)
        {
            var amount = playerBag.itemList[index].itemAmount - removeAmount;
            var item = new InventoryItem { itemID = ID, itemAmount = amount };
            playerBag.itemList[index] = item;
        }
        else if (playerBag.itemList[index].itemAmount == removeAmount)
        {
            var item = new InventoryItem();
            playerBag.itemList[index] = item;
        }

        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
    }

    /// <summary>
    /// 交易物品
    /// </summary>
    /// <param name="itemDetails">物品信息</param>
    /// <param name="amount">交易数量</param>
    /// <param name="isSellTrade">是否卖东西</param>
    public void TradeItem(ItemDetails itemDetails, int amount, bool isSellTrade)
    {
        int cost = itemDetails.itemPrice * amount;
        //获得物品背包位置
        int index = GetItemIndexInBag(itemDetails.itemID);

        if (isSellTrade) //卖
        {
            if (playerBag.itemList[index].itemAmount >= amount)
            {
                RemoveItem(itemDetails.itemID, amount);
                cost = (int)(cost * itemDetails.sellPercentage);
                playerMoney += cost;
            }
        }
        else if (playerMoney - cost >= 0) //买
        {
            if (CheckBagCapacity()) //确认背包还有空间
            {
                AddItem(itemDetails.itemID, amount);
            }
            playerMoney -= cost;
        }
        //刷新UI  
        EventHandler.CallUpdateInventoryUI(InventoryLocation.Player, playerBag.itemList);
    }

    /// <summary>
    /// 根据位置返回背包数据列表
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private List<InventoryItem> GetItemList(InventoryLocation location)
    {
        return location switch
        {
            InventoryLocation.Player => playerBag.itemList,
            InventoryLocation.Box => currentBoxBag.itemList,
            _ => null
        };
    }

    /// <summary>
    /// 查找箱子数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public List<InventoryItem> GetBoxDataList(string key)
    {
        if (boxDataDict.ContainsKey(key))
            return boxDataDict[key];
        return null;
    }

    /// <summary>
    /// 加入箱子数据字典
    /// </summary>
    /// <param name="box"></param>
    //public void AddBoxDataDict(Box box)
    //{
    //    var key = box.name + box.index;
    //    if (!boxDataDict.ContainsKey(key))
    //        boxDataDict.Add(key, box.boxBagData.itemList);
    //    Debug.Log(key);
    //}
}