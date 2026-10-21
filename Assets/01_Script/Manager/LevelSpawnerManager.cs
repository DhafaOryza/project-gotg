using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[System.Serializable]
public struct NPCSpawnConfig
{
    [SerializeField] private PoolIdSO _npcPoolId;
    [SerializeField] private Transform _spawnPosition;

    public PoolIdSO NpcPoolId => _npcPoolId;
    public Transform SpawnPosition => _spawnPosition;
}

public class LevelSpawnerManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PoolIdSO playerId;
    [SerializeField] private Transform spawnPlayerPosition;

    [Header("Building")]
    [SerializeField] private PoolIdSO buildingId;
    [SerializeField] private Transform spawnBuildingPosition;

    [Header("NPC Spawning")]
    [SerializeField] private List<NPCSpawnConfig> npcSpawnConfigs = new List<NPCSpawnConfig>();

    [Header("Enemy")]
    [SerializeField] private List<Transform> enemySpawnPositions = new List<Transform>();

    private PoolManager poolManager;
    private PlayerBaseEntity player;
    private GranaryBase granary;
    private CinemachineCamera cinemachine;

    public void Initialize()
    {
        poolManager = GameManager.Instance?.poolManager;

        SpawnPlayer();
        SpawnBuilding();
        SpawnNPCs();
    }

    private void SpawnPlayer()
    {
        if (playerId == null || spawnPlayerPosition == null) return;

        var currentPlayer = poolManager.Spawn(playerId, spawnPlayerPosition.position, Quaternion.identity);
        player = currentPlayer.GetComponent<PlayerBaseEntity>();

        cinemachine = GameManager.Instance?.cinemachine;
        if (cinemachine == null) return;

        cinemachine.Target = new CameraTarget { TrackingTarget = player.transform };
    }

    private void SpawnBuilding()
    {
        if (buildingId == null || spawnBuildingPosition == null) return;

        var currentGranary = poolManager.Spawn(buildingId, spawnBuildingPosition.position, Quaternion.identity);
        granary = currentGranary.GetComponent<GranaryBase>();
    }

    /// <summary>
    /// Spawns predefined NPCs at their specific assigned positions without randomization.
    /// </summary>
    private void SpawnNPCs()
    {
        if (poolManager == null || npcSpawnConfigs == null) return;

        foreach (var config in npcSpawnConfigs)
        {
            if (config.NpcPoolId != null && config.SpawnPosition != null)
            {
                var spawnedNPC = poolManager.Spawn(config.NpcPoolId, config.SpawnPosition.position, Quaternion.identity);

                if (spawnedNPC == null)
                {
                    Debug.LogWarning($"[LevelSpawnerManager] Gagal spawn NPC dengan ID: {config.NpcPoolId.name}. Pastikan ID sudah terdaftar di PoolCatalog!");
                }
                else
                {
                    Debug.Log($"[LevelSpawnerManager] Berhasil spawn NPC: {spawnedNPC.name} di posisi {config.SpawnPosition.position}");
                }
            }

        }
    }

    #region Spawning

    /// <summary>
    /// Spawn enemies for an interval based on wave data.
    /// </summary>
    public void SpawnEnemy(NightWaveDataSO.WaveEnemy wave)
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

    private bool IsSpawnValid(NightWaveDataSO.WaveEnemy wave)
    {
        if (poolManager == null)
        {
            Debug.LogWarning("[LevelSpawnerManager] PoolManager not ready.");
            return false;
        }

        if (wave == null)
        {
            Debug.LogWarning("[LevelSpawnerManager] Wave data null.");
            return false;
        }

        if (enemySpawnPositions == null || enemySpawnPositions.Count == 0)
        {
            Debug.LogWarning("[LevelSpawnerManager] Enemy spawn positions are empty.");
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

    #region Public API

    public PlayerBaseEntity GetCurrentPlayer() { return player; }
    public GranaryBase GetCurrentGranary() { return granary; }

    public void SetAllInteractable(bool currentFlag)
    {
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
                interactable.SetInteractable(currentFlag);
        }
    }

    #endregion
}