using System.Collections.Generic;
using UnityEngine;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; set; }

    [Header("Skill Data")]
    [SerializeField] private List<SkillDataSO> choosenSkills = new List<SkillDataSO>();

    [Header("Consumable Items")]
    [SerializeField] private List<ConsumableDataSO> consumableItem = new List<ConsumableDataSO>();

    [Header("Wave")]
    [SerializeField] private WaveDataSO choosenWave;

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

    #region Skill Data

    public List<SkillDataSO> GetSkillsData()
    {
        return choosenSkills;
    }

    public void SetSkillData(SkillDataSO setSkillData)
    {
        choosenSkills.Add(setSkillData);
    }

    public void SetSkillsData(List<SkillDataSO> setSkillsData)
    {
        choosenSkills = setSkillsData;
    }

    #endregion

    #region Consumable Item

    public List<ConsumableDataSO> GetConsumables()
    {
        return consumableItem;
    }

    public void SetConsumable(ConsumableDataSO setConsumable)
    {
        consumableItem.Add(setConsumable);
    }

    public void SetConsumables(List<ConsumableDataSO> setConsumables)
    {
        consumableItem = setConsumables;
    }

    #endregion

    #region Wave Data

    public WaveDataSO GetWaveData()
    {
        return choosenWave;
    }

    public void SetWaveData(WaveDataSO setWaveData)
    {
        choosenWave = setWaveData;
    }

    #endregion

    public void GetDataRegistry()
    {
        var data = Resources.Load<Registry>("TestRegistry");
        if (data == null)
        {
            return;
        }

        if (choosenSkills == null || choosenSkills.Count == 0)
        {
            choosenSkills = new List<SkillDataSO>(data.defaultSkills);
        }

        if (choosenWave == null)
        {
            choosenWave = data.waveData;
        }
    }
}