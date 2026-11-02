using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseEntity : BaseEntity
{
    [Header("Skill Loadout")]
    [SerializeField] private List<SkillDataSO> equippedSkills = new List<SkillDataSO>();

    [Header("Consumable Loadout")]
    [SerializeField] private List<ConsumableDataSO> equippedConsumables = new List<ConsumableDataSO>();
    // [SerializeField] private int defaultConsumableQuantity = 2;

    [Header("Knockdown & Revive Configs")]
    [SerializeField] private float reviveDuration = 60f;
    [SerializeField] private float reviveHpPercentage = 0.15f;
    [SerializeField, Min(0.1f)] private float attackEventFallbackTime = 1.5f;

    private List<SkillRuntimeData> skillRuntime = new List<SkillRuntimeData>();
    private float currentReviveTimer;
    private PlayerDataSO runtimePlayerData;
    private PlayerAnimation playerAnimation;
    private int pendingSkillSlot = -1;
    private Transform pendingAttackPoint;
    private Vector2 pendingSkillDirection;
    private float pendingSkillTimer;

    public bool IsKnockDown { get; private set; }
    public bool HasPendingSkill => pendingSkillSlot >= 0;
    public float CurrentReviveTimer => currentReviveTimer;
    public float ReviveDuration => reviveDuration;
    public bool CanAct => !IsDead && !IsKnockDown;

    // Semua data stat tambahan (list, angka konversi) ada di PlayerDataSO.
    private PlayerDataSO PlayerData => entityData as PlayerDataSO;

    public IReadOnlyList<AdditionalStatsSO> AdditionalStats => PlayerData != null ? PlayerData.AdditionalStats : null;

    /// <summary>
    /// Kecepatan gerak setelah bonus Endurance. Dipakai PlayerController
    /// (gantikan playerEntity.entityData.moveSpeed).
    /// </summary>
    public float MoveSpeed
    {
        get
        {
            if (PlayerData != null) return PlayerData.GetMoveSpeed();
            return entityData != null ? entityData.moveSpeed : 0f;
        }
    }

    /// <summary>Pengali cooldown skill (skillCooldownMultiplier bawaan x pengurangan dari Haste).</summary>
    public float CooldownMultiplier
    {
        get
        {
            if (PlayerData != null) return PlayerData.GetCooldownMultiplier();
            return entityData != null ? entityData.skillCooldownMultiplier : 1f;
        }
    }

    public int CharacterLevel => PlayerData != null ? PlayerData.CurrentLevel : 1;
    public bool CanLevelUpCharacter => PlayerData != null && !PlayerData.IsCharacterMaxLevel;

    public int GetCharacterLevelUpPrice()
    {
        return PlayerData != null ? PlayerData.GetLevelUpPrice() : 0;
    }

    public int GetCharacterMaxHealthAtLevel(int targetLevel)
    {
        return PlayerData != null ? PlayerData.GetMaxHealthAtLevel(targetLevel, bonusMaxHP) : MaxHealth;
    }

    public float GetCharacterCooldownMultiplierAtLevel(int targetLevel)
    {
        return PlayerData != null ? PlayerData.GetCooldownMultiplierAtLevel(targetLevel) : CooldownMultiplier;
    }

    public bool LevelUpCharacter()
    {
        if (PlayerData == null || !PlayerData.TryLevelUpCharacter()) return false;

        RecalculateMaxHealth();
        return true;
    }


    protected override void Awake()
    {
        if (entityData is PlayerDataSO playerData)
        {
            runtimePlayerData = playerData.CreateRuntimeCopy();
            entityData = runtimePlayerData;
        }

        base.Awake();

        playerAnimation = GetComponentInChildren<PlayerAnimation>(true);
        InitializeSkillRuntimes();
    }

    private void OnDestroy()
    {
        if (runtimePlayerData != null)
            runtimePlayerData.DestroyRuntimeCopy();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        foreach (var runtime in skillRuntime)
        {
            runtime.UpdateCooldown(deltaTime);
        }

        if (IsKnockDown)
        {
            ClearPendingSkill();
            currentReviveTimer -= deltaTime;
            if (currentReviveTimer <= 0f)
                Revive();
        }

        if (pendingSkillSlot >= 0)
        {
            pendingSkillTimer -= deltaTime;
            if (pendingSkillTimer <= 0f)
            {
                Debug.LogWarning("[PlayerBaseEntity] Attack Animation Event tidak terpanggil; skill dijalankan lewat fallback.", this);
                ExecutePendingSkillFromAnimationEvent();
            }
        }
    }

    #region Additional Stats

    // Dipanggil BaseEntity.CalcMaxHP: Vitality tambahan ikut dihitung ke max HP.
    protected override int GetAdditionalVitality()
    {
        return PlayerData != null ? PlayerData.GetAdditionalVitality() : 0;
    }

    public void AddAdditionalStat(AdditionalStatsSO stat)
    {
        if (stat == null || PlayerData == null) return;

        PlayerData.AddAdditionalStat(stat);
        RecalculateMaxHealth();
    }

    public bool RemoveAdditionalStat(AdditionalStatsSO stat)
    {
        if (PlayerData == null || !PlayerData.RemoveAdditionalStat(stat)) return false;

        RecalculateMaxHealth();
        return true;
    }

    public void ClearAdditionalStats()
    {
        if (PlayerData == null) return;

        PlayerData.ClearAdditionalStats();
        RecalculateMaxHealth();
    }

    /// <summary>
    /// Naikkan level satu stat (maksimal sesuai Max Level di asset-nya) lalu hitung ulang max HP.
    /// Return false kalau stat sudah di level maksimal.
    /// </summary>
    public bool LevelUpAdditionalStat(AdditionalStatsSO stat)
    {
        if (stat == null || !stat.TryLevelUp()) return false;

        RecalculateMaxHealth();
        return true;
    }

    // Defense: damage yang diterima dikurangi persentase tertentu (minimal tetap 1).
    private int ApplyDefense(int amount)
    {
        return PlayerData != null ? PlayerData.ReduceDamage(amount) : amount;
    }

    #endregion

    // Satu runtime per slot, termasuk slot kosong (null), supaya indeks skillRuntime
    // selalu sejajar dengan equippedSkills (slot 2 = tombol E walau slot 0 dan 1 kosong).
    private void InitializeSkillRuntimes()
    {
        skillRuntime.Clear();

        if (equippedSkills == null) return;

        foreach (var skill in equippedSkills)
        {
            skillRuntime.Add(new SkillRuntimeData(skill));
        }
    }

    public void RedrawSkill()
    {
        var skillsManager = GameManager.Instance?.skillsManager;
        if (skillsManager == null)
        {
            Debug.LogWarning($"[{name}] RedrawSkill: skillsManager tidak ditemukan, loadout tidak diperbarui.");
            return;
        }

        // SALIN list-nya (jangan di-assign langsung). Kalau equippedSkills menunjuk list yang sama
        // dengan skillsManager.choosenSkills, pemanggilan RedrawSkill berikutnya akan menjalankan
        // equippedSkills.Clear() dan ikut menghapus pilihan skill di SkillsManager.
        equippedSkills = new List<SkillDataSO>(skillsManager.choosenSkills);

        InitializeSkillRuntimes();
    }

    public override void TakeDamage(int amount)
    {
        if (IsKnockDown || IsDead) return;

        // Defense mengurangi damage sebelum dihitung ke HP.
        base.TakeDamage(ApplyDefense(amount));

        if (currentHealth <= 0f && !IsKnockDown)
            StartKnockdown();

    }

    protected override void HandleDeathOrKnockdown()
    {
        if (!IsKnockDown)
            StartKnockdown();
    }

    public void StartKnockdown()
    {
        IsKnockDown = true;
        currentReviveTimer = reviveDuration;
        Debug.Log($"[{name}] Player Knockdown! Revive dalam {reviveDuration} detik.");
    }

    public void Revive()
    {
        if (!IsKnockDown) return;

        IsKnockDown = false;
        IsDead = false;

        int healAmount = Mathf.RoundToInt(maxHealth * reviveHpPercentage);
        Heal(healAmount);
        Debug.Log($"[{name}] Player berhasil Revive dengan {currentHealth} HP ({reviveHpPercentage * 100}% HP)!");
    }

    public bool TryUseSkill(int slotIndex, Transform attackPoint, Vector2 aimDirection)
    {
        if (!CanAct) return false;

        if (slotIndex < 0 || slotIndex >= equippedSkills.Count) return false;

        var skill = equippedSkills[slotIndex];
        if (skill == null) return false;

        // skillRuntime dibuat dari equippedSkills dengan urutan yang sama.
        if (slotIndex >= skillRuntime.Count) return false;

        var runtime = skillRuntime[slotIndex];
        if (runtime.isOnCooldown) return false;

        // Gagal (mis. prefab belum diisi) -> cooldown tidak dimulai.
        if (!skill.Activate(this, attackPoint, aimDirection)) return false;

        // Haste (dan skillCooldownMultiplier bawaan) memengaruhi cooldown.
        runtime.StartCooldown(skill.cooldown * CooldownMultiplier);

        return true;
    }

    public bool RequestSkillWithAnimation(int slotIndex, Transform attackPoint, Vector2 aimDirection)
    {
        if (!CanAct || pendingSkillSlot >= 0 || slotIndex < 0 || slotIndex >= equippedSkills.Count)
            return false;

        if (equippedSkills[slotIndex] == null || GetSkillCooldownRemaining(slotIndex) > 0f)
            return false;

        pendingSkillSlot = slotIndex;
        pendingAttackPoint = attackPoint;
        pendingSkillDirection = aimDirection.sqrMagnitude > 0f ? aimDirection.normalized : Vector2.right;
        pendingSkillTimer = attackEventFallbackTime;

        if (playerAnimation == null || !playerAnimation.PlayAttack())
            ExecutePendingSkillFromAnimationEvent();

        return true;
    }

    public void ExecutePendingSkillFromAnimationEvent()
    {
        if (pendingSkillSlot < 0) return;

        int slot = pendingSkillSlot;
        Transform attackOrigin = pendingAttackPoint;
        Vector2 direction = pendingSkillDirection;
        ClearPendingSkill();

        if (CanAct)
            TryUseSkill(slot, attackOrigin, direction);
    }

    private void ClearPendingSkill()
    {
        pendingSkillSlot = -1;
        pendingAttackPoint = null;
        pendingSkillDirection = Vector2.zero;
        pendingSkillTimer = 0f;
    }

    /// <summary>
    ///  Helper Methods
    /// </summary>
    public float GetSkillCooldownRemaining(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < skillRuntime.Count)
            return skillRuntime[slotIndex].RemainingCooldown;

        return 0f;
    }

    public int CalculateDamageWithLucky(int baseDamage, out bool isLucky)
    {
        if (PlayerData != null)
            return PlayerData.CalculatedLuckyDamage(baseDamage, out isLucky);

        isLucky = false;
        return baseDamage;
    }

    public override void ResetState()
    {
        IsKnockDown = false;
        currentReviveTimer = 0f;

        base.ResetState();
    }

    /// <summary>
    /// Memberi tahu player jika drop skill meleset (opsional untuk feedback UI/SFX).
    /// </summary>
    public void NotifySkillDropMissed(int slotIndex)
    {
        Debug.Log($"[{name}] Skill pada slot {slotIndex} di-drop di tempat yang tidak valid.");
    }

    public SkillDataSO GetEquippedSkillData(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedSkills.Count)
            return equippedSkills[slotIndex];

        return null;
    }
}