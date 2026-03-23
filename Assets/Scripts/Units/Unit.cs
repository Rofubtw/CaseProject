using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a soldier unit on the game board.
/// Handles movement along A* paths and animation state.
/// </summary>
public class Unit : DamageableEntity
{
    public UnitData Data { get; private set; }
    public bool IsMoving => path != null && pathIndex < path.Count;
    public bool IsPlayerUnit { get; private set; } = true;

    private SpriteRenderer spriteRenderer;
    private UnitAnimator unitAnimator;
    private List<Vector2Int> path;
    private int pathIndex;
    private float cellSize;
    private DamageableEntity attackTarget;
    private float attackTimer;
    private const float AttackCooldown = 1f;
    private GridModel gridModel;
    private Vector2Int currentGridPos;

    private static readonly int TintColorId = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock propertyBlock;

    public void Setup(UnitData data, float cellSize, GridModel grid)
    {
        Data = data;
        this.cellSize = cellSize;
        this.gridModel = grid;
        MaxHealth = data.maxHealth;
        CurrentHealth = data.maxHealth;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        unitAnimator = GetComponentInChildren<UnitAnimator>();

        gameObject.name = data.entityName;

        if (cellSize > 0)
        {
            currentGridPos = new Vector2Int(
                Mathf.FloorToInt(transform.position.x / cellSize),
                Mathf.FloorToInt(transform.position.y / cellSize));

            if (gridModel != null)
            {
                Cell cell = gridModel.GetCell(currentGridPos);
                if (cell != null) cell.SetHasUnit(true);
            }
        }
    }

    /// <summary>
    /// Sets the team color tint via MaterialPropertyBlock (no extra draw call).
    /// </summary>
    public void SetTeam(bool isPlayer)
    {
        IsPlayerUnit = isPlayer;

        if (spriteRenderer == null) return;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        spriteRenderer.GetPropertyBlock(propertyBlock);
        Color tint = isPlayer ? new Color(0.6f, 0.7f, 1f) : new Color(1f, 0.6f, 0.6f);
        propertyBlock.SetColor(TintColorId, tint);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// Starts moving the unit along the given A* path.
    /// </summary>
    public void SetPath(List<Vector2Int> newPath, float cellSize)
    {
        path = newPath;
        pathIndex = 1;
        this.cellSize = cellSize;
        if (unitAnimator != null) unitAnimator.PlayLoop("Run");
    }

    public void StopMovement()
    {
        path = null;
        pathIndex = 0;
        if (unitAnimator != null) unitAnimator.PlayLoop("Idle");
    }

    private void Update()
    {
        HandleMovement();
        HandleCombat();
    }

    private void HandleMovement()
    {
        if (path == null || pathIndex >= path.Count) return;

        Vector2Int target = path[pathIndex];
        Vector3 targetWorld = new Vector3((target.x + 0.5f) * cellSize, (target.y + 0.5f) * cellSize, 0f);

        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            float dir = targetWorld.x - transform.position.x;
            if (Mathf.Abs(dir) > 0.01f)
                spriteRenderer.flipX = dir < 0;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetWorld, Data.moveSpeed * Time.deltaTime);

        if (Vector3.SqrMagnitude(transform.position - targetWorld) < 0.001f)
        {
            transform.position = targetWorld;
            UpdateGridPosition(target);
            pathIndex++;

            if (pathIndex >= path.Count)
                StopMovement();
        }
    }

    private void UpdateGridPosition(Vector2Int newPos)
    {
        if (gridModel == null) return;

        Cell oldCell = gridModel.GetCell(currentGridPos);
        if (oldCell != null) oldCell.SetHasUnit(false);

        currentGridPos = newPos;

        Cell newCell = gridModel.GetCell(currentGridPos);
        if (newCell != null) newCell.SetHasUnit(true);
    }

    private void HandleCombat()
    {
        if (attackTarget == null) return;

        if (attackTarget.IsDead)
        {
            ClearAttackTarget();
            return;
        }

        if (IsMoving) return;

        if (!IsAdjacentTo(attackTarget)) return;

        // Face the target
        if (spriteRenderer != null)
        {
            float dir = attackTarget.transform.position.x - transform.position.x;
            if (Mathf.Abs(dir) > 0.01f)
                spriteRenderer.flipX = dir < 0;
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = AttackCooldown;
        }
    }

    /// <summary>
    /// Plays the unit's attack animation (Attack, Shoot, etc.).
    /// </summary>
    public void PlayAttackAnimation(string attackStateName)
    {
        if (unitAnimator != null)
            unitAnimator.PlayOneShot(attackStateName, "Idle");
    }

    public void SetAttackTarget(DamageableEntity target)
    {
        attackTarget = target;
        attackTimer = 0f;
    }

    public void ClearAttackTarget()
    {
        attackTarget = null;
    }

    /// <summary>
    /// Called when this unit takes damage from another unit. Triggers counter-attack.
    /// </summary>
    public void TakeDamageFrom(int damage, Unit attacker)
    {
        TakeDamage(damage);

        if (attacker != null && attackTarget == null && !IsDead)
        {
            attackTarget = attacker;
            attackTimer = AttackCooldown * 0.5f;
        }
    }

    private void PerformAttack()
    {
        PlayAttackAnimation(Data.attackAnimationName);

        if (attackTarget is Unit enemyUnit)
            enemyUnit.TakeDamageFrom(Data.attackDamage, this);
        else
            attackTarget.TakeDamage(Data.attackDamage);
    }

    private bool IsAdjacentTo(DamageableEntity target)
    {
        if (cellSize <= 0) return false;

        Vector2Int myGrid = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / cellSize),
            Mathf.FloorToInt(transform.position.y / cellSize));

        if (target is Building building)
        {
            for (int x = building.GridPosition.x; x < building.GridPosition.x + building.Data.size.x; x++)
            {
                for (int y = building.GridPosition.y; y < building.GridPosition.y + building.Data.size.y; y++)
                {
                    int dist = Mathf.Abs(myGrid.x - x) + Mathf.Abs(myGrid.y - y);
                    if (dist <= 1) return true;
                }
            }
            return false;
        }

        Vector2Int targetGrid = new Vector2Int(
            Mathf.FloorToInt(target.transform.position.x / cellSize),
            Mathf.FloorToInt(target.transform.position.y / cellSize));
        return Mathf.Abs(myGrid.x - targetGrid.x) + Mathf.Abs(myGrid.y - targetGrid.y) <= 1;
    }

    /// <summary>
    /// Resets the unit for object pool reuse.
    /// </summary>
    public void ResetUnit(UnitData data, float cellSize, GridModel grid)
    {
        ClearAttackTarget();
        StopMovement();
        Setup(data, cellSize, grid);
    }

    private void UnmarkCurrentCell()
    {
        if (gridModel == null) return;
        Cell cell = gridModel.GetCell(currentGridPos);
        if (cell != null) cell.SetHasUnit(false);
    }

    protected override void Die()
    {
        ClearAttackTarget();
        StopMovement();
        UnmarkCurrentCell();
        EventBus<UnitDestroyedEvent>.Raise(new UnitDestroyedEvent(this));
        gameObject.SetActive(false);
    }
}
