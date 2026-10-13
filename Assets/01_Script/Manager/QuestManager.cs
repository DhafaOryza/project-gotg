using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Database Quests")]
    [SerializeField] private List<QuestDataSO> allDailyQuests = new List<QuestDataSO>();
    [SerializeField] private List<QuestDataSO> allUrgentQuests = new List<QuestDataSO>();

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
        // Hubungkan event NightController secara aman tanpa nesting callback
        var nightController = GameManager.Instance?.nightController;
        if (nightController != null)
        {
            // Pastikan tidak terjadi double subscription
            nightController.OnNightStarted -= HandleNightStarted;
            nightController.OnNightStarted += HandleNightStarted;
        }
    }

    private void OnDestroy()
    {
        // Bersihkan event listener saat objek dihancurkan
        var nightController = GameManager.Instance?.nightController;
        if (nightController != null)
        {
            nightController.OnNightStarted -= HandleNightStarted;
        }
    }

    /// <summary>
    /// Handler yang terpanggil otomatis saat NightController memicu event OnNightStarted.
    /// </summary>
    private void HandleNightStarted(int nightIndex)
    {
        CheckAndTriggerUrgentQuest();
    }

    /// <summary>
    /// Dipanggil saat Start Night untuk mengecek kemunculan Urgent Quest secara random.
    /// </summary>
    public void CheckAndTriggerUrgentQuest()
    {
        if (allUrgentQuests == null || allUrgentQuests.Count == 0) return;

        bool isUrgentHappening = UnityEngine.Random.value <= 0.5f;

        if (isUrgentHappening)
        {
            int randomIndex = UnityEngine.Random.Range(0, allUrgentQuests.Count);
            QuestDataSO selectedUrgent = allUrgentQuests[randomIndex];

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
    /// Dipanggil ketika player berinteraksi dengan NPC untuk mengambil Daily Quest.
    /// </summary>
    public void RequestDailyQuestFromNPC()
    {
        if (allDailyQuests == null || allDailyQuests.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, allDailyQuests.Count);
        QuestDataSO selectedDaily = allDailyQuests[randomIndex];

        if (QuestInformationUI.Instance != null)
        {
            QuestInformationUI.Instance.ShowQuestOffer(selectedDaily);
        }
    }

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

    public void DeclineQuest(QuestDataSO quest)
    {
        Debug.Log($"[QuestManager] Quest Ditolak: {(quest != null ? quest.questTitle : "Null")}");
    }

    public List<QuestDataSO> GetActiveQuests() => activeQuests;
}