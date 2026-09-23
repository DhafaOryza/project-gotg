using UnityEngine;
[CreateAssetMenu(fileName = "newFireBallSkill", menuName = "Data/Skill/FireBall Data")]
public class FireballSkillSO : AOESkillSO
{
    [Header ("VFX")]
    public PoolIdSO VfxID;
    [Header ("stats")]
    public int damage = 40;

    public override void Activate(PlayerBaseEntity caster)
    {
        base.Activate(caster);
        Vector2 explosionPos = (Vector2)caster.transform.position + (caster.AimDirection * 2f);

        //Visual Effect
        if (GameManager.Instance != null && GameManager.Instance.poolManager != null)
        {
            GameManager.Instance.poolManager.Spawn(VfxID, explosionPos);
        }

        var targets = GetAOETargets(caster, explosionPos);
        foreach (var hit in targets)
        {
            hit.TakeDamage(damage);
        }
    }
}