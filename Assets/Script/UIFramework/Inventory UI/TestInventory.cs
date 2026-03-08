using UnityEngine;

public class TestInventory : MonoBehaviour
{
    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
    }

    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
    }

    private void OnItemSelectedEvent(ItemDetails details, bool arg2)
    {
        Debug.Log("选中的物品类型为" + details.itemType);
    }
}
