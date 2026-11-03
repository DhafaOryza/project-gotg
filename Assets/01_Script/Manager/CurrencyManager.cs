using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int _money = 0;

    [Header("Battle Reward")]
    [SerializeField] private int _maxBattleReward = 1000;
    [SerializeField] private float _targetSurvivalSeconds = 180f;
    [SerializeField] private int _knockdownLimit = 3;
    [SerializeField] private float _loseRewardMultiplier = 0.5f;

    [Header("Battle Score Weights")]
    [SerializeField] private float _survivalWeight = 1f;
    [SerializeField] private float _killWeight = 1f;
    [SerializeField] private float _knockdownWeight = 1f;
    [SerializeField] private float _granaryWeight = 1f;

    public int Money => _money;

    public event Action OnChange;

    void OnValidate()
    {
        OnChange?.Invoke();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        _money += amount;
        OnChange?.Invoke();
    }

    /// <summary>Kurangi uang. Return false (tanpa mengubah apa pun) kalau uang tidak cukup.</summary>
    public bool SpendMoney(int amount)
    {
        if (amount < 0 || amount > _money) return false;

        _money -= amount;

        // Dulu tidak memicu event, jadi UI (teks uang, tombol upgrade) tidak ikut berubah setelah membeli.
        OnChange?.Invoke();
        return true;
    }

    #region Battle Reward

    /// <summary>
    /// Turns a battle report into percentages and money without paying anything.
    /// </summary>
    public BattleReward CalculateBattleReward(BattleReport report, bool won)
    {
        float survival = Mathf.Clamp01(report.SurvivalTime / Mathf.Max(1f, _targetSurvivalSeconds));

        float kill = report.EnemiesSpawned > 0
            ? Mathf.Clamp01((float)report.EnemiesKilled / report.EnemiesSpawned)
            : 0f;

        float knockdown = 1f - Mathf.Clamp01((float)report.PlayerKnockdowns / Mathf.Max(1, _knockdownLimit));

        float granary = report.GranaryMaxHealth > 0
            ? 1f - Mathf.Clamp01((float)report.GranaryDamage / report.GranaryMaxHealth)
            : 0f;

        float totalWeight = _survivalWeight + _killWeight + _knockdownWeight + _granaryWeight;
        float score = totalWeight > 0f
            ? (survival * _survivalWeight + kill * _killWeight
               + knockdown * _knockdownWeight + granary * _granaryWeight) / totalWeight
            : 0f;

        // The total score is scaled again by the percentage of enemies killed.
        float finalPercent = score * kill;

        float multiplier = won ? 1f : _loseRewardMultiplier;
        int money = Mathf.RoundToInt(_maxBattleReward * finalPercent * multiplier);

        return new BattleReward(survival, kill, knockdown, granary, score, finalPercent, money);
    }

    /// <summary>
    /// Calculates the reward and adds the money. Call this once per battle.
    /// </summary>
    public BattleReward ApplyBattleReward(BattleReport report, bool won)
    {
        BattleReward reward = CalculateBattleReward(report, won);
        AddMoney(reward.Money);
        return reward;
    }

    #endregion
}