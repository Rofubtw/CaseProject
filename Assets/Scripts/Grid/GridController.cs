using UnityEngine;

/// <summary>
/// Orchestrates grid operations. Bridges Model and View (MVC Controller layer).
/// </summary>
public class GridController : MonoBehaviour
{
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private GridView gridView;

    public GridModel Model { get; private set; }

    private void Awake()
    {
        Model = new GridModel(gridWidth, gridHeight, cellSize);
        gridView.Initialize(Model);
    }

    private void OnEnable()
    {
        EventBus<BuildingDestroyedEvent>.OnEvent += OnBuildingDestroyed;
    }

    private void OnDisable()
    {
        EventBus<BuildingDestroyedEvent>.OnEvent -= OnBuildingDestroyed;
    }

    private void OnBuildingDestroyed(BuildingDestroyedEvent evt)
    {
        Building b = evt.Building;
        if (b != null && b.Data != null)
            RemoveBuilding(b.GridPosition, b.Data.size);
    }

    /// <summary>
    /// Attempts to place a building on the grid. Returns true if placement succeeded.
    /// </summary>
    public bool TryPlaceBuilding(Vector2Int origin, Vector2Int size)
    {
        if (!Model.IsAreaAvailable(origin, size)) return false;

        Model.SetAreaOccupied(origin, size, true);
        gridView.ClearHighlights();
        return true;
    }

    /// <summary>
    /// Frees grid cells when a building is destroyed.
    /// </summary>
    public void RemoveBuilding(Vector2Int origin, Vector2Int size)
    {
        Model.SetAreaOccupied(origin, size, false);
    }

    public void ShowPlacementPreview(Vector2Int origin, Vector2Int size)
    {
        bool isValid = Model.IsAreaAvailable(origin, size);
        gridView.ShowPlacementPreview(origin, size, isValid);
    }

    public void ClearPreview()
    {
        gridView.ClearHighlights();
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float cs = Model.CellSize;
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / cs), Mathf.FloorToInt(worldPos.y / cs));
    }
}
