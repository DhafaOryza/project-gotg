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
    public GameObject currentTarget {get; private set;}
    public bool Hastarget => currentTarget != null;
    public bool isInRange
    {
        get
        {
            if (!Hastarget) return false;

            float executeRange = 1.5f;
            if (_enemyBase != null && _enemyBase.enemyData != null)
                executeRange = _enemyBase.enemyData.executeRange;

            float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
            return distance <= executeRange;
        }
    }

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
        GameObject bestTarget = null;
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
                    bestTarget = target.gameObject;
                } 
            }
            
        currentTarget = bestTarget;
        // _movement.SetTarget(bestTarget);   
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    } 
}