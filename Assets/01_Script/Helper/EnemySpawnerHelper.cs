using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerHelper : MonoBehaviour
{
    [Header ("Pool Config")]
    [SerializeField] private List<PoolIdSO> enemyPoolId;

    [Header ("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector2 randomOffset = new Vector2(1f,1f);

    [ContextMenu ("Spawn Enemy Now")]
    public void SpawnEnemy()
    {
        if (enemyPoolId == null)
        {
            Debug.LogWarning("[EnemySpawnerHelper] Enemy Pool Id belum diisi!");
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.poolManager == null)
        {
            Debug.LogError("[EnemySpawnerHelper] GameManager / PoolManager tidak ditemukan!");
            return;
        }

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;
        foreach (PoolIdSO poolid in enemyPoolId)
        {
            if (poolid == null) continue;

            Vector3 spawnPos = basePos + new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y),
            0
        );

        GameObject spawnedEnemy = GameManager.Instance.poolManager.Spawn(poolid, spawnPos);
        if (spawnedEnemy != null)
            Debug.Log($"[EnemySpawnerHelper] Berhasil spawn musuh di posisi: {spawnPos}");
        }
        
    }
}