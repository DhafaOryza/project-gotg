using UnityEngine;

public class TrackerBattle : MonoBehaviour
{
    // public static TrackerBattle Instance { get; private set; }

    private float _survivalTime;
    private int _enemiesSpawned;
    private int _enemiesKilled;
    private int _playerKnockdowns;
    private int _granaryDamage;
    private int _granaryMaxHealth;
    private bool _isRunning;
    private GranaryBase _granary;

    public bool IsRunning => _isRunning;

    private void OnDestroy()
    {
        UnsubscribeGranary();

        // if (Instance == this)
        //     Instance = null;
    }

    private void Update()
    {
        // Time.deltaTime is 0 while the game is paused, so paused time is not counted.
        if (_isRunning)
            _survivalTime += Time.deltaTime;
    }

    /// <summary>
    /// Called by GameManager. Registers this tracker and clears any leftover data.
    /// </summary>
    public void Initialize()
    {
        // Instance = this;
        ResetBattle();
    }

    /// <summary>
    /// Resets all numbers and starts counting a new battle.
    /// </summary>
    public void BeginBattle()
    {
        ResetBattle();

        _granary = FindAnyObjectByType<GranaryBase>();
        if (_granary != null)
        {
            _granaryMaxHealth = _granary.MaxHealth;
            _granary.OnDamaged += HandleGranaryDamaged;
        }

        _isRunning = true;
    }

    /// <summary>
    /// Stops counting and returns the report. Returns false if no battle is running,
    /// so the same battle can never be rewarded twice.
    /// </summary>
    public bool TryEndBattle(out BattleReport report)
    {
        report = null;
        if (!_isRunning)
        {
            Debug.Log("TrackerBattle never play");
            return false;
        }

        _isRunning = false;
        UnsubscribeGranary();

        report = new BattleReport(_survivalTime, _enemiesSpawned, _enemiesKilled,
            _playerKnockdowns, _granaryDamage, _granaryMaxHealth);
        return true;
    }

    /// <summary>
    /// Called by EnemyBaseEntity when it spawns.
    /// </summary>
    public void RegisterEnemySpawned()
    {
        if (_isRunning) _enemiesSpawned++;
    }

    /// <summary>
    /// Called by EnemyBaseEntity when it dies.
    /// </summary>
    public void RegisterEnemyKilled()
    {
        if (_isRunning) _enemiesKilled++;
    }

    /// <summary>
    /// Called by PlayerBaseEntity when it gets knocked down.
    /// </summary>
    public void RegisterPlayerKnockdown()
    {
        if (_isRunning) _playerKnockdowns++;
    }

    private void ResetBattle()
    {
        UnsubscribeGranary();

        _isRunning = false;
        _survivalTime = 0f;
        _enemiesSpawned = 0;
        _enemiesKilled = 0;
        _playerKnockdowns = 0;
        _granaryDamage = 0;
        _granaryMaxHealth = 0;
    }

    private void HandleGranaryDamaged(int amount)
    {
        if (_isRunning) _granaryDamage += amount;
    }

    private void UnsubscribeGranary()
    {
        if (_granary != null)
            _granary.OnDamaged -= HandleGranaryDamaged;

        _granary = null;
    }
}