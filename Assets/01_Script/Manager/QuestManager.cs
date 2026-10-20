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

    public void Initialize()
    {

    }
}