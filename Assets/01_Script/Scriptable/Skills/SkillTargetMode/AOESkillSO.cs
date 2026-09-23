using UnityEngine;
using System.Collections.Generic;
public abstract class AOESkillSO : SkillDataSO
{
    [Header ("AOE Target Setting")]
    public float aoeRadius = 3f;

    protected List<BaseEntity> GetAOETargets(PlayerBaseEntity caster, Vector2 centerPosition)
    {
        List<BaseEntity> validTargets = new List<BaseEntity>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(centerPosition, aoeRadius);
        foreach (var hit in hits)
        {
            BaseEntity target = hit.GetComponentInChildren<BaseEntity>();
            if (target != null && target.entityData.Faction != caster.entityData.Faction)
            {
                if (!validTargets.Contains(target))
                {
                    validTargets.Add(target);
                }
            }
        }
        return validTargets;
    }
}