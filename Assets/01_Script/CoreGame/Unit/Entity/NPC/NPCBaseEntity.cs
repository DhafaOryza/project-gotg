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

    public void Interact(PlayerBaseEntity player)
    {
        Debug.Log($"Berinteraksi dengan NPC: {(npcData != null ? npcData.npcName : name)}");
        
        if (ConversationUIManager.Instance != null && npcData != null)
        {
            ConversationUIManager.Instance.StartConversation(npcData, () =>
            {
                // Setelah obrolan selesai, jika NPC ini memiliki assignedQuest, tawarkan lewat QuestManager
                if (npcData.assignedQuest != null)
                {
                    if (QuestManager.Instance != null)
                    {
                        Debug.Log($"[NPCBaseEntity] Membuka Quest Information untuk quest: {npcData.assignedQuest.questTitle}");
                        QuestManager.Instance.RequestSpecificQuest(npcData.assignedQuest);
                    }
                    else
                    {
                        Debug.LogWarning("[NPCBaseEntity] QuestManager.Instance tidak ditemukan!");
                    }
                }
                else
                {
                    Debug.Log("NPC ini tidak memiliki assignedQuest khusus.");
                }
            });
        }
        else
        {
            Debug.LogWarning("[NPCBaseEntity] ConversationUIManager atau NPCData belum di-setup!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}