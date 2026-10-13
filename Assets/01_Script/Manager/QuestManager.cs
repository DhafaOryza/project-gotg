using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Database Quests")]
    [SerializeField] private List<DailyQuestDataSO> allDailyQuests = new List<DailyQuestDataSO>();
    [SerializeField] private List<UrgentQuestDataSO> allUrgentQuests = new List<UrgentQuestDataSO>();

    [Header("UI References")]
    [SerializeField] private QuestInformationUI questInformationUI;
    [SerializeField] private QuestListUI questListUI;

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

    public void Initialize()
    {
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

    private void HandleNightStarted(int nightIndex)
    {
        CheckAndTriggerUrgentQuest();
    }

    public void CheckAndTriggerUrgentQuest()
    {
        if (allUrgentQuests == null || allUrgentQuests.Count == 0) return;

        bool isUrgentHappening = UnityEngine.Random.value <= 0.5f;

        if (isUrgentHappening)
        {
            int randomIndex = UnityEngine.Random.Range(0, allUrgentQuests.Count);
            UrgentQuestDataSO selectedUrgent = allUrgentQuests[randomIndex];

            if (questInformationUI != null)
            {
                questInformationUI.ShowQuestOffer(selectedUrgent, (isAccepted) =>
                {
                    if (isAccepted) AcceptQuest(selectedUrgent);
                    else DeclineQuest(selectedUrgent);
                });
            }
            else
            {
                Debug.LogWarning("[QuestManager] questInformationUI belum di-assign di Inspector!");
            }
        }
    }

    public void RequestDailyQuestFromNPC()
    {
        if (allDailyQuests == null || allDailyQuests.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, allDailyQuests.Count);
        DailyQuestDataSO selectedDaily = allDailyQuests[randomIndex];

        if (questInformationUI != null)
        {
            questInformationUI.ShowQuestOffer(selectedDaily, (isAccepted) =>
            {
                if (isAccepted) AcceptQuest(selectedDaily);
                else DeclineQuest(selectedDaily);
            });
        }
        else
        {
            Debug.LogWarning("[QuestManager] questInformationUI belum di-assign di Inspector!");
        }
    }

    public void AcceptQuest(QuestDataSO quest)
    {
        if (quest == null || activeQuests.Contains(quest)) return;

        activeQuests.Add(quest);
        OnQuestAccepted?.Invoke(quest);
        Debug.Log($"[QuestManager] Quest Diterima: {quest.questTitle}");

        if (questListUI != null)
        {
            questListUI.RefreshQuestList(activeQuests);
        }
        else
        {
            Debug.LogWarning("[QuestManager] questListUI belum di-assign di Inspector!");
        }
    }

    /// <summary>
    /// Menawarkan quest spesifik (misalnya quest yang di-assign langsung ke NPC tertentu).
    /// </summary>
    public void RequestSpecificQuest(QuestDataSO questToOffer)
    {
        if (questToOffer == null) return;
    
        if (questInformationUI != null)
        {
            questInformationUI.ShowQuestOffer(questToOffer, (isAccepted) =>
            {
                if (isAccepted) AcceptQuest(questToOffer);
                else DeclineQuest(questToOffer);
            });
        }
        else
        {
            Debug.LogWarning("[QuestManager] questInformationUI belum di-assign di Inspector!");
        }
    }

    public void DeclineQuest(QuestDataSO quest)
    {
        Debug.Log($"[QuestManager] Quest Ditolak: {(quest != null ? quest.questTitle : "Null")}");
    }

    public List<QuestDataSO> GetActiveQuests() => activeQuests;
}