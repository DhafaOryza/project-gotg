using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public List<PoolCatalogSO> catalogs;

    private class Pool
    {
        public PoolDataSO data;
        public Transform root;
        public readonly Stack<PooledObject> available = new Stack<PooledObject>();
        public int totalCreated;
    }

    private readonly Dictionary<PoolIdSO, Pool> pools = new Dictionary<PoolIdSO, Pool>();

    public void Initialize()
    {
        ClearAll(); // aman jika Initialize dipanggil lagi saat pindah scene

        if (catalogs == null) return;

        foreach (var catalog in catalogs)
        {
            if (catalog == null) continue;

            foreach (var data in catalog.pools)
            {
                if (data == null || data.poolId == null || data.prefab == null)
                {
                    Debug.LogWarning($"[PoolManager] Data pool tidak valid di katalog '{catalog.categoryName}'.", catalog);
                    continue;
                }

                if (pools.ContainsKey(data.poolId))
                {
                    Debug.LogWarning($"[PoolManager] PoolId '{data.poolId.name}' duplikat, dilewati.", data);
                    continue;
                }

                CreatePool(data);
            }
        }
    }

    private void CreatePool(PoolDataSO data)
    {
        var rootGO = new GameObject($"Pool_{data.poolId.name}");
        rootGO.transform.SetParent(transform, false);

        var pool = new Pool { data = data, root = rootGO.transform };

        // Prewarm: instantiate di awal supaya tidak ada spike saat gameplay
        for (int i = 0; i < data.initialSize; i++)
        {
            var po = CreateInstance(pool);
            po.IsInPool = true;
            pool.available.Push(po);
        }

        pools.Add(data.poolId, pool);
    }

    private PooledObject CreateInstance(Pool pool)
    {
        GameObject go = Instantiate(pool.data.prefab, pool.root);
        go.SetActive(false);

        if (!go.TryGetComponent(out PooledObject po))
            po = go.AddComponent<PooledObject>();

        po.Setup(pool.data.poolId);
        pool.totalCreated++;
        return po;
    }

    // ---------- SPAWN ----------

    public GameObject Spawn(PoolIdSO id, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (id == null || !pools.TryGetValue(id, out var pool))
        {
            Debug.LogWarning($"[PoolManager] Pool untuk '{(id != null ? id.name : "null")}' tidak ditemukan.");
            return null;
        }

        PooledObject po = null;

        // Lewati objek yang mungkin sudah di-Destroy dari luar
        while (pool.available.Count > 0 && po == null)
            po = pool.available.Pop();

        if (po == null)
        {
            if (!pool.data.isExpandable)
            {
                Debug.LogWarning($"[PoolManager] Pool '{id.name}' habis dan tidak expandable.");
                return null;
            }
            po = CreateInstance(pool);
        }

        Transform t = po.transform;
        t.SetParent(parent != null ? parent : pool.root, false);
        t.SetPositionAndRotation(position, rotation);

        po.IsInPool = false;
        po.gameObject.SetActive(true);
        po.NotifySpawn();

        return po.gameObject;
    }

    public GameObject Spawn(PoolIdSO id, Vector3 position)
    {
        return Spawn(id, position, Quaternion.identity);
    }

    public T Spawn<T>(PoolIdSO id, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
    {
        GameObject go = Spawn(id, position, rotation, parent);
        return go != null && go.TryGetComponent(out T comp) ? comp : null;
    }

    // ---------- DESPAWN ----------

    public void Despawn(GameObject go)
    {
        if (go == null) return;

        if (!go.TryGetComponent(out PooledObject po) || !pools.TryGetValue(po.PoolId, out var pool))
        {
            Destroy(go); // bukan objek pool
            return;
        }

        if (po.IsInPool) return; // cegah double despawn

        po.NotifyDespawn();
        go.SetActive(false);
        go.transform.SetParent(pool.root, false);

        po.IsInPool = true;
        pool.available.Push(po);
    }

    // ---------- CLEANUP ----------

    private void ClearAll()
    {
        foreach (var pool in pools.Values)
        {
            if (pool.root != null) Destroy(pool.root.gameObject);
        }
        pools.Clear();
    }

    private void OnDestroy()
    {
        pools.Clear();
    }
}