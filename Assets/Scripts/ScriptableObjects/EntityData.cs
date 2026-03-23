using UnityEngine;

/// <summary>
/// Base data definition for all entities that can take damage and be destroyed.
/// </summary>
public abstract class EntityData : ScriptableObject
{
    [Header("Base Info")]
    public string entityName;
    public Sprite sprite;
    public int maxHealth;
}
