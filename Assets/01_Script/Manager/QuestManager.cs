using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Database Quests")]
    [SerializeField] private List<DailyQuestDataSO> allDailyQuests = new List<DailyQuestDataSO>();
    [SerializeField] private List<UrgentQuestDataSO> allUrgentQuests = new List<UrgentQuestDataSO>();

    [Header("UI References (Diisi dari Inspector)")]
    [SerializeField] private QuestInformationUI questInformationUI;
    [SerializeField] private QuestListUI questListUI;

    [Header("ChanceUrqentQuest")]
    [SerializeField] private float chance = 0.5f;

    public event Action<QuestDataSO> OnQuestAccepted;

    public void Initialize()
    {
        var nightController = GameManager.Instance?.nightController;
        if (nightController != null)
        {
            nightController.OnNightStarted -= HandleNightStarted;
            nightController.OnNightStarted += HandleNightStarted;
        }

        RefreshUI();
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

    /// <summary>
    /// Mengecek kemunculan Urgent Quest secara acak (misalnya peluang 50%) saat mendekati malam.
    /// </summary>
    public void CheckAndTriggerUrgentQuest()
    {
        if (allUrgentQuests == null || allUrgentQuests.Count == 0) return;

        bool isUrgentHappening = UnityEngine.Random.value <= chance;

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
                Debug.Log($"[QuestManager] Urgent Quest Muncul: {selectedUrgent.questTitle}");
            }
        }
        else
        {
            Debug.Log("[QuestManager] Tidak ada Urgent Quest untuk malam ini.");
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

    public void AcceptQuest(QuestDataSO quest)
    {
        if (quest == null) return;

        // Simpan ke GameSessionData agar persisten
        if (GameSessionData.Instance != null)
        {
            GameSessionData.Instance.AddActiveQuestSession(quest);
        }

        OnQuestAccepted?.Invoke(quest);
        Debug.Log($"[QuestManager] Quest Diterima: {quest.questTitle}");

        RefreshUI();
    }

    public void DeclineQuest(QuestDataSO quest)
    {
        Debug.Log($"[QuestManager] Quest Ditolak: {(quest != null ? quest.questTitle : "Null")}");
    }

    public void CompleteQuest(QuestDataSO quest)
    {
        if (quest == null) return;

        if (GameSessionData.Instance != null)
        {
            GameSessionData.Instance.RemoveActiveQuestSession(quest);

        }

        Debug.Log($"[QuestManager] Quest Selesai: {quest.questTitle}. Reward diberikan!");
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (questListUI != null && GameSessionData.Instance != null)
        {
            List<QuestDataSO> currentActiveQuests = GameSessionData.Instance.GetActiveQuestsSession();
            questListUI.RefreshQuestList(currentActiveQuests);
        }
    }

    public List<QuestDataSO> GetActiveQuests()
    {
        if (GameSessionData.Instance != null)
        {
            return GameSessionData.Instance.GetActiveQuestsSession();
        }
        return new List<QuestDataSO>();
    }
}