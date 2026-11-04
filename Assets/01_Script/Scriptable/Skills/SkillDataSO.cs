using System.Threading;
using UnityEngine;

public enum SkillTargetType
{
    Enemy,      
    Self,   
    Any     
}

public enum SkillExecutionType
{
    Active,
    Passive
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

    [Header ("Execution Mode")]
    public SkillExecutionType executionType = SkillExecutionType.Active;

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
    public GameObject damageTextPrefab;

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
        
        bool isLucky = false;
        int finalDamage = user != null ? user.CalculateDamageWithLucky(baseValue, out isLucky) : baseValue;
        if (isLucky)
        {
            Debug.Log($"<color=yellow>[LUCKY CRITICAL!] Serangan {displayName} memicu Lucky! Damage: {baseValue} -> {finalDamage}</color>");
        }

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
                projectile.Setup(user, finalDamage, maxRange, isAOE, isLucky, damageTextPrefab);
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

            if (closestTarget != null)
            {
                closestTarget?.TakeDamage(finalDamage);
                closestTarget?.ApplyKnockback(spawnPos, force: 8f);
                ShowDamageText(closestTarget.transform.position, finalDamage, isLucky);
                
                if ((closestTarget.currentHealth <= 0 || closestTarget.IsDead) && user != null)
                {
                    // Debug.Log("<color=green>[KILL DETECTED] Single target musuh tewas!</color>");
                    user.OnEnemyKilledByPlayer();
                }
            }
            
        }
        else
        {
            foreach (var hit in hits)
            {
                var target = hit.GetComponentInParent<BaseEntity>();
                if (target != null && target != user && !(target is PlayerBaseEntity))
                {
                    target.TakeDamage(finalDamage);
                    target?.ApplyKnockback(spawnPos, force: 8f);
                    ShowDamageText(target.transform.position, finalDamage, isLucky);

                    if ((target.currentHealth <= 0 || target.IsDead) && user != null)
                    {
                        // Debug.Log("<color=green>[KILL DETECTED] AOE target musuh tewas!</color>");
                        user.OnEnemyKilledByPlayer();
                    }
                }
            }
        }
    
        return true;
    }

    public void ShowDamageText(Vector3 targetPos, int damage, bool isLucky)
    {
        if (damageTextPrefab == null) return;
        Vector3 spawnPos = targetPos + Vector3.up * 1.5f + new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f);
        GameObject textGO = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        if (textGO.TryGetComponent(out FloatingDamageText floatText))
            floatText.Setup(damage, isLucky);
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