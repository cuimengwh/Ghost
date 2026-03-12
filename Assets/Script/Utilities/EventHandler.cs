using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class EventHandler
{
    #region 对话事件
    /// <summary>
    /// 获取字幕数据
    /// </summary>
    public static event Action<string, string> GetDialogueDataEvent;
    public static void CallGetDialogueDataEvent(string sceneName, string dialogueID)
    {
        GetDialogueDataEvent?.Invoke(sceneName, dialogueID);
    }

    /// <summary>
    /// 播放字幕UI
    /// </summary>
    public static event Action<float, float, string[]> DisplayDialogueUIEvent;
    public static void CallDisplayDialogueUIEvent(float textSpeed, float sentenceSpacing, string[] dialogueText)
    {
        DisplayDialogueUIEvent?.Invoke(textSpeed, sentenceSpacing, dialogueText);
    }
    #endregion

    #region 异步场景加载事件
    public static event Action<string, Vector3> TransitionEvent;
    public static void CallTransitionEvent(string sceneName, Vector3 pos)
    {
        TransitionEvent?.Invoke(sceneName, pos);
    }

    public static event Action BeforeSceneUnloadEvent;
    public static void CallBeforeSceneUnloadEvent()
    {
        BeforeSceneUnloadEvent?.Invoke();
    }

    public static event Action AfterSceneLoadedEvent;
    public static void CallAfterSceneLoadedEvent()
    {
        AfterSceneLoadedEvent?.Invoke();
    }

    public static event Action<Vector3> MoveToPosition;
    public static void CallMoveToPosition(Vector3 targetPosition)
    {
        MoveToPosition?.Invoke(targetPosition);
    }

    #endregion

    #region 物品事件
    //更新背包及物品栏UI
    public static event Action<InventoryLocation, List<InventoryItem>> UpdateInventoryUI;
    public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list)
    {
        UpdateInventoryUI?.Invoke(location, list);
    }

    //实例化物品到场景中
    public static event Action<int, Vector3> InstantiateItemScene;
    public static void CallInstantiateItemScene(int ID, Vector3 pos)
    {
        InstantiateItemScene?.Invoke(ID, pos);
    }

    //扔下物品事件
    public static event Action<int, Vector3, ItemType> DropItemEvent;
    public static void CallDropItemEvent(int ID, Vector3 pos, ItemType itemType)
    {
        DropItemEvent?.Invoke(ID, pos, itemType);
    }

    //物品被选中事件
    public static event Action<ItemDetails, bool> ItemSelectedEvent;
    public static void CallItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        ItemSelectedEvent?.Invoke(itemDetails, isSelected);
    }

    //打开关闭背包UI事件
    public static event Action<SlotType, InventoryBag_SO> BaseBagOpenEvent;
    public static void CallBaseBagOpenEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBagOpenEvent?.Invoke(slotType, bag_SO);
    }

    public static event Action<SlotType, InventoryBag_SO> BaseBagCloseEvent;
    public static void CallBaseBagCloseEvent(SlotType slotType, InventoryBag_SO bag_SO)
    {
        BaseBagCloseEvent?.Invoke(slotType, bag_SO);
    }
    #endregion

    public static event Action<bool> SetCusorVisibleEvent;
    public static void CallSetCusorVisibleEvent(bool isVisible)
    {
        SetCusorVisibleEvent?.Invoke(isVisible);
    }

    public static event Action<ParticleEffectType, Vector3> ParticleEffectEvent;
    public static void CallParticleEffectEvent(ParticleEffectType effectType, Vector3 pos)
    {
        ParticleEffectEvent?.Invoke(effectType, pos);
    }

    public static event Action<GameState> UpdateGameStateEvent;
    public static void CallUpdateGameStateEvent(GameState gameState)
    {
        UpdateGameStateEvent?.Invoke(gameState);
    }

    public static event Action<int> StartNewGameEvent;
    public static void CallStartNewGameEvent(int index)
    {
        StartNewGameEvent?.Invoke(index);
    }

    public static event Action EndGameEvent;
    public static void CallEndGameEvent()
    {
        EndGameEvent?.Invoke();
    }

}
