using System.Collections.Generic;
using UnityEngine;

public class LevelSpawnerManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PoolIdSO playerId;
    [SerializeField] private Transform spawnPlayerPosition;

    [Header("Building")]
    [SerializeField] private PoolIdSO buildingId;
    [SerializeField] private Transform spawnBuildingPosition;

    [Header("Enemy")]
    [SerializeField] private List<Transform> enemySpawnPositions = new List<Transform>();

    private PoolManager poolManager;

    public void Initialize()
    {
        poolManager = GameManager.Instance?.poolManager;

        SpawnPlayer();
        SpawnBuilding();
    }

    private void SpawnPlayer()
    {
        poolManager.Spawn(playerId, spawnPlayerPosition.position, Quaternion.identity);
    }

    private void SpawnBuilding()
    {
        poolManager.Spawn(buildingId, spawnBuildingPosition.position, Quaternion.identity);
    }

    #region Spawning

    /// <summary>
    /// Spawn musuh untuk satu interval berdasarkan data wave yang dikirim WaveController:
    /// jumlahnya acak 1 sampai wave.spawnPerInterval (maksimal), masing-masing di titik acak
    /// pada enemySpawnPositions, dengan jenis musuh acak dari wave.enemyIds.
    /// </summary>
    public void SpawnEnemy(WaveDataSO.WaveEnemy wave)
    {
        if (!IsSpawnValid(wave)) return;

        int spawnCount = Random.Range(1, Mathf.Max(1, wave.spawnPerInterval) + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            Transform position = enemySpawnPositions[Random.Range(0, enemySpawnPositions.Count)];
            if (position == null) continue;

            PoolIdSO enemyId = wave.enemyIds[Random.Range(0, wave.enemyIds.Count)];
            if (enemyId == null) continue;

            poolManager.Spawn(enemyId, position.position, Quaternion.identity);
        }
    }

    private bool IsSpawnValid(WaveDataSO.WaveEnemy wave)
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[LevelSpawnerManager] PoolManager not ready, call Initialize() first.");
            return false;
        }

        if (wave == null)
        {
            Debug.LogWarning("[LevelSpawnerManager] Wave data null.");
            return false;
        }

        if (enemySpawnPositions == null || enemySpawnPositions.Count == 0)
        {
            Debug.LogWarning("[LevelSpawnerManager] enemySpawnPositions kosong.");
            return false;
        }

        if (wave.enemyIds == null || wave.enemyIds.Count == 0)
        {
            Debug.LogWarning("[LevelSpawnerManager] Wave doesn't have enemyIds.");
            return false;
        }

        return true;
    }

    #endregion
}