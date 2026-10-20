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

    private List<SkillRuntimeData> skillRuntime = new List<SkillRuntimeData>();
    private float currentReviveTimer;

    public bool IsKnockDown { get; private set; }
    public float CurrentReviveTimer => currentReviveTimer;
    public float ReviveDuration => reviveDuration;
    public bool CanAct => !IsDead && !IsKnockDown;


    protected override void Awake()
    {
        base.Awake();

        InitializeSkillRuntimes();
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
            currentReviveTimer -= deltaTime;
            if (currentReviveTimer <= 0f)
                Revive();
        }
    }

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
        base.TakeDamage(amount);

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

    public bool TryUseSkill(int slotIndex, Transform attackPoint , Vector2 aimDirection)
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

        runtime.StartCooldown(skill.cooldown);

        return true;
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