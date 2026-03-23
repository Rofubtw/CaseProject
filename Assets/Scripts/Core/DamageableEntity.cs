using UnityEngine;

/// <summary>
/// Base class for all damageable entities in the game (buildings, units).
/// </summary>
public abstract class DamageableEntity : MonoBehaviour
{
    public int CurrentHealth { get; protected set; }
    public int MaxHealth { get; protected set; }
    public bool IsDead => CurrentHealth <= 0;

    public virtual void Initialize(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage and triggers death if health reaches zero.
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        EventBus<EntityDamagedEvent>.Raise(new EntityDamagedEvent(this, damage, CurrentHealth));

        if (IsDead) Die();
    }

    protected abstract void Die();
}
