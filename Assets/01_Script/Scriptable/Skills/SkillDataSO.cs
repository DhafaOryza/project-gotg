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
    [Header("General Info")]
    public string skillId;
    public string displayName;
    [TextArea] public string description;
    public int baseValue;

    [Header("UI Progression & Level")]
    public int currentLevel = 1;
    public int maxLevel = 5;
    public int upgradeCost = 500;

    [Header("Skill Tier & Type")]
    [Range(1, 4)]
    public int tier = 1;
    public TypeRange typeRange;
    public SkillType skillType;
    public TargetMode targetMode;
    public bool isProjectile = false;

    [Header("UI & Visual")]
    public Sprite icon;

    [Header("Progression Default")]
    public bool locked = true;

    [Header("Targeting & Cooldown")]
    public SkillTargetType targetType = SkillTargetType.Enemy;
    public float cooldown = 3f;
    public Color skillColor = Color.white;

    [Header("Range hitbox")]
    public float maxRange = 2f;
    public Vector3 HitboxSize = new Vector3(2f, 2f, 1f);

    [Header("Visual Prefabs")]
    public GameObject skillPrefab;
    public GameObject ghostPreviewPrefab;

    public bool IsLocked => locked;

    /// <summary>
    /// Fungsi untuk menaikkan level skill saat tombol Upgrade ditekan di UI.
    /// </summary>
    public void LevelUp()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // Anda bisa menambahkan penyesuaian harga atau stat di sini jika diperlukan
        }
    }

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
                bool isAOE = (targetMode == TargetMode.AOE_TARGET);
                projectile.Setup(user, baseValue, maxRange, isAOE);
            }
            return true;
        }

        Vector3 boxCenter = spawnPos + (Vector3)(aimDirection.normalized * (maxRange * 0.5f));
        Collider[] hits = Physics.OverlapBox(boxCenter, HitboxSize * 0.5f, rotation);
        
        if (targetMode == TargetMode.SINGLE_TARGET)
        {
            BaseEntity closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var hit in hits)
            {
                var target = hit.GetComponentInParent<BaseEntity>();
                if (target != null && target != user && !(target is PlayerBaseEntity))
                {
                    float dist = Vector3.Distance(spawnPos, target.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestTarget = target;
                    }
                }
            }

            closestTarget?.TakeDamage(baseValue);
        }
        else
        {
            foreach (var hit in hits)
            {
                var target = hit.GetComponentInParent<BaseEntity>();
                if (target != null && target != user && !(target is PlayerBaseEntity))
                    target.TakeDamage(baseValue);
            }
        }
    
        return true;
    }
    
    /// <summary>
    /// Checks if this skill can be unlocked based on the previous tier's progress.
    /// </summary>
    public bool CanUnlock(GameSessionData sessionData)
    {
        if (tier == 1) return true; // Tier 1 is always unlocked by default

        int targetTier = tier - 1;
        // Contoh syarat: minimal ada 1 skill yang terbuka di tier sebelumnya
        int unlockedPreviousTierCount = sessionData.skills.FindAll(s => s != null && s.tier == targetTier && !s.locked).Count;

        return unlockedPreviousTierCount >= 1; // Ubah angka sesuai kebutuhan (misal: 10 dari 100)
    }
}