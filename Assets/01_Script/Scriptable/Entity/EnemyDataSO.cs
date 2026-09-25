using UnityEngine;
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Data/Entity/Enemy Data")]
public class EnemyDataSO : EntityDataSO
{
    [Header ("Execute Settings")]
    public float executeDamage = 90f;
    public float executeRange = 1.5f;
    public float executeCooldown = 1.2f;

    [Header ("Target Priority")]
    public EnemyTargetPriority targetPriority = EnemyTargetPriority.CLOSEST;
    [Header ("Threat Level")]
    public ThreatTier threatTier = ThreatTier.LOW;
    private void OnEnable()
    {
        _faction = FactionType.ENEMY;
    }
}