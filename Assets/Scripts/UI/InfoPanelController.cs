using UnityEngine;

/// <summary>
/// Listens to selection events and updates InfoPanelView accordingly (MVC Controller).
/// Also handles unit production requests from the panel.
/// </summary>
public class InfoPanelController : MonoBehaviour
{
    [SerializeField] private InfoPanelView view;
    [Inject] private UnitFactory unitFactory;
    [Inject] private GridController gridController;

    private Building selectedBuilding;
    private Unit selectedUnit;

    private void Start()
    {
        view.Hide();
    }

    private void OnEnable()
    {
        EventBus<EntitySelectedEvent>.OnEvent += OnEntitySelected;
        EventBus<SelectionClearedEvent>.OnEvent += OnSelectionCleared;
        EventBus<EntityDamagedEvent>.OnEvent += OnEntityDamaged;
    }

    private void OnDisable()
    {
        EventBus<EntitySelectedEvent>.OnEvent -= OnEntitySelected;
        EventBus<SelectionClearedEvent>.OnEvent -= OnSelectionCleared;
        EventBus<EntityDamagedEvent>.OnEvent -= OnEntityDamaged;
    }

    private void OnEntitySelected(EntitySelectedEvent evt)
    {
        if (evt.Entity is Building building)
        {
            selectedBuilding = building;
            selectedUnit = null;
            view.ShowBuildingInfo(building, OnUnitProductionRequested);
        }
        else if (evt.Entity is Unit unit)
        {
            selectedBuilding = null;
            selectedUnit = unit;
            view.ShowUnitInfo(unit);
        }
    }

    private void OnSelectionCleared(SelectionClearedEvent evt)
    {
        selectedBuilding = null;
        selectedUnit = null;
        view.Hide();
    }

    private void OnEntityDamaged(EntityDamagedEvent evt)
    {
        if (evt.Entity is Building b && b == selectedBuilding)
            view.UpdateHealth(evt.RemainingHealth, b.MaxHealth);
        else if (evt.Entity is Unit u && u == selectedUnit)
            view.UpdateHealth(evt.RemainingHealth, u.MaxHealth);
    }

    private void OnUnitProductionRequested(UnitData data)
    {
        if (selectedBuilding == null) return;

        GridModel grid = gridController.Model;
        Vector2Int? spawnCell = selectedBuilding.FindSpawnPoint(grid);

        if (spawnCell == null)
        {
            Debug.LogWarning("No free adjacent cell to spawn unit.");
            return;
        }

        float cs = grid.CellSize;
        Vector3 spawnPos = new Vector3((spawnCell.Value.x + 0.5f) * cs, (spawnCell.Value.y + 0.5f) * cs, 0f);
        Unit unit = unitFactory.Create(data, spawnPos, cs, grid);
        unit.SetTeam(true);

        EventBus<UnitProducedEvent>.Raise(new UnitProducedEvent(unit, selectedBuilding));
    }
}
