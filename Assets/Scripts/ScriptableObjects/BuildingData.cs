using UnityEngine;

/// <summary>
/// Data definition for a building type.
/// Defines size, health, and what units it can produce (if any).
/// </summary>
[CreateAssetMenu(fileName = "NewBuilding", menuName = "Game/Building Data")]
public class BuildingData : EntityData
{
    [Header("Building")]
    public Vector2Int size = Vector2Int.one;

    [Header("Production")]
    [Tooltip("Units this building can produce. Leave empty if non-producing.")]
    public UnitData[] producibleUnits;

    public bool CanProduceUnits => producibleUnits != null && producibleUnits.Length > 0;
}
