using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Player Quest")]
    [SerializeField] private List<DailyQuestDataSO> dailyQuest = new List<DailyQuestDataSO>();
    [SerializeField] private List<UrgentQuestDataSO> urgentQuest = new List<UrgentQuestDataSO>();

    [Header("ChanceUrqentQuest")]
    [SerializeField] private float chance = 0.5f;

    [Header("UI References")]
    [SerializeField] private QuestInformationUI questInformationUI;
    [SerializeField] private QuestListUI questListUI;

    [Header("Active Quests")]
    private List<QuestDataSO> activeQuests = new List<QuestDataSO>();

    public void Initialize()
    {
        if (questInformationUI == null) questInformationUI = FindFirstObjectByType<QuestInformationUI>();
        if (questListUI == null) questListUI = FindFirstObjectByType<QuestListUI>();
    }

    public void RequestSpecificQuest(QuestDataSO questToOffer)
    {
        if (questToOffer == null) return;

        if (questInformationUI != null)
        {
            questInformationUI.ShowQuestOffer(questToOffer, (isAccepted) =>
            {
                if (isAccepted) AcceptQuest(questToOffer);
            });
        }
    }

    public void AcceptQuest(QuestDataSO quest)
    {
        if (quest == null || activeQuests.Contains(quest)) return;

        activeQuests.Add(quest);

        if (questListUI != null)
        {
            questListUI.RefreshQuestList(activeQuests);
        }
    }
}