using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Database Quests")]
    [SerializeField] private List<DailyQuestDataSO> allDailyQuests = new List<DailyQuestDataSO>();
    [SerializeField] private List<UrgentQuestDataSO> allUrgentQuests = new List<UrgentQuestDataSO>();

    [Header("Active Quests")]
    private List<QuestDataSO> activeQuests = new List<QuestDataSO>();

    public event Action<QuestDataSO> OnQuestAccepted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Dipanggil secara seragam oleh GameManager saat inisialisasi scene.
    /// </summary>
    public void Initialize()
    {
        // PENTING: Mendengarkan event OnNightStarted dari NightController secara mandiri (Loose Coupling)
        var nightController = GameManager.Instance?.nightController;
        if (nightController != null)
        {
            nightController.OnNightStarted -= HandleNightStarted;
            nightController.OnNightStarted += HandleNightStarted;
        }
    }

    private void OnDestroy()
    {
        var nightController = GameManager.Instance?.nightController;
        if (nightController != null)
        {
            nightController.OnNightStarted -= HandleNightStarted;
        }
    }

    /// <summary>
    /// Handler yang terpanggil otomatis saat malam dimulai untuk memicu pengecekan Urgent Quest.
    /// </summary>
    private void HandleNightStarted(int nightIndex)
    {
        CheckAndTriggerUrgentQuest();
    }

    /// <summary>
    /// Mengecek kemunculan Urgent Quest secara acak saat malam dimulai.
    /// </summary>
    public void CheckAndTriggerUrgentQuest()
    {
        if (allUrgentQuests == null || allUrgentQuests.Count == 0) return;

        bool isUrgentHappening = UnityEngine.Random.value <= 0.5f;

        if (isUrgentHappening)
        {
            int randomIndex = UnityEngine.Random.Range(0, allUrgentQuests.Count);
            UrgentQuestDataSO selectedUrgent = allUrgentQuests[randomIndex];

            if (QuestInformationUI.Instance != null)
            {
                QuestInformationUI.Instance.ShowQuestOffer(selectedUrgent);
            }
        }
        else
        {
            Debug.Log("[QuestManager] Tidak ada Urgent Quest malam ini.");
        }
    }

    /// <summary>
    /// Mengambil Daily Quest secara acak ketika player berinteraksi dengan NPC.
    /// </summary>
    public void RequestDailyQuestFromNPC()
    {
        if (allDailyQuests == null || allDailyQuests.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, allDailyQuests.Count);
        DailyQuestDataSO selectedDaily = allDailyQuests[randomIndex];

        if (QuestInformationUI.Instance != null)
        {
            QuestInformationUI.Instance.ShowQuestOffer(selectedDaily);
        }
    }

    /// <summary>
    /// Menerima quest yang ditawarkan dan memasukkannya ke daftar quest aktif player.
    /// </summary>
    public void AcceptQuest(QuestDataSO quest)
    {
        if (quest == null || activeQuests.Contains(quest)) return;

        activeQuests.Add(quest);
        OnQuestAccepted?.Invoke(quest);
        Debug.Log($"[QuestManager] Quest Diterima: {quest.questTitle}");

        if (QuestListUI.Instance != null)
        {
            QuestListUI.Instance.RefreshQuestList(activeQuests);
        }
    }

    /// <summary>
    /// Menolak quest yang ditawarkan.
    /// </summary>
    public void DeclineQuest(QuestDataSO quest)
    {
        Debug.Log($"[QuestManager] Quest Ditolak: {(quest != null ? quest.questTitle : "Null")}");
    }

    public List<QuestDataSO> GetActiveQuests() => activeQuests;
}