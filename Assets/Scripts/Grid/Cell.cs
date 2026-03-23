using UnityEngine;

/// <summary>
/// Represents a single cell in the grid. Pure data class (MVC Model).
/// </summary>
public class Cell
{
    public Vector2Int Position { get; }
    public bool IsOccupied { get; private set; }
    public bool HasUnit { get; private set; }
    public bool IsWalkable => !IsOccupied && !HasUnit;

    public Cell(Vector2Int position)
    {
        Position = position;
        IsOccupied = false;
        HasUnit = false;
    }

    public void SetOccupied(bool occupied)
    {
        IsOccupied = occupied;
    }

    public void SetHasUnit(bool hasUnit)
    {
        HasUnit = hasUnit;
    }
}
