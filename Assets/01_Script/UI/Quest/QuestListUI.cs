using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform scrollContentTransform;
    [SerializeField] private GameObject questItemPrefab;

    /// <summary>
    /// Membersihkan dan memperbarui tampilan daftar quest aktif menggunakan Scroll View.
    /// </summary>
    public void RefreshQuestList(List<QuestDataSO> activeQuests)
    {
        if (scrollContentTransform == null || questItemPrefab == null)
        {
            Debug.LogWarning("[QuestListUI] ScrollContentTransform atau QuestItemPrefab belum di-assign di Inspector!");
            return;
        }

        // 1. Bersihkan list item quest lama di dalam Content ScrollView
        foreach (Transform child in scrollContentTransform)
        {
            Destroy(child.gameObject);
        }

        // 2. Buat elemen baru untuk setiap quest yang sedang aktif
        foreach (var quest in activeQuests)
        {
            if (quest != null)
            {
                GameObject itemObj = Instantiate(questItemPrefab, scrollContentTransform);
                itemObj.transform.localScale = Vector3.one;

                var layoutElement = itemObj.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = itemObj.AddComponent<LayoutElement>();
                }
                layoutElement.minHeight = 40;

                TextMeshProUGUI txt = itemObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.text = $"• {quest.questTitle}";
                }
            }
        }

        Debug.Log($"[QuestListUI] Scroll View diperbarui. Total quest aktif: {activeQuests.Count}");
    }
}