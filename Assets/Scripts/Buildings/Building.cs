using UnityEngine;

/// <summary>
/// Represents a placed building on the game board.
/// </summary>
public class Building : DamageableEntity
{
    public BuildingData Data { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public bool IsPlayerBuilding { get; private set; } = true;

    private SpriteRenderer spriteRenderer;
    private static readonly int TintColorId = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock propertyBlock;

    public void Setup(BuildingData data, Vector2Int gridPosition, float cellSize)
    {
        Data = data;
        GridPosition = gridPosition;
        MaxHealth = data.maxHealth;
        CurrentHealth = data.maxHealth;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = data.sprite;
            FitSpriteToGridSize(data.size, cellSize);
        }

        FitCollider(data.size, cellSize);
        gameObject.name = data.entityName;
    }

    /// <summary>
    /// Sets the team color tint via MaterialPropertyBlock.
    /// </summary>
    public void SetTeam(bool isPlayer)
    {
        IsPlayerBuilding = isPlayer;

        if (spriteRenderer == null) return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        spriteRenderer.GetPropertyBlock(propertyBlock);
        Color tint = isPlayer ? new Color(0.6f, 0.7f, 1f) : new Color(1f, 0.6f, 0.6f);
        propertyBlock.SetColor(TintColorId, tint);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void FitCollider(Vector2Int size, float cellSize)
    {
        BoxCollider2D col = GetComponentInChildren<BoxCollider2D>();
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();

        // Collider is on the child whose scale was changed by FitSpriteToGridSize.
        // Set size in local space (1,1) so it matches the already-scaled sprite.
        Vector3 ls = col.transform.localScale;
        col.size = new Vector2(
            Mathf.Abs(ls.x) > 0.001f ? (size.x * cellSize) / ls.x : size.x * cellSize,
            Mathf.Abs(ls.y) > 0.001f ? (size.y * cellSize) / ls.y : size.y * cellSize
        );
    }

    private void FitSpriteToGridSize(Vector2Int size, float cellSize)
    {
        Sprite sprite = spriteRenderer.sprite;
        float spriteWorldWidth = sprite.rect.width / sprite.pixelsPerUnit;
        float spriteWorldHeight = sprite.rect.height / sprite.pixelsPerUnit;

        Vector3 scale = spriteRenderer.transform.localScale;
        scale.x = (size.x * cellSize) / spriteWorldWidth;
        scale.y = (size.y * cellSize) / spriteWorldHeight;
        spriteRenderer.transform.localScale = scale;
    }

    /// <summary>
    /// Finds a free adjacent cell around the building for unit spawning.
    /// Checks all cells bordering the building, returns null if none available.
    /// </summary>
    public Vector2Int? FindSpawnPoint(GridModel grid)
    {
        Vector2Int size = Data.size;

        // Check bottom row first, then left, right, top
        for (int x = GridPosition.x; x < GridPosition.x + size.x; x++)
        {
            Vector2Int below = new Vector2Int(x, GridPosition.y - 1);
            if (IsCellFreeForSpawn(grid, below)) return below;
        }

        for (int y = GridPosition.y; y < GridPosition.y + size.y; y++)
        {
            Vector2Int left = new Vector2Int(GridPosition.x - 1, y);
            if (IsCellFreeForSpawn(grid, left)) return left;

            Vector2Int right = new Vector2Int(GridPosition.x + size.x, y);
            if (IsCellFreeForSpawn(grid, right)) return right;
        }

        for (int x = GridPosition.x; x < GridPosition.x + size.x; x++)
        {
            Vector2Int above = new Vector2Int(x, GridPosition.y + size.y);
            if (IsCellFreeForSpawn(grid, above)) return above;
        }

        return null;
    }

    private bool IsCellFreeForSpawn(GridModel grid, Vector2Int pos)
    {
        if (!grid.IsInBounds(pos.x, pos.y)) return false;
        Cell cell = grid.GetCell(pos);
        if (cell == null || cell.IsOccupied) return false;

        // Check if another unit already occupies this cell
        float cs = grid.CellSize;
        Vector3 worldCenter = new Vector3((pos.x + 0.5f) * cs, (pos.y + 0.5f) * cs, 0f);
        Collider2D hit = Physics2D.OverlapPoint(worldCenter);
        if (hit != null && hit.GetComponentInParent<Unit>() != null) return false;

        return true;
    }

    protected override void Die()
    {
        EventBus<BuildingDestroyedEvent>.Raise(new BuildingDestroyedEvent(this));
        Destroy(gameObject);
    }
}
