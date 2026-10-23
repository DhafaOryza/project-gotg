using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float _interactionRadius = 2f;

    private NPCMovement _npcMovement;
    protected NPCDataSO _npcData;

    public float InteractionRadius => _interactionRadius;

    protected override void Awake()
    {
        base.Awake();
        _npcData = entityData as NPCDataSO;
        _npcMovement = GetComponent<NPCMovement>();
    }

    /// <summary>
    /// Handles player interaction with the NPC, initiating dialogue and pausing movement.
    /// </summary>
    public virtual void Interact(PlayerBaseEntity player)
    {
        if (_npcData != null && !_npcData.IsInteractable)
        {
            return;
        }

        if (_npcMovement != null)
        {
            _npcMovement.PauseMovement();
        }

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

    /// <summary>
    /// Handles events after the conversation ends, requesting quests and opening role panels.
    /// </summary>
    protected virtual void OnConversationEnded()
    {
        if (_npcData != null && _npcData.AssignedQuest != null && GameManager.Instance?.questManager != null)
        {
            GameManager.Instance.questManager.RequestSpecificQuest(_npcData.AssignedQuest);
        }

        if (PlayerUI.Instance != null && _npcData != null)
        {
            PlayerUI.Instance.HandleNpcRolePanel(_npcData, _npcMovement);
        }
        else
        {
            ResumeMovement();
        }
    }

    /// <summary>
    /// Resumes the NPC movement behavior when panels close or interaction ends.
    /// </summary>
    public void ResumeMovement()
    {
        if (_npcMovement != null)
        {
            _npcMovement.ResumeMovement();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);

        if (_npcMovement != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 centerPoint = Application.isPlaying ? _npcMovement.StartPosition : transform.position;
            float radius = _npcMovement.WanderRadius;
            Vector3 leftLimit = centerPoint + Vector3.left * radius;
            Vector3 rightLimit = centerPoint + Vector3.right * radius;
            Gizmos.DrawLine(leftLimit, rightLimit);
            Gizmos.DrawWireSphere(leftLimit, 0.2f);
            Gizmos.DrawWireSphere(rightLimit, 0.2f);
        }
    }
}