// using System.Collections.Generic;
// using UnityEngine;
// [CreateAssetMenu(fileName = "newFireBallSkill", menuName = "Data/Skill/FireBall Data")]
// public class FireballSkillSO : AOESkillSO
// {
//     [Header ("VFX")]
//     public PoolIdSO VfxID;
//     [Header ("stats")]
//     public int damage = 40;

//     public override void Activate(PlayerBaseEntity caster, BaseEntity target)
//     {
//         base.Activate(caster, target);
//         Vector2 explosionPos = (Vector2)caster.transform.position + (caster.AimDirection * 2f);

//         //Visual Effect
//         if (GameManager.Instance != null && GameManager.Instance.poolManager != null)
//         {
//             GameManager.Instance.poolManager.Spawn(VfxID, explosionPos);
//         }

//         Collider2D[] hitCollider = Physics2D.OverlapCircleAll(explosionPos, aoeRadius);
//         HashSet<BaseEntity> processedEntities = new HashSet<BaseEntity>();

//         foreach (var col in hitCollider)
//         {
//             target = col.GetComponentInParent<BaseEntity>();
//             if (target != null && processedEntities.Add(target))
//             {
//                 if (target.IsDead || target.entityData == null) continue;

//                 if (target.entityData.Faction != caster.entityData.Faction)
//                 {
//                     target.TakeDamage(damage);
//                     Debug.Log($"[Fireball] Mengenai target: {target.name}");
//                 }
//             }
//         }
//     }
// }