using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ConversationUIManager : MonoBehaviour
{
    public static ConversationUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject conversationPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portraitImage;

    private NPCDataSO currentNPCData;
    private int currentIndex = 0;
    private bool isConversationActive = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (conversationPanel != null)
            conversationPanel.SetActive(false);
    }

    private void Update()
    {
        if (isConversationActive)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                OnNextButtonClicked();
            }
        }
    }

    public void StartConversation(NPCDataSO data)
    {
        currentNPCData = data;
        currentIndex = 0;
        isConversationActive = true;

        if (conversationPanel != null)
            conversationPanel.SetActive(true);

        DisplayLine();
    }

    public void OnNextButtonClicked()
    {
        if (currentNPCData == null) return;

        currentIndex++;
        if (currentIndex < currentNPCData.dialogueLines.Length)
        {
            DisplayLine();
        }
        else
        {
            EndConversation();
        }
    }

    private void DisplayLine()
    {
        if (nameText != null) 
            nameText.text = currentNPCData.npcName;
        
        if (dialogueText != null) 
            dialogueText.text = currentNPCData.dialogueLines[currentIndex];
        
        if (portraitImage != null)
        {
            if (currentNPCData.npcPortrait != null)
            {
                portraitImage.gameObject.SetActive(true);
                portraitImage.sprite = currentNPCData.npcPortrait;
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }
    }

    public void EndConversation()
    {
        currentNPCData = null;
        isConversationActive = false;

        if (conversationPanel != null)
            conversationPanel.SetActive(false);
    }
}