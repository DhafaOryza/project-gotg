using System.Collections.Generic;
using UnityEngine;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; set; }

    [Header("Data Whole Game")]
    public List<EnemyDataSO> enemies = new List<EnemyDataSO>();
    public List<SkillDataSO> skills = new List<SkillDataSO>();
    public List<ConsumableDataSO> consumables = new List<ConsumableDataSO>();
    public List<NightWaveDataSO> nightwaves = new List<NightWaveDataSO>();

    [Header("Registry Data")]
    public Registry registryData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        GetDataRegistry();
    }

    public static GameSessionData GetOrCreate()
    {
        if (Instance != null) return Instance;

        var existing = Object.FindAnyObjectByType<GameSessionData>();
        if (existing != null)
        {
            if (Instance == null) Instance = existing;
            return existing;
        }

        var go = new GameObject("GameSessionData [Auto]");
        var gsd = go.AddComponent<GameSessionData>();

        return gsd;
    }

    private void GetDataRegistry()
    {
        var data = Resources.Load<Registry>("Registry");
        if (data == null)
        {
            return;
        }

        registryData = data;

        enemies = data.Enemies;
        skills = data.Skills;
        consumables = data.Consumables;
        nightwaves = data.NightWaves;
    }
}