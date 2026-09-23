using UnityEngine;

public abstract class SingleTargetSkillSO : SkillDataSO
{
    [Header ("Single Target Setting")]
    public float castRange = 5f;

    protected BaseEntity GetSingleTarget(PlayerBaseEntity caster)
    {
        RaycastHit2D hit = Physics2D.Raycast(caster.transform.position, caster.AimDirection, castRange);
        if (hit.collider != null)
        {
            BaseEntity target = hit.collider.GetComponentInParent<BaseEntity>();
            if (target != null && target.entityData.Faction != caster.entityData.Faction)
            {
                return target;
            }
        }
        return null;
    }
}