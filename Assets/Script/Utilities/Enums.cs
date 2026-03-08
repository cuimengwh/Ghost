using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleEffectType
{
    None,
}

public enum GameState
{
    GamePlay, Pause
}

public enum LightShift
{
    Morning, Night, 
}

public enum SoundName
{
    none, FootStepSoft, FoodStepHard,
}

//关于物品类型的枚举
public enum ItemType
{
    Seed, //种子
    Commodity, //商品 
    Furniture, //家具
    HoeTool, //锄头(犁地工具)
    ChopTool, //斧头(砍树工具)
    BreakTool, //镐头(挖矿工具)
    ReapTool, //镰刀(割草工具)
    WaterTool, //水壶(浇水工具)
    CollectTool, //菜篮(收集作物工具)
    ReapableScenery //杂草
}

//关于背包格子类型的枚举
public enum SlotType
{
    Bag, //背包格子
    Box, //物品箱格子
    Shop //商店格子
}

public enum InventoryLocation
{
    Player, //玩家身上的背包
    Box //箱子里的背包
}