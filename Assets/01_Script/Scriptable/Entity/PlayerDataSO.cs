using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Entity/Player Data")]
public class PlayerDataSO : EntityDataSO
{
    // Pengaman agar persen pengurangan tidak pernah mencapai 100% (kebal total / cooldown nol).
    private const float MaxReductionPercent = 95f;
    private const float MinimumCooldownMultiplier = 0.1f;
    private const int PriceRounding = 5;

    public const int CharacterMaxLevel = 20;

    [Header ("Leveling Data")]
    [Min(1)] public int level = 1;

    [Header("Character Level Up")]
    [Min(0)] [SerializeField] private int vitalityPerLevel = 1;
    [Min(0f)] [SerializeField] private float cooldownReductionPerLevel = 0.02f;
    [Min(0)] [SerializeField] private int levelUpBasePrice = 250;
    [Min(1f)] [SerializeField] private float levelUpPriceGrowth = 1.5f;

    [Header("Additional Stats")]
    [SerializeField] private List<AdditionalStatsSO> additionalStats = new List<AdditionalStatsSO>();

    [System.NonSerialized] private bool isRuntimeCopy;

    private void OnEnable()
    {
       _faction = FactionType.ALLY;
    }

    #region Additional Stats (data)

    public IReadOnlyList<AdditionalStatsSO> AdditionalStats => additionalStats;
    public int CurrentLevel => Mathf.Clamp(level, 1, CharacterMaxLevel);
    public bool IsCharacterMaxLevel => CurrentLevel >= CharacterMaxLevel;

    public int GetLevelUpPrice()
    {
        if (IsCharacterMaxLevel) return 0;

        float rawPrice = levelUpBasePrice * Mathf.Pow(levelUpPriceGrowth, CurrentLevel - 1);
        return Mathf.Max(0, Mathf.FloorToInt(rawPrice / PriceRounding + 0.5f) * PriceRounding);
    }

    public bool TryLevelUpCharacter()
    {
        if (IsCharacterMaxLevel) return false;

        level = CurrentLevel + 1;
        return true;
    }

    public PlayerDataSO CreateRuntimeCopy()
    {
        var runtimeCopy = Instantiate(this);
        runtimeCopy.isRuntimeCopy = true;
        runtimeCopy.level = 1;
        runtimeCopy.additionalStats = new List<AdditionalStatsSO>();

        foreach (var stat in additionalStats)
        {
            if (stat == null)
            {
                runtimeCopy.additionalStats.Add(null);
                continue;
            }

            var runtimeStat = Instantiate(stat);
            runtimeStat.ResetLevel();
            runtimeCopy.additionalStats.Add(runtimeStat);
        }

        return runtimeCopy;
    }

    public void DestroyRuntimeCopy()
    {
        if (!isRuntimeCopy) return;

        foreach (var stat in additionalStats)
        {
            if (stat != null) Destroy(stat);
        }

        Destroy(this);
    }

    // Catatan: tambah / lepas stat sebaiknya lewat PlayerBaseEntity (AddAdditionalStat dst.)
    // supaya max HP ikut dihitung ulang. Method di sini hanya mengubah datanya.
    public void AddAdditionalStat(AdditionalStatsSO stat)
    {
        if (stat != null) additionalStats.Add(stat);
    }

    public bool RemoveAdditionalStat(AdditionalStatsSO stat)
    {
        return stat != null && additionalStats.Remove(stat);
    }

    public void ClearAdditionalStats()
    {
        additionalStats.Clear();
    }

    /// <summary>
    /// Total hasil kalkulasi untuk satu jenis stat. Rumus konversi dan batasnya
    /// ada di AdditionalStatsSO, bukan di sini.
    /// </summary>
    public float GetCalculatedStat(StatsType type)
    {
        return AdditionalStatsSO.CalculateTotal(type, additionalStats);
    }

    #endregion

    #region Derived Values

    /// <summary>Poin Vitality tambahan dari stat dan level karakter.</summary>
    public int GetAdditionalVitality()
    {
        return GetAdditionalVitalityAtLevel(CurrentLevel);
    }

    public int GetAdditionalVitalityAtLevel(int targetLevel)
    {
        int clampedLevel = Mathf.Clamp(targetLevel, 1, CharacterMaxLevel);
        float levelVitality = (clampedLevel - 1) * vitalityPerLevel;
        return Mathf.RoundToInt(GetCalculatedStat(StatsType.Vitality) + levelVitality);
    }

    public int GetMaxHealthAtLevel(int targetLevel, int bonusMaxHP = 0)
    {
        float vitality = baseVitality + GetAdditionalVitalityAtLevel(targetLevel);
        return Mathf.Max(1, Mathf.RoundToInt(vitality * hpPerVitality) + bonusMaxHP);
    }

    /// <summary>moveSpeed dasar + bonus Endurance.</summary>
    public float GetMoveSpeed()
    {
        return moveSpeed + GetCalculatedStat(StatsType.Endurance);
    }

    /// <summary>Persentase damage yang dikurangi Defense.</summary>
    public float GetDefenseReductionPercent()
    {
        return Mathf.Clamp(GetCalculatedStat(StatsType.Defense), 0f, MaxReductionPercent);
    }

    /// <summary>Damage setelah dikurangi Defense. Damage positif minimal tetap 1.</summary>
    public int ReduceDamage(int amount)
    {
        if (amount <= 0) return amount;

        float reduced = amount * (1f - GetDefenseReductionPercent() / 100f);
        return Mathf.Max(1, Mathf.RoundToInt(reduced));
    }

    /// <summary>
    /// Pengali cooldown skill: skillCooldownMultiplier bawaan dikali pengurangan dari Haste.
    /// Contoh: multiplier bawaan 1 dan Haste 10% -> 0.9.
    /// </summary>
    public float GetCooldownMultiplier()
    {
        return GetCooldownMultiplierAtLevel(CurrentLevel);
    }

    public float GetCooldownMultiplierAtLevel(int targetLevel)
    {
        int clampedLevel = Mathf.Clamp(targetLevel, 1, CharacterMaxLevel);
        float levelMultiplier = skillCooldownMultiplier - (clampedLevel - 1) * cooldownReductionPerLevel;
        float reduction = Mathf.Clamp(GetCalculatedStat(StatsType.Haste), 0f, MaxReductionPercent);

        float hasteMultiplier = 1f - reduction / 100f;
        return Mathf.Max(MinimumCooldownMultiplier, levelMultiplier * hasteMultiplier);
    }

    #endregion
}