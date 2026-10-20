using UnityEngine;
public enum SkillTargetType
{
    Enemy,      
    Self,   
    Any     
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Data/Skill/Skill Data")]
public class SkillDataSO : ScriptableObject
{
    [Header ("General Info")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;
    public int baseValue;

    [Header("skill Tier & Type")]
    [Range(1, 4)]
    public int tier;
    public TypeRange typeRange;
    public SkillType skillType;
    public bool isProjectile = false;

    [Header("UI & Visual")]
    public Sprite icon;

    [Header("Progession Default")]
    public bool locked = true;

    [Header("Targeting & Cooldown")]
    public SkillTargetType targetType = SkillTargetType.Enemy;
    public float cooldown = 3f;
    public Color skillColor = Color.white;

    [Header ("Range hitbox")]
    public float maxRange = 2f;
    public Vector3 HitboxSize = new Vector3(2f,2f,1f);

    [Header("Visual Prefabs")]
    public GameObject skillPrefab;

    public bool Activate(PlayerBaseEntity user, Transform attackPoint, Vector2 aimDirection)
    {
        Vector3 spawnPos = attackPoint != null ? attackPoint.position : user.transform.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        if (isProjectile)
        {
            if (skillPrefab == null)
            {
                Debug.LogWarning($"[{displayName}] Prefab Projectile belum diisi!");
                return false;
            }

            GameObject spawned = Instantiate(skillPrefab, spawnPos, rotation);
            if (spawned.TryGetComponent(out SkillProjectile projectile))
            {
                projectile.Setup(user, baseValue, maxRange);
            }
            return true;

        }

         Vector3 boxCenter = spawnPos + (Vector3)(aimDirection.normalized * (maxRange * 0.5f));
        Collider[] hits = Physics.OverlapBox(boxCenter, HitboxSize * 0.5f, rotation);

        foreach (var hit in hits)
        {
            var target = hit.GetComponentInParent<BaseEntity>();
            if (target != null && target != user && !(target is PlayerBaseEntity))
            {
                target.TakeDamage(baseValue);
            }
        }

        return true;
    }
}