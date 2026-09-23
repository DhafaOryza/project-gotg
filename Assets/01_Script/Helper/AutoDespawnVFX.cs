using UnityEngine;
public class AutoDespawnVFX : MonoBehaviour, IPoolable
{
    [Header ("Pool Setting")]
    [SerializeField] private float lifetime = 0.5f;
    public void OnDespawn()
    {
        CancelInvoke(nameof(ReturnToPool));
    }

    public void OnSpawn()
    {
        Invoke(nameof(ReturnToPool), lifetime);
    }

    private void ReturnToPool()
    {
        if (GameManager.Instance != null && GameManager.Instance.poolManager != null)
        {
            GameManager.Instance.poolManager.Despawn(gameObject);
        }
    }
}
