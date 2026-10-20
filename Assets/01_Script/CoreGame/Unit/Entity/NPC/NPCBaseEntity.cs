using UnityEngine;

public class NPCBaseEntity : BaseEntity, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    
    private NPCDataSO npcData;

    protected override void Awake()
    {
        base.Awake();
        npcData = entityData as NPCDataSO;
    }

    public void Interact(PlayerBaseEntity player)
    {
        throw new System.NotImplementedException();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}