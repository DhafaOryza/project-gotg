using System;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    public EntityDataSO entityData;

    // Bonus Stats
    public int bonusMaxHP;

    public int maxHealth { get; private set; }
    public int currentHealth;

    public bool IsDead { get; protected set; }

    public event Action<int, int> OnHealthChanged; // current, max
    public event Action<int> OnDamaged;             // damage amount
    public event Action OnDied;

    protected virtual void Awake()
    {
        maxHealth = CalcMaxHP();
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnDamaged?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public virtual void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // Dipakai Morning Management ("Memulihkan HP player")
    public virtual void RecoverForMorning()
    {
        IsDead = false;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    protected virtual void Die()
    {
        IsDead = true;
        OnDied?.Invoke();
    }

    #region Helper

    private int CalcMaxHP()
    {
        // entityData bisa null kalau GameSessionData/Registry ternyata tidak menyediakan PlayerDataSO
        // sama sekali (mis. lupa isi Registry asset) -> jangan crash, fallback ke 1 HP dan kasih tau lewat log.
        if (entityData == null)
        {
            Debug.LogError($"[{name}] entityData belum ter-set (GameSessionData/Registry tidak menyediakan PlayerDataSO). MaxHP di-default ke 1.");
            return 1;
        }

        int baseHp = Mathf.Max(1, Mathf.RoundToInt(entityData.baseVitality * entityData.hpPerVitality) + bonusMaxHP);
        return baseHp;
    }

    #endregion
}