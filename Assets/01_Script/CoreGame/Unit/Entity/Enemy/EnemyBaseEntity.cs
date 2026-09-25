using System.Collections;
using UnityEngine;
public class EnemyBaseEntity : BaseEntity, IPoolable
{
    [Header ("Visual FeedBack")]
    [SerializeField] private SpriteRenderer _sr;
    [SerializeField] private Color _hitcolor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;
    public EnemyDataSO enemyData => entityData as EnemyDataSO;
    protected override void Awake()
    {
        base.Awake();
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
        if (_sr != null) originalColor = _sr.color;

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
        if (TryGetComponent<EnemyStateMachine>(out var stateMachine))
        {
            stateMachine.ResetFSM();
            Debug.Log($"[{gameObject.name}] Di-spawn dari Pool! HP Reset ke: {currentHealth}");
        }
    }

    public void OnDestroy()
    {
        OnDied -= HandleEnemyDefeat;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        if (_sr != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRedRoutine());
        }
    }
    public virtual void PerformAction(GameObject target) {}
    private IEnumerator FlashRedRoutine()
    {
        _sr.color = _hitcolor;
        yield return new WaitForSeconds(0.5f); // Bikin durasi lebih lama dulu buat dipastikan
        _sr.color = originalColor;
    }
}