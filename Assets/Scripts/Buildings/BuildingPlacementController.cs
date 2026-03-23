using UnityEngine;

/// <summary>
/// Handles building placement mode. Activated when a building is selected from Production Menu.
/// Shows grid preview (green/red) and places building on left click.
/// </summary>
public class BuildingPlacementController : MonoBehaviour
{
    [Inject] private GridController gridController;
    [Inject] private BuildingFactory buildingFactory;

    private Camera mainCamera;
    private BuildingData currentBuildingData;
    private bool isPlacing;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EventBus<BuildingSelectedForPlacementEvent>.OnEvent += OnBuildingSelectedForPlacement;
    }

    private void OnDisable()
    {
        EventBus<BuildingSelectedForPlacementEvent>.OnEvent -= OnBuildingSelectedForPlacement;
    }

    private void Update()
    {
        if (!isPlacing) return;

        Vector2Int gridPos = GetMouseGridPosition();
        gridController.ShowPlacementPreview(gridPos, currentBuildingData.size);

        // Left click: place building
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            TryPlace(gridPos);
        }

        // Right click or Escape: cancel
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    private void OnBuildingSelectedForPlacement(BuildingSelectedForPlacementEvent evt)
    {
        currentBuildingData = evt.Data;
        isPlacing = true;
    }

    private void TryPlace(Vector2Int gridPos)
    {
        if (!gridController.TryPlaceBuilding(gridPos, currentBuildingData.size)) return;

        Building building = buildingFactory.Create(currentBuildingData, gridPos, gridController.Model.CellSize);
        building.SetTeam(true);
        EventBus<BuildingPlacedEvent>.Raise(new BuildingPlacedEvent(building));
        CancelPlacement();
    }

    private void CancelPlacement()
    {
        isPlacing = false;
        currentBuildingData = null;
        gridController.ClearPreview();
    }

    private Vector2Int GetMouseGridPosition()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return gridController.WorldToGrid(worldPos);
    }

    /// <summary>
    /// Prevents placing buildings when clicking on UI elements.
    /// </summary>
    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
