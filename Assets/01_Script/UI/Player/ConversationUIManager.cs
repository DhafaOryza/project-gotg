using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ConversationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject conversationPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portraitImage;

    private NPCDataSO currentNPCData;
    private int currentIndex = 0;
    private bool isConversationActive = false;
    private Action onConversationFinished;

    public void Setup(NPCDataSO npc)
    {
        
    }

    public void StartConversation(NPCDataSO data, Action finishCallBack = null)
    {
        currentNPCData = data;
        currentIndex = 0;
        isConversationActive = true;
        onConversationFinished = finishCallBack;

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

        onConversationFinished?.Invoke();
        onConversationFinished = null;
    }
}