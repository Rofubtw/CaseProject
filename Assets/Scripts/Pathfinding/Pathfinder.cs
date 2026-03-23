using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* pathfinding on GridModel. Pure C# — no MonoBehaviour dependency.
/// </summary>
public static class Pathfinder
{
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private struct Node : IComparable<Node>
    {
        public int fScore;
        public int insertOrder;
        public Vector2Int pos;

        public int CompareTo(Node other)
        {
            int cmp = fScore.CompareTo(other.fScore);
            if (cmp != 0) return cmp;
            return insertOrder.CompareTo(other.insertOrder);
        }
    }

    /// <summary>
    /// Finds the shortest walkable path from start to end on the grid.
    /// Returns null if no path exists.
    /// </summary>
    public static List<Vector2Int> FindPath(GridModel grid, Vector2Int start, Vector2Int end)
    {
        if (!grid.IsInBounds(start.x, start.y) || !grid.IsInBounds(end.x, end.y))
            return null;

        Cell endCell = grid.GetCell(end);
        if (endCell == null || !endCell.IsWalkable)
            return null;

        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, int> gScore = new Dictionary<Vector2Int, int>();

        SortedSet<Node> open = new SortedSet<Node>();
        int insertCounter = 0;

        gScore[start] = 0;
        int startH = Heuristic(start, end);
        open.Add(new Node { fScore = startH, insertOrder = insertCounter++, pos = start });

        while (open.Count > 0)
        {
            Node current = open.Min;
            open.Remove(current);
            Vector2Int currentPos = current.pos;

            if (currentPos == end)
                return ReconstructPath(cameFrom, end);

            closed.Add(currentPos);

            foreach (var dir in Directions)
            {
                Vector2Int neighbor = currentPos + dir;

                if (!grid.IsInBounds(neighbor.x, neighbor.y)) continue;
                if (closed.Contains(neighbor)) continue;

                Cell neighborCell = grid.GetCell(neighbor);
                if (neighborCell == null || !neighborCell.IsWalkable) continue;

                int tentativeG = gScore[currentPos] + 1;

                if (gScore.TryGetValue(neighbor, out int existingG) && tentativeG >= existingG)
                    continue;

                cameFrom[neighbor] = currentPos;
                gScore[neighbor] = tentativeG;

                int fScore = tentativeG + Heuristic(neighbor, end);
                open.Add(new Node { fScore = fScore, insertOrder = insertCounter++, pos = neighbor });
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the nearest walkable cell adjacent to a building.
    /// Used for unit movement towards a building target.
    /// </summary>
    public static Vector2Int? FindNearestAdjacentCell(GridModel grid, Vector2Int buildingOrigin, Vector2Int buildingSize, Vector2Int from)
    {
        Vector2Int? best = null;
        int bestDist = int.MaxValue;

        for (int x = buildingOrigin.x - 1; x <= buildingOrigin.x + buildingSize.x; x++)
        {
            for (int y = buildingOrigin.y - 1; y <= buildingOrigin.y + buildingSize.y; y++)
            {
                // Skip cells inside the building
                if (x >= buildingOrigin.x && x < buildingOrigin.x + buildingSize.x &&
                    y >= buildingOrigin.y && y < buildingOrigin.y + buildingSize.y)
                    continue;

                // Skip diagonal (corner) cells — only orthogonal adjacency
                bool xOutside = x < buildingOrigin.x || x >= buildingOrigin.x + buildingSize.x;
                bool yOutside = y < buildingOrigin.y || y >= buildingOrigin.y + buildingSize.y;
                if (xOutside && yOutside) continue;

                if (!grid.IsInBounds(x, y)) continue;

                Cell cell = grid.GetCell(x, y);
                if (cell == null || !cell.IsWalkable) continue;

                int dist = Heuristic(new Vector2Int(x, y), from);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = new Vector2Int(x, y);
                }
            }
        }

        return best;
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> path = new List<Vector2Int> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}
