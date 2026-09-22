using UnityEngine;

public interface IPoolable
{
    void OnSpawn();   // dipanggil saat diambil dari pool (reset state di sini)
    void OnDespawn(); // dipanggil saat dikembalikan ke pool
}

public class PooledObject : MonoBehaviour
{
    public PoolIdSO PoolId { get; private set; }
    public bool IsInPool { get; set; }

    private IPoolable[] poolables;

    public void Setup(PoolIdSO id)
    {
        PoolId = id;
        poolables = GetComponentsInChildren<IPoolable>(true); // di-cache sekali saja
    }

    public void NotifySpawn()
    {
        for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawn();
    }

    public void NotifyDespawn()
    {
        for (int i = 0; i < poolables.Length; i++) poolables[i].OnDespawn();
    }
}