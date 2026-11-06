using UnityEngine;

public class EnemyRangedEntity : EnemyBaseEntity
{
    private EnemyRangeDataSO rangedData;

    protected override void Awake()
    {
        base.Awake();
        rangedData = entityData as EnemyRangeDataSO;
    }

    protected override void TryAttackTarget()
    {
        if (attackCooldownRemaining > 0f || rangedData == null) return;

        Transform targetTransform = null;
        if (targetPlayer != null) targetTransform = targetPlayer.transform;
        else if (targetBuilding != null) targetTransform = targetBuilding.transform;

        Vector2 origin = AttackOrigin;
        Vector2 aimDir = ((Vector2)targetTransform.position - origin).normalized;

        if (targetTransform == null) return;
        
        if (rangedData.castSkill != null)
        {
            rangedData.castSkill.Activate(this, attackPoint != null ? attackPoint : transform ,aimDir);
        }
        else
        {
            base.TryAttackTarget();
        }

        attackCooldownRemaining = rangedData.attackCooldown;

    }
}