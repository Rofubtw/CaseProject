using UnityEngine;

/// <summary>
/// Core grid data model. Holds all cells and provides query/modify operations.
/// No MonoBehaviour dependency — pure C# (MVC Model layer).
/// </summary>
public class GridModel
{
    public int Width { get; }
    public int Height { get; }
    public float CellSize { get; }

    private readonly Cell[,] cells;

    public GridModel(int width, int height, float cellSize = 1f)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
        cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                cells[x, y] = new Cell(new Vector2Int(x, y));
    }

    public Cell GetCell(int x, int y)
    {
        if (!IsInBounds(x, y)) return null;
        return cells[x, y];
    }

    public Cell GetCell(Vector2Int pos) => GetCell(pos.x, pos.y);

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Checks if a rectangular area starting at origin with given size is completely free.
    /// Used before placing a building.
    /// </summary>
    public bool IsAreaAvailable(Vector2Int origin, Vector2Int size)
    {
        for (int x = origin.x; x < origin.x + size.x; x++)
        {
            for (int y = origin.y; y < origin.y + size.y; y++)
            {
                if (!IsInBounds(x, y)) return false;
                if (cells[x, y].IsOccupied) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Marks a rectangular area as occupied or free.
    /// </summary>
    public void SetAreaOccupied(Vector2Int origin, Vector2Int size, bool occupied)
    {
        for (int x = origin.x; x < origin.x + size.x; x++)
            for (int y = origin.y; y < origin.y + size.y; y++)
                cells[x, y].SetOccupied(occupied);
    }
}
