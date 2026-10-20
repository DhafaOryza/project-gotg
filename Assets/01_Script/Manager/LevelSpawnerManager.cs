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
    [SerializeField] private PoolIdSO _playerId;
    [SerializeField] private Transform _spawnPlayerPosition;

    [Header("Building")]
    [SerializeField] private PoolIdSO _buildingId;
    [SerializeField] private Transform _spawnBuildingPosition;

    [Header("NPC Spawning")]
    [SerializeField] private List<NPCSpawnConfig> _npcSpawnConfigs = new List<NPCSpawnConfig>();

    [Header("Enemy")]
    [SerializeField] private List<Transform> _enemySpawnPositions = new List<Transform>();

    private PoolManager _poolManager;
    private PlayerBaseEntity _player;
    private GranaryBase _granary;
    private CinemachineCamera _cinemachine;

    public void Initialize()
    {
        _poolManager = GameManager.Instance?.poolManager;

        SpawnPlayer();
        SpawnBuilding();
        SpawnNPCs();
    }

    private void SpawnPlayer()
    {
        if (_playerId == null || _spawnPlayerPosition == null) return;

        var currentPlayer = _poolManager.Spawn(_playerId, _spawnPlayerPosition.position, Quaternion.identity);
        _player = currentPlayer.GetComponent<PlayerBaseEntity>();

        _cinemachine = GameManager.Instance?.cinemachine;
        if (_cinemachine == null) return;

        _cinemachine.Target = new CameraTarget { TrackingTarget = _player.transform };
    }

    private void SpawnBuilding()
    {
        if (_buildingId == null || _spawnBuildingPosition == null) return;

        var currentGranary = _poolManager.Spawn(_buildingId, _spawnBuildingPosition.position, Quaternion.identity);
        _granary = currentGranary.GetComponent<GranaryBase>();
    }

    /// <summary>
    /// Spawns predefined NPCs at their specific assigned positions without randomization.
    /// </summary>
    private void SpawnNPCs()
    {
        if (_poolManager == null || _npcSpawnConfigs == null) return;

        foreach (var config in _npcSpawnConfigs)
        {
            if (config.NpcPoolId != null && config.SpawnPosition != null)
            {
                var spawnedNPC = _poolManager.Spawn(config.NpcPoolId, config.SpawnPosition.position, Quaternion.identity);

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
            Transform position = _enemySpawnPositions[Random.Range(0, _enemySpawnPositions.Count)];
            if (position == null) continue;

            PoolIdSO enemyId = wave.enemyIds[Random.Range(0, wave.enemyIds.Count)];
            if (enemyId == null) continue;

            _poolManager.Spawn(enemyId, position.position, Quaternion.identity);
        }
    }

    private bool IsSpawnValid(NightWaveDataSO.WaveEnemy wave)
    {
        if (_poolManager == null)
        {
            Debug.LogWarning("[LevelSpawnerManager] PoolManager not ready.");
            return false;
        }

        if (wave == null)
        {
            Debug.LogWarning("[LevelSpawnerManager] Wave data null.");
            return false;
        }

        if (_enemySpawnPositions == null || _enemySpawnPositions.Count == 0)
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

    public PlayerBaseEntity GetCurrentPlayer() => _player;
    public GranaryBase GetCurrentGranary() => _granary;

    #endregion
}