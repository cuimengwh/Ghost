using UnityEngine;

public class LandInteraction : MonoBehaviour
{
    PlayerController playerController;
    Land selectedLand = null;

    [SerializeField] private float raycastDistance = 2f; //射线长度
    [SerializeField] private float selectionDelay = 0.1f; // 延迟时间
    private float lastValidLandTime; // 最后一次有效土地的时间
    private Land lastValidLand; // 最后一次有效的土地
    [SerializeField] private SeedDataList_SO SeedDataList_SO;

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
        {
            Land currentLand = hit.collider.GetComponent<Land>();

            if (currentLand != null)
            {
                // 记录有效的土地和时间
                lastValidLand = currentLand;
                lastValidLandTime = Time.time;
                SelectLand(currentLand);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    OnInteractableHit(hit);
                }
            }
            else
            {
                // 如果射线命中的不是土地，不要立即取消选择
                CheckAndDeselectLand();
            }
        }
        else
        {
            // 射线没有命中任何物体，检查是否需要取消选择
            CheckAndDeselectLand();
        }
    }

    // 处理与可交互物体的交互
    public void OnInteractableHit(RaycastHit hit)
    {
        Collider sth = hit.collider; // 获取碰撞体
        // 检查是否为土地
        if (sth.CompareTag("Land"))
        {
            Land land = sth.GetComponent<Land>(); // 获取土地组件
            //空值判断
            if (land == null)
            {
                Debug.Log("当前选择土地为空");
                return;
            }
            //收获作物
            if (land.plant != null && land.plant.CurrentStatus == PlantStatus.matureStage)
            {
                Harvest(land);
                return;
            }
            switch(InventoryManager.Instance.currentItemDetails.itemType)
            {
                case ItemType.HoeTool:
                    {
                        if(selectedLand.landStatus == Land.LandStatus.dirt)
                        {
                            land.ChangStatusToFarmland();
                        }
                        break;
                    }
                case ItemType.WaterTool:
                    {
                        if (selectedLand.landStatus == Land.LandStatus.farmland)
                        {
                            land.ChangStatusToWatered();
                        }
                        break;
                    }
                case ItemType.ReapTool:
                    {
                        if(selectedLand.landStatus == Land.LandStatus.weeded)
                        {
                            land.ChangStatusToWeeded();
                        }
                        break;
                    }
                case ItemType.Seed:
                    {
                        if (selectedLand.landStatus == Land.LandStatus.farmland || selectedLand.landStatus == Land.LandStatus.watered)
                        {
                            if(InventoryManager.Instance.currentItemDetails.itemType == ItemType.Seed)
                            {
                                Plant(selectedLand, SeedDataList_SO.Find(InventoryManager.Instance.currentItemDetails.itemID));
                            }
                        }
                            break;
                    }
            }
        }
    }

    void CheckAndDeselectLand()
    {
        // 如果超过一定时间没有检测到土地，才取消选择
        if (Time.time - lastValidLandTime > 0.2f) // 0.2秒的缓冲时间
        {
            SelectLand(null);
        }
    }

    void SelectLand(Land land)
    {
        if (land == selectedLand) return;

        // 取消旧的选择
        if (selectedLand != null)
        {
            selectedLand.Select(false);
        }

        // 设置新的选择
        selectedLand = land;
        if (selectedLand != null)
        {
            selectedLand.Select(true);
        }
    }

    /// <summary>
    /// 种植作物
    /// </summary>
    public void Plant(Land land,Seed seed)
    {
        //播放播种动作
        land.PlantOnLand(seed);
    }
    /// <summary>
    /// 收获作物
    /// </summary>
    public void Harvest(Land land)
    {
        //播放收获动作
        land.HarvestFormLand();
    }
}