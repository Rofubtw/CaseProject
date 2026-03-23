
public interface IEvent { }

#region GameplayEvents

public struct EntityDamagedEvent : IEvent
{
    public DamageableEntity Entity { get; }
    public int Damage { get; }
    public int RemainingHealth { get; }

    public EntityDamagedEvent(DamageableEntity entity, int damage, int remainingHealth)
    {
        Entity = entity;
        Damage = damage;
        RemainingHealth = remainingHealth;
    }
}

public struct BuildingDestroyedEvent : IEvent
{
    public Building Building { get; }

    public BuildingDestroyedEvent(Building building)
    {
        Building = building;
    }
}

public struct UnitDestroyedEvent : IEvent
{
    public Unit Unit { get; }

    public UnitDestroyedEvent(Unit unit)
    {
        Unit = unit;
    }
}

public struct BuildingPlacedEvent : IEvent
{
    public Building Building { get; }

    public BuildingPlacedEvent(Building building)
    {
        Building = building;
    }
}

public struct UnitProducedEvent : IEvent
{
    public Unit Unit { get; }
    public Building SourceBuilding { get; }

    public UnitProducedEvent(Unit unit, Building sourceBuilding)
    {
        Unit = unit;
        SourceBuilding = sourceBuilding;
    }
}

public struct EntitySelectedEvent : IEvent
{
    public DamageableEntity Entity { get; }

    public EntitySelectedEvent(DamageableEntity entity)
    {
        Entity = entity;
    }
}

public struct SelectionClearedEvent : IEvent { }

public struct BuildingSelectedForPlacementEvent : IEvent
{
    public BuildingData Data { get; }

    public BuildingSelectedForPlacementEvent(BuildingData data)
    {
        Data = data;
    }
}

#endregion

#region Utils

#endregion