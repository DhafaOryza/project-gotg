using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBase : MonoBehaviour, IDamageable, IHealthSource
{
    public BuildingDataSO buildingData;

    public int bonusMaxHp;

    public int maxHealth { get; private set; }
    public int currentHealth;

    public bool IsDestroy { get; protected set; }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("Level Up")]
    [SerializeField] private int _maxLevel = 5;
    [SerializeField] private int _healthPerLevel = 100;
    [SerializeField] private int _levelUpBasePrice = 100;
    [SerializeField] private int _levelUpPriceStep = 50;

    [Header("Repair")]
    [SerializeField] private int _repairAmount = 100;
    [SerializeField] private int _repairPrice = 50;

    private int _level = 1;
    private static readonly List<AdditionalStatsSO> _emptyStats = new List<AdditionalStatsSO>();

    public int Level => _level;
    public int MaxLevel => _maxLevel;
    public bool IsMaxLevel => _level >= _maxLevel;
    public bool CanLevelUp => !IsDestroy && !IsMaxLevel;
    public bool CanRepair => !IsDestroy && currentHealth < maxHealth;
    public int RepairPrice => _repairPrice;

    public IReadOnlyList<AdditionalStatsSO> AdditionalStats =>
        buildingData != null ? buildingData.additionalStats : _emptyStats;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void Awake()
    {
        maxHealth = CalcMaxHP();
        currentHealth = maxHealth;
    }

    void OnValidate()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Destroy();
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (IsDestroy || amount <= 0) return;

        float damage = buildingData != null
            ? buildingData.GetDamageAfterDefense(amount)
            : amount;

        // Health is stored as an integer, so round the reduced damage to the nearest integer.
        int finalDamage = Mathf.RoundToInt(damage);
        if (finalDamage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Destroy();
    }

    protected virtual void Destroy()
    {
        IsDestroy = true;
        OnDied?.Invoke();
    }

    public virtual void ResetState()
    {
        IsDestroy = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    #region Level Up

    /// <summary>
    /// Returns the max health this building would have at the given level.
    /// </summary>
    public int GetMaxHealthAtLevel(int level)
    {
        if (buildingData == null) return 1;

        int clampedLevel = Mathf.Clamp(level, 1, Mathf.Max(1, _maxLevel));
        int levelBonus = (clampedLevel - 1) * _healthPerLevel;

        // MaxHealth includes base vitality and additional Vitality stats.
        return Mathf.Max(1, Mathf.RoundToInt(buildingData.MaxHealth) + bonusMaxHp + levelBonus);
    }

    /// <summary>
    /// Returns the price to reach the next level.
    /// </summary>
    public int GetLevelUpPrice()
    {
        return _levelUpBasePrice + (_level - 1) * _levelUpPriceStep;
    }

    /// <summary>
    /// Raises the level by one. Max health grows and current health grows by the same amount.
    /// </summary>
    public bool LevelUp()
    {
        if (!CanLevelUp) return false;

        _level++;
        RefreshMaxHealth();
        return true;
    }

    /// <summary>
    /// Levels up one additional stat of this building and recalculates max health.
    /// Returns false if the stat does not belong to this building or is already at max level.
    /// </summary>
    public bool LevelUpAdditionalStat(AdditionalStatsSO stats)
    {
        if (IsDestroy || stats == null || buildingData == null) return false;
        if (!buildingData.additionalStats.Contains(stats)) return false;
        if (!stats.TryLevelUp()) return false;

        // A Vitality upgrade changes buildingData.MaxHealth, so max health must be recalculated.
        RefreshMaxHealth();
        return true;
    }

    /// <summary>
    /// Recalculates max health and moves current health by the same difference.
    /// </summary>
    private void RefreshMaxHealth()
    {
        int oldMaxHealth = maxHealth;
        maxHealth = CalcMaxHP();

        currentHealth = Mathf.Clamp(currentHealth + (maxHealth - oldMaxHealth), 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    #endregion

    #region Repair

    /// <summary>
    /// Restores health by the repair amount, never above max health.
    /// </summary>
    public bool Repair()
    {
        if (!CanRepair) return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + _repairAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        return true;
    }

    #endregion

    #region Helper

    private int CalcMaxHP()
    {
        if (buildingData == null)
        {
            Debug.Log($"[{name}] buildingData not found, Health will be set 1");
            return 1;
        }

        return GetMaxHealthAtLevel(_level);
    }

    #endregion
}