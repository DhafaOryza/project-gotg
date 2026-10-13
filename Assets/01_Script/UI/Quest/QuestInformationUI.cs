using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestInformationUI : MonoBehaviour
{
    public static QuestInformationUI Instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private GameObject questInfoPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    private QuestDataSO currentPendingQuest;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (questInfoPanel != null)
            questInfoPanel.SetActive(false);

        if (acceptButton != null)
            acceptButton.onClick.AddListener(OnAcceptClicked);

        if (declineButton != null)
            declineButton.onClick.AddListener(OnDeclineClicked);
    }

    public void ShowQuestOffer(QuestDataSO quest)
    {
        currentPendingQuest = quest;
        if (currentPendingQuest == null) return;

        if (titleText != null) titleText.text = quest.questTitle;
        if (descriptionText != null) descriptionText.text = quest.questDescription;
        if (rewardText != null) rewardText.text = $"Reward: Gold {quest.goldReward}";

        if (questInfoPanel != null)
            questInfoPanel.SetActive(true);

        // Opsional: Pause game saat pilihan quest muncul agar player bisa membaca dulu
        Time.timeScale = 0f;
    }

    private void OnAcceptClicked()
    {
        if (currentPendingQuest != null)
        {
            QuestManager.Instance.AcceptQuest(currentPendingQuest);
        }
        ClosePanel();
    }

    private void OnDeclineClicked()
    {
        if (currentPendingQuest != null)
        {
            QuestManager.Instance.DeclineQuest(currentPendingQuest);
        }
        ClosePanel();
    }

    private void ClosePanel()
    {
        currentPendingQuest = null;
        if (questInfoPanel != null)
            questInfoPanel.SetActive(false);

        Time.timeScale = 1f; // Kembalikan waktu game normal
    }
}