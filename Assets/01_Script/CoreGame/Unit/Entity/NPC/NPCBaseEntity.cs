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

    public virtual void Interact(PlayerBaseEntity player)
    {        
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