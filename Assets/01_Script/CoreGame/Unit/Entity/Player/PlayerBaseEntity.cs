using System.Collections.Generic;
using UnityEngine;

public class PlayerBaseEntity : BaseEntity
{
    [Header("Skill Loadout (3-5 skill)")]
    [SerializeField] private List<SkillDataSO> equippedSkills = new List<SkillDataSO>();

    [Header("Consumable Loadout")]
    [SerializeField] private List<ConsumableDataSO> equippedConsumables = new List<ConsumableDataSO>();
    [SerializeField] private int defaultConsumableQuantity = 2;

    private  List<SkillRuntimeData> skillRuntime = new List<SkillRuntimeData>();
    public bool CanAct => !IsDead;

    protected override void Awake()
    {
        // GetOrCreate() sekarang menjamin GameSessionData sudah selesai EnsureDataLoaded()
        // (baik lewat instance yang sudah ada di scene maupun yang baru dibuat), jadi
        // baris-baris di bawah ini tidak lagi "balapan" dengan Start() milik GameSessionData.
        var session = GameSessionData.GetOrCreate();
        entityData = session.GetPlayerData();

        base.Awake(); // BaseEntity.Awake pakai entityData buat hitung maxHealth -> aman karena sudah terisi

        equippedSkills = session.GetSkillsData();
        equippedConsumables = session.GetConsumables();

        InitializeSkillRuntimes();
    }

    private void Update()
    {
       float deltaTime = Time.deltaTime;
       foreach (var runtime in skillRuntime)
        {
            runtime.UpdateCooldown(deltaTime);
        } 
    }

    /// <summary>
    /// Menyiapkan data runtime cooldown berdasarkan equippedSkills.
    /// </summary>
    private void InitializeSkillRuntimes()
    {
        skillRuntime.Clear();
        foreach (var skill in equippedSkills)
        {
            if (skill != null)
            {
                skillRuntime.Add(new SkillRuntimeData(skill));
            }
        }
    }

    /// <summary>
    /// Method untuk memanggil Skill yang dipunyai player.
    /// </summary>
    public bool TryUseSkill(int slotIndex, BaseEntity target)
    {
        if (!CanAct)
        {
            Debug.LogWarning($"[{name}] Tidak bisa menggunakan skill karena karakter tidak aktif/mati.");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= skillRuntime.Count)
        {
            Debug.LogWarning($"[{name}] Slot Index skill {slotIndex} di luar jangkauan.");
            return false;
        }

        SkillRuntimeData runtime = skillRuntime[slotIndex];
        SkillDataSO skill = runtime.skilldata;

        if (skill == null)
        {
            Debug.LogWarning($"[{name}] Skill pada slot {slotIndex} kosong.");
            return false;
        }

        if (runtime.isOnCooldown)
        {
            Debug.LogWarning($"[{name}] Skill '{skill.displayName}' masih cooldown! (Sisa: {runtime.RemainingCooldown:F1}s)");
            return false;
        }

        if (!skill.IsValidTarget(this, target))
        {
            Debug.LogWarning($"[{name}] Target {(target != null ? target.name : "null")} tidak valid untuk skill '{skill.displayName}'. Tipe Target: {skill.targetType}");
            return false;
        }

        skill.Activate(this, target);
        float cdMultiplier = entityData != null ? entityData.skillCooldownMultiplier : 1f;
        float finalcooldown = skill.cooldown * cdMultiplier;

        runtime.StartCooldown(finalcooldown);
        Debug.Log($"<color=green>[PlayerSkill]</color> Skill '{skill.displayName}' berhasil digunakan ke {(target != null ? target.name : "null")}. Cooldown: {finalcooldown:F1}s");
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
    public SkillDataSO GetDataData(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < skillRuntime.Count)
            return skillRuntime[slotIndex].skilldata;

        return null;
    }

    /// <summary>
    /// Memberi tahu player jika drop skill meleset (opsional untuk feedback UI/SFX).
    /// </summary>
    public void NotifySkillDropMissed(int slotIndex)
    {
        Debug.Log($"[{name}] Skill pada slot {slotIndex} di-drop di tempat yang tidak valid.");
    }

    // private void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying) return;
    //     Gizmos.color = Color.yellow;
    //     Vector3 origin = transform.position;
    //     Vector3 target = origin + (Vector3)AimDirection * 1.5f;
    //     Gizmos.DrawLine(origin,target);
    //     Gizmos.DrawWireSphere(target, 0.2f);
    // }
}