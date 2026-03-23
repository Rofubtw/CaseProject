using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks enemy count and triggers victory when all enemies are eliminated (MVC Controller).
/// </summary>
public class VictoryController : MonoBehaviour
{
    [SerializeField] private VictoryView view;

    private int enemyBuildingCount;
    private int enemyUnitCount;

    private void Awake()
    {
        view.Hide();
        view.OnRestartClicked += OnRestartClicked;
    }

    private void OnDestroy()
    {
        view.OnRestartClicked -= OnRestartClicked;
    }

    private void OnEnable()
    {
        EventBus<BuildingPlacedEvent>.OnEvent += OnBuildingPlaced;
        EventBus<BuildingDestroyedEvent>.OnEvent += OnBuildingDestroyed;
        EventBus<UnitDestroyedEvent>.OnEvent += OnUnitDestroyed;
    }

    private void OnDisable()
    {
        EventBus<BuildingPlacedEvent>.OnEvent -= OnBuildingPlaced;
        EventBus<BuildingDestroyedEvent>.OnEvent -= OnBuildingDestroyed;
        EventBus<UnitDestroyedEvent>.OnEvent -= OnUnitDestroyed;
    }

    private void OnBuildingPlaced(BuildingPlacedEvent evt)
    {
        if (!evt.Building.IsPlayerBuilding)
            enemyBuildingCount++;
    }

    private void OnBuildingDestroyed(BuildingDestroyedEvent evt)
    {
        if (!evt.Building.IsPlayerBuilding)
        {
            enemyBuildingCount--;
            CheckVictory();
        }
    }

    private void OnUnitDestroyed(UnitDestroyedEvent evt)
    {
        if (!evt.Unit.IsPlayerUnit)
        {
            enemyUnitCount--;
            CheckVictory();
        }
    }

    public void SetEnemyUnitCount(int count)
    {
        enemyUnitCount = count;
    }

    private void CheckVictory()
    {
        if (enemyBuildingCount <= 0 && enemyUnitCount <= 0)
            view.Show();
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
