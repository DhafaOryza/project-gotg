using UnityEngine;

public class LevelSpawnerManager : MonoBehaviour
{
    [SerializeField] private PoolIdSO playerId;

    private PoolManager poolManager;

    public void Initialize()
    {
        poolManager = GameManager.Instance?.poolManager;

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        poolManager.Spawn(playerId, new Vector3(0, 0, 0), Quaternion.identity);
    }
}