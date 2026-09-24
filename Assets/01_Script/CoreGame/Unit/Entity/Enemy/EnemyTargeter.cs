using UnityEngine;

public class EnemyTargeter : MonoBehaviour
{
    private EnemyBaseEntity _enemyBase;
    private EnemyMovement _movement;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask targetLayer; 
    [SerializeField] private float searchInterval = 0.3f; 

    private float _timer;
    private void Awake()
    {
        _enemyBase = GetComponent<EnemyBaseEntity>();
        _movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= searchInterval)
        {
            _timer = 0F;
            FindAndAssignTarget();
        }
    }

    /// <summary>
    /// Mencari Target Berdasarkan TargetPriority Musuh.
    /// </summary>
    private void FindAndAssignTarget()
    {
        if (_enemyBase == null || _enemyBase.entityData == null) return;

        EnemyDataSO enemyData = _enemyBase.entityData as EnemyDataSO;
        if (enemyData == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, targetLayer);
        Transform bestTarget = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            BaseEntity target = hit.GetComponentInParent<BaseEntity>();
            if (target == null || target.IsDead) continue;
            if (target.entityData == null || target.entityData.Faction == _enemyBase.entityData.Faction) continue;

            bool isPlayer = target is PlayerBaseEntity;

            //Jika Building Only
            if (enemyData.targetPriority == EnemyTargetPriority.OBJECT && isPlayer) continue;
            //Jika Player Only
            if (enemyData.targetPriority == EnemyTargetPriority.PLAYER && !isPlayer) continue;
            // Closest(Semuanya)
            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist < minDistance)
                {
                    minDistance = dist;
                    bestTarget = target.transform;
                } 
            }

        _movement.SetTarget(bestTarget);   
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    } 
}