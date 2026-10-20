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
        if (PlayerUI.Instance != null && npcData != null)
        {
            PlayerUI.Instance.DialogSystem.StartNPCDialog(npcData, () =>
            {
                if (npcData.assignedQuest != null && GameManager.Instance != null && GameManager.Instance.questManager != null)
                {
                    GameManager.Instance.questManager.RequestSpecificQuest(npcData.assignedQuest);
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