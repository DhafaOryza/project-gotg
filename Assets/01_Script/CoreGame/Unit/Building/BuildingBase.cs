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

        currentHealth = Mathf.Max(0, currentHealth - amount);
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

        int baseHp = Mathf.Max(1, Mathf.RoundToInt(buildingData.baseVitality * buildingData.hpPerVitality) + bonusMaxHp);
        return baseHp;
    }

    #endregion
}