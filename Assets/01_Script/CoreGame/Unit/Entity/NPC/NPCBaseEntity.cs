using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [SerializeField] private float _interactionRadius = 2f;

    public float InteractionRadius => _interactionRadius;
    
    protected NPCDataSO _npcData;

    protected override void Awake()
    {
        base.Awake();
        _npcData = entityData as NPCDataSO;
    }

    public virtual void Interact(PlayerBaseEntity player)
    {
        Debug.Log($"[NPCBaseEntity] Interacting with NPC Role: {(_npcData != null ? _npcData.NpcRole.ToString() : "Unknown")}");

        ConversationUI conversationUI = PlayerUI.Instance != null ? PlayerUI.Instance.ConversationUI : null;
        if (conversationUI == null)
        {
            conversationUI = Object.FindFirstObjectByType<ConversationUI>();
        }
        
        if (conversationUI != null && _npcData != null)
        {
            conversationUI.StartNPCDialog(_npcData, OnConversationEnded);
        }
    }

    protected virtual void OnConversationEnded()
    {
        if (_npcData.AssignedQuest != null && GameManager.Instance != null && GameManager.Instance.questManager != null)
        {
            GameManager.Instance.questManager.RequestSpecificQuest(_npcData.AssignedQuest);
        }

        if (PlayerUI.Instance != null && _npcData != null)
        {
            PlayerUI.Instance.HandleNpcRolePanel(_npcData.NpcRole);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
}