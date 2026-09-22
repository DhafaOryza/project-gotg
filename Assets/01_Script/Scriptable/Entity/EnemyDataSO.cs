using UnityEngine;
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Data/Entity/Enemy Data")]
public class EnemyDataSO : EntityDataSO
{
    [Header ("Target Priority")]
    public EnemyTargetPriority targetPriority = EnemyTargetPriority.CLOSEST;
    [Header ("Threat Level")]
    public ThreatTier threatTier = ThreatTier.LOW;
    private void OnEnable()
    {
        _faction = FactionType.ENEMY;
    }
}