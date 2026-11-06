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
        if (_rangedData == null) return;

        Vector3 targetCenterPos;

        if (targetCollider != null && targetCollider.enabled) targetCenterPos = targetCollider.bounds.center;
        else if (targetPlayer != null) targetCenterPos  = targetPlayer.transform.position + Vector3.up * 1f;
        else if (targetBuilding != null) targetCenterPos  = targetBuilding.transform.position + Vector3.up * 1.5f;
        else return;

        if (targetCenterPos  == null) return;

        Transform originTransform  = attackPoint != null ? attackPoint : transform;
        Vector2 aimDirection = ((Vector2)targetCenterPos - AttackOrigin).normalized;

        if (_rangedData.castSkill != null)
        {
            _rangedData.castSkill.Activate(this, originTransform , aimDirection);
        }
        else
        {
            base.PerformAttack();
        }
    }
}