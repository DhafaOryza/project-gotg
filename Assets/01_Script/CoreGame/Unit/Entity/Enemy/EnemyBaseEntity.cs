using UnityEngine;

public class EnemyBaseEntity : BaseEntity, IPoolable
{
    [SerializeField] private Transform attackPoint;

    private EnemyDataSO enemyData;
    private BuildingBase targetBuilding;
    private Collider2D targetCollider;
    private Rigidbody2D rb;
    private float attackCooldownRemaining;
    private Vector2 AttackOrigin => attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

    protected override void Awake()
    {
        base.Awake();
        enemyData = entityData as EnemyDataSO;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (IsDead) return;

        TickAttackCooldown(Time.deltaTime);

        if (targetBuilding != null && targetBuilding.IsDestroy)
            SetTarget(null);
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        if (targetBuilding == null)
        {
            StopMoving();
            return;
        }

        MoveOrAttack();
    }

    protected override void Die()
    {
        if (IsDead) return;

        base.Die();
        ReturnToPool();
    }

    public void OnSpawn()
    {
        ResetState();
        attackCooldownRemaining = 0f;
        AcquireTarget();
    }

    public void OnDespawn()
    {
        SetTarget(null);
        attackCooldownRemaining = 0f;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void ReturnToPool()
    {
        var poolManager = GameManager.Instance?.poolManager;

        if (poolManager != null)
        {
            poolManager.Despawn(gameObject);
            return;
        }

        // Fallback kalau PoolManager tidak ditemukan: tetap hilangkan objek
        Debug.LogWarning($"[{name}] PoolManager tidak ditemukan, objek dinonaktifkan manual.");
        OnDespawn();
        gameObject.SetActive(false);
    }

    #region Targeting

    /// <summary>
    /// Set target sekaligus cache collider-nya. Panggil dengan null untuk menghapus target.
    /// </summary>
    private void SetTarget(BuildingBase building)
    {
        targetBuilding = building;
        targetCollider = building != null ? building.GetComponentInChildren<Collider2D>() : null;
    }

    /// <summary>
    /// Dipanggil saat spawn. Kalau EnemyDataSO.targetPriority == OBJECT, cari BuildingBase
    /// terdekat di scene dan simpan sebagai target. Priority CLOSEST/PLAYER belum ditangani
    /// di sini (butuh sistem targeting musuh/player terpisah).
    /// </summary>
    private void AcquireTarget()
    {
        if (enemyData == null)
        {
            Debug.LogWarning($"[{name}] entityData bukan EnemyDataSO, target priority tidak bisa dibaca.");
            return;
        }

        if (enemyData.targetPriority != EnemyTargetPriority.OBJECT)
            return;

        SetTarget(FindClosestBuilding());
    }

    private BuildingBase FindClosestBuilding()
    {
        var buildings = FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);
        if (buildings == null || buildings.Length == 0) return null;

        Vector2 origin = AttackOrigin;
        BuildingBase closest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (var building in buildings)
        {
            if (building == null || building.IsDestroy) continue;

            // Ukur ke tepi collider, bukan ke pusat building
            Collider2D col = building.GetComponentInChildren<Collider2D>();
            Vector2 point = col != null
                ? col.ClosestPoint(origin)
                : (Vector2)building.transform.position;

            float distanceSqr = (point - origin).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = building;
            }
        }

        return closest;
    }

    private Vector2 GetTargetClosestPoint(Vector2 from)
    {
        if (targetCollider != null && targetCollider.enabled)
            return targetCollider.ClosestPoint(from);

        return targetBuilding.transform.position;
    }

    #endregion

    #region Movement & Attack

    private void MoveOrAttack()
    {
        float attackRange = enemyData != null ? enemyData.attackRange : 1.5f;
        float moveSpeed = enemyData != null ? enemyData.moveSpeed : 2f;

        Vector2 origin = AttackOrigin;
        Vector2 closestPoint = GetTargetClosestPoint(origin);

        // Jarak dari attackPoint ke tepi collider (sama dengan lingkaran gizmo)
        float distance = Vector2.Distance(origin, closestPoint);

        if (distance > attackRange)
        {
            // Gerak hanya di sumbu X, sumbu Y diurus gravitasi
            float dx = closestPoint.x - origin.x;
            float dirX = Mathf.Abs(dx) > 0.01f ? Mathf.Sign(dx) : 0f;
            rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            StopMoving();
            TryAttackTarget();
        }
    }

    private void StopMoving()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Y dibiarkan
    }

    private void TickAttackCooldown(float deltaTime)
    {
        if (attackCooldownRemaining > 0f)
            attackCooldownRemaining = Mathf.Max(0f, attackCooldownRemaining - deltaTime);
    }

    private void TryAttackTarget()
    {
        if (targetBuilding == null || attackCooldownRemaining > 0f || enemyData == null) return;

        targetBuilding.TakeDamage(Mathf.RoundToInt(enemyData.attackDamage));
        attackCooldownRemaining = enemyData.attackCooldown;
    }

    #endregion

    private void OnDrawGizmos()
    {
        float range = enemyData != null ? enemyData.attackRange : 1.5f;
        Vector2 origin = AttackOrigin;

        Gizmos.DrawWireSphere(origin, range);

        if (Application.isPlaying && targetBuilding != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, GetTargetClosestPoint(origin));
        }
    }
}