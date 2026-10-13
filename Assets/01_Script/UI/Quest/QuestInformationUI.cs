using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class QuestInformationUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject questInfoPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    private QuestDataSO currentPendingQuest;
    private Action<bool> onQuestResponseCallback; // PENTING: Menggunakan callback untuk merespon pilihan player

    private void Awake()
    {
        if (questInfoPanel != null)
            questInfoPanel.SetActive(false);

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);

        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineClicked);
    }

    /// <summary>
    /// Menampilkan panel tawaran quest kepada pemain beserta callback ketika tombol dipilih.
    /// </summary>
    public void ShowQuestOffer(QuestDataSO quest, Action<bool> responseCallback)
    {
        currentPendingQuest = quest;
        onQuestResponseCallback = responseCallback;

        if (currentPendingQuest == null) return;

        if (titleText != null) titleText.text = quest.questTitle;
        if (descriptionText != null) descriptionText.text = quest.questDescription;
        if (rewardText != null) rewardText.text = $"Reward: Gold {quest.goldReward}";

        if (questInfoPanel != null)
            questInfoPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Dipanggil ketika tombol Accept ditekan oleh pemain.
    /// </summary>
    private void OnAcceptClicked()
    {
        onQuestResponseCallback?.Invoke(true);
        ClosePanel();
    }

    /// <summary>
    /// Dipanggil ketika tombol Decline ditekan oleh pemain.
    /// </summary>
    private void OnDeclineClicked()
    {
        onQuestResponseCallback?.Invoke(false);
        ClosePanel();
    }

    /// <summary>
    /// Menutup panel informasi quest dan mengembalikan waktu game normal.
    /// </summary>
    private void ClosePanel()
    {
        currentPendingQuest = null;
        onQuestResponseCallback = null;

        if (questInfoPanel != null)
            questInfoPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}