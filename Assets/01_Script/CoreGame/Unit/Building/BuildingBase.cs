using System;
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

    #region Helper

    private int CalcMaxHP()
    {
        if (buildingData == null)
        {
            Debug.Log($"[{name}] buildingData not found, Health will be set 1");
            return 1;
        }

        // MaxHealth includes base vitality and additional Vitality stats.
        return Mathf.Max(1, Mathf.RoundToInt(buildingData.MaxHealth) + bonusMaxHp);
    }

    #endregion
}