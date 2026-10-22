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
    private List<NPCBaseEntity> NPCs = new List<NPCBaseEntity>();
    private List<EnemyBaseEntity> enemies = new List<EnemyBaseEntity>();
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

    #region Spawning

    /// <summary>
    /// Spawns predefined NPCs at their specific assigned positions without randomization.
    /// </summary>
    public void SpawnNPCs()
    {
        if (poolManager == null) poolManager = GameManager.Instance?.poolManager;
        if (poolManager == null || npcSpawnConfigs == null) return;

        // Hindari NPC ganda kalau SpawnNPCs dipanggil lagi sebelum DespawnAllNPC.
        if (NPCs.Count > 0) DespawnAllNPC();

        foreach (var config in npcSpawnConfigs)
        {
            if (config.NpcPoolId == null || config.SpawnPosition == null) continue;

            var spawnedNPC = poolManager.Spawn(config.NpcPoolId, config.SpawnPosition.position, Quaternion.identity);

            if (spawnedNPC == null)
            {
                Debug.LogWarning($"[LevelSpawnerManager] Gagal spawn NPC dengan ID: {config.NpcPoolId.name}. Pastikan ID sudah terdaftar di PoolCatalog!");
                continue;
            }

            var npc = spawnedNPC.GetComponent<NPCBaseEntity>();
            if (npc != null) NPCs.Add(npc);

            Debug.Log($"[LevelSpawnerManager] Berhasil spawn NPC: {spawnedNPC.name} di posisi {config.SpawnPosition.position}");
        }
    }

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

            var currentEnemy = poolManager.Spawn(enemyId, position.position, Quaternion.identity);
            if (currentEnemy == null) continue;

            var enemy = currentEnemy.GetComponent<EnemyBaseEntity>();

            // Objek dari pool bisa dipakai ulang, jadi jangan masukkan instance yang sama dua kali.
            if (enemy != null && !enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
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
    public List<NPCBaseEntity> GetAllNPC() { return NPCs; }

    /// <summary>
    /// Jumlah musuh yang masih hidup. Musuh yang sudah mati / dikembalikan ke pool
    /// (objeknya nonaktif) dibuang dari daftar, jadi hasilnya selalu terbaru.
    /// </summary>
    public int GetAliveEnemyCount()
    {
        enemies.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy || e.IsDead);
        return enemies.Count;
    }

    /// <summary>
    /// Pulihkan player dan lumbung: HP penuh, state kembali normal. Player juga dikembalikan
    /// ke titik spawn. Dipakai saat kembali ke GameState.Starting setelah kalah.
    /// </summary>
    public void ResetCharacters()
    {
        if (player != null)
        {
            player.ResetState();

            if (spawnPlayerPosition != null)
            {
                player.transform.position = spawnPlayerPosition.position;

                if (player.TryGetComponent(out Rigidbody rb))
                    rb.linearVelocity = Vector3.zero;
            }
        }

        if (granary != null)
            granary.ResetState();
    }

    public void SetAllInteractable(bool currentFlag)
    {
        var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
                interactable.SetInteractable(currentFlag);
        }
    }

    public void DespawnAllNPC()
    {
        if (poolManager == null)
        {
            poolManager = GameManager.Instance?.poolManager;
        }

        foreach (var npc in NPCs)
        {
            if (npc != null)
            {
                poolManager.Despawn(npc.gameObject);
            }
        }

        // Daftar dikosongkan supaya SpawnNPCs berikutnya tidak menumpuk instance lama.
        NPCs.Clear();
    }

    public void DespawnAllEnemy()
    {
        if (poolManager == null)
        {
            poolManager = GameManager.Instance?.poolManager;
        }

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                poolManager.Despawn(enemy.gameObject);
            }
        }

        enemies.Clear();
    }

    #endregion
}