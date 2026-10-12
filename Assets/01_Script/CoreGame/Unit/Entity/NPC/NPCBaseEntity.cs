using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCDataSO npcData;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;

    // private NPCDataSO npcData;

    public float InteractionRadius => interactionRadius;
    public NPCDataSO NpcData => npcData;

    public void Interact(PlayerBaseEntity player)
    {
        Debug.Log($"Berinteraksi dengan NPC: {(npcData != null ? npcData.npcName : name)}");
        
        if (ConversationUIManager.Instance != null && npcData != null)
        {
            ConversationUIManager.Instance.StartConversation(npcData);
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