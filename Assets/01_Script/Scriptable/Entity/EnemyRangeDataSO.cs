using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyRangedData", menuName = "Data/Entity/Enemy Ranged Data")]
public class EnemyRangeDataSO : EnemyDataSO
{
    [Header("Skill Execution")]
    public SkillDataSO castSkill;
}