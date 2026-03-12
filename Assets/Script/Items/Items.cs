using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : ScriptableObject
{
    [SerializeField] private int id;   //物品id（唯一标识）
    [SerializeField] private string itemname; //物品名称
    [SerializeField] private Sprite icon; //物品图标

    public int Id { get => id; }
    public string Itemname { get => itemname; }
    public Sprite Icon { get => icon;}
}
