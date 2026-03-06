using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Collection", menuName = "Item/New Collection")]
public class Collection : ItemData
{
    [Header("收集品属性")]
    public bool isQuestItem; // 是否为任务物品
    public bool isStoryItem; // 是否为剧情物品
    
    // 其他与收集品相关的属性和方法可以在这里添加
}
