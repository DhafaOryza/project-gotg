using System.Collections.Generic;
using UnityEngine;

public class GameSessionData : MonoBehaviour
{
    public static GameSessionData Instance { get; set; }

    [Header("Player Data")]
    [SerializeField] private PlayerDataSO playerData;

    [Header("Skill Data")]
    [SerializeField] private List<SkillDataSO> choosenSkills = new List<SkillDataSO>();

    [Header("Consumable Items")]
    [SerializeField] private List<ConsumableDataSO> consumableItem = new List<ConsumableDataSO>();

    [Header("Fallback / Testing")]
    [Tooltip("Nama asset Registry di folder Resources. Dipakai kalau GameSessionData belum punya data " +
             "sama sekali, misalnya saat scene gameplay dijalankan langsung tanpa lewat Main Menu/Preparation.")]
    [SerializeField] private string registryResourcePath = "TestRegistry";

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

        EnsureDataLoaded();
    }

    void Start()
    {
        EnsureDataLoaded();
    }

    public static GameSessionData GetOrCreate()
    {
        if (Instance != null) return Instance;

        var existing = FindObjectOfType<GameSessionData>();
        if (existing != null)
        {
            existing.EnsureDataLoaded();
            if (Instance == null) Instance = existing;
            return existing;
        }

        var go = new GameObject("GameSessionData [Auto]");
        var gsd = go.AddComponent<GameSessionData>();

        return gsd;
    }

    #region Player Data

    public PlayerDataSO GetPlayerData()
    {
        EnsureDataLoaded();
        return playerData;
    }

    public void SetPlayerData(PlayerDataSO setPlayerData)
    {
        playerData = setPlayerData;
    }

    #endregion

    #region Skill Data

    public List<SkillDataSO> GetSkillsData()
    {
        EnsureDataLoaded();
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
        EnsureDataLoaded();
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

    #region Helper

    private bool dataLoadAttempted;

    private void EnsureDataLoaded()
    {
        if (dataLoadAttempted) return;
        dataLoadAttempted = true;

        bool hasNoPlayerData = playerData == null;
        bool hasNoSkills = choosenSkills == null || choosenSkills.Count == 0;
        bool hasNoConsumables = consumableItem == null || consumableItem.Count == 0;

        if (hasNoPlayerData && hasNoSkills && hasNoConsumables)
        {
            GetRegistry();
        }
    }

    private void GetRegistry()
    {
        var data = Resources.Load<Registry>(registryResourcePath);
        if (data == null)
        {
            Debug.LogWarning($"[GameSessionData] Registry '{registryResourcePath}' tidak ditemukan di folder Resources. Data testing tidak dimuat.");
            return;
        }

        if (playerData == null)
            playerData = data.defaultPlayerData;

        if (choosenSkills == null || choosenSkills.Count == 0)
            choosenSkills = new List<SkillDataSO>(data.defaultSkills);

        if (consumableItem == null || consumableItem.Count == 0)
            consumableItem = new List<ConsumableDataSO>(data.defaultConsumables);

        Debug.Log($"[GameSessionData] Data testing dimuat dari Registry '{registryResourcePath}'.");
    }

    #endregion
}