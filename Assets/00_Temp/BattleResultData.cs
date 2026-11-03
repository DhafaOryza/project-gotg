/// <summary>
/// Raw numbers collected during one battle (one night).
/// </summary>
public class BattleReport
{
    public float SurvivalTime { get; }
    public int EnemiesSpawned { get; }
    public int EnemiesKilled { get; }
    public int PlayerKnockdowns { get; }
    public int GranaryDamage { get; }
    public int GranaryMaxHealth { get; }

    public BattleReport(float survivalTime, int enemiesSpawned, int enemiesKilled,
        int playerKnockdowns, int granaryDamage, int granaryMaxHealth)
    {
        SurvivalTime = survivalTime;
        EnemiesSpawned = enemiesSpawned;
        EnemiesKilled = enemiesKilled;
        PlayerKnockdowns = playerKnockdowns;
        GranaryDamage = granaryDamage;
        GranaryMaxHealth = granaryMaxHealth;
    }
}

/// <summary>
/// Percentages (0 to 1) and the final money calculated from a BattleReport.
/// </summary>
public class BattleReward
{
    public float SurvivalPercent { get; }
    public float KillPercent { get; }
    public float KnockdownPercent { get; }
    public float GranaryPercent { get; }
    public float ScorePercent { get; }
    public float FinalPercent { get; }
    public int Money { get; }

    public BattleReward(float survivalPercent, float killPercent, float knockdownPercent,
        float granaryPercent, float scorePercent, float finalPercent, int money)
    {
        SurvivalPercent = survivalPercent;
        KillPercent = killPercent;
        KnockdownPercent = knockdownPercent;
        GranaryPercent = granaryPercent;
        ScorePercent = scorePercent;
        FinalPercent = finalPercent;
        Money = money;
    }
}