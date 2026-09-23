using UnityEngine;
public class EnemyBaseEntity : BaseEntity, IPoolable
{
    protected override void Awake()
    {
        base.Awake();

        // if (entityData != null)
        // {
        //     entityData.Faction = FactionType.ENEMY;
        // }
        OnDied += HandleEnemyDefeat;
    }
    protected virtual void HandleEnemyDefeat()
    {
        Debug.Log($"[{gameObject.name}] Mati!");
        if (GameManager.Instance != null && GameManager.Instance.poolManager != null)
        {
            GameManager.Instance.poolManager.Despawn(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void OnDespawn() {}
    public void OnSpawn()
    {
        RecoverForMorning();
        Debug.Log($"[{gameObject.name}] Di-spawn dari Pool! HP Reset ke: {currentHealth}");
    }

    public void OnDestroy()
    {
        OnDied -= HandleEnemyDefeat;
    }
}