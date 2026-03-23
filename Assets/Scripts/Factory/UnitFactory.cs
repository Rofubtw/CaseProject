using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates unit instances using per-type Object Pools for efficient reuse (Factory Pattern).
/// Pools are created lazily on first request — no pre-allocation.
/// </summary>
public class UnitFactory : MonoBehaviour
{
    private Dictionary<UnitData, ObjectPool<Unit>> pools = new Dictionary<UnitData, ObjectPool<Unit>>();

    /// <summary>
    /// Gets a unit from the matching pool and configures it with the given data at spawnPosition.
    /// </summary>
    public Unit Create(UnitData data, Vector3 spawnPosition, float cellSize, GridModel grid)
    {
        if (!pools.ContainsKey(data))
            pools[data] = new ObjectPool<Unit>(data.prefab, transform);

        Unit unit = pools[data].Get();
        unit.transform.position = spawnPosition;
        unit.ResetUnit(data, cellSize, grid);
        return unit;
    }

    /// <summary>
    /// Returns a unit back to its pool (called on death instead of Destroy).
    /// </summary>
    public void Recycle(Unit unit)
    {
        if (unit.Data != null && pools.ContainsKey(unit.Data))
            pools[unit.Data].Release(unit);
        else
            unit.gameObject.SetActive(false);
    }
}
