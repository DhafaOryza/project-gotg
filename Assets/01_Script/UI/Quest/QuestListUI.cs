using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform questListContainer; 
    [SerializeField] private GameObject questItemPrefab;   

    /// <summary>
    /// Membersihkan dan memperbarui tampilan daftar quest aktif pada panel kiri layar.
    /// </summary>
    public void RefreshQuestList(List<QuestDataSO> activeQuests)
    {
        if (questListContainer == null || questItemPrefab == null) return;

        // Bersihkan list item quest lama di dalam container
        foreach (Transform child in questListContainer)
        {
            Destroy(child.gameObject);
        }

        // Buat elemen baru untuk setiap quest yang sedang aktif
        foreach (var quest in activeQuests)
        {
            if (quest != null)
            {
                GameObject itemObj = Instantiate(questItemPrefab, questListContainer);
                itemObj.transform.localScale = Vector3.one;

                // PENTING: Memastikan ukuran elemen terbaca oleh Vertical Layout Group
                var layoutElement = itemObj.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = itemObj.AddComponent<LayoutElement>();
                }
                layoutElement.minHeight = 30;

                TextMeshProUGUI txt = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = $"• {quest.questTitle}";
                }
            }
        }
    }
}