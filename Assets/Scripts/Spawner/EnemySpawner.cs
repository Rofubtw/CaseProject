using UnityEngine;

/// <summary>
/// Spawns enemy buildings and units at the top portion of the grid when the game starts.
/// Attach to a GameObject in the scene and assign building/unit data in the Inspector.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Inject] private GridController gridController;
    [Inject] private BuildingFactory buildingFactory;
    [Inject] private UnitFactory unitFactory;
    [Inject] private VictoryController victoryController;

    [Header("Enemy Buildings")]
    [SerializeField] private BuildingData[] enemyBuildings;
    [SerializeField] private int buildingCount = 3;

    [Header("Enemy Units")]
    [SerializeField] private UnitData[] enemyUnits;
    [SerializeField] private int unitCount = 5;

    private void Start()
    {
        SpawnEnemyBuildings();
        int spawnedUnits = SpawnEnemyUnits();
        victoryController.SetEnemyUnitCount(spawnedUnits);
    }

    private void SpawnEnemyBuildings()
    {
        GridModel grid = gridController.Model;
        int topThirdStart = grid.Height * 2 / 3;

        for (int i = 0; i < buildingCount; i++)
        {
            BuildingData data = enemyBuildings[Random.Range(0, enemyBuildings.Length)];

            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = Random.Range(0, grid.Width - data.size.x + 1);
                int y = Random.Range(topThirdStart, grid.Height - data.size.y + 1);
                Vector2Int pos = new Vector2Int(x, y);

                if (gridController.TryPlaceBuilding(pos, data.size))
                {
                    Building building = buildingFactory.Create(data, pos, grid.CellSize);
                    building.SetTeam(false);
                    EventBus<BuildingPlacedEvent>.Raise(new BuildingPlacedEvent(building));
                    break;
                }
            }
        }
    }

    private int SpawnEnemyUnits()
    {
        GridModel grid = gridController.Model;
        float cs = grid.CellSize;
        int topThirdStart = grid.Height * 2 / 3;
        int spawned = 0;

        for (int i = 0; i < unitCount; i++)
        {
            UnitData data = enemyUnits[Random.Range(0, enemyUnits.Length)];

            for (int attempt = 0; attempt < 50; attempt++)
            {
                int x = Random.Range(0, grid.Width);
                int y = Random.Range(topThirdStart, grid.Height);

                Cell cell = grid.GetCell(x, y);
                if (cell == null || !cell.IsWalkable) continue;

                Vector3 worldCenter = new Vector3((x + 0.5f) * cs, (y + 0.5f) * cs, 0f);
                Collider2D overlap = Physics2D.OverlapPoint(worldCenter);
                if (overlap != null) continue;

                Unit unit = unitFactory.Create(data, worldCenter, cs, grid);
                unit.SetTeam(false);
                spawned++;
                break;
            }
        }

        return spawned;
    }
}
