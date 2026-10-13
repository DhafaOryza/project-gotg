using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;

    public float InteractionRadius => interactionRadius;
    
    private NPCDataSO npcData;

    protected override void Awake()
    {
        base.Awake();
        npcData = entityData as NPCDataSO;
    }

    /// <summary>
    /// Menangani interaksi player: Memulai obrolan, lalu memicu Quest Information setelah obrolan selesai.
    /// </summary>
    public void Interact(PlayerBaseEntity player)
    {
        Debug.Log($"Berinteraksi dengan NPC: {(npcData != null ? npcData.npcName : name)}");
        
        if (ConversationUIManager.Instance != null && npcData != null)
        {
            // Mulai percakapan, berikan callback untuk memunculkan Quest Information di akhir obrolan
            ConversationUIManager.Instance.StartConversation(npcData, () =>
            {
                // Bagian ini dijalankan tepat setelah obrolan selesai (setelah player klik teks terakhir)
                if (npcData.assignedQuest != null && QuestInformationUI.Instance != null)
                {
                    QuestInformationUI.Instance.ShowQuestOffer(npcData.assignedQuest);
                }
            });
        }
        else
        {
            Debug.LogWarning("ConversationUIManager atau NPCData belum di-setup!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}