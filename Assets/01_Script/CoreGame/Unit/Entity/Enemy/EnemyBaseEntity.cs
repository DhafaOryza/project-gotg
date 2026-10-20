using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseEntity : BaseEntity, IPoolable
{
    [SerializeField] private Transform attackPoint;

    private EnemyDataSO enemyData;
    private BaseEntity targetPlayer;
    private BuildingBase targetBuilding;
    private Collider targetCollider;
    private Rigidbody rb;
    private float attackCooldownRemaining;
    private Vector2 AttackOrigin => attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;
    private bool HasTarget => targetPlayer != null || targetBuilding != null;

    // Buffer bersama supaya GetSolidCollider tidak membuat array baru tiap dipanggil.
    private static readonly List<Collider> colliderBuffer = new List<Collider>();

    protected override void Awake()
    {
        base.Awake();
        enemyData = entityData as EnemyDataSO;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (IsDead) return;

        TickAttackCooldown(Time.deltaTime);

        if (targetPlayer != null && targetPlayer.IsDead)
            ClearTarget();

        if (targetBuilding != null && targetBuilding.IsDestroy)
            ClearTarget();

        if (!HasTarget)
            AcquireTarget();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        if (!HasTarget)
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
        ClearTarget();
        attackCooldownRemaining = 0f;

        if (rb != null)
            rb.linearVelocity = Vector3.zero;
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
    private void ClearTarget()
    {
        targetPlayer = null;
        targetBuilding = null;
        targetCollider = null;
    }
    /// <summary>
    /// Set target sekaligus cache collider-nya. Panggil dengan null untuk menghapus target.
    /// </summary>
    private void SetTargetPlayer(BaseEntity player)
    {
        targetPlayer = player;
        targetBuilding = null;
        targetCollider = GetSolidCollider(player);
    }
    private void SetTargetBuilding(BuildingBase building)
    {
        targetBuilding = building;
        targetPlayer = null;
        targetCollider = GetSolidCollider(building);
    }

    /// <summary>
    /// Dipanggil saat spawn. Kalau EnemyDataSO.targetPriority == OBJECT, cari TargetBase
    /// terdekat di scene dan simpan sebagai target. Priority CLOSEST/PLAYER belum ditangani
    /// di sini (butuh sistem targeting musuh/player terpisah).
    /// </summary>
    private void AcquireTarget()
    {
        if (enemyData == null)
            return;

        switch (enemyData.targetPriority)
        {
            case EnemyTargetPriority.PLAYER:
                PlayerBaseEntity player = FindPlayer();
                if (player != null)
                    SetTargetPlayer(player);
                else
                    FindClosestTarget();
                break;

            case EnemyTargetPriority.OBJECT:
                SetTargetBuilding(FindClosestBuilding());
                break;

            case EnemyTargetPriority.CLOSEST:
                FindClosestTarget();
                break;
        }
    }

    private PlayerBaseEntity FindPlayer()
    {
        PlayerBaseEntity player = FindAnyObjectByType<PlayerBaseEntity>();
        if (player != null && !player.IsDead && player.CanAct)
            return player;

        return null;
    }

    private BuildingBase FindClosestBuilding()
    {
        var Buildings = FindObjectsByType<BuildingBase>(FindObjectsSortMode.None);
        if (Buildings == null || Buildings.Length == 0) return null;

        Vector2 origin = AttackOrigin;
        BuildingBase closest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (var building in Buildings)
        {
            if (building == null || building.IsDestroy) continue;

            // Ukur ke tepi collider, bukan ke pusat Target
            Collider col = GetSolidCollider(building);
            Vector2 point = col != null
                ? ClosestPointXY(col, origin)
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

    private void FindClosestTarget()
    {
        PlayerBaseEntity player = FindPlayer();
        BuildingBase building = FindClosestBuilding();

        if (player == null && building == null)
        {
            ClearTarget();
            return;
        }
        if (player == null)
        {
            SetTargetBuilding(building);
            return;
        }
        if (building == null)
        {
            SetTargetPlayer(player);
            return;
        }

        float distToPlayer = Vector2.Distance(AttackOrigin, player.transform.position);
        float distToBuilding = Vector2.Distance(AttackOrigin, building.transform.position);

        if (distToPlayer < distToBuilding)
            SetTargetPlayer(player);
        else
            SetTargetBuilding(building);
        
    }

    private Vector2 GetTargetClosestPoint(Vector2 from)
    {
        if (targetCollider != null && targetCollider.enabled)
            return ClosestPointXY(targetCollider, from);

        return from;
    }

    /// <summary>
    /// Ambil collider "padat" (bukan trigger) milik objek atau child-nya.
    /// Collider trigger (mis. area deteksi sign) sengaja dilewati supaya musuh
    /// tidak mengukur jarak ke area trigger yang lebih besar dari bangunannya.
    /// </summary>
    private static Collider GetSolidCollider(Component root)
    {
        if (root == null) return null;

        root.GetComponentsInChildren(colliderBuffer);
        foreach (var col in colliderBuffer)
        {
            if (col.enabled && !col.isTrigger)
                return col;
        }
        return null;
    }

    /// <summary>
    /// Titik terdekat di collider dari titik 'from', diukur di bidang XY (sumbu Z diabaikan
    /// karena game bergerak di bidang 2D). Untuk MeshCollider non-convex dipakai bounds,
    /// karena Collider.ClosestPoint tidak mendukungnya.
    /// </summary>
    private static Vector2 ClosestPointXY(Collider col, Vector2 from)
    {
        Vector3 from3 = new Vector3(from.x, from.y, col.bounds.center.z);

        if (col is MeshCollider mesh && !mesh.convex)
            return col.bounds.ClosestPoint(from3);

        return col.ClosestPoint(from3);
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
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(dirX * moveSpeed, vel.y, vel.z);
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
        {
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, vel.y, vel.z); // Y dibiarkan
        }
    }

    private void TickAttackCooldown(float deltaTime)
    {
        if (attackCooldownRemaining > 0f)
            attackCooldownRemaining = Mathf.Max(0f, attackCooldownRemaining - deltaTime);
    }

    private void TryAttackTarget()
    {
        if (attackCooldownRemaining > 0f || enemyData == null) return;

        // int damage = Mathf.RoundToInt(enemyData.attackDamage);
        if (targetPlayer != null)
        {
            targetPlayer.TakeDamage(Mathf.RoundToInt(enemyData.attackDamage * 1.0f));
            attackCooldownRemaining = enemyData.attackCooldown;
        }
        else if (targetBuilding != null)
        {
            targetBuilding.TakeDamage(Mathf.RoundToInt(enemyData.attackDamage * 0.2f));
            attackCooldownRemaining = enemyData.attackCooldown;
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (this == null) return;
        
        float range = enemyData != null ? enemyData.attackRange : 1.5f;
        Vector2 origin = AttackOrigin;

        Gizmos.DrawWireSphere(origin, range);

        if (Application.isPlaying && HasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, GetTargetClosestPoint(origin));
        }
    }
}