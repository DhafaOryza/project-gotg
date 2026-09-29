using UnityEngine;
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Data/Entity/Enemy Data")]
public class EnemyDataSO : EntityDataSO
{
    [Header ("Execute Settings")]
    public float attackDamage = 90f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1.2f;

    [Header ("Target Priority")]
    public EnemyTargetPriority targetPriority = EnemyTargetPriority.CLOSEST;
    [Header ("Threat Level")]
    public ThreatTier threatTier = ThreatTier.LOW;
    private void OnEnable()
    {
        _faction = FactionType.ENEMY;
    }
}