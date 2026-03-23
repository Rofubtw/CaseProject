using UnityEngine;

/// <summary>
/// Data definition for a unit (soldier) type.
/// </summary>
[CreateAssetMenu(fileName = "NewUnit", menuName = "Game/Unit Data")]
public class UnitData : EntityData
{
    [Header("Visuals")]
    public Unit prefab;

    [Header("Animation")]
    [Tooltip("Name of the attack animation state (e.g. Attack, Shoot, Guard)")]
    public string attackAnimationName = "Attack";

    [Header("Combat")]
    public int attackDamage;

    [Header("Movement")]
    public float moveSpeed = 3f;
}
