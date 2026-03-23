using UnityEngine;

/// <summary>
/// Creates building instances from BuildingData definitions (Factory Pattern).
/// </summary>
public class BuildingFactory : MonoBehaviour
{
    [SerializeField] private GameObject buildingPrefab;

    /// <summary>
    /// Instantiates a new building at the given grid position.
    /// </summary>
    public Building Create(BuildingData data, Vector2Int gridPosition, float cellSize)
    {
        Vector3 worldPos = new Vector3(
            (gridPosition.x + data.size.x * 0.5f) * cellSize,
            (gridPosition.y + data.size.y * 0.5f) * cellSize,
            0f
        );

        GameObject go = Instantiate(buildingPrefab, worldPos, Quaternion.identity);
        Building building = go.GetComponent<Building>();
        building.Setup(data, gridPosition, cellSize);
        return building;
    }
}
