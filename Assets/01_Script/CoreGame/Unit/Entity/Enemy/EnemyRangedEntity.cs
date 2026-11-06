using UnityEngine;

public class EnemyRangedEntity : EnemyBaseEntity
{
    private EnemyRangeDataSO _rangedData;

    protected override void Awake()
    {
        base.Awake();
        _rangedData = entityData as EnemyRangeDataSO;
    }

    /// <summary>
    /// Runs on the hit frame of the attack animation. Fires the cast skill at where the target is right now.
    /// </summary>
    protected override void PerformAttack()
    {
        if (_rangedData == null || _rangedData.castSkill == null)
        {
            // No ranged skill assigned: use the normal melee damage.
            base.PerformAttack();
            return;
        }

        Transform targetTransform = null;
        if (targetPlayer != null) targetTransform = targetPlayer.transform;
        else if (targetBuilding != null) targetTransform = targetBuilding.transform;

        if (targetTransform == null) return;

        Transform origin = attackPoint != null ? attackPoint : transform;
        Vector2 aimDirection = ((Vector2)targetTransform.position - AttackOrigin).normalized;

        _rangedData.castSkill.Activate(this, origin, aimDirection);
    }
}