using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unit movement commands. Right-click on a walkable cell
/// to move the selected unit via A* pathfinding.
/// </summary>
public class UnitMovementController : MonoBehaviour
{
    [Inject] private GridController gridController;

    private Camera mainCamera;
    private Unit selectedUnit;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        EventBus<EntitySelectedEvent>.OnEvent += OnEntitySelected;
        EventBus<SelectionClearedEvent>.OnEvent += OnSelectionCleared;
    }

    private void OnDisable()
    {
        EventBus<EntitySelectedEvent>.OnEvent -= OnEntitySelected;
        EventBus<SelectionClearedEvent>.OnEvent -= OnSelectionCleared;
    }

    private void Update()
    {
        if (selectedUnit == null) return;
        if (!Input.GetMouseButtonDown(1)) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        GridModel grid = gridController.Model;
        Vector2Int clickedGrid = gridController.WorldToGrid(worldPos);

        if (!grid.IsInBounds(clickedGrid.x, clickedGrid.y)) return;

        // Check clicked grid cell center for enemy entity (easier than pixel-precise clicking)
        float cs = grid.CellSize;
        Vector2 cellCenter = new Vector2((clickedGrid.x + 0.5f) * cs, (clickedGrid.y + 0.5f) * cs);
        Collider2D overlap = Physics2D.OverlapPoint(cellCenter);
        if (overlap != null)
        {
            DamageableEntity entity = overlap.GetComponentInParent<DamageableEntity>();
            if (entity != null && IsEnemy(entity))
            {
                MoveToAndAttack(entity);
                return;
            }
        }

        // Also check the exact click position for larger buildings
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            DamageableEntity entity = hit.collider.GetComponentInParent<DamageableEntity>();
            if (entity != null && IsEnemy(entity))
            {
                MoveToAndAttack(entity);
                return;
            }
        }

        // Regular movement — clear any existing attack target
        selectedUnit.ClearAttackTarget();

        Vector2Int targetGrid = gridController.WorldToGrid(worldPos);

        if (!grid.IsInBounds(targetGrid.x, targetGrid.y)) return;

        Cell targetCell = grid.GetCell(targetGrid);
        if (targetCell == null || !targetCell.IsWalkable) return;

        Vector2Int unitGrid = gridController.WorldToGrid(selectedUnit.transform.position);
        List<Vector2Int> path = Pathfinder.FindPath(grid, unitGrid, targetGrid);

        if (path != null && path.Count > 1)
        {
            selectedUnit.SetPath(path, grid.CellSize);
        }
    }

    private bool IsEnemy(DamageableEntity entity)
    {
        if (entity is Unit unit) return !unit.IsPlayerUnit;
        if (entity is Building building) return !building.IsPlayerBuilding;
        return false;
    }

    private void MoveToAndAttack(DamageableEntity target)
    {
        GridModel grid = gridController.Model;
        Vector2Int unitGrid = gridController.WorldToGrid(selectedUnit.transform.position);

        // Already adjacent — attack immediately without moving
        if (IsOrthogonallyAdjacent(unitGrid, target))
        {
            selectedUnit.StopMovement();
            selectedUnit.SetAttackTarget(target);
            return;
        }

        Vector2Int? nearestCell = null;

        if (target is Building building)
        {
            nearestCell = Pathfinder.FindNearestAdjacentCell(
                grid, building.GridPosition, building.Data.size, unitGrid);
        }
        else
        {
            Vector2Int targetGrid = gridController.WorldToGrid(target.transform.position);
            nearestCell = Pathfinder.FindNearestAdjacentCell(
                grid, targetGrid, Vector2Int.one, unitGrid);
        }

        if (nearestCell == null) return;

        List<Vector2Int> path = Pathfinder.FindPath(grid, unitGrid, nearestCell.Value);

        if (path != null && path.Count > 0)
        {
            selectedUnit.SetPath(path, grid.CellSize);
            selectedUnit.SetAttackTarget(target);
        }
    }

    private bool IsOrthogonallyAdjacent(Vector2Int unitPos, DamageableEntity target)
    {
        if (target is Building building)
        {
            for (int x = building.GridPosition.x; x < building.GridPosition.x + building.Data.size.x; x++)
            {
                for (int y = building.GridPosition.y; y < building.GridPosition.y + building.Data.size.y; y++)
                {
                    int dist = Mathf.Abs(unitPos.x - x) + Mathf.Abs(unitPos.y - y);
                    if (dist <= 1) return true;
                }
            }
            return false;
        }

        Vector2Int targetGrid = gridController.WorldToGrid(target.transform.position);
        return Mathf.Abs(unitPos.x - targetGrid.x) + Mathf.Abs(unitPos.y - targetGrid.y) <= 1;
    }

    private void OnEntitySelected(EntitySelectedEvent evt)
    {
        if (evt.Entity is Unit unit && unit.IsPlayerUnit)
            selectedUnit = unit;
        else
            selectedUnit = null;
    }

    private void OnSelectionCleared(SelectionClearedEvent evt)
    {
        selectedUnit = null;
    }
}
