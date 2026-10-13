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
                if (npcData.assignedQuest != null)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.questManager.RequestSpecificQuest(npcData.assignedQuest);
                    }
                }
            });
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}