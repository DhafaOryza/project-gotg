using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestListUI : MonoBehaviour
{
    public static QuestListUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Transform questListContainer; // Parent dengan Vertical Layout Group di panel kiri
    [SerializeField] private GameObject questItemPrefab;   // Prefab teks/panel kecil untuk item quest di list

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RefreshQuestList(List<QuestDataSO> activeQuests)
    {
        // Bersihkan list lama
        foreach (Transform child in questListContainer)
        {
            Destroy(child.gameObject);
        }

        // Buat elemen baru untuk setiap quest aktif
        foreach (var quest in activeQuests)
        {
            if (questItemPrefab != null && questListContainer != null)
            {
                GameObject itemObj = Instantiate(questItemPrefab, questListContainer);
                TextMeshProUGUI txt = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = $"• {quest.questTitle}";
                }
            }
        }
    }
}