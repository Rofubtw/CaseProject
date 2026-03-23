using UnityEngine;

/// <summary>
/// Populates the Production Menu with an infinite looping scroll of BuildingData items.
/// </summary>
public class ProductionMenuView : MonoBehaviour
{
    [SerializeField] private InfiniteScrollView infiniteScroll;
    [SerializeField] private BuildingData[] availableBuildings;

    private void Start()
    {
        infiniteScroll.Setup(availableBuildings, BindItem);
    }

    private void BindItem(RectTransform itemTransform, int dataIndex)
    {
        ProductionItemUI item = itemTransform.GetComponent<ProductionItemUI>();
        item.Setup(availableBuildings[dataIndex], OnBuildingSelected);
    }

    private void OnBuildingSelected(BuildingData data)
    {
        EventBus<BuildingSelectedForPlacementEvent>.Raise(new BuildingSelectedForPlacementEvent(data));
    }
}
